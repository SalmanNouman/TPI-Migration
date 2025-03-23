using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///  Information dialogs have a few customizations available:
    ///     - Canvas Dim: The background of the information dialog can be dimmed to bring focus to the UI and block interaction with other UI
    ///     - Title: The title element can be toggled on or off
    ///     - Description: The description can be bolded or not
    ///     
    /// Intended use of this class is to connect InformationDialog.HandleDisplayUI() to a UnityEvent in your DLX. Methods changing
    /// content, canvas dim, and show/hide have been exposed if DLX would prefer to serialize and control individual stages of
    /// the information dialog
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class InformationDialog : MonoBehaviour, IUserInterface
    {
        [HideInInspector] public VisualElement Root { private set; get; }

        [Header("Event Hook-Ins")]
        public UnityEvent OnDialogShown;
        public UnityEvent OnDialogHidden;
        public UnityEvent OnBtnClicked;

        private Label nameLabel;
        private Label titleLabel;
        private Label descriptionLabel;

        private VisualElement Icon;

        private Button primaryBtn;
        private VisualElement canvas;

        private const string DimmedBackgroundClass = "information-dialog-canvas";
        private const string BoldUssClass = "fw-700";
        private const string RegularUssClass = "fw-400";

        // Store the current dialog SO for reference
        private InformDialog currentDialogSO;

        private void Start()
        {
            Root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            nameLabel = Root.Q<Label>("NameLabel");
            titleLabel = Root.Q<Label>("TitleLabel");
            descriptionLabel = Root.Q<Label>("DescriptionLabel");

            Icon = Root.Q<VisualElement>("IconBackground");

            primaryBtn = Root.Q<TemplateContainer>().Q<Button>("Button");
            canvas = Root.Q<VisualElement>("Canvas");

            OnDialogShown ??= new UnityEvent();
            OnDialogHidden ??= new UnityEvent();
            OnBtnClicked ??= new UnityEvent();

            primaryBtn.clicked += () =>
            {
                OnBtnClicked?.Invoke();
                currentDialogSO.InvokePrimaryAction();
                Hide();
            };

            UIHelper.Hide(Root);
        }

        /// <summary>
        /// This method is intended to be a single access public method to populate the information dialog with text, 
        /// buttons, and display it
        /// </summary>
        /// <param name="informationDialogSO"></param>
        public void HandleDisplayUI(InformDialog informationDialogSO)
        {
            SetContent(informationDialogSO);
            Show();
        }

        /// <summary>
        /// Information dialogs only have one customization available:
        ///     - Canvas Dim: The background of the information dialog can be dimmed to bring focus to the UI and block interaction with other UI
        ///     - Title: The title element can be toggled on or off
        ///     - Description: The description can be bolded or not
        /// </summary>
        /// <param name="informationDialogSO"></param>
        public void SetContent(InformDialog informationDialogSO)
        {
            ClearClasses();
            // Store the current dialog SO for reference
            currentDialogSO = informationDialogSO;

            string fontClass = informationDialogSO.IsDescriptionBolded ? BoldUssClass : RegularUssClass;
            descriptionLabel.EnableInClassList(fontClass, true);

            if (informationDialogSO.IsIconVisible)
            {
                Icon.style.display = DisplayStyle.Flex;
            }
            else
            {
                Icon.style.display = DisplayStyle.None;
            }

            UIHelper.SetElementText(nameLabel, informationDialogSO.Name);
            UIHelper.SetElementText(titleLabel, informationDialogSO.Title);
            UIHelper.SetElementText(descriptionLabel, informationDialogSO.Description);
            UIHelper.SetElementText(primaryBtn, informationDialogSO.PrimaryBtnText);
            StyleHelper.SetCanvasDim(canvas, informationDialogSO.IsBackgroundDimmed, DimmedBackgroundClass);
        }

        /// <summary>
        /// Shows the root of the information dialog and triggers OnDialogShown
        /// </summary>
        public void Show()
        {
            UIHelper.Show(Root);
            OnDialogShown?.Invoke();
        }

        /// <summary>
        /// Hides the root of the information dialog and triggers OnDialogHidden
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(Root);
            OnDialogHidden?.Invoke();
        }

        /// <summary>
        /// Clears all font weight classes on the description label
        /// </summary>
        private void ClearClasses()
        {
            descriptionLabel.EnableInClassList(BoldUssClass, false);
            descriptionLabel.EnableInClassList(RegularUssClass, false);
        }
    }
}