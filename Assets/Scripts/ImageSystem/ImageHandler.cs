using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.DLX
{
    /// <summary>
    /// This class is responsible for getting the picture of the inspectable and displaying it in the
    /// inspection window. It also contains a list of images that are "taken" by the inspector and saved.
    /// </summary>
    public class ImageHandler : MonoBehaviour
    {
        [Tooltip("List of photos")]
        public List<InspectablePhoto> Photos;

        private RenderTexture renderTexture;
        private InspectablePhoto tempPhoto;

        // Render texture settings
        private const int ResWidth = 770;
        private const int ResHeight = 580;
        private const int ResDepth = 16;

        // Unity Events
        public UnityEvent<InspectablePhoto> OnTempPhotoTaken;
        public UnityEvent OnPhotoSaved;
        public UnityEvent OnPhotoDeleted;

        private void Awake()
        {
            // Initialize the Unity Events so they don't throw a null ref in tests
            OnTempPhotoTaken ??= new();
            OnPhotoSaved ??= new();
            OnPhotoDeleted ??= new();

            // Initialize a new render texture
            renderTexture = new(ResWidth, ResHeight, ResDepth);

            // Initialize a new photos list
            Photos = new();
        }

        /// <summary>
        /// Takes the temporary photo that will be displayed in the inspection window if the objects does not 
        /// use object viewer.
        /// <see cref=""/> This will be called by the inspection window on show
        /// </summary>
        /// <param name="obj">Inspectable object that is being inspected</param>
        public void TakeTempPhoto(InspectableObject obj)
        {
            if (obj.Cam == null)
            {
                Debug.Log($"Inspectable object: {obj.Name} in the {obj.Location} is missing a camera");
                return;
            }

            // TODO: Check if the object uses ObjectViewer

            Camera cam = obj.Cam;

            // Code to take a screen capture of a given camera
            cam.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            cam.Render();

            Texture2D renderedTexture = new(ResWidth, ResHeight);
            renderedTexture.ReadPixels(new Rect(0, 0, ResWidth, ResHeight), 0, 0);

            RenderTexture.active = null;
            byte[] byteArray = renderedTexture.EncodeToPNG();

            tempPhoto = new(byteArray, obj.ObjectId, obj.Location.ToString(), TimerManager.Instance.GetElapsedTime());

            // Destroy the rendered texture because it does not get destroyed automatically causing a memory leak
            // TODO: This might have to be done in a different place if the image is not showing in the inspection window.
            Destroy(renderedTexture);

            OnTempPhotoTaken?.Invoke(tempPhoto);
        }

        /// <summary>
        /// Saves photos confirmed (when inspector completes an inspection) to the photos list so that they can be displayed in the gallery.
        /// </summary>
        /// <param name="obj">Inspectable object that is currently being inspected</param>
        public void TakePhoto(InspectableObject obj)
        {
            // TODO: If the object uses object viewer the picture will be taken here for set images or for the obj current position
            tempPhoto.Timestamp = TimerManager.Instance.GetElapsedTime();
            Photos.Add(tempPhoto);

            OnPhotoSaved?.Invoke();
        }

        /// <summary>
        /// Removes a photo from the photos list if it matches the id.
        /// </summary>
        /// <param name="id">InspectablePhoto ID</param>
        public bool RemovePhotoFromList(string id)
        {
            int index = Photos.FindIndex(p => p.Id == id);
            if (index == -1)
            {
                Debug.Log($"Image for id: {id} not found");
                return false;
            }
            Photos.RemoveAt(index);

            OnPhotoDeleted?.Invoke();
            return true;
        }

        /// <summary>
        /// This method takes pictures all of the inspectables that have photo on load.
        /// </summary>
        public void TakePhotoForLoad()
        {
            // TODO: This will be implemented when we add save and load
        }

        /// <summary>
        /// This is used by the gallery builder to get the list of photos.
        /// </summary>
        /// <returns>The list og photos</returns>
        public List<InspectablePhoto> GetListOfPhotos()
        {
            return Photos;
        }
    }
}
