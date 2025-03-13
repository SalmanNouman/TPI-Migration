using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace VARLab.DLX
{
    /// <summary>
    ///     Builds the content inside the inspection tab.
    ///     Data is pulled from the Inspections component and displayed in a Velcro Table.
    /// </summary>
    public class InspectionLogBuilder : TabContentBuilder
    {
        [SerializeField]
        private TpiTable table;

        [SerializeField]
        private Sprite compliantIcon;

        [SerializeField]
        private Sprite nonCompliantIcon;
        /// <summary>
        ///     The list of inspection data records.
        /// </summary>
        private List<InspectionData> inspectionList;

        /// <summary>
        ///     Flag to check if the empty log message has been displayed.
        /// </summary>
        private bool isEmptyLogMessageDisplayed = false;

        public UnityEvent<string> DeleteInspection;
        public UnityEvent<string> DisplayPopUp;

        /// <summary>
        /// Updates the local inspection list with the provided data.
        /// </summary>
        /// <param name="inspections">A list of inspection data to be displayed.</param>
        public void GetInspectionList(List<InspectionData> inspections)
        {
            inspectionList ??= new List<InspectionData>();
            inspectionList = inspections;
        }

        /// <summary>
        /// Refreshes and displays the inspection log.
        /// This method updates the inspection data from the Inspections component,
        /// logs the current count, and then populates the table with the latest information.
        /// </summary>
        public void HandleDisplayInspectionLog()
        {
            //check if list is empty or null
            if (inspectionList == null || inspectionList.Count <= 0)
            {
                // check if the empty log message has not been displayed yet
                if (!isEmptyLogMessageDisplayed)
                {
                    DisplayEmptyLogMessage();
                    isEmptyLogMessageDisplayed = true;
                }
                return;
            }

            //if the list is not empty, check if the empty log message has been displayed
            if (isEmptyLogMessageDisplayed)
            {
                //hide the empty log message
                HideEmptyLogMessage();
                isEmptyLogMessageDisplayed = false;
            }
            //populate the table with the inspection data.
            PopulateInspectionTable();
        }

        /// <summary>
        /// Populates the Velcro Table with inspection data grouped by location.
        /// It creates table columns, adds categories for each location, and fills each row with inspection details.
        /// </summary>
        private void PopulateInspectionTable()
        {
            ContentContainer.Add(table.Root);
            table.Root.style.flexGrow = 1;
            ContentContainer.Q<TemplateContainer>().style.flexGrow = 1;
            ContentContainer.Q<VisualElement>("unity-content-container").AddToClassList("grow");
            ContentContainer.Q<VisualElement>("unity-content-container").AddToClassList("h-100");

            // Define the column headers for the table.
            string[] columns = { "Location", "Item", "Compliancy", "Info" };
            table.HandleDisplayUI(columns);

            // Group inspections by Location.
            var groupedInspections = inspectionList.GroupBy(i => i.Obj.Location.ToString());

            // For each location group, create a category and add entries.
            foreach (var group in groupedInspections)
            {
                //Add a new category to the table for this location.
                table.AddCategory(group.Key);
                TpiTableCategory category = table.FindCategoryByName(group.Key);

                //subscribed to row removal event
                category.OnEntryRemoved.AddListener(OnRowRemoved);

                // Loop through each inspection in this group.
                foreach (var inspection in group)
                {
                    // Add a new row/entry in the category.
                    category.AddEntry();
                    // Get the entry that was just added.
                    var entry = category.Entries.Last();

                    // Set the text for each column in the row:
                    // Column 0: Location
                    entry.Elements.ElementAt(0).Text = inspection.Obj.Location.ToString();

                    // Column 1: Item.
                    entry.Elements.ElementAt(1).Text = inspection.Obj.Name.ToString();

                    // Column 2: Compliancy status
                    entry.Elements.ElementAt(2).Text = inspection.IsCompliant ? " " : " ";
                    entry.Elements.ElementAt(2).Icon = inspection.IsCompliant ? compliantIcon : nonCompliantIcon;

                    // Column 3: Info – if a photo exists, show a "View Photo" link, otherwise indicate no photo.
                    entry.Elements.ElementAt(3).Text = inspection.HasPhoto ? "View photo" : "----";
                    if (inspection.HasPhoto)
                    {
                        entry.Elements.ElementAt(3).Button.SetEnabled(true);
                        entry.Elements.ElementAt(3).Button.clicked += () => DisplayPopUPUI(inspection.Obj.ObjectId);
                    }
                }
            }
        }

        public void DisplayPopUPUI(string objectId)
        {
            DisplayPopUp?.Invoke(objectId);
        }

        /// <summary>
        /// Handles the removal of a row from the table.
        /// Extracts the location and name from the removed entry, constructs an object ID,
        /// and invokes the DeleteInspection event to remove the corresponding inspection.
        /// </summary>
        /// <param name="removedEntry">The TableEntry that was removed.</param>
        private void OnRowRemoved(TpiTableEntry removedEntry)
        {
            string location = removedEntry.Elements.ElementAt(0).Text;
            string name = removedEntry.Elements.ElementAt(1).Text;
            Debug.Log($"Row removed location: {location}");
            Debug.Log($"Row removed name: {name}");

            //create objectId from retrieval
            string objectId = $"{location}_{name}";

            //Deletes the inspection from the inspection list.
            DeleteInspection?.Invoke(objectId);

            //// Log for debugging
            Debug.Log($"Row removed for object: {objectId}");

        }
    }
}
