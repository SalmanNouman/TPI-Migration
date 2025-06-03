using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using VARLab.CloudSave;
using VARLab.DeveloperTools;
using static VARLab.DLX.SaveData;
using Debug = UnityEngine.Debug;

namespace VARLab.DLX
{

    /// <summary>
    ///     Extends the <see cref="ExperienceSaveHandler"/> provided by the CloudSave package.
    /// </summary>
    /// <remarks>
    ///     This class should interface with other services in the DLX in order to safely handle
    ///     saving and loading the game state.
    ///     
    ///     Modify it to suit the needs of the DLX project.
    /// </remarks>
    public class CustomSaveHandler : ExperienceSaveHandler
    {
        [Tooltip("Indicates whether the Load action should be performed automatically once a learner is logged in")]
        public bool LoadOnStart = false;

        public bool? LoadSuccess = null;

        public bool CanSave;

        // This static bool tracks if the simulation has been restarted by either being kicked out
        // or selecting the restart button.
        // It is static because static variable persist on SceneManager.LoadSceneAsync()
        public static bool Restarted = false;

        public static CustomSaveHandler Instance;

        /// <summary>
        ///     Flag that tracks the success status of the delete operation.
        /// </summary>
        public bool? DeleteSuccess = null;

        private bool saveFlag = false;
        private bool requestDone = false;

        private Queue<Action> saveQueue = new();

        // Flag to track load status
        private bool isAlreadyLoaded = false;

        private SaveData saveData;

        // Flags to track save file status
        private bool saveFileExists = false;
        private bool isVersionValid = true;

        // Auto Save settings
        private const int AutoSaveInterval = 180;
        private Stopwatch autoSaveStopwatch = new Stopwatch();

        [Header("Load Events")]
        /// <summary>
        ///     Event triggered when a valid save file is loaded successfully.
        ///     Used to update in-simulation objects/data based on loaded save data.
        ///     <see cref="IntroductionTask.LoadSaveTask"/>
        ///     <see cref="OfficeTask.LoadSaveTask"/>
        ///     <see cref="HandwashingMovementTile.LoadSaveTask"/>
        ///     <see cref="InspectionHandler.SetHandwashingTaskCompleted"/>
        /// </summary>
        /// <remarks>
        ///     Inspector connections:
        /// <remarks>
        public UnityEvent OnLoad;

        /// <summary>
        /// Initializes the cloud save and if a 
        /// </summary>
        public UnityEvent OnInitialize;

        /// <summary>
        /// Event triggered when a new simulation should be started (no save file, invalid save, or restart).
        /// </summary>
        /// <remarks>
        ///     Inspector connections:
        /// - <see cref="StartInfoWindowBuilder.Show"/>
        /// </remarks>
        public UnityEvent OnFreshLoad;

        /// <summary>
        ///     Event triggered when a valid save file is found
        ///     Used to show the continue/restart UI.
        /// </summary>
        /// <remarks>
        ///     Inspector connections:
        ///     - <see cref="StartPauseWindowBuilder.ShowAsWelcome"/>
        /// </remarks>
        public UnityEvent OnValidSaveFileFound;

        [Header("Load events for different classes"), Space(5f)]

        public UnityEvent<string> MovePlayer;

        /// <summary>
        /// <see cref="InspectionHandler.LoadInspectionLog(List{InspectionSaveData})"/>
        /// </summary>
        public UnityEvent<List<InspectionSaveData>> LoadInspectionList;

        /// <summary>
        /// <see cref="InspectionHandler.LoadPhotos(Dictionary{string, string})"/>
        /// </summary>
        public UnityEvent<Dictionary<string, string>> LoadPhotos;

        public UnityEvent<List<ActivityData>> LoadActivityList;

        public UnityEvent<bool> LoadPiercerInteraction;

        public UnityEvent<bool> LoadTattooArtistInteraction;

        protected virtual void OnValidate()
        {
            if (m_AzureSaveSystem == null)
            {
                m_AzureSaveSystem = GetComponent<AzureSaveSystem>();
            }
        }

