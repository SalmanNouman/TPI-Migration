using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    /// Simple class to manage the Download Confirmation window.
    /// No data is passed to this class, it is only shown after download is initiated.
    /// It can be hidden after clicking on the "OK" button.
    /// </summary>
    public class DownloadConfirmationBuilder : MonoBehaviour, IUserInterface
    {
        [SerializeField, Tooltip("Reference to the UI Document")]
        private UIDocument downloadConfirmationDoc;

        [Header("Events")]
        [Tooltip("Event invoked when the confirmation window is shown")]
        public UnityEvent OnShow;

        [Tooltip("Event invoked when the confirmation window is hidden")]
        public UnityEvent OnHide;

        // Root element
        private VisualElement root;

        // Reference to the primary button
        private Button primaryButton;

        private void Start()
        {
            // Initialize events
            OnShow ??= new UnityEvent();
            OnHide ??= new UnityEvent();

            // Get the root element
            root = downloadConfirmationDoc.rootVisualElement;

            // Get the primary button
            primaryButton = root.Q<Button>("Button");

            // Set up button listener
            primaryButton.clicked += HandlePrimaryButtonClicked;

            // Hide the window initially
            UIHelper.Hide(root);
        }

        /// <summary>
        /// Handles the primary button click (OK)
        /// </summary>
        private void HandlePrimaryButtonClicked()
        {
            Hide();
        }

        /// <summary>
        /// Displays the Download Confirmation window
        /// </summary>
        public void Show()
        {
            UIHelper.Show(root);
            OnShow?.Invoke();
        }

        /// <summary>
        /// Hides the Download Confirmation window
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(root);
            OnHide?.Invoke();
        }
    }
}
