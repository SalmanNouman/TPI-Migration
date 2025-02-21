using System.Collections.Generic;
using UnityEngine;

namespace VARLab.DLX
{
    /// <summary>
    ///     Manages a list of inspections, allowing adding, updating, retrieving, and deleting inspection records.
    /// </summary>
    public class Inspections : MonoBehaviour
    {
        //Properties
        public List<InspectionData> InspectionsList { get; set; } //list of objects each representing an inspection

        //Methods
        /// <summary>
        ///     Unity's Start method, initializes the inspection list.
        /// </summary>
        public void Start()
        {
            InspectionsList = new List<InspectionData>();
        }

        /// <summary>
        ///     Adds a new inspection or updates an existing one if it already exists.
        /// </summary>
        /// <param name="newInspection">The inspection data to add or update</param>
        public void AddInspection(InspectionData newInspection)
        {
            var existingInspection = InspectionsList.Find(i => i.Obj == newInspection.Obj);//find existing inspection for the same obj
            if (existingInspection != null) //if inspection exists / already has an inspection recorded
            {
                //if compliance status changed, update the record
                if (existingInspection.IsCompliant != newInspection.IsCompliant)
                {
                    existingInspection.IsCompliant = newInspection.IsCompliant;
                }
                if (!existingInspection.HasPhoto)
                {
                    existingInspection.HasPhoto = newInspection.HasPhoto;
                }
            }
            else //if no inspection exists it adds a new one
            {
                InspectionsList.Add(newInspection);
            }
        }

        /// <summary>
        ///     check if an inspection exists for the given obj and returns it.
        /// </summary>
        /// <param name="obj">The inspectable object to look for</param>
        /// <returns>Returns the InspectionData if found, otherwise null.</returns>
        public InspectionData CheckInspection(InspectableObject obj)
        {
            return InspectionsList.Find(i => i.Obj == obj); //searches the list for InspectionData where obj matches the given obj, if find an inspection return it. if not return null.
        }

        /// <summary>
        ///     Delete an inspection if it exists
        /// </summary>
        /// <param name="obj">The inspectable object whose inspection should be removed.</param>
        public void DeleteInspection(InspectableObject obj)
        {
            var inspectionToRemove = InspectionsList.Find(i => i.Obj == obj); //find inspection in the list
            if (inspectionToRemove != null) // if it exists remove it
            {
                InspectionsList.Remove(inspectionToRemove);
            }
        }

        public List<InspectionData> GetInspectionsList()
        {
            return InspectionsList;
        }
    }
}
