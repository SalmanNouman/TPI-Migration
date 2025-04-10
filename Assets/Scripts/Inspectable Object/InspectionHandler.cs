using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using VARLab.Interactions;
using VARLab.ObjectViewer;
using static VARLab.DLX.SaveData;


namespace VARLab.DLX
{
    /// <summary>
    ///     In this class we will find and manage all the inspectable objects in the scene.
    /// </summary>
    public class InspectionHandler : MonoBehaviour
    {
        private List<InspectableObject> inspectables;

        // This will be set when handwashing task is completed.
        private bool HandWashingCompleted;

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
        ///     Unity Event that is triggered if handwashing task is found to be not completed after a click on an inspectable.
        /// </summary>
        public UnityEvent OnHandwashingTaskNotCompleted;

        /// <summary>
        ///     Event triggered when the learner loads from a saved file 
        ///     after the inspection list is populated from the save file.
        ///     <see cref="Inspections.GetSavedList(List{InspectionData})"/>
        /// </summary>
        public UnityEvent<List<InspectionData>> LoadInspectionLogList;

        /// <summary>
        ///     Event invoked when the learner loads from a saved file
        ///     <see cref="ImageHandler.TakePhotoForLoad(Dictionary{InspectableObject, string})"/>
        /// </summary>
        public UnityEvent<Dictionary<InspectableObject, string>> LoadSavedPhotos;

        /// <summary>
        ///     Initialize events if they are null
        /// </summary>
        private void Awake()
        {
            OnObjectClicked ??= new();
            OnObjectViewerObjectClicked ??= new();
            OnInspectionCompleted ??= new();
            OnHandwashingTaskNotCompleted ??= new();
            LoadInspectionLogList ??= new();
            LoadSavedPhotos ??= new();
        }

        /// <summary>
        ///     Finds all inspectable objects in the scene and links their events.
        /// </summary>
        private void OnEnable()
        {
            inspectables = new();
            inspectables = FindObjectsByType<InspectableObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();

            LinkEvents();

            foreach (var inspectableObject in inspectables)
            { 
                inspectableObject.GetComponent<Interactable>().enabled = false;
            }
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
            // Early exit if handwashing task not completed and invoke event.
            if (!HandWashingCompleted)
            {
                OnHandwashingTaskNotCompleted?.Invoke();
                return;
            }

            InspectableObject inspectable = obj.GetComponent<InspectableObject>();

            if (inspectable == null) { return; }

            if (inspectable.GetComponent<ObjectViewerInspectables>())
            {
                OnObjectViewerObjectClicked?.Invoke(obj);
            }

            if (inspectable.GetComponent<ToggleableInspectable>())
            {
                ToggleInspectable(inspectable);
            }

            OnObjectClicked?.Invoke(inspectable);
        }

        /// <summary>
        /// <see cref="ImageHandler.OnPhotoDeleted"/>
        /// </summary>
        /// <param name="id"></param>
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

        /// <summary>
        ///     This method is added as a listener of the <see cref="SaveDataSupport.LoadInspectionList"/>
        ///     to populate the inspection log from the save file.
        /// </summary>
        /// <param name="savedList"> list of inspections retrieved from the cloud </param>
        public void LoadInspectionLog(List<InspectionSaveData> savedList)
        {
            List<InspectionData> tempList = new();

            if (savedList == null || savedList.Count == 0)
            {
                Debug.LogWarning("No inspection data found in the save file.");
                return;
            }

            foreach (var data in savedList)
            {
                InspectableObject obj = inspectables.Find(o => o.ObjectId == data.ObjectId);
                if (obj != null)
                {
                    InspectionData tempData = new();
                    tempData.Obj = obj;
                    tempData.IsCompliant = data.IsCompliant;
                    tempData.HasPhoto = data.HasPhoto;

                    tempList.Add(tempData);
                }
            }

            LoadInspectionLogList?.Invoke(tempList);
        }

        public void LoadPhotos(Dictionary<string, string> photos)
        {
            Dictionary<InspectableObject, string> tempObjectsAndTimestamps = new();
            foreach(KeyValuePair<string, string> kvp in photos)
            {
                InspectableObject obj = inspectables.Find(o => o.ObjectId == kvp.Key);

                if (obj != null)
                { 
                    tempObjectsAndTimestamps.Add(obj, kvp.Value);
                    obj.HasPhoto = true;
                }
            }

            LoadSavedPhotos?.Invoke(tempObjectsAndTimestamps);
        }

        /// <summary>
        ///     Sets the handwashing task as completed, allowing inspections to proceed.
        /// </summary>
        public void SetHandwashingTaskCompleted()
        {
            HandWashingCompleted = true;
            SaveDataSupport.Instance.CanSave = true;
        }

        /// <summary>
        /// This will toggle the object in a toggle inspectable
        /// Example: open a lid for an inspection.
        /// </summary>
        /// <param name="obj"></param>
        public void ToggleInspectable(InspectableObject obj)
        {
            if (obj == null) 
            {
                Debug.LogWarning("Toggling an null InspectableObject");
                return;
            }

            if (obj.GetComponent<ToggleableInspectable>())
            {
                obj.GetComponent<ToggleableInspectable>().ToggleForInspection();
            }
        }

        /// <summary>
        /// Disables / Enables interactions on POI enter and exit.
        /// Only inspectables in the current POI should be interactable
        /// </summary>
        /// <param name="poi"></param>
        public void ToggleInteractions(Poi poi)
        {
            if (poi != null)
            {
                List<InspectableObject> poiInspectables = inspectables.FindAll(i => i.Location == poi.SelectedPoiName);

                foreach (InspectableObject obj in poiInspectables)
                {
                    obj.GetComponent<Interactable>().enabled = !obj.GetComponent<Interactable>().enabled;
                }
            }
        }
    }
}