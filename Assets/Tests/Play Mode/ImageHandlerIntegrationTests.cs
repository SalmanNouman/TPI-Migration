using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.DLX;

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
            inspectableObject = inspectableObjectGO.AddComponent<ToggleableInspectable>();
            objectViewerObject = objectViewerInspectableGO.AddComponent<ObjectViewerInspectables>();
            inspectableObjectCamera = inspectableObjectGO.AddComponent<Camera>();
            mainCamera = objectViewerInspectableGO.AddComponent<Camera>();
            mainCamera.enabled = true;
            timerGO.AddComponent<TimerManager>();

            // Set up the inspectable object
            inspectableObject.Name = "Test";
            inspectableObject.Location = PoiList.PoiName.Reception;
            inspectableObject.Cam = inspectableObjectCamera;
            inspectableObject.Toggleables.Add(toggleableGO);
            inspectableObject.ObjectId = inspectableObject.GeneratedId();

            objectViewerObject.Name = "ObjectViewer";
            objectViewerObject.Location = PoiList.PoiName.Bathroom;
            objectViewerObject.Cam = mainCamera;
            objectViewerObject.FieldOfView = 10f;
            objectViewerObject.ObjectId = objectViewerObject.GeneratedId();

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

            imageHandler.OnPhotoDeleted.AddListener(() => wasDeleted = true);

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

            imageHandler.OnPhotoDeleted.AddListener(() => wasDeleted = true);

            // Act
            photoDeleted = imageHandler.RemovePhotoFromList("Object_Id");
            yield return null;

            // Assert
            Assert.IsFalse(photoDeleted);
            Assert.IsFalse(wasDeleted);
        }

        /// <summary>
        /// Checks if the remove function returns false if the object id is not found when removing a photo from the list 
        /// and that the photo deleted event does not get invoked.
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
            imageHandler.OnPhotoSaved.AddListener((InspectableObject) => isSaved = true);
            imageHandler.TakePhoto(inspectableObject);

            // Act
            imageHandler.TakePhoto(inspectableObject);

            yield return null;

            // Assert
            Assert.IsTrue(isSaved);
        }
    }

}
