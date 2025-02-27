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
    /// <summary>
    ///     Play mode tests for the <see cref="InspectionWindowBuilder"/> class.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - Scene loading and component reference validation
    ///     - Photo capture functionality
    ///     - Window show/hide behavior
    ///     - Compliant/Non-compliant selection handling
    ///     - Inspection logging
    ///     - Notification display
    ///     - Inspection label updates
    /// </remarks>
    public class InspectionWindowIntegrationTest
    {
        #region Fields

        // Inspection Window
        private UIDocument inspectionWindowDoc;
        private InspectionWindowBuilder inspectionWindowBuilder;
        private VisualElement root;

        // Inspectable Object
        private InspectableObject inspectable;

        private const string SceneName = "InspectionWindowTestScene";

        //notification
        public UnityEvent<NotificationSO> DisplayNotification;

        #endregion

        #region Test Setup


        /// <summary>
        ///     Loads the <see cref="InspectionWindowTestScene"/>.
        /// </summary>
        [OneTimeSetUp]
        [Category("BuildServer")]
        public void RunOnce()
        {
            SceneManager.LoadScene(SceneName);
        }

        /// <summary>
        ///     Checks if test scene is loaded and gets component references from the loaded scene.
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

        #endregion

        #region Tests

        /// <summary>
        ///     Tests if the <see cref="InspectionWindowBuilder.OnPhotoTaken"/> event is invoked when the camera button is clicked.
        /// </summary>
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

        /// <summary>
        ///     Tests if the <see cref="InspectionWindowBuilder.OnWindowOpened"/> event is invoked when the window is displayed.
        /// </summary>
        [UnityTest, Order(2)]
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

        /// <summary>
        ///     Tests if the <see cref="InspectionWindowBuilder.OnWindowClosed"/> event is invoked when the window is hidden.
        /// </summary>
        [UnityTest, Order(3)]
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

        /// <summary>
        ///     Tests if the <see cref="InspectionWindowBuilder.OnCompliantSelected"/> event is invoked when the compliant button is clicked.
        /// </summary>
        [UnityTest, Order(4)]
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

        /// <summary>
        ///     Tests if the <see cref="InspectionWindowBuilder.OnNonCompliantSelected"/> event is invoked when the non-compliant button is clicked.
        /// </summary>
        [UnityTest, Order(5)]
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
        ///     Tests if a log is created and the <see cref="InspectionWindowBuilder.OnInspectionLog"/> event is invoked 
        ///     when compliant is selected.
        /// </summary>
        [UnityTest, Order(6)]
        [Category("BuildServer")]
        public IEnumerator OnInspectionLogInvokedWhenCompliantSelected()
        {
            // Arrange
            bool eventTriggered = false;
            InspectionData data = new InspectionData();
            Button button = root.Q<Button>("PositiveButton");
            inspectionWindowBuilder.OnInspectionLog.AddListener((inspectionData) =>
            {
                eventTriggered = true;
                data = inspectionData;

            });

            // Act
            var e = new NavigationSubmitEvent() { target = button };
            button.SendEvent(e);
            yield return null;

            // Assert
            Assert.IsTrue(eventTriggered);
        }

        /// <summary>
        ///     Tests if a log is created and the <see cref="InspectionWindowBuilder.OnInspectionLog"/> event is invoked 
        ///     when non-compliant is selected.
        /// </summary>
        [UnityTest, Order(7)]
        [Category("BuildServer")]
        public IEnumerator OnInspectionLogInvokedWhenNonCompliantSelected()
        {
            // Arrange
            bool eventTriggered = false;
            InspectionData data = new InspectionData();
            Button button = root.Q<Button>("NegativeButton");
            inspectionWindowBuilder.OnInspectionLog.AddListener((inspectionData) =>
            {
                eventTriggered = true;
                data = inspectionData;

            });

            inspectionWindowBuilder.HandleInspectionWindowDisplay(inspectable);

            // Act
            var e = new NavigationSubmitEvent() { target = button };
            button.SendEvent(e);
            yield return null;

            // Assert
            Assert.IsTrue(eventTriggered);

        }

        /// <summary>
        ///     Tests if a success notification appears when the Compliant button is clicked (without photo).
        /// </summary>
        [UnityTest, Order(8)]
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
        ///     Tests if an error notification appears when the Non-Compliant button is clicked.
        /// </summary>
        [UnityTest, Order(9)]
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
        ///     Tests if a success notification including photo information appears when the camera button is pressed 
        ///     before the compliant button is clicked.
        /// </summary>
        [UnityTest, Order(10)]
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
        ///     Tests if an error notification including photo information appears when the camera button is pressed 
        ///     before the non-compliant button is clicked.
        /// </summary>
        [UnityTest, Order(11)]
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

        /// <summary>
        ///     Tests if inspection label shows default message when no previous inspection exists.
        /// </summary>
        [UnityTest, Order(12)]
        [Category("BuildServer")]
        public IEnumerator InspectionWindow_UpdateInspectionLabel_ShowsDefaultMessageWhenNoData()
        {
            // Arrange
            InspectionData inspectionData = null;
            
            // Act
            inspectionWindowBuilder.UpdateInspectionLabel(inspectionData);
            yield return null;
            
            // Assert
            var label = inspectionWindowDoc.rootVisualElement.Q<Label>("BottomLabel");
            Assert.That(label.text, Is.EqualTo("Is this visual inspection compliant or non-compliant?"),
                "Inspection window should display default prompt message when no previous inspection data exists.");
        }

        /// <summary>
        ///     Tests if inspection label shows an appropriate message when the inspection is compliant without photo.
        /// </summary>
        [UnityTest, Order(13)]
        [Category("BuildServer")]
        public IEnumerator InspectionWindow_UpdateInspectionLabel_ShowsCompliantWithoutPhotoMessage()
        {
            // Arrange
            InspectionData inspectionData = new InspectionData(inspectable, true, false);
            
            // Act
            inspectionWindowBuilder.UpdateInspectionLabel(inspectionData);
            yield return null;
            
            // Assert
            var label = inspectionWindowDoc.rootVisualElement.Q<Label>("BottomLabel");
            Assert.That(label.text, Is.EqualTo("Visual inspection reported as compliant."),
                "Inspection window should display an appropriate message when the previous inspection reported as compliant without photo.");
        }

        /// <summary>
        ///     Tests if inspection label shows an appropriate message when the inspection is non-compliant with photo.
        /// </summary>
        [UnityTest, Order(14)]
        [Category("BuildServer")]
        public IEnumerator InspectionWindow_UpdateInspectionLabel_ShowsNonCompliantWithPhotoMessage()
        {
            // Arrange
            InspectionData inspectionData = new InspectionData(inspectable, false, true);
            
            // Act
            inspectionWindowBuilder.UpdateInspectionLabel(inspectionData);
            yield return null;
            
            // Assert
            var label = inspectionWindowDoc.rootVisualElement.Q<Label>("BottomLabel");
            Assert.That(label.text, Is.EqualTo("Visual inspection with photo reported as non-compliant."),
                "Inspection window should display an appropriate message when the previous inspection reported as non-compliant with photo.");
        }

        #endregion
    }
}

