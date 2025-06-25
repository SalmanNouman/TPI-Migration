using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///     Manages the Welcome Back window UI and its functionality.
    /// </summary>
    /// <remarks>
    ///     This window is shown when a learner resumes a previous inspection session
    ///     to indicate which inspection area they were last working on.
    ///     
    ///     Flow: Continue button clicked → Saved data applied to scene → Player warped to last POI → 
    ///           <see cref="PoiHandler.OnWarpComplete"/> event → <see cref="SetLocationText"/> called → Window displays location
    ///     
    ///     UI Elements:
    ///     - Location pin icon with the last inspection area name
    ///     - Instruction message to review inspection log
    ///     - Button to close the window
    /// </remarks>
    public class WelcomeBackWindowBuilder : MonoBehaviour, IUserInterface
    {
        #region Fields

        // Root visual element from the UI document that contains all UI elements
        private VisualElement root;

        // UI Elements
        private Label locationText;
        private Button closeButton;

        #endregion

        #region Events

        [Header("Unity Events")]

        /// <summary>
        ///     Event invoked when the Welcome Back window is shown.
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
        ///     Event invoked when the Welcome Back window is hidden.
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
            locationText = root.Q<Label>("LocationText");
            closeButton = root.Q<Button>("Button");
        }

        /// <summary>
        ///     Connects close button click event to <see cref="Hide"/> method.
        /// </summary>
        private void SetupButtonListeners()
        {
            closeButton.clicked += () =>
            {
                Debug.Log("WelcomeBackWindow: Close button clicked - Hiding window");
                Hide();
            };
        }

        /// <summary>
        ///     Sets the location text from POI and shows the window.
        /// </summary>
        /// <remarks>
        ///     - Called from <see cref="PoiHandler.OnWarpComplete"/> event when player is warped to last POI.
        /// </remarks>
        /// <param name="poi">The POI that was warped to</param>
        public void SetLocationText(Poi poi)
        {
            if (poi != null)
            {
                string locationName = PoiList.GetPoiName(poi.SelectedPoiName.ToString());
                if (locationText != null)
                {
                    locationText.text = locationName;
                    Debug.Log($"WelcomeBackWindow: Location text set to: {locationName}");
                }
            }
            else
            {
                Debug.LogWarning("WelcomeBackWindow: POI parameter is null");
            }

            Show();
        }

        /// <summary>
        ///     Shows the window and invokes <see cref="OnWindowShow"/> event.
        /// </summary>
        public void Show()
        {
            UIHelper.Show(root);
            Debug.Log("WelcomeBackWindow: Window shown");
            OnWindowShow?.Invoke();
        }

        /// <summary>
        ///     Hides the window and invokes <see cref="OnWindowHide"/> event.
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(root);
            Debug.Log("WelcomeBackWindow: Hidden");
            OnWindowHide?.Invoke();
        }

        #endregion
    }
}