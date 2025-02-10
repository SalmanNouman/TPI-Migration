using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.DLX;

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

        private GameObject poiObject;
        private Poi poi;
        private GameObject playerObject;
        private float safeDistance;  // Distance required for the POI trigger testing

        #endregion

        #region Test Setup

        /// <summary>
        ///     Sets up the test environment.
        /// </summary>
        /// <remarks>
        ///     Creates and configures necessary test components:
        ///     - POI object for area detection
        ///     - Player object for trigger interaction testing
        /// </remarks>
        [SetUp]
        [Category("BuildServer")]
        public void Setup()
        {
            // Setup POI object
            poiObject = new GameObject("TestPOI");
            poi = poiObject.AddComponent<Poi>();
            var poiCollider = poiObject.AddComponent<BoxCollider>();
            poiCollider.isTrigger = true;

            // Initialize POI settings
            poi.SelectedPoiName = PoiList.PoiName.TattooArea;

            // Calculate safe distance for efficient POI trigger event testing by ensuring proper enter/exit actions
            safeDistance = poiCollider.bounds.extents.x * 2.1f;

            // Setup Player object
            playerObject = new GameObject("TestPlayer");
            playerObject.AddComponent<BoxCollider>();
            var rb = playerObject.AddComponent<Rigidbody>(); // Add and configure Rigidbody
            rb.isKinematic = true; // Set to kinematic to detect triggers without physics simulation
        }

        /// <summary>
        ///     Cleans up test objects.
        /// </summary>
        [TearDown]
        [Category("BuildServer")]
        public void Teardown()
        {
            Object.DestroyImmediate(poiObject);
            Object.DestroyImmediate(playerObject);
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
            poi.OnPoiEnter += () => eventInvoked = true;  // Set status to true when the event is triggered
            playerObject.transform.position = poiObject.transform.position + Vector3.right * safeDistance; // Position the player outside the trigger zone
            yield return new WaitForFixedUpdate(); // Wait for physics update

            // Act
            playerObject.transform.position = poiObject.transform.position; // Move the player into the POI trigger zone
            yield return new WaitForFixedUpdate(); // Wait for physics update

            // Assert
            Assert.IsTrue(eventInvoked, "OnPoiEnter event was not invoked.");
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
            poi.OnPoiExit += () => eventInvoked = true;  // Set status to true when the event is triggered
            playerObject.transform.position = poiObject.transform.position; // First, position the player inside the trigger zone (required to test exit)
            yield return new WaitForFixedUpdate(); // Wait for physics update

            // Act
            playerObject.transform.position = poiObject.transform.position + Vector3.right * safeDistance; // Move the player out of the POI trigger zone
            yield return new WaitForFixedUpdate(); // Wait for physics update

            // Assert
            Assert.IsTrue(eventInvoked, "OnPoiExit event was not invoked.");
        }

        #endregion
    }
}