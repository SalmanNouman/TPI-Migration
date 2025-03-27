using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.DLX;
using VARLab.Navigation.PointClick;
using VARLab.Velcro;

namespace Tests.PlayMode
{
    /// <summary>
    ///     Play mode tests for the <see cref="OfficeTask"/> class.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - Component initialization
    ///     - Task state management (started, autoNavigating, officeConversationShown, completed)
    ///     - Waypoint detection for task
    ///     - Early return behaviors for different task states
    ///     - Event invocation verification
    ///     - Conversation delay functionality
    /// </remarks>
    public class OfficeTaskTests
    {
        #region Fields

        private GameObject testObject;
        private OfficeTask officeTask;
        private GameObject waypointObject;
        private Waypoint waypointComponent;

        // Field info for private fields access
        private FieldInfo isTaskStarted;
        private FieldInfo isAutoNavigating;
        private FieldInfo isOfficeConversationShown;
        private FieldInfo isTaskCompleted;
        private FieldInfo conversationDelay;

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
            testObject = new GameObject("TestOfficeTask");
            officeTask = testObject.AddComponent<OfficeTask>();

            // Setup test waypoint with OfficeTask component
            waypointObject = new GameObject("TestWaypoint");
            waypointComponent = waypointObject.AddComponent<Waypoint>();
            // Add OfficeTask component to waypoint to match detection logic
            waypointObject.AddComponent<OfficeTask>();

            // Get private fields using reflection
            isTaskStarted = typeof(OfficeTask).GetField("isTaskStarted", BindingFlags.NonPublic | BindingFlags.Instance);
            isAutoNavigating = typeof(OfficeTask).GetField("isAutoNavigating", BindingFlags.NonPublic | BindingFlags.Instance);
            isOfficeConversationShown = typeof(OfficeTask).GetField("isOfficeConversationShown", BindingFlags.NonPublic | BindingFlags.Instance);
            isTaskCompleted = typeof(OfficeTask).GetField("isTaskCompleted", BindingFlags.NonPublic | BindingFlags.Instance);
            conversationDelay = typeof(OfficeTask).GetField("conversationDelay", BindingFlags.NonPublic | BindingFlags.Instance);

            // Set a small delay for testing
            conversationDelay.SetValue(officeTask, TestDelay);
        }

