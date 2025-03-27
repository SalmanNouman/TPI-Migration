using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.DLX;
using static VARLab.DLX.SaveData;

namespace Tests.PlayMode
{
    /// <summary>
    ///     Play mode tests for the <see cref="ActivityLog"/> class.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - Activity log initialization and configuration
    ///     - Log message creation with timestamps
    ///     - OnLogAdded event invocation
    ///     - Primary and secondary log recording
    ///     - POI enter/exit logging
    ///     - Inspectable object click interaction logging
    ///     - Taken Photo logging
    /// </remarks>
    public class ActivityLogTests
    {
        #region Classes

        /// <summary>
        ///     Minimal test implementation of InspectableObject.
        ///     Properties like 'Name' can be set directly in test setup.
        /// </summary>
        private class TestInspectableObject : InspectableObject
        {
        }

        #endregion

        #region Fields

        private GameObject activityLogObject;
        private ActivityLog activityLog;
        private GameObject poiObject;
        private Poi poi;
        private GameObject inspectableObject;
        private InspectableObject inspectable;

        // Time delay (in seconds) for verifying activity log timestamps in tests
        private const float TestElapsedTime = 1.0f;

        #endregion

        #region Helper Methods

        /// <summary>
        ///     Simulates time passing in the test environment.
        ///     Used to verify that activity logs correctly include elapsed time in their messages.
        /// </summary>
        /// <param name="elapsedTime">Wait time (default value is <see cref="TestElapsedTime"/>)</param>
        private IEnumerator SetupElapsedTime(float elapsedTime = TestElapsedTime)
        {
            yield return new WaitForSeconds(elapsedTime);
        }

        #endregion

        #region Test Setup

        /// <summary>
        ///     Sets up the test environment.
        /// </summary>
        /// <remarks>
        ///     Creates and configures necessary test components:
        ///     - TimerManager for timestamp tracking
        ///     - ActivityLog object for logging functionality
        ///     - POI object for location tracking
        ///     - Inspectable object with collider for interaction testing
        /// </remarks>
        [SetUp]
        [Category("BuildServer")]
        public void Setup()
        {
            // Ensure TimerManager instance exists
            if (TimerManager.Instance == null)
            {
                var timerManagerObject = new GameObject("TimerManager");
                timerManagerObject.AddComponent<TimerManager>();
            }

            // Initialize and start the timer for activity logging
            TimerManager.Instance.StartTimers();

            // Setup ActivityLog object
            activityLogObject = new GameObject("TestActivityLog");
            activityLog = activityLogObject.AddComponent<ActivityLog>();

            // Setup POI object
            poiObject = new GameObject("TestPOI");
            poi = poiObject.AddComponent<Poi>();
            poi.SelectedPoiName = PoiList.PoiName.TattooArea;

            // Setup Inspectable object with test implementation
            inspectableObject = new GameObject("TestInspectable");
            var collider = inspectableObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            inspectable = inspectableObject.AddComponent<TestInspectableObject>();
            inspectable.Name = "Test Object";
        }

        /// <summary>
        ///     Cleans up test objects.
        /// </summary>
        [TearDown]
        [Category("BuildServer")]
        public void Teardown()
        {
            Object.DestroyImmediate(activityLogObject);
            Object.DestroyImmediate(poiObject);
            Object.DestroyImmediate(inspectableObject);
        }

        #endregion

        #region Initialization Tests

        /// <summary>
        ///     Tests if ActivityLog is correctly initialized during Start.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator ActivityLog_Start_InitializesCorrectly()
        {
            // Arrange
            // ActivityLog object is already setup in the Setup() method
            yield return null;  // Wait for Start() to be called

            // Act
            // Start() method is called

            // Assert
            Assert.IsNotNull(activityLog.ActivityLogList, "ActivityLogList was not initialized in Start()");
            Assert.IsNotNull(activityLog.OnLogAdded, "OnLogAdded event was not initialized in Start()");
            Assert.AreEqual(0, activityLog.ActivityLogList.Count, "ActivityLogList was not empty after initialization");
        }

        #endregion

        #region Logging Tests

        /// <summary>
        ///     Tests if OnLogAdded event is properly invoked.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator ActivityLog_OnLogAdded_IsInvokedCorrectly()
        {
            // Arrange
            activityLog.CanLog = true;
            bool eventInvoked = false;
            activityLog.OnLogAdded.AddListener((logs) => eventInvoked = true);
            yield return null;  // Wait for Start()

            // Act
            activityLog.LogPoiEnter(poi);

            // Assert
            Assert.IsTrue(eventInvoked, "OnLogAdded event was not invoked after adding a new log");
        }

        /// <summary>
        ///     Tests if logging is properly disabled when CanLog is false.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator ActivityLog_DisabledLogging_DoesNotCreateLogs()
        {
            // Arrange
            activityLog.CanLog = false;
            yield return null;  // Wait for Start()

            // Act
            activityLog.LogPoiEnter(poi);

            // Assert
            Assert.AreEqual(0, activityLog.ActivityLogList.Count, "Logs were created when logging was disabled");
        }

