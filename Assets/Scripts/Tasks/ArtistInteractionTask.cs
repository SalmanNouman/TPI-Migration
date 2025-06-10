using System.Collections;
using UnityEngine;
using VARLab.Navigation.PointClick;

namespace VARLab.DLX
{
    /// <summary>
    ///     Handles the interaction with the Piercer NPC to request and display the procedure tray
    /// </summary>
    /// <remarks>
    ///     The Piercer Interaction Task follows this flow:
    ///     1. Player reaches Piercer waypoint → <see cref="CheckCurrentWaypoint"/> is called
    ///     2. After a short delay (<see cref="conversationDelay"/>), task starts → <see cref="HandleTask"/> is called
    ///     3. <see cref="OnTaskStarted"/> event is invoked → Connected to <see cref="ConversationBuilder.HandleDisplayConversation"/>
    ///     4. When conversation ends → <see cref="CompleteTask"/> is called
    ///     5. <see cref="OnTaskCompleted"/> event is invoked → Shows the procedure tray
    /// </remarks>
    public class ArtistInteractionTask : Tasks
    {
        #region Fields

        [SerializeField, Tooltip("The waypoint that starts the artist interaction")]
        private Waypoint artistWaypoint;

        [SerializeField, Tooltip("Delay time before starting the conversation")]
        private float conversationDelay = 0.5f;

        [SerializeField, Tooltip("The prepared procedure tray that will be revealed after the conversation")]
        private GameObject preparedProcedureTray;

        [SerializeField, Tooltip("The artist cutout that will be hidden after the conversation")]
        private GameObject artistCutout;

        /// <summary>
        ///     Tracks if the task has been started
        /// </summary>
        /// <remarks>
        ///     Set to true in <see cref="HandleTask"/>
        ///     Used to prevent duplicate task activation
        /// </remarks>
        private bool isTaskStarted = false;

        /// <summary>
        ///     Tracks if the task has been completed
        /// </summary>
        /// <remarks>
        ///     Set to true in <see cref="CompleteTask"/>
        ///     Used to track if the procedure tray has been revealed
        /// </remarks>
        private bool isTaskCompleted = false;

        #endregion

        #region Events

        // Note: OnTaskStarted, OnTaskCompleted, and OnTaskFailed are inherited from <see cref="Task.cs"/>

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes events to prevent null reference errors and auto-assigns waypoint reference.
        /// </summary>
        private void Awake()
        {
            OnTaskStarted ??= new();
            OnTaskCompleted ??= new();
            OnTaskFailed ??= new();

            // Auto-assign if not set and this component is on a waypoint
            if (artistWaypoint == null)
            {
                artistWaypoint = GetComponent<Waypoint>();
                Debug.Log("ArtistInteractionTask: Auto-assigned current waypoint as piercer waypoint");
            }
        }

        private void Start()
        {
            // Ensure the procedure tray is initially inactive
            if (preparedProcedureTray != null)
            {
                preparedProcedureTray.SetActive(false);
            }
            else
            {
                Debug.LogWarning("ArtistInteractionTask: Prepared procedure tray reference is missing");
            }

            // Ensure the piercer cutout is initially active
            if (artistCutout != null)
            {
                artistCutout.SetActive(true);
            }
            else
            {
                Debug.LogWarning("ArtistInteractionTask: Piercer cutout reference is missing");
            }
        }

        /// <summary>
        ///     Checks if the waypoint that the player has reached is the artist waypoint.
        /// </summary>
        /// <remarks>
        ///     In Inspector connections:
        ///     - Connected from: <see cref="PointClickNavigation.WalkCompleted"/> event
        ///     - Purpose: Detects when player reaches the Artist waypoint
        ///     - Workflow: Player reaches waypoint → WalkCompleted event → CheckCurrentWaypoint → DelayedTaskStart coroutine
        /// </remarks>
        /// <param name="waypoint">The waypoint that the player has reached</param>
        public void CheckCurrentWaypoint(Waypoint waypoint)
        {
            // Check if this is the piercer waypoint and the task hasn't started yet
            if (!isTaskStarted && !isTaskCompleted && waypoint == artistWaypoint)
            {
                Debug.Log("ArtistInteractionTask: Player reached the artist waypoint: " + waypoint.name);

                // Start coroutine instead of immediately calling HandleTask()
                StartCoroutine(DelayedTaskStart());
            }
        }

