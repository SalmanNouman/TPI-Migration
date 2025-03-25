using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using VARLab.CloudSave;
using VARLab.DeveloperTools;

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

        private bool saveFlag = false;
        private bool requestDone = false;

        private Queue<Action> saveQueue = new();

        [Header("Initial Session Events")]
        public UnityEvent SaveFileNotFound = new();
        public UnityEvent SaveFileFound = new();

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

            // starts a save background loop
            StartCoroutine(SaveLoop());

            OnSaveComplete.AddListener(SetSaveRequestCompletion);
            OnLoadComplete.AddListener(SetLoadCompletion);
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

            // Call the appropriate events if the Blob name is one of
            // the available save files.
            var tokens = args.Data.Split("\"");
            (tokens.Contains(Blob) ? SaveFileFound : SaveFileNotFound)?.Invoke();
        }
    }
}
