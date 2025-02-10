using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


namespace VARLab.DLX
{
    /// <summary>
    /// In this class we will find and manage all the inspectable objects in the scene.
    /// </summary>
    public class InspectionHandler : MonoBehaviour
    {
        private List<InspectableObject> inspectables;
        public UnityEvent<InspectableObject> OnObjectClicked;
        public UnityEvent<InspectableObject> OnInspectionCompleted;

        /// <summary>
        /// Initialize events if they are null
        /// </summary>
        private void Awake()
        {
            OnObjectClicked ??= new();
            OnInspectionCompleted ??= new();
        }

        /// <summary>
        /// Finds all inspectable objects in the scene and links their events.
        /// </summary>
        private void OnEnable()
        {
            inspectables = new();
            inspectables = FindObjectsByType<InspectableObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();

            LinkEvents();
        }

        /// <summary>
        ///     Links inspection events to all inspectable objects.
        /// </summary>
        private void LinkEvents()
        {
            foreach (var obj in inspectables)
            {
                obj.OnObjectClicked.AddListener(HandleInspectionStarted);
                obj.OnObjectInspected.AddListener(HandleInspectionCompleted);
            }
        }

        /// <summary>
        ///     Handles when an inspection starts and invokes the event.
        /// </summary>
        /// <param name="obj">The inspectable object that was clicked.</param>
        public void HandleInspectionStarted(InspectableObject obj) => OnObjectClicked?.Invoke(obj);

        /// <summary>
        ///     Handles when an inspection is completed and invokes the event.
        /// </summary>
        /// <param name="obj">The inspectable object whose inspection is completed.</param>
        public void HandleInspectionCompleted(InspectableObject obj) => OnInspectionCompleted?.Invoke(obj);
    }
}
