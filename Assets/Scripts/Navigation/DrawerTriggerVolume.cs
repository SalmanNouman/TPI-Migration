using UnityEngine;
using System.Collections.Generic;

namespace VARLab.DLX
{
    /// <summary>
    ///     Automatically moves objects (typically drawers) as players approach and leave trigger area.
    /// </summary>
    /// <remarks>
    ///     Objects will continuously interpolate towards their target position while in Opening or Closing state. 
    ///     The state only changes when player enters or exits the trigger volume.
    ///     - Opens/moves objects when player enters trigger area (changes to Opening state)
    ///     - Closes/returns objects when player exits trigger area (changes to Closing state)
    ///     - Can be used for drawers, sliding panels, or any objects that need automatic movement
    /// </remarks>
    public class DrawerTriggerVolume : MonoBehaviour
    {
        #region Fields

        /// <summary>
        ///     Represents the possible states of the movement system
        /// </summary>
        /// <remarks>
        ///     - Opening: Objects are moving to target position
        ///     - Closing: Objects are returning to original position
        ///     - Idle: Objects are not moving (inactive state)
        /// </remarks>
        private enum DrawerState
        {
            Opening,
            Closing,
            Idle
        }
        
        [SerializeField, Tooltip("List of movement settings for objects controlled by this trigger")]
        private List<DrawerSettings> drawerSettings = new List<DrawerSettings>();

        [SerializeField, Tooltip("Speed at which objects move when triggered")]
        private float drawerSpeed = 3.0f;

        // list of initial positions of objects
        private List<Vector3> originalPositions = new List<Vector3>();

        // list of target positions for objects
        private List<Vector3> targetOpenPositions = new List<Vector3>();
        
        // current state of the movement system
        private DrawerState drawerState = DrawerState.Idle;

        #endregion

        #region Methods

        private void Start()
        {
            InitializeDrawers();
        }

        /// <summary>
        ///     Updates object positions each frame when not in Idle state.
        /// </summary>
        private void Update()
        {
            // skip update if in idle state
            if (drawerState == DrawerState.Idle)
                return;
                
            // move objects and handle state changes
            MoveDrawers();
        }

        /// <summary>
        ///     Initializes all objects by storing their original positions and calculating their target positions.
        /// </summary>
        private void InitializeDrawers()
        {
            if (drawerSettings == null || drawerSettings.Count == 0)
            {
                Debug.LogWarning("DrawerTriggerVolume: Settings list is empty!");
                return;
            }

            for (int i = 0; i < drawerSettings.Count; i++)
            {
                if (drawerSettings[i].Drawer == null)
                {
                    Debug.LogError($"DrawerTriggerVolume: Transform for object {i} is not assigned!");
                    continue;
                }

                // store original position
                originalPositions.Add(drawerSettings[i].Drawer.localPosition);

                // calculate and store target open position
                Vector3 openPosition = CalculateDrawerTargetPosition(drawerSettings[i]);
                targetOpenPositions.Add(openPosition);
            }
        }

        /// <summary>
        ///     Calculates the target position for an object based on its settings.
        /// </summary>
        /// <param name="settings">The settings to use for calculation</param>
        /// <returns>The calculated target position when triggered</returns>
        private Vector3 CalculateDrawerTargetPosition(DrawerSettings settings)
        {
            // start with the current position
            Vector3 targetPosition = settings.Drawer.localPosition;
            
            // calculate movement amount (positive or negative)
            float amount = settings.UseNegativeDirection ? -settings.OpenAmount : settings.OpenAmount;
            
            // apply movement based on the selected axis
            switch (settings.Axis)
            {
                case DrawerSettings.MovementAxis.X:
                    targetPosition.x += amount;
                    break;
                case DrawerSettings.MovementAxis.Y:
                    targetPosition.y += amount;
                    break;
                case DrawerSettings.MovementAxis.Z:
                    targetPosition.z += amount;
                    break;
            }
            
            return targetPosition;
        }

        /// <summary>
        ///     Moves all objects towards their target positions and handles state changes.
        /// </summary>
        private void MoveDrawers()
        {
            bool allObjectsReachedTarget = true;
            
            // move all objects based on current state
            for (int i = 0; i < drawerSettings.Count; i++)
            {
                if (drawerSettings[i].Drawer == null) continue;

                // determine target position based on current state
                Vector3 targetPosition = (drawerState == DrawerState.Opening) 
                    ? targetOpenPositions[i] 
                    : originalPositions[i];
                
                // get current position of the object
                Vector3 currentPosition = drawerSettings[i].Drawer.localPosition;
                
                // use Lerp for smoother movement
                Vector3 newPosition = Vector3.Lerp(
                    currentPosition,
                    targetPosition,
                    Time.deltaTime * drawerSpeed
                );
                
                // update object position
                drawerSettings[i].Drawer.localPosition = newPosition;
                
                // check if this object has reached its target position
                // use a small threshold since Lerp never exactly reaches the target
                if (Vector3.Distance(newPosition, targetPosition) > 0.001f)
                {
                    allObjectsReachedTarget = false;
                }
            }
            
            // if all objects have reached their targets, change to idle state
            if (allObjectsReachedTarget)
            {
                string stateText = drawerState == DrawerState.Opening ? "open" : "closed";
                Debug.Log($"DrawerTriggerVolume: All objects have completely {stateText}.");
                drawerState = DrawerState.Idle;
            }
        }

        /// <summary>
        ///     Called when a collider enters the trigger volume.
        ///     Changes state to Opening to make objects move to target position.
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
        ///     Changes state to Closing to make objects return to original position.
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

        #endregion
    }
}
