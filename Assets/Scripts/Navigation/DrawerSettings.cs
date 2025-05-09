using System;
using UnityEngine;

namespace VARLab.DLX
{
    /// <summary>
    ///     Settings class for storing drawer-specific configuration data.
    ///     Used by <see cref="DrawerTriggerVolume"/> to animate drawer movement when player approaches the toolbox.
    /// </summary>
    /// <remarks>
    ///     - Each instance represents a single drawer in the toolbox
    ///     - Defines how far the drawer should open when triggered
    ///     - Stores reference to the drawer's Transform component
    ///     - Added to <see cref="DrawerTriggerVolume.drawerSettings"/> list in the Inspector
    /// </remarks>
    [Serializable]
    public class DrawerSettings
    {
        [Tooltip("Reference to the drawer Transform")]
        public Transform Drawer;

        [Tooltip("Distance the drawer will move when opened (along the local -X axis)")]
        public float OpenAmount = 0.3f;
    }
} 