        /// <summary>
        ///     Tests if POI entry is correctly logged.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator ActivityLog_LogPoiEnter_CreatesCorrectLog()
        {
            // Arrange
            activityLog.CanLog = true;
            yield return null;  // Wait for Start()
            yield return SetupElapsedTime();  // Add time delay for timestamp

            // Act
            activityLog.LogPoiEnter(poi);

            // Assert
            Assert.AreEqual(1, activityLog.ActivityLogList.Count, "Log entry was not created");
            Assert.IsTrue(activityLog.ActivityLogList[0].IsPrimary, "POI entry log was not marked as primary");
            string expectedFormat = $"{TimerManager.Instance.GetElapsedTime()} Entered {poi.PoiName}";
            StringAssert.IsMatch(expectedFormat, activityLog.ActivityLogList[0].Message,
                $"Expected message format: '{expectedFormat}', Current Result: '{activityLog.ActivityLogList[0].Message}'");
        }

        /// <summary>
        ///     Tests if POI exit is correctly logged.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator ActivityLog_LogExitPoi_CreatesCorrectLog()
        {
            // Arrange
            activityLog.CanLog = true;
            yield return null;  // Wait for Start()
            yield return SetupElapsedTime();  // Add time delay for updating timestamp

            // Act
            activityLog.LogExitPoi(poi);

            // Assert
            Assert.AreEqual(1, activityLog.ActivityLogList.Count, "Log entry was not created");
            Assert.IsFalse(activityLog.ActivityLogList[0].IsPrimary, "POI exit log was not marked as secondary");
            string expectedFormat = $"{TimerManager.Instance.GetElapsedTime()} Exited {poi.PoiName}";
            StringAssert.IsMatch(expectedFormat, activityLog.ActivityLogList[0].Message,
                $"Expected message format: '{expectedFormat}', Current Result: '{activityLog.ActivityLogList[0].Message}'");
        }

        /// <summary>
        ///     Tests if object click interaction is correctly logged.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator ActivityLog_LogObjectClicked_CreatesCorrectLog()
        {
            // Arrange
            activityLog.CanLog = true;
            yield return null;  // Wait for Start()
            yield return SetupElapsedTime();  // Add time delay for updating timestamp

            // Act
            activityLog.LogObjectClicked(inspectable);

            // Assert
            Assert.AreEqual(1, activityLog.ActivityLogList.Count, "Log entry was not created");
            Assert.IsFalse(activityLog.ActivityLogList[0].IsPrimary, "Object click interaction log was not marked as secondary");
            string expectedFormat = $"{TimerManager.Instance.GetElapsedTime()} {inspectable.Name} - Visual Inspection";
            StringAssert.IsMatch(expectedFormat, activityLog.ActivityLogList[0].Message,
                $"Expected message format: '{expectedFormat}', Current Result: '{activityLog.ActivityLogList[0].Message}'");
        }

        /// <summary>
        ///     Tests if photo taken interaction is correctly logged.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator ActivityLog_LogPhotoTaken_CreatesCorrectLog()
        {
            // Arrange
            activityLog.CanLog = true;
            yield return null;  // Wait for Start()
            yield return SetupElapsedTime();  // Add time delay for updating timestamp

            // Act
            activityLog.LogPhotoTaken(inspectable);

            // Assert
            Assert.AreEqual(1, activityLog.ActivityLogList.Count, "Log entry was not created");
            Assert.IsFalse(activityLog.ActivityLogList[0].IsPrimary, "Photo taken log was not marked as secondary");
            string expectedFormat = $"{TimerManager.Instance.GetElapsedTime()} {inspectable.Name} - Photo Taken";
            StringAssert.IsMatch(expectedFormat, activityLog.ActivityLogList[0].Message,
                $"Expected message format: '{expectedFormat}', Current Result: '{activityLog.ActivityLogList[0].Message}'");
        }

        /// <summary>
        ///     Tests if photo deleted interaction is correctly logged.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator ActivityLog_LogPhotoDeleted_CreatesCorrectLog()
        {
            // Arrange
            activityLog.CanLog = true;
            yield return null;  // Wait for Start()
            yield return SetupElapsedTime();  // Add time delay for updating timestamp

            // Act
            activityLog.LogDeletedPhoto(inspectable.Name);

            // Assert
            Assert.AreEqual(1, activityLog.ActivityLogList.Count, "Log entry was not created");
            Assert.IsFalse(activityLog.ActivityLogList[0].IsPrimary, "Photo deleted log was not marked as secondary");
            string expectedFormat = $"{TimerManager.Instance.GetElapsedTime()} {inspectable.Name} - Photo Deleted";
            StringAssert.IsMatch(expectedFormat, activityLog.ActivityLogList[0].Message,
                $"Expected message format: '{expectedFormat}', Current Result: '{activityLog.ActivityLogList[0].Message}'");
        }

