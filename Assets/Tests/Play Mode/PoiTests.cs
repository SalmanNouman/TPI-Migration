using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VARLab.DLX;
using VARLab.Velcro;

namespace Tests.PlayMode
{
    /// <summary>
    ///     Play mode tests for the <see cref="Poi"/> class.
    /// </summary>
    /// <remarks>
    ///     Tests the following functionalities:
    ///     - POI initialization and configuration
    ///     - POI name formatting
    ///     - Player trigger interaction detection
    ///     - Enter/exit event handling
    /// </remarks>
    public class PoiTests
    {
        #region Fields

        private const string SceneName = "POITestScene";

        private Poi poi;
        private GameObject playerObject;
        private float safeDistance;  // Distance required for the POI trigger testing
        private PoiHandler poiHandler;
        private InspectableObject obj;

        #endregion

        #region Test Setup

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

            poi = GameObject.FindAnyObjectByType<Poi>();
            poiHandler = GameObject.FindAnyObjectByType<PoiHandler>();
            obj = GameObject.FindAnyObjectByType<InspectableObject>();
            playerObject = GameObject.FindGameObjectWithTag("Player");

            // Calculate safe distance for efficient POI trigger event testing by ensuring proper enter/exit actions
            safeDistance = poi.GetComponent<BoxCollider>().bounds.extents.x * 2.1f;

            // Disable the isInitialLoad flag for testing
            DisableInitialLoadFlag();

            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
        }

