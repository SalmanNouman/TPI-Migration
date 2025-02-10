using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.DLX;

namespace Tests.PlayMode
{
    public class InspectableObjectIntegrationTest
    {
        bool listenerTest = false;
        private GameObject inspectableGameObject;
        private GameObject handlerGameObject;
        private GameObject toggleable;
        ToggleableInspectable inspectable;

        [SetUp]
        [Category("BuildServer")]
        public void RunBeforeEveryTest()
        {
            inspectableGameObject = new();
            inspectableGameObject.AddComponent<BoxCollider>();
            inspectable = inspectableGameObject.AddComponent<ToggleableInspectable>();
            inspectableGameObject.AddComponent<Camera>();
            toggleable = new();
            inspectable.Toggleables.Add(toggleable);

            inspectable.Name = "Test";
            inspectable.Location = PoiList.PoiName.Reception;
            inspectable.Cam = inspectableGameObject.GetComponent<Camera>();

            inspectable.States = new();
            inspectable.States.Add(new State(inspectableGameObject, Compliancy.Compliant));
            inspectable.States.Add(new State(inspectableGameObject, Compliancy.NonCompliant));

            handlerGameObject = new GameObject("HandlerObject");
            handlerGameObject.AddComponent<InspectionHandler>();
        }

        // Test Object Name and Location are set in the Inspectable
        [Test]
        [Category("BuildServer")]
        public void CheckObjectId()
        {
            //arrange
            string expectedResult = "Reception_Test";

            //act
            string objectId = inspectableGameObject.GetComponent<InspectableObject>().GeneratedId();

            //assert
            Assert.AreEqual(expectedResult, objectId);
        }

        // Test list of state is getting populated
        [Test]
        [Category("BuildServer")]
        public void CheckListOfObjectState()
        {
            //arrange
            int expectedResult = 2;

            //act
            List<string> stateList = inspectableGameObject.GetComponent<InspectableObject>().GetListOfObjectStates();

            //assert
            Assert.AreEqual(expectedResult, stateList.Count);
        }

        // Test Photo Taken Event is getting trigger
        [Test]
        [Category("BuildServer")]
        public void CheckPhotoTaken()
        {
            //arrange
            bool expectedResult = true;
            InspectableObject obj;

            //act
            obj = inspectableGameObject.GetComponent<InspectableObject>();
            obj.HasPhoto = false;
            obj.PhotoTaken();

            //assert
            Assert.AreEqual(expectedResult, obj.HasPhoto);
        }

        // Test Photo Delete Event is getting trigger
        [Test]
        [Category("BuildServer")]
        public void CheckPhotoDeleted()
        {
            //arrange
            bool expectedResult = false;
            InspectableObject obj;

            //act
            obj = inspectableGameObject.GetComponent<InspectableObject>();
            obj.HasPhoto = true;
            obj.PhotoDelete();

            //assert
            Assert.AreEqual(expectedResult, obj.HasPhoto);
        }

        // Test Generate Id return null if object name is null
        [UnityTest]
        public IEnumerator GenerateIdFailsIfObjectNameIsNull()
        {
            //arrange
            inspectable.Name = "";
            string expectedResult = null;

            //act
            var result = inspectable.GeneratedId();
            yield return null;

            //assert
            Assert.AreEqual(expectedResult, result);
        }

        public void Ping(InspectableObject ping)
        {
            listenerTest = true;
        }
    }
}
