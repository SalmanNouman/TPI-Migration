using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.DLX;
using VARLab.Velcro;

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

        //notification
        public UnityEvent<NotificationSO> DisplayNotification;

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

        /// <summary>
        /// Tests if a success notification appears when the Compliant button is clicked.
        /// </summary>
        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator CompliantButtonDisplaysNotification()
        {
            // Arrange
            NotificationSO displayedNotification = null;
            Button compliantButton = root.Q<Button>("PositiveButton");
            inspectionWindowBuilder.HandleInspectionWindowDisplay(inspectable);

            inspectionWindowBuilder.DisplayNotification.AddListener((notification) => displayedNotification = notification);

            // Act
            var clickEvent = new NavigationSubmitEvent { target = compliantButton };
            compliantButton.SendEvent(clickEvent);

            yield return null;

            // Assert
            Assert.IsNotNull(displayedNotification, "No notification was displayed when the Compliant button was clicked.");
            Assert.AreEqual(NotificationType.Info, displayedNotification.NotificationType, "Notification type is incorrect.");
            Assert.IsTrue(displayedNotification.Message.Contains("compliant"), "Notification message is incorrect.");
        }

        /// <summary>
        /// Tests if an error notification appears when the Non-Compliant button is clicked.
        /// </summary>
        [UnityTest, Order(6)]
        [Category("BuildServer")]
        public IEnumerator NonCompliantButtonDisplaysNotification()
        {
            // Arrange
            NotificationSO displayedNotification = null;
            Button nonCompliantButton = root.Q<Button>("NegativeButton");
            inspectionWindowBuilder.HandleInspectionWindowDisplay(inspectable);

            inspectionWindowBuilder.DisplayNotification.AddListener((notification) => displayedNotification = notification);

            // Act
            var clickEvent = new NavigationSubmitEvent { target = nonCompliantButton };
            nonCompliantButton.SendEvent(clickEvent);

            yield return null;

            // Assert
            Assert.IsNotNull(displayedNotification, "No notification was displayed when the Non-Compliant button was clicked.");
            Assert.AreEqual(NotificationType.Info, displayedNotification.NotificationType, "Notification type is incorrect.");
            Assert.IsTrue(displayedNotification.Message.Contains("non-compliant"), "Notification message is incorrect.");
        }

        /// <summary>
        /// Tests if a success notification including photo information appears when the camera button is pressed 
        /// before the compliant button is clicked.
        /// </summary>
        [UnityTest, Order(7)]
        [Category("BuildServer")]
        public IEnumerator CompliantButtonDisplaysNotification_WithPhoto()
        {
            // Arrange
            NotificationSO displayedNotification = null;
            Button cameraButton = root.Q<Button>("CameraButton");
            Button compliantButton = root.Q<Button>("PositiveButton");
            inspectionWindowBuilder.HandleInspectionWindowDisplay(inspectable);

            // Simulate pressing the camera button to mark that a photo was taken
            var cameraClickEvent = new NavigationSubmitEvent { target = cameraButton };
            cameraButton.SendEvent(cameraClickEvent);

            // Subscribe to the DisplayNotification event
            inspectionWindowBuilder.DisplayNotification.AddListener((notification) =>
            {
                displayedNotification = notification;
            });

            // Act
            var compliantClickEvent = new NavigationSubmitEvent { target = compliantButton };
            compliantButton.SendEvent(compliantClickEvent);

            yield return null;

            // Assert
            Assert.IsNotNull(displayedNotification, "No notification was displayed when the Compliant button was clicked with photo.");
            Assert.AreEqual(NotificationType.Info, displayedNotification.NotificationType, "Notification type is incorrect.");
            StringAssert.Contains("with photo", displayedNotification.Message, "Notification message should indicate that a photo was taken.");
        }

        /// <summary>
        /// Tests if an error notification including photo information appears when the camera button is pressed 
        /// before the non-compliant button is clicked.
        /// </summary>
        [UnityTest, Order(8)]
        [Category("BuildServer")]
        public IEnumerator NonCompliantButtonDisplaysNotification_WithPhoto()
        {
            // Arrange
            NotificationSO displayedNotification = null;
            Button cameraButton = root.Q<Button>("CameraButton");
            Button nonCompliantButton = root.Q<Button>("NegativeButton");
            inspectionWindowBuilder.HandleInspectionWindowDisplay(inspectable);

            // Simulate pressing the camera button to mark that a photo was taken
            var cameraClickEvent = new NavigationSubmitEvent { target = cameraButton };
            cameraButton.SendEvent(cameraClickEvent);

            // Subscribe to the DisplayNotification event
            inspectionWindowBuilder.DisplayNotification.AddListener((notification) =>
            {
                displayedNotification = notification;
            });

            // Act
            var nonCompliantClickEvent = new NavigationSubmitEvent { target = nonCompliantButton };
            nonCompliantButton.SendEvent(nonCompliantClickEvent);

            yield return null;

            // Assert
            Assert.IsNotNull(displayedNotification, "No notification was displayed when the Non-Compliant button was clicked with photo.");
            Assert.AreEqual(NotificationType.Info, displayedNotification.NotificationType, "Notification type is incorrect.");
            StringAssert.Contains("with photo", displayedNotification.Message, "Notification message should indicate that a photo was taken.");
        }
    }
}