        /// <summary>
        /// Uses reflection to disable the isInitialLoad flag in PoiHandler for testing purposes
        /// </summary>
        private void DisableInitialLoadFlag()
        {
            // Get the private field using reflection
            FieldInfo isInitialLoadField = typeof(PoiHandler).GetField("isInitialLoad",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (isInitialLoadField != null)
            {
                // Set the field to false to allow event invocation during tests
                isInitialLoadField.SetValue(poiHandler, false);
            }
        }
        #endregion

        #region POI Name Formatting Tests

        /// <summary>
        ///     Tests if POI name is correctly initialized and formatted during Start.
        /// </summary>
        /// <remarks>
        ///     Verifies that PoiName property is set using PoiList.GetPoiName(),
        ///     which converts enum names to user-friendly format (e.g., "TattooArea" to "Tattoo Area").
        /// </remarks>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator Poi_Start_InitializesNameCorrectly()
        {
            // Arrange
            poi.SelectedPoiName = PoiList.PoiName.TattooArea;
            string expectedName = "Tattoo Area";
            yield return null;  // Wait for Start() to be called

            // Act
            // Start() method is called

            // Assert
            Assert.AreEqual(expectedName, poi.PoiName);
        }

        #endregion

        #region Trigger Event Tests

        /// <summary>
        ///     Tests if OnPoiEnter event is properly invoked when a player enters the POI trigger zone.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator Poi_OnTriggerEnter_InvokesOnEnterEvent()
        {
            // Arrange
            bool eventInvoked = false;  // Initialize the event status to false
            bool handlerTriggerInvoked = false;
            poi.OnPoiEnter.AddListener(poiHandler.HandlePoiEnter);
            poi.OnPoiEnter.AddListener((poi) => eventInvoked = true);  // Set status to true when the event is triggered
            poiHandler.OnPoiEnter.AddListener((poi) => handlerTriggerInvoked = true);
            playerObject.transform.position = poi.gameObject.transform.position + Vector3.right * safeDistance; // Position the player outside the trigger zone
            yield return new WaitForFixedUpdate(); // Wait for physics update

            // Act
            playerObject.transform.position = poi.gameObject.transform.position; // Move the player into the POI trigger zone
            yield return new WaitForFixedUpdate(); // Wait for physics update

            // Assert
            Assert.IsTrue(eventInvoked, "OnPoiEnter event was not invoked.");
            Assert.IsTrue(handlerTriggerInvoked);
        }

        /// <summary>
        ///     Tests if OnPoiEnter event in the POI Handler is properly invoked when a player enters the POI trigger zone.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator Poi_OnTriggerEnter_InvokesHandlerOnEnterEvent()
        {
            // Arrange
            bool handlerTriggerInvoked = false;
            poi.OnPoiEnter.AddListener(poiHandler.HandlePoiEnter);
            poiHandler.OnPoiEnter.AddListener((poi) => handlerTriggerInvoked = true);
            playerObject.transform.position = poi.gameObject.transform.position + Vector3.right * safeDistance; // Position the player outside the trigger zone
            yield return new WaitForFixedUpdate(); // Wait for physics update

            // Act
            playerObject.transform.position = poi.gameObject.transform.position; // Move the player into the POI trigger zone
            yield return new WaitForFixedUpdate(); // Wait for physics update

            // Assert
            Assert.IsTrue(handlerTriggerInvoked);
        }

        /// <summary>
        ///     Tests if OnPoiExit event is properly invoked when a player exits the POI trigger zone.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator Poi_OnTriggerExit_InvokesOnExitEvent()
        {
            // Arrange
            bool eventInvoked = false;  // Initialize the event status to false
            poi.OnPoiExit.AddListener((poi) => eventInvoked = true);  // Set status to true when the event is triggered
            playerObject.transform.position = poi.gameObject.transform.position; // First, position the player inside the trigger zone (required to test exit)
            yield return new WaitForFixedUpdate(); // Wait for physics update

            // Act
            playerObject.transform.position = poi.gameObject.transform.position + Vector3.right * safeDistance; // Move the player out of the POI trigger zone
            yield return new WaitForFixedUpdate(); // Wait for physics update

            // Assert
            Assert.IsTrue(eventInvoked, "OnPoiExit event was not invoked.");
        }

        /// <summary>
        ///     Tests if OnPoiExit event is properly invoked when a player exits the POI trigger zone.
        /// </summary>
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator Poi_OnTriggerExit_InvokesHandlerOnExitEvent()
        {
            // Arrange
            bool handlerTriggerInvoked = false;  // Initialize the event status to false
            poiHandler.OnPoiExit.AddListener((poi) => handlerTriggerInvoked = true);  // Set status to true when the event is triggered
            poi.OnPoiExit.AddListener(poiHandler.HandlePoiExit);
            playerObject.transform.position = poi.gameObject.transform.position; // First, position the player inside the trigger zone (required to test exit)
            yield return new WaitForFixedUpdate(); // Wait for physics update

            // Act
            playerObject.transform.position = poi.gameObject.transform.position + Vector3.right * safeDistance; // Move the player out of the POI trigger zone
            yield return new WaitForFixedUpdate(); // Wait for physics update

            // Assert
            Assert.IsTrue(handlerTriggerInvoked);
        }

        #endregion

        #region POI Handler
        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator CheckPoiInteracted_TriggersPoiInteracted_IfPoiNotInteracted()
        {
            // Arrange
            bool wasTriggered = false;
            poiHandler.PoiInteracted.AddListener((int count) => wasTriggered = true);
            GameObject inspectableGO = new("Inspectable");
            inspectableGO.AddComponent<BoxCollider>();
            InspectableObject inspectable = inspectableGO.AddComponent<InspectableObject>();
            inspectable.Name = "Test";
            inspectable.Location = PoiList.PoiName.TattooArea;
            inspectable.Interacted = false;
            poi.HasInspectables = true;
            poi.Interacted = false;

            // Act
            poiHandler.CheckPoiInteracted(inspectable);

            yield return null;

            // Assert
            Assert.IsTrue(wasTriggered);
        }

        [UnityTest]
        [Category("BuildServer")]
        public IEnumerator CheckPoiInteracted_DoesNotTriggersPoiInteracted_IfPoiInteracted()
        {
            // Arrange
            bool wasTriggered = false;
            poiHandler.PoiInteracted.AddListener((int count) => wasTriggered = true);
            obj.Interacted = true;
            poi.Interacted = true;

            // Act
            poiHandler.CheckPoiInteracted(obj);

            yield return null;

            // Assert
            Assert.IsFalse(wasTriggered);
        }

        #endregion
    }
}