using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///     Manages the StartInfoWindow UI and its functionality.
    /// </summary>
    /// <remarks>
    ///     The StartInfoWindow follows this flow:
    ///     1. Simulation starts → Start Info Window is initially hidden
    ///     2. Window is shown when <see cref="SaveDataSupport.OnFreshLoad"/> event is triggered in two cases:
    ///        - No valid save file exists
    ///        - Player chooses to restart (via <see cref="SaveDataSupport.OnLoadRestart"/> method)
    ///     3. <see cref="SaveDataSupport.OnFreshLoad"/> event calls <see cref="Show"/> to display welcome screen
    ///     4. Player clicks Begin button → <see cref="Hide"/> is called
    ///     5. <see cref="OnWindowHide"/> event is triggered → Re-enables navigation and starts the simulation
    /// </remarks>
    public class StartInfoWindowBuilder : MonoBehaviour, IUserInterface
    {
        #region Fields
        
        // Root visual element from the UI document that contains all UI elements
        private VisualElement root;

        // UI Buttons
        private Button beginButton;
        
        #endregion

        #region Events
        
        [Header("Unity Events")]
        /// <summary>
        ///     Event triggered when the welcome window is shown.
        /// </summary>
        /// <remarks>
        ///     Inspector connections:
        ///     - <see cref="PointClickNavigation.EnableNavigation(false)"/>
        ///     - <see cref="PointClickNavigation.EnableCameraPanAndZoom(false)"/>
        ///     - <see cref="MenuBuilder.Hide()"/>
        ///     - <see cref="BlurBackground.Volume.enabled(true)"/>
        ///     - <see cref="CountupTimer.Hide()"/>
        /// </remarks>
        public UnityEvent OnWindowShow;

        /// <summary>
        ///     Event triggered when the welcome window is hidden (after Begin button is clicked).
        /// </summary>
        /// <remarks>
        ///     Inspector connections:
        ///     - <see cref="PointClickNavigation.EnableNavigation(true)"/>
        ///     - <see cref="PointClickNavigation.EnableCameraPanAndZoom(true)"/>
        ///     - <see cref="MenuBuilder.Show()"/>
        ///     - <see cref="BlurBackground.Volume.enabled(false)"/>
        ///     - <see cref="CountupTimer.Show()"/>
        /// </remarks>
        public UnityEvent OnWindowHide;
        
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
            Hide();
        }

        /// <summary>
        ///     Gets references to all UI elements from the root visual element.
        /// </summary>
        private void GetAllReferences()
        {
            beginButton = root.Q<Button>("Button");
        }

        /// <summary>
        ///     Sets up click event listeners for the begin button.
        /// </summary>
        private void SetupButtonListeners()
        {
            beginButton.clicked += () =>
            {
                Debug.Log("StartInfoWindow: Begin button clicked - Starting simulation");
                Hide();
            };
        }

        /// <summary>
        ///     Shows the welcome window and notifies listeners through OnWindowShow event.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Called by: <see cref="SaveDataSupport.OnFreshLoad"/> when no valid save file exists 
        ///       or player chooses to restart
        /// </remarks>
        public void Show()
        {
            UIHelper.Show(root);
            Debug.Log("StartInfoWindow: Welcome screen shown");
            OnWindowShow?.Invoke();
        }

        /// <summary>
        ///     Hides the welcome window and notifies listeners through OnWindowHide event.
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(root);
            Debug.Log("StartInfoWindow: Welcome screen hidden");
            OnWindowHide?.Invoke();
        }

        #endregion
    }
}