        /// <summary>
        ///     Cleans up the test environment after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.Destroy(testObject);
            Object.Destroy(waypointObject);
        }

        #endregion

        #region Initialization Tests

        /// <summary>
        ///     Tests if Awake() initializes events to prevent null reference errors.
        /// </summary>
        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator OfficeTask_Awake_InitializesEvents()
        {
            // Arrange & Act
            yield return null; // Wait for Awake to be called

            // Assert
            Assert.NotNull(officeTask.OnTaskStarted, "OnTaskStarted event should be initialized");
            Assert.NotNull(officeTask.OnWaypointReached, "OnWaypointReached event should be initialized");
            Assert.NotNull(officeTask.OnTaskCompleted, "OnTaskCompleted event should be initialized");
        }

        #endregion

        #region HandleTask Tests

        /// <summary>
        ///     Tests if HandleTask() sets task as started, enables auto-navigation and invokes OnTaskStarted event.
        /// </summary>
        [Test, Order(2)]
        [Category("BuildServer")]
        public void OfficeTask_HandleTask_SetsTaskStartedAndInvokesEvent()
        {
            // Arrange
            bool eventInvoked = false;
            officeTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            // Act
            officeTask.HandleTask();

            // Assert
            Assert.IsTrue((bool)isTaskStarted.GetValue(officeTask), "isTaskStarted should be set to true");
            Assert.IsTrue((bool)isAutoNavigating.GetValue(officeTask), "isAutoNavigating should be set to true");
            Assert.IsTrue(eventInvoked, "OnTaskStarted event should be invoked");
        }

        /// <summary>
        ///     Tests if HandleTask() returns early and doesn't invoke event when task is already completed.
        /// </summary>
        [Test, Order(3)]
        [Category("BuildServer")]
        public void OfficeTask_HandleTask_ReturnsEarlyIfTaskCompleted()
        {
            // Arrange - Set task to completed state
            bool eventInvoked = false;
            officeTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            isTaskCompleted.SetValue(officeTask, true);

            // Act
            officeTask.HandleTask();

            // Assert
            Assert.IsFalse(eventInvoked, "OnTaskStarted should not be invoked if task is already completed");
        }

        #endregion

        #region HandleWaypointReached Tests

        /// <summary>
        ///     Tests if HandleWaypointReached() starts the conversation coroutine for office waypoint during auto-navigation.
        /// </summary>
        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator OfficeTask_HandleWaypointReached_StartsConversationCoroutineForOfficeWaypoint()
        {
            // Arrange
            isAutoNavigating.SetValue(officeTask, true);
            bool eventInvoked = false;
            officeTask.OnWaypointReached.AddListener(() => eventInvoked = true);

            // Act
            officeTask.HandleWaypointReached(waypointComponent);

            // Assert auto-navigation is disabled immediately
            Assert.IsFalse((bool)isAutoNavigating.GetValue(officeTask), "isAutoNavigating should be set to false");

            // Wait for delay to complete
            yield return new WaitForSeconds(TestDelay * 1.1f);

            // Assert conversation flag and event
            Assert.IsTrue((bool)isOfficeConversationShown.GetValue(officeTask), "isOfficeConversationShown should be set to true");
            Assert.IsTrue(eventInvoked, "OnWaypointReached event should be invoked after delay");
        }

        /// <summary>
        ///     Tests if HandleWaypointReached() skips processing for non-office waypoints.
        /// </summary>
        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator OfficeTask_HandleWaypointReached_IgnoresNonOfficeWaypoints()
        {
            // Arrange
            isAutoNavigating.SetValue(officeTask, true);
            bool eventInvoked = false;
            officeTask.OnWaypointReached.AddListener(() => eventInvoked = true);

            // Create a different waypoint without OfficeTask component
            var otherWaypointObject = new GameObject("OtherWaypoint");
            var otherWaypoint = otherWaypointObject.AddComponent<Waypoint>();

            // Act
            officeTask.HandleWaypointReached(otherWaypoint);

            // Wait for possible delay
            yield return new WaitForSeconds(TestDelay * 1.1f);

            // Assert
            Assert.IsTrue((bool)isAutoNavigating.GetValue(officeTask), "isAutoNavigating should still be true");
            Assert.IsFalse(eventInvoked, "OnWaypointReached should not be invoked for non-office waypoints");
            Assert.IsFalse((bool)isOfficeConversationShown.GetValue(officeTask), "isOfficeConversationShown should still be false");

            // Cleanup
            Object.Destroy(otherWaypointObject);
        }

        /// <summary>
        ///     Tests if HandleWaypointReached() returns early if task is already completed.
        /// </summary>
        [UnityTest, Order(6)]
        [Category("BuildServer")]
        public IEnumerator OfficeTask_HandleWaypointReached_ReturnsEarlyIfTaskCompleted()
        {
            // Arrange
            isAutoNavigating.SetValue(officeTask, true);
            isTaskCompleted.SetValue(officeTask, true);
            bool eventInvoked = false;
            officeTask.OnWaypointReached.AddListener(() => eventInvoked = true);

            // Act
            officeTask.HandleWaypointReached(waypointComponent);

            // Wait for possible delay
            yield return new WaitForSeconds(TestDelay * 1.1f);

            // Assert
            Assert.IsFalse(eventInvoked, "OnWaypointReached should not be invoked if task is already completed");
            Assert.IsFalse((bool)isOfficeConversationShown.GetValue(officeTask), "isOfficeConversationShown should remain false");
        }

        /// <summary>
        /// Call CompleteTask() when run LoadSaveTask
        /// </summary>
        [Test,Order(7)]
        [Category("BuildServer")]

        public void OfficeTask_LoadSaveTask_SetTaskFlagAndCallCompleteTask()
        {
            //Arrange
            isTaskStarted.SetValue(officeTask, true);
            isTaskCompleted.SetValue(officeTask, false);
            isOfficeConversationShown.SetValue(officeTask, true);

            bool eventInvoked = false;
            officeTask.OnTaskCompleted.AddListener(() => eventInvoked = true);
           
            //Act
            officeTask.LoadSaveTask();

            //Asset
            Assert.IsTrue((bool)isTaskCompleted.GetValue(officeTask), "isTaskCompleted should be set to true");
            Assert.IsTrue(eventInvoked, "CompleteTask should be called");

        }

        #endregion

        #region CompleteTask Tests

        /// <summary>
        ///     Tests if CompleteTask() sets task as completed and invokes OnTaskCompleted event.
        /// </summary>
        [Test, Order(8)]
        [Category("BuildServer")]
        public void OfficeTask_CompleteTask_SetsTaskCompletedAndInvokesEvent()
        {
            // Arrange
            isTaskStarted.SetValue(officeTask, true);
            isOfficeConversationShown.SetValue(officeTask, true);
            bool eventInvoked = false;
            officeTask.OnTaskCompleted.AddListener(() => eventInvoked = true);

            // Act
            officeTask.CompleteTask();

            // Assert
            Assert.IsTrue((bool)isTaskCompleted.GetValue(officeTask), "isTaskCompleted should be set to true");
            Assert.IsTrue(eventInvoked, "OnTaskCompleted event should be invoked");
        }

        /// <summary>
        ///     Tests if CompleteTask() returns early if the office conversation not shown yet.
        /// </summary>
        [Test, Order(9)]
        [Category("BuildServer")]
        public void OfficeTask_CompleteTask_ReturnsEarlyIfConversationNotShown()
        {
            // Arrange
            isTaskStarted.SetValue(officeTask, true);
            isOfficeConversationShown.SetValue(officeTask, false);
            bool eventInvoked = false;
            officeTask.OnTaskCompleted.AddListener(() => eventInvoked = true);

            // Act
            officeTask.CompleteTask();

            // Assert
            Assert.IsFalse((bool)isTaskCompleted.GetValue(officeTask), "isTaskCompleted should remain false if conversation not shown");
            Assert.IsFalse(eventInvoked, "OnTaskCompleted should not be invoked if conversation not shown");
        }

        #endregion
    }
}
