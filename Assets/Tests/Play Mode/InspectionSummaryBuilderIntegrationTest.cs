using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.DLX;

namespace Tests.PlayMode
{
    /// <summary>
    /// Integration tests for the InspectionSummaryBuilder class
    /// </summary>
    public class InspectionSummaryBuilderIntegrationTest
    {
        // Inspection summary window
        private UIDocument inspectionSummaryDoc;
        private InspectionSummaryBuilder inspectionSummaryBuilder;
        private VisualElement root;

        // Test data
        private List<InspectionData> testInspectionData;

        private const string SceneName = "InspectionSummaryTestScene";

        /// <summary>
        /// Loads the inspection summary test scene
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

            inspectionSummaryBuilder = GameObject.FindAnyObjectByType<InspectionSummaryBuilder>();
            inspectionSummaryDoc = inspectionSummaryBuilder.GetComponent<UIDocument>();

            root = inspectionSummaryDoc.rootVisualElement;

            // Create test data
            CreateTestData();

            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
        }

        /// <summary>
        /// Creates test inspection data for testing
        /// </summary>
        private void CreateTestData()
        {
            // Create test inspection data
            testInspectionData = new List<InspectionData>
            {
                new InspectionData { IsCompliant = true, HasPhoto = true },   // Compliant with photo
                new InspectionData { IsCompliant = false, HasPhoto = true },  // Non-compliant with photo
                new InspectionData { IsCompliant = true, HasPhoto = false }   // Compliant without photo
            };

            // Create test POI data
            var testPoiData = CreateTestPoiData();

            // Set the test data in the InspectionSummaryBuilder
            inspectionSummaryBuilder.SetInspectionsList(testInspectionData);
            inspectionSummaryBuilder.SetPoiData(testPoiData);
        }

        /// <summary>
        /// Creates test POI data for testing
        /// </summary>
        private List<Poi> CreateTestPoiData()
        {
            // Create a list of test POIs
            var poiList = new List<Poi>();

            // Create POIs for each location type
            // We'll create 3 interacted POIs and 4 non-interacted POIs

            // Interacted POIs (inspected)
            poiList.Add(CreateTestPoi(PoiList.PoiName.TattooArea, true));
            poiList.Add(CreateTestPoi(PoiList.PoiName.PiercingArea, true));
            poiList.Add(CreateTestPoi(PoiList.PoiName.Bathroom, true));

            // Non-interacted POIs (not inspected)
            poiList.Add(CreateTestPoi(PoiList.PoiName.Office, false));
            poiList.Add(CreateTestPoi(PoiList.PoiName.Lobby, false));
            poiList.Add(CreateTestPoi(PoiList.PoiName.Reception, false));
            poiList.Add(CreateTestPoi(PoiList.PoiName.ReprocessingArea, false));

            return poiList;
        }

        /// <summary>
        /// Creates a test POI with the specified location and interaction state
        /// </summary>
        private Poi CreateTestPoi(PoiList.PoiName poiName, bool interacted)
        {
            // Create a GameObject for the POI
            var poiObject = new GameObject($"TestPoi_{poiName}");

            // Add a Poi component
            var poi = poiObject.AddComponent<Poi>();

            // Set the POI properties
            poi.SelectedPoiName = poiName;
            poi.PoiName = PoiList.GetPoiName(poiName.ToString());
            poi.Interacted = interacted;
            poi.HasInspectables = true;

            return poi;
        }

        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator InspectionSummaryWindowIsHiddenOnStart()
        {
            // Arrange
            var expectedResult = DisplayStyle.None;

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.AreEqual(expectedResult.ToString().Trim(), root.style.display.ToString().Trim());
        }

        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator InspectionSummaryWindowDisplayStyleIsFlex()
        {
            // Arrange
            var expectedResult = DisplayStyle.Flex;

            // Act
            inspectionSummaryBuilder.Show();

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.AreEqual(expectedResult.ToString().Trim(), root.style.display.ToString().Trim());
        }

        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator UIElementsAreNotNull()
        {
            // Arrange
            Label inspectionDateText;
            Label totalTimeText;
            Label nonCompliancesText;
            Label compliancesText;
            Label locationsInspectedText;
            Label locationsNotInspectedText;
            Label locationsCountText;
            Button primaryButton;
            Button secondaryButton;

            // Act
            inspectionSummaryBuilder.Show();
            inspectionDateText = root.Q<Label>("InspectionDateText");
            totalTimeText = root.Q<Label>("TotalTimeText");
            nonCompliancesText = root.Q<Label>("NonCompliancesText");
            compliancesText = root.Q<Label>("CompliancesText");
            locationsInspectedText = root.Q<Label>("LocationsText");
            locationsNotInspectedText = root.Q<Label>("LocationListLabel");
            locationsCountText = root.Q<Label>("LocationCount");
            primaryButton = root.Q<Button>("PrimaryButton");
            secondaryButton = root.Q<Button>("SecondaryButton");

            yield return null;

            // Assert
            Assert.IsNotNull(inspectionDateText, "InspectionDateText not found");
            Assert.IsNotNull(totalTimeText, "TotalTimeText not found");
            Assert.IsNotNull(nonCompliancesText, "NonCompliancesText not found");
            Assert.IsNotNull(compliancesText, "CompliancesText not found");
            Assert.IsNotNull(locationsInspectedText, "LocationsText not found");
            Assert.IsNotNull(locationsNotInspectedText, "LocationListLabel not found");
            Assert.IsNotNull(locationsCountText, "LocationCount not found");
            Assert.IsNotNull(primaryButton, "PrimaryButton not found");
            Assert.IsNotNull(secondaryButton, "SecondaryButton not found");
        }

        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator SetInspectionsListUpdatesInspectionUI()
        {
            // Arrange
            Label compliancesText = root.Q<Label>("CompliancesText");
            Label nonCompliancesText = root.Q<Label>("NonCompliancesText");

            // Act
            inspectionSummaryBuilder.SetInspectionsList(testInspectionData);

            yield return null;

            // Assert
            Assert.AreEqual("2", compliancesText.text, "Compliant count incorrect");
            Assert.AreEqual("1", nonCompliancesText.text, "Non-compliant count incorrect");
        }

        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator SetPoiDataUpdatesLocationUI()
        {
            // Arrange
            Label locationsInspectedText = root.Q<Label>("LocationsText");
            Label locationsCountText = root.Q<Label>("LocationCount");
            Label locationsNotInspectedText = root.Q<Label>("LocationListLabel");

            // Create test POI data
            var testPoiData = CreateTestPoiData();

            // Act - Set the POI data
            inspectionSummaryBuilder.SetPoiData(testPoiData);

            yield return null;

            // Expected values
            int totalInspectableLocations = 7; // All locations have HasInspectables = true
            int inspectedLocations = 3; // TattooArea, PiercingArea, Bathroom
            int notInspectedLocations = 4; // Office, Lobby, Reception, ReprocessingArea

            // Assert
            Assert.AreEqual($"{inspectedLocations} / {totalInspectableLocations}", locationsInspectedText.text, "Locations inspected count incorrect");
            Assert.AreEqual($"Count: {notInspectedLocations}", locationsCountText.text, "Locations not inspected count incorrect");

            // Verify that the not inspected locations text contains the expected locations
            string notInspectedText = locationsNotInspectedText.text;
            Assert.IsTrue(notInspectedText.Contains("Office"), "Not inspected locations should contain Office");
            Assert.IsTrue(notInspectedText.Contains("Lobby"), "Not inspected locations should contain Lobby");
            Assert.IsTrue(notInspectedText.Contains("Reception"), "Not inspected locations should contain Reception");
            Assert.IsTrue(notInspectedText.Contains("Reprocessing Area"), "Not inspected locations should contain Reprocessing Area");
        }

