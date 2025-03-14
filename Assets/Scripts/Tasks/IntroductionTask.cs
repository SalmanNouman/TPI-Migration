using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VARLab.Velcro;
using VARLab.Navigation.PointClick;

namespace VARLab.DLX
{
    /// <summary>
    ///     Handles the first task of the inspection process (Conversation with the studio manager before the inspection starts)
    /// </summary>
    /// <remarks>
    ///     The Introduction Task follows this flow:
    ///     1. Player reaches Introduction waypoint → <see cref="CheckCurrentWaypoint"/> is called
    ///     2. After a short delay (<see cref="conversationDelay"/>), task starts → <see cref="HandleTask"/> is called
    ///     3. <see cref="OnTaskStarted"/> event is invoked → Connected to <see cref="ConversationBuilder.HandleDisplayConversation"/>
    ///     4. When conversation ends → <see cref="CompleteTask"/> is called
    ///     5. <see cref="OnTaskCompleted"/> event is invoked → Connected to next task
    ///     6. If player enters restricted area → <see cref="CheckCurrentPoi"/> is called
    ///     7. <see cref="OnTaskFailed"/> event is invoked → Connected to <see cref="InformationDialog.HandleDisplayUI"/>
    /// </remarks>
    public class IntroductionTask : Tasks
    {
        #region Fields

        [SerializeField, Tooltip("Reference to the Reception POI (allowed area)")]
        private Poi receptionPoi;

        [SerializeField, Tooltip("Reference to the Information Dialog ScriptableObject for task failure")]
        private InformationDialogSO introductionExitSO;

        [SerializeField, Tooltip("The waypoint that starts this introduction task")]
        private Waypoint introductionWaypoint;

        [SerializeField, Tooltip("Delay time before starting the conversation")]
        private float conversationDelay = 0.5f;

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
        ///     Used to prevent task failure after completion
        /// </remarks>
        private bool isTaskCompleted = false;

        /// <summary>
        ///     Reference to the current POI the player is in
        /// </summary>
        /// <remarks>
        ///     Updated in <see cref="CheckCurrentPoi"/>
        /// </remarks>
        private Poi currentPoi;

        #endregion

        #region Events

        // Note: OnTaskStarted, OnTaskCompleted, and OnTaskFailed are inherited from <see cref="Task.cs"/>
        
        /// <summary>
        ///     Event triggered when the task starts
        /// </summary>
        /// <remarks>
        ///     In Inspector connections:
        ///     - Connect to: <see cref="ConversationBuilder.HandleDisplayConversation"/>
        ///     - Purpose: Displays the introduction conversation when task starts
        /// </remarks>
        // public UnityEvent OnTaskStarted; (inherited from <see cref="Task.cs"/>)
        
        /// <summary>
        ///     Event triggered when the task is completed
        /// </summary>
        /// <remarks>
        ///     In Inspector connections:
        ///     - Should be connected to trigger the next task or scene transition
        /// </remarks>
        // public UnityEvent OnTaskCompleted; (inherited from <see cref="Task.cs"/>)
        
        /// <summary>
        ///     Event triggered when the task fails
        /// </summary>
        /// <remarks>
        ///     In Inspector connections:
        ///     - Connect to: <see cref="InformationDialog.HandleDisplayUI"/>
        ///     - Purpose: Displays warning dialog when player enters restricted area
        ///     - Passes: <see cref="introductionExitSO"/> containing failure message
        /// </remarks>
        // public UnityEvent OnTaskFailed; (inherited from <see cref="Task.cs"/>)

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
            if (introductionWaypoint == null)
            {
                introductionWaypoint = GetComponent<Waypoint>();
                Debug.Log("IntroductionTask: Auto-assigned current waypoint as introduction waypoint");
            }
        }

