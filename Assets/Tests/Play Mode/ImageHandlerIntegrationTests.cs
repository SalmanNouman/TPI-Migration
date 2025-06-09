using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.DLX;
using UnityEditor;
using VARLab.ObjectViewer;

namespace Tests.PlayMode
{
    public class ImageHandlerIntegrationTests
    {
        // Game Objects
        private GameObject imageHandlerGO;
        private GameObject inspectableObjectGO;
        private GameObject toggleableGO;
        private GameObject timerGO;
        private GameObject objectViewerInspectableGO;

        private ImageHandler imageHandler;
        private ToggleableInspectable inspectableObject;
        private ObjectViewerInspectables objectViewerObject;

        private Camera inspectableObjectCamera;
        private Camera mainCamera;

        [OneTimeSetUp]
        [Category("BuildServer")]
        public void RunOnce()
        {
            // Creating a new instance for each of the game objects
            imageHandlerGO = new();
            inspectableObjectGO = new();
            objectViewerInspectableGO = new();
            toggleableGO = new();
            timerGO = new();

            // Setting up the game objects by adding the components
            // The Inspectables (ToggleableInspectable, ObjectViewerInspectable, etc) require a collider to be added first
            inspectableObjectGO.AddComponent<BoxCollider>();
            objectViewerInspectableGO.AddComponent<BoxCollider>();
            objectViewerInspectableGO.AddComponent<MeshRenderer>();
            
            imageHandler = imageHandlerGO.AddComponent<ImageHandler>();
            inspectableObjectCamera = inspectableObjectGO.AddComponent<Camera>();
            mainCamera = objectViewerInspectableGO.AddComponent<Camera>();
            mainCamera.enabled = true;
            timerGO.AddComponent<TimerManager>();

            // Set up the inspectable object (set properties BEFORE adding component)
            inspectableObject = inspectableObjectGO.AddComponent<ToggleableInspectable>();
            inspectableObject.Name = "Test";
            inspectableObject.Location = PoiList.PoiName.Reception;
            inspectableObject.Cam = inspectableObjectCamera;
            inspectableObject.Toggleables = new ();
            inspectableObject.Toggleables.Add(toggleableGO);
            inspectableObject.ObjectId = inspectableObject.GeneratedId();

            // Set up the ObjectViewer object with proper sprite and state configuration
            objectViewerObject = objectViewerInspectableGO.AddComponent<ObjectViewerInspectables>();
            objectViewerObject.Name = "ObjectViewer";
            objectViewerObject.Location = PoiList.PoiName.Bathroom;
            objectViewerObject.Cam = mainCamera;
            objectViewerObject.FieldOfView = 10f;
            objectViewerObject.States = new ();
            objectViewerObject.GallerySprites = new ();
            objectViewerObject.ObjectId = objectViewerObject.GeneratedId();
            
            // Load test sprite from project assets
            Sprite testSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/GallerySprites/PiercingNeedle_NonCompliant.png");
            objectViewerObject.GallerySprites.Add(testSprite);
            
            // Create and setup a state for ObjectViewer
            GameObject stateGO = new GameObject("TestState");
            stateGO.transform.SetParent(objectViewerInspectableGO.transform);
            stateGO.SetActive(true); // Make sure the state is active
            State testState = new State(stateGO, Compliancy.Compliant);
            objectViewerObject.States.Add(testState);

            // Start the timer
            TimerManager.Instance.StartTimers();
        }

