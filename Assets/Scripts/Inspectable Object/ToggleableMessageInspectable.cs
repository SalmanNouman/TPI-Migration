using System.Collections.Generic;
using UnityEngine;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///     Extends <see cref="MessageInspectable"/> that extends <see cref="InspectableObject"/>
    ///     with game object toggling functionality during inspection.
    /// </summary>
    /// <remarks>
    ///     - Inherits dynamic state-based notification message selection from <see cref="MessageInspectable"/>
    ///     - Inherits all standard <see cref="InspectableObject"/> functionality
    ///     - Toggles (show/hide) game objects
    ///     - Can be used for objects that need to reveal hidden elements during inspection
    /// </remarks>
    public class ToggleableMessageInspectable : MessageInspectable
    {
        [Header("Toggleable Object Properties"), Space(5f)]
        [Tooltip("List of game objects that will be toggled (activated/deactivated) when this object is inspected")] 
        public List<GameObject> Toggleables = new();
        
        /// <summary>
        ///     Toggles the visibility of all game objects in the Toggleables list.
        /// </summary>
        /// <remarks>
        ///     This method inverts the active state of each object in the Toggleables list.
        ///     Used for showing/hiding elements during inspection.
        /// </remarks>
        public void ToggleForInspection()
        {
            foreach (var toggleable in Toggleables)
            {
                toggleable.SetActive(!toggleable.activeSelf);
            }
        }
    }
} 