        /// <summary>
        /// Tests if inspection log deleted interaction is correctly logged.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator ActivityLog_LogInspectionDeleted_CreatesCorrectLog()
        {
            // Arrange
            activityLog.CanLog = true;
            yield return null;  // Wait for Start()
            yield return SetupElapsedTime();  // Add time delay for updating timestamp

            // Act
            activityLog.LogDeletedInspection(inspectable.Name);

            // Assert
            Assert.AreEqual(1, activityLog.ActivityLogList.Count, "Log entry was not created");
            Assert.IsFalse(activityLog.ActivityLogList[0].IsPrimary, "Inspection deleted log was not marked as secondary");
            string expectedFormat = $"{TimerManager.Instance.GetElapsedTime()} {inspectable.Name} - Visual Inspection Deleted";
            StringAssert.IsMatch(expectedFormat, activityLog.ActivityLogList[0].Message,
                $"Expected message format: '{expectedFormat}', Current Result: '{activityLog.ActivityLogList[0].Message}'");
        }

        #endregion

        #region Save/Load Tests

        /// <summary>
        /// Tests if LoadSavedLogs correctly updates the ActivityLogList and triggers the OnLogAdded event.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator ActivityLog_LoadSavedLogs_UpdatesActivityLogList()
        {
            // Arrange
            bool eventInvoked = false;
            List<Log> loadedLogs = null;
            activityLog.OnLogAdded.AddListener((logs) =>
            {
                eventInvoked = true;
                loadedLogs = logs;
            });

            List<Log> testLogs = new List<Log>
            {
                new Log(true, "Test Primary Log"),
                new Log(false, "Test Secondary Log")
            };

            // Clear any existing logs
            activityLog.ActivityLogList.Clear();

            // Act
            activityLog.LoadSavedLogs(testLogs);
            yield return null;

            // Assert
            Assert.IsTrue(eventInvoked, "OnLogAdded event was not invoked after loading logs");
            Assert.IsNotNull(loadedLogs, "Loaded logs are null");
            Assert.AreEqual(testLogs.Count, activityLog.ActivityLogList.Count, "ActivityLogList count does not match test logs count");

            // Check content of first log
            Assert.AreEqual(testLogs[0].IsPrimary, activityLog.ActivityLogList[0].IsPrimary, "Primary flag was not loaded correctly");
            Assert.AreEqual(testLogs[0].Message, activityLog.ActivityLogList[0].Message, "Log message was not loaded correctly");

            // Check content of second log
            Assert.AreEqual(testLogs[1].IsPrimary, activityLog.ActivityLogList[1].IsPrimary, "Primary flag was not loaded correctly");
            Assert.AreEqual(testLogs[1].Message, activityLog.ActivityLogList[1].Message, "Log message was not loaded correctly");
        }

        /// <summary>
        /// Tests if LoadSavedLogs handles empty log lists correctly.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator ActivityLog_LoadSavedLogs_HandlesEmptyLogList()
        {
            // Arrange
            bool eventInvoked = false;
            activityLog.OnLogAdded.AddListener((logs) => eventInvoked = true);

            // Add some initial logs
            activityLog.ActivityLogList.Clear();
            activityLog.ActivityLogList.Add(new Log(true, "Initial Log"));

            // Create empty test logs
            List<Log> emptyLogs = new List<Log>();

            // Act
            activityLog.LoadSavedLogs(emptyLogs);
            yield return null;

            // Assert
            Assert.IsTrue(eventInvoked, "OnLogAdded event was not invoked after loading empty logs");
            Assert.AreEqual(0, activityLog.ActivityLogList.Count, "ActivityLogList should be empty after loading empty logs");
        }

        /// <summary>
        /// Tests if GetSavedLogs correctly converts ActivityData to Log objects.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator ActivityLog_GetSavedLogs_ConvertsActivityDataToLogs()
        {
            // Arrange
            bool eventInvoked = false;
            List<Log> loadedLogs = null;
            activityLog.OnLogAdded.AddListener((logs) =>
            {
                eventInvoked = true;
                loadedLogs = logs;
            });

            // Create test ActivityData objects (the format used in cloud save)
            List<ActivityData> savedActivityData = new List<ActivityData>
            {
                new ActivityData { IsPrimary = true, LogString = "Test Primary Log" },
                new ActivityData { IsPrimary = false, LogString = "Test Secondary Log" }
            };

            // Clear existing logs
            activityLog.ActivityLogList.Clear();

            // Act
            activityLog.GetSavedLogs(savedActivityData);
            yield return null;

            // Assert
            Assert.IsTrue(eventInvoked, "OnLogAdded event was not invoked after loading logs");
            Assert.IsNotNull(loadedLogs, "Loaded logs are null");
            Assert.AreEqual(2, activityLog.ActivityLogList.Count, "ActivityLogList should contain 2 logs");

            // Check content of first log
            Assert.IsTrue(activityLog.ActivityLogList[0].IsPrimary, "First log should be primary");
            Assert.AreEqual("Test Primary Log", activityLog.ActivityLogList[0].Message, "First log message should match");

            // Check content of second log
            Assert.IsFalse(activityLog.ActivityLogList[1].IsPrimary, "Second log should be secondary");
            Assert.AreEqual("Test Secondary Log", activityLog.ActivityLogList[1].Message, "Second log message should match");
        }

        #endregion
    }
}