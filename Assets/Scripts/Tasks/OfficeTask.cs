using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VARLab.Velcro;
using VARLab.Navigation.PointClick;

namespace VARLab.DLX
{
    /// <summary>
    ///     Handles the second task of the inspection process.
    ///     Manages automatic player movement to the office waypoint and triggers dialogue interactions upon arrival.
    /// </summary>
    /// <remarks>
    ///     The Office Task follows this flow:
    ///     1. IntroductionTask completion → <see cref="HandleTask"/> is called
    ///     2. <see cref="OnTaskStarted"/> event is invoked → Disables navigation, starts auto-movement
    ///     3. When player reaches the office waypoint → After a short delay <see cref="OnWaypointReached"/> event is invoked
    ///     4. The OnWaypointReached event displays the conversation, hides menu, and enables background blur effect
    ///     5. When conversation ends → <see cref="CompleteTask"/> is called through <see cref="ConversationBuilder.OnWindowHide"/>
    ///     6. <see cref="OnTaskCompleted"/> event is invoked → Connected to next task
    /// </remarks>
    public class OfficeTask : Tasks
    {
        #region Fields
        
        [SerializeField, Tooltip("Delay time before triggering the office conversation")]
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
        ///     Tracks if the player is currently being auto-navigated
        /// </summary>
        /// <remarks>
        ///     Set to true when auto-navigation begins
        ///     Used to prevent player interaction during auto-navigation
        /// </remarks>
        private bool isAutoNavigating = false;

        /// <summary>
        ///     Tracks if the office conversation has been shown
        /// </summary>
        /// <remarks>
        ///     Set to true in <see cref="StartConversationAfterDelay"/>
        ///     Used to prevent CompleteTask from being triggered by other conversations
        /// </remarks>
        private bool isOfficeConversationShown = false;

        /// <summary>
        ///     Tracks if the task has been completed
        /// </summary>
        /// <remarks>
        ///     Set to true in <see cref="CompleteTask"/>
        /// </remarks>
        private bool isTaskCompleted = false;

        #endregion

        #region Events

        // Note: OnTaskStarted, OnTaskCompleted are inherited from <see cref="Tasks"/> base class
        
        /// <summary>
        ///     Event triggered when the task starts (after introduction task completion)
        /// </summary>
        /// <remarks>
        ///     In Inspector connections:
        ///     - Connect to: <see cref="PointClickNavigation.Walk"/> with the waypoint that has OfficeTask component
        ///     - Connect to: <see cref="PointClickNavigation.EnableNavigation"/> (false) to disable player controls
        ///     - Connect to: <see cref="PointClickNavigation.EnableCameraPanAndZoom"/> (false) to disable camera controls
        /// </remarks>
        // public UnityEvent OnTaskStarted; (inherited from <see cref="Tasks"/> base class)
        
        /// <summary>
        ///     Event triggered when the player reaches the office waypoint (after a short delay)
        /// </summary>
        /// <remarks>
        ///     In Inspector connections:
        ///     - Connect to: <see cref="ConversationBuilder.HandleDisplayConversation"/> with Office ConversationSO
        ///     - Connect to: <see cref="MenuBuilder.Hide"/> to hide menu buttons
        ///     - Connect to: <see cref="Volume.enabled"/> to enable background blur effect
        /// </remarks>
        public UnityEvent OnWaypointReached;
        
        /// <summary>
        ///     Event triggered when the task is completed
        /// </summary>
        /// <remarks>
        ///     In Inspector connections:
        ///     - Should be connected to trigger the next task or scene transition
        ///     - Note: Navigation and camera controls are re-enabled through <see cref="ConversationBuilder.OnWindowHide"/>
        /// </remarks>
        // public UnityEvent OnTaskCompleted; (inherited from <see cref="Tasks"/> base class)

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes events to prevent null reference errors.
        /// </summary>
        private void Awake()
        {
            OnTaskStarted ??= new();
            OnWaypointReached ??= new();
            OnTaskCompleted ??= new();
        }

        /// <summary>
        ///     Initiates the office task after introduction task is completed.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Connected from: <see cref="IntroductionTask.OnTaskCompleted"/> event
        ///     - Updates: Sets <see cref="isTaskStarted"/> to true
        ///     - Invokes: <see cref="OnTaskStarted"/> event to begin auto-navigation
        ///     
        ///     Note: This will automatically trigger navigation and disable player controls
        ///     through the events connected to OnTaskStarted
        /// </remarks>
        public override void HandleTask()
        {
            // Return early if the task has already been completed
            if (isTaskCompleted)
            {
                return;
            }

            if (!isTaskStarted)
            {
                isTaskStarted = true;
                isAutoNavigating = true;
                OnTaskStarted?.Invoke();
                Debug.Log("OfficeTask: Task started. Auto-navigation to office initiated.");
            }
        }

        /// <summary>
        ///     Handles the event when the player reaches a waypoint.
        ///     Triggers the office conversation after a short delay if it's the office waypoint.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Connected from: <see cref="PointClickNavigation.WalkCompleted"/> event
        ///     - Checks: If <see cref="isAutoNavigating"/> is true and waypoint has OfficeTask component
        ///     - If matching: Sets <see cref="isAutoNavigating"/> to false and starts <see cref="StartConversationAfterDelay"/> coroutine
        /// </remarks>
        /// <param name="waypoint">The waypoint that was reached</param>
        public void HandleWaypointReached(Waypoint waypoint)
        {
            // Early return if the office task is already completed
            if (isTaskCompleted)
            {
                return;
            }

            // Check if this is the waypoint with the OfficeTask component (the office waypoint)
            // and if we are currently auto-navigating
            if (isAutoNavigating && waypoint.GetComponent<OfficeTask>() != null)
            {
                isAutoNavigating = false;
                Debug.Log("OfficeTask: Player reached office waypoint. Starting office conversation after delay.");
                StartCoroutine(StartConversationAfterDelay());
            }
        }

        /// <summary>
        ///     Coroutine that adds a short delay before triggering the office conversation.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Called by: <see cref="HandleWaypointReached"/> when player reaches office waypoint
        ///     - Waits for: <see cref="conversationDelay"/> seconds
        ///     - Then invokes: <see cref="OnWaypointReached"/> event to display the office conversation,
        ///       hide menu buttons, and enable background blur
        /// </remarks>
        private IEnumerator StartConversationAfterDelay()
        {
            // Wait for the specified delay time before starting the conversation
            yield return new WaitForSeconds(conversationDelay);
            Debug.Log("OfficeTask: Office conversation displayed after delay.");
            
            // Set the flag to indicate that the office conversation has been shown
            isOfficeConversationShown = true;

            OnWaypointReached?.Invoke();
        }

        /// <summary>
        ///     Completes the office task and triggers transition to the next task.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Connected from: <see cref="ConversationBuilder.OnWindowHide"/> event
        ///     - Updates: Sets <see cref="isTaskCompleted"/> to true
        ///     - Invokes: <see cref="OnTaskCompleted"/> event to trigger next task
        ///     
        ///     Note: Navigation and camera controls are re-enabled through 
        ///     <see cref="ConversationBuilder.OnWindowHide"/> event connections
        /// </remarks>
        public void CompleteTask()
        {
            // Skip if task not started, already completed, or office conversation not shown yet
            if (!isTaskStarted || isTaskCompleted || !isOfficeConversationShown)
            {
                return;
            }

            isTaskCompleted = true;
            OnTaskCompleted?.Invoke();
            Debug.Log("OfficeTask: Task completed. Player can now freely navigate.");
        }

        #endregion
    }
} 