        /// <summary>
        ///  This test checks if the inspectable object camera is not null
        /// </summary>
        [UnityTest, Order(0)]
        [Category("BuildServer")]
        public IEnumerator CheckIfInspectableCameraIsNotNull()
        {
            // Arrange
            bool result = false;

            // Act
            if (inspectableObjectCamera != null)
            {
                result = true;
            }
            yield return null;

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Checks if the temporary photo, that will be displayed in the inspection window is taken
        /// </summary>
        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator CheckIfTempPhotoWasTaken()
        {
            // Arrange
            bool wasTaken = false;

            imageHandler.OnTempPhotoTaken.AddListener((tempPhoto) => wasTaken = true);

            // Act
            imageHandler.TakeTempPhoto(inspectableObject);

            yield return null;

            // Assert
            Assert.IsTrue(wasTaken);
        }

        /// <summary>
        /// Check if the photo is saved to the list and if the photo id matches the obejct id.
        /// </summary>
        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator CheckIfPhotoIsSavedToList()
        {
            // Arrange
            int expectedResult = 1;
            bool wasTaken = false;

            imageHandler.OnPhotoSaved.AddListener((InspectableObject obj) => wasTaken = true);

            // Act
            imageHandler.TakePhoto(inspectableObject);
            yield return null;
            var photos = imageHandler.GetListOfPhotos();

            // Assert
            Assert.IsTrue(wasTaken);
            Assert.AreEqual(expectedResult, photos.Count());
            Assert.AreEqual(inspectableObject.ObjectId.ToString(), photos[0].Id);
        }

        /// <summary>
        /// checks if the photo is removed from the list
        /// </summary>
        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator CheckIfPhotoIsRemovedFromList()
        {
            // Arrange
            bool wasDeleted = false;
            int expectedResult = 0;

            imageHandler.OnPhotoDeleted.AddListener((id) => wasDeleted = true);

            // Act
            imageHandler.RemovePhotoFromList(inspectableObject.ObjectId.ToString());
            var photos = imageHandler.GetListOfPhotos();
            yield return null;

            // Assert
            Assert.IsTrue(wasDeleted);
            Assert.AreEqual(expectedResult, photos.Count());
        }

        /// <summary>
        /// Checks if the remove function returns false if the object id is not found when removing a photo from the list 
        /// and that the photo deleted event does not get invoked.
        /// </summary>
        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator RemovePhotoReturnsFalseIfIDNotFound()
        {
            // Arrange
            bool photoDeleted = true;
            bool wasDeleted = false;

            imageHandler.OnPhotoDeleted.AddListener((id) => wasDeleted = true);

            // Act
            photoDeleted = imageHandler.RemovePhotoFromList("Object_Id");
            yield return null;

            // Assert
            Assert.IsFalse(photoDeleted);
            Assert.IsFalse(wasDeleted);
        }

        /// <summary>
        /// Checks if TakeTempPhoto does not get invoked for ObjectViewerInspectables
        /// </summary>
        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator OnTempPhotoTakenDoesNotGetInvokedWithObjectViewerInspectables()
        {
            // Arrange
            bool wasTaken = false;

            imageHandler.OnTempPhotoTaken.AddListener((tempPhoto) => wasTaken = true);

            // Act
            imageHandler.TakeTempPhoto(objectViewerObject);

            yield return null;

            // Assert
            Assert.IsFalse(wasTaken);
        }

        [UnityTest, Order(6)]
        [Category("BuildServer")]
        public IEnumerator PhotoSavedEventIsTriggeredWhenAPhotoIsSaved()
        {
            // Arrange
            bool isSaved = false;
            imageHandler.OnPhotoSaved.AddListener((inspectableObject) => isSaved = true);
            imageHandler.TakeTempPhoto(inspectableObject);
            inspectableObject.HasPhoto = false;

            // Act
            imageHandler.TakePhoto(inspectableObject);

            yield return null;

            // Assert
            Assert.IsTrue(isSaved);
        }

        [UnityTest, Order(7)]
        public IEnumerator TakePhotoForLoad_Triggers_OnPhotoListChanged()
        {
            // Arrange
            imageHandler.Photos.Clear();

            Dictionary<InspectableObject, string> savedPhotos = new Dictionary<InspectableObject, string>();
            savedPhotos.Add(objectViewerObject, "timestamp");
            savedPhotos.Add(inspectableObject, "timestamp");

            bool wasTriggered = false;
            imageHandler.OnPhotoListChanged.AddListener((photoList) => wasTriggered = true);

            // Act
            imageHandler.TakePhotoForLoad(savedPhotos);
            yield return null;

            // Assert
            Assert.IsTrue(wasTriggered);
            Assert.AreEqual(savedPhotos.Count, imageHandler.Photos.Count);
        }

        /// <summary>
        ///     Tests that ObjectViewer objects can generate gallery image bytes from sprites
        /// </summary>
        [UnityTest, Order(8)]
        [Category("BuildServer")]
        public IEnumerator ObjectViewerInspectables_GetGalleryImageBytes_ReturnsValidImageData()
        {
            // Arrange
            objectViewerObject.HasPhoto = false; // Reset the photo state
            
            // Act
            byte[] imageBytes = objectViewerObject.GetGalleryImageBytes();
            yield return null;

            // Assert
            Assert.IsNotNull(imageBytes, "Gallery image bytes should not be null");
        }

        /// <summary>
        ///     Tests that CreateObjectViewerTempPhoto works correctly for ObjectViewer objects
        /// </summary>
        [UnityTest, Order(9)]
        [Category("BuildServer")]
        public IEnumerator ImageHandler_CreateObjectViewerTempPhoto_CreatesValidTempPhoto()
        {
            // Arrange
            imageHandler.Photos.Clear();
            objectViewerObject.HasPhoto = false; // Reset the photo state

            // Act
            imageHandler.CreateObjectViewerTempPhoto(objectViewerObject);
            imageHandler.TakePhoto(objectViewerObject); // Save the temp photo
            yield return null;

            var photos = imageHandler.GetListOfPhotos();

            // Assert
            Assert.AreEqual(1, photos.Count, "Should have one photo");
            Assert.AreEqual(objectViewerObject.ObjectId, photos[0].Id, "Photo ID should match object ID");
            Assert.IsNotNull(photos[0].Data, "Photo data should not be null");
        }

        /// <summary>
        ///     Tests that ObjectViewer objects are handled correctly in TakePhotoForLoad
        /// </summary>
        [UnityTest, Order(10)]
        [Category("BuildServer")]
        public IEnumerator ImageHandler_TakePhotoForLoad_HandlesObjectViewerCorrectly()
        {
            // Arrange
            imageHandler.Photos.Clear();
            objectViewerObject.HasPhoto = false; // Reset the photo state
            
            Dictionary<InspectableObject, string> savedPhotos = new Dictionary<InspectableObject, string>();
            savedPhotos.Add(objectViewerObject, "12:34:56");

            // Act
            imageHandler.TakePhotoForLoad(savedPhotos);
            yield return null;

            var photos = imageHandler.GetListOfPhotos();

            // Assert
            Assert.AreEqual(1, photos.Count, "Should have one photo");
            Assert.AreEqual(objectViewerObject.ObjectId, photos[0].Id, "Photo ID should match object ID");
            Assert.AreEqual("12:34:56", photos[0].Timestamp, "Timestamp should match");
            Assert.IsTrue(objectViewerObject.HasPhoto, "ObjectViewer object should be marked as having photo");
        }
    }

}
