using UnityEngine;
using UnityEngine.Events;
using VARLab.Navigation.PointClick;

namespace VARLab.DLX
{
    /// <summary>
    ///     This class represents a Point of Interest (POI) component in the editor.
    /// </summary>
    /// <remarks>
    ///     This script should be attached as a component to GameObjects that represent points of interest (e.g., Bathroom, Reception).
    /// </remarks>
    public class Poi : MonoBehaviour
    {
        #region Fields

        [HideInInspector] public string PoiName;

        [HideInInspector] public bool Interacted; // Use to determine if the POI has been interacted with vs walked through.

        [Tooltip("Select the POI name from the list")]
        public PoiList.PoiName SelectedPoiName;

        [Tooltip("Look At target on POI Load")] public GameObject LookAtTarget;
        public Waypoint DefaultWaypoint;

        [Tooltip("Is there anything to inspect it this POI?")]
        public bool HasInspectables;

        #endregion

        #region Events

        /// <summary>
        ///     Event invoked when OnTriggerEnter detects a object that has a collider component entering this POI's trigger zone.
        /// </summary>
        public UnityEvent<Poi> OnPoiEnter;

        /// <summary>
        ///     Event invoked when OnTriggerExit detects a object that has a collider component exiting this POI's trigger zone.
        /// </summary>
        public UnityEvent<Poi> OnPoiExit;

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes the POI and sets the readable POI name.
        /// </summary>
        private void Start()
        {
            PoiName = PoiList.GetPoiName(SelectedPoiName.ToString());
            Interacted = false;
        }

        private void Awake()
        {
            OnPoiEnter ??= new();
            OnPoiExit ??= new();
        }

        /// <summary>
        ///     Unity callback triggered when an object that has a collider component enters this POI's collider area.
        /// </summary>
        /// <param name="collider">The collider that entered the trigger zone.</param>
        private void OnTriggerEnter(Collider collider)
        {
            Debug.Log($"Player entered {PoiName}");
            // Invoke the OnPoiEnter event
            OnPoiEnter?.Invoke(this);
        }

        /// <summary>
        ///     Unity callback triggered when an object that has a collider component leaves this POI's collider area.
        /// </summary>
        /// <param name="collider">The collider that exited the trigger zone.</param>
        private void OnTriggerExit(Collider collider)
        {
            Debug.Log($"Player exited {PoiName}");
            // Invoke the OnPoiExit event
            OnPoiExit?.Invoke(this);
        }

        #endregion
    }
}
