using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VARLab.DLX;

namespace Tests.PlayMode
{
    public class SaveDataSupportTests
    {
        private CustomSaveHandler customSaveHandler;
        private SaveData saveData;
        private TimerManager timerManager;
        private GameObject objectOneGO;
        private GameObject objectTwoGO;
        private InspectableObject objectOne;
        private InspectableObject objectTwo;
        private GameObject poiObject;
        private Poi poi;

        private const string SceneName = "CloudSaveTestScene";

        [OneTimeSetUp]
        [Category("BuildServer")]
        public void RunOnce()
        {
            SceneManager.LoadScene(SceneName);
        }

        [OneTimeTearDown]
        [Category("BuildServer")]
        public void RunOnceTearDown()
        {
            GameObject.Destroy(timerManager);
            SceneManager.UnloadSceneAsync(SceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        }

        /// <summary>
        /// Checks if the test scene is loaded
        /// </summary>
        [UnityTest, Order(0)]
        [Category("BuildServer")]
        public IEnumerator SceneLoaded()
        {
            yield return new WaitUntil(() => SceneManager.GetSceneByName(SceneName).isLoaded);

            customSaveHandler = GameObject.FindAnyObjectByType<CustomSaveHandler>();
            saveData = GameObject.FindAnyObjectByType<SaveData>();
            timerManager = GameObject.FindAnyObjectByType<TimerManager>();

            timerManager.StartTimers();

            // Set up inspectable objects
            objectOneGO = new();
            objectOneGO.AddComponent<BoxCollider>();
            objectOne = objectOneGO.AddComponent<InspectableObject>();
            objectOne.Name = "Obj1";
            objectOne.Location = PoiList.PoiName.Reception;
            objectOne.GeneratedId();

            objectTwoGO = new();
            objectTwoGO.AddComponent<BoxCollider>();
            objectTwo = objectTwoGO.AddComponent<InspectableObject>();
            objectTwo.Name = "Obj2";
            objectTwo.Location = PoiList.PoiName.Reception;
            objectTwo.GeneratedId();

            // Set up POI object
            poiObject = new();
            poi = poiObject.AddComponent<Poi>();

            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
        }

        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator SaveDataVersionIsSetToApplicationVersion()
        {
            // Arrange
            string expectedVersion = "0.0.1";
            saveData.Version = "1.0.0";

            // Act
            customSaveHandler.SetUpInitialData();
            yield return null;

            // Assert
            Assert.AreEqual(expectedVersion, saveData.Version);
        }

        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator OnLoadIsTriggeredWhenCloudSaveLoadIsSuccessful()
        {
            // Arrange
            bool triggered = false;
            customSaveHandler.LoadSuccess = true;
            customSaveHandler.OnLoad.AddListener(() => triggered = true);

            // Act
            customSaveHandler.TriggerLoad();
            yield return null;

            // Assert
            Assert.IsTrue(triggered);
        }

        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator OnLoadIsNotTriggerIfLoadFails()
        {
            // Arrange
            bool triggered = false;
            customSaveHandler.LoadSuccess = false;
            customSaveHandler.OnLoad.AddListener(() => triggered = true);

            // Act
            customSaveHandler.TriggerLoad();
            yield return null;

            // Assert
            Assert.IsFalse(triggered);
        }

        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator CustomSaveHandlerDeleteTriggered()
        {
            // Arrange
            bool triggered = false;
            customSaveHandler.OnDeleteStart.AddListener(() => triggered = true);

            // Act
            customSaveHandler.TriggerDelete();
            yield return null;

            // Assert
            Assert.IsTrue(triggered);
        }

        [UnityTest, Order(6)]
        [Category("BuildServer")]
        public IEnumerator SavingInspectionLog_Adds_LogList_To_SaveData()
        {
            // Arrange
            List<InspectionData> inspectionLog = new();
            InspectionData inspectionDataOne = new(objectOne, true, false);
            InspectionData inspectionDataTwo = new(objectTwo, true, false);
            inspectionLog.Add(inspectionDataOne);
            inspectionLog.Add(inspectionDataTwo);

            // Act
            customSaveHandler.SaveInspectionLog(inspectionLog);
            yield return null;

            // Assert
            Assert.AreEqual(inspectionLog.Count, saveData.InspectionLog.Count);
        }

        /// <summary>
        /// Tests if SaveActivityLog correctly adds activity logs to SaveData
        /// </summary>
        [UnityTest, Order(7)]
        [Category("BuildServer")]
        public IEnumerator SavingActivityLog_Adds_LogList_To_SaveData()
        {
            // Arrange
            customSaveHandler.CanSave = true;
            List<Log> activityLog = new List<Log>
            {
                new Log(true, "Test Primary Log"),
                new Log(false, "Test Secondary Log")
            };

            // Act
            customSaveHandler.SaveActivityLog(activityLog);
            yield return null;

            // Assert
            Assert.IsNotNull(saveData.ActivityLog, "Activity log was not saved to SaveData");
            Assert.AreEqual(activityLog.Count, saveData.ActivityLog.Count, "Activity log count does not match");

            // Check content of first log
            Assert.AreEqual(activityLog[0].IsPrimary, saveData.ActivityLog[0].IsPrimary, "Primary flag was not saved correctly");
            Assert.AreEqual(activityLog[0].Message, saveData.ActivityLog[0].LogString, "Log message was not saved correctly");

            // Check content of second log
            Assert.AreEqual(activityLog[1].IsPrimary, saveData.ActivityLog[1].IsPrimary, "Primary flag was not saved correctly");
            Assert.AreEqual(activityLog[1].Message, saveData.ActivityLog[1].LogString, "Log message was not saved correctly");
        }

        [UnityTest, Order(8)]
        [Category("BuildServer")]
        public IEnumerator SavingPhotos_Adds_ObjectIdAndTimestamp_To_SaveData()
        {
            // Arrange
            List<InspectablePhoto> photos = new();
            byte[] data = null;
            InspectablePhoto photoOne = new(data, objectOne.ObjectId, objectOne.Location.ToString(), "Timestamp");
            photos.Add(photoOne);

            // Act
            customSaveHandler.SavePhotos(photos);
            yield return null;

            // Assert
            Assert.AreEqual(photos.Count, saveData.PhotoIdAndTimeStamp.Count);
            Assert.IsTrue(saveData.PhotoIdAndTimeStamp.ContainsKey(photos[0].Id));
        }

        #region Auto-Save Tests

        /// <summary>
        /// Tests if auto-save does not trigger when conditions are not met.
        /// </summary>
        [UnityTest, Order(9)]
        [Category("BuildServer")]
        public IEnumerator AutoSave_DoesNotTriggerWhenConditionsNotMet()
        {
            // Arrange

            // Get access to the autoSaveTimer field using reflection
            var autoSaveTimerField = typeof(CustomSaveHandler).GetField("autoSaveStopwatch",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var autoSaveTimer = (System.Diagnostics.Stopwatch)autoSaveTimerField.GetValue(customSaveHandler);

            // We wont start any timer

            // Act
            bool autoSaveTriggered = customSaveHandler.CheckAndTriggerAutoSave();

            yield return null;

            // Assert
            Assert.IsFalse(autoSaveTriggered, "Auto-save should not be triggered when conditions are not met");
        }
        #endregion

        /// <summary>
        ///     Tests if SaveLastPOI() correctly stores the POI name in the save data when CanSave is true.
        /// </summary>
        [UnityTest, Order(10)]
        [Category("BuildServer")]
        public IEnumerator SaveDataSupport_SaveLastPOI_StoresPOINameInSaveData()
        {
            // Arrange
            poi.SelectedPoiName = PoiList.PoiName.Reception;
            customSaveHandler.CanSave = true;

            // Act
            customSaveHandler.SaveLastPOI(poi);
            yield return null;

            // Assert
            Assert.AreEqual(poi.SelectedPoiName.ToString(), saveData.LastPOI);
        }

        /// <summary>
        ///     Tests if SaveLastPOI() does not modify the save data when CanSave is false.
        /// </summary>
        [UnityTest, Order(11)]
        [Category("BuildServer")]
        public IEnumerator SaveDataSupport_SaveLastPOI_DoesNotSaveWhenCanSaveIsFalse()
        {
            // Arrange
            poi.SelectedPoiName = PoiList.PoiName.Bathroom;
            customSaveHandler.CanSave = false;
            saveData.LastPOI = "";

            // Act
            customSaveHandler.SaveLastPOI(poi);
            yield return null;

            // Assert
            Assert.AreEqual("", saveData.LastPOI);
        }

        /// <summary>
        ///     Tests if MovePlayer event is triggered with LastPOI when OnLoad is invoked.
        /// </summary>
        [UnityTest, Order(12)]
        [Category("BuildServer")]
        public IEnumerator SaveDataSupport_OnLoad_InvokesMovePlayerWithLastPOI()
        {
            // Arrange
            string testPOI = "Reception";
            saveData.LastPOI = testPOI;
            string movedToPOI = null;
            customSaveHandler.MovePlayer.RemoveAllListeners();
            customSaveHandler.MovePlayer.AddListener((poi) => movedToPOI = poi);

            // Act
            customSaveHandler.OnLoad.Invoke();
            yield return null;

            // Assert
            Assert.AreEqual(testPOI, movedToPOI, "MovePlayer event should be invoked with the LastPOI value");
        }

        /// <summary>
        ///     Tests if OnLoadRestart sets Restarted flag to true when deletion succeeds
        /// </summary>
        [UnityTest, Order(13)]
        [Category("BuildServer")]
        public IEnumerator SaveDataSupport_OnLoadRestart_SetsRestartedFlagOnSuccessfulDeletion()
        {
            // Arrange
            CustomSaveHandler.Restarted = false;
            customSaveHandler.DeleteSuccess = null;

            // Act
            customSaveHandler.OnLoadRestart();
            yield return null;
            customSaveHandler.DeleteSuccess = true;
            yield return new WaitForSeconds(0.1f); // Wait for coroutine to process it

            // Assert
            Assert.IsTrue(CustomSaveHandler.Restarted);

            // Cleanup
            CustomSaveHandler.Restarted = false;
        }
    }
}
