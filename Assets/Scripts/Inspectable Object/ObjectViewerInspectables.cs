using UnityEngine;
using VARLab.ObjectViewer;

namespace VARLab.DLX
{
    [RequireComponent(typeof(WorldObject))]
    [RequireComponent(typeof(RotateObject))]
    /// <summary>
    /// Represent an object that can be viewed and inspected. 
    /// </summary>
    /// <remarks>
    /// Requires both <see cref="WorldObject"/> and <see cref="RotateObject"/> components
    /// to function correctly in the Object Viewer system.
    /// </remarks>
    public class ObjectViewerInspectables : InspectableObject
    {
        /// <summary>
        /// Sets the initial Field of View (FOV) value for the camera when viewing this object.
        /// This value directly determines the initial zoom level when the inspection window opens.
        /// <see cref="InspectionWindowBuilder.HandleInspectionWindowDisplay"/>
        /// </summary>
        [Tooltip("Camera field of view value for this specific object. Lower values zoom in, higher values zoom out.")]
        public float FieldOfView = 35.0f;
        

        // Note: For additional object transform settings (rotation and scale settings), 
        // use the <see cref="ObjectTransformDetails"/> ScriptableObject and assign it to the TransformDetails field in the WorldObject component.
        // This is especially useful for objects of different sizes that need custom display adjustments.

    }
}
