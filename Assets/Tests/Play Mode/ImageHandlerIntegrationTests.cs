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

        private ImageHandler imageHandler;
        private ToggleableInspectable inspectableObject;

        private Camera inspectableObjectCamera;

        [OneTimeSetUp]
        [Category("BuildServer")]
        public void RunOnce()
        {
            // Creating a new instance for each of the game objects
            imageHandlerGO = new();
            inspectableObjectGO = new();
            toggleableGO = new();
            timerGO = new();

            // Setting up the game objects by adding the components
            // The Inspectables (ToggleableInspectable, ObjectViewerInspectable, etc) require a collider to be added first
            inspectableObjectGO.AddComponent<BoxCollider>();
            imageHandler = imageHandlerGO.AddComponent<ImageHandler>();
            inspectableObject = inspectableObjectGO.AddComponent<ToggleableInspectable>();
            inspectableObjectCamera = inspectableObjectGO.AddComponent<Camera>();
            timerGO.AddComponent<TimerManager>();

            // Set up the inspectable object
            inspectableObject.Name = "Test";
            inspectableObject.Location = PoiList.PoiName.Reception;
            inspectableObject.Cam = inspectableObjectCamera;
            inspectableObject.Toggleables.Add(toggleableGO);
            inspectableObject.ObjectId = inspectableObject.GeneratedId();

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

            imageHandler.OnPhotoSaved.AddListener(() => wasTaken = true);

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
    }

}
