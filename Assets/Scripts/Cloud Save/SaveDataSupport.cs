using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static VARLab.DLX.SaveData;

namespace VARLab.DLX
{
    /// <summary>
    /// The purpose of this class is to act as the "in between" the SaveData object, The cloudSave system, and the rest of the Unity project
    /// use this class to add data to the SaveData object, get data from the SaveData object, trigger saves and loads, etc.
    /// </summary>
    public class SaveDataSupport : MonoBehaviour
    {
        private const int AutoSaveInterval = 180;

        private SaveData saveData;

        private CustomSaveHandler saveHandler;

        private System.Diagnostics.Stopwatch autoSaveTimer;

        public bool CanSave;

        // This is a static bool that tracks if the simulation has been restarted (Restart button or player has been forced to restart)
        public static bool Restarted = false;

        public static SaveDataSupport Instance;

        // Flags to track save file status
        private bool saveFileExists = false;
        private bool isVersionValid = true;

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
        [Header("Main Load Events"), Space(5f)]
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

        private void Start()
        {
            Instance = this;
            CanSave = false;
            saveHandler = GetComponent<CustomSaveHandler>();
            saveData = GetComponent<SaveData>();

            // Initialize auto save timer
            autoSaveTimer = new System.Diagnostics.Stopwatch();

            // Initialize events
            OnLoad ??= new();
            OnInitialize ??= new();
            OnFreshLoad ??= new();
            OnValidSaveFileFound ??= new();
            MovePlayer ??= new();
            LoadInspectionList ??= new();
            LoadPhotos ??= new();
            LoadActivityList ??= new();

            AddListeners();

#if UNITY_EDITOR
            Initialize();
#endif

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

        /// <summary>
        /// Checks if conditions are met for auto-save and triggers a save if needed.
        /// This method is extracted from Update to make it testable.
        /// </summary>
        /// <returns>True if auto-save was triggered, false otherwise</returns>
        public bool CheckAndTriggerAutoSave()
        {
            // Check if TimeManager timer is running
            // It will run after handwashingtask is completed so CanSave will be true
            if (TimerManager.Instance != null && TimerManager.Instance.Timer.IsRunning)
            {
                // Check if the timer has reached 3 minutes
                if (autoSaveTimer.Elapsed.TotalSeconds >= AutoSaveInterval)
                {
                    // Invoke save
                    TriggerSave();
                    return true;
                }
            }
            return false;
        }

        private void AddListeners()
        {
            OnLoad.AddListener(() =>
            {
                LoadInspectionList?.Invoke(saveData.InspectionLog);
                LoadPhotos?.Invoke(saveData.PhotoIdAndTimeStamp);
                LoadActivityList?.Invoke(saveData.ActivityLog);
                LoadTimer();
            });
        }

        /// <summary>
        /// This function is called when Learner Session Handler is initialized
        /// </summary>
        public void Initialize()
        {
            OnInitialize?.Invoke();
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
        /// <see cref="CustomSaveHandler.Save"/> is called to add the save action to the queue.
        /// </summary>
        private void TriggerSave()
        {
            if (CanSave)
            {
                SaveTimer();
                autoSaveTimer.Restart();
                saveHandler.Save();
            }
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
            saveFileExists = exists;
            Debug.Log($"SaveDataSupport: Save file exists: {saveFileExists}");
            
            if (exists)
            {
                saveHandler.Load();
                Debug.Log("SaveDataSupport: Save file exists, loading");
                // Version check will happen in <see cref="HandleLoadComplete"/>
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
                // Verify version is valid
                // NOTE: Consider empty version as valid for this branch for testing
                isVersionValid = saveData.Version == Application.version || string.IsNullOrEmpty(saveData.Version);
                Debug.Log($"SaveDataSupport: Version check - Save: {saveData.Version ?? "null"}, App: {Application.version}, Valid: {isVersionValid}");
                
                if (isVersionValid)
                {
                    Debug.Log("SaveDataSupport: Valid save file, showing continue/restart UI");
                    OnValidSaveFileFound?.Invoke();
                }
                else
                {
                    // Invalid version - delete old save and start new simulation
                    Debug.Log("SaveDataSupport: Invalid version, deleting previous save file and starting new simulation");
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
            saveHandler.Load();

            yield return new WaitUntil(() => saveHandler.LoadSuccess != null);

            if (saveHandler.LoadSuccess == true)
            {
                OnLoad?.Invoke();
            }
        }

        /// <summary>
        /// Deletes the save file from the cloud save system.
        /// </summary>
        public void TriggerDelete()
        {
            saveHandler.Delete();
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
        }

        /// <summary>
        /// Starts the auto save timer. Will be used to start the auto save timer when the learner completes HandwashingTask.
        /// </summary>
        public void StartAutoSaveTimer()
        {
            autoSaveTimer.Start();
        }

        /// <summary>
        /// Stops the auto save timer. Will determine use case when needed.
        public void StopAutoSaveTimer()
        {
            autoSaveTimer.Stop();
        }

        /// <summary>
        /// Wrapper method for the <see cref="CustomSaveHandler.OnFreshLoad"/> event.
        /// </summary>
        /// <remarks>
        /// Called when player clicks the restart button via <see cref="StartPauseWindowBuilder.OnRestartScene"/>
        /// Sets the Restarted flag which will be detected in Start() after scene reload
        /// </remarks>
        public void OnLoadRestart()
        {
            Debug.Log("SaveDataSupport: OnLoadRestart called - Deleting save file");
            TriggerDelete();
            
            // Set restart flag to true before restarting the scene
            Debug.Log("SaveDataSupport: Set Restarted flag to true");
            Restarted = true;
            
            Debug.Log("SaveDataSupport: Restarting scene after save file deletion");
            TPISceneManager.Instance.RestartScene();
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
    }
}
