using UnityEngine;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///  Timers have no customization, but 2 different starting states available:
    ///     - Start Open: Whether the timer starts expanded/open or collapsed/closed
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CountupTimer : MonoBehaviour, IUserInterface
    {
        [HideInInspector] public VisualElement Root { private set; get; }

        private VisualElement timerContainer;
        private VisualElement arrow;
        private Button arrowBtn;
        private Label elapsedTimeLabel;

        private const string ClosedClassName = "timer-closed";
        private const string ArrowClosedClassName = "timer-arrow-closed";

        /// <summary>
        /// Whether the timer is expanded or not
        /// </summary>
        public bool IsOpen { get; private set; } = false;

        private void Start()
        {
            Root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            elapsedTimeLabel = Root.Q<Label>("ElapsedTimeLabel");
            timerContainer = Root.Q("CountupTimer");
            arrowBtn = Root.Q<Button>("ArrowBtn");
            arrow = Root.Q("Arrow");

            arrowBtn.clicked += () =>
            {
                ToggleTimer();
            };

            Show();
        }

        /// <summary>
        /// Add delta time to elapsed time if its not paused
        /// </summary>
        private void Update()
        {
            if (IsOpen)
            {
                SetUpTimer();
            }
        }

        /// <summary>
        /// Sets the timer container and arrow to its open/closed state
        ///     - Closed: Arrow pointing left with body hidden
        ///     - Opened: Arrow pointing right with body shown
        /// </summary>
        public void ToggleTimer()
        {
            timerContainer.ToggleInClassList(ClosedClassName);
            arrow.ToggleInClassList(ArrowClosedClassName);
            IsOpen = !IsOpen;
        }

        /// <summary>
        /// Shows the root of the timer and triggers OnTimerShown
        /// </summary>
        public void Show()
        {
            UIHelper.Show(Root);
        }

        /// <summary>
        /// Hides the root of the timer and triggers OnTimerHidden
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(Root);
        }

        /// <summary>
        /// Sets up the timer by updating the elapsed time label with the formatted total seconds from the TimerManager.
        /// </summary>
        public void SetUpTimer()
        {
            UIHelper.SetElementText(elapsedTimeLabel, TimerManager.Instance.GetElapsedTime());
        }

    }
}