using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.DLX;

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
    }
}
