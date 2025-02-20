using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
        /// </summary>
        public UnityEvent<List<Log>> OnLogAdded;

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes the log list and ensures that the OnLogAdded event is assigned to prevent null reference errors.
        /// </summary>
        private void Start()
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
        ///     Records photo deletion events. Implementation pending.
        /// </summary>
        public void LogDeletedPhoto()
        {
            // TODO: This method will be implemented later, once we add the image manager
        }

        /// <summary>
        ///     Records inspection deletion events. Implementation pending.
        /// </summary>
        public void LogDeletedInspection()
        {
            // TODO: This method will be implemented later, once we add the ability to delete inspections
        }

        /// <summary>
        ///     Loads saved activity log list. Implementation pending.
        /// </summary>
        /// <param name="logs">The logs to load.</param>
        public void LoadSavedLogs(List<Log> logs)
        {
            // TODO: This method will be implemented later, once we add the ability to save and load
        }

        #endregion
    }
}