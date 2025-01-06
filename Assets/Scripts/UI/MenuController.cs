using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace VARLAB.DLX
{
    public abstract class MenuController : MonoBehaviour
    {
        public UIDocument Document;
        public VisualElement Root;
        public bool OpenOnStart = false;

        [Header("Menu Events")]

        /// <summary> Invoked when the window is opened </summary>
        public UnityEvent Opened = new();

        /// <summary> Invoked when the window is closed </summary>
        public UnityEvent Closed = new();

        /// <summary> Indicates the current state of the menu </summary>
        public bool IsOpen => Root != null && Root.style.display == DisplayStyle.Flex;

        public virtual void OnValidate()
        {
            if (!Document)
            {
                Document = GetComponent<UIDocument>();
            }
        }

        public virtual void Start()
        {
            if (!Document)
            {
                Document = GetComponent<UIDocument>();
            }

            Root = Document.rootVisualElement;

            // Set display based on initial IsOpen property
            Display(OpenOnStart);

            Initialize();
        }

        /// <summary>
        ///      Configures visual elements and their interactions
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        ///     Sets the current display state according to the <paramref name="enabled"/> parameter
        /// </summary>
        /// <param name="enabled">Indicates whether or not the window should be shown</param>
        public virtual void Display(bool enabled)
        {
            if (enabled) { Open(); }
            else { Close(); }
        }

        /// <summary>
        ///     Toggles the current state of display
        /// </summary>
        public virtual void Toggle() => Display(!IsOpen);

        public virtual void Open()
        {
            Root.style.display = DisplayStyle.Flex;
            Opened?.Invoke();
        }

        public virtual void Close()
        {
            Root.style.display = DisplayStyle.None;
            Closed?.Invoke();
        }
    }
}