        /// <summary>
        ///     Checks if the waypoint that the player has reached is the introduction waypoint.
        /// </summary>
        /// <remarks>
        ///     In Inspector connections:
        ///     - Connected from: <see cref="PointClickNavigation.WalkCompleted"/> event
        ///     - Purpose: Detects when player reaches the Introduction waypoint
        ///     - Workflow: Player reaches waypoint → WalkCompleted event → CheckCurrentWaypoint → DelayedTaskStart coroutine
        /// </remarks>
        /// <param name="waypoint">The waypoint that the player has reached</param>
        public void CheckCurrentWaypoint(Waypoint waypoint)
        {
            // Check if this is the introduction waypoint and the task hasn't started yet
            if (!isTaskStarted && !isTaskCompleted && waypoint == introductionWaypoint)
            {
                Debug.Log("IntroductionTask: Player reached the introduction waypoint: " + waypoint.name);
                
                // Start coroutine instead of immediately calling HandleTask()
                StartCoroutine(DelayedTaskStart());
            }
        }

        /// <summary>
        ///     Coroutine that adds a short delay before starting the introduction conversation.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Called by: <see cref="CheckCurrentWaypoint"/> when player reaches the waypoint
        ///     - Waits for: <see cref="conversationDelay"/> seconds
        ///     - Then calls: <see cref="HandleTask"/> to initiate the introduction task
        /// </remarks>
        private IEnumerator DelayedTaskStart()
        {
            // Wait for the specified delay time before starting the conversation
            yield return new WaitForSeconds(conversationDelay);
            
            Debug.Log("IntroductionTask: Starting introduction task after delay.");
            HandleTask();
        }

        /// <summary>
        ///     Initiates the introduction task when player reaches the manager waypoint.
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
                Debug.Log("IntroductionTask: Task has already been completed.");
                return;
            }

            // Start task if not already started
            if (!isTaskStarted)
            {
                isTaskStarted = true;
                OnTaskStarted?.Invoke();
                Debug.Log("IntroductionTask: Task started successfully.");
            }
        }

        /// <summary>
        ///     Completes the introduction task and triggers transition to the next task.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Called when: The introduction conversation with the manager ends
        ///     - Updates: Sets <see cref="isTaskCompleted"/> to true
        ///     - Invokes: <see cref="Tasks.OnTaskCompleted"/> event
        ///     
        ///     In Inspector connections:
        ///     - Connect from: <see cref="ConversationBuilder.OnConversationCompleted"/> in the Inspector
        ///     - <see cref="Tasks.OnTaskCompleted"/> should be connected to trigger the next task or scene transition
        /// </remarks>
        public void CompleteTask()
        {
            // Early return if task is not started or already completed
            if (!isTaskStarted || isTaskCompleted)
                return;

            isTaskCompleted = true;
            
            // Trigger task completed event
            OnTaskCompleted?.Invoke();
            
            Debug.Log("IntroductionTask: Task completed. Ready to proceed to the next task.");         
        }

        /// <summary>
        ///     Checks if player entered a restricted area and fails the task if needed.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Connected from: <see cref="PoiHandler.OnPoiEnter"/> in the Inspector
        ///     - Purpose: Monitors player's location to prevent entering restricted areas
        ///     - Fails the task if: Player enters any POI other than <see cref="receptionPoi"/> before task completion
        ///     - Invokes: <see cref="Tasks.OnTaskFailed"/> event when player enters restricted area
        ///     
        ///     In Inspector connections:
        ///     - <see cref="Tasks.OnTaskFailed"/> should be connected to:
        ///       <see cref="InformationDialog.HandleDisplayUI"/> to show failure message
        /// </remarks>
        /// <param name="poi">The POI that was entered</param>
        public void CheckCurrentPoi(Poi poi)
        {
            currentPoi = poi;

            // Check if player entered a restricted area during introduction task
            if (!isTaskCompleted && poi != receptionPoi && !isTaskStarted)
            {
                Debug.Log($"IntroductionTask: Player entered restricted area: {poi.PoiName}. Task failed.");
                
                // Invoke task failed event
                OnTaskFailed?.Invoke();
            }
        }

        #endregion
    }
}
