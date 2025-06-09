using System.Collections.Generic;
using UnityEngine;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///     Extends <see cref="InspectableObject"/> with element toggling and message display.
    /// </summary>
    /// <remarks>
    ///     - Provides ability to toggle (show/hide) game objects when inspected
    ///     - Supports different notifications for compliant and non-compliant states
    ///     - Inherits all standard inspectable object functionality
    ///     - Can be used for objects that need to reveal hidden elements during inspection
    /// </remarks>
    public class ToggleableMessageInspectable : InspectableObject
    {
        [Header("Toggleable Object Properties"), Space(5f)]
        [Tooltip("List of game objects that will be toggled (activated/deactivated) when this object is inspected")] 
        public List<GameObject> Toggleables = new();

        [Header("Message Properties"), Space(5f)]
        [Tooltip("Notification to display when in compliant state")]
        public NotificationSO InspectionNotificationCompliant;

        [Tooltip("Notification to display when in non-compliant state")]
        public NotificationSO InspectionNotificationNonCompliant;

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