        public virtual void Start()
        {
            CommandInterpreter.Instance?.Add(new CloudSaveCommand(this));

            Instance = this;
            CanSave = false;

            saveData = GetComponent<SaveData>();

            // Initialize status flags
            LoadSuccess = null;
            DeleteSuccess = null;

            // starts a save background loop
            StartCoroutine(SaveLoop());

            // Initialize events
            OnLoad ??= new();
            OnInitialize ??= new();
            OnFreshLoad ??= new();
            OnValidSaveFileFound ??= new();
            MovePlayer ??= new();
            LoadInspectionList ??= new();
            LoadPhotos ??= new();
            LoadActivityList ??= new();
            LoadPiercerInteraction ??= new();
            LoadTattooArtistInteraction ??= new();

            AddListeners();

            // Check for restart flag with slight delay to ensure it happens after UI initialization
            if (Restarted)
            {
                Debug.Log("SaveDataSupport: Detected restart flag, showing welcome screen");
                StartCoroutine(DelayedFreshLoad());
            }
        }

        /// <summary>
        /// Called every frame to check TimerManager timer and trigger save if the SaveDataSupport
        /// Timer reaches a certain threshold.
        /// </summary>
        private void Update()
        {
            CheckAndTriggerAutoSave();
        }

        private void AddListeners()
        {
            OnSaveComplete.AddListener(SetSaveRequestCompletion);
            OnLoadComplete.AddListener(SetLoadCompletion);
            OnDeleteComplete.AddListener(SetDeleteCompletion);
            OnFreshLoad.AddListener(SetUpInitialData);
            OnLoadComplete.AddListener(HandleLoadComplete);

            OnLoad.AddListener(() =>
            {
                LoadTimer();
                StartAutoSaveTimer();
                LoadInspectionList?.Invoke(saveData.InspectionLog);
                LoadActivityList?.Invoke(saveData.ActivityLog);
                LoadPiercerInteraction?.Invoke(saveData.PiercerInteractionCompleted);
                LoadTattooArtistInteraction?.Invoke(saveData.TattooArtistInteractionCompleted);
                LoadPhotos?.Invoke(saveData.PhotoIdAndTimeStamp);
                MovePlayer?.Invoke(saveData.LastPOI);
            });
        }

