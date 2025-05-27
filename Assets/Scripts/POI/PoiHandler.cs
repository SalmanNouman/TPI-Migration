using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace VARLab.DLX
{
    /// <summary>
    /// In this class we will find and manage all of the POIs in the scene.
    /// </summary>
    public class PoiHandler : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent player;

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
        /// This event is linked to the <see cref="Poi.OnPoiEnter"/> event and
        /// is invoked every time the poi event is invoked.
        /// <see cref="ActivityLog.LogPoiEnter(Poi)"/>
        /// <see cref="SaveDataSupport.SaveLastPOI(Poi)"/>
        /// <see cref="InspectionHandler.ToggleInteractions(Poi)"/>
        /// </summary>
        public UnityEvent<Poi> OnPoiEnter;

        /// <summary>
        /// This event is linked to the <see cref="Poi.OnPoiExit"/> event and
        /// is invoked every time the poi event is invoked.
        /// <see cref="ActivityLog.LogExitPoi(Poi)"/>
        /// <see cref="InspectionHandler.ToggleInteractions(Poi)"/>
        /// </summary>
        public UnityEvent<Poi> OnPoiExit;

        /// <summary>
        /// Invoked every time a inspectable object is clicked
        /// <see cref="ProgressBuilder.UpdateInspectedPois(int)"/>
        /// </summary>
        public UnityEvent<int> PoiInteracted;

        /// <summary>
        /// Invoked every time a inspectable object is clicked
        /// <see cref="InspectionSummaryBuilder.HandlePoiDataListReceived(List{Poi})"/>"
        /// </summary>
        public UnityEvent<List<Poi>> OnGetPoiList;

        /// <summary>
        /// Invoked on start to get the total number of inspectable pois
        /// <see cref="ProgressBuilder.GetPoiCount(int)"/>
        /// </summary>
        public UnityEvent<int> OnStart;

        /// <summary>
        /// Invoked when warpping to a POI is complete.
        /// <see cref="ActivityLog.SetCanLog(bool)"/>
        /// </summary>
        public UnityEvent OnWarpComplete;

        /// <summary>
        /// Initialize events if they are null
        /// </summary>
        private void Awake()
        {
            OnPoiEnter ??= new();
            OnPoiExit ??= new();
            PoiInteracted ??= new();
            OnGetPoiList ??= new();
            OnStart ??= new();
            OnWarpComplete ??= new();
        }

        private void Start()
        {
            // runs on start to get the total number of POIs that contain inspectables
            InteractablePoiCount();

            OnStart?.Invoke(interactablePois);
            PoiInteracted?.Invoke(poisInteracted);
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

        /// <summary>
        ///     Warps the player to the default waypoint of the specified POI.
        ///     Used for loading player position from saved data.
        /// </summary>
        /// <remarks>
        ///     Call flow:
        ///     - Connected from: <see cref="SaveDataSupport.MovePlayer"/> event
        ///     - Finds the POI with matching name and its default waypoint
        ///     - Uses NavMeshAgent.Warp to instantly teleport player
        /// </remarks>
        /// <param name="poiName">Name of the POI to warp to</param>
        public void WarpToLastPOI(string poiName)
        {
            poiName = poiName == "" ? "Bathroom" : poiName;
            Poi lastPoi = pois.Find(p => p.SelectedPoiName.ToString() == poiName);

            if (lastPoi == null || lastPoi.DefaultWaypoint == null)
            {
                Debug.LogWarning($"PoiHandler: Failed to warp - '{poiName}' POI or default waypoint not found");
                return;
            }

            StartCoroutine(WarpCoroutine(lastPoi));
        }

        /// <summary>
        /// Coroutine to warp the player to a specific POI's default waypoint.
        /// </summary>
        /// <param name="lastPoi">POI to warp to</param>
        /// <returns>IEnumerator for coroutine</returns>
        private IEnumerator WarpCoroutine(Poi lastPoi)
        {
            player.Warp(lastPoi.DefaultWaypoint.transform.position);
            currentPoi = lastPoi;
            Debug.Log($"PoiHandler: Player warped to {lastPoi.PoiName}'s default waypoint");

            yield return new WaitForSeconds(0.1f); // Wait for a short time to ensure the warp is complete

            OnWarpComplete?.Invoke();
        }

        // For use in data retrieval
        /// <see cref="InspectionReviewBuilder.OnEndInspectionConfirmation"/>
        /// Event calls this method to fire off a data sending event with the list of POIs.
        public void GetPoiList()
        {
            OnGetPoiList?.Invoke(pois);
        }
    }
}
