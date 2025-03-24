using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///     Manages the conversation UI for dialogue interactions.
    ///     Handles displaying dialogue entries from ConversationSO data.
    /// </summary>
    public class ConversationBuilder : MonoBehaviour, IUserInterface
    {
        #region Fields

        [Header("UI Templates"), Space(10f)]
        [SerializeField, Tooltip("The UXML template for conversation entries")]
        private VisualTreeAsset conversationUXML;

        // Root visual element from the UI document that contains all UI elements
        private VisualElement root;

        // UI Elements
        private VisualElement content;
        private Label titleLabel;
        private Label taskTextLabel;
        private Button primaryButton;

        //  ScriptableObject for conversation
        private ConversationSO currentConversation;

        // Current dialogue index
        private int currentIndex = 0;

        private const int DefaultBorderRadius = 10;

        // TODO: Implement audio clips
        // private List<AudioClip> audioClips;

        #endregion

        #region Events

        [Header("Events"), Space(10f)]
        /// <summary>
        ///     Event triggered when the conversation window is shown.
        ///     <see cref="PointClickNavigation.EnableNavigation(false)"/>
        ///     <see cref="PointClickNavigation.EnableCameraPanAndZoom(false)"/>
        ///     <see cref="BlurBackground.Volume.enabled(true)"/>
        ///     <see cref="MenuBuilder.Hide"/>
        /// </summary>
        [SerializeField, Tooltip("Invoked when window is shown.")]
        public UnityEvent OnWindowShow;

        /// <summary>
        ///     Event triggered when the conversation window is hidden.
        ///     <see cref="PointClickNavigation.EnableNavigation(true)"/>
        ///     <see cref="PointClickNavigation.EnableCameraPanAndZoom(true)"/>
        ///     <see cref="BlurBackground.Volume.enabled(false)"/>
        ///     <see cref="MenuBuilder.Show"/>
        ///     <see cref="IntroductionTask.CompleteTask"/>
        /// </summary>
        [SerializeField, Tooltip("Invoked when window is hidden.")]
        public UnityEvent OnWindowHide;

        // TODO: Implement audio event
        // public UnityEvent<AudioClip[]> OnConversationAudio;

        #endregion

        #region Methods

        /// <summary>
        ///     Gets UI document component reference and initializes events to prevent null reference errors.
        /// </summary>
        private void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            OnWindowShow ??= new();
            OnWindowHide ??= new();
        }

        /// <summary>
        ///     Sets up UI references, button listeners, and ensures window is hidden when scene starts.
        /// </summary>
        private void Start()
        {
            GetAllReferences();
            SetupButtonListeners();
            Hide();  // Set initial display state to none
        }

        #endregion

        #region UI Setup Methods

        /// <summary>
        ///     Gets references to all UI elements from the root visual element.
        /// </summary>
        private void GetAllReferences()
        {
            content = root.Q<VisualElement>("Content");
            var header = root.Q<VisualElement>("Header");
            titleLabel = header.Q<Label>("NameLabel");
            taskTextLabel = root.Q<Label>("TaskText");
            primaryButton = root.Q<Button>("Button");
        }

        /// <summary>
        ///     Sets up click event listeners for the primary button.
        /// </summary>
        private void SetupButtonListeners()
        {
            // Primary button hides the conversation window and completes the task
            primaryButton.clicked += () =>
            {
                Hide();
                HandleResetConversation();
            };
        }

        /// <summary>
        ///     Resets the conversation window and sets the conversation to null.   
        /// </summary>
        public void HandleResetConversation()
        {
            root.style.display = DisplayStyle.None;
            StartCoroutine(InteractionsRoutine());
            currentIndex = 0;
            currentConversation = null;
            root.Q<VisualElement>("Content").Clear();
        }

        /// <summary>
        ///     Shows the conversation window and notifies listeners through OnWindowShow event.
        /// </summary>
        public void Show()
        {
            UIHelper.Show(root);
            OnWindowShow?.Invoke();
        }

        /// <summary>
        ///     Hides the conversation window and notifies listeners through OnWindowHide event.
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(root);
            OnWindowHide?.Invoke();
        }

        /// <summary>
        ///     Displays a conversation in the UI.
        ///     Called via <see cref="IntroductionTask.OnStartConversation"/> 
        /// </summary>
        /// <param name="conversation"> Dialogue entries to display</param>
        public void HandleDisplayConversation(ConversationSO conversation)
        {
            currentIndex = 0;
            currentConversation = conversation;

            HandleSetContent();
            StartCoroutine(InteractionsRoutine());
            Show();
        }

        /// <summary>
        ///     Sets the content of the conversation window.
        /// </summary>
        public void HandleSetContent()
        {
            titleLabel.text = currentConversation.Name;
            taskTextLabel.text = currentConversation.TaskText;
            primaryButton.text = currentConversation.ButtonText;

            SetupAllDialogues();
        }

        /// <summary>
        ///     Creates and displays all dialogue items
        /// </summary>
        private void SetupAllDialogues()
        {
            // Create and display all dialogue items
            foreach (ConversationSO.Dialogue dialogue in currentConversation.dialogue)
            {
                // Get element references
                VisualElement bodyElement = conversationUXML.CloneTree();
                VisualElement border = bodyElement.Q<VisualElement>("Border");
                VisualElement body = bodyElement.Q<VisualElement>("Body");
                VisualElement image = bodyElement.Q<VisualElement>("Image");
                Label caption = bodyElement.Q<Label>("Caption");
                Label description = bodyElement.Q<Label>("Description");

                // Dialogue content
                caption.text = dialogue.Speaker;
                description.text = dialogue.Text;

                // Avatar image
                image.style.backgroundImage = dialogue.Avatar.texture;

                // Border color
                border.style.borderTopColor = dialogue.ImageBorderColour;
                border.style.borderRightColor = dialogue.ImageBorderColour;
                border.style.borderBottomColor = dialogue.ImageBorderColour;
                border.style.borderLeftColor = dialogue.ImageBorderColour;

                // Text background color
                description.style.backgroundColor = dialogue.TextBackgroundColour;

                // Text border color
                description.style.borderTopColor = dialogue.TextBorderTop;
                description.style.borderRightColor = dialogue.TextBorderRight;
                description.style.borderBottomColor = dialogue.TextBorderBottom;
                description.style.borderLeftColor = dialogue.TextBorderLeft;

                // Conversation dialogue alignment
                if (currentIndex % 2 != 0 && currentConversation.StartDialogueOnLeft)
                {
                    body.Q<VisualElement>("Body").style.flexDirection = FlexDirection.RowReverse;
                    description.style.borderTopLeftRadius = DefaultBorderRadius;
                    description.style.borderTopRightRadius = 0;
                    caption.style.alignSelf = Align.FlexEnd;
                }
                else if (currentIndex % 2 == 0 && !currentConversation.StartDialogueOnLeft)
                {
                    body.Q<VisualElement>("Body").style.flexDirection = FlexDirection.RowReverse;
                    description.style.borderTopLeftRadius = DefaultBorderRadius;
                    description.style.borderTopRightRadius = 0;
                    caption.style.alignSelf = Align.FlexEnd;
                }

                currentIndex++;
                body.AddToClassList("body");
                content.Add(bodyElement);
            }
        }

        private IEnumerator InteractionsRoutine()
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
        }

        #endregion
    }
}
