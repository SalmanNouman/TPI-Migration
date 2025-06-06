using UnityEngine;
using VARLab.ObjectViewer;
using System.Collections.Generic;

namespace VARLab.DLX
{
    [RequireComponent(typeof(WorldObject))]
    [RequireComponent(typeof(RotateObject))]
    /// <summary>
    ///     Represent an object that can be viewed and inspected using the Object Viewer system.
    /// </summary>
    /// <remarks>
    ///     - Requires both <see cref="WorldObject"/> and <see cref="RotateObject"/> components to function correctly
    ///     - References sprite assets for gallery image generation and display
    /// </remarks>
    public class ObjectViewerInspectables : InspectableObject
    {
        /// <summary>
        ///     Sets the initial Field of View (FOV) value for the camera when viewing this object.
        ///     This value directly determines the initial zoom level when the inspection window opens.
        ///     <see cref="InspectionWindowBuilder.HandleInspectionWindowDisplay"/>
        /// </summary>
        [Tooltip("Camera field of view value for this specific object. Lower values zoom in, higher values zoom out.")]
        public float FieldOfView = 35.0f;

        [Header("Gallery Images"), Space(5f)]
        [Tooltip("PNG sprites for gallery display")]
        public List<Sprite> GallerySprites = new ();

        // Note: For additional object transform settings (rotation and scale settings), 
        // use the <see cref="ObjectTransformDetails"/> ScriptableObject and assign it to the TransformDetails field in the WorldObject component.
        // This is especially useful for objects of different sizes that need custom display adjustments.

        /// <summary>
        ///     Gets the PNG byte data for gallery display from the assigned sprites.
        /// </summary>
        /// <remarks>
        ///     Currently returns the first sprite until scenario system is implemented.
        ///     TODO: When scenario system is ready, this will select appropriate sprite based on current object state/compliance status.
        /// </remarks>
        /// <returns> PNG byte array of the first sprite's texture, or null if no valid sprites are assigned </returns>
        public byte[] GetGalleryImageBytes()
        {
            if (GallerySprites.Count == 0 || GallerySprites[0] == null)
            {
                Debug.LogWarning($"No valid sprite found for object {Name}. Please assign a sprite to the GallerySprites list.");
                return null;
            }
            
            return GallerySprites[0].texture.EncodeToPNG();
        }
    }
}