        [UnityTest, Order(6)]
        [Category("BuildServer")]
        public IEnumerator SetDateTimeAndTimerUpdatesTimerUIAndInspectionDateUI()
        {
            // Arrange
            Label totalTimeText = root.Q<Label>("TotalTimeText");
            Label inspectionDateText = root.Q<Label>("InspectionDateText");

            // Set the timer manager instance value
            TimerManager.Instance.StartTimers();
            TimerManager.Instance.Offset = new System.TimeSpan(0, 1, 30, 45); // Set a non-zero offset for testing

            DateTime currentDateTime = DateTime.Now;
            string formattedDate = currentDateTime.ToString("MMMM dd, yyyy");

            // Act
            inspectionSummaryBuilder.SetDateTimeAndTimer();

            yield return null;

            // Assert
            Assert.IsNotNull(totalTimeText.text, "Timer text should not be null");
            Assert.IsNotEmpty(totalTimeText.text, "Timer text should not be empty");
            Assert.AreEqual(formattedDate, inspectionDateText.text, "Inspection date text does not match current date");
            Debug.Log($"Timer text value: {totalTimeText.text}");
        }

        [UnityTest, Order(7)]
        [Category("BuildServer")]
        public IEnumerator HideDisplayStyleIsNone()
        {
            // Arrange
            var expectedResult = DisplayStyle.None;

            // Act
            inspectionSummaryBuilder.Hide();

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.AreEqual(expectedResult.ToString().Trim(), root.style.display.ToString().Trim());
        }

        [UnityTest, Order(8)]
        [Category("BuildServer")]
        public IEnumerator PrimaryButtonInvokesDownloadEvent()
        {
            // Arrange
            bool downloadInvoked = false;
            Button primaryButton = root.Q<Button>("PrimaryButton");
            inspectionSummaryBuilder.Download.AddListener(() => downloadInvoked = true);

            // Act
            inspectionSummaryBuilder.Show();
            var e = new NavigationSubmitEvent() { target = primaryButton };
            primaryButton.SendEvent(e);
            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.IsTrue(downloadInvoked, "Download event was not invoked when primary button was clicked");
            
            // Cleanup
            inspectionSummaryBuilder.Download.RemoveAllListeners();
            inspectionSummaryBuilder.Hide();
        }
        
        [UnityTest, Order(9)]
        [Category("BuildServer")]
        public IEnumerator SecondaryButtonInvokesPlayAgainEvent()
        {
            // Arrange
            bool playAgainInvoked = false;
            Button secondaryButton = root.Q<Button>("SecondaryButton");
            inspectionSummaryBuilder.PlayAgain.AddListener(() => playAgainInvoked = true);

            // Act
            inspectionSummaryBuilder.Show();
            var e = new NavigationSubmitEvent() { target = secondaryButton };
            secondaryButton.SendEvent(e);
            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.IsTrue(playAgainInvoked, "PlayAgain event was not invoked when secondary button was clicked");
            
            // Cleanup
            inspectionSummaryBuilder.PlayAgain.RemoveAllListeners();
            inspectionSummaryBuilder.Hide();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            // Clean up test data
            if (testInspectionData != null)
            {
                testInspectionData.Clear();
                testInspectionData = null;
            }

            // Clean up POI GameObjects
            var testPois = GameObject.FindObjectsOfType<Poi>();
            foreach (var poi in testPois)
            {
                if (poi.name.StartsWith("TestPoi_"))
                {
                    GameObject.Destroy(poi.gameObject);
                }
            }
        }
    }
}
