using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.DLX
{
    /// <summary>
    /// In this class we will find and manage all of the POIs in the scene.
    /// </summary>
    public class PoiHandler : MonoBehaviour
    {
        // List containing all the POIs in the scene
        private List<Poi> pois;

        // number of POIs that have been interacted with and total number of interactable POIs
        private int poisInteracted = 0;
        private int interactablePois = 0;

        /// <summary>
        ///     Tracks the player's current POI location.
        /// </summary>
        private Poi currentPoi;

        /// <summary>
        ///     Flag to prevent automatic POI enter events when the game is first loaded.
        /// </summary>
        private bool isInitialLoad = true;

        /// <summary>
        /// This event is linked to the <see cref="Poi.OnPoiEnter"/> event and
        /// is invoked every time the poi event is invoked.
        /// <see cref="IntroductionTask.HandlePoiEnter"/>
        /// </summary>
        public UnityEvent<Poi> OnPoiEnter;

        /// <summary>
        /// This event is linked to the <see cref="Poi.OnPoiExit"/> event and
        /// is invoked every time the poi event is invoked.
        /// <see cref=""/>
        /// </summary>
        public UnityEvent<Poi> OnPoiExit;

        /// <summary>
        /// Invoked every time a inspectable object is clicked
        /// <see cref="ProgressBuilder.UpdateInspectedPois(int)"/>
        /// </summary>
        public UnityEvent<int> PoiInteracted;

        /// <summary>
        /// Invoked on start to get the total number of inspectable pois
        /// <see cref="ProgressBuilder.GetPoiCount(int)"/>
        /// </summary>
        public UnityEvent<int> OnStart;


        /// <summary>
        /// Initialize events if they are null
        /// </summary>
        private void Awake()
        {
            OnPoiEnter ??= new();
            OnPoiExit ??= new();
            PoiInteracted ??= new();
            OnStart ??= new();
        }

        private void Start()
        {
            // runs on start to get the total number of POIs that contain inspectables
            InteractablePoiCount();

            OnStart?.Invoke(interactablePois);
            PoiInteracted?.Invoke(poisInteracted);

            // Wait a short time before allowing POI enter events to be processed
            // This prevents automatic POI enter events when the game is first loaded
            Invoke("EnablePoiEvents", 1.0f);
        }

        /// <summary>
        /// Enables POI enter events after initial load is complete
        /// </summary>
        private void EnablePoiEvents()
        {
            isInitialLoad = false;
        }

        /// <summary>
        /// Runs before Start and gets a list of all the pois in the scene and links the events.
        /// </summary>
        private void OnEnable()
        {
            pois = new();
            pois = FindObjectsByType<Poi>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();

            LinkEvents();
        }

        /// <summary>
        /// Links the events from the poi with the handler events.
        /// </summary>
        private void LinkEvents()
        {
            foreach (var poi in pois)
            {
                poi.OnPoiEnter.AddListener(HandlePoiEnter);
                poi.OnPoiExit.AddListener(HandlePoiExit);
            }
        }

        /// <summary>
        /// Handles when the POI is entered and invokes the event.
        /// Updates currentPoi to track player location.
        /// </summary>
        /// <param name="poi">The POI that was entered.</param>
        public void HandlePoiEnter(Poi poi)
        {
            // Skip POI enter events during initial load
            if (isInitialLoad)
            {
                currentPoi = poi; // Still update current POI
                return;
            }

            currentPoi = poi; // Update current POI
            OnPoiEnter?.Invoke(poi);
        }

        /// <summary>
        /// Handles when the POI is exited and invokes the event.
        /// </summary>
        /// <param name="poi">The POI that was exited.</param>
        public void HandlePoiExit(Poi poi) => OnPoiExit?.Invoke(poi);

        /// <summary>
        /// Checks the POI of the object that was clicked and if it has
        /// not been interacted it will increment the POI interacted counter
        /// and invoke the POI Interacted event. This will update the progress
        /// indicator in the inspection review window.
        /// </summary>
        /// <param name="obj">Inspectable object clicked.</param>
        public void CheckPoiInteracted(InspectableObject obj)
        {
            var poi = pois.Find(p => p.SelectedPoiName.ToString() == obj.Location.ToString());
            if (poi.Interacted)
            {
                return;
            }
            poi.Interacted = true;
            poisInteracted++;
            PoiInteracted?.Invoke(poisInteracted);
        }

        /// <summary>
        /// Checks the total number of POIs that contain inspectables.
        /// </summary>
        private void InteractablePoiCount()
        {
            foreach (var poi in pois)
            {
                if (poi.HasInspectables)
                {
                    interactablePois++;
                }
            }
        }
    }
}
