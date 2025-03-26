using NUnit.Framework;
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
        private SaveData saveData;

        private CustomSaveHandler saveHandler;

        public bool CanSave;

        // This is a static bool that tracks if the simulation has been restarted (Restart button or player has been forced to restart)
        public static bool Restarted = false;

        public static SaveDataSupport Instance;

        /// <summary>
        /// Unity Event invoked when the learner choose to start from a saved file.
        /// </summary>
        [Header("Main Load Events"), Space(5f)]
        public UnityEvent OnLoad;

        /// <summary>
        /// Initializes the cloud save and if a 
        /// </summary>
        public UnityEvent OnInitialize;

        public UnityEvent OnFreshLoad;

        [Header("Load events for different classes"), Space(5f)]

        public UnityEvent<string> MovePlayer;

        public UnityEvent<List<InspectionSaveData>> LoadInspectionList;

        private void Start()
        {
            Instance = this;
            CanSave = false;
            saveHandler = GetComponent<CustomSaveHandler>();
            saveData = GetComponent<SaveData>();

            // Initialize events
            OnLoad ??= new();
            OnInitialize ??= new();
            OnFreshLoad ??= new();
            MovePlayer ??= new();
            LoadInspectionList ??= new();

            AddListeners();

#if UNITY_EDITOR
            Initialize();
#endif
        }

        private void AddListeners()
        {
            OnLoad.AddListener(() =>
            {
                LoadInspectionList?.Invoke(saveData.InspectionLog);
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
                saveHandler.Save();
            }
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
        /// Wrapper method for the <see cref="CustomSaveHandler.OnFreshLoad"/> event.
        /// </summary>
        public void OnLoadRestart()
        {
            TriggerDelete();
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
    }
}
