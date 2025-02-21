using UnityEngine;
using VARLab.ObjectViewer;

namespace VARLab.DLX
{
    [RequireComponent(typeof(WorldObject))]
    [RequireComponent(typeof(RotateObject))]
    /// <summary>
    /// Represent an object that can be viewed and inspected. 
    /// </summary>
    public class ObjectViewerInspectables : InspectableObject
    {
        [Tooltip("Field of view value")]
        public float FieldOfView;
    }
}
