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
    ///     Play mode tests for UI window component: <see cref="StartPauseWindowBuilder"/> which now includes
    ///     both pause window and welcome window functionality.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - Scene loading and component reference validation
    ///     - Default window state on scene start
    ///     - Window show/hide functionality in both pause and welcome modes
    ///     - Button click event handling for both modes
    ///     - Timer-related events isolation to pause mode only
    /// </remarks>
    public class StartPauseWindowTests
    {
        #region Fields

        // Window references
        private UIDocument pauseWindowDoc;
        private StartPauseWindowBuilder pauseWindow;
        private VisualElement pauseRoot;
        private Label headerLabel;

        private const string SceneName = "StartPauseWindowTestScene";

        #endregion

        #region Test Setup

        /// <summary>
        ///     Loads the <see cref="PauseWindowTestScene"/> containing the UI window prefabs.
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

            // Act - Get Window references
            pauseWindow = GameObject.FindFirstObjectByType<StartPauseWindowBuilder>();
            pauseWindowDoc = pauseWindow.GetComponent<UIDocument>();
            pauseRoot = pauseWindowDoc.rootVisualElement;
            headerLabel = pauseRoot.Q<Label>("NameLabel");

            // Assert
            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded, "Test scene is not loaded");
            Assert.NotNull(pauseWindow, "Cannot find PauseWindow in the scene");
            Assert.NotNull(pauseWindowDoc, "Cannot find UIDocument on PauseWindow");
            Assert.NotNull(pauseRoot, "Cannot get root VisualElement for PauseWindow");
            Assert.NotNull(headerLabel, "Cannot find NameLabel in the window");
        }

        #endregion

        #region Pause Window Tests

        /// <summary>
        ///     Tests if Window is hidden (display style None) by default when scene starts.
        /// </summary>
        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator Window_Start_IsHidden()
        {
            // Arrange
            yield return null;  // Wait for Start() to be called

            // Act
            // Start() method is called

            // Assert
            Assert.AreEqual(DisplayStyle.None, pauseRoot.style.display.value, "Window is not hidden at scene start");
        }

        /// <summary>
        ///     Tests if Show() method displays window in pause mode (display style Flex), sets correct header text, 
        ///     and invokes both OnWindowShow and OnPauseWindowShow events.
        /// </summary>
        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator PauseWindow_Show_DisplaysWindowAndInvokesEvents()
        {
            // Arrange
            bool windowShowEventInvoked = false;
            bool pauseWindowShowEventInvoked = false;
            pauseWindow.OnWindowShow.AddListener(() => windowShowEventInvoked = true);
            pauseWindow.OnPauseWindowShow.AddListener(() => pauseWindowShowEventInvoked = true);

            // Act
            pauseWindow.Show();
            yield return null;  // Wait for UI update

            // Assert
            Assert.AreEqual(DisplayStyle.Flex, pauseRoot.style.display.value, "Window is not visible after Show()");
            Assert.AreEqual("Pause", headerLabel.text, "Header text is not set to 'Pause' in pause mode");
            Assert.IsTrue(windowShowEventInvoked, "Show() did not invoke OnWindowShow event");
            Assert.IsTrue(pauseWindowShowEventInvoked, "Show() did not invoke OnPauseWindowShow event");

            // Clean up
            pauseWindow.Hide();
            yield return null;
        }

        /// <summary>
        ///     Tests if ShowAsWelcome() method displays window in welcome mode (display style Flex), sets correct header text, 
        ///     invokes OnWindowShow event but NOT OnPauseWindowShow event.
        /// </summary>
        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator WelcomeWindow_ShowAsWelcome_DisplaysWindowAndInvokesOnlyWindowShowEvent()
        {
            // Arrange
            bool windowShowEventInvoked = false;
            bool pauseWindowShowEventInvoked = false;
            pauseWindow.OnWindowShow.AddListener(() => windowShowEventInvoked = true);
            pauseWindow.OnPauseWindowShow.AddListener(() => pauseWindowShowEventInvoked = true);

            // Act
            pauseWindow.ShowAsWelcome();
            yield return null;  // Wait for UI update

            // Assert
            Assert.AreEqual(DisplayStyle.Flex, pauseRoot.style.display.value, "Window is not visible after ShowAsWelcome()");
            Assert.AreEqual("Welcome", headerLabel.text, "Header text is not set to 'Welcome' in welcome mode");
            Assert.IsTrue(windowShowEventInvoked, "ShowAsWelcome() did not invoke OnWindowShow event");
            Assert.IsFalse(pauseWindowShowEventInvoked, "ShowAsWelcome() incorrectly invoked OnPauseWindowShow event");

            // Clean up
            pauseWindow.Hide();
            yield return null;
        }

        /// <summary>
        ///     Tests if restart button click hides window (display style None) and invokes appropriate events.
        /// </summary>
        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator Window_RestartButton_HidesWindowAndInvokesEvents()
        {
            // Arrange
            bool hideEventInvoked = false;
            bool restartEventInvoked = false;
            Button restartButton = pauseRoot.Q<Button>("Restart");

            pauseWindow.Show();
            yield return null;  // Wait for UI update

            pauseWindow.OnWindowHide.AddListener(() => hideEventInvoked = true);
            pauseWindow.OnRestartScene.AddListener(() => restartEventInvoked = true);

            // Act
            var e = new NavigationSubmitEvent() { target = restartButton };
            restartButton.SendEvent(e);
            yield return null;  // Wait for event processing

            // Assert
            Assert.AreEqual(DisplayStyle.None, pauseRoot.style.display.value, "Window is not hidden after clicking restart button");
            Assert.IsTrue(hideEventInvoked, "Restart button click did not invoke OnWindowHide event");
            Assert.IsTrue(restartEventInvoked, "Restart button click did not invoke OnRestartScene event");
        }

        /// <summary>
        ///     Tests if continue button click in pause mode hides window (display style None) and invokes 
        ///     both OnWindowHide and OnPauseWindowHide events.
        /// </summary>
        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator PauseWindow_ContinueButton_HidesWindowAndInvokesEvents()
        {
            // Arrange
            bool windowHideEventInvoked = false;
            bool pauseWindowHideEventInvoked = false;
            Button continueButton = pauseRoot.Q<Button>("Continue");

            pauseWindow.Show(); // Show in pause mode
            yield return null;  // Wait for UI update

            pauseWindow.OnWindowHide.AddListener(() => windowHideEventInvoked = true);
            pauseWindow.OnPauseWindowHide.AddListener(() => pauseWindowHideEventInvoked = true);

            // Act
            var e = new NavigationSubmitEvent() { target = continueButton };
            continueButton.SendEvent(e);
            yield return null;  // Wait for event processing

            // Assert
            Assert.AreEqual(DisplayStyle.None, pauseRoot.style.display.value, "Window is not hidden after clicking continue button in pause mode");
            Assert.IsTrue(windowHideEventInvoked, "Continue button click did not invoke OnWindowHide event");
            Assert.IsTrue(pauseWindowHideEventInvoked, "Continue button click did not invoke OnPauseWindowHide event");
        }

        /// <summary>
        ///     Tests if continue button click in welcome mode hides window, invokes OnWindowHide and OnContinueSavedGame events,
        ///     but NOT OnPauseWindowHide event.
        /// </summary>
        [UnityTest, Order(6)]
        [Category("BuildServer")]
        public IEnumerator WelcomeWindow_ContinueButton_HidesWindowAndInvokesCorrectEvents()
        {
            // Arrange
            bool windowHideEventInvoked = false;
            bool pauseWindowHideEventInvoked = false;
            bool continueEventInvoked = false;
            Button continueButton = pauseRoot.Q<Button>("Continue");

            pauseWindow.ShowAsWelcome(); // Show in welcome mode
            yield return null;  // Wait for UI update

            pauseWindow.OnWindowHide.AddListener(() => windowHideEventInvoked = true);
            pauseWindow.OnPauseWindowHide.AddListener(() => pauseWindowHideEventInvoked = true);
            pauseWindow.OnContinueSavedGame.AddListener(() => continueEventInvoked = true);

            // Act
            var e = new NavigationSubmitEvent() { target = continueButton };
            continueButton.SendEvent(e);
            yield return null;  // Wait for event processing

            // Assert
            Assert.AreEqual(DisplayStyle.None, pauseRoot.style.display.value, "Window is not hidden after clicking continue button in welcome mode");
            Assert.IsTrue(windowHideEventInvoked, "Continue button click did not invoke OnWindowHide event");
            Assert.IsTrue(continueEventInvoked, "Continue button click did not invoke OnContinueSavedGame event");
            Assert.IsFalse(pauseWindowHideEventInvoked, "Continue button click incorrectly invoked OnPauseWindowHide event in welcome mode");
        }

        #endregion
    }
}