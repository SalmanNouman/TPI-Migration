using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static VARLab.DLX.SaveData;

namespace VARLab.DLX
{
    /// <summary>
    ///     Manages the collection (list) of player activity logs.
    /// </summary>
    /// <remarks>
    ///     Maintains activity logs in a centralized list and notifies listeners of updates through <see cref="OnLogAdded"/>.
    ///     Provides methods for recording and managing player activity logs.
    /// </remarks>
    public class ActivityLog : MonoBehaviour
    {
        #region Fields

        // Determines if logging is enabled. Logging is disabled (false) by default and can be enabled externally when needed.
        public bool CanLog = false;

        // Centralized list of all recorded activity logs.
        public List<Log> ActivityLogList;

        #endregion

        #region Events

        /// <summary>
        ///     Event triggered when a new log entry is added, providing the updated log list.
        ///     <see cref="ActivityLogBuilder.GetActivityLog(List{Log})"/>
        /// </summary>
        public UnityEvent<List<Log>> OnLogAdded;

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes the log list and ensures that the OnLogAdded event is assigned to prevent null reference errors.
        /// </summary>
        private void Awake()
        {
            ActivityLogList = new List<Log>();
            OnLogAdded ??= new UnityEvent<List<Log>>();
        }

        /// <summary>
        ///     Creates and adds a new log entry to the activity log list.
        ///     Notifies listeners of the update through <see cref="OnLogAdded"/> event.
        /// </summary>
        /// <param name="isPrimary">Whether this is a primary (true) or secondary (false) log.</param>
        /// <param name="message">The message to record.</param>
        private void AddLog(bool isPrimary, string message)
        {
            if (!CanLog) // If logging is disabled, return immediately (do nothing).
            {
                return;
            }

            Log newLog = new Log(isPrimary, message);
            ActivityLogList.Add(newLog);
            OnLogAdded.Invoke(ActivityLogList);
        }

        /// <summary>
        ///     Records a primary log entry, typically for POI enter events.
        /// </summary>
        /// <param name="message">The message to record.</param>
        private void AddPrimaryLog(string message)
        {
            AddLog(true, message);
        }

        /// <summary>
        ///     Records a secondary log entry, typically for inspection events.
        /// </summary>
        /// <param name="message">The message to record.</param>
        private void AddSecondaryLog(string message)
        {
            AddLog(false, message);
        }

        /// <summary>
        ///     Records a primary log when a player enters a POI.
        /// </summary>
        /// <param name="poi">The POI where the player has entered.</param>
        public void LogPoiEnter(Poi poi)
        {
            string elapsedTime = TimerManager.Instance.GetElapsedTime();
            string message = $"{elapsedTime} Entered {poi.PoiName}";
            AddPrimaryLog(message);
        }

        /// <summary>
        ///     Records a secondary log when a player interacts with an inspectable object.
        /// </summary>
        /// <param name="obj">The object that the player clicked.</param>
        public void LogObjectClicked(InspectableObject obj)
        {
            string elapsedTime = TimerManager.Instance.GetElapsedTime();
            string message = $"{elapsedTime} {obj.Name} - Visual Inspection";
            AddSecondaryLog(message);
        }

        /// <summary>
        ///     Records a secondary log for an inspection result with photo status and compliance status.
        /// </summary>
        /// <param name="inspectionData">The inspection details to record.</param>
        public void LogObjectCompliancy(InspectionData inspectionData)
        {
            string elapsedTime = TimerManager.Instance.GetElapsedTime();
            string objectName = inspectionData.Obj.Name;
            string photoStatus = inspectionData.HasPhoto ? "with photo" : "without photo";
            string complianceStatus = inspectionData.IsCompliant ? "compliant" : "non compliant";

            string message = $"{elapsedTime} {objectName} - Visual Inspection {photoStatus} - {complianceStatus}";
            AddSecondaryLog(message);
        }

        /// <summary>
        ///     Records a secondary log when a player takes a photo of an inspectable object.
        /// </summary>
        /// <param name="obj">The inspectable object that the player has taken a photo of.</param>
        public void LogPhotoTaken(InspectableObject obj)
        {
            string elapsedTime = TimerManager.Instance.GetElapsedTime();
            string message = $"{elapsedTime} {obj.Name} - Photo Taken";
            AddSecondaryLog(message);
        }

