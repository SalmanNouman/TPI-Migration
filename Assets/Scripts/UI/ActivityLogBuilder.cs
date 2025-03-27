using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace VARLab.DLX
{
    public class ActivityLogBuilder : TabContentBuilder
    {
        private List<Log> activityLog;
        private Foldout selectedFoldout;

        /// <summary>
        /// Gets the updated activity log every time a new log is added to the list.
        /// </summary>
        /// <param name="logs">list of logs</param>
        public void GetActivityLog(List<Log> logs)
        {
            activityLog ??= new List<Log>();
            activityLog = logs;
        }

        /// <summary>
        /// When the inspection review window is opened and closed this is triggered.
        /// True: Loads the content on window open
        /// False: Clears the content container when the inspection review window is closed.
        /// </summary>
        /// <param name="open"></param>
        public void HandleDisplayActivityLog(bool open)
        {
            if (open)
            {
                LoadContent();
            }
            else
            {
                ContentContainer.Clear();
            }
        }

        /// <summary>
        /// If the Inspection Review window is opened we load the tab content.
        /// Loads the empty content message if the activity log is empty.
        /// </summary>
        private void LoadContent()
        {
            if (activityLog == null || activityLog.Count <= 0)
            {
                DisplayEmptyLogMessage();
                return;
            }

            HideEmptyLogMessage();
            PopulateActivityLog();
        }

        /// <summary>
        /// If the activity log list contains logs the activity log gets populated
        /// This method is used to create the Foldout.
        /// </summary>
        private void PopulateActivityLog()
        {
            ContentContainer.Clear();

            Foldout currentFoldout = null;

            foreach (var log in activityLog)
            {
                if (log.IsPrimary)
                {
                    Foldout foldout = new() { text = log.Message };
                    SetUpFoldout(foldout);
                    ContentContainer.Add(foldout);
                    currentFoldout = foldout;
                }
                else
                {
                    Label label = new() { text = log.Message };
                    if (currentFoldout != null)
                    {
                        currentFoldout.Add(label);
                    }
                }
            }
        }

        /// <summary>
        /// Sets up the callback for the primary log foldout.
        /// </summary>
        /// <param name="foldout"></param>
        private void SetUpFoldout(Foldout foldout)
        {
            foldout.value = false;

            // Register callback to change font colour when selected
            foldout.RegisterCallback<ClickEvent>(evt =>
            {
                if (selectedFoldout != null && selectedFoldout != foldout)
                {
                    selectedFoldout.value = false;
                }
                // Highlight the exit message, if it was logged
                if (foldout.childCount > 0)
                {
                    Label exitLabel = foldout.ElementAt(foldout.childCount - 1).Q<Label>();
                    if (exitLabel != null && exitLabel.text.Contains("Exit"))
                    {
                        exitLabel.AddToClassList("activity-log-exit-label");
                    }
                }
                selectedFoldout = foldout;
            });
        }
    }
}
