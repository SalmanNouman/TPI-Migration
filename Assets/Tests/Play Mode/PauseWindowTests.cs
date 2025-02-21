using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.DLX;

namespace Tests.PlayMode
{
    /// <summary>
    ///     Play mode tests for the <see cref="PauseWindowBuilder"/> class.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - Scene loading and component reference validation
    ///     - Default window state on scene start
    ///     - Window show/hide functionality
    ///     - Button click event handling
    /// </remarks>
    public class PauseWindowTests
    {
        #region Fields

        private UIDocument pauseWindowDoc;
        private PauseWindowBuilder pauseWindow;
        private VisualElement root;

        private const string SceneName = "PauseWindowTestScene";

        #endregion

        #region Test Setup

        /// <summary>
        ///     Loads the <see cref="PauseWindowTestScene"/> containing the PauseWindow prefab.
        /// </summary>
        [OneTimeSetUp]
        [Category("BuildServer")]
        public void LoadTestScene()
        {
            SceneManager.LoadScene(SceneName);
        }

        /// <summary>
        ///     Tests if test scene is loaded and gets component references from the loaded scene.
        /// </summary>
        [UnityTest, Order(0)]
        [Category("BuildServer")]
        public IEnumerator SetupTestEnvironment()
        {
            // Arrange
            yield return new WaitUntil(() => SceneManager.GetSceneByName(SceneName).isLoaded);

            // Act
            pauseWindow = GameObject.FindAnyObjectByType<PauseWindowBuilder>();
            pauseWindowDoc = pauseWindow.GetComponent<UIDocument>();
            root = pauseWindowDoc.rootVisualElement;

            // Assert
            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded, "Test scene is not loaded");
            Assert.NotNull(pauseWindow, "Cannot find PauseWindow in the scene");
            Assert.NotNull(pauseWindowDoc, "Cannot find UIDocument on PauseWindow");
            Assert.NotNull(root, "Cannot get root VisualElement");
        }

        #endregion

        #region Tests

        /// <summary>
        ///     Tests if PauseWindow is hidden (display style None) by default when scene starts.
        /// </summary>
        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator PauseWindow_Start_IsHidden()
        {
            // Arrange
            yield return null;  // Wait for Start() to be called

            // Act
            // Start() method is called

            // Assert
            Assert.AreEqual(DisplayStyle.None, root.style.display.value, "PauseWindow is not hidden at scene start");
        }

        /// <summary>
        ///     Tests if Show() method displays window (display style Flex) and invokes OnWindowShow event.
        /// </summary>
        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator PauseWindow_Show_DisplaysWindowAndInvokesEvent()
        {
            // Arrange
            bool eventInvoked = false;
            pauseWindow.OnWindowShow.AddListener(() => eventInvoked = true);

            // Act
            pauseWindow.Show();
            yield return null;  // Wait for UI update

            // Assert
            Assert.AreEqual(DisplayStyle.Flex, root.style.display.value, "PauseWindow is not visible after Show()");
            Assert.IsTrue(eventInvoked, "Show() did not invoke OnWindowShow event");
        }

        /// <summary>
        ///     Tests if restart button click hides window (display style None) and invokes appropriate events.
        /// </summary>
        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator PauseWindow_RestartButton_HidesWindowAndInvokesEvents()
        {
            // Arrange
            bool hideEventInvoked = false;
            bool restartEventInvoked = false;
            Button restartButton = root.Q<Button>("Restart");
            
            pauseWindow.Show();
            yield return null;  // Wait for UI update

            pauseWindow.OnWindowHide.AddListener(() => hideEventInvoked = true);
            pauseWindow.OnRestartScene.AddListener(() => restartEventInvoked = true);

            // Act
            var e = new NavigationSubmitEvent() { target = restartButton };
            restartButton.SendEvent(e);
            yield return null;  // Wait for event processing

            // Assert
            Assert.AreEqual(DisplayStyle.None, root.style.display.value, "PauseWindow is not hidden after clicking restart button");
            Assert.IsTrue(hideEventInvoked, "Restart button click did not invoke OnWindowHide event");
            Assert.IsTrue(restartEventInvoked, "Restart button click did not invoke OnRestartScene event");
        }

        /// <summary>
        ///     Tests if continue button click hides window (display style None) and invokes OnWindowHide event.
        /// </summary>
        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator PauseWindow_ContinueButton_HidesWindowAndInvokesEvent()
        {
            // Arrange
            bool hideEventInvoked = false;
            Button continueButton = root.Q<Button>("Continue");
            
            pauseWindow.Show();
            yield return null;  // Wait for UI update

            pauseWindow.OnWindowHide.AddListener(() => hideEventInvoked = true);

            // Act
            var e = new NavigationSubmitEvent() { target = continueButton };
            continueButton.SendEvent(e);
            yield return null;  // Wait for event processing

            // Assert
            Assert.AreEqual(DisplayStyle.None, root.style.display.value, "PauseWindow is not hidden after clicking continue button");
            Assert.IsTrue(hideEventInvoked, "Continue button click did not invoke OnWindowHide event");
        }

        #endregion
    }
}