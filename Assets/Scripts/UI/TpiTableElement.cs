using UnityEngine;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    public class TpiTableElement
    {
        /// <summary>
        /// The table which the element is contained in.
        /// </summary>
        public TpiTable Owner { get; private set; }

        /// <summary>
        /// The entry which this element is contained in.
        /// </summary>
        public TpiTableEntry Entry { get; private set; }

        private string text;

        /// <summary>
        /// The text of the element.
        /// </summary>
        public string Text
        {
            get => text;
            set
            {
                text = value;
                if (button != null)
                {
                    button.text = text;
                }
            }
        }

        public Button Button
        {
            get => button;
            set
            {
                button = value;
            }
        }

        private Sprite icon;

        /// <summary>
        /// The icon of the element.
        /// </summary>
        public Sprite Icon
        {
            get => icon;
            set
            {
                icon = value;
                if (iconElement != null)
                {
                    if (icon != null)
                    {
                        iconElement.style.backgroundImage = new StyleBackground(icon);
                        UIHelper.Show(iconElement);
                    }
                    else
                    {
                        iconElement.style.backgroundImage = StyleKeyword.Null;
                        UIHelper.Hide(iconElement);
                    }
                }
            }
        }

        private Color textColour;

        /// <summary>
        /// The colour of the text.
        /// </summary>
        public Color TextColour
        {
            get => textColour;
            set
            {
                textColour = value;
                if (button != null)
                {
                    button.style.color = textColour;
                }
            }
        }

        // UI references
        private VisualElement iconElement;
        private Button button;

        // Sets the owner table of the element
        internal void SetOwner(TpiTable owner)
        {
            Owner = owner;
        }

        // Sets the owner entry of the element
        internal void SetEntry(TpiTableEntry entry)
        {
            Entry = entry;
        }

        // Sets the UI references for the table element
        internal void SetUIReferences(Button btn, VisualElement iconElement)
        {
            this.button = btn;
            this.iconElement = iconElement;
            text = button.text;
        }

        /// <summary>
        /// Reverts the colour of the text back to the theme's default colour.
        /// </summary>
        public void ClearTextColour()
        {
            if (button != null)
            {
                button.style.color = StyleKeyword.Null;
            }
        }
    }
}
