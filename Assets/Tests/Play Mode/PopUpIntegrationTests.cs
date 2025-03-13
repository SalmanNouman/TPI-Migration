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
    public class PopUpIntegrationTests
    {
        public class GalleryBuilderIntegrationTest
        {
            // Inspection review window
            private UIDocument inspectionReviewDoc;
            private InspectionReviewBuilder inspectionReviewBuilder;
            private GalleryBuilder galleryBuilder;
            private VisualElement root;
            private List<InspectablePhoto> photos;
            private PopUpBuilder popUpBuilder;

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
                popUpBuilder = GameObject.FindAnyObjectByType<PopUpBuilder>();

                root = inspectionReviewDoc.rootVisualElement;

                TimerManager.Instance.StartTimers();

                photos = new();
                photos.Add(new InspectablePhoto(new byte[14000], "Bathroom_SoapDispenser", "Bathroom", TimerManager.Instance.GetElapsedTime()));

                Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
            }

            [UnityTest, Order(1)]
            [Category("BuildServer")]
            public IEnumerator PopUpImageCanBeLoadedUsingInspectablePhoto()
            {
                // Arrange
                InspectablePhoto photo = photos[0];

                // Act
                popUpBuilder.HandleDisplayUI(photo);
                yield return null;

                // Assert
                Assert.AreEqual(DisplayStyle.Flex.ToString().Trim(), popUpBuilder.Root.style.display.ToString().Trim());
                Assert.AreEqual(photo.Location, popUpBuilder.Root.Q<Label>("Primary").text);
                Assert.AreEqual(photo.ParseNameFromID(photo.Id), popUpBuilder.Root.Q<Label>("Secondary").text);

                // Clean up
                popUpBuilder.Hide();
            }

            [UnityTest, Order(2)]
            [Category("BuildServer")]
            public IEnumerator PopUpImageCanBeLoadedUsingObjectID()
            {
                // Arrange
                InspectablePhoto photo = photos[0];
                string objectId = photo.Id;
                popUpBuilder.GetPhotosList(photos);

                // Act
                popUpBuilder.HandleDisplayUIFromInspectionLog(objectId);
                yield return null;

                // Assert
                Assert.AreEqual(DisplayStyle.Flex.ToString().Trim(), popUpBuilder.Root.style.display.ToString().Trim());
                Assert.AreEqual(photo.Location, popUpBuilder.Root.Q<Label>("Primary").text);
                Assert.AreEqual(photo.ParseNameFromID(photo.Id), popUpBuilder.Root.Q<Label>("Secondary").text);

                // Clean up
                popUpBuilder.Hide();
            }
        }
    }
}
