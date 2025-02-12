using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.DLX;

namespace Tests.PlayMode
{
    public class MenuIntegrationTests
    {
        private GameObject menuObject;
        private UIDocument menuDocument;
        private MenuBuilder menuBuilder;

        private const string SceneName = "MenuButtonsTestScene";

        /// <summary>
        /// Loads the menu buttons test scene
        /// </summary>
        [OneTimeSetUp]
        [Category("BuildServer")]
        public void RunOnce()
        {
            SceneManager.LoadScene(SceneName);
        }


        /// <summary>
        /// Checks if the test scene is loaded
        /// </summary>
        [UnityTest, Order(0)]
        [Category("BuildServer")]
        public IEnumerator SceneLoaded()
        {
            yield return new WaitUntil(() => SceneManager.GetSceneByName(SceneName).isLoaded);

            menuBuilder = GameObject.FindAnyObjectByType<MenuBuilder>();
            menuDocument = menuBuilder.GetComponent<UIDocument>();

            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
        }

        /// <summary>
        /// Checks if the UI buttons are visible when Show is called
        /// </summary>
        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator CheckIfUIIsEnabledOnShow()
        {
            // Arrange
            var expectedResults = DisplayStyle.Flex.ToString().Trim();
            VisualElement root = menuDocument.rootVisualElement;

            // Act
            menuBuilder.Hide();
            yield return new WaitForSeconds(1f);
            menuBuilder.Show();
            yield return new WaitForSeconds(1f);

            // Assert
            Assert.AreEqual(expectedResults, root.style.display.ToString().Trim());
        }

        /// <summary>
        /// Checks if the UI is hidden on Hide
        /// </summary>
        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator CheckIfUIIsDisabledOnHide()
        {
            // Arrange
            var expectedResults = DisplayStyle.None.ToString().Trim();
            VisualElement root = menuDocument.rootVisualElement;

            // Act
            menuBuilder.Hide();
            yield return new WaitForSeconds(1f);

            // Assert
            Assert.AreEqual(expectedResults, root.style.display.ToString().Trim());
        }

        /// <summary>
        /// Checks if the OnPauseClicked event is triggered when the pause button is clicked
        /// </summary>
        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator PauseButtonClickTriggersOnPauseClickedEvent()
        {
            // Arrange
            bool wasClicked = false;
            bool expectedResult = true;
            VisualElement root = menuDocument.rootVisualElement;
            Button button = root.Q<Button>("Pause");
            menuBuilder.Show();
            menuBuilder.OnPauseClicked.AddListener(() => wasClicked = true);

            // Act
            var e = new NavigationSubmitEvent() { target = button };
            button.SendEvent(e);

            yield return null;

            // Assert
            Assert.AreEqual(expectedResult, wasClicked);
        }

        /// <summary>
        /// Checks if the OnInspectionReviewClicked event is triggered when the inspection review button is clicked
        /// </summary>
        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator InspectionReviewClickTriggersOnInspectionReviewClickedEvent()
        {
            // Arrange
            bool wasClicked = false;
            bool expectedResult = true;
            VisualElement root = menuDocument.rootVisualElement;
            Button button = root.Q<Button>("InspectionReview");

            menuBuilder.OnInspectionReviewClicked.AddListener(() => wasClicked = true);

            // Act
            var e = new NavigationSubmitEvent() { target = button };
            button.SendEvent(e);

            yield return null;

            // Assert
            Assert.AreEqual(expectedResult, wasClicked);
        }

        /// <summary>
        /// Checks if the OnSettingsClicked event is triggered when the settings button is clicked
        /// </summary>
        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator SettingsClickTriggersOnSettingsClickedEvent()
        {
            // Arrange
            bool wasClicked = false;
            bool expectedResult = true;
            VisualElement root = menuDocument.rootVisualElement;
            Button button = root.Q<Button>("Settings");

            menuBuilder.OnSettingsClicked.AddListener(() => wasClicked = true);

            // Act
            var e = new NavigationSubmitEvent() { target = button };
            button.SendEvent(e);

            yield return null;

            // Assert
            Assert.AreEqual(expectedResult, wasClicked);
        }

        /// <summary>
        /// Checks if none of the button events get triggered when the buttons as disabled
        /// </summary>
        [UnityTest, Order(6)]
        [Category("BuildServer")]
        public IEnumerator ButtonClickedIsNotTriggeredWhenButtonsAreDisabled()
        {
            // Arrange
            bool wasClicked = false;
            bool expectedResult = false;
            bool enabled = true;
            VisualElement root = menuDocument.rootVisualElement;
            Button pauseButton = root.Q<Button>("Pause");
            Button inspectionReviewButton = root.Q<Button>("InspectionReview");
            Button settingsButton = root.Q<Button>("Settings");

            menuBuilder.OnPauseClicked.AddListener(() => wasClicked = true);
            menuBuilder.OnInspectionReviewClicked.AddListener(() => wasClicked = true);
            menuBuilder.OnSettingsClicked.AddListener(() => wasClicked = true);

            // Act
            menuBuilder.ToggleButtons(!enabled);

            var pauseEvt = new NavigationSubmitEvent() { target = pauseButton };
            pauseButton.SendEvent(pauseEvt);

            var inspectionReviewEvt = new NavigationSubmitEvent() { target = inspectionReviewButton };
            inspectionReviewButton.SendEvent(inspectionReviewEvt);

            var settingsEvt = new NavigationSubmitEvent() { target = settingsButton };
            settingsButton.SendEvent(settingsEvt);

            yield return null;

            // Assert
            Assert.AreEqual(expectedResult, wasClicked);
        }
    }
}
