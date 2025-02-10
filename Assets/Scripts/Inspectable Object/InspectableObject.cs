using EPOOutline;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VARLab.Interactions;

namespace VARLab.DLX
{
    /// <summary>
    ///     This is the class that is used to make objects inspectable. It contains all the information needed to inspect an object. 
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(Outlinable))]
    [RequireComponent(typeof(Collider))]

    public abstract class InspectableObject : MonoBehaviour
    {
        public string Name;
        [SerializeField, Tooltip("Inspectable Object POI")]
        private PoiList.PoiName location;
        [SerializeField, Tooltip("Reference to Inspectable Object Camera")]
        private Camera cam;
        private Compliancy currentObjectState;
        [Tooltip("List of Inspectable Object State")]
        public List<State> States;
        private string objectId;
        public bool HasPhoto;
        public bool Interacted;

        [Header("Inspectable Object event"), Space(5f)]

        public UnityEvent<InspectableObject> OnObjectClicked;
        public UnityEvent<InspectableObject> OnObjectInspected;

        /// <summary>
        /// Start is called before the first frame update.
        /// Ensures the camera is properly set up.
        /// </summary>
        void Start()
        {
            if (cam == null)
            {
#if UNITY_EDITOR
                Debug.Log("No Camera set for an inspectable ");
#endif
                return;
            }
            //and a guard to prevent forgetting to turn off the cameras in the editor
            else if (cam.enabled == true)
            {
                cam.enabled = false;
            }
        }

        /// <summary>
        /// Called when the object is instantiated.
        /// Initializes UnityEvents if they are null.
        /// </summary>
        private void Awake()
        {
            OnObjectClicked ??= new();
            OnObjectInspected ??= new();
        }

        /// <summary>
        /// Generates a unique object id for each inspectable
        /// </summary>
        /// <returns>Inspectable object ID or null if location or name are empty</returns>
        public string GeneratedId()
        {
            //if the location empty, do not generate ID and send debug to the user. 
            if ((location == PoiList.PoiName.None) || (string.IsNullOrEmpty(this.Name)))
            {
                Debug.Log("Invalid inspectable on " + this.Name + ", please set the location of the inspectable object");
                return null;
            }

            //Using the location and the name of the object to create a unique ID
            objectId = location.ToString() + "_" + Name;
            return objectId;

        }

        /// <summary>
        /// Returns a list of the current states of the object, based on the states list
        /// </summary>
        /// <returns>List of states for each inspectable object</returns>
        public List<string> GetListOfObjectStates()
        {
            List<string> objectStates = new List<string>();
            foreach (var state in States)
            {
                objectStates.Add(state.Compliancy.ToString());
            }
            return objectStates;
        }

        /// <summary>
        /// Marks the object as having a photo taken
        /// </summary>
        public void PhotoTaken()
        {
            if (!HasPhoto)
            {
                HasPhoto = true;
            }
        }

        /// <summary>
        /// Marks the object as NOT having a photo
        /// </summary>
        public void PhotoDelete()
        {
            HasPhoto = false;
        }
    }
}