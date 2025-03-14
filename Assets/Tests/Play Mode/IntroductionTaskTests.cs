using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Events;
using VARLab.DLX;
using System.Reflection;

namespace Tests.PlayMode
{
    /// <summary>
    ///     Play mode tests for the <see cref="IntroductionTask"/> class.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - Component initialization
    ///     - Task state management (started, completed, failed)
    ///     - Trigger detection for task
    ///     - Early return behaviors for different task states
    ///     - Event invocation verification
    /// </remarks>
    public class IntroductionTaskTests
    {
        #region Fields

        private GameObject testObject;
        private IntroductionTask introductionTask;
        private GameObject receptionPoiObject;
        private GameObject restrictedPoiObject;
        private Poi receptionPoiComponent;
        private Poi restrictedPoiComponent;
        private GameObject playerObject;
        private Collider playerCollider;

        // Field info for private fields access
        private FieldInfo receptionPoi;
        private FieldInfo isTaskStarted;
        private FieldInfo isTaskCompleted;
        private FieldInfo conversationDelay;
        
        // Test constants
        private const float testDelay = 0.1f;

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

            restrictedPoiObject = new GameObject("TestRestrictedPOI");
            restrictedPoiComponent = restrictedPoiObject.AddComponent<Poi>();
            restrictedPoiComponent.PoiName = "RestrictedArea";
            
            // Setup player object
            playerObject = new GameObject("TestPlayer");
            playerObject.tag = "Player";
            playerCollider = playerObject.AddComponent<BoxCollider>();

            // Get private fields using reflection
            // * Reflection is used to access a private field
            receptionPoi = typeof(IntroductionTask).GetField("receptionPoi", BindingFlags.NonPublic | BindingFlags.Instance);
            isTaskStarted = typeof(IntroductionTask).GetField("isTaskStarted", BindingFlags.NonPublic | BindingFlags.Instance);
            isTaskCompleted = typeof(IntroductionTask).GetField("isTaskCompleted", BindingFlags.NonPublic | BindingFlags.Instance);
            conversationDelay = typeof(IntroductionTask).GetField("conversationDelay", BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Set Reception POI as the allowed area
            receptionPoi.SetValue(introductionTask, receptionPoiComponent);
            
            // Set a small delay for testing
            conversationDelay.SetValue(introductionTask, testDelay);
        }

        /// <summary>
        ///     Cleans up the test environment after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.Destroy(testObject);
            Object.Destroy(receptionPoiObject);
            Object.Destroy(restrictedPoiObject);
            Object.Destroy(playerObject);
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

        #region OnTriggerEnter Tests

        /// <summary>
        ///     Tests if OnTriggerEnter() starts the task when player enters the trigger area.
        /// </summary>
        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator IntroductionTask_OnTriggerEnter_StartsTaskForPlayerCollider()
        {
            // Arrange
            bool eventInvoked = false;
            introductionTask.OnTaskStarted.AddListener(() => eventInvoked = true);
            
            // Act - Invoke OnTriggerEnter through reflection
            MethodInfo onTriggerEnterMethod = typeof(IntroductionTask).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            onTriggerEnterMethod.Invoke(introductionTask, new object[] { playerCollider });
            yield return new WaitForSeconds(testDelay); // Wait for delay to complete
            
            // Assert
            Assert.IsTrue((bool)isTaskStarted.GetValue(introductionTask), "isTaskStarted should be set to true");
            Assert.IsTrue(eventInvoked, "OnTaskStarted event should be invoked");
        }

        /// <summary>
        ///     Tests if OnTriggerEnter starts the DelayedTaskStart coroutine.
        /// </summary>
        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator IntroductionTask_OnTriggerEnter_StartsDelayedTaskCoroutine()
        {
            // Arrange
            bool eventInvoked = false;
            introductionTask.OnTaskStarted.AddListener(() => eventInvoked = true);
            
            // Act - Invoke OnTriggerEnter through reflection
            MethodInfo onTriggerEnterMethod = typeof(IntroductionTask).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            onTriggerEnterMethod.Invoke(introductionTask, new object[] { playerCollider });
            
            // Wait less than the delay - task shouldn't be started yet
            yield return new WaitForSeconds(testDelay * 0.5f);
            Assert.IsFalse(eventInvoked, "Task should not be started before delay completes");
            
            // Wait for the full delay
            yield return new WaitForSeconds(testDelay * 0.6f);
            
            // Assert - Task should be started after delay
            Assert.IsTrue(eventInvoked, "OnTaskStarted should be invoked after delay");
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

            // Act - Invoke OnTriggerEnter to start the DelayedTaskStart coroutine
            MethodInfo onTriggerEnterMethod = typeof(IntroductionTask).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            onTriggerEnterMethod.Invoke(introductionTask, new object[] { playerCollider });

            yield return null;
            yield return new WaitForSeconds(testDelay); // Wait for delay duration
            yield return null; // Wait for next frame for coroutine to complete

            // Assert - Task should be started after delay
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

        #endregion

        #region CheckCurrentPoi Tests

        /// <summary>
        ///     Tests if CheckCurrentPoi() invokes OnTaskFailed when player enters restricted area.
        /// </summary>
        [Test, Order(10)]
        [Category("BuildServer")]
        public void IntroductionTask_CheckCurrentPoi_InvokesOnTaskFailedForRestrictedArea()
        {
            // Arrange
            bool eventInvoked = false;
            introductionTask.OnTaskFailed.AddListener(() => eventInvoked = true);
            
            // Act
            introductionTask.CheckCurrentPoi(restrictedPoiComponent);
            
            // Assert
            Assert.IsTrue(eventInvoked, "OnTaskFailed should be invoked when entering restricted area");
        }

        /// <summary>
        ///     Tests if CheckCurrentPoi() doesn't invoke OnTaskFailed if task is already completed.
        /// </summary>
        [Test, Order(11)]
        [Category("BuildServer")]
        public void IntroductionTask_CheckCurrentPoi_DoesNotInvokeOnTaskFailedIfTaskCompleted()
        {
            // Arrange
            isTaskCompleted.SetValue(introductionTask, true);
            bool eventInvoked = false;
            introductionTask.OnTaskFailed.AddListener(() => eventInvoked = true);
            
            // Act
            introductionTask.CheckCurrentPoi(restrictedPoiComponent);
            
            // Assert
            Assert.IsFalse(eventInvoked, "OnTaskFailed should not be invoked if task is already completed");
        }

        #endregion
    }
}
