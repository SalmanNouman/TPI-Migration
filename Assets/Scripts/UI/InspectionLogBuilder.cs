using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///     Builds the content inside the inspection tab.
    ///     Data is pulled from the Inspections component and displayed in a Velcro Table.
    /// </summary>
    public class InspectionLogBuilder : TabContentBuilder
    {
        [SerializeField]
        private Table table;

        [SerializeField]
        private Sprite compliantIcon;

        [SerializeField]
        private Sprite nonCompliantIcon;
        /// <summary>
        ///     The list of inspection data records.
        /// </summary>
        private List<InspectionData> inspectionList;



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
                DisplayEmptyLogMessage();
                return;
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
                TableCategory category = table.FindCategoryByName(group.Key);

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
                    entry.Elements.ElementAt(3).Text = inspection.HasPhoto ? "View Photo" : "----";
                }
            }
        }
    }
}
