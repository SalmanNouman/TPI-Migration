using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    public class TpiTableCategory
    {
        /// <summary>
        /// The table which the category is contained in.
        /// </summary>
        public TpiTable Owner { get; private set; }

        private string name;

        /// <summary>
        /// The name of the category.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                if (nameLabel != null)
                {
                    nameLabel.text = name;
                }
            }
        }

        // List of entries
        private List<TpiTableEntry> entries = new List<TpiTableEntry>();

        /// <summary>
        /// The list of entries contained inside the category.
        /// </summary>
        public IEnumerable<TpiTableEntry> Entries => entries;

        /// <summary>
        /// The amount of entries contained in the category.
        /// </summary>
        public int EntryCount => entries.Count;

        // UI references
        private VisualElement categoryElement;
        private Label nameLabel;
        internal VisualElement entryHolder;

        /// <summary>
        /// Invoked when a new entry has been added to the category.
        /// </summary>
        public UnityEvent<TpiTableEntry> OnEntryAdded;

        /// <summary>
        /// Invoked when an entry is removed from the category.
        /// </summary>
        public UnityEvent<TpiTableEntry> OnEntryRemoved;

        /// <summary>
        /// Table category constructor.
        /// </summary>
        public TpiTableCategory()
        {
            OnEntryAdded ??= new UnityEvent<TpiTableEntry>();
            OnEntryRemoved ??= new UnityEvent<TpiTableEntry>();
        }

        // Sets the owner table of the category
        internal void SetOwner(TpiTable owner)
        {
            Owner = owner;
        }

        // Sets the UI references for the category 
        internal void SetUIReferences(VisualElement categoryElement, Label nameLabel, VisualElement entryHolder)
        {
            this.categoryElement = categoryElement;
            this.nameLabel = nameLabel;
            this.entryHolder = entryHolder;
        }

        /// <summary>
        /// Adds a new entry to the category.
        /// </summary>
        public void AddEntry()
        {
            VisualElement newEntry = Owner.EntryTemplate.CloneTree();
            newEntry.name = $"Element {EntryCount + 1}";
            VisualElement elementHolder = newEntry.Q("Entry");
            Button deleteButton = elementHolder.Q<Button>("DeleteButton");
            entryHolder.Add(newEntry);

            TpiTableEntry entry = new TpiTableEntry();
            entry.SetOwner(Owner);
            entry.SetCategory(this);
            entry.SetUIReferences(newEntry, elementHolder, deleteButton);
            entry.GenerateElements();
            entries.Add(entry);

            // Delete button callback to Show the delete confirmation dialog
            deleteButton.RegisterCallback<ClickEvent>((evt) => { ShowDeleteConfirmationDialog(entry); });

            OnEntryAdded.Invoke(entry);
        }

        // Button callback
        private void RemoveEntryFromListAndUI(TpiTableEntry entry)
        {
            entry.RemoveFromCategory();
        }

        /// <summary>
        /// Removes all entries from the category.
        /// </summary>
        public void RemoveAllEntries()
        {
            for (int i = entries.Count - 1; i > -1; i--)
            {
                entries[i].RemoveFromCategory();
            }
        }

        /// <summary>
        /// Removes the category UI element from the table
        /// </summary>
        public void RemoveFromTable()
        {
            Owner.RemoveCategory(this);
            categoryElement.RemoveFromHierarchy();
        }

        // Internal use only
        // Used for the TableEntry.RemoveFromCategory() method.
        internal void RemoveEntry(TpiTableEntry entry)
        {
            entries.Remove(entry);
            OnEntryRemoved.Invoke(entry);

            entry.deleteButton.UnregisterCallback<ClickEvent>((evt) => { ShowDeleteConfirmationDialog(entry); });
        }

        /// <summary>
        /// Shows the delete confirmation dialog for the user to confirm deletion of the inspection log.
        public void ShowDeleteConfirmationDialog(TpiTableEntry removedEntry)
        {
            removedEntry.Owner.DeleteInspectionSO.SetPrimaryAction(() =>
            {
                RemoveEntryFromListAndUI(removedEntry);
                removedEntry.Owner.Notification = ScriptableObject.CreateInstance<NotificationSO>();
                removedEntry.Owner.Notification.NotificationType = NotificationType.Success;
                removedEntry.Owner.Notification.Alignment = Align.FlexStart;
                removedEntry.Owner.Notification.Message = "Inspection log deleted";
                removedEntry.Owner.OnDeleteInspectionLog?.Invoke(removedEntry.Owner.Notification);
            });
            removedEntry.Owner.OnShowDeleteInspectionDialog?.Invoke(removedEntry.Owner.DeleteInspectionSO);
        }
    }
}
