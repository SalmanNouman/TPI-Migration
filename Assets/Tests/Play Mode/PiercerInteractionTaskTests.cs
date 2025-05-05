using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.DLX;
using VARLab.Navigation.PointClick;

namespace Tests.PlayMode
{
    /// <summary>
    ///     Play mode tests for the <see cref="PiercerInteractionTask"/> class.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - Component initialization
    ///     - Task state management (started, completed)
    ///     - Waypoint detection for task
    ///     - GameObject activation/deactivation (procedure tray and piercer cutout)
    ///     - Early return behaviors for different task states
    ///     - Event invocation verification
    /// </remarks>
    public class PiercerInteractionTaskTests
    {
        #region Fields

        private GameObject testObject;
        private PiercerInteractionTask piercerTask;
        private GameObject waypointObject;
        private Waypoint waypointComponent;
        private GameObject preparedProcedureTrayObject;
        private GameObject piercerCutoutObject;

        // Field info for private fields access
        private FieldInfo isTaskStarted;
        private FieldInfo isTaskCompleted;
        private FieldInfo conversationDelay;
        private FieldInfo piercerWaypoint;
        private FieldInfo preparedProcedureTray;
        private FieldInfo piercerCutout;

        // Test constants
        private const float TestDelay = 0.1f;

        #endregion

        #region Test Setup

        /// <summary>
        ///     Sets up the test environment before each test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Setup test objects
            testObject = new GameObject("TestPiercerInteractionTask");
            piercerTask = testObject.AddComponent<PiercerInteractionTask>();

            // Setup test waypoint
            waypointObject = new GameObject("TestWaypoint");
            waypointComponent = waypointObject.AddComponent<Waypoint>();

            // Setup test procedure tray and piercer cutout
            preparedProcedureTrayObject = new GameObject("TestProcedureTray");
            piercerCutoutObject = new GameObject("TestPiercerCutout");

            // Get private fields using reflection
            isTaskStarted = typeof(PiercerInteractionTask).GetField("isTaskStarted", BindingFlags.NonPublic | BindingFlags.Instance);
            isTaskCompleted = typeof(PiercerInteractionTask).GetField("isTaskCompleted", BindingFlags.NonPublic | BindingFlags.Instance);
            conversationDelay = typeof(PiercerInteractionTask).GetField("conversationDelay", BindingFlags.NonPublic | BindingFlags.Instance);
            piercerWaypoint = typeof(PiercerInteractionTask).GetField("piercerWaypoint", BindingFlags.NonPublic | BindingFlags.Instance);
            preparedProcedureTray = typeof(PiercerInteractionTask).GetField("preparedProcedureTray", BindingFlags.NonPublic | BindingFlags.Instance);
            piercerCutout = typeof(PiercerInteractionTask).GetField("piercerCutout", BindingFlags.NonPublic | BindingFlags.Instance);

            // Set piercer waypoint
            piercerWaypoint.SetValue(piercerTask, waypointComponent);

            // Set procedure tray and piercer cutout
            preparedProcedureTray.SetValue(piercerTask, preparedProcedureTrayObject);
            piercerCutout.SetValue(piercerTask, piercerCutoutObject);

            // Set a small delay for testing
            conversationDelay.SetValue(piercerTask, TestDelay);
        }

