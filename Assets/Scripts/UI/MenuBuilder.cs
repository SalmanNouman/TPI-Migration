using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    /// This class is responsible for setting up, building and managing the menu buttons and its events.
    /// </summary>
    public class MenuBuilder : MonoBehaviour, IUserInterface
    {
        /// <summary>
        /// Reference to the UI document root visual element
        /// </summary>
        private VisualElement root;

        /// <summary>
        /// Menu buttons
        /// </summary>
        private Button pauseButton;
        private Button inspectionReviewButton;
        private Button settingsButton;
        
        /// <summary>
        /// Button Events
        /// </summary>
        public UnityEvent OnPauseClicked;

        /// <summary>
        /// This event is invoked when the inspection review button is clicked.
        /// <see cref="InspectionReviewBuilder.Show"/>
        /// </summary>
        public UnityEvent OnInspectionReviewClicked;

        /// <summary>
        /// This event is invoked when the settings menu button is clicked.
        /// <see cref="SettingsMenuComplex.Show"/>
        /// </summary>
        public UnityEvent OnSettingsClicked;
        
        /// <summary>
        /// This event invoked when hovering on button
        /// <see cref="TooltipUI.HandleDisplayUI"/>
        /// </summary>
        public UnityEvent<VisualElement, TooltipType, string, FontSize> ShowToolTip;
        
        /// <summary>
        /// This event invoked when unhovering on button
        /// <see cref="TooltipUI.CloseTooltip"/>
        /// </summary>
        public UnityEvent HideToolTip;

        /// <summary>
        /// Gets the references to all the buttons and the root visual element.
        /// </summary>
        private void Start()
        {
            pauseButton = root.Q<Button>("Pause");
            inspectionReviewButton = root.Q<Button>("InspectionReview");
            settingsButton = root.Q<Button>("Settings");
            
            SetListeners();
            SetToolTips();
        }

        private void Awake()
        {
            root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            OnPauseClicked ??= new();
            OnInspectionReviewClicked ??= new();
            OnSettingsClicked ??= new();
        }

        /// <summary>
        /// Sets the buttons clicked events for all the buttons
        /// </summary>
        private void SetListeners()
        {
            pauseButton.clicked += PauseClicked;
            inspectionReviewButton.clicked += InspectionReviewClicked;
            settingsButton.clicked += SettingsClicked;
        }

        private void SetToolTips()
        {
            pauseButton.RegisterCallback<MouseOverEvent>(evt => 
                ShowToolTip?.Invoke(pauseButton, TooltipType.Left, "Pause", FontSize.Medium));
            pauseButton.RegisterCallback<MouseOutEvent>(evt => HideToolTip?.Invoke());
            
            inspectionReviewButton.RegisterCallback<MouseOverEvent>(evt => 
                ShowToolTip?.Invoke(inspectionReviewButton, TooltipType.Left, "Inspection Review", FontSize.Medium));
            inspectionReviewButton.RegisterCallback<MouseOutEvent>(evt => HideToolTip?.Invoke());
            
            settingsButton.RegisterCallback<MouseOverEvent>(evt => 
                ShowToolTip?.Invoke(settingsButton, TooltipType.Left, "Settings", FontSize.Medium));
            settingsButton.RegisterCallback<MouseOutEvent>(evt => HideToolTip?.Invoke());
            
        }
        
        /// <summary>
        /// Actions linked to the pause button clicked event.
        /// Invokes the OnPauseclicked event.
        /// </summary>
        private void PauseClicked()
        {
            OnPauseClicked?.Invoke();
            Debug.Log("Pause button clicked");
        }

        /// <summary>
        /// Actions linked to the inspection review button clicked event.
        /// Invokes the OnInspectionReviewClicked event.
        /// </summary>
        private void InspectionReviewClicked()
        {
            OnInspectionReviewClicked?.Invoke();
            Debug.Log("Inspection review button clicked");
        }

        /// <summary>
        /// Actions linked to the settings button clicked event.
        /// Invokes the OnSettingsClicked event.
        /// </summary>
        private void SettingsClicked()
        {
            OnSettingsClicked?.Invoke();
            Debug.Log("Settings button clicked");
        }

        /// <summary>
        /// Enables and disables the menu buttons
        /// </summary>
        /// <param name="toggle">true = buttons enabled, false = buttons disabled</param>
        public void ToggleButtons(bool toggle)
        {
            pauseButton.SetEnabled(toggle);
            inspectionReviewButton.SetEnabled(toggle);
            settingsButton.SetEnabled(toggle);
        }

        /// <summary>
        /// Hides the menu buttons
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(root);
        }

        /// <summary>
        /// Displays the menu buttons
        /// </summary>
        public void Show()
        {
            UIHelper.Show(root);
        }
    }
}
