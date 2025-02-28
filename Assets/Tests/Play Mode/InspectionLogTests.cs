using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.DLX;
using VARLab.Velcro;

namespace Tests.PlayMode
{
    public class InspectionLogTests
    {
        // Inspection review window
        private UIDocument inspectionReviewDoc;
        private InspectionReviewBuilder inspectionReviewBuilder;
        private InspectionLogBuilder inspectionLogBuilder;
        private VisualElement root;
        private Table table;
        private const string SceneName = "InspectionReviewTestScene";
        private List<InspectionData> inspectionList;
        private InspectableObject inspectable;
        private GameObject inspectableGameObject;

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
            inspectionLogBuilder = GameObject.FindAnyObjectByType<InspectionLogBuilder>();
            root = inspectionReviewDoc.rootVisualElement;

            inspectableGameObject = new();
            inspectableGameObject.AddComponent<BoxCollider>();
            inspectable = inspectableGameObject.AddComponent<InspectableObject>();
            inspectableGameObject.AddComponent<Camera>();

            inspectable.Name = "Test";
            inspectable.Location = PoiList.PoiName.Reception;
            inspectable.Cam = inspectableGameObject.GetComponent<Camera>();
            inspectionList = new();
            inspectionList.Add(new InspectionData(inspectable, true, true));

            Debug.Log(inspectionList.ToString());

            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
        }

        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator ContentContainerVisibleWhenInspectionLogListEmpty()
        {
            // Arrange
            var displayStyle = DisplayStyle.Flex.ToString().Trim();
            VisualElement emptyContainer = inspectionLogBuilder.LogContainer.Q<VisualElement>("EmptyContainer");

            // Act
            inspectionLogBuilder.HandleDisplayInspectionLog();

            yield return new WaitForSeconds(0.2f);

            // Assert
            Assert.AreEqual(displayStyle, emptyContainer.style.display.ToString().Trim());
        }

        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator ContentContainerVisibleWhenInspectionLogListFull()
        {
            // Arrange
            var displayStyle = DisplayStyle.Flex.ToString().Trim();

            // Act
            //InspectionLogBuilder call list
            inspectionLogBuilder.GetInspectionList(inspectionList);
            yield return new WaitForSeconds(0.2f);
            inspectionLogBuilder.HideEmptyLogMessage();
            inspectionLogBuilder.HandleDisplayInspectionLog();

            //list check if container empty of full by seeing if flex or non-flex.

            Debug.Log(inspectionList.ToString());

            yield return new WaitForSeconds(0.2f);

            // Assert
            Assert.AreEqual(displayStyle, inspectionLogBuilder.ContentContainer.style.display.ToString().Trim());
        }
    }
}
