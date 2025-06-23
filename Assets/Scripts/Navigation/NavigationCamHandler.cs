using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using VARLab.Navigation.PointClick;

namespace VARLab.DLX
{
    /// <summary>
    /// Data class to hold animation details for drilling
    /// </summary>
    /// <remarks>
    /// Waypoint : the waypoint that will trigger the camera look at
    /// Camera Target: What camera will look at
    /// </remarks>
    [Serializable]
    public class TargetsWithLookAt
    {
        [Tooltip("Waypoint that will trigger camera look at")] public Waypoint Waypoint;
        [Tooltip("Target for camera to look at on waypoint")] public Transform Target;
        [Tooltip("Target for camera that is used one time initially then defaults to Target")] public Transform OneTimeTarget;
        [HideInInspector] public bool OneTimeTargetTriggered;
    }

    /// <summary>
    /// Container for easier organization for unity editor serialization side
    /// </summary>
    [Serializable]
    public class TargetsInPoi
    {
        [Tooltip("Inspectable Object POI")]
        public PoiList.PoiName Location;
        public List<TargetsWithLookAt> TargetsWithLookAt;
    }
    
    public class NavigationCamHandler : MonoBehaviour
    {
        [SerializeField] private List<TargetsInPoi> targetsInPoi = new();
        private PointClickNavigation nav;
        
        public UnityEvent<List<TargetsInPoi>> SaveTargetsOneTimeTriggered = new UnityEvent<List<TargetsInPoi>>();
        
        private void Start()
        {
            nav = GetComponent<PointClickNavigation>();
            nav.WalkCompleted.AddListener((_) => ResetLookAt());
            nav.WalkCompleted.AddListener((_) => SaveTargetsOneTimeTriggered?.Invoke(targetsInPoi)); 
        }
        
        /// <summary>
        /// Checks if the waypoint has camera target to look at triggers it
        /// </summary>
        /// <param name="waypoint">The waypoint to check for target for</param>
        /// <see cref="PointClickNavigation.WalkStarted"/>
        public void TriggerLookAt(Waypoint waypoint)
        {
            var target = targetsInPoi
                .SelectMany(t => t.TargetsWithLookAt) // Flatten all targets into a single list
                .FirstOrDefault(t => t.Waypoint == waypoint); // Search for the specific waypoint
            if (target != null)
            {
                if ( target.OneTimeTarget != null && !target.OneTimeTargetTriggered)
                {
                    nav.LookAtTarget = target.OneTimeTarget.transform;
                    target.OneTimeTargetTriggered = true;
                }
                else
                {
                    nav.LookAtTarget = target.Target;
                }
            }
        }
        
        /// <summary>
        /// Clears the camera look at target to allow for panning and removes listener for walk complete
        /// </summary>
        /// <see cref="PointClickNavigation.WalkCompleted"/>s
        public void ResetLookAt()
        {
            nav.LookAtTarget = null;
        }

        /// <summary>
        /// Go through the location and sets the bools of one time target as ordered in load.
        /// </summary>
        /// <param name="oneTimeTargetBools"></param>
        public void LoadOneTimeLookAtFlags(Dictionary<PoiList.PoiName, List<bool>> oneTimeTargetBools)
        {
            foreach (var poi in targetsInPoi)
            {
                if (oneTimeTargetBools.TryGetValue(poi.Location, out var boolList))
                {
                    for (int i = 0; i < poi.TargetsWithLookAt.Count && i < boolList.Count; i++)
                    {
                        poi.TargetsWithLookAt[i].OneTimeTargetTriggered = boolList[i];
                    }
                }
                else
                {
                    Debug.LogWarning($"No saved data found for POI: {poi.Location}");
                }
            }

        }
    }
}
