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
        //No additional properties at this time
        //Once we add the option of taking pictures we might save the images here.

    }
}
