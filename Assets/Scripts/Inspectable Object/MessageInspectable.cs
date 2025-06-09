using System.Collections.Generic;
using UnityEngine;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    ///     An inspectable object that displays state-based notification messages.
    /// </summary>
    /// <remarks>
    ///     Dynamically selects and displays appropriate notification messages based on the currently active compliance state.
    ///     Message selection matches the States list indices for consistent state-based content delivery.
    /// </remarks>
    public class MessageInspectable : InspectableObject
    {
        /// <summary>
        ///     Notification messages for different compliance states, ordered to match the States list indices.
        /// </summary>
        /// <remarks>
        ///     Each notification corresponds to the same index in the States list. When a specific state is active,
        ///     the notification at the same index will be displayed to the user.
        /// </remarks>
        [Header("Message Properties"), Space(5f)]
        [Tooltip("Notification messages for inspection display.\nOrder must match States list:\n(The 1st State = The 1st Message, ···)")]
        public List<NotificationSO> InspectionNotifications = new();

        /// <summary>
        ///     Gets the notification message for the currently active compliance state.
        /// </summary>
        /// <returns>Notification for the active state, or 'null' if validation fails</returns>
        public NotificationSO GetInspectionNotification()
        {
            if (InspectionNotifications.Count == 0)
            {
                Debug.LogWarning($"No inspection notifications assigned for '{Name}'. Please assign notifications to the InspectionNotifications list.");
                return null;
            }

            // Find the currently active state
            int activeStateIndex = GetActiveStateIndex();
            if (activeStateIndex == -1)
            {
                Debug.LogWarning($"No active state found for '{Name}'.");
                return null;
            }

            // Check if notification exists for the active state
            if (activeStateIndex >= InspectionNotifications.Count)
            {
                Debug.LogError($"Active state index '{activeStateIndex}' exceeds InspectionNotifications count '{InspectionNotifications.Count}' for '{Name}'.");
                return null;
            }

            // Check if the notification is assigned
            if (InspectionNotifications[activeStateIndex] == null)
            {
                Debug.LogError($"Notification at index '{activeStateIndex}' is null for '{Name}'. Please assign a valid notification.");
                return null;
            }

            return InspectionNotifications[activeStateIndex];
        }

    }
}
