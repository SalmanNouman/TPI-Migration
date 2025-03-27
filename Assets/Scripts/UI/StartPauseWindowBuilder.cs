using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///     Manages the pause/welcome window UI and its functionality.
    ///     This window can function in two modes:
    ///     1. Pause mode - Shown when the game is paused, continue button just hides the window
    ///     2. Welcome mode - Shown at the start, continue button loads saved game data
    /// </summary>
    public class StartPauseWindowBuilder : MonoBehaviour, IUserInterface
    {
        #region Fields

        // Root visual element from the UI document that contains all UI elements
        private VisualElement root;

        // UI Elements
        private Button continueButton;
        private Button restartButton;
        private Label headerLabel;

        // Window mode
        private bool isWelcomeMode = false;

        #endregion

        #region Events

        [Header("Unity Events")]

        /// <summary>
        ///     Event triggered when the window is shown.
        ///     <see cref="PointClickNavigation.EnableNavigation(false)"/>
        ///     <see cref="PointClickNavigation.EnableCameraPanAndZoom(false)"/>
        ///     <see cref="BlurBackground.Volume.enabled(true)"/>
        ///     <see cref="MenuBuilder.Hide"/>
        /// </summary>
        public UnityEvent OnWindowShow;

        /// <summary>
        ///     Event triggered when the window is shown in pause mode.
        ///     <see cref="TimerManager.PauseTimers"/>
        /// </summary>
        public UnityEvent OnPauseWindowShow;

        /// <summary>
        ///     Event triggered when the window is hidden.
        ///     <see cref="PointClickNavigation.EnableNavigation(true)"/>
        ///     <see cref="PointClickNavigation.EnableCameraPanAndZoom(true)"/>
        ///     <see cref="BlurBackground.Volume.enabled(false)"/>
        ///     <see cref="MenuBuilder.Show"/>
        /// </summary>
        public UnityEvent OnWindowHide;

        /// <summary>
        ///     Event triggered when the window is hidden from pause mode.
        ///     <see cref="TimerManager.StartTimers"/>
        /// </summary>
        public UnityEvent OnPauseWindowHide;

        /// <summary>
        ///     Event triggered when the restart button is clicked.
        /// </summary>
        /// <remarks>
        ///     Inspector connections:
        ///     - <see cref="SaveDataSupport.OnLoadRestart"/> to delete save file and restart scene
        /// </remarks>
        public UnityEvent OnRestartScene;

        /// <summary>
        ///     Event triggered when the continue button is clicked in welcome mode.
        ///     <see cref="SaveDataSupport.OnLoad"/>
        /// </summary>
        public UnityEvent OnContinueSavedGame;

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
            OnContinueSavedGame ??= new();
            OnPauseWindowShow ??= new();
            OnPauseWindowHide ??= new();
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

            headerLabel = root.Q<Label>("NameLabel");
        }

        /// <summary>
        ///     Sets up click event listeners for the continue and restart buttons.
        /// </summary>
        private void SetupButtonListeners()
        {
            // Continue button behavior depends on the window mode
            continueButton.clicked += () => 
            {
                if (isWelcomeMode)
                {
                    Debug.Log("Continue button clicked in welcome mode - Loading saved game");
                    OnContinueSavedGame?.Invoke();
                }
                else
                {
                    // invoke pause-specific events when in pause mode
                    OnPauseWindowHide?.Invoke();
                }
                
                Hide();
            };

            // Restart button hides the window and triggers scene restart
            restartButton.clicked += () =>
            {
                Debug.Log("Restart button clicked - Starting from beginning");
                Hide();
                OnRestartScene?.Invoke();
                // This will go through the same event for Loading a fresh game
            };
        }

        /// <summary>
        ///     Shows the window in pause mode and notifies listeners through OnWindowShow event.
        /// </summary>
        public void Show()
        {
            isWelcomeMode = false;
            headerLabel.text = "Pause";
            UIHelper.Show(root);
            OnWindowShow?.Invoke();
            
            // invoke pause-specific events when in pause mode
            OnPauseWindowShow?.Invoke();
        }

        /// <summary>
        ///     Shows the window in welcome mode and notifies listeners through OnWindowShow event.
        /// </summary>
        public void ShowAsWelcome()
        {
            isWelcomeMode = true;
            headerLabel.text = "Welcome";
            UIHelper.Show(root);
            OnWindowShow?.Invoke();
        }

        /// <summary>
        ///     Hides the window and notifies listeners through OnWindowHide event.
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(root);
            OnWindowHide?.Invoke();
        }

        #endregion
    }
}