        /// <summary>
        ///     Coroutine that adds a short delay before starting the artist conversation.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Called by: <see cref="CheckCurrentWaypoint"/> when player reaches the waypoint
        ///     - Waits for: <see cref="conversationDelay"/> seconds
        ///     - Then calls: <see cref="HandleTask"/> to initiate the artist interaction
        /// </remarks>
        private IEnumerator DelayedTaskStart()
        {
            // Wait for the specified delay time before starting the conversation
            yield return new WaitForSeconds(conversationDelay);

            Debug.Log("ArtistInteractionTask: Starting artist interaction after delay.");
            HandleTask();
        }

        /// <summary>
        ///     Initiates the artist interaction when player reaches the artist waypoint.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Called by: <see cref="DelayedTaskStart"/> coroutine after delay
        ///     - Updates: Sets <see cref="isTaskStarted"/> to true
        ///     - Invokes: <see cref="Tasks.OnTaskStarted"/> event
        ///     
        ///     In Inspector connections:
        ///     - <see cref="Tasks.OnTaskStarted"/> should be connected to:
        ///       <see cref="ConversationBuilder.HandleDisplayConversation"/> to start the conversation
        /// </remarks>
        public override void HandleTask()
        {
            // Early return if task is already completed
            if (isTaskCompleted)
            {
                Debug.Log("ArtistInteractionTask: Task has already been completed.");
                return;
            }

            // Start task if not already started
            if (!isTaskStarted)
            {
                isTaskStarted = true;
                OnTaskStarted?.Invoke();
                CompleteTask();
                Debug.Log("ArtistInteractionTask: Task started successfully.");
            }
        }

        /// <summary>
        ///     Completes the artist interaction task and reveals the procedure tray.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Called when: The conversation with the artist ends
        ///     - Updates: Sets <see cref="isTaskCompleted"/> to true
        ///     - Actions: Activates the procedure tray and hides the artist cutout
        ///     - Invokes: <see cref="Tasks.OnTaskCompleted"/> event
        ///     
        ///     In Inspector connections:
        ///     - Connect from: <see cref="ConversationBuilder.OnConversationCompleted"/> in the Inspector
        /// </remarks>
        public void CompleteTask()
        {
            // Early return if task is not started or already completed
            if (!isTaskStarted || isTaskCompleted)
                return;

            isTaskCompleted = true;

            // Show the procedure tray
            if (preparedProcedureTray != null)
            {
                preparedProcedureTray.SetActive(true);
                Debug.Log("ArtistInteractionTask: Procedure tray revealed.");
            }

            // Hide the piercer cutout
            /*if (artistCutout != null)
            {
                artistCutout.SetActive(false);
                Debug.Log("ArtistInteractionTask: artist cutout hidden.");
            }*/

            // Disable the piercer cutout collider
            var boxCollider = artistCutout.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }
            else
            {
                Debug.LogWarning("ArtistInteractionTask: BoxCollider component not found on artist cutout.");
            }

            // Trigger task completed event
            OnTaskCompleted?.Invoke();

            Debug.Log("ArtistInteractionTask: Task completed.");
        }

        /// <summary>
        /// Loads the state of interaction with the Piercer NPC.
        /// Called from SaveDataSupport through event <see cref="SaveDataSupport.LoadPiercerInteraction"/>
        /// </summary>
        public void LoadArtistInteraction(bool isCompleted)
        {
            preparedProcedureTray.SetActive(isCompleted);
            var boxCollider = artistCutout.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.enabled = !isCompleted;
            }
            else
            {
                Debug.LogWarning("ArtistInteractionTask: BoxCollider component not found on artist cutout.");
            }
            isTaskCompleted = isCompleted;
        }

        #endregion
    }
}