        /// <summary>
        /// Checks if conditions are met for auto-save and triggers a save if needed.
        /// This method is extracted from Update to make it testable.
        /// </summary>
        /// <returns>True if auto-save was triggered, false otherwise</returns>
        public bool CheckAndTriggerAutoSave()
        {
            // Check if TimeManager timer is running
            // It will run after handwashingTask is completed so CanSave will be true
            if (TimerManager.Instance != null && TimerManager.Instance.Timer.IsRunning)
            {
                // Check if the timer has reached 3 minutes
                if (autoSaveStopwatch.Elapsed.TotalSeconds >= AutoSaveInterval)
                {
                    // Invoke save
                    TriggerSave();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the save data version to the application version.
        /// This is used to check if the application saved version matches the application version.
        /// If the versions do not match the save file is not valid.
        /// </summary>
        public void SetUpInitialData()
        {
            saveData.Version = Application.version;
        }

        /// <summary>
        /// If CanSave is true the elapsed time is saved and the 
        /// </summary>
        private void TriggerSave()
        {
            if (CanSave)
            {
                SaveTimer();
                autoSaveStopwatch.Restart();
                Save();
            }
        }

        /// <summary>
        /// This is the coroutine that runs in the background looping handling actions if they are queued otherwise just does nothing.
        /// This ensures that saves don't get completed out of order.
        /// </summary>
        private IEnumerator SaveLoop()
        {
            while (true)
            {
                if (saveFlag)
                {
                    Action currentAction = saveQueue.Dequeue();
                    currentAction?.Invoke();

                    yield return new WaitUntil(() => requestDone == true);

                    requestDone = false;
                }

                if (saveQueue.Count == 0)
                {
                    saveFlag = false;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Overrides the <see cref="ExperienceSaveHandler.Save"/>
        /// Instead of saving we are adding the save actions to a queue to ensure they 
        /// don't get saved out of order.
        /// </summary>
        public override void Save()
        {
            if (!enabled) { return; }

            OnSaveStart?.Invoke();

            saveQueue.Enqueue(SaveAction);

            saveFlag = true;
        }

        /// <summary>
        /// The save action that is added to the queue
        /// </summary>
        public void SaveAction()
        {
            var data = CloudSerializer.Serialize();
            var save = SaveSystem.Save(FilePath, data);
        }

        /// <summary>
        ///     Receives a username externally (typically from SCORM, may soon be from LTI)
        ///     and updates the 'Blob' name with the specified username.
        /// </summary>
        /// <remarks>
        ///     It can either load the game state or list the available save files.
        /// </remarks>
        /// <param name="username">
        ///     A unique user ID to provide session identification
        /// </param>
        public void HandleLogin(string username)
        {
            Blob = $"TPI_{username}";

            if (LoadOnStart)
            {
                Load();
            }
            else
            {
                List();
            }
        }

        /// <summary>
        /// Gets the response from <see cref="ExperienceSaveHandler.HandleRequestCompleted(object, RequestCompletedEventArgs)"/>
        /// If the save is completed successfully the next save action in the queue
        /// is triggered.
        /// </summary>
        /// <param name="done">if the save request is completed.</param>
        private void SetSaveRequestCompletion(bool done)
        {
            requestDone = done;
        }

        /// <summary>
        /// Gets the response from <see cref="ExperienceSaveHandler.HandleRequestCompleted(object, RequestCompletedEventArgs)"/>
        /// If load is completed the <see cref="SaveDataSupport.OnLoad"/> will be triggered.
        /// </summary>
        /// <param name="completed">True: azure load completed successfully
        ///                         False: azure did not find a file to load from.</param>
        public void SetLoadCompletion(bool completed)
        {
            LoadSuccess = completed;
        }

        /// <summary>
        ///     Gets the response from <see cref="ExperienceSaveHandler.HandleRequestCompleted(object, RequestCompletedEventArgs)"/>
        ///     Updates the DeleteSuccess flag to track the completion status of the delete operation.
        /// </summary>
        /// <remarks>
        ///     - Connected from <see cref="OnDeleteComplete"/> event
        ///     - Used by <see cref="SaveDataSupport.OnLoadRestartCoroutine"/> to determine if scene restart should proceed
        /// </remarks>
        /// <param name="completed">True: delete succeeded, False: delete failed</param>
        public void SetDeleteCompletion(bool completed)
        {
            DeleteSuccess = completed;
            Debug.Log($"CustomSaveHandler: Delete operation {(completed ? "succeeded" : "failed")}");
        }

        /// <summary>
        ///     Overrides the <see cref="ExperienceSaveHandler.HandleRequestCompleted(object, RequestCompletedEventArgs)"/>
        ///     This method has a custom handling for List action.
        /// </summary>
        public override void HandleRequestCompleted(object sender, RequestCompletedEventArgs args)
        {
            base.HandleRequestCompleted(sender, args);

            if (args == null)
            {
                return;
            }

            // Handle only list action
            if (args.Action != RequestAction.List)
            {
                return;
            }

            // Error state check
            if (!args.Success)
            {
                return;
            }

            // early return if the save file has already been loaded
            if (isAlreadyLoaded)
            {
                return;
            }

            // Check if the Blob name is one of the available save files
            var tokens = args.Data.Split("\"");
            Debug.Log($"CustomSaveHandler: Found these tokens in cloud save list: [{string.Join(", ", tokens)}]");

            // Determine if save file exists and notify through event
            bool saveFileExists = tokens.Contains(Blob);
            HandleSaveFileStatus(saveFileExists);
            isAlreadyLoaded = true;
        }

        /// <summary>
        ///     Handles the save file status check result from cloud
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Connected from: <see cref="CustomSaveHandler.OnSaveFileStatusCheck"/> event
        ///     - If file exists: Triggers Save Handler's Load method to retrieve the save data
        ///     - If no file exists: Invokes <see cref="OnFreshLoad"/> to start a new simulation
        /// </remarks>
        /// <param name="exists">Whether the save file exists in cloud</param>
        public void HandleSaveFileStatus(bool exists)
        {
            // Early return if already loaded
            if (isAlreadyLoaded)
            {
                return;
            }

            saveFileExists = exists;
            Debug.Log($"SaveDataSupport: Save file exists: {saveFileExists}");

            if (exists)
            {
                Load();
                isAlreadyLoaded = true;
                Debug.Log("SaveDataSupport: Save file exists, loading");
            }
            else
            {
                // No save file exists, start new simulation
                Debug.Log("SaveDataSupport: No save file exists, starting new simulation");
                OnFreshLoad?.Invoke();
            }
        }

        /// <summary>
        ///     Handles the load completion event.
        ///     Validates save file version and determines simulation loading state.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Connected from: <see cref="CustomSaveHandler.OnLoadComplete(bool)"/> event
        ///     - If successful load: Checks version validity against application version
        ///     - If valid version: Invokes <see cref="OnValidSaveFileFound"/> to show continue/restart UI
        ///     - If invalid version: Deletes save file and invokes <see cref="OnFreshLoad"/>
        ///     - If load failed: Deletes corrupted save and invokes <see cref="OnFreshLoad"/>
        /// </remarks>
        /// <param name="success">Whether the load operation was successful</param>
        public void HandleLoadComplete(bool success)
        {
            if (success)
            {
                // Verify version is valid - compare save file version with application version
                isVersionValid = saveData.Version == Application.version;
                Debug.Log($"SaveDataSupport: Version check - Save file version: {saveData.Version}, Application version: {Application.version}, Valid: {isVersionValid}");

                if (isVersionValid)
                {
                    Debug.Log("SaveDataSupport: Valid save file version, showing continue/restart UI");
                    OnValidSaveFileFound?.Invoke();
                }
                else
                {
                    // Invalid version - delete old save and start new simulation
                    Debug.Log($"SaveDataSupport: Version mismatch detected, deleting outdated save file and starting new simulation");
                    TriggerDelete();
                    OnFreshLoad?.Invoke();
                }
            }
            else
            {
                // Load failed - delete the potentially corrupted save file and start new simulation
                Debug.Log("SaveDataSupport: Load failed, deleting the corrupted save file and starting new simulation");
                TriggerDelete();
                OnFreshLoad?.Invoke();
            }
        }

        /// <summary>
        /// Method to invoke OnLoad event
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Connected from: <see cref="StartPauseWindowBuilder.OnContinueSavedGame"/>
        /// </remarks>
        public void InvokeOnLoad()
        {
            Debug.Log("SaveDataSupport: OnLoadEvent invoked - setting up in-simulation data");
            OnLoad?.Invoke();
        }

        public void TriggerLoad()
        {
            StartCoroutine(TriggerLoadCoroutine());
        }

        /// <summary>
        /// This method will trigger a load from the cloud save system and then call the OnLoad event
        /// </summary>
        public IEnumerator TriggerLoadCoroutine()
        {
            Load();

            yield return new WaitUntil(() => LoadSuccess != null);

            if (LoadSuccess == true)
            {
                OnLoad?.Invoke();
            }
        }

        /// <summary>
        /// Deletes the save file from the cloud save system.
        /// </summary>
        public void TriggerDelete()
        {
            Delete();
        }

        /// <summary>
        /// This method saves the current elapsed time from the TimerManager to the save data object
        /// </summary>
        private void SaveTimer()
        {
            TimeSpan ts = TimerManager.Instance.GetTimeSpan();
            saveData.Time = ts;
        }

        /// <summary>
        /// This method loads the time from the save data object into the TimerManager. Called when the sim is loading.
        /// </summary>
        public void LoadTimer()
        {
            TimeSpan ts = saveData.Time;
            TimerManager.Instance.Offset = ts;
            TimerManager.Instance.StartTimers();
        }

        /// <summary>
        /// Starts the auto save timer. Will be used to start the auto save timer when the learner completes HandwashingTask.
        /// </summary>
        public void StartAutoSaveTimer()
        {
            autoSaveStopwatch.Start();
        }

        /// <summary>
        /// Stops the auto save timer. Will determine use case when needed.
        public void StopAutoSaveTimer()
        {
            autoSaveStopwatch.Stop();
        }

        /// <summary>
        ///     Handles save file deletion and scene restart.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Connected from: <see cref="StartPauseWindowBuilder.OnRestartScene"/> when player clicks restart
        ///     - Calls coroutine <see cref="OnLoadRestartCoroutine"/> to ensure proper deletion before restarting
        /// </remarks>
        public void OnLoadRestart()
        {
            Debug.Log("SaveDataSupport: OnLoadRestart called - Deleting save file");
            StartCoroutine(OnLoadRestartCoroutine());
        }

        /// <summary>
        ///     Manages save deletion flow and conditional scene restart.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Called from <see cref="OnLoadRestart"/>
        ///     - Triggers save file deletion with <see cref="TriggerDelete"/>
        ///     - Waits for deletion completion and checks success status
        ///     - If successful: Sets <see cref="Restarted"/> flag and restarts scene
        ///     - If failed: Logs error and prevents scene restart to avoid issues
        /// </remarks>
        private IEnumerator OnLoadRestartCoroutine()
        {
            // Trigger save file deletion
            TriggerDelete();

            // Wait until the deletion operation completes (success or failure)
            Debug.Log("SaveDataSupport: Waiting for save file deletion to complete...");
            yield return new WaitUntil(() => DeleteSuccess != null);

            // Check if deletion was successful
            if (DeleteSuccess == true)
            {
                Debug.Log("SaveDataSupport: Save file deletion completed successfully");

                // Set restart flag to true before restarting the scene
                Debug.Log("SaveDataSupport: Set Restarted flag to true");
                Restarted = true;

                Debug.Log("SaveDataSupport: Restarting scene after save file deletion");
                TPISceneManager.Instance.RestartScene();
            }
            else
            {
                // If deletion failed, do not restart the scene
                Debug.Log("SaveDataSupport: Save file deletion failed. The simulation will not restart to prevent issues.");
            }
        }

        /// <summary>
        ///     Invokes OnFreshLoad event with a slight delay to ensure it happens after UI initialization.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Called in <see cref="Start"/> when <see cref="Restarted"/> flag is true
        ///     - Adds a small delay to ensure UI components are ready
        ///     - Resets <see cref="Restarted"/> flag to false after processing
        ///     - Invokes <see cref="OnFreshLoad"/> to start a new simulation
        private IEnumerator DelayedFreshLoad()
        {
            yield return null;
            yield return new WaitForSeconds(0.1f);
            Restarted = false; // Reset the flag
            OnFreshLoad?.Invoke();
        }

        #region Inspection Data
        /// <summary>
        /// Saves the list of inspections made.
        /// </summary>
        public void SaveInspectionLog(List<InspectionData> inspectionList)
        {
            List<InspectionSaveData> tempList = new();

            foreach (InspectionData data in inspectionList)
            {
                InspectionSaveData saveData = new InspectionSaveData();
                saveData.ObjectId = data.Obj.ObjectId;
                saveData.IsCompliant = data.IsCompliant;
                saveData.HasPhoto = data.HasPhoto;
                tempList.Add(saveData);
            }

            saveData.InspectionLog = tempList;
            TriggerSave();
        }
        #endregion


        /// <summary>
        /// Saves the object Id and the photo timestamp
        /// </summary>
        /// <param name="photos"></param>
        public void SavePhotos(List<InspectablePhoto> photos)
        {
            Dictionary<string, string> tempDictionary = new Dictionary<string, string>();

            foreach (var photo in photos)
            {
                tempDictionary.Add(photo.Id, photo.Timestamp);
            }

            saveData.PhotoIdAndTimeStamp = tempDictionary;
            TriggerSave();
        }

        #region Activity Log Data
        /// <summary>
        /// Saves the list of activity logs.
        /// </summary>
        public void SaveActivityLog(List<Log> activityLogList)
        {
            List<ActivityData> tempList = new();

            foreach (Log log in activityLogList)
            {
                ActivityData saveData = new ActivityData();
                saveData.IsPrimary = log.IsPrimary;
                saveData.LogString = log.Message;
                tempList.Add(saveData);
            }

            saveData.ActivityLog = tempList;
            TriggerSave();
        }
        #endregion

        #region POI Data

        /// <summary>
        ///     Saves the current POI to the save data.
        ///     Called when player enters a new POI through OnPoiEnter event.
        /// </summary>
        /// <param name="poi">The POI that was entered</param>
        public void SaveLastPOI(Poi poi)
        {
            if (!CanSave) return;

            saveData.LastPOI = poi.SelectedPoiName.ToString();
            TriggerSave();
        }

        #endregion

        #region Artist Interaction
        /// <summary>
        /// Saves the state of the piercer interaction to the save data.
        /// Called when the piercer interaction is completed.
        /// see <see cref="ArtistInteractionTask.OnTaskCompleted"/>
        /// </summary>
        /// <param name="isCompleted"></param>
        public void SavePiercerInteraction(bool isCompleted)
        {
            saveData.PiercerInteractionCompleted = isCompleted;
            TriggerSave();
        }

        /// <summary>
        /// Saves the state of the tattoo artist interaction to the save data.
        /// Called when the tattoo artist interaction is completed.
        /// see <see cref="ArtistInteractionTask.OnTaskCompleted"/>
        /// </summary>
        /// <param name="isCompleted"></param>
        public void SaveTatooArtistInteraction(bool isCompleted)
        {
            saveData.TattooArtistInteractionCompleted = isCompleted;
            TriggerSave();
        }

        #endregion
    }
}
