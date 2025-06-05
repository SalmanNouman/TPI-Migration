using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VARLab.Navigation.PointClick;

namespace VARLab.DLX
{
    /// <summary>
    ///     Handles the handwashing task in the inspection process
    /// </summary>
    /// <remarks>
    ///     The Handwashing Task follows this flow:
    ///     1. Player completes introduction and office tasks
    ///     2. Player reaches Handwashing waypoint → <see cref="CheckCurrentWaypoint"/> is called
    ///     3. Camera pans to handwashing station → <see cref="OnCameraPan"/> event is invoked
    ///     4. Information dialog is shown → <see cref="OnShowInformationDialog"/> event is invoked
    ///     5. When dialog is confirmed, handwashing animation and audio play
    ///     6. Task completes → <see cref="OnTaskCompleted"/> event is invoked
    ///     7. If player tries to inspect anything before handwashing → <see cref="OnTaskFailed"/> is invoked
    /// </remarks>
    public class HandwashingMovementTile : Tasks
    {
        #region Fields

        [Header("References")]
        [SerializeField, Tooltip("Reference to the handwashing station GameObject")]
        private GameObject handwashingStation;

        [SerializeField, Tooltip("Reference to the running water audio source")]
        private AudioSource runningWater;

        [SerializeField, Tooltip("Reference to the water visual effect GameObject")]
        private GameObject handwashingWater;

        [SerializeField, Tooltip("Reference to the Information Dialog for handwashing instructions")]
        private InformDialog handwashInstructionsDialog;

        [SerializeField, Tooltip("Reference to the Information Dialog for task failure")]
        private InformDialog handwashFailureDialog;

        [SerializeField, Tooltip("Reference to the Information Dialog for task failure")]
        private InformDialog introductionExitDialog;

        [SerializeField, Tooltip("The waypoint that starts this handwashing task")]
        private Waypoint handwashingWaypoint;

        [Header("Settings")]
        [SerializeField, Tooltip("Duration of the handwashing animation/audio")]
        private float handwashingDuration = 5f;

        [SerializeField, Tooltip("Delay before showing the information dialog after camera pan")]
        private float dialogDelay = 0.5f;

        // Task state tracking
        private bool isTaskStarted = false;
        private bool isTaskCompleted = false;
        private bool isIntroductionCompleted = false;
        private bool isOfficeTaskCompleted = false;

        #endregion

        #region Events

        /// <summary>
        ///     Event triggered to show the information dialog
        /// </summary>
        /// <remarks>
        ///     In Inspector connections:
        ///     - Connect to: <see cref="InformationDialog.HandleDisplayUI"/>
        /// </remarks>
        public UnityEvent<InformDialog> OnShowInformationDialog;

        /// <summary>
        ///     Event triggered when the handwashing audio/animation should start
        /// </summary>
        public UnityEvent OnHandwashingStart;

        /// <summary>
        ///     Event triggered when the handwashing audio/animation should end
        /// </summary>
        public UnityEvent OnHandwashingEnd;

        /// <summary>
        ///     Event triggered when the task fails and scene should restart
        /// </summary>
        public UnityEvent RestartScene;

        #endregion

        #region Unity Lifecycle Methods

