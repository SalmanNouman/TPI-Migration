using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.DLX
{
    public class TpiTableEntry
    {
        /// <summary>
        /// The table which the entry is contained in.
        /// </summary>
        public TpiTable Owner { get; private set; }

        /// <summary>
        /// The parent category of the table entry.
        /// </summary>
        public TpiTableCategory Category { get; private set; }

        // List of elements
        private List<TpiTableElement> elements = new List<TpiTableElement>();

        /// <summary>
        /// The list of elements contained inside the entry.
        /// </summary>
        public IEnumerable<TpiTableElement> Elements => elements;

        // UI references
        internal VisualElement entryElement;
        internal Button deleteButton;
        private VisualElement elementHolder;
        private VisualElement deleteButtonContainer;
        private VisualElement entryBackground;

        // Custom style variables
        private CustomStyleProperty<Color> backgroundColour1Property = new CustomStyleProperty<Color>("--entry-background-colour-1");
        private CustomStyleProperty<Color> backgroundColour2Property = new CustomStyleProperty<Color>("--entry-background-colour-2");
        private Color backgroundColour1;
        private Color backgroundColour2;

        // Sets the owner table of the entry
        internal void SetOwner(TpiTable owner)
        {
            Owner = owner;
        }

        // Sets the owner category of the entry
        internal void SetCategory(TpiTableCategory category)
        {
            Category = category;
        }

        // Sets the UI references for the entry  
        internal void SetUIReferences(VisualElement entryElement, VisualElement elementHolder, Button deleteButton)
        {
            this.entryElement = entryElement;
            this.elementHolder = elementHolder;
            this.deleteButton = deleteButton;
            deleteButtonContainer = this.deleteButton.parent;

            entryBackground = entryElement.Q("Entry");
            entryElement.RegisterCallback<GeometryChangedEvent>(RefreshBackgroundColours);
            entryBackground.RegisterCallback<CustomStyleResolvedEvent>(RefreshBackgroundColours);
        }

        internal void GenerateElements()
        {
            for (int i = 0; i < Owner.ColumnCount; i++)
            {
                AddElement();
            }
        }

        // Adds new element to the entry
        internal void AddElement()
        {
            VisualElement tableElement = Owner.ElementTemplate.CloneTree();
            tableElement.style.width = Length.Percent(100f);
            VisualElement elementIcon = tableElement.Q("Icon");
            Button elementButton = tableElement.Q<Button>("Label");
            elementButton.SetEnabled(false);
            elementHolder.Add(tableElement);

            TpiTableElement newElement = new TpiTableElement();
            newElement.SetOwner(Owner);
            newElement.SetEntry(this);
            newElement.SetUIReferences(elementButton, elementIcon);
            elements.Add(newElement);

            deleteButtonContainer.BringToFront();
        }

        // Removes element from the entry
        internal void RemoveElement(int elementIndex)
        {
            elementHolder.RemoveAt(elementIndex);
            elements.RemoveAt(elementIndex);
        }

        /// <summary>
        /// Returns the first element found with the specified text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public TpiTableElement FindElementByText(string text)
        {
            return elements.Find(e => e.Text == text);
        }

        /// <summary>
        /// Removes the entry from its parent category.
        /// </summary>
        public void RemoveFromCategory()
        {
            Category.RemoveEntry(this);
            entryElement.RemoveFromHierarchy();

            entryElement.UnregisterCallback<GeometryChangedEvent>(RefreshBackgroundColours);
            entryBackground.UnregisterCallback<CustomStyleResolvedEvent>(RefreshBackgroundColours);
        }

        // Refreshes background colours based on the position in hierarchy and theme
        private void RefreshBackgroundColours(EventBase evt)
        {
            int index = entryElement.parent.IndexOf(entryElement);
            if (index == -1)
            {
                return;
            }

            if (index % 2 == 1)
            {
                if (entryBackground.customStyle.TryGetValue(backgroundColour1Property, out backgroundColour1))
                {
                    entryBackground.style.backgroundColor = backgroundColour1;
                }
            }
            else
            {
                if (entryBackground.customStyle.TryGetValue(backgroundColour2Property, out backgroundColour2))
                {
                    entryBackground.style.backgroundColor = backgroundColour2;
                }
            }
        }
    }
}

