using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using VARLab.Interactions;
using VARLab.ObjectViewer;


namespace VARLab.DLX
{
    /// <summary>
    ///     In this class we will find and manage all the inspectable objects in the scene.
    /// </summary>
    public class InspectionHandler : MonoBehaviour
    {
        private List<InspectableObject> inspectables;

        /// <summary>
        ///     Unity Event that is triggered when an inspectable object is clicked.
        /// <see cref="ImageHandler.TakeTempPhoto(InspectableObject)"/>
        /// <see cref="InspectionWindowBuilder.HandleInspectionWindowDisplay(InspectableObject)"/>
        /// <see cref="PoiHandler.CheckPoiInteracted(InspectableObject)"/>
        /// <see cref="Inspections.OnInspectionRequested(InspectableObject)"/>
        /// </summary>
        public UnityEvent<InspectableObject> OnObjectClicked;

        /// <summary>
        ///     This gets invoked if the object clicked has object viewer.
        /// <see cref="ObjectViewerController.View"/>
        /// </summary>
        public UnityEvent<GameObject> OnObjectViewerObjectClicked;

        /// <summary>
        ///     Unity Event that is triggered when the inspection is completed.
        /// </summary>
        public UnityEvent<InspectableObject> OnInspectionCompleted;

        /// <summary>
        ///     Initialize events if they are null
        /// </summary>
        private void Awake()
        {
            OnObjectClicked ??= new();
            OnObjectViewerObjectClicked ??= new();
            OnInspectionCompleted ??= new();
        }

        /// <summary>
        ///     Finds all inspectable objects in the scene and links their events.
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

        /// <summary>
        ///     This is used to check if what was clicked is and inspectable object and
        ///     invokes the OnObjectClickedEvent
        ///     Invoked by <see cref="InteractionHandler.HandleMouseClick(GameObject)"/>
        /// </summary>
        /// <param name="obj">Game object that was clicked.</param>
        public void StartInspection(GameObject obj)
        {
            InspectableObject inspectable = obj.GetComponent<InspectableObject>();

            if (inspectable == null) { return; }

            if (inspectable.GetComponent<WorldObject>())
            {
                OnObjectViewerObjectClicked?.Invoke(obj);
            }

            OnObjectClicked?.Invoke(inspectable);
        }

        public void SetRemovedPhotoForInspectable(string id)
        {
            // Find the inspectable object with the given id
            InspectableObject inspectable = inspectables.Find(i => i.ObjectId == id);
            // If the inspectable object is found, set HasPhoto to false
            if (inspectable != null)
            {
                inspectable.PhotoDelete();
            }
        }
    }
}