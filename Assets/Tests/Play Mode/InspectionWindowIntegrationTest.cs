using NUnit.Framework;
using System.Collections;
using UnityEditor;
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

        // Inspection Window Notification
        private InspectionWindowNotification notification;

        private VisualElement notificationContainer;

        private Sprite successIcon;
        private Sprite errorIcon;
        private Sprite infoIcon;
        private Sprite customIcon;

        

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

            //Load required assets from project files
            successIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/VELCRO UI/Sprites/Icons/Success_Sprite.png");
            errorIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/VELCRO UI/Sprites/Icons/Warning_Sprite.png");
            infoIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/VELCRO UI/Sprites/Icons/Info_Sprite.png");
            customIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/VELCRO UI/Sprites/Checkmarks/Checkmark_Sprite.png");

            notification = inspectionWindowBuilder.GetComponent<InspectionWindowNotification>();

            //Reference icons as SerializedFields
            SerializedObject so2 = new SerializedObject(notification);
            so2.FindProperty("successIcon").objectReferenceValue = successIcon;
            so2.FindProperty("errorIcon").objectReferenceValue = errorIcon;
            so2.FindProperty("infoIcon").objectReferenceValue = infoIcon;
            so2.FindProperty("customIcon").objectReferenceValue = customIcon;
            so2.ApplyModifiedProperties();

            notificationContainer = root.Q<VisualElement>("NotificationContainer");

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

        /// <summary>
        ///Test the Flash Effect
        /// </summary>
        [UnityTest, Order(15)]
        [Category("BuildServer")]
        public IEnumerator HasFlashCoroutine()
        {
            // Arrange
            var flashContainer = root.Q<VisualElement>("FlashContainer");

            // Act
            inspectionWindowBuilder.TakePhoto();
            yield return null;

            // Assert if the flash effect was triggered by adding the class
            bool flashAdded = flashContainer.ClassListContains("card-body-flash");
            Assert.IsTrue(flashAdded, "Flash effect was not triggered on the flash container.");
        }

        /// <summary>
        ///Test the photo frame
        /// </summary>
        [UnityTest, Order(16)]
        [Category("BuildServer")]
        public IEnumerator HasPhotoFrame()
        {
            // Arrange
            var flashContainer = root.Q<VisualElement>("FlashContainer");

            // Act
            inspectionWindowBuilder.TakePhoto(); // Call the method that triggers the photo frame
            yield return null;

            // Assert
            bool photoFrameAdded = flashContainer.ClassListContains("card-body-photo-frame");
            Assert.IsTrue(photoFrameAdded, "Photo frame was not added to the flash container.");

        }

        /// <summary>
        ///     Tests if correct confirmation dialog is shown when changing from compliant to non-compliant.
        /// </summary>
        [UnityTest, Order(17)]
        [Category("BuildServer")]
        public IEnumerator InspectionWindow_CompliantToNonCompliant_ShowsConfirmationDialog()
        {
            // Arrange
            ConfirmationDialogSO shownDialog = null;
            inspectionWindowBuilder.OnShowConfirmationDialog.AddListener((dialog) => shownDialog = dialog);

            yield return SetupPreviousInspection(isCompliant: true);

            // Act
            var nonCompliantButton = root.Q<Button>("NegativeButton");
            var e = new NavigationSubmitEvent() { target = nonCompliantButton };
            nonCompliantButton.SendEvent(e);
            yield return null;

            // Assert
            Assert.IsNotNull(shownDialog, "Confirmation dialog was not shown when changing from compliant to non-compliant");
            Assert.AreEqual(inspectionWindowBuilder.NonCompliantConfirmationDialog, shownDialog,
                "Incorrect confirmation dialog was shown when changing from compliant to non-compliant");
        }

        /// <summary>
        ///     Tests if correct confirmation dialog is shown when changing from non-compliant to compliant.
        /// </summary>
        [UnityTest, Order(18)]
        [Category("BuildServer")]
        public IEnumerator InspectionWindow_NonCompliantToCompliant_ShowsConfirmationDialog()
        {
            // Arrange
            ConfirmationDialogSO shownDialog = null;
            inspectionWindowBuilder.OnShowConfirmationDialog.AddListener((dialog) => shownDialog = dialog);

            yield return SetupPreviousInspection(isCompliant: false);

            // Act
            var compliantButton = root.Q<Button>("PositiveButton");
            var e = new NavigationSubmitEvent() { target = compliantButton };
            compliantButton.SendEvent(e);
            yield return null;

            // Assert
            Assert.IsNotNull(shownDialog, "Confirmation dialog was not shown when changing from non-compliant to compliant");
            Assert.AreEqual(inspectionWindowBuilder.CompliantConfirmationDialog, shownDialog,
                "Incorrect confirmation dialog was shown when changing from non-compliant to compliant");
        }

        /// <summary>
        ///     Tests if notification is shown with correct message when attempting to mark as compliant when already compliant.
        /// </summary>
        [UnityTest, Order(19)]
        [Category("BuildServer")]
        public IEnumerator InspectionWindow_AlreadyCompliant_ShowsNotification()
        {
            // Arrange
            NotificationSO shownNotification = null;
            inspectionWindowBuilder.DisplayNotification.AddListener((notification) => shownNotification = notification);

            yield return SetupPreviousInspection(isCompliant: true, hasPhoto: true);

            // Act
            var compliantButton = root.Q<Button>("PositiveButton");
            var e = new NavigationSubmitEvent() { target = compliantButton };
            compliantButton.SendEvent(e);
            yield return null;

            // Assert
            Assert.IsNotNull(shownNotification, "Notification was not shown");
            Assert.AreEqual("You have already reported this as compliant.", shownNotification.Message,
                "Incorrect notification message was shown");
            Assert.AreEqual(NotificationType.Info, shownNotification.NotificationType,
                "Incorrect notification type was used");
        }

        /// <summary>
        ///     Tests if notification is shown with correct message when attempting to mark as non-compliant when already non-compliant.
        /// </summary>
        [UnityTest, Order(20)]
        [Category("BuildServer")]
        public IEnumerator InspectionWindow_AlreadyNonCompliant_ShowsNotification()
        {
            // Arrange
            NotificationSO shownNotification = null;
            inspectionWindowBuilder.DisplayNotification.AddListener((notification) => shownNotification = notification);

            yield return SetupPreviousInspection(isCompliant: false, hasPhoto: true);

            // Act
            var nonCompliantButton = root.Q<Button>("NegativeButton");
            var e = new NavigationSubmitEvent() { target = nonCompliantButton };
            nonCompliantButton.SendEvent(e);
            yield return null;

            // Assert
            Assert.IsNotNull(shownNotification, "Notification was not shown");
            Assert.AreEqual("You have already reported this as non-compliant.", shownNotification.Message,
                "Incorrect notification message was shown");
            Assert.AreEqual(NotificationType.Info, shownNotification.NotificationType,
                "Incorrect notification type was used");
        }

        /// <summary>
        ///     Tests if confirmation dialog primary button click saves inspection and closes window.
        /// </summary>
        [UnityTest, Order(21)]
        [Category("BuildServer")]
        public IEnumerator InspectionWindow_ConfirmationDialogPrimaryButton_SavesAndClosesWindow()
        {
            // Arrange
            bool isInspectionSaved = false;
            bool isWindowClosed = false;
            inspectionWindowBuilder.OnInspectionLog.AddListener((data) => isInspectionSaved = true);
            inspectionWindowBuilder.OnWindowClosed.AddListener((obj) => isWindowClosed = true);

            yield return SetupPreviousInspection(isCompliant: true);

            // Trigger confirmation dialog
            var nonCompliantButton = root.Q<Button>("NegativeButton");
            var e = new NavigationSubmitEvent() { target = nonCompliantButton };
            nonCompliantButton.SendEvent(e);
            yield return null;

            // Act
            inspectionWindowBuilder.OnConfirmationDialogPrimaryClicked();
            yield return null;

            // Assert
            Assert.IsTrue(isInspectionSaved, "Inspection was not saved after confirmation dialog primary button click");
            Assert.IsTrue(isWindowClosed, "Window was not closed after confirmation dialog primary button click");
        }

        [UnityTest, Order(22)]
        [Category("BuildServer")]
        public IEnumerator HandleDisplayUI_WithSO_SetsRootToDisplayFlex()
        {
            //Arrange
            StyleEnum<DisplayStyle> expectedStyle = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            NotificationSO notificationSO = ScriptableObject.CreateInstance<NotificationSO>();
            notificationSO.NotificationType = NotificationType.Success;
            notificationSO.Alignment = Align.FlexStart;
            notificationSO.FontSize = FontSize.Medium;
            notificationSO.Message = "Test notification";

            //Act
            notification.HandleDisplayUI(notificationSO);
            yield return null;

            //Assert
            Assert.AreEqual(expectedStyle, notificationContainer.style.display);
        }

        [Test, Order(23)]
        [Category("BuildServer")]
        public void SetContent_WithSuccessType_ShouldPopulateNotification()
        {
            //Arrange
            StyleBackground expectedIcon = new StyleBackground(successIcon);
            string expectedMessage = "Test Message";

            //Act
            notification.SetContent(NotificationType.Success, "Test Message");

            //Assert
            Assert.AreEqual(expectedIcon, notificationContainer.Q<VisualElement>("Icon").style.backgroundImage);
            Assert.AreEqual(expectedMessage, notificationContainer.Q<Label>("Text").text);
        }

        [Test, Order(24)]
        [Category("BuildServer")]
        public void SetContent_WithInfoType_ShouldPopulateNotification()
        {
            //Arrange
            StyleBackground expectedIcon = new StyleBackground(infoIcon);
            string expectedMessage = "Test Message";

            //Act
            notification.SetContent(NotificationType.Info, "Test Message");

            //Assert
            Assert.AreEqual(expectedIcon, notificationContainer.Q<VisualElement>("Icon").style.backgroundImage);
            Assert.AreEqual(expectedMessage, notificationContainer.Q<Label>("Text").text);
        }

        [Test, Order(25)]
        [Category("BuildServer")]
        public void SetContent_WithErrorType_ShouldPopulateNotification()
        {
            //Arrange
            StyleBackground expectedIcon = new StyleBackground(errorIcon);
            string expectedMessage = "Test Message";

            //Act
            notification.SetContent(NotificationType.Error, "Test Message");

            //Assert
            Assert.AreEqual(expectedIcon, notificationContainer.Q<VisualElement>("Icon").style.backgroundImage);
            Assert.AreEqual(expectedMessage, notificationContainer.Q<Label>("Text").text);
        }

        [UnityTest, Order(26)]
        [Category("BuildServer")]
        public IEnumerator FadeIn_ShouldSetOpacityToOne()
        {
            //Arrange
            float expectedOpacity = 1.0f;

            //Act
            notification.FadeIn();
            yield return new WaitForSeconds(1.5f);

            //Assert
            Assert.AreEqual(expectedOpacity, notificationContainer.resolvedStyle.opacity);
        }

        [Test, Order(27)]
        [Category("BuildServer")]
        public void Show_Triggers_OnNotificationOpened()
        {
            bool triggered = false;
            //Arrange
            notification.OnNotificationShown.AddListener(() => triggered = true);

            //Act
            notification.Show();

            //Assert
            Assert.IsTrue(triggered);
        }

        [Test, Order(28)]
        [Category("BuildServer")]
        public void Hide_Triggers_OnNotificationClosed()
        {
            //Arrange
            bool triggered = false;
            notification.OnNotificationHidden.AddListener(() => triggered = true);

            //Act
            notification.Hide();

            //Assert
            Assert.IsTrue(triggered);
        }

        [Test, Order(29)]
        [Category("BuildServer")]
        public void SetContent_WithCustomType_ShouldPopulateNotification()
        {
            //Arrange
            StyleBackground expectedIcon = new StyleBackground(customIcon);
            string expectedMessage = "Test Message";

            //Act
            notification.SetContent(NotificationType.Custom, "Test Message");

            //Assert
            Assert.AreEqual(expectedIcon, notificationContainer.Q<VisualElement>("Icon").style.backgroundImage);
            Assert.AreEqual(expectedMessage, notificationContainer.Q<Label>("Text").text);
        }

        [Test, Order(30)]
        [Category("BuildServer")]
        public void SetContent_WithCustomType_ShouldSetInlineValues()
        {
            //Act
            notification.SetContent(NotificationType.Custom, "Test Message");

            //Assert
            Assert.AreNotEqual(StyleKeyword.Null, notificationContainer.Q<VisualElement>("Icon").style.backgroundImage);
            Assert.AreNotEqual(StyleKeyword.Null, notificationContainer.Q<VisualElement>("NotificationContainer").style.backgroundColor);
            Assert.AreNotEqual(StyleKeyword.Null, notificationContainer.Q<Label>("Text").style.color);
        }

        [Test, Order(31)]
        [Category("BuildServer")]
        public void SetCustomNotification_WithIcon_SetsValues()
        {
            // Arrange
            StyleBackground expectedIcon = new StyleBackground(customIcon);
            StyleColor expectedBackgroundColour = new StyleColor(Color.red);
            StyleColor expectedTextColour = new StyleColor(Color.green);

            //Act
            notification.SetCustomNotification(Color.red, Color.green, customIcon);
            notification.HandleDisplayUI(NotificationType.Custom, "Test Message");

            //Assert
            Assert.AreEqual(expectedIcon, notificationContainer.Q<VisualElement>("Icon").style.backgroundImage);
            Assert.AreEqual(expectedBackgroundColour, notificationContainer.Q<VisualElement>("NotificationContainer").style.backgroundColor);
            Assert.AreEqual(expectedTextColour, notificationContainer.Q<Label>("Text").style.color);
        }

        [Test, Order(32)]
        [Category("BuildServer")]
        public void SetCustomNotification_WithoutIcon_SetsValues()
        {
            // Arrange
            StyleKeyword expectedStyle = StyleKeyword.Null;
            StyleColor expectedBackgroundColour = new StyleColor(Color.red);
            StyleColor expectedTextColour = new StyleColor(Color.green);

            //Act
            notification.SetCustomNotification(Color.red, Color.green);
            notification.HandleDisplayUI(NotificationType.Custom, "Test Message");

            //Assert
            Assert.AreEqual(expectedStyle.ToString(), notificationContainer.Q<VisualElement>("Icon").style.backgroundImage.ToString());
            Assert.AreEqual(expectedBackgroundColour, notificationContainer.Q<VisualElement>("NotificationContainer").style.backgroundColor);
            Assert.AreEqual(expectedTextColour, notificationContainer.Q<Label>("Text").style.color);
        }

        [Test, Order(33)]
        [Category("BuildServer")]
        public void ClearClasses_WithSuccess_RemovesClass()
        {
            // Arrange
            string disabledClass = "notification-success";

            //Act
            notification.HandleDisplayUI(NotificationType.Success, "Success Message");
            notification.ClearClasses();

            //Assert
            Assert.IsFalse(notificationContainer.Q<VisualElement>("NotificationContainer").ClassListContains(disabledClass));
        }

        [Test, Order(34)]
        [Category("BuildServer")]
        public void ClearClasses_WithInfo_RemovesClass()
        {
            // Arrange
            string disabledClass = "notification-info";

            //Act
            notification.HandleDisplayUI(NotificationType.Info, "Info Message");
            notification.ClearClasses();

            //Assert
            Assert.IsFalse(notificationContainer.Q<VisualElement>("NotificationContainer").ClassListContains(disabledClass));
        }

        [Test, Order(35)]
        [Category("BuildServer")]
        public void ClearClasses_WithError_RemovesClass()
        {
            // Arrange
            string disabledClass = "notification-error";

            //Act
            notification.HandleDisplayUI(NotificationType.Error, "Error Message");
            notification.ClearClasses();

            //Assert
            Assert.IsFalse(notificationContainer.Q<VisualElement>("NotificationContainer").ClassListContains(disabledClass));
        }
        #endregion

        #region Helper Methods

        /// <summary>
        ///     Sets up a previous inspection with the specified compliance status.
        /// </summary>
        /// <param name="isCompliant">Whether the previous inspection should be marked as compliant.</param>
        /// <param name="hasPhoto">Whether the previous inspection includes a photo.</param>
        /// <returns>IEnumerator for test coroutine.</returns>
        private IEnumerator SetupPreviousInspection(bool isCompliant, bool hasPhoto = false)
        {
            var inspection = new InspectionData(inspectable, isCompliant, hasPhoto);
            inspectionWindowBuilder.UpdateInspectionLabel(inspection);
            yield return null;
        }

        #endregion
    }
}