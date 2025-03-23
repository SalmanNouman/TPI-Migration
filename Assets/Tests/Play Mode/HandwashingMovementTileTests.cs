using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Events;
using VARLab.DLX;
using System.Reflection;
using VARLab.Navigation.PointClick;

namespace Tests.PlayMode
{
    /// <summary>
    ///     Play mode tests for the <see cref="HandwashingMovementTile"/> class.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - Component initialization
    ///     - Task state management (started, completed, failed)
    ///     - Waypoint detection for handwashing task
    ///     - Handwashing process flow
    ///     - Inspection attempt handling
    ///     - Event invocation verification
    /// </remarks>
    public class HandwashingMovementTileTests
    {
        #region Fields

        private GameObject testObject;
        private HandwashingMovementTile handwashingTask;
        private GameObject handwashingStationObject;
        private GameObject waypointObject;
        private Waypoint waypointComponent;
        private GameObject waterEffectObject;
        private GameObject audioSourceObject;
        private AudioSource runningWaterAudio;

        // Field info for private fields access
        private FieldInfo isTaskStarted;
        private FieldInfo isTaskCompleted;
        private FieldInfo isIntroductionCompleted;
        private FieldInfo isOfficeTaskCompleted;
        private FieldInfo handwashingWaypoint;
        private FieldInfo handwashingWater;
        private FieldInfo runningWater;
        private FieldInfo handwashInstructionsDialog;
        private FieldInfo handwashFailureDialog;
        private FieldInfo dialogDelay;
        private FieldInfo handwashingDuration;

        // Test constants
        private const float TestDelay = 0.1f;
        private const float TestHandwashingDuration = 0.2f;

        #endregion

        #region Test Setup

        /// <summary>
        ///     Sets up the test environment before each test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Setup test objects
            testObject = new GameObject("TestHandwashingTask");
            handwashingTask = testObject.AddComponent<HandwashingMovementTile>();

            // Setup test waypoint
            waypointObject = new GameObject("TestWaypoint");
            waypointComponent = waypointObject.AddComponent<Waypoint>();

            // Setup handwashing station
            handwashingStationObject = new GameObject("TestHandwashingStation");

            // Setup water effect
            waterEffectObject = new GameObject("TestWaterEffect");
            waterEffectObject.SetActive(false);

            // Setup audio source
            audioSourceObject = new GameObject("TestAudioSource");
            runningWaterAudio = audioSourceObject.AddComponent<AudioSource>();

            // Get private fields using reflection
            isTaskStarted = typeof(HandwashingMovementTile).GetField("isTaskStarted", BindingFlags.NonPublic | BindingFlags.Instance);
            isTaskCompleted = typeof(HandwashingMovementTile).GetField("isTaskCompleted", BindingFlags.NonPublic | BindingFlags.Instance);
            isIntroductionCompleted = typeof(HandwashingMovementTile).GetField("isIntroductionCompleted", BindingFlags.NonPublic | BindingFlags.Instance);
            isOfficeTaskCompleted = typeof(HandwashingMovementTile).GetField("isOfficeTaskCompleted", BindingFlags.NonPublic | BindingFlags.Instance);
            handwashingWaypoint = typeof(HandwashingMovementTile).GetField("handwashingWaypoint", BindingFlags.NonPublic | BindingFlags.Instance);
            handwashingWater = typeof(HandwashingMovementTile).GetField("handwashingWater", BindingFlags.NonPublic | BindingFlags.Instance);
            runningWater = typeof(HandwashingMovementTile).GetField("runningWater", BindingFlags.NonPublic | BindingFlags.Instance);
            handwashInstructionsDialog = typeof(HandwashingMovementTile).GetField("handwashInstructionsDialog", BindingFlags.NonPublic | BindingFlags.Instance);
            handwashFailureDialog = typeof(HandwashingMovementTile).GetField("handwashFailureDialog", BindingFlags.NonPublic | BindingFlags.Instance);
            dialogDelay = typeof(HandwashingMovementTile).GetField("dialogDelay", BindingFlags.NonPublic | BindingFlags.Instance);
            handwashingDuration = typeof(HandwashingMovementTile).GetField("handwashingDuration", BindingFlags.NonPublic | BindingFlags.Instance);

            // Set handwashing waypoint
            handwashingWaypoint.SetValue(handwashingTask, waypointComponent);

            // Set water effect
            handwashingWater.SetValue(handwashingTask, waterEffectObject);

