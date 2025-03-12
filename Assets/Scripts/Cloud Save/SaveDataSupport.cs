using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

        public UnityEvent OnLoad;

        public UnityEvent OnInitialize;

        public UnityEvent OnFreshLoad;

        public UnityEvent<string> MovePlayer;

        private void Start()
        {
            Instance = this;
            CanSave = false;
            saveHandler = GetComponent<CustomSaveHandler>();
            saveData = GetComponent<SaveData>();

#if UNITY_EDITOR
            Initialize();
#endif
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
    }
}
