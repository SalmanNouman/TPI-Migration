using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///     Manages the pause window UI and its functionality.
    /// </summary>
    public class PauseWindowBuilder : MonoBehaviour, IUserInterface
    {
        #region Fields

        // Root visual element from the UI document that contains all UI elements
        private VisualElement root;

        // UI Buttons
        private Button continueButton;
        private Button restartButton;

        #endregion

        #region Events

        [Header("Unity Events")]

        /// <summary>
        ///     Event triggered when the window is shown.
        ///     <see cref="PointClickNavigation.EnableNavigation(false)"/>
        ///     <see cref="PointClickNavigation.EnableCameraPanAndZoom(false)"/>
        ///     <see cref="BlurBackground.Volume.enabled(true)"/>
        ///     <see cref="MenuBuilder.Hide"/>
        ///     <see cref="TimerManager.PauseTimers"/>
        /// </summary>
        public UnityEvent OnWindowShow;

        /// <summary>
        ///     Event triggered when the window is hidden.
        ///     <see cref="PointClickNavigation.EnableNavigation(true)"/>
        ///     <see cref="PointClickNavigation.EnableCameraPanAndZoom(true)"/>
        ///     <see cref="BlurBackground.Volume.enabled(false)"/>
        ///     <see cref="MenuBuilder.Show"/>
        ///     <see cref="TimerManager.StartTimers"/>
        /// </summary>
        public UnityEvent OnWindowHide;

        /// <summary>
        ///     Event triggered when the restart button is clicked.
        ///     Will be linked to scene manager's restart functionality.
        /// </summary>
        public UnityEvent OnRestartScene;

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
            OnRestartScene ??= new();
        }

        /// <summary>
        ///     Sets up UI references, button listeners, and ensures window is hidden when scene starts.
        /// </summary>
        private void Start()
        {
            GetAllReferences();
            SetupButtonListeners();
            Hide();
        }

        /// <summary>
        ///     Gets references to all UI elements from the root visual element.
        /// </summary>
        private void GetAllReferences()
        {
            continueButton = root.Q<Button>("Continue");
            restartButton = root.Q<Button>("Restart");
        }

        /// <summary>
        ///     Sets up click event listeners for the continue and restart buttons.
        /// </summary>
        private void SetupButtonListeners()
        {
            // Continue button simply hides the pause window
            continueButton.clicked += Hide;

            // Restart button hides the pause window and triggers scene restart
            restartButton.clicked += () =>
            {
                Debug.Log("Restart button clicked - Starting from beginning");
                Hide();
                OnRestartScene?.Invoke();
            };
        }

        /// <summary>
        ///     Shows the pause window and notifies listeners through OnWindowShow event.
        /// </summary>
        public void Show()
        {
            UIHelper.Show(root);
            OnWindowShow?.Invoke();
        }

        /// <summary>
        ///     Hides the pause window and notifies listeners through OnWindowHide event.
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(root);
            OnWindowHide?.Invoke();
        }

        #endregion
    }
}