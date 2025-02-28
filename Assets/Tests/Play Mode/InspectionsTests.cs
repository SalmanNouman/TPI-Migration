using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.DLX;

namespace Tests.PlayMode
{
    /// <summary>
    ///     Tests for the Inspections System ensuring that inspections can be added, checked, updated and deleted correctly.
    /// </summary>
    public class InspectionsTests
    {
        //GameObjects used in testing
        private GameObject inspectableOneGO; // used as a test object in the unity scene.
        private GameObject inspectableTwoGO;
        private GameObject inspectionsGO;

        //using a concrete class ToggleableInspectable
        private ToggleableInspectable inspectableOne;
        private ToggleableInspectable inspectableTwo;
        private Inspections inspectionsManager;

        //inspectionData instances for testing
        public InspectionData InspectOneData;
        public InspectionData InspectTwoData;


        /// <summary>
        ///     One-time setup method that runs before all tests. Initializes test objects, assigns required components, and sets default properties.
        /// </summary>
        [OneTimeSetUp]
        [Category("BuildServer")]
        public void RunBeforeAllTests()
        {
            // create the game objects using script
            inspectableOneGO = new GameObject("InspectableOne");
            inspectableTwoGO = new GameObject("InspectableTwo");
            inspectionsGO = new GameObject("Inspections");

            //adding required component for toggleable inspectable to work
            inspectableOneGO.AddComponent<BoxCollider>();
            inspectableTwoGO.AddComponent<BoxCollider>();

            // Add a dummy Camera to each inspectable game object
            Camera camOne = inspectableOneGO.AddComponent<Camera>();
            Camera camTwo = inspectableTwoGO.AddComponent<Camera>();


            //add any components
            inspectableOne = inspectableOneGO.AddComponent<ToggleableInspectable>();
            inspectableTwo = inspectableTwoGO.AddComponent<ToggleableInspectable>();

            // Set the Camera reference so that the Start() method doesn't log the warning
            inspectableOne.Cam = camOne;
            inspectableTwo.Cam = camTwo;

            // Create dummy toggleable GameObjects and add them to the Toggleables list, prevents null reference issues when toggling is performed.
            GameObject toggleableOne = new GameObject("ToggleableOne");
            GameObject toggleableTwo = new GameObject("ToggleableTwo");
            inspectableOne.Toggleables.Add(toggleableOne);
            inspectableTwo.Toggleables.Add(toggleableTwo);

            //setting up the inspectable properties (name and location)
            inspectableOne.Name = "ObjectOne";
            inspectableOne.Location = PoiList.PoiName.Bathroom;
            inspectableTwo.Name = "ObjectTwo";
            inspectableTwo.Location = PoiList.PoiName.Bathroom;

            //setting up inspections manager bby adding the component to its game object.
            inspectionsManager = inspectionsGO.AddComponent<Inspections>();
            //initialize the inspections list explicitly
            inspectionsManager.InspectionsList = new List<InspectionData>();

            // Create InspectionData instances.
            InspectOneData = new InspectionData(inspectableOne, true, true);
            InspectTwoData = new InspectionData(inspectableTwo, true, false);

        }

        /// <summary>
        ///     Test to verify that the inspections list is initially empty.
        /// </summary>
        [UnityTest, Order(0)]
        public IEnumerator CheckIfListEmpty()
        {
            //Arrange
            int expectedResult = 0;

            //Act
            yield return null;

            //Assert
            Assert.AreEqual(expectedResult, inspectionsManager.InspectionsList.Count);
        }

        /// <summary>
        ///     Test to verify that adding an inspection increases the count by one.
        /// </summary>
        [UnityTest, Order(1)]
        public IEnumerator AddInspectionToList()
        {
            //Arrange
            int expectedResult = 1;

            //Act
            inspectionsManager.AddInspection(InspectOneData);//added inspection
            yield return null;

            //Assert
            Assert.AreEqual(expectedResult, inspectionsManager.InspectionsList.Count);
        }