        /// <summary>
        ///     Records a secondary log when a player exits a POI.
        /// </summary>
        /// <param name="poi">The POI where the player has exited.</param>
        public void LogExitPoi(Poi poi)
        {
            string elapsedTime = TimerManager.Instance.GetElapsedTime();
            string message = $"{elapsedTime} Exited {poi.PoiName}";
            AddSecondaryLog(message);
        }

        /// <summary>
        ///    Records a primary log when the user deletes a photo from an inspectable object.
        /// </summary>
        /// <param name="objName"> The name of the inspectable object </param>
        public void LogDeletedPhoto(string objName)
        {
            string elapsedTime = TimerManager.Instance.GetElapsedTime();
            string log = $"{elapsedTime} {objName} - Photo Deleted";
            AddSecondaryLog(log);
        }

        /// <summary>
        ///    Records a primary log when the user deletes an inspection.
        /// </summary>
        /// <param name="objName"> The name of the inspectable object </param>
        public void LogDeletedInspection(string objName)
        {
            string elapsedTime = TimerManager.Instance.GetElapsedTime();
            string log = $"{elapsedTime} {objName} - Visual Inspection Deleted";
            AddSecondaryLog(log);
        }

        /// <summary>
        ///     Changes the flag that allows logs to be saved. 
        ///     If the count of primary logs is 0, a new primary log is created indicating the start of the inspection.
        /// </summary>
        /// <param name="canLog">What to set canLog to</param>
        public void SetCanLog(bool canLog)
        {
            CanLog = canLog;
            if (ActivityLogList.Count == 0)
            {
                string elapsedTime = TimerManager.Instance.GetElapsedTime();
                string primaryLog = $"{elapsedTime} - Inspection Started";
                AddPrimaryLog(primaryLog);
            }
        }

        /// <summary>
        ///     Gets saved activity logs from the cloud save system.
        ///     This method is called by the SaveDataSupport.LoadActivityList event.
        /// </summary>
        /// <param name="savedLogs">The saved activity logs from the cloud save system.</param>
        public void GetSavedLogs(List<ActivityData> savedLogs)
        {
            // Create a temporary list to store the logs
            List<Log> tempList = new();
            // Early return if no logs are found
            if (savedLogs == null || savedLogs.Count == 0)
            {
                return;
            }
            // Fill in the temporary list with the saved logs
            foreach (var savedLog in savedLogs)
            {
                Log log = new Log(savedLog.IsPrimary, savedLog.LogString);
                tempList.Add(log);
            }

            LoadSavedLogs(tempList);
        }

        /// <summary>
        ///     Loads saved activity log list and notifies listeners of the update.
        /// </summary>
        /// <param name="logs">The logs to load.</param>
        public void LoadSavedLogs(List<Log> logs)
        {
            if (logs == null || logs.Count == 0)
            {
                ActivityLogList.Clear();
                OnLogAdded.Invoke(ActivityLogList);
                return;
            }
            // Clear the current log list and update the UI
            ActivityLogList.Clear();
            OnLogAdded.Invoke(ActivityLogList);

            CanLog = true;

            // Load logs
            ActivityLogList = logs;

            // Find the last primary log
            Log lastPrimaryLog = ActivityLogList.LastOrDefault(log => log.IsPrimary);
            if (lastPrimaryLog != null)
            {
                string elapsedTime = TimerManager.Instance.GetElapsedTime();
                string lastPrimaryMessage = "";
                if (lastPrimaryLog.Message.Contains("Simulation Loaded"))
                {
                    // Find the index of the colon that comes after "Simulation Loaded"
                    int simLoadedIndex = lastPrimaryLog.Message.IndexOf("Simulation Loaded");
                    int colonIndex = lastPrimaryLog.Message.IndexOf(":", simLoadedIndex);
                    lastPrimaryMessage = lastPrimaryLog.Message.Substring(colonIndex + 1).Trim();
                }
                else
                {
                    lastPrimaryMessage = lastPrimaryLog.Message.Substring(lastPrimaryLog.Message.IndexOf(" ") + 1);
                }
                // Create simulation loaded log
                string loadLog = $"{elapsedTime} Simulation Loaded: {lastPrimaryMessage}";
                ActivityLogList.Add(new Log(true, loadLog));
            }

            OnLogAdded.Invoke(ActivityLogList);

        }

        #endregion
    }
}