            // Set audio source
            runningWater.SetValue(handwashingTask, runningWaterAudio);

            // Set dialogs
            handwashInstructionsDialog.SetValue(handwashingTask, ScriptableObject.CreateInstance<InformDialog>());
            handwashFailureDialog.SetValue(handwashingTask, ScriptableObject.CreateInstance<InformDialog>());

            // Set small delays for testing
            dialogDelay.SetValue(handwashingTask, TestDelay);
            handwashingDuration.SetValue(handwashingTask, TestHandwashingDuration);
        }

        /// <summary>
        ///     Cleans up the test environment after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.Destroy(testObject);
            Object.Destroy(waypointObject);
            Object.Destroy(handwashingStationObject);
            Object.Destroy(waterEffectObject);
            Object.Destroy(audioSourceObject);
        }

        #endregion

        #region Initialization Tests

        /// <summary>
        ///     Tests if Awake() initializes events to prevent null reference errors.
        /// </summary>
        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator HandwashingMovementTile_Awake_InitializesEvents()
        {
            // Arrange & Act
            yield return null; // Wait for Awake to be called

            // Assert
            Assert.NotNull(handwashingTask.OnTaskStarted, "OnTaskStarted event should be initialized");
            Assert.NotNull(handwashingTask.OnTaskCompleted, "OnTaskCompleted event should be initialized");
            Assert.NotNull(handwashingTask.OnTaskFailed, "OnTaskFailed event should be initialized");
            Assert.NotNull(handwashingTask.OnShowInformationDialog, "OnShowInformationDialog event should be initialized");
            Assert.NotNull(handwashingTask.OnHandwashingStart, "OnHandwashingStart event should be initialized");
            Assert.NotNull(handwashingTask.OnHandwashingEnd, "OnHandwashingEnd event should be initialized");
            Assert.NotNull(handwashingTask.RestartScene, "RestartScene event should be initialized");
        }

        #endregion

        #region CheckCurrentWaypoint Tests

        /// <summary>
        ///     Tests if CheckCurrentWaypoint() starts the task when player reaches the handwashing waypoint
        ///     and prerequisite tasks are completed.
        /// </summary>
        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator HandwashingMovementTile_CheckCurrentWaypoint_StartsTaskWhenPrerequisitesCompleted()
        {
            // Arrange
            isIntroductionCompleted.SetValue(handwashingTask, true);
            isOfficeTaskCompleted.SetValue(handwashingTask, true);
            bool eventInvoked = false;
            handwashingTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            // Act
            handwashingTask.CheckCurrentWaypoint(waypointComponent);
            yield return new WaitForSeconds(TestDelay * 2); // Wait for dialog delay

            // Assert
            Assert.IsTrue((bool)isTaskStarted.GetValue(handwashingTask), "isTaskStarted should be set to true");
            Assert.IsTrue(eventInvoked, "OnTaskStarted event should be invoked");
        }

        /// <summary>
        ///     Tests if CheckCurrentWaypoint() shows failure dialog when introduction task is not completed.
        /// </summary>
        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator HandwashingMovementTile_CheckCurrentWaypoint_ShowsFailureDialogWhenIntroductionNotCompleted()
        {
            // Arrange
            isIntroductionCompleted.SetValue(handwashingTask, false);
            isOfficeTaskCompleted.SetValue(handwashingTask, true);
            bool dialogShown = false;
            handwashingTask.OnShowInformationDialog.AddListener((dialog) => dialogShown = true);

            // Act
            handwashingTask.CheckCurrentWaypoint(waypointComponent);
            yield return null;

            // Assert
            Assert.IsFalse((bool)isTaskStarted.GetValue(handwashingTask), "isTaskStarted should remain false");
            Assert.IsTrue(dialogShown, "Failure dialog should be shown");
        }

        /// <summary>
        ///     Tests if CheckCurrentWaypoint() shows failure dialog when office task is not completed.
        /// </summary>
        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator HandwashingMovementTile_CheckCurrentWaypoint_ShowsFailureDialogWhenOfficeTaskNotCompleted()
        {
            // Arrange
            isIntroductionCompleted.SetValue(handwashingTask, true);
            isOfficeTaskCompleted.SetValue(handwashingTask, false);
            bool dialogShown = false;
            handwashingTask.OnShowInformationDialog.AddListener((dialog) => dialogShown = true);

            // Act
            handwashingTask.CheckCurrentWaypoint(waypointComponent);
            yield return null;

            // Assert
            Assert.IsFalse((bool)isTaskStarted.GetValue(handwashingTask), "isTaskStarted should remain false");
            Assert.IsTrue(dialogShown, "Failure dialog should be shown");
        }

        /// <summary>
        ///     Tests if CheckCurrentWaypoint() does not start task for non-handwashing waypoints.
        /// </summary>
        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator HandwashingMovementTile_CheckCurrentWaypoint_DoesNotStartTaskForNonHandwashingWaypoint()
        {
            // Arrange
            isIntroductionCompleted.SetValue(handwashingTask, true);
            isOfficeTaskCompleted.SetValue(handwashingTask, true);
            bool eventInvoked = false;
            handwashingTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            // Create a different waypoint
            var otherWaypointObject = new GameObject("OtherWaypoint");
            var otherWaypoint = otherWaypointObject.AddComponent<Waypoint>();

            // Act
            handwashingTask.CheckCurrentWaypoint(otherWaypoint);
            yield return new WaitForSeconds(TestDelay * 2);

            // Assert
            Assert.IsFalse((bool)isTaskStarted.GetValue(handwashingTask), "isTaskStarted should remain false");
            Assert.IsFalse(eventInvoked, "OnTaskStarted should not be invoked");

            // Cleanup
            Object.Destroy(otherWaypointObject);
        }

        #endregion

        #region HandleTask Tests

        /// <summary>
        ///     Tests if HandleTask() sets task as started and invokes OnTaskStarted event.
        /// </summary>
        [UnityTest, Order(6)]
        [Category("BuildServer")]
        public IEnumerator HandwashingMovementTile_HandleTask_SetsTaskStartedAndInvokesEvent()
        {
            // Arrange
            bool eventInvoked = false;
            handwashingTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            // Act
            handwashingTask.HandleTask();
            yield return null;

            // Assert
            Assert.IsTrue((bool)isTaskStarted.GetValue(handwashingTask), "isTaskStarted should be set to true");
            Assert.IsTrue(eventInvoked, "OnTaskStarted event should be invoked");
        }

        /// <summary>
        ///     Tests if HandleTask() returns early and doesn't invoke event when task is already completed.
        /// </summary>
        [Test, Order(7)]
        [Category("BuildServer")]
        public void HandwashingMovementTile_HandleTask_ReturnsEarlyIfTaskCompleted()
        {
            // Arrange - Set task to completed state
            bool eventInvoked = false;
            handwashingTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            isTaskStarted.SetValue(handwashingTask, true);
            isTaskCompleted.SetValue(handwashingTask, true);

            // Act
            handwashingTask.HandleTask();

            // Assert
            Assert.IsFalse(eventInvoked, "OnTaskStarted should not be invoked if task is already completed");
        }

        #endregion

        #region Handwashing Process Tests

        /// <summary>
        ///     Tests if StartHandwashing() starts the handwashing coroutine and triggers events.
        /// </summary>
        [UnityTest, Order(8)]
        [Category("BuildServer")]
        public IEnumerator HandwashingMovementTile_StartHandwashing_StartsHandwashingProcess()
        {
            // Arrange
            bool startEventInvoked = false;
            bool endEventInvoked = false;
            handwashingTask.OnHandwashingStart.AddListener(() => startEventInvoked = true);
            handwashingTask.OnHandwashingEnd.AddListener(() => endEventInvoked = true);

            // Act
            handwashingTask.StartHandwashing();

            // Check start event
            yield return null;
            Assert.IsTrue(startEventInvoked, "OnHandwashingStart should be invoked");
            Assert.IsTrue(waterEffectObject.activeSelf, "Water effect should be activated");

            // Wait for handwashing duration
            yield return new WaitForSeconds(TestHandwashingDuration + 1.1f); // Duration + 1 second wait before completing

            // Assert
            Assert.IsTrue(endEventInvoked, "OnHandwashingEnd should be invoked");
            Assert.IsFalse(waterEffectObject.activeSelf, "Water effect should be deactivated");
        }

        #endregion

        #region CompleteTask Tests

        /// <summary>
        ///     Tests if CompleteTask() sets task as completed and invokes OnTaskCompleted event.
        /// </summary>
        [Test, Order(9)]
        [Category("BuildServer")]
        public void HandwashingMovementTile_CompleteTask_SetsTaskCompletedAndInvokesEvent()
        {
            // Arrange
            isTaskStarted.SetValue(handwashingTask, true);
            bool eventInvoked = false;
            handwashingTask.OnTaskCompleted.AddListener(() => eventInvoked = true);

            // Act
            handwashingTask.CompleteTask();

            // Assert
            Assert.IsTrue((bool)isTaskCompleted.GetValue(handwashingTask), "isTaskCompleted should be set to true");
            Assert.IsTrue(eventInvoked, "OnTaskCompleted event should be invoked");
        }

        /// <summary>
        ///     Tests if CompleteTask() returns early if task is not started.
        /// </summary>
        [Test, Order(10)]
        [Category("BuildServer")]
        public void HandwashingMovementTile_CompleteTask_ReturnsEarlyIfTaskNotStarted()
        {
            // Arrange
            isTaskStarted.SetValue(handwashingTask, false);
            bool eventInvoked = false;
            handwashingTask.OnTaskCompleted.AddListener(() => eventInvoked = true);

            // Act
            handwashingTask.CompleteTask();

            // Assert
            Assert.IsFalse((bool)isTaskCompleted.GetValue(handwashingTask), "isTaskCompleted should remain false if task is not started");
            Assert.IsFalse(eventInvoked, "OnTaskCompleted should not be invoked if task is not started");
        }

        /// <summary>
        ///     Tests if CompleteTask() returns early if task is already completed.
        /// </summary>
        [Test, Order(11)]
        [Category("BuildServer")]
        public void HandwashingMovementTile_CompleteTask_ReturnsEarlyIfTaskAlreadyCompleted()
        {
            // Arrange
            isTaskStarted.SetValue(handwashingTask, true);
            isTaskCompleted.SetValue(handwashingTask, true);
            bool eventInvoked = false;
            handwashingTask.OnTaskCompleted.AddListener(() => eventInvoked = true);

            // Act
            handwashingTask.CompleteTask();

            // Assert
            Assert.IsFalse(eventInvoked, "OnTaskCompleted should not be invoked if task is already completed");
        }

        #endregion

        #region Task Prerequisite Tests

        /// <summary>
        ///     Tests if OnIntroductionCompleted() sets the introduction task as completed.
        /// </summary>
        [Test, Order(12)]
        [Category("BuildServer")]
        public void HandwashingMovementTile_OnIntroductionCompleted_SetsIntroductionTaskCompleted()
        {
            // Act
            handwashingTask.OnIntroductionCompleted();

            // Assert
            Assert.IsTrue((bool)isIntroductionCompleted.GetValue(handwashingTask), "isIntroductionCompleted should be set to true");
        }

        /// <summary>
        ///     Tests if OnOfficeTaskCompleted() sets the office task as completed.
        /// </summary>
        [Test, Order(13)]
        [Category("BuildServer")]
        public void HandwashingMovementTile_OnOfficeTaskCompleted_SetsOfficeTaskCompleted()
        {
            // Act
            handwashingTask.OnOfficeTaskCompleted();

            // Assert
            Assert.IsTrue((bool)isOfficeTaskCompleted.GetValue(handwashingTask), "isOfficeTaskCompleted should be set to true");
        }

        #endregion

        #region CheckInspectionAttempt Tests

        /// <summary>
        ///     Tests if CheckInspectionAttempt() invokes OnTaskFailed when prerequisites are met but handwashing is not completed.
        /// </summary>
        [Test, Order(14)]
        [Category("BuildServer")]
        public void HandwashingMovementTile_CheckInspectionAttempt_InvokesOnTaskFailedWhenPrerequisitesMet()
        {
            // Arrange
            isIntroductionCompleted.SetValue(handwashingTask, true);
            isOfficeTaskCompleted.SetValue(handwashingTask, true);
            isTaskCompleted.SetValue(handwashingTask, false);
            bool failedEventInvoked = false;
            bool dialogShown = false;
            handwashingTask.OnTaskFailed.AddListener(() => failedEventInvoked = true);
            handwashingTask.OnShowInformationDialog.AddListener((dialog) => dialogShown = true);

            // Act
            handwashingTask.FailedInspectionAttempt();

            // Assert
            Assert.IsTrue(failedEventInvoked, "OnTaskFailed should be invoked");
            Assert.IsTrue(dialogShown, "Failure dialog should be shown");
        }
        #endregion
    }
}