        /// <summary>
        ///     Test to verify that updating an existing inspection modifies the correct record without adding a duplicate.
        /// </summary>
        [UnityTest, Order(2)]
        public IEnumerator CheckInspectionWorks()
        {
            //Arrange
            // Get the current count of inspections
            int initialCount = inspectionsManager.InspectionsList.Count;
            //create new inspection data with false compliance objectONE
            InspectionData updatedData = new InspectionData(inspectableOne, false, InspectOneData.HasPhoto);


            //Act
            // Add the updated inspection, which updates the existing record without changing the count
            inspectionsManager.AddInspection(updatedData);
            yield return null;//unity's way of letting the system process the changes (wait one frame)


            //Assert
            //Retrieve the updated inspection record
            InspectionData result = inspectionsManager.CheckInspection(inspectableOne); //looks up the inspection record for ObjectOne
            Assert.IsNotNull(result); // checks if there's an inspection for object one
            Assert.IsFalse(result.IsCompliant); // checks that the compliance status of the inspection record is now false
            Assert.AreEqual(initialCount, inspectionsManager.InspectionsList.Count); // confirms system did not add a new record and only updated.
        }

        /// <summary>
        ///     test to verify that deleting an inspection removes the correct record and decreases the list count.
        /// </summary>
        [UnityTest, Order(3)]
        public IEnumerator DeleteInspectionWorks()
        {
            // create an inspection and try to delete
            //Arrange
            bool inspectionExistsBefore = inspectionsManager.CheckInspection(inspectableTwo) != null;
            if (!inspectionExistsBefore)//check if inspection exists
            {
                //if no inspection found then it adds an inspection using InspectTwoData
                inspectionsManager.AddInspection(InspectTwoData);
                yield return null;//unity process a frame.
            }
            int countBeforeDelete = inspectionsManager.InspectionsList.Count; //inspections before delete

            //Act
            inspectionsManager.DeleteInspection(inspectableTwo); //calls delete inspection which removes the inspection record.
            yield return null; //process a frame.


            //Assert
            InspectionData result = inspectionsManager.CheckInspection(inspectableTwo); //check if inspection still exists
            bool inspectionExistAfter = result != null; //if result is null the deletion was successful

            Assert.IsFalse(inspectionExistAfter); //checks that inspectionExistAfter is false meaning the inspection was actually deleted.
            Assert.AreEqual(countBeforeDelete - 1, inspectionsManager.InspectionsList.Count); // ensures that the number of inspections decreased by exactly 1.
        }

        [UnityTest, Order(4)]
        public IEnumerator PhotoDoesNotGetDeleteInReinspection()
        {
            //Arrange
            inspectionsManager.DeleteInspection(inspectableOne);
            InspectionData firstInspection = new InspectionData(inspectableOne, true, true);
            InspectionData secondInspection = new InspectionData(inspectableOne, false, false);

            //Act
            inspectionsManager.AddInspection(firstInspection);
            inspectionsManager.AddInspection(secondInspection);
            InspectionData recordedInspection = inspectionsManager.CheckInspection(inspectableOne);
            yield return null;

            //Assert
            Assert.IsTrue(recordedInspection.HasPhoto);
            Assert.IsFalse(recordedInspection.IsCompliant);
        }

        /// <summary>
        ///     Tests if RetrievePreviousInspection correctly retrieves inspection data and invokes OnPreviousInspectionRetrieved event.
        /// </summary>
        [UnityTest, Order(5)]
        public IEnumerator RetrievePreviousInspection_InvokesEventWithCorrectData()
        {
            // Arrange
            InspectionData expectedInspection = null;
            InspectionData retrievedInspection = null;
            inspectionsManager.OnPreviousInspectionRetrieved.AddListener((inspection) => retrievedInspection = inspection);
            // Add test inspection data
            expectedInspection = new InspectionData(inspectableOne, true, true);
            inspectionsManager.AddInspection(expectedInspection);
            yield return null;  // Wait for inspection to be added

            // Act
            inspectionsManager.RetrievePreviousInspection(inspectableOne);
            yield return null;  // Wait for event processing

            // Assert
            Assert.IsNotNull(retrievedInspection,
                "RetrievePreviousInspection did not invoke the event with inspection data");
            Assert.AreEqual(expectedInspection.Obj.ObjectId, retrievedInspection.Obj.ObjectId,
                "Retrieved inspection object ID does not match expected inspection");
            Assert.AreEqual(expectedInspection.IsCompliant, retrievedInspection.IsCompliant,
                "Retrieved inspection compliance status does not match expected inspection");
            Assert.AreEqual(expectedInspection.HasPhoto, retrievedInspection.HasPhoto,
                "Retrieved inspection photo status does not match expected inspection");
        }
    }
}
