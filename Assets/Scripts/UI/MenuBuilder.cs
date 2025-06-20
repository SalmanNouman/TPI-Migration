using System.Collections.Generic;
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
        /// Tracks whether each button has shown its tooltip for the first time
        /// </summary>
        private Dictionary<Button, bool> hasShownTooltip = new Dictionary<Button, bool>();

        /// <summary>
        /// Debounce timer for tooltip events
        /// </summary>
        private float lastTooltipEventTime = 0f;

        /// <summary>
        /// Minimum time between tooltip events (in seconds)
        /// </summary>
        private const float ToolTipDebounceTime = 0.001f;

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
            
            // Initialize tracking for each button
            hasShownTooltip[pauseButton] = false;
            hasShownTooltip[inspectionReviewButton] = false;
            hasShownTooltip[settingsButton] = false;
            
            SetListeners();
            SetToolTips();
        }

        private void Awake()
        {
            root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            OnPauseClicked ??= new();
            OnInspectionReviewClicked ??= new();
            OnSettingsClicked ??= new();
            
            // Initialize the dictionary
            hasShownTooltip = new Dictionary<Button, bool>();
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
        /// <summary>
        /// Sets the tooltips for all the buttons with debounced events to prevent jittering tooltips.
        /// </summary>
        private void SetToolTips()
        {
            // Add debounced tooltip events for pause button
            pauseButton.RegisterCallback<MouseOverEvent>(evt =>
            {
                if (!hasShownTooltip[pauseButton])
                {
                    // First show - apply debounce
                    if (ShouldProcessTooltipEvent())
                    {
                        ShowToolTip?.Invoke(pauseButton, TooltipType.Left, "Pause", FontSize.Medium);
                        hasShownTooltip[pauseButton] = true;
                    }
                }
                else
                {
                    // After first show - no debounce
                    ShowToolTip?.Invoke(pauseButton, TooltipType.Left, "Pause", FontSize.Medium);
                }
            });

            pauseButton.RegisterCallback<MouseOutEvent>(evt =>
            {
                if (ShouldProcessTooltipEvent())
                {
                    HideToolTip?.Invoke();
                }
            });
            
            // Add debounced tooltip events for inspection review button
            inspectionReviewButton.RegisterCallback<MouseOverEvent>(evt =>
            {
                if (!hasShownTooltip[inspectionReviewButton])
                {
                    // First show - apply debounce
                    if (ShouldProcessTooltipEvent())
                    {
                        ShowToolTip?.Invoke(inspectionReviewButton, TooltipType.Left, "Inspection Review", FontSize.Medium);
                        hasShownTooltip[inspectionReviewButton] = true;
                    }
                }
                else
                {
                    // After first show - no debounce
                    ShowToolTip?.Invoke(inspectionReviewButton, TooltipType.Left, "Inspection Review", FontSize.Medium);
                }
            });

            inspectionReviewButton.RegisterCallback<MouseOutEvent>(evt =>
            {
                HideToolTip?.Invoke();
            });
            
            // Add debounced tooltip events for settings button
            settingsButton.RegisterCallback<MouseOverEvent>(evt =>
            {
                if (!hasShownTooltip[settingsButton])
                {
                    // First show - apply debounce
                    if (ShouldProcessTooltipEvent())
                    {
                        ShowToolTip?.Invoke(settingsButton, TooltipType.Left, "Settings", FontSize.Medium);
                        hasShownTooltip[settingsButton] = true;
                    }
                }
                else
                {
                    // After first show - no debounce
                    ShowToolTip?.Invoke(settingsButton, TooltipType.Left, "Settings", FontSize.Medium);
                }
            });

            settingsButton.RegisterCallback<MouseOutEvent>(evt =>
            {
                HideToolTip?.Invoke();
            });
        }

        /// <summary>
        /// Determines if enough time has passed to process a new tooltip event
        /// </summary>
        /// <returns>True if the event should be processed, false if it should be ignored</returns>
        private bool ShouldProcessTooltipEvent()
        {
            float currentTime = Time.time;
            
            // Check if enough time has passed since the last tooltip event
            if (currentTime - lastTooltipEventTime < ToolTipDebounceTime)
            {
                return false; // Ignore this event (debounce)
            }
            
            // Update the last event time and allow this event
            lastTooltipEventTime = currentTime;
            return true;
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
