using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///  Confirmation dialogs have a few different customizations:
    ///     - Close Button: The close button "X" in the top right can be enabled or disabled
    ///     - Canvas Dim: The background of the confirmation dialog can be dimmed to bring focus to the UI and block interaction with other UI
    ///     - Primary Button: The primary button type can use any ButtonType.cs template. The primary button will be the right most button
    ///     - Secondary Button: The secondary button type can use any ButtonType.cs template. The primary button will be the left most button
    /// 
    /// Intended use of this class is to connect ConfirmationDialog.HandleDisplayUI() to a UnityEvent in your DLX. Methods changing
    /// content, canvas dim, and show/hide have been exposed if DLX would prefer to serialize and control individual stages of
    /// the confirmation dialog
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ConfirmationDialog : MonoBehaviour, IUserInterface
    {
        [Header("Starting Values"), Space(10f)]
        [Tooltip("The button templates linked to the button types.")]
        [SerializeField] private List<VisualTreeAsset> buttonTemplates;

        [HideInInspector] public VisualElement Root { private set; get; }

        [Header("Event Hook-Ins")]
        public UnityEvent OnDialogShown;
        public UnityEvent OnDialogHidden;
        public UnityEvent OnPrimaryBtnClicked;
        public UnityEvent OnSecondaryBtnClicked;

        private VisualElement buttonContainer;
        private VisualElement canvas;
        private Label descriptionLabel;
        private Label nameLabel;
        private VisualElement checkBoxRow;
        private Label checkboxLabel;
        private Toggle checkbox;
        private Button closeBtn;
        private Button primaryBtn;
        private Button secondaryBtn;

        // window variables
        private VisualElement window;
        private int windowWidth;
        private int windowHeight;

        private const string DimmedBackgroundClass = "confirmation-dialog-canvas";
        private const string CancelButtonMarginClass = "mr-20";

        // Store the current dialog SO for reference
        private ConfirmDialogSO currentDialogSO;

        private void Start()
        {
            Root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            buttonContainer = Root.Q<VisualElement>("ButtonContainer");
            canvas = Root.Q<VisualElement>("Canvas");
            descriptionLabel = Root.Q<Label>("DescriptionLabel");
            nameLabel = Root.Q<Label>("NameLabel");
            checkBoxRow = Root.Q<VisualElement>("CheckboxRow");
            checkboxLabel = Root.Q<Label>("CheckboxLabel");
            checkbox = Root.Q<Toggle>("Checkbox");
            closeBtn = Root.Q<Button>("CloseBtn");

            window = Root.Q<VisualElement>("ConfirmationDialog");

            OnDialogShown ??= new UnityEvent();
            OnDialogHidden ??= new UnityEvent();
            OnPrimaryBtnClicked ??= new UnityEvent();
            OnSecondaryBtnClicked ??= new UnityEvent();

            closeBtn.clicked += () =>
            {
                Hide();
            };

            // Add checkbox value changed event handler
            if (checkbox != null)
            {
                checkbox.RegisterValueChangedCallback(CheckboxValueChanged);
            }

            UIHelper.Hide(Root);
        }

        /// <summary>
        /// This method is intended to be a single access public method to populate the confirmation dialog with text, 
        /// buttons, and display it
        /// </summary>
        /// <param name="confirmationDialogSO"></param>
        public void HandleDisplayUI(ConfirmDialogSO confirmationDialogSO)
        {
            SetContent(confirmationDialogSO);
            Show();
        }

        /// <summary>
        /// Confirmation dialogs have a few different customizations:
        ///     - Close Button: The close button "X" in the top right can be enabled or disabled
        ///     - Canvas Dim: The background of the confirmation dialog can be dimmed to bring focus to the UI and block interaction with other UI
        ///     - Primary Button: The primary button type can use any ButtonType.cs template. The primary button will be the right most button
        ///     - Secondary Button: The secondary button type can use any ButtonType.cs template. The primary button will be the left most button
        /// </summary>
        /// <param name="confirmDialogSO"></param>
        public void SetContent(ConfirmDialogSO confirmDialogSO)
        {
            ClearButtons();

            // Store the current dialog SO for reference
            currentDialogSO = confirmDialogSO;

            UIHelper.SetElementText(nameLabel, confirmDialogSO.Name);
            UIHelper.SetElementText(descriptionLabel, confirmDialogSO.Description);
            StyleHelper.SetCanvasDim(canvas, confirmDialogSO.IsBackgroundDimmed, DimmedBackgroundClass);

            AddSecondaryButton(confirmDialogSO.SecondaryBtnType, confirmDialogSO.SecondaryBtnText);
            AddPrimaryButton(confirmDialogSO.PrimaryBtnType, confirmDialogSO.PrimaryBtnText);
            SetCloseButton(confirmDialogSO.IsCloseBtnVisible);
            SetCheckbox(confirmDialogSO.IsCheckboxVisible, confirmDialogSO.CheckBoxText);
            // Need to specify size params for all windows
            window.style.width = confirmDialogSO.WindowWidth;
            window.style.height = confirmDialogSO.WindowHeight;
        }

        /// <summary>
        /// Sets the closeBtn to DisplayStyle.Flex or DisplayStyle.None depending on incoming isCloseBtnVisible
        /// </summary>
        /// <param name="isCloseBtnVisible"></param>
        public void SetCloseButton(bool isCloseBtnVisible)
        {
            if (isCloseBtnVisible)
            {
                UIHelper.Show(closeBtn);
            }
            else
            {
                UIHelper.Hide(closeBtn);
            }
        }

        /// <summary>
        /// Sets the checkbox to DisplayStyle.Flex or DisplayStyle.None depending on incoming checkBoxVisible
        /// </summary>
        /// <param name="checkBoxVisible"></param>
        /// <param name="checkboxText"></param>
        public void SetCheckbox(bool checkBoxVisible, string checkboxText)
        {
            if (checkBoxVisible)
            {
                UIHelper.Show(checkBoxRow);
                UIHelper.Show(checkboxLabel);
                UIHelper.SetElementText(checkboxLabel, checkboxText);
                checkbox.value = false;
            }
            else
            {
                UIHelper.Hide(checkBoxRow);
                UIHelper.Hide(checkboxLabel);
            }
        }

        /// <summary>
        /// Shows the root of the confirmation dialog and triggers OnDialogShown
        /// </summary>
        public void Show()
        {
            UIHelper.Show(Root);
            OnDialogShown.Invoke();
        }

        /// <summary>
        /// Hides the root of the confirmation dialog and triggers OnDialogHidden
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(Root);
            OnDialogHidden.Invoke();
        }

        /// <summary>
        /// Adds a primary button to the confirmation dialog that matches the incoming ButtonType. 
        /// This uses VELCRO button template uxml files
        /// </summary>
        /// <param name="buttonType"></param>
        /// <param name="buttonText"></param>
        private void AddPrimaryButton(ButtonType buttonType, string buttonText)
        {
            VisualElement newButton = CreateButton(buttonType, buttonText);
            primaryBtn = newButton.Q<Button>();
            primaryBtn.RegisterCallback<ClickEvent>(PrimaryBtnClicked);
        }

        /// <summary>
        /// Adds a secondary button to the confirmation dialog that matches the incoming ButtonType. 
        /// This uses VELCRO button template uxml files
        /// </summary>
        /// <param name="buttonType"></param>
        /// <param name="buttonText"></param>
        private void AddSecondaryButton(ButtonType buttonType, string buttonText)
        {
            VisualElement newButton = CreateButton(buttonType, buttonText);
            newButton.AddToClassList(CancelButtonMarginClass);
            secondaryBtn = newButton.Q<Button>();
            secondaryBtn.RegisterCallback<ClickEvent>(SecondaryBtnClicked);
        }

        /// <summary>
        /// Clones a uxml template matching incoming ButtonType and returns the cloned Button. Internal 
        /// method for use with AddPrimaryButton/AddSecondaryButton methods
        /// </summary>
        /// <param name="buttonType"></param>
        /// <param name="buttonText"></param>
        /// <returns></returns>
        private VisualElement CreateButton(ButtonType buttonType, string buttonText)
        {
            VisualTreeAsset template = buttonTemplates[(int)buttonType];
            VisualElement buttonClone = template.CloneTree();
            Button button = buttonClone.Q<Button>();
            UIHelper.SetElementText(button, buttonText);
            buttonContainer.Add(buttonClone);
            return buttonClone;
        }

        /// <summary>
        /// Removes any existing buttons in the #ButtonContainer element
        /// </summary>
        private void ClearButtons()
        {
            primaryBtn?.UnregisterCallback<ClickEvent>(PrimaryBtnClicked);
            secondaryBtn?.UnregisterCallback<ClickEvent>(SecondaryBtnClicked);
            buttonContainer?.Clear();
        }

        /// <summary>
        /// Handles the checkbox value changed event
        /// </summary>
        /// <param name="evt">The event containing the new value</param>
        private void CheckboxValueChanged(ChangeEvent<bool> evt)
        {
            if (currentDialogSO != null)
            {
                currentDialogSO.InvokeToggleAction(evt.newValue);
            }
        }

        /// <summary>
        /// Hides the root of the confirmation dialog and triggers OnPrimaryBtnClicked
        /// </summary>
        /// <param name="evt"></param>
        private void PrimaryBtnClicked(ClickEvent evt)
        {
            Hide();

            // Invoke the primary action from the scriptable object if available
            if (currentDialogSO != null)
            {
                currentDialogSO.InvokePrimaryAction();
            }

            OnPrimaryBtnClicked.Invoke();
        }

        /// <summary>
        /// Hides the root of the confirmation dialog and triggers OnSecondaryBtnClicked
        /// </summary>
        /// <param name="evt"></param>
        private void SecondaryBtnClicked(ClickEvent evt)
        {
            Hide();

            // Invoke the secondary action from the scriptable object if available
            if (currentDialogSO != null)
            {
                currentDialogSO.InvokeSecondaryAction();
            }

            OnSecondaryBtnClicked.Invoke();
        }

        private void OnDestroy()
        {
            ClearButtons();
        }
    }
}