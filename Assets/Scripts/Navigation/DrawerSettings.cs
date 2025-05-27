using System;
using UnityEngine;

namespace VARLab.DLX
{
    /// <summary>
    ///     Settings class for storing drawer-specific configuration data.
    ///     Used by <see cref="DrawerTriggerVolume"/> to animate drawer movement when player approaches.
    /// </summary>
    /// <remarks>
    ///     - Each instance can represent a drawer or any object that needs to move when triggered
    ///       - Can be used for objects that are visually inside a drawer but exist in separate hierarchy paths
    ///     - Defines how far the object should move when triggered
    ///     - Stores reference to the object's Transform component
    ///     - Added to <see cref="DrawerTriggerVolume.drawerSettings"/> list in the Inspector
    /// </remarks>
    [Serializable]
    public class DrawerSettings
    {
        #region Fields

        /// <summary>
        ///     Defines the axis along which the object will move when triggered.
        /// </summary>
        /// <remarks>
        ///     To add support for additional axes if needed in the future,
        ///     add new enum values here and update the movement calculation in
        ///     <see cref = "DrawerTriggerVolume.CalculateDrawerTargetPosition()"/> method.
        /// </remarks>
        public enum MovementAxis
        {
            X,
            Y,
            Z
        }

        [Tooltip("Reference to the Transform of the object that will move")]
        public Transform Drawer;

        [Tooltip("Distance the object will move when triggered")]
        public float OpenAmount = 0.3f;
        
        [Tooltip("Axis along which the object will move (X, Y, or Z)")]
        public MovementAxis Axis = MovementAxis.X;

        [Tooltip("When checked, object moves in negative direction along the selected axis")]
        public bool UseNegativeDirection = true;

        #endregion
    }
} 