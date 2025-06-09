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
        /// <remarks>
        ///     For custom transform settings (rotation and scale), use <see cref="ObjectTransformDetails"/> 
        ///     ScriptableObject in WorldObject's TransformDetails field. Useful for different sized objects.
        /// </remarks>
        [Tooltip("Camera field of view value for this specific object. Lower values zoom in, higher values zoom out.")]
        public float FieldOfView = 35.0f;

        [Space(5f)]
        [Tooltip("PNG sprites for gallery display.\nOrder must match States list:\n(The 1st State = The 1st Sprite, ···)")]
        public List<Sprite> GallerySprites = new ();

        /// <summary>
        ///     Gets PNG byte data of the sprite matching the current active state for gallery display.
        /// </summary>
        /// <remarks>
        ///     Automatically finds the active (visible) state from the States list and returns 
        ///     the corresponding sprite from GallerySprites at the same index.
        /// </remarks>
        /// <returns>PNG byte array of the matching sprite, or 'null' if validation fails</returns>
        public byte[] GetGalleryImageBytes()
        {
            if (GallerySprites.Count == 0)
            {
                Debug.LogWarning($"No images assigned for '{Name}'. Please assign sprite assets to the GallerySprites list.");
                return null;
            }

            // Find the index of the currently active (visible) state
            int activeStateIndex = GetActiveStateIndex();

            // Check if no active state is found
           if (activeStateIndex == -1)
            {
                Debug.LogWarning($"No active state found for '{Name}'.");
                return null;
            }

            // Check if the active state index exceeds the GallerySprites count
            if (activeStateIndex >= GallerySprites.Count)
            {
                Debug.LogError($"Active state index '{activeStateIndex}' exceeds GallerySprites count '{GallerySprites.Count}' for '{Name}'.");
                return null;
            }

            // Check if the sprite at the active state index is null
            if (GallerySprites[activeStateIndex] == null)
            {
                Debug.LogError($"Sprite at index '{activeStateIndex}' is null for '{Name}'. Please assign a valid sprite.");
                return null;
            }

            // Encode the sprite to PNG and return the byte array
            return GallerySprites[activeStateIndex].texture.EncodeToPNG();
        }

    }
}
