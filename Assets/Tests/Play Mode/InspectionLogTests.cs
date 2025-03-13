using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
        private TpiTable table;
        private const string SceneName = "InspectionReviewTestScene";
        private List<InspectionData> inspectionList;
        private List<InspectionData> tempList;
        private InspectableObject inspectable;
        private GameObject inspectableGameObject;
        private Button allButton;
        private Button compliantButton;
        private Button nonCompliantButton;
        
        private enum SortType
        {
            All,
            Compliant,
            NonCompliant
        }
       
        /// <summary>
        /// Loads the inspection review test scene
        /// </summary>
        [UnitySetUp]
        [Category("BuildServer")]
        public IEnumerator RunOnce()
        {
            SceneManager.LoadScene(SceneName);
            yield return null;

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

            nonCompliantButton = inspectionLogBuilder.SortBtnContainer.Q<Button>("SortBtnThree");
            allButton = inspectionLogBuilder.SortBtnContainer.Q<Button>("SortBtnOne");
            compliantButton = inspectionLogBuilder.SortBtnContainer.Q<Button>("SortBtnTwo");
            yield return null;

        }
        /// <summary>
        /// Checks if the test scene is loaded
        /// </summary>
        [UnityTest, Order(0)]
        [Category("BuildServer")]
        public IEnumerator SceneLoaded()
        {
            yield return new WaitUntil(() => SceneManager.GetSceneByName(SceneName).isLoaded);

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

        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator WhenRowRemovedInvokeDeleteInspection()
        {
            // Arrange
            bool eventTriggered = false; //flag to indicate whether deleteInspection was fired
            string capturedObjectId = ""; //holds objectId when event is triggered
            inspectionLogBuilder.DeleteInspection = new UnityEvent<string>(); //subcribe to deleteInspection
            //when event fires, it sets eventTriggered to true and assigns the passed id to captured object ID
            inspectionLogBuilder.DeleteInspection.AddListener(id => { eventTriggered = true; capturedObjectId = id; });

            //gets list to store in inspectionList
            inspectionLogBuilder.GetInspectionList(inspectionList);
            //does display
            inspectionLogBuilder.HandleDisplayInspectionLog();
            yield return new WaitForSeconds(0.2f);

            // Create a TableEntry instance.
            TpiTableEntry tableEntry = new TpiTableEntry();

            // reflection is used to access a private field, used in this case to get elements inside table entry class
            var elementsField = typeof(TpiTableEntry).GetField("elements", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(elementsField, "Could not find the private field 'elements' in TableEntry.");//makes sure not null

            // Create and assign a new list with test data.
            var testElements = new List<TpiTableElement>
            {
                new TpiTableElement { Text = "Reception" },  // Column 0: Location
                new TpiTableElement { Text = "Test" }          // Column 1: Item Name
            };
            elementsField.SetValue(tableEntry, testElements);

            // Act
            // Invoke the private OnRowRemoved method using reflection.
            var onRowRemovedMethod = typeof(InspectionLogBuilder)
                .GetMethod("OnRowRemoved", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(onRowRemovedMethod, "Could not find the private method OnRowRemoved.");

            onRowRemovedMethod.Invoke(inspectionLogBuilder, new object[] { tableEntry });

            // Wait a frame for events to process.
            yield return null;

            // Assert
            Assert.IsTrue(eventTriggered, "DeleteInspection event was not triggered.");
            Assert.AreEqual("Reception_Test", capturedObjectId, "The objectId passed to DeleteInspection is incorrect.");
        }

        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator WhenClickButtonAllInvokeAddSortButtonListener()
        {
            // Arrange
            inspectionLogBuilder.SelectButton(nonCompliantButton);
          
            // Act
            inspectionLogBuilder.SelectButton(allButton);

            yield return null;

            // Assert
            Assert.AreEqual(allButton, inspectionLogBuilder.CurrentButton);  
        }

        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator WhenClickButtonCompliantInvokeAddSortButtonListener()
        {
            // Arrange
            inspectionLogBuilder.SelectButton(nonCompliantButton);

            // Act
            inspectionLogBuilder.SelectButton(compliantButton);

            yield return null;

            // Assert
            Assert.AreEqual(compliantButton, inspectionLogBuilder.CurrentButton);   
        }

        [UnityTest, Order(6)]
        [Category("BuildServer")]
        public IEnumerator WhenClickButtonNonCompliantInvokeAddSortButtonListener()
        {
            // Arrange
            inspectionLogBuilder.SelectButton(nonCompliantButton);
          
            // Act
            inspectionLogBuilder.SelectButton(nonCompliantButton);

            yield return null;

            // Assert
            Assert.AreEqual(nonCompliantButton, inspectionLogBuilder.CurrentButton);
        }
    }
}
