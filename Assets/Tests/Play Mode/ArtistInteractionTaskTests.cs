using EPOOutline;
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
    ///     Play mode tests for the <see cref="ArtistInteractionTask"/> class.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - Component initialization
    ///     - Task state management (started, completed)
    ///     - Waypoint detection for task
    ///     - GameObject activation/deactivation (procedure tray and artist cutout)
    ///     - Early return behaviors for different task states
    ///     - Event invocation verification
    /// </remarks>
    public class ArtistInteractionTaskTests
    {
        #region Fields

        private GameObject testObject;
        private ArtistInteractionTask artistTask;
        private GameObject waypointObject;
        private Waypoint waypointComponent;
        private GameObject preparedProcedureTrayObject;
        private GameObject artistCutoutObject;

        // Field info for private fields access
        private FieldInfo isTaskStarted;
        private FieldInfo isTaskCompleted;
        private FieldInfo conversationDelay;
        private FieldInfo artistWaypoint;
        private FieldInfo preparedProcedureTray;
        private FieldInfo artistCutout;

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
            testObject = new GameObject("TestArtistInteractionTask");
            artistTask = testObject.AddComponent<ArtistInteractionTask>();

            // Setup test waypoint
            waypointObject = new GameObject("TestWaypoint");
            waypointComponent = waypointObject.AddComponent<Waypoint>();

            // Setup test procedure tray and artist cutout
            preparedProcedureTrayObject = new GameObject("TestProcedureTray");
            artistCutoutObject = new GameObject("TestArtistCutout");

            // Get private fields using reflection
            isTaskStarted = typeof(ArtistInteractionTask).GetField("isTaskStarted", BindingFlags.NonPublic | BindingFlags.Instance);
            isTaskCompleted = typeof(ArtistInteractionTask).GetField("isTaskCompleted", BindingFlags.NonPublic | BindingFlags.Instance);
            conversationDelay = typeof(ArtistInteractionTask).GetField("conversationDelay", BindingFlags.NonPublic | BindingFlags.Instance);
            artistWaypoint = typeof(ArtistInteractionTask).GetField("artistWaypoint", BindingFlags.NonPublic | BindingFlags.Instance);
            preparedProcedureTray = typeof(ArtistInteractionTask).GetField("preparedProcedureTray", BindingFlags.NonPublic | BindingFlags.Instance);
            artistCutout = typeof(ArtistInteractionTask).GetField("artistCutout", BindingFlags.NonPublic | BindingFlags.Instance);

            // Set artist waypoint
            artistWaypoint.SetValue(artistTask, waypointComponent);

            // Set procedure tray and artist cutout
            preparedProcedureTray.SetValue(artistTask, preparedProcedureTrayObject);
            artistCutout.SetValue(artistTask, artistCutoutObject);
            artistCutoutObject.AddComponent<Outlinable>(); // Add Outlinable component for testing

            // Set a small delay for testing
            conversationDelay.SetValue(artistTask, TestDelay);
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
            Object.Destroy(artistCutoutObject);
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
            Assert.NotNull(artistTask.OnTaskStarted, "OnTaskStarted event should be initialized");
            Assert.NotNull(artistTask.OnTaskCompleted, "OnTaskCompleted event should be initialized");
            Assert.NotNull(artistTask.OnTaskFailed, "OnTaskFailed event should be initialized");
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
            Assert.IsTrue(artistCutoutObject.activeSelf, "Artist cutout should be initially active");
        }

        #endregion

        #region CheckCurrentWaypoint Tests

        /// <summary>
        ///     Tests if CheckCurrentWaypoint() starts the task when player reaches the artist waypoint.
        /// </summary>
        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator CheckCurrentWaypoint_StartsTaskForArtistWaypoint()
        {
            // Arrange
            bool taskStartedEventInvoked = false;
            bool taskCompletedEventInvoked = false;
            artistTask.OnTaskStarted.AddListener(() => taskStartedEventInvoked = true);
            artistTask.OnTaskCompleted.AddListener(() => taskCompletedEventInvoked = true);

            // Act
            artistTask.CheckCurrentWaypoint(waypointComponent);
            yield return new WaitForSeconds(TestDelay);
            yield return null; // Wait for next frame for coroutine to complete

            // Assert
            Assert.IsTrue((bool)isTaskStarted.GetValue(artistTask), "isTaskStarted should be set to true");
            Assert.IsTrue((bool)isTaskCompleted.GetValue(artistTask), "isTaskCompleted should be set to true");
            Assert.IsTrue(taskStartedEventInvoked, "OnTaskStarted event should be invoked");
            Assert.IsTrue(taskCompletedEventInvoked, "OnTaskCompleted event should be invoked");
            Assert.IsTrue(preparedProcedureTrayObject.activeSelf, "Procedure tray should be active after task completes");
        }

        /// <summary>
        ///     Tests if CheckCurrentWaypoint does not start the task for non-artist waypoints.
        /// </summary>
        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator CheckCurrentWaypoint_DoesNotStartTaskForNonArtistWaypoint()
        {
            // Arrange
            bool eventInvoked = false;
            artistTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            // Create a different waypoint
            var otherWaypointObject = new GameObject("OtherWaypoint");
            var otherWaypoint = otherWaypointObject.AddComponent<Waypoint>();

            // Act
            artistTask.CheckCurrentWaypoint(otherWaypoint);
            yield return new WaitForSeconds(TestDelay);
            yield return null; // Wait for next frame for coroutine to complete

            // Assert
            Assert.IsFalse((bool)isTaskStarted.GetValue(artistTask), "isTaskStarted should remain false");
            Assert.IsFalse(eventInvoked, "OnTaskStarted should not be invoked");
            Assert.IsFalse(preparedProcedureTrayObject.activeSelf, "Procedure tray should remain inactive");
            Assert.IsTrue(artistCutoutObject.activeSelf, "Artist cutout should remain active");

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
            artistTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            // Act - Call CheckCurrentWaypoint with the artist waypoint to start DelayedTaskStart
            artistTask.CheckCurrentWaypoint(waypointComponent);

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
            bool interactedLogEventInvoked = false;
            artistTask.OnTaskStarted.AddListener(() => taskStartedEventInvoked = true);
            artistTask.OnInteractedLog.AddListener((artistName) => interactedLogEventInvoked = true);
            artistTask.OnTaskCompleted.AddListener(() => taskCompletedEventInvoked = true);
            waypointObject.gameObject.name = "TpiWaypoint_PiercerInteractionTask"; // Ensure waypoint has one of two expected names

            // Act
            artistTask.HandleTask();

            // Assert
            Assert.IsTrue((bool)isTaskStarted.GetValue(artistTask), "isTaskStarted should be set to true");
            Assert.IsTrue((bool)isTaskCompleted.GetValue(artistTask), "isTaskCompleted should be set to true");
            Assert.IsTrue(interactedLogEventInvoked, "OnInteractedLog event should be invoked for artist interaction");
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
            artistTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            isTaskStarted.SetValue(artistTask, true);
            isTaskCompleted.SetValue(artistTask, true);

            // Act
            artistTask.HandleTask();

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
            isTaskStarted.SetValue(artistTask, false);
            bool eventInvoked = false;
            artistTask.OnTaskCompleted.AddListener(() => eventInvoked = true);

            // Disable tray and enable cutout for testing
            preparedProcedureTrayObject.SetActive(false);
            artistCutoutObject.SetActive(true);
            var outlinable = artistCutoutObject.GetComponent<Outlinable>();

            // Act
            artistTask.CompleteTask();

            // Assert
            Assert.IsFalse((bool)isTaskCompleted.GetValue(artistTask), "isTaskCompleted should remain false if task is not started");
            Assert.IsFalse(eventInvoked, "OnTaskCompleted should not be invoked if task is not started");
            Assert.IsFalse(preparedProcedureTrayObject.activeSelf, "Procedure tray should remain inactive if task is not started");
            Assert.IsTrue(artistCutoutObject.activeSelf, "Artist cutout should remain active if task is not started");
            Assert.IsTrue(outlinable.enabled, "Outlinable component on artist cutout should remain enabled if task is not started");
        }

        /// <summary>
        ///     Tests if CompleteTask() returns early if task is already completed.
        /// </summary>
        [Test, Order(9)]
        [Category("BuildServer")]
        public void CompleteTask_ReturnsEarlyIfTaskAlreadyCompleted()
        {
            // Arrange
            isTaskStarted.SetValue(artistTask, true);
            isTaskCompleted.SetValue(artistTask, true);
            bool eventInvoked = false;
            artistTask.OnTaskCompleted.AddListener(() => eventInvoked = true);

            // Disable tray and enable cutout to check they don't change
            preparedProcedureTrayObject.SetActive(false);
            artistCutoutObject.SetActive(true);

            // Act
            artistTask.CompleteTask();

            // Assert
            Assert.IsFalse(eventInvoked, "OnTaskCompleted should not be invoked if task is already completed");
            Assert.IsFalse(preparedProcedureTrayObject.activeSelf, "Procedure tray state shouldn't change if task is already completed");
            Assert.IsTrue(artistCutoutObject.activeSelf, "Artist cutout state shouldn't change if task is already completed");
        }

        /// <summary>
        ///     Tests if CompleteTask() sets task as completed, shows the procedure tray, hides the cutout, and invokes OnTaskCompleted event.
        /// </summary>
        [Test, Order(10)]
        [Category("BuildServer")]
        public void CompleteTask_SetsTaskCompletedAndModifiesGameObjects()
        {
            // Arrange
            isTaskStarted.SetValue(artistTask, true);
            bool eventInvoked = false;
            artistTask.OnTaskCompleted.AddListener(() => eventInvoked = true);
            var outlinable = artistCutoutObject.GetComponent<Outlinable>();

            // Act
            artistTask.CompleteTask();

            // Assert
            Assert.IsTrue((bool)isTaskCompleted.GetValue(artistTask), "isTaskCompleted should be set to true");
            Assert.IsTrue(eventInvoked, "OnTaskCompleted event should be invoked");
            Assert.IsTrue(preparedProcedureTrayObject.activeSelf, "Procedure tray should be active after task completes");
            Assert.IsFalse(outlinable.enabled, "Outlinable component on artist cutout should be disabled after task completes");
        }

        #endregion
    }
}
