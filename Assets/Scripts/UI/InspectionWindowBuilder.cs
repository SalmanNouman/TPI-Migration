using Kamgam.UIToolkitWorldImage;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Interactions;
using VARLab.Navigation.PointClick;
using VARLab.ObjectViewer;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///     Manages the inspection window UI and its interactions.
    ///     This includes displaying InspectableObject's Information, handling photo captures,
    ///     and managing inspection data updates.
    /// </summary>
    /// <remarks>
    ///     Provides photo capture functionality and integrates with object viewer system.
    ///     Uses event-based communication for inspection data updates.
    /// </remarks>
    public class InspectionWindowBuilder : MonoBehaviour, IUserInterface
    {
        #region Fields

        // Visual Elements
        private VisualElement root;
        private VisualElement imageViewer3d;
        private VisualElement imageViewer;
        private VisualElement flashContainer;

        // Labels
        private Label locationLabel;
        private Label objectNameLabel;
        private Label inspectionLabel;

        // Buttons
        private Button closeButton;
        private Button cameraButton;
        private Button compliantButton;
        private Button nonCompliantButton;

        private WorldObjectRenderer worldObjectRenderer;

        // Inspectable object that is currently displayed in the inspection window.
        [HideInInspector]
        public InspectableObject CurrentInspectable;

        // Stores the previous inspection data for the current inspectable object.
        private InspectionData previousInspectionData;

        // Image 
        private Texture2D texture;
        private const int ImageHeight = 580;
        private const int ImageWidth = 770;

        // Flag to track if the camera button was pressed.
        private bool photoTaken = false;

        // Notification instance to be updated and reused.
        private NotificationSO notification;

        #endregion

        #region Events

        [Header("Unity Events")]
        /// <summary>
        ///     Unity Event that is invoked when the inspection window is displayed.
        ///     <see cref="InteractionHandler.enabled(false)"/>
        ///     <see cref="PointClickNavigation.EnableCameraPanAndZoom(false)"/>
        ///     <see cref="PointClickNavigation.EnableNavigation(false)"/>
        ///     <see cref="MenuBuilder.Hide"/>
        /// </summary>
        public UnityEvent<InspectableObject> OnWindowOpened;

        /// <summary>
        ///     Unity Event that is triggered when the inspection window is closed.
        ///     <see cref="InteractionHandler.enabled(true)"/>
        ///     <see cref="PointClickNavigation.EnableCameraPanAndZoom(true)"/>
        ///     <see cref="PointClickNavigation.EnableNavigation(true)"/>
        ///     <see cref="MenuBuilder.Show"/>
        /// </summary>
        public UnityEvent<InspectableObject> OnWindowClosed;

        /// <summary>
        ///     Unity Event that is triggered when the camera button is clicked.
        /// </summary>
        public UnityEvent<InspectableObject> OnPhotoTaken;

        /// <summary>
        ///     Unity Event that is triggered when the compliant button is clicked.
        /// </summary>
        public UnityEvent<InspectableObject> OnCompliantSelected;

        /// <summary>
        ///     Unity Event that is triggered when the non-compliant button is clicked.
        /// </summary>
        public UnityEvent<InspectableObject> OnNonCompliantSelected;

        /// <summary>
        ///     This only gets invoke if the inspectable object that is displayed in the inspection
        ///     window uses object viewer to reset the object position.
        ///     <see cref="ObjectViewerController.MoveBack"/>
        /// </summary>
        public UnityEvent<GameObject> OnObjectViewerClosed;

        /// <summary>
        ///     Unity Event that is triggered when an inspection log is recorded.
        ///     <see cref="Inspections.AddInspection(InspectionData)"/>
        /// </summary>
        public UnityEvent<InspectionData> OnInspectionLog;

        /// <summary>
        /// Invoked when a photo is taken and compliant or non-compliant is selected.
        /// Returns: The inspectable object that was inspected.
        /// <see cref="ImageHandler.TakePhoto"/>
        /// </summary>
        public UnityEvent<InspectableObject> PhotoConfirmed;

        [Header("Notification Event"), Space(10f)]
        [Tooltip("Invoked to display a notification. The event takes a NotificationSO as its parameter.")]
        public UnityEvent<NotificationSO> DisplayNotification;

        #endregion

        #region Methods

        private void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            OnWindowOpened ??= new();
            OnWindowClosed ??= new();
            OnPhotoTaken ??= new();
            OnCompliantSelected ??= new();
            OnNonCompliantSelected ??= new();
            DisplayNotification ??= new UnityEvent<NotificationSO>();

            worldObjectRenderer = GetComponent<WorldObjectRenderer>();

            // Create a new instance of NotificationSO to be reused
            notification = ScriptableObject.CreateInstance<NotificationSO>();
        }

        private void Start()
        {
            GetAllReferences();
            AddButtonListeners();
            Hide();
        }

        /// <summary>
        ///     Get the reference of all visual elements that are required to build the inspection window.
        /// </summary>
        private void GetAllReferences()
        {
            // Visual Elements
            imageViewer3d = root.Q<VisualElement>("3DViewer");
            imageViewer = root.Q<VisualElement>("Image");
            flashContainer = root.Q<VisualElement>("FlashContainer");

            // Labels
            locationLabel = root.Q<Label>("Secondary");
            objectNameLabel = root.Q<Label>("Primary");
            inspectionLabel = root.Q<Label>("BottomLabel");

            // Buttons
            closeButton = root.Q<TemplateContainer>("Button-Close").Q<Button>("CloseBtn");
            cameraButton = root.Q<Button>("CameraButton");
            compliantButton = root.Q<Button>("PositiveButton");
            nonCompliantButton = root.Q<Button>("NegativeButton");
        }

        /// <summary>
        ///     Add the listeners to all the button in the inspection window.
        /// </summary>
        private void AddButtonListeners()
        {
            closeButton.clicked += () =>
            {
                Debug.Log("Close button clicked");
                Hide();
                OnWindowClosed?.Invoke(CurrentInspectable);
            };

            cameraButton.clicked += TakePhoto;

            compliantButton.clicked += CompliantSelected;
            nonCompliantButton.clicked += NonCompliantSelected;
        }

        /// <summary>
        ///     Method that is called when the camera button is clicked.
        ///     This invokes the OnPhotoTaken event.
        /// </summary>
        public void TakePhoto()
        {
            // TODO: display flash and camera outline
            flashContainer.AddToClassList("card-body-photo-frame");
            photoTaken = true;
            StartCoroutine(FlashCoroutine());

            OnPhotoTaken?.Invoke(CurrentInspectable);
        }

        /// <summary>
        /// This method handles the flash effect
        /// </summary>
        private IEnumerator FlashCoroutine()
        {
            flashContainer.AddToClassList("card-body-flash");
            yield return new WaitForSeconds(0.2f);
            flashContainer.RemoveFromClassList("card-body-flash");
        }

        /// <summary>
        ///     Sets up the notification based on whether the inspection is compliant or non-compliant.
        /// </summary>
        /// <param name="compliant">If true, the inspection is compliant; otherwise, it is non-compliant.</param>
        private void SetUpNotification(bool compliant)
        {
            // Determine compliancy text
            string compliancy = compliant ? "compliant" : "non-compliant";
            string message;

            if (photoTaken)
            {
                message = $"Visual inspection with photo of {CurrentInspectable.Name} reported as {compliancy}.";
            }
            else
            {
                message = $"Visual inspection of {CurrentInspectable.Name} reported as {compliancy}.";
            }

            // Update the notification fields
            notification.NotificationType = NotificationType.Info;
            notification.Alignment = Align.FlexStart;
            notification.FontSize = FontSize.Medium;
            notification.Message = message;
        }


        /// <summary>
        ///     This method is called when the compliant button is clicked.
        ///     Invokes the OnCompliantSelected event.
        /// </summary>
        private void CompliantSelected()
        {
            // TODO: Check if this object already has an inspection.
            // If true and value changed display modal.
            // If false and value is the same display toast.

            if (!CurrentInspectable.HasPhoto)
            {
                ConfirmPhoto();
            };

            OnInspectionLog?.Invoke(new InspectionData(CurrentInspectable, true, photoTaken));
            OnCompliantSelected?.Invoke(CurrentInspectable);

            SetUpNotification(true);
            DisplayNotification?.Invoke(notification);
            Hide();
        }

        /// <summary>
        ///     This method is called when the non-compliant button is clicked.
        ///     Invokes the OnNonCompliantSelected event.
        /// </summary>
        private void NonCompliantSelected()
        {
            // TODO: Check if this object already has an inspection.
            // If true and value changed display modal.
            // If false and value is the same display toast.

            if (!CurrentInspectable.HasPhoto)
            {
                ConfirmPhoto();
            }

            OnInspectionLog?.Invoke(new InspectionData(CurrentInspectable, false, photoTaken));
            OnNonCompliantSelected?.Invoke(CurrentInspectable);

            SetUpNotification(false);
            DisplayNotification?.Invoke(notification);
            Hide();
        }

        /// <summary>
        /// upon user compliance selection if camera is selected add photo to list
        /// </summary>
        private void ConfirmPhoto()
        {
            if (photoTaken && !CurrentInspectable.HasPhoto)
            {
                CurrentInspectable.HasPhoto = true;
                PhotoConfirmed?.Invoke(CurrentInspectable);
                Debug.Log("Photo is added to list");
            }
        }

        /// <summary>
        /// This is used to set up the inspection window and display it.
        /// Invoked by <see cref="InspectionHandler.OnObjectClicked"/>
        /// </summary>
        /// <param name="obj">Inspectable Object that was clicked.</param>
        public void HandleInspectionWindowDisplay(InspectableObject obj)
        {
            CurrentInspectable = obj;

            if (obj.GetComponent<WorldObject>())
            {
                imageViewer.style.display = DisplayStyle.None;
                imageViewer3d.style.display = DisplayStyle.Flex;
                worldObjectRenderer.CameraFieldOfView = obj.GetComponent<ObjectViewerInspectables>().FieldOfView;
            }
            else
            {
                imageViewer.style.display = DisplayStyle.Flex;
                imageViewer3d.style.display = DisplayStyle.None;
            }

            // Set labels
            UIHelper.SetElementText(locationLabel, CurrentInspectable.Location.ToString());
            UIHelper.SetElementText(objectNameLabel, CurrentInspectable.Name);

            Show();
        }

        /// <summary>
        ///     Hides the UI and invokes OnWindowClosed event.
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(root);
            OnWindowClosed?.Invoke(CurrentInspectable);

            if (CurrentInspectable != null && CurrentInspectable.GetComponent<WorldObject>())
            {
                OnObjectViewerClosed?.Invoke(CurrentInspectable.gameObject);
            }

            // Reset the flag after the selection
            photoTaken = false;
            flashContainer.RemoveFromClassList("card-body-photo-frame");
        }

        /// <summary>
        ///     Shows the UI and invokes OnWindowOpened event.
        /// </summary>
        public void Show()
        {
            UIHelper.Show(root);
            OnWindowOpened?.Invoke(CurrentInspectable);
        }

        /// <summary>
        ///     Gets the photo that will be displayed in the inspection window.
        ///     Invoked by <see cref="ImageHandler.OnTempPhotoTaken"/>
        /// </summary>
        /// <param name="photo">Inspectable photo of the current object</param>
        public void GetPhoto(InspectablePhoto photo)
        {
            texture = new(ImageWidth, ImageHeight);
            texture.LoadImage(photo.Data);

            imageViewer.style.backgroundImage = texture;
        }

        /// <summary>
        ///     Updates the inspection label based on the previous inspection data.
        /// Invoked by <see cref="Inspections.OnPreviousInspectionRetrieved"/>
        /// </summary>
        /// <param name="inspection">The previous inspection data to display, or null if no previous inspection exists.</param>
        public void UpdateInspectionLabel(InspectionData inspection)
        {
            previousInspectionData = inspection;

            if (inspection == null)
            {
                UIHelper.SetElementText(inspectionLabel, "Is this visual inspection compliant or non-compliant?");
            }
            else
            {
                string compliancy = inspection.IsCompliant ? "compliant" : "non-compliant";
                string message = inspection.HasPhoto
                    ? $"Visual inspection with photo reported as {compliancy}."
                    : $"Visual inspection reported as {compliancy}.";

                UIHelper.SetElementText(inspectionLabel, message);
            }
        }
        #endregion
    }
}