        private void Awake()
        {
            // Initialize events to prevent null reference errors
            OnTaskStarted ??= new();
            OnTaskCompleted ??= new();
            OnTaskFailed ??= new();
            OnShowInformationDialog ??= new();
            OnHandwashingStart ??= new();
            OnHandwashingEnd ??= new();
            RestartScene ??= new();

            // Auto-assign if not set and this component is on a waypoint
            if (handwashingWaypoint == null)
            {
                handwashingWaypoint = GetComponent<Waypoint>();
                Debug.Log("HandwashingMovementTile: Auto-assigned current waypoint as handwashing waypoint");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Checks if the waypoint that the player has reached is the handwashing waypoint.
        /// </summary>
        /// <param name="waypoint">The waypoint that the player has reached</param>
        public void CheckCurrentWaypoint(Waypoint waypoint)
        {
            // Check if this is the handwashing waypoint and the task hasn't started yet
            if (!isTaskStarted && !TPISceneManager.HandWashingCompleted && waypoint == handwashingWaypoint)
            {
                Debug.Log("HandwashingMovementTile: Player reached the handwashing waypoint: " + waypoint.name);

                // Check if introduction and office tasks are completed
                if (!TPISceneManager.IntroductionCompleted)
                {
                    Debug.Log("HandwashingMovementTile: Previous tasks not completed. Showing introduction exit dialog.");
                    introductionExitDialog.SetPrimaryAction(() => RestartScene?.Invoke());
                    OnShowInformationDialog?.Invoke(introductionExitDialog);
                    return;
                }

                // Start the handwashing task
                HandleTask();
            }
        }

        /// <summary>
        ///     Initiates the handwashing task when player reaches the handwashing waypoint.
        /// </summary>
        public override void HandleTask()
        {
            // Early return if task is already completed
            if (TPISceneManager.HandWashingCompleted)
            {
                Debug.Log("HandwashingMovementTile: Task has already been completed.");
                return;
            }

            // Start task if not already started
            if (!isTaskStarted)
            {
                isTaskStarted = true;
                OnTaskStarted?.Invoke();
                Debug.Log("HandwashingMovementTile: Task started successfully.");

                // Show information dialog after a short delay
                StartCoroutine(ShowDialogAfterDelay());
            }
        }

        /// <summary>
        ///     Coroutine that adds a short delay before showing the information dialog.
        /// </summary>
        private IEnumerator ShowDialogAfterDelay()
        {
            // Wait for the specified delay time before showing dialog
            yield return new WaitForSeconds(dialogDelay);

            Debug.Log("HandwashingMovementTile: Showing handwashing instructions dialog.");

            // Set up the dialog's primary action to start handwashing when confirmed
            handwashInstructionsDialog.SetPrimaryAction(StartHandwashing);

            // Show the dialog
            OnShowInformationDialog?.Invoke(handwashInstructionsDialog);
        }

        /// <summary>
        ///     Starts the handwashing process after dialog confirmation.
        /// </summary>
        public void StartHandwashing()
        {
            Debug.Log("HandwashingMovementTile: Starting handwashing process.");
            StartCoroutine(HandwashingCoroutine());
        }

        /// <summary>
        ///     Coroutine that handles the handwashing process with audio and visual effects.
        /// </summary>
        private IEnumerator HandwashingCoroutine()
        {
            // Trigger handwashing start event
            // Can show toast here
            OnHandwashingStart?.Invoke();

            // Play water sound
            if (runningWater != null)
            {
                runningWater.Play();
            }

            // Show water visual effect
            if (handwashingWater != null)
            {
                handwashingWater.SetActive(true);
            }

            // Wait for handwashing duration
            yield return new WaitForSeconds(handwashingDuration);

            // Turn off water effect
            if (handwashingWater != null)
            {
                handwashingWater.SetActive(false);
            }

            // Stop water sound
            if (runningWater != null && runningWater.isPlaying)
            {
                runningWater.Stop();
            }

            // Trigger handwashing end event
            // Can hide toast here
            OnHandwashingEnd?.Invoke();

            // Wait a moment before completing the task
            yield return new WaitForSeconds(1f);

            // Complete the task
            CompleteTask();
        }

        /// <summary>
        ///     Completes the handwashing task and triggers transition to the next task.
        /// </summary>
        public void CompleteTask()
        {
            // Early return if task is not started or already completed
            if (!isTaskStarted || TPISceneManager.HandWashingCompleted)
                return;

            TPISceneManager.HandWashingCompleted = true;

            // Trigger task completed event
            OnTaskCompleted?.Invoke();

            Debug.Log("HandwashingMovementTile: Task completed. Ready to proceed to the next task.");
        }

        /// <summary>
        ///     Called when the introduction task is completed.
        /// </summary>
        public void OnIntroductionCompleted()
        {
            isIntroductionCompleted = true;
            Debug.Log("HandwashingMovementTile: Introduction task completed.");
        }

        /// <summary>
        ///     Called when the office task is completed.
        /// </summary>
        public void OnOfficeTaskCompleted()
        {
            isOfficeTaskCompleted = true;
            Debug.Log("HandwashingMovementTile: Office task completed. Handwashing task is now available.");
        }

        /// <summary>
        ///     Called when the player tries to inspect something before washing hands.
        /// </summary>
        public void FailedInspectionAttempt()
        {
            // Setup the failure dialog's primary action to restart the scene
            handwashFailureDialog.SetPrimaryAction(() => StartCoroutine(Fade.Instance.FadeButtonCoroutine(RestartScene.Invoke)));

            OnShowInformationDialog?.Invoke(handwashFailureDialog);
            // Trigger task failed event
            OnTaskFailed?.Invoke();
        }

        /// <summary>
        /// The tasks are set True when user click on Continue button
        /// </summary>
        public void LoadSaveTask()
        {
            isTaskStarted = true;
            TPISceneManager.HandWashingCompleted = true;
        }

        #endregion
    }
}
