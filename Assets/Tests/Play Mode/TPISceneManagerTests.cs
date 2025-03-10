using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.DLX;

namespace Tests.PlayMode
{
    /// <summary>
    ///     Play mode tests for the <see cref="TPISceneManager"/> class.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - Singleton initialization and duplicate instance destruction
    ///     - Event invocation on scene restart
    /// </remarks>
    public class TPISceneManagerTests
    {
        #region Fields

        private GameObject sceneManagerObject;
        private TPISceneManager sceneManager;

        #endregion

        #region Test Setup

        /// <summary>
        ///     Sets up a fresh TPISceneManager instance before each test.
        /// </summary>
        [SetUp]
        [Category("BuildServer")]
        public void Setup()
        {
            // Reset static fields
            TPISceneManager.LoadCompleted = false;
            TPISceneManager.CloudSaveDeleted = false;

            // Create a new TPISceneManager instance for testing
            sceneManagerObject = new GameObject("TPISceneManager");
            sceneManager = sceneManagerObject.AddComponent<TPISceneManager>();
        }

        /// <summary>
        ///     Cleans up after each test.
        /// </summary>
        [TearDown]
        [Category("BuildServer")]
        public void Teardown()
        {
            Object.DestroyImmediate(sceneManagerObject);
        }

        #endregion

        #region Tests

        /// <summary>
        ///     Tests if the singleton instance is correctly initialized.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator TPISceneManager_Awake_InitializesSingleton()
        {
            // Arrange - Setup already creates the instance

            // Act
            yield return null; // Wait for Awake to be called

            // Assert
            Assert.IsNotNull(TPISceneManager.Instance, "Singleton instance was not initialized");
            Assert.AreEqual(sceneManager, TPISceneManager.Instance, "Singleton instance does not match the created instance");
        }

        /// <summary>
        ///     Tests if duplicate instances are properly handled.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator TPISceneManager_Awake_DestroysDuplicateInstances()
        {
            // Arrange
            yield return null;  // Wait for the first instance to initialize completely

            // Act - Create the second TPISceneManager instance
            GameObject duplicatedObject = new GameObject("DuplicatedTPISceneManager");
            TPISceneManager duplicatedManager = duplicatedObject.AddComponent<TPISceneManager>();
            yield return null;  // Wait for the duplicatedManager's Awake() to execute and destroy itself.

            // Assert
            Assert.IsTrue(duplicatedObject == null || duplicatedObject.GetComponent<TPISceneManager>() == null, "Duplicated instance was not destroyed");
            Assert.AreEqual(sceneManager, TPISceneManager.Instance, "Original singleton instance was replaced");

            // Cleanup - Destroy the duplicated object if it exists
            if (duplicatedObject != null)
            {
                Object.DestroyImmediate(duplicatedObject);
            }
        }

        /// <summary>
        ///     Tests if OnSceneRestart event is invoked when scene restart is triggered.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator TPISceneManager_RestartScene_InvokesEvent()
        {
            // Arrange
            bool eventInvoked = false;
            sceneManager.OnSceneRestart.AddListener(() => eventInvoked = true);
            yield return null;

            // Act
            sceneManager.RestartScene();
            yield return new WaitForSeconds(0.2f); // Wait for coroutine to complete

            // Assert
            Assert.IsTrue(eventInvoked, "'RestartScene()' did not invoke 'OnSceneRestart' event");
        }

        #endregion
    }
}