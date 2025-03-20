using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.DLX;
using VARLab.Velcro;

namespace Tests.PlayMode
{
    public class GalleryBuilderIntegrationTest
    {
        // Inspection review window
        private UIDocument inspectionReviewDoc;
        private InspectionReviewBuilder inspectionReviewBuilder;
        private GalleryBuilder galleryBuilder;
        private VisualElement root;
        private List<InspectablePhoto> photos;

        private const string SceneName = "InspectionReviewTestScene";

        /// <summary>
        /// Loads the inspection review test scene
        /// </summary>
        [OneTimeSetUp]
        [Category("BuildServer")]
        public void RunOnce()
        {
            SceneManager.LoadScene(SceneName);
        }

        /// <summary>
        /// Checks if the test scene is loaded
        /// </summary>
        [UnityTest, Order(0)]
        [Category("BuildServer")]
        public IEnumerator SceneLoaded()
        {
            yield return new WaitUntil(() => SceneManager.GetSceneByName(SceneName).isLoaded);

            inspectionReviewBuilder = GameObject.FindAnyObjectByType<InspectionReviewBuilder>();
            inspectionReviewDoc = inspectionReviewBuilder.GetComponent<UIDocument>();
            galleryBuilder = GameObject.FindAnyObjectByType<GalleryBuilder>();

            root = inspectionReviewDoc.rootVisualElement;

            TimerManager.Instance.StartTimers();

            photos = new();
            photos.Add(new InspectablePhoto(new byte[14000], "Bathroom_SoapDispenser", "Bathroom", TimerManager.Instance.GetElapsedTime()));

            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
        }

        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator EmptyContentContainerVisibleWhenActivityLogListEmpty()
        {
            // Arrange
            var displayStyle = DisplayStyle.Flex.ToString().Trim();
            VisualElement emptyContainer = galleryBuilder.LogContainer.Q<VisualElement>("EmptyContainer");

            // Act
            galleryBuilder.BuildGallery();

            yield return new WaitForSeconds(0.2f);

            // Assert
            Assert.AreEqual(displayStyle, emptyContainer.style.display.ToString().Trim());
        }

        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator ContentContainerVisibleWhenActivityLogListHasEntries()
        {
            // Arrange
            var displayStyle = DisplayStyle.Flex.ToString().Trim();

            // Act
            inspectionReviewBuilder.Hide();
            galleryBuilder.GetPhotoList(photos);
            galleryBuilder.BuildGallery();

            yield return new WaitForSeconds(0.2f);

            // Assert
            Assert.AreEqual(displayStyle, galleryBuilder.ContentContainer.style.display.ToString().Trim());
        }

        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator ImageGalleyContainsOnePhoto()
        {
            // Arrange
            var displayStyle = DisplayStyle.Flex.ToString().Trim();
            VisualElement photosContainer = galleryBuilder.ContentContainer.Q<VisualElement>("ImageScroll");

            // Act

            yield return null;

            // Assert
            Assert.AreEqual(photos.Count, 1);
            Assert.AreEqual(photos.Count, photosContainer.childCount);
        }

        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator DeletePhotoConfirmedRemovesPhotoFromUI()
        {
            // Arrange
            var displayStyle = DisplayStyle.Flex.ToString().Trim();
            VisualElement photosContainer = galleryBuilder.ContentContainer.Q<VisualElement>("ImageScroll");

            // Verify container has photos before deletion
            Assert.IsTrue(photosContainer != null, "ImageScroll container should not be null");
            Assert.AreEqual(photos.Count, photosContainer.childCount, "Container should have same number of photos as the photos list");

            // Act - Delete the photo
            galleryBuilder.DeletePhotoConfirmed(photos[0].Id, photos[0].Location);

            yield return new WaitForSeconds(0.2f);

            // Assert
            Assert.AreEqual(0, photosContainer.childCount, "Container should be empty after photo deletion");
        }

        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator ConfirmationDialogInvokesDeletePhotoWhenConfirmed()
        {
            // --- Arrange

            // Create a flag to track if the event was invoked
            bool confirmationDialogInvoked = false;
            string invokedPhotoId = null;

            // Subscribe to the OnShowConfirmationDialog event
            galleryBuilder.OnShowConfirmationDialog.AddListener((dialog) =>
            {
                confirmationDialogInvoked = true;
                // This simulates the user clicking "Confirm" on the dialog
                dialog.InvokePrimaryAction();
            });

            // Also track if DeletePhoto event was invoked
            bool deletePhotoInvoked = false;
            galleryBuilder.DeletePhoto.AddListener((id) =>
            {
                deletePhotoInvoked = true;
                invokedPhotoId = id;
            });

            // --- Act
            inspectionReviewBuilder.Hide();
            galleryBuilder.GetPhotoList(photos);
            galleryBuilder.BuildGallery();

            yield return new WaitForSeconds(0.2f);

            // Find the ImageScroll container
            VisualElement photosContainer = galleryBuilder.ContentContainer.Q<VisualElement>("ImageScroll");
            Assert.IsNotNull(photosContainer, "ImageScroll container should not be null");
            Assert.IsTrue(photosContainer.childCount > 0, "ImageScroll container should have children");

            // Find the first image container
            VisualElement imageContainer = photosContainer.Children().First();
            Assert.IsNotNull(imageContainer, "Image container should not be null");

            // Find the delete button
            Button deleteButton = imageContainer.Q<Button>("DeleteButton");
            Assert.IsNotNull(deleteButton, "Delete button should not be null");

            // This simulates what happens in GalleryBuilder when the button is clicked
            string photoId = photos[0].Id;
            string photoLocation = photos[0].Location;

            // Create a test-specific ConfirmDialogSO to use for our test
            var testDialog = ScriptableObject.CreateInstance<ConfirmDialogSO>();
            testDialog.SetPrimaryAction(() => galleryBuilder.DeletePhotoConfirmed(photoId, photoLocation));

            // Manually invoke the confirmation dialog
            galleryBuilder.OnShowConfirmationDialog?.Invoke(testDialog);

            yield return new WaitForSeconds(0.2f);

            // --- Assert
            Assert.IsTrue(confirmationDialogInvoked, "Confirmation dialog should have been invoked");
            Assert.IsTrue(deletePhotoInvoked, "DeletePhoto event should have been invoked");
            Assert.AreEqual(photoId, invokedPhotoId, "The correct photo ID should be passed to DeletePhoto");
            Assert.AreEqual(0, photosContainer.childCount, "Container should have no photo after deletion");

            // Clean up
            galleryBuilder.OnShowConfirmationDialog.RemoveAllListeners();
            galleryBuilder.DeletePhoto.RemoveAllListeners();
            Object.Destroy(testDialog);
        }
    }
}
