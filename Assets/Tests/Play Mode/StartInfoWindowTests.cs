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
    ///     Play mode tests for the <see cref="StartInfoWindowBuilder"/> class.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - Scene loading and component reference validation
    ///     - Default window state on scene start
    ///     - Window show/hide functionality
    ///     - Button click event handling
    /// </remarks>
    public class StartInfoWindowTests
    {
        #region Fields

        // Window references
        private UIDocument startInfoWindowDoc;
        private StartInfoWindowBuilder startInfoWindow;
        private VisualElement root;
        private Button beginButton;

        private const string SceneName = "StartInfoWindowTestScene";

        #endregion

        #region Test Setup

        /// <summary>
        ///     Loads the <see cref="StartInfoWindowTestScene"/> containing the UI window prefab.
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
        public IEnumerator StartInfoWindowTests_SetupTestEnvironment_LoadsComponentReferences()
        {
            // Arrange
            yield return new WaitUntil(() => SceneManager.GetSceneByName(SceneName).isLoaded);

            // Act - Get Window references
            startInfoWindow = GameObject.FindFirstObjectByType<StartInfoWindowBuilder>();
            startInfoWindowDoc = startInfoWindow.GetComponent<UIDocument>();
            root = startInfoWindowDoc.rootVisualElement;

            // Get correct button reference from UXML (named "Button" in the actual UXML)
            beginButton = root.Q<Button>("Button");

            // Assert
            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded, "Test scene is not loaded");
            Assert.NotNull(startInfoWindow, "Cannot find StartInfoWindow in the scene");
            Assert.NotNull(startInfoWindowDoc, "Cannot find UIDocument on StartInfoWindow");
            Assert.NotNull(root, "Cannot get root VisualElement for StartInfoWindow");
            Assert.NotNull(beginButton, "Cannot find Button in the window");
        }

        #endregion

        #region Tests

        /// <summary>
        ///     Tests if Window is hidden (display style None) by default when scene starts.
        /// </summary>
        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator StartInfoWindowBuilder_Start_InitializesWithHiddenState()
        {
            // Arrange
            yield return null;  // Wait for Start() to be called

            // Act
            // Start() method is called

            // Assert
            Assert.AreEqual(DisplayStyle.None, root.style.display.value, "Window is not hidden at scene start");
        }

        /// <summary>
        ///     Tests if Show() method displays window (display style Flex) and invokes OnWindowShow event.
        /// </summary>
        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator StartInfoWindowBuilder_Show_DisplaysWindowAndInvokesEvent()
        {
            // Arrange
            bool windowShowEventInvoked = false;
            startInfoWindow.OnWindowShow.AddListener(() => windowShowEventInvoked = true);

            // Act
            startInfoWindow.Show();
            yield return null;  // Wait for UI update

            // Assert
            Assert.AreEqual(DisplayStyle.Flex, root.style.display.value, "Window is not visible after Show()");
            Assert.IsTrue(windowShowEventInvoked, "Show() did not invoke OnWindowShow event");

            // Clean up
            startInfoWindow.Hide();
            yield return null;
        }

        /// <summary>
        ///     Tests if begin button click hides window (display style None) and invokes OnWindowHide event.
        /// </summary>
        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator StartInfoWindowBuilder_SetupButtonListeners_HandleBeginButtonClickCorrectly()
        {
            // Arrange
            bool windowHideEventInvoked = false;
            startInfoWindow.Show();
            yield return null;  // Wait for UI update

            startInfoWindow.OnWindowHide.AddListener(() => windowHideEventInvoked = true);

            // Act
            var e = new NavigationSubmitEvent() { target = beginButton };
            beginButton.SendEvent(e);
            yield return null;  // Wait for event processing

            // Assert
            Assert.AreEqual(DisplayStyle.None, root.style.display.value, "Window is not hidden after clicking begin button");
            Assert.IsTrue(windowHideEventInvoked, "Begin button click did not invoke OnWindowHide event");
        }

        #endregion
    }
}