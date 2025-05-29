using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.DLX;

namespace Tests.PlayMode
{
    public class DownloadConfirmationTests
    {
        private GameObject confirmationObject;
        private UIDocument confirmationDocument;
        private DownloadConfirmationBuilder confirmationBuilder;

        private const string SceneName = "DownloadConfirmationTestScene";

        /// <summary>
        /// Loads the download confirmation test scene
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

            confirmationBuilder = GameObject.FindAnyObjectByType<DownloadConfirmationBuilder>();
            confirmationDocument = confirmationBuilder.GetComponent<UIDocument>();

            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
        }

        /// <summary>
        /// Checks if the UI is visible when Show is called
        /// </summary>
        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator CheckIfUIIsEnabledOnShow()
        {
            // Arrange
            var expectedResults = DisplayStyle.Flex.ToString().Trim();
            VisualElement root = confirmationDocument.rootVisualElement;

            // Act
            confirmationBuilder.Hide();
            yield return new WaitForSeconds(1f);
            confirmationBuilder.Show();
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
            VisualElement root = confirmationDocument.rootVisualElement;

            // Act
            confirmationBuilder.Hide();
            yield return new WaitForSeconds(1f);

            // Assert
            Assert.AreEqual(expectedResults, root.style.display.ToString().Trim());
        }

        /// <summary>
        /// Checks if the OnHide event is triggered when the primary button is clicked
        /// </summary>
        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator PrimaryButtonClickTriggersHide()
        {
            // Arrange
            bool wasHidden = false;
            bool expectedResult = true;
            VisualElement root = confirmationDocument.rootVisualElement;
            Button button = root.Q<Button>("Button");
            
            confirmationBuilder.Show();
            yield return new WaitForSeconds(0.5f);
            
            confirmationBuilder.OnHide.AddListener(() => wasHidden = true);

            // Act
            var e = new NavigationSubmitEvent() { target = button };
            button.SendEvent(e);

            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.AreEqual(expectedResult, wasHidden);
        }

        /// <summary>
        /// Checks if the OnShow event is triggered when Show is called
        /// </summary>
        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator ShowTriggersOnShowEvent()
        {
            // Arrange
            bool wasShown = false;
            bool expectedResult = true;
            
            confirmationBuilder.Hide();
            yield return new WaitForSeconds(0.5f);
            
            confirmationBuilder.OnShow.AddListener(() => wasShown = true);

            // Act
            confirmationBuilder.Show();
            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.AreEqual(expectedResult, wasShown);
            
            // Cleanup
            confirmationBuilder.OnShow.RemoveAllListeners();
        }

        /// <summary>
        /// Checks if the window is hidden after clicking the primary button
        /// </summary>
        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator WindowHidesAfterPrimaryButtonClick()
        {
            // Arrange
            var expectedResults = DisplayStyle.None.ToString().Trim();
            VisualElement root = confirmationDocument.rootVisualElement;
            Button button = root.Q<Button>("Button");
            
            confirmationBuilder.Show();
            yield return new WaitForSeconds(0.5f);

            // Act
            var e = new NavigationSubmitEvent() { target = button };
            button.SendEvent(e);
            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.AreEqual(expectedResults, root.style.display.ToString().Trim());
        }
    }
}
