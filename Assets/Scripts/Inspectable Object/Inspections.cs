using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.DLX
{
    /// <summary>
    ///     Manages a list of inspections, allowing adding, updating, retrieving, and deleting inspection records.
    /// </summary>
    public class Inspections : MonoBehaviour
    {
        #region Events

        /// <summary>
        ///     Invoked when an inspection is made.
        ///     Returns the inspections list count.
        /// <see cref="ProgressBuilder.GetInspectionsCount(List{InspectionData})"/>
        /// <see cref="InspectionLogBuilder.GetInspectionList(List{InspectionData})"/>
        /// </summary>
        public UnityEvent<List<InspectionData>> OnInspectionCompleted;


        /// <summary>
        ///     Invoked when inspection record is requested.
        /// <see cref="InspectionWindowBuilder.UpdateInspectionLabel(InspectionData)"/>
        /// </summary>
        public UnityEvent<InspectionData> OnPreviousInspectionRetrieved;

        #endregion

        #region Properties

        /// <summary>
        ///     List of objects each representing an inspection.
        /// </summary>
        public List<InspectionData> InspectionsList { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Initialize events and collection if they are null.
        /// </summary>
        private void Awake()
        {
            OnInspectionCompleted ??= new();
            OnPreviousInspectionRetrieved ??= new();
            InspectionsList ??= new();
        }

        /// <summary>
        ///     Checks if an inspection exists for the given obj and returns it.
        /// </summary>
        /// <param name="obj">The inspectable object to look for</param>
        /// <returns>Returns the InspectionData if found, otherwise null.</returns>
        public InspectionData CheckInspection(string obj)
        {
            // Searches the list for InspectionData where obj.ObjectId matches the given obj.ObjectId
            var inspection = InspectionsList.Find(i => i.Obj.ObjectId == obj);
            //Debug.Log($"[CheckInspection] Checking {inspection.Obj.Name} (ID: {inspection.Obj.ObjectId}), Found: {inspection != null}");
            return inspection;
        }

        /// <summary>
        ///     Retrieves the previous inspection record for the given object and delivers it through an event.
        ///     Invoked by <see cref="InspectionHandler.OnObjectClicked(InspectableObject)"/>
        /// </summary>
        /// <param name="obj">The inspectable object to retrieve previous inspection data for.</param>
        public void RetrievePreviousInspection(InspectableObject obj)
        {
            var inspection = CheckInspection(obj.ObjectId);
            OnPreviousInspectionRetrieved?.Invoke(inspection);
        }

        /// <summary>
        ///     Adds a new inspection or updates an existing one if it already exists.
        /// </summary>
        /// <param name="newInspection">The inspection data to add or update</param>
        public void AddInspection(InspectionData newInspection)
        {
            // Find existing inspection for the same obj
            var existingInspection = InspectionsList.Find(i => i.Obj.ObjectId == newInspection.Obj.ObjectId);

            if (existingInspection != null) // If inspection exists / already has an inspection recorded
            {
                Debug.Log($"[AddInspection] Found existing inspection for {existingInspection.Obj.Name}.");

                // If compliance status changed, update the record
                if (existingInspection.IsCompliant != newInspection.IsCompliant)
                {
                    Debug.Log($"[AddInspection] Updated compliance state from {existingInspection.IsCompliant} to {newInspection.IsCompliant}.");
                    existingInspection.IsCompliant = newInspection.IsCompliant;
                }

                // If photo status changed, update the record
                if (!existingInspection.HasPhoto && newInspection.HasPhoto)
                {
                    Debug.Log($"[AddInspection] Updated photo state from {existingInspection.HasPhoto} to {newInspection.HasPhoto}.");
                    existingInspection.HasPhoto = newInspection.HasPhoto;
                }
            }
            else // If no inspection exists, it adds a new one
            {
                Debug.Log($"[AddInspection] No existing inspection found. Adding new inspection for {newInspection.Obj.Name}.");
                InspectionsList.Add(newInspection);
            }

            OnInspectionCompleted?.Invoke(InspectionsList);
            Debug.Log($"[Inspection details] Compliant: {newInspection.IsCompliant}, HasPhoto: {newInspection.HasPhoto}");
            Debug.Log($"[AddInspection] Total inspections in record: {InspectionsList.Count}");
        }

        /// <summary>
        ///     Removes an inspection record for the given object if it exists.
        /// </summary>
        /// <param name="obj">The inspectable object whose inspection should be removed.</param>
        public void DeleteInspection(string obj)
        {
            var inspectionToRemove = CheckInspection(obj);

            if (inspectionToRemove != null)
            {
                InspectionsList.Remove(inspectionToRemove);
                OnInspectionCompleted?.Invoke(InspectionsList);
            }
        }

        /// <summary>
        ///     Returns the complete list of inspection records.
        /// </summary>
        /// <returns>List of all inspection records.</returns>
        public List<InspectionData> GetInspectionsList()
        {
            return InspectionsList;
        }

        #endregion
    }
}