        /// <summary>
        ///     Cleans up the test environment after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.Destroy(testObject);
            Object.Destroy(waypointObject);
            Object.Destroy(preparedProcedureTrayObject);
            Object.Destroy(piercerCutoutObject);
        }

        #endregion

        #region Initialization Tests

        /// <summary>
        ///     Tests if Awake() initializes events to prevent null reference errors.
        /// </summary>
        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator Awake_InitializesEvents()
        {
            // Arrange & Act
            yield return null; // Wait for Awake to be called

            // Assert
            Assert.NotNull(piercerTask.OnTaskStarted, "OnTaskStarted event should be initialized");
            Assert.NotNull(piercerTask.OnTaskCompleted, "OnTaskCompleted event should be initialized");
            Assert.NotNull(piercerTask.OnTaskFailed, "OnTaskFailed event should be initialized");
        }

        /// <summary>
        ///     Tests if Start() properly initializes GameObjects' active state.
        /// </summary>
        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator Start_InitializesGameObjectsActiveState()
        {
            // Arrange & Act
            yield return null; // Wait for Start to be called

            // Assert
            Assert.IsFalse(preparedProcedureTrayObject.activeSelf, "Procedure tray should be initially inactive");
            Assert.IsTrue(piercerCutoutObject.activeSelf, "Piercer cutout should be initially active");
        }

        #endregion

        #region CheckCurrentWaypoint Tests

        /// <summary>
        ///     Tests if CheckCurrentWaypoint() starts the task when player reaches the piercer waypoint.
        /// </summary>
        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator CheckCurrentWaypoint_StartsTaskForPiercerWaypoint()
        {
            // Arrange
            bool taskStartedEventInvoked = false;
            bool taskCompletedEventInvoked = false;
            piercerTask.OnTaskStarted.AddListener(() => taskStartedEventInvoked = true);
            piercerTask.OnTaskCompleted.AddListener(() => taskCompletedEventInvoked = true);

            // Act
            piercerTask.CheckCurrentWaypoint(waypointComponent);
            yield return new WaitForSeconds(TestDelay);
            yield return null; // Wait for next frame for coroutine to complete

            // Assert
            Assert.IsTrue((bool)isTaskStarted.GetValue(piercerTask), "isTaskStarted should be set to true");
            Assert.IsTrue((bool)isTaskCompleted.GetValue(piercerTask), "isTaskCompleted should be set to true");
            Assert.IsTrue(taskStartedEventInvoked, "OnTaskStarted event should be invoked");
            Assert.IsTrue(taskCompletedEventInvoked, "OnTaskCompleted event should be invoked");
            Assert.IsTrue(preparedProcedureTrayObject.activeSelf, "Procedure tray should be active after task completes");
            Assert.IsFalse(piercerCutoutObject.activeSelf, "Piercer cutout should be inactive after task completes");
        }

        /// <summary>
        ///     Tests if CheckCurrentWaypoint does not start the task for non-piercer waypoints.
        /// </summary>
        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator CheckCurrentWaypoint_DoesNotStartTaskForNonPiercerWaypoint()
        {
            // Arrange
            bool eventInvoked = false;
            piercerTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            // Create a different waypoint
            var otherWaypointObject = new GameObject("OtherWaypoint");
            var otherWaypoint = otherWaypointObject.AddComponent<Waypoint>();

            // Act
            piercerTask.CheckCurrentWaypoint(otherWaypoint);
            yield return new WaitForSeconds(TestDelay);
            yield return null; // Wait for next frame for coroutine to complete

            // Assert
            Assert.IsFalse((bool)isTaskStarted.GetValue(piercerTask), "isTaskStarted should remain false");
            Assert.IsFalse(eventInvoked, "OnTaskStarted should not be invoked");
            Assert.IsFalse(preparedProcedureTrayObject.activeSelf, "Procedure tray should remain inactive");
            Assert.IsTrue(piercerCutoutObject.activeSelf, "Piercer cutout should remain active");

            // Cleanup
            Object.Destroy(otherWaypointObject);
        }

        #endregion

        #region DelayedTaskStart Tests

        /// <summary>
        ///     Tests if DelayedTaskStart coroutine calls HandleTask after the specified delay.
        /// </summary>
        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator DelayedTaskStart_CallsHandleTaskAfterDelay()
        {
            // Arrange
            bool eventInvoked = false;
            piercerTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            // Act - Call CheckCurrentWaypoint with the piercer waypoint to start DelayedTaskStart
            piercerTask.CheckCurrentWaypoint(waypointComponent);

            // Check that task is not started yet (before delay completes)
            yield return new WaitForSeconds(TestDelay * 0.5f);
            Assert.IsFalse(eventInvoked, "Task should not be started before delay completes");

            // Wait for the delay to complete
            yield return new WaitForSeconds(TestDelay * 0.6f);

            // Assert
            Assert.IsTrue(eventInvoked, "OnTaskStarted should be invoked after delay");
        }

        #endregion

        #region HandleTask Tests

        /// <summary>
        ///     Tests if HandleTask() sets task as started and invokes OnTaskStarted event.
        /// </summary>
        [Test, Order(6)]
        [Category("BuildServer")]
        public void HandleTask_SetsTaskStartedAndInvokesEvent()
        {
            // Arrange
            bool taskStartedEventInvoked = false;
            bool taskCompletedEventInvoked = false;
            piercerTask.OnTaskStarted.AddListener(() => taskStartedEventInvoked = true);
            piercerTask.OnTaskCompleted.AddListener(() => taskCompletedEventInvoked = true);

            // Act
            piercerTask.HandleTask();

            // Assert
            Assert.IsTrue((bool)isTaskStarted.GetValue(piercerTask), "isTaskStarted should be set to true");
            Assert.IsTrue((bool)isTaskCompleted.GetValue(piercerTask), "isTaskCompleted should be set to true");
            Assert.IsTrue(taskStartedEventInvoked, "OnTaskStarted event should be invoked");
            Assert.IsTrue(taskCompletedEventInvoked, "OnTaskCompleted event should be invoked");
        }

        /// <summary>
        ///     Tests if HandleTask() returns early and doesn't invoke event when task is already completed.
        /// </summary>
        [Test, Order(7)]
        [Category("BuildServer")]
        public void HandleTask_ReturnsEarlyIfTaskCompleted()
        {
            // Arrange - Set task to completed state
            bool eventInvoked = false;
            piercerTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            isTaskStarted.SetValue(piercerTask, true);
            isTaskCompleted.SetValue(piercerTask, true);

            // Act
            piercerTask.HandleTask();

            // Assert
            Assert.IsFalse(eventInvoked, "OnTaskStarted should not be invoked if task is already completed");
        }

        #endregion

        #region CompleteTask Tests

        /// <summary>
        ///     Tests if CompleteTask() returns early if task is not started.
        /// </summary>
        [Test, Order(8)]
        [Category("BuildServer")]
        public void CompleteTask_ReturnsEarlyIfTaskNotStarted()
        {
            // Arrange
            isTaskStarted.SetValue(piercerTask, false);
            bool eventInvoked = false;
            piercerTask.OnTaskCompleted.AddListener(() => eventInvoked = true);

            // Disable tray and enable cutout for testing
            preparedProcedureTrayObject.SetActive(false);
            piercerCutoutObject.SetActive(true);

            // Act
            piercerTask.CompleteTask();

            // Assert
            Assert.IsFalse((bool)isTaskCompleted.GetValue(piercerTask), "isTaskCompleted should remain false if task is not started");
            Assert.IsFalse(eventInvoked, "OnTaskCompleted should not be invoked if task is not started");
            Assert.IsFalse(preparedProcedureTrayObject.activeSelf, "Procedure tray should remain inactive if task is not started");
            Assert.IsTrue(piercerCutoutObject.activeSelf, "Piercer cutout should remain active if task is not started");
        }

        /// <summary>
        ///     Tests if CompleteTask() returns early if task is already completed.
        /// </summary>
        [Test, Order(9)]
        [Category("BuildServer")]
        public void CompleteTask_ReturnsEarlyIfTaskAlreadyCompleted()
        {
            // Arrange
            isTaskStarted.SetValue(piercerTask, true);
            isTaskCompleted.SetValue(piercerTask, true);
            bool eventInvoked = false;
            piercerTask.OnTaskCompleted.AddListener(() => eventInvoked = true);

            // Disable tray and enable cutout to check they don't change
            preparedProcedureTrayObject.SetActive(false);
            piercerCutoutObject.SetActive(true);

            // Act
            piercerTask.CompleteTask();

            // Assert
            Assert.IsFalse(eventInvoked, "OnTaskCompleted should not be invoked if task is already completed");
            Assert.IsFalse(preparedProcedureTrayObject.activeSelf, "Procedure tray state shouldn't change if task is already completed");
            Assert.IsTrue(piercerCutoutObject.activeSelf, "Piercer cutout state shouldn't change if task is already completed");
        }

        /// <summary>
        ///     Tests if CompleteTask() sets task as completed, shows the procedure tray, hides the cutout, and invokes OnTaskCompleted event.
        /// </summary>
        [Test, Order(10)]
        [Category("BuildServer")]
        public void CompleteTask_SetsTaskCompletedAndModifiesGameObjects()
        {
            // Arrange
            isTaskStarted.SetValue(piercerTask, true);
            bool eventInvoked = false;
            piercerTask.OnTaskCompleted.AddListener(() => eventInvoked = true);

            // Act
            piercerTask.CompleteTask();

            // Assert
            Assert.IsTrue((bool)isTaskCompleted.GetValue(piercerTask), "isTaskCompleted should be set to true");
            Assert.IsTrue(eventInvoked, "OnTaskCompleted event should be invoked");
            Assert.IsTrue(preparedProcedureTrayObject.activeSelf, "Procedure tray should be active after task completes");
            Assert.IsFalse(piercerCutoutObject.activeSelf, "Piercer cutout should be inactive after task completes");
        }

        #endregion
    }
}
