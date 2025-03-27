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
    ///     Play mode tests for the <see cref="IntroductionTask"/> class.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - Component initialization
    ///     - Task state management (started, completed, failed)
    ///     - Waypoint detection for task
    ///     - Early return behaviors for different task states
    ///     - Event invocation verification
    /// </remarks>
    public class IntroductionTaskTests
    {
        #region Fields

        private GameObject testObject;
        private IntroductionTask introductionTask;
        private GameObject receptionPoiObject;
        private Poi receptionPoiComponent;
        private GameObject waypointObject;
        private Waypoint waypointComponent;

        // Field info for private fields access
        private FieldInfo receptionPoi;
        private FieldInfo isTaskStarted;
        private FieldInfo isTaskCompleted;
        private FieldInfo conversationDelay;
        private FieldInfo introductionWaypoint;
        private FieldInfo informDialogSO;

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
            testObject = new GameObject("TestIntroductionTask");
            introductionTask = testObject.AddComponent<IntroductionTask>();

            // Setup test POIs
            receptionPoiObject = new GameObject("TestReceptionPOI");
            receptionPoiComponent = receptionPoiObject.AddComponent<Poi>();
            receptionPoiComponent.PoiName = "Reception";
            
            // Setup test waypoint
            waypointObject = new GameObject("TestWaypoint");
            waypointComponent = waypointObject.AddComponent<Waypoint>();

            // Get private fields using reflection
            // * Reflection is used to access a private field
            receptionPoi = typeof(IntroductionTask).GetField("receptionPoi", BindingFlags.NonPublic | BindingFlags.Instance);
            isTaskStarted = typeof(IntroductionTask).GetField("isTaskStarted", BindingFlags.NonPublic | BindingFlags.Instance);
            isTaskCompleted = typeof(IntroductionTask).GetField("isTaskCompleted", BindingFlags.NonPublic | BindingFlags.Instance);
            conversationDelay = typeof(IntroductionTask).GetField("conversationDelay", BindingFlags.NonPublic | BindingFlags.Instance);
            introductionWaypoint = typeof(IntroductionTask).GetField("introductionWaypoint", BindingFlags.NonPublic | BindingFlags.Instance);
            informDialogSO = typeof(IntroductionTask).GetField("informDialogSO", BindingFlags.NonPublic | BindingFlags.Instance);

            // Set Reception POI as the allowed area
            receptionPoi.SetValue(introductionTask, receptionPoiComponent);

            // Set introduction waypoint
            introductionWaypoint.SetValue(introductionTask, waypointComponent);

            // Set a small delay for testing
            conversationDelay.SetValue(introductionTask, TestDelay);
        }

        /// <summary>
        ///     Cleans up the test environment after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.Destroy(testObject);
            Object.Destroy(receptionPoiObject);
            Object.Destroy(waypointObject);
        }

        #endregion

        #region Initialization Tests

        /// <summary>
        ///     Tests if Awake() initializes events to prevent null reference errors.
        /// </summary>
        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator IntroductionTask_Awake_InitializesEvents()
        {
            // Arrange & Act
            yield return null; // Wait for Awake to be called

            // Assert
            Assert.NotNull(introductionTask.OnTaskStarted, "OnTaskStarted event should be initialized");
            Assert.NotNull(introductionTask.OnTaskCompleted, "OnTaskCompleted event should be initialized");
            Assert.NotNull(introductionTask.OnTaskFailed, "OnTaskFailed event should be initialized");
        }

        #endregion

        #region CheckCurrentWaypoint Tests

        /// <summary>
        ///     Tests if CheckCurrentWaypoint() starts the task when player reaches the introduction waypoint.
        /// </summary>
        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator IntroductionTask_CheckCurrentWaypoint_StartsTaskForIntroductionWaypoint()
        {
            // Arrange
            bool eventInvoked = false;
            introductionTask.OnTaskStarted.AddListener(() => eventInvoked = true);
            
            // Act
            introductionTask.CheckCurrentWaypoint(waypointComponent);
            yield return new WaitForSeconds(TestDelay);
            yield return null; // Wait for next frame for coroutine to complete
            
            // Assert
            Assert.IsTrue((bool)isTaskStarted.GetValue(introductionTask), "isTaskStarted should be set to true");
            Assert.IsTrue(eventInvoked, "OnTaskStarted event should be invoked");
        }

        /// <summary>
        ///     Tests if CheckCurrentWaypoint does not start the task for non-introduction waypoints.
        /// </summary>
        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator IntroductionTask_CheckCurrentWaypoint_DoesNotStartTaskForNonIntroductionWaypoint()
        {
            // Arrange
            bool eventInvoked = false;
            introductionTask.OnTaskStarted.AddListener(() => eventInvoked = true);
            
            // Create a different waypoint
            var otherWaypointObject = new GameObject("OtherWaypoint");
            var otherWaypoint = otherWaypointObject.AddComponent<Waypoint>();
            
            // Act
            introductionTask.CheckCurrentWaypoint(otherWaypoint);
            yield return new WaitForSeconds(TestDelay);
            yield return null; // Wait for next frame for coroutine to complete
            
            // Assert
            Assert.IsFalse((bool)isTaskStarted.GetValue(introductionTask), "isTaskStarted should remain false");
            Assert.IsFalse(eventInvoked, "OnTaskStarted should not be invoked");
            
            // Cleanup
            Object.Destroy(otherWaypointObject);
        }

        #endregion

        #region DelayedTaskStart Tests

        /// <summary>
        ///     Tests if DelayedTaskStart coroutine calls HandleTask after the specified delay.
        /// </summary>
        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator IntroductionTask_DelayedTaskStart_CallsHandleTaskAfterDelay()
        {
            // Arrange
            bool eventInvoked = false;
            introductionTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            // Act - Call CheckCurrentWaypoint with the introduction waypoint to start DelayedTaskStart
            introductionTask.CheckCurrentWaypoint(waypointComponent);
            
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
        [Test, Order(5)]
        [Category("BuildServer")]
        public void IntroductionTask_HandleTask_SetsTaskStartedAndInvokesEvent()
        {
            // Arrange
            bool eventInvoked = false;
            introductionTask.OnTaskStarted.AddListener(() => eventInvoked = true);

            // Act
            introductionTask.HandleTask();

            // Assert
            Assert.IsTrue((bool)isTaskStarted.GetValue(introductionTask), "isTaskStarted should be set to true");
            Assert.IsTrue(eventInvoked, "OnTaskStarted event should be invoked");
        }

        /// <summary>
        ///     Tests if HandleTask() returns early and doesn't invoke event when task is already completed.
        /// </summary>
        [Test, Order(6)]
        [Category("BuildServer")]
        public void IntroductionTask_HandleTask_ReturnsEarlyIfTaskCompleted()
        {
            // Arrange - Set task to completed state
            bool eventInvoked = false;
            introductionTask.OnTaskStarted.AddListener(() => eventInvoked = true);
            
            isTaskStarted.SetValue(introductionTask, true);
            isTaskCompleted.SetValue(introductionTask, true);
            
            // Act
            introductionTask.HandleTask();
            
            // Assert
            Assert.IsFalse(eventInvoked, "OnTaskStarted should not be invoked if task is already completed");
        }

        #endregion

        #region CompleteTask Tests

        /// <summary>
        ///     Tests if CompleteTask() sets task as completed and invokes OnTaskCompleted event.
        /// </summary>
        [Test, Order(7)]
        [Category("BuildServer")]
        public void IntroductionTask_CompleteTask_SetsTaskCompletedAndInvokesEvent()
        {
            // Arrange
            isTaskStarted.SetValue(introductionTask, true);
            bool eventInvoked = false;
            introductionTask.OnTaskCompleted.AddListener(() => eventInvoked = true);
            
            // Act
            introductionTask.CompleteTask();
            
            // Assert
            Assert.IsTrue((bool)isTaskCompleted.GetValue(introductionTask), "isTaskCompleted should be set to true");
            Assert.IsTrue(eventInvoked, "OnTaskCompleted event should be invoked");
        }

        /// <summary>
        ///     Tests if CompleteTask() returns early if task is not started.
        /// </summary>
        [Test, Order(8)]
        [Category("BuildServer")]
        public void IntroductionTask_CompleteTask_ReturnsEarlyIfTaskNotStarted()
        {
            // Arrange
            isTaskStarted.SetValue(introductionTask, false);
            bool eventInvoked = false;
            introductionTask.OnTaskCompleted.AddListener(() => eventInvoked = true);
            
            // Act
            introductionTask.CompleteTask();
            
            // Assert
            Assert.IsFalse((bool)isTaskCompleted.GetValue(introductionTask), "isTaskCompleted should remain false if task is not started");
            Assert.IsFalse(eventInvoked, "OnTaskCompleted should not be invoked if task is not started");
        }

        /// <summary>
        ///     Tests if CompleteTask() returns early if task is already completed.
        /// </summary>
        [Test, Order(9)]
        [Category("BuildServer")]
        public void IntroductionTask_CompleteTask_ReturnsEarlyIfTaskAlreadyCompleted()
        {
            // Arrange
            isTaskStarted.SetValue(introductionTask, true);
            isTaskCompleted.SetValue(introductionTask, true);
            bool eventInvoked = false;
            introductionTask.OnTaskCompleted.AddListener(() => eventInvoked = true);
            
            // Act
            introductionTask.CompleteTask();
            
            // Assert
            Assert.IsFalse(eventInvoked, "OnTaskCompleted should not be invoked if task is already completed");
        }

        /// <summary>
        /// Call CompleteTask() when run LoadSaveTask
        /// </summary>

        [Test, Order(10)]
        [Category("BuildServer")]

        public void IntroductionTask_LoadSaveTask_SetTaskFlagAndCallCompleteTask()
        {
            //Arrange
            isTaskStarted.SetValue(introductionTask, true);
            isTaskCompleted.SetValue(introductionTask, true);
            //Act
            introductionTask.LoadSaveTask();

            //Asset
            Assert.IsTrue((bool)isTaskCompleted.GetValue(introductionTask), "isTaskCompleted should be set to true");
        }

        #endregion

        #region CheckPoiExit Tests

        /// <summary>
        ///     Tests if CheckPoiExit() invokes OnTaskFailed when player exits Reception POI before task starts.
        /// </summary>
        [Test, Order(11)]
        [Category("BuildServer")]
        public void IntroductionTask_CheckPoiExit_InvokesOnTaskFailedWhenExitingReception()
        {
            // Arrange
            bool eventInvoked = false;
            introductionTask.OnTaskFailed.AddListener(() => eventInvoked = true);
            informDialogSO.SetValue(introductionTask, ScriptableObject.CreateInstance<InformDialog>());
            // Act - Player exits Reception POI before task starts
            introductionTask.CheckPoiExit(receptionPoiComponent);

            // Assert
            Assert.IsTrue(eventInvoked, "OnTaskFailed should be invoked when exiting Reception before task starts");
        }

        /// <summary>
        ///     Tests if CheckPoiExit() doesn't invoke OnTaskFailed if task is already started.
        /// </summary>
        [Test, Order(12)]
        [Category("BuildServer")]
        public void IntroductionTask_CheckPoiExit_DoesNotInvokeOnTaskFailedIfTaskStarted()
        {
            // Arrange
            isTaskStarted.SetValue(introductionTask, true);
            bool eventInvoked = false;
            introductionTask.OnTaskFailed.AddListener(() => eventInvoked = true);
            
            // Act
            introductionTask.CheckPoiExit(receptionPoiComponent);
            
            // Assert
            Assert.IsFalse(eventInvoked, "OnTaskFailed should not be invoked if task is already started");
        }

        #endregion
    }
}
