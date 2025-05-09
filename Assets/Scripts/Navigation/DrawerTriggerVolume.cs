using UnityEngine;
using System.Collections.Generic;

namespace VARLab.DLX
{
    /// <summary>
    ///     Automatically opens and closes toolbox drawers as players approach and leave.
    /// </summary>
    /// <remarks>
    ///     Drawers will continuously interpolate towards their target position while in Opening or Closing state. 
    ///     The state only changes when player enters or exits the trigger volume.
    ///     - Opens drawers when player enters trigger area (changes to Opening state)
    ///     - Closes drawers when player exits trigger area (changes to Closing state)
    /// </remarks>
    public class DrawerTriggerVolume : MonoBehaviour
    {
        /// <summary>
        ///     Represents the possible states of the drawer system
        /// </summary>
        /// <remarks>
        ///     - Opening: Drawers are moving to open position
        ///     - Closing: Drawers are moving to closed position
        ///     - Idle: Drawers are not moving (inactive state)
        /// </remarks>
        private enum DrawerState
        {
            Opening,
            Closing,
            Idle
        }
        
        [SerializeField, Tooltip("List of drawer settings")]
        private List<DrawerSettings> drawerSettings = new List<DrawerSettings>();

        [SerializeField, Tooltip("Speed at which drawers open and close")]
        private float drawerSpeed = 3.0f;

        // list of initial positions of drawers
        private List<Vector3> originalPositions = new List<Vector3>();

        // list of target positions for drawers
        private List<Vector3> targetOpenPositions = new List<Vector3>();
        
        // current state of the drawers (controls movement behavior)
        private DrawerState drawerState = DrawerState.Idle;

        /// <summary>
        ///     Initializes drawer positions and calculates target positions.
        /// </summary>
        private void Start()
        {
            if (drawerSettings == null || drawerSettings.Count == 0)
            {
                Debug.LogWarning("DrawerTriggerVolume: Drawer settings list is empty!");
                return;
            }

            for (int i = 0; i < drawerSettings.Count; i++)
            {
                if (drawerSettings[i].Drawer == null)
                {
                    Debug.LogError($"DrawerTriggerVolume: Transform for drawer {i} is not assigned!");
                    continue;
                }

                // store original position
                originalPositions.Add(drawerSettings[i].Drawer.localPosition);

                // calculate open position (move along negative X axis by openAmount)
                Vector3 openPosition = drawerSettings[i].Drawer.localPosition;
                openPosition.x -= drawerSettings[i].OpenAmount;
                targetOpenPositions.Add(openPosition);
            }
        }

        /// <summary>
        ///     Updates drawer positions each frame when not in Idle state.
        ///     Continuously interpolates drawer positions toward their target.
        /// </summary>
        private void Update()
        {
            // skip update if drawers are in idle state
            if (drawerState == DrawerState.Idle)
                return;
                
            bool allDrawersReachedTarget = true;
            
            // move all drawers based on current state
            for (int i = 0; i < drawerSettings.Count; i++)
            {
                if (drawerSettings[i].Drawer == null) continue;

                // determine target position based on drawer state
                Vector3 targetPosition = (drawerState == DrawerState.Opening) 
                    ? targetOpenPositions[i] 
                    : originalPositions[i];
                
                // get current position of the drawer
                Vector3 currentPosition = drawerSettings[i].Drawer.localPosition;
                
                // use Lerp for smoother movement
                Vector3 newPosition = Vector3.Lerp(
                    currentPosition,
                    targetPosition,
                    Time.deltaTime * drawerSpeed
                );
                
                // update drawer position
                drawerSettings[i].Drawer.localPosition = newPosition;
                
                // check if this drawer has reached its target position
                // use a small threshold since Lerp never exactly reaches the target
                if (Vector3.Distance(newPosition, targetPosition) > 0.001f)
                {
                    allDrawersReachedTarget = false;
                }
            }
            
            // if all drawers have reached their targets, change to idle state
            if (allDrawersReachedTarget)
            {
                string stateText = drawerState == DrawerState.Opening ? "open" : "closed";
                Debug.Log($"DrawerTriggerVolume: All drawers have completely {stateText}.");
                drawerState = DrawerState.Idle;
            }
        }

        /// <summary>
        ///     Called when a collider enters the trigger volume.
        ///     Changes state to Opening to make drawers move to open position.
        /// </summary>
        /// <param name="other">The collider that entered the trigger</param>
        private void OnTriggerEnter(Collider other)
        {
            if (drawerState != DrawerState.Opening)
            {
                drawerState = DrawerState.Opening;
                Debug.Log("DrawerTriggerVolume: Trigger activated. Opening drawers.");
            }
        }

        /// <summary>
        ///     Called when a collider exits the trigger volume.
        ///     Changes state to Closing to make drawers move to closed position.
        /// </summary>
        /// <param name="other">The collider that exited the trigger</param>
        private void OnTriggerExit(Collider other)
        {
            if (drawerState != DrawerState.Closing)
            {
                drawerState = DrawerState.Closing;
                Debug.Log("DrawerTriggerVolume: Trigger deactivated. Closing drawers.");
            }
        }
    }
}
