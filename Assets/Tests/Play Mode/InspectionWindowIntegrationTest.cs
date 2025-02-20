using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.DLX;

namespace Tests.PlayMode
{
    public class InspectionWindowIntegrationTest
    {
        // Inspection Window
        private UIDocument inspectionWindowDoc;
        private InspectionWindowBuilder inspectionWindowBuilder;
        private VisualElement root;

        // Inspectable Object
        private InspectableObject inspectable;

        private const string SceneName = "InspectionWindowTestScene";

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

            inspectionWindowBuilder = GameObject.FindAnyObjectByType<InspectionWindowBuilder>();
            inspectionWindowDoc = inspectionWindowBuilder.GetComponent<UIDocument>();
            inspectable = GameObject.FindAnyObjectByType<InspectableObject>();

            inspectionWindowBuilder.CurrentInspectable = inspectable;
            root = inspectionWindowDoc.rootVisualElement;

            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
        }

        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator OnPhotoTakenIsInvokedWhenCameraButtonIsClicked()
        {
            // Arrange
            bool wasClicked = false;
            Button button = root.Q<Button>("CameraButton");
            inspectionWindowBuilder.HandleInspectionWindowDisplay(inspectable);

            // Act
            inspectionWindowBuilder.OnPhotoTaken.AddListener((inspectable) => wasClicked = true);
            var e = new NavigationSubmitEvent() { target = button };
            button.SendEvent(e);

            yield return null;
            // Assert
            Assert.IsTrue(wasClicked);
        }

        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator OnWindowOpenedIsInvokedWhenWindowIsDisplayed()
        {
            // Arrange
            bool wasClicked = false;
            inspectionWindowBuilder.Hide();

            // Act
            inspectionWindowBuilder.OnWindowOpened.AddListener((inspectable) => wasClicked = true);
            inspectionWindowBuilder.HandleInspectionWindowDisplay(inspectable);

            yield return null;
            // Assert
            Assert.IsTrue(wasClicked);
        }

        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator OnWindowClosedIsInvokedWhenWindowIsHidden()
        {
            // Arrange
            bool wasClicked = false;

            // Act
            inspectionWindowBuilder.OnWindowClosed.AddListener((inspectable) => wasClicked = true);
            inspectionWindowBuilder.Hide();

            yield return null;
            // Assert
            Assert.IsTrue(wasClicked);
        }

        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator OnCompliantSelectIsInvokedWhenCompliantButtonIsClicked()
        {
            // Arrange
            bool wasClicked = false;
            Button button = root.Q<Button>("PositiveButton");
            inspectionWindowBuilder.HandleInspectionWindowDisplay(inspectable);

            // Act
            inspectionWindowBuilder.OnCompliantSelected.AddListener((inspectable) => wasClicked = true);
            var e = new NavigationSubmitEvent() { target = button };
            button.SendEvent(e);

            yield return null;
            // Assert
            Assert.IsTrue(wasClicked);
        }

        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator OnNonCompliantSelectIsInvokedWhenNonCompliantButtonIsClicked()
        {
            // Arrange
            bool wasClicked = false;
            Button button = root.Q<Button>("NegativeButton");
            inspectionWindowBuilder.HandleInspectionWindowDisplay(inspectable);

            // Act
            inspectionWindowBuilder.OnNonCompliantSelected.AddListener((inspectable) => wasClicked = true);
            var e = new NavigationSubmitEvent() { target = button };
            button.SendEvent(e);

            yield return null;
            // Assert
            Assert.IsTrue(wasClicked);
        }
    }
}
