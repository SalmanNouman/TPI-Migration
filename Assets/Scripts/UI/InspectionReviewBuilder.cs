using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Navigation.PointClick;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    /// Class that manages the inspection review window in UI.
    /// </summary>
    public class InspectionReviewBuilder : MonoBehaviour, IUserInterface
    {
        [Header("Inspection Review UI")]

        [Tooltip("Window Title")]
        [SerializeField] private string windowTitle = "Inspection Review";

        [Header("Tabs Setting"), Space(10f)]

        [Tooltip("Tab 1 Icon")]
        [SerializeField] private Sprite iconOne;

        [Tooltip("Tab 1 Label")]
        [SerializeField] private string tabOne;

        [Tooltip("Tab 2 Icon")]
        [SerializeField] private Sprite iconTwo;

        [Tooltip("Tab 2 Label")]
        [SerializeField] private string tabTwo;

        [Tooltip("Tab 3 Icon")]
        [SerializeField] private Sprite iconThree;

        [Tooltip("Tab 3 Label")]
        [SerializeField] private string tabThree;

        private VisualElement root;

        private Label windowTitleLabel;
        private Label tabOneLabel;
        private Label tabTwoLabel;
        private Label tabThreeLabel;

        private VisualElement iconOneElement;
        private VisualElement iconTwoElement;
        private VisualElement iconThreeElement;

        private Button closeButton;
        private Button tabOneButton;
        private Button tabTwoButton;
        private Button tabThreeButton;

        private Button currentButton;

        private Button endInspectionButton;

        // uss classes
        private const string SelectedTab = "navigation-horizontal-button-selected";
        private const string FontRegular = "fw-400";
        private const string FontBold = "fw-700";

        // Dialog SO for End Inspection pop up
        public ConfirmDialogSO EndInspectionDialogSO;

        /// <summary>
        /// Invoked when the toggles are clicked in the end inspection confirmation dialog.
        /// <see cref="ConfirmationDialog.UpdatePrimaryButtonState"/>
        /// </summary>
        public UnityEvent EndInspectionToggleEvent;

        /// <summary>
        /// Invoked when Show is called.
        /// <see cref="MenuBuilder.Hide"/>
        /// <see cref="Volume.enable"/>true
        /// <see cref="PointClickNavigation.EnableCameraPanAndZoom(bool)"/>false
        /// <see cref="PointClickNavigation.EnableNavigation(bool)"/>false
        /// <see cref="ProgressBuilder.ProgressOpen(bool)"/>true
        /// <see cref="ActivityLogBuilder.HandleDisplayActivityLog(bool)"/>true
        /// </summary>
        [Header("Events"), Space(10f)]
        [Tooltip("Invoked when Show is called.")]
        public UnityEvent OnShow;

        /// <summary>
        /// Invoked when Hide is called.
        /// <see cref="MenuBuilder.Show"/>
        /// <see cref="Volume.enable(bool)"/>false
        /// <see cref="PointClickNavigation.EnableCameraPanAndZoom(bool)"/>true
        /// <see cref="PointClickNavigation.EnableNavigation(bool)"/>true
        /// <see cref="ProgressBuilder.ProgressOpen(bool)"/>false
        /// <see cref="ActivityLogBuilder.HandleDisplayActivityLog(bool)"/>false
        /// </summary>
        [Tooltip("Invoked when Hide is called.")]
        public UnityEvent OnHide;

        /// <summary>
        /// Invoked when tab one is selected.
        /// <see cref="InspectionLogBuilder.TabSelected"/>
        /// </summary>
        [Tooltip("Invoked when tab one is selected.")]
        public UnityEvent OnTabOneSelected;

        /// <summary>
        /// Invoked when tab two is selected.
        /// <see cref="ActivityLogBuilder.TabSelected"/>
        /// </summary>
        [Tooltip("Invoked when tab two is selected.")]
        public UnityEvent OnTabTwoSelected;

        /// <summary>
        /// Invoked when tab three is selected.
        /// <see cref="GalleryBuilder.TabSelected"/>
        /// </summary>
        [Tooltip("Invoked when tab three is selected.")]
        public UnityEvent OnTabThreeSelected;

        /// <summary>
        /// Invoked when the end inspection button is clicked.
        /// <see cref="ConfirmationDialog.HandleDisplayUI(ConfirmDialogSO)"/>
        /// </summary>
        public UnityEvent<ConfirmDialogSO> OnEndInspectionButtonClicked;

        /// <summary>
        /// Invoked when the End Inspection dialog is confirmed.
        /// <see cref="Hide"/>
        /// <see cref="TimerManager.PauseTimers"/>
        /// <see cref="PoiHandler.GetPoiList"
        /// <see cref="InspectionSummaryBuilder.SetDateTimeAndTimer"/>
        /// <see cref="InspectionSummaryBuilder.Show"/>
        /// </summary>
        public UnityEvent OnEndInspectionConfirmation;

        private void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;

            GetAllReferences();

            // Disable the end inspection button by default
            endInspectionButton.SetEnabled(false);

            // initialize events
            OnEndInspectionConfirmation ??= new();
            OnEndInspectionButtonClicked ??= new();

            SetContentAnListeners();

            // initialize the Unity Events
            OnTabOneSelected ??= new();
            OnTabTwoSelected ??= new();
            OnTabThreeSelected ??= new();

            // Hides the UI on Start
            UIHelper.Hide(root);
        }

        /// <summary>
        /// Gets References to all the Visual Elements
        /// </summary>
        private void GetAllReferences()
        {
            // Buttons
            closeButton = root.Q<TemplateContainer>("CloseBtnContainer").Q<Button>("CloseBtn");
            tabOneButton = root.Q<Button>("TabOne");
            tabTwoButton = root.Q<Button>("TabTwo");
            tabThreeButton = root.Q<Button>("TabThree");

            // Labels
            windowTitleLabel = root.Q<Label>("NameLabel");
            tabOneLabel = tabOneButton.Q<Label>("TabLabel");
            tabTwoLabel = tabTwoButton.Q<Label>("TabLabel");
            tabThreeLabel = tabThreeButton.Q<Label>("TabLabel");

            // Tab icons
            iconOneElement = tabOneButton.Q<VisualElement>("Icon");
            iconTwoElement = tabTwoButton.Q<VisualElement>("Icon");
            iconThreeElement = tabThreeButton.Q<VisualElement>("Icon");

            // End Inspection button
            endInspectionButton = root.Q<Button>("PrimaryButton");
        }

        /// <summary>
        /// Set the content for all labels, icons and buttons.
        /// </summary>
        private void SetContentAnListeners()
        {
            // Set label text
            UIHelper.SetElementText(windowTitleLabel, windowTitle);
            UIHelper.SetElementText(tabOneLabel, tabOne);
            UIHelper.SetElementText(tabTwoLabel, tabTwo);
            UIHelper.SetElementText(tabThreeLabel, tabThree);

            // Set icons
            UIHelper.SetElementSprite(iconOneElement, iconOne);
            UIHelper.SetElementSprite(iconTwoElement, iconTwo);
            UIHelper.SetElementSprite(iconThreeElement, iconThree);

            // Set tab button listeners
            closeButton.clicked += Hide;
            tabOneButton.clicked += () =>
            {
                SelectTab(tabOneButton);
                OnTabOneSelected?.Invoke();
            };
            tabTwoButton.clicked += () =>
            {
                SelectTab(tabTwoButton);
                OnTabTwoSelected?.Invoke();
            };
            tabThreeButton.clicked += () =>
            {
                SelectTab(tabThreeButton);
                OnTabThreeSelected?.Invoke();
            };

            // Set up primary button listener
            endInspectionButton.clicked += () =>
            {
                EndInspectionDialogSO.SetPrimaryAction(() => { OnEndInspectionConfirmation?.Invoke(); });
                EndInspectionDialogSO.SetPrimaryToggleAction((isChecked) => { EndInspectionToggleEvent?.Invoke(); });
                EndInspectionDialogSO.SetSecondaryToggleAction((isChecked) => { EndInspectionToggleEvent?.Invoke(); });

                OnEndInspectionButtonClicked?.Invoke(EndInspectionDialogSO);
            };
        }

        /// <summary>
        /// Selects the new tab that was clicked.
        /// </summary>
        /// <param name="button">Tab that was selected</param>
        public void SelectTab(Button button)
        {
            if (currentButton != null)
            {
                UnselectTab(currentButton);
            }

            button.AddToClassList(SelectedTab);
            button.Q<Label>().RemoveFromClassList(FontRegular);
            button.Q<Label>().AddToClassList(FontBold);
            button.SetEnabled(false);

            currentButton = button;
        }

        /// <summary>
        /// Unselected the tab that was previously selected
        /// </summary>
        /// <param name="tab">Button that will be unselected.</param>
        private void UnselectTab(Button tab)
        {
            tab.RemoveFromClassList(SelectedTab);
            tab.Q<Label>().RemoveFromClassList(FontBold);
            tab.Q<Label>().AddToClassList(FontRegular);
            tab.SetEnabled(true);
        }

        /// <summary>
        /// Updates the end inspection button state based on task completion status
        /// Handwashing task must be completed to enable the end inspection button
        /// </summary>
        public void UpdateEndInspectionButtonState()
        {
            // Enable the end inspection button only when both handwashing is completed
            bool tasksCompleted = TPISceneManager.HandWashingCompleted;
            endInspectionButton.SetEnabled(tasksCompleted);
        }

        /// <summary>
        /// Is called when the menu inspection button is clicked to display the inspection window.
        /// Tab one is always the active tab on show.
        /// OnShow event is invoked.
        /// </summary>
        public void Show()
        {
            SelectTab(tabOneButton);
            OnTabOneSelected.Invoke();
            UpdateEndInspectionButtonState();
            UIHelper.Show(root);
            OnShow?.Invoke();
        }

        /// <summary>
        /// Hides the inspection window and invoke the On Hide event.
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(root);
            OnHide?.Invoke();
        }
    }
}
