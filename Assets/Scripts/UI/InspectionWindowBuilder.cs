using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Interactions;
using VARLab.Navigation.PointClick;
using VARLab.Velcro;

namespace VARLab.DLX
{
    public class InspectionWindowBuilder : MonoBehaviour, IUserInterface
    {
        // Visual Elements
        private VisualElement root;

        //private VisualElement objectViewer;
        private VisualElement imageViewer;

        // Labels
        private Label locationLabel;
        private Label objectNameLabel;
        private Label inspectionLabel;

        // Buttons
        private Button closeButton;
        private Button cameraButton;
        private Button compliantButton;
        private Button nonCompliantButton;

        [Header("Unity Events")]
        /// <summary>
        /// Unity Event that is invoked when the inspection window is displayed.
        /// <see cref="InteractionHandler.enabled(false)"/>
        /// <see cref="PointClickNavigation.EnableCameraPanAndZoom(false)"/>
        /// <see cref="PointClickNavigation.EnableNavigation(false)"/>
        /// <see cref="MenuBuilder.Hide"/>
        /// </summary>
        public UnityEvent<InspectableObject> OnWindowOpened;

        /// <summary>
        /// Unity Event that is triggered when the inspection window is closed
        /// <see cref="InteractionHandler.enabled(true)"/>
        /// <see cref="PointClickNavigation.EnableCameraPanAndZoom(true)"/>
        /// <see cref="PointClickNavigation.EnableNavigation(true)"/>
        /// <see cref="MenuBuilder.Show"/>
        /// </summary>
        public UnityEvent<InspectableObject> OnWindowClosed;

        /// <summary>
        /// Unity Event that is triggered when the camera button is clicked.
        /// </summary>
        public UnityEvent<InspectableObject> OnPhotoTaken;

        /// <summary>
        /// Unity Event that is triggered when the compliant button is clicked.
        /// </summary>
        public UnityEvent<InspectableObject> OnCompliantSelected;

        /// <summary>
        /// Unity Event that is triggered when the non-compliant button is clicked.
        /// </summary>
        public UnityEvent<InspectableObject> OnNonCompliantSelected;

        [Header("Notification Event"), Space(10f)]
        [Tooltip("Invoked to display a notification. The event takes a NotificationSO as its parameter.")]
        public UnityEvent<NotificationSO> DisplayNotification;

        // Inspectable object that is currently displayed in the inspection window.
        [HideInInspector]
        public InspectableObject CurrentInspectable;

        // Image 
        private Texture2D texture;
        private const int ImageHeight = 580;
        private const int ImageWidth = 770;

        //Flag to track if the camera button was pressed
        private bool photoTaken = false;
        //Notification instance to be updated and reused
        private NotificationSO notification;

        private void Start()
        {
            OnWindowOpened ??= new();
            OnWindowClosed ??= new();
            OnPhotoTaken ??= new();
            OnCompliantSelected ??= new();
            OnNonCompliantSelected ??= new();
            DisplayNotification ??= new UnityEvent<NotificationSO>();

            root = GetComponent<UIDocument>().rootVisualElement;
            Hide();

            GetAllReferences();
            AddButtonListeners();

            // Create a new instance of NotificationSO to be reused
            notification = ScriptableObject.CreateInstance<NotificationSO>();


            // TODO: Remove this hardcoded text when reinspection is implemented
            UIHelper.SetElementText(inspectionLabel, "Is this visual inspection compliant or non-compliant?");

        }

        /// <summary>
        /// Get the reference of all visual elements that are required to build the inspection window.
        /// </summary>
        private void GetAllReferences()
        {
            // Visual Elements
            //objectViewer = root.Q<VisualElement>("ObjectViewer");
            imageViewer = root.Q<VisualElement>("Image");

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
        /// Add the listeners to all the button in the inspection window.
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
        /// Method that is called when the camera button is clicked.
        /// This invokes the OnPhotoTaken event.
        /// </summary>
        private void TakePhoto()
        {
            // TODO: display flash and camera outline
            photoTaken = true;
            OnPhotoTaken?.Invoke(CurrentInspectable);
        }

        /// <summary>
        /// Sets up the notification based on whether the inspection is compliant or non-compliant.
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
        /// This method is called when the compliant button is clicked.
        /// Invokes the OnCompliantSelected event.
        /// </summary>
        private void CompliantSelected()
        {
            // TODO: Check if this object already has an inspection.
            // If true and value changed display modal.
            // If false and value is the same display toast.
            OnCompliantSelected?.Invoke(CurrentInspectable);

            SetUpNotification(true);
            DisplayNotification?.Invoke(notification);
            Hide();

            // Reset the flag after the selection
            photoTaken = false;
        }

        /// <summary>
        /// This method is called when the non-compliant button is clicked.
        /// Invokes the OnNonCompliantSelected event.
        /// </summary>
        private void NonCompliantSelected()
        {
            // TODO: Check if this object already has an inspection.
            // If true and value changed display modal.
            // If false and value is the same display toast.
            OnNonCompliantSelected?.Invoke(CurrentInspectable);

            SetUpNotification(false);
            DisplayNotification?.Invoke(notification);
            Hide();

            // Reset the flag after the selection
            photoTaken = false;
        }

        /// <summary>
        /// This is used to set up the inspection window and display it.
        /// Invoked by <see cref="InspectionHandler.OnObjectClicked"/>
        /// </summary>
        /// <param name="obj">Inspectable Object that was clicked.</param>
        public void HandleInspectionWindowDisplay(InspectableObject obj)
        {
            CurrentInspectable = obj;

            // Set labels
            UIHelper.SetElementText(locationLabel, CurrentInspectable.Location.ToString());
            UIHelper.SetElementText(objectNameLabel, CurrentInspectable.Name);

            Show();

        }

        /// <summary>
        /// Hides the UI and invokes OnWindowClosed event.
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(root);
            OnWindowClosed?.Invoke(CurrentInspectable);
        }

        /// <summary>
        /// Shows the UI and invokes OnWindowOpened event.
        /// </summary>
        public void Show()
        {
            UIHelper.Show(root);
            OnWindowOpened?.Invoke(CurrentInspectable);
        }

        /// <summary>
        /// Gets the photo that will be displayed in the inspection window.
        /// Invoked by <see cref="ImageHandler.OnTempPhotoTaken"/>
        /// </summary>
        /// <param name="photo">Inspectable photo of the current object</param>
        public void GetPhoto(InspectablePhoto photo)
        {
            texture = new(ImageWidth, ImageHeight);
            texture.LoadImage(photo.Data);

            imageViewer.style.backgroundImage = texture;
        }
    }
}
