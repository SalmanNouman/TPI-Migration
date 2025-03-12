using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VARLab.DLX;

namespace Tests.PlayMode
{
    public class SaveDataSupportTests
    {
        private SaveDataSupport saveDataSupport;
        private SaveData saveData;
        private CustomSaveHandler customSaveHandler;
        private TimerManager timerManager;

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

            saveDataSupport = GameObject.FindAnyObjectByType<SaveDataSupport>();
            saveData = GameObject.FindAnyObjectByType<SaveData>();
            customSaveHandler = GameObject.FindAnyObjectByType<CustomSaveHandler>();
            timerManager = GameObject.FindAnyObjectByType<TimerManager>();

            timerManager.StartTimers();

            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
        }

        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator OnInitializeIsInvokedOnStart()
        {
            // Arrange
            var triggered = false;
            saveDataSupport.OnInitialize.AddListener(() => triggered = true);

            // Act
            saveDataSupport.Initialize();

            yield return null;

            // Assert
            Assert.IsTrue(triggered);
        }

        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator SaveDataVersionIsSetToApplicationVersion()
        {
            // Arrange
            string expectedVersion = "0.0.1";
            saveData.Version = "1.0.0";

            // Act
            saveDataSupport.SetUpInitialData();
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
            saveDataSupport.OnLoad.AddListener(() => triggered = true);

            // Act
            saveDataSupport.TriggerLoad();
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
            saveDataSupport.OnLoad.AddListener(() => triggered = true);

            // Act
            saveDataSupport.TriggerLoad();
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
            saveDataSupport.TriggerDelete();
            yield return null;

            // Assert
            Assert.IsTrue(triggered);
        }
    }
}
