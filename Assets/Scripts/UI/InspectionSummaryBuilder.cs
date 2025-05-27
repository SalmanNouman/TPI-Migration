using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    /// Manages the Inspection Summary window that displays inspection results and statistics.
    /// Uses an event-based approach for data communication to improve testability.
    /// </summary>
    public class InspectionSummaryBuilder : MonoBehaviour, IUserInterface
    {
        [Header("UI References")]
        [SerializeField, Tooltip("Reference to the UI Document")]
        private UIDocument inspectionSummaryDoc;

        [Header("Events")]
        [Tooltip("Play Again Button Events")]
        public UnityEvent PlayAgain;

        [Tooltip("Download Button Events")]
        public UnityEvent Download;

        // Root element
        private VisualElement root;

        // Overview Section
        private Label inspectionDateText;
        private Label totalTimeText;
        private Label nonCompliancesText;
        private Label compliancesText;
        private Label locationsInspectedText;

        // Locations not inspected section
        private Label locationsNotInspectedText;
        private Label locationsCountText;

        // Buttons
        private Button primaryButton;
        private Button secondaryButton;

        // Data
        private List<InspectionData> inspectionsList = new List<InspectionData>();
        private List<Poi> poiDataList = new List<Poi>();

        /// <summary>
        /// Event invoked when the Inspection Summary window is shown
        /// Blur background is enabled
        /// Point and Click Navigation is disabled
        /// Interaction handler is disabled
        /// <see cref="MenuBuilder.Hide"/>
        /// <see cref="CountupTimer.Hide"/>
        public UnityEvent OnShow;

        /// <summary>
        /// Event invoked when the Inspection Summary window is hidden
        /// Blur background is disabled
        /// Point and Click Navigation is enabled
        /// <see cref="TimerManager.ResumeTimers"/>
        /// InteractionHandler is enabled
        /// <see cref="MenuBuilder.Show"/>
        /// <see cref="CountupTimer.Show"/>
        /// </summary>
        public UnityEvent OnHide;

        private void Start()
        {
            // Initialize events
            PlayAgain ??= new UnityEvent();
            Download ??= new UnityEvent();

            // Get the root element
            root = inspectionSummaryDoc.rootVisualElement;

            // Get UI references
            GetUIElements();

            // Set up button listeners
            SetButtonListeners();

            // Hide the window initially
            UIHelper.Hide(root);
        }

        /// <summary>
        /// Gets the references for the visual elements in the UI
        /// </summary>
        private void GetUIElements()
        {
            // Overview
            inspectionDateText = root.Q<Label>("InspectionDateText");
            totalTimeText = root.Q<Label>("TotalTimeText");
            nonCompliancesText = root.Q<Label>("NonCompliancesText");
            compliancesText = root.Q<Label>("CompliancesText");
            locationsInspectedText = root.Q<Label>("LocationsText");

            // Locations not Inspected
            locationsNotInspectedText = root.Q<Label>("LocationListLabel");
            locationsCountText = root.Q<Label>("LocationCount");

            // Buttons
            primaryButton = root.Q<Button>("PrimaryButton");
            secondaryButton = root.Q<Button>("SecondaryButton");
        }

        /// <summary>
        /// Sets up the listeners for the buttons in the UI
        /// </summary>
        private void SetButtonListeners()
        {
            primaryButton.clicked += HandlePrimaryButtonClicked;
            secondaryButton.clicked += HandleSecondaryButtonClicked;
        }

        /// <summary>
        /// Handles the primary button click (Download)
        /// </summary>
        private void HandlePrimaryButtonClicked()
        {
            Download?.Invoke();
        }

        /// <summary>
        /// Handles the secondary button click (Play Again)
        /// </summary>
        private void HandleSecondaryButtonClicked()
        {
            PlayAgain?.Invoke();
        }

        /// <summary>
        /// Handles received inspections list
        /// </summary>
        /// <param name="inspections"></param>
        private void HandleInspectionsListReceived(List<InspectionData> inspections)
        {
            inspectionsList = inspections ?? new List<InspectionData>();
            UpdateInspectionUI();
        }

        /// <summary>
        /// Handles received Poi List
        /// </summary>
        /// <param name="poiData"></param>
        private void HandlePoiDataListReceived(List<Poi> poiData)
        {
            poiDataList = poiData ?? new List<Poi>();
            UpdateLocationUI();
        }

        /// <summary>
        /// Updates the inspection UI elements with current data
        /// </summary>
        private void UpdateInspectionUI()
        {
            int complianceCount = inspectionsList.Count(i => i.IsCompliant);
            int nonComplianceCount = inspectionsList.Count(i => !i.IsCompliant);

            compliancesText.text = complianceCount.ToString();
            nonCompliancesText.text = nonComplianceCount.ToString();
        }

        /// <summary>
        /// Updates the location UI elements with current data
        /// </summary>
        private void UpdateLocationUI()
        {
            // Skip if we don't have any POI data
            if (poiDataList == null || poiDataList.Count == 0)
            {
                return;
            }

            // Get all POIs with inspectables
            var poisWithInspectables = poiDataList.Where(p => p.HasInspectables).ToList();
            
            // Get inspected POIs (those with Interacted = true)
            var inspectedPois = poisWithInspectables.Where(p => p.Interacted).ToList();
            
            // Get POIs that haven't been inspected
            var notInspectedPois = poisWithInspectables.Where(p => !p.Interacted).ToList();
            
            // Update locations inspected count
            locationsInspectedText.text = $"{inspectedPois.Count} / {poisWithInspectables.Count}";

            // Update locations not inspected list
            if (notInspectedPois.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var poi in notInspectedPois)
                {
                    // Use the formatted POI name
                    sb.AppendLine(poi.PoiName);
                }
                locationsNotInspectedText.text = sb.ToString();
                locationsCountText.text = $"Count: {notInspectedPois.Count}";
            }
            else
            {
                locationsNotInspectedText.text = "All locations inspected";
                locationsCountText.text = "Count: 0";
            }
        }

        /// <summary>
        /// Updates the timer UI element with current data
        /// </summary>
        private void UpdateTimerUI()
        {
            // Format the elapsed time
            string elapsedTimeFormatted = TimerManager.Instance.ConvertTimeSpanToString();
            totalTimeText.text = elapsedTimeFormatted;
        }

        private void UpdateDateUI()
        {
            DateTime currentDate = DateTime.Now;
            string formattedDate = currentDate.ToString("MMMM dd, yyyy");
            inspectionDateText.text = formattedDate;
        }

        /// <summary>
        /// Displays the Inspection Summary window
        /// </summary>
        public void Show()
        {
            UIHelper.Show(root);
            OnShow?.Invoke();
        }

        /// <summary>
        /// Hides the Inspection Summary window
        /// </summary>
        public void Hide()
        {
            UIHelper.Hide(root);
            OnHide?.Invoke();
        }

        /// <summary>
        /// Displays the Inspection Summary UI with the data
        /// </summary>
        public void HandleDisplayUI()
        {
            Show();
        }

        // Public methods for testing and external data setting

        /// <summary>
        /// Sets inspection list directly (useful for testing)
        /// </summary>
        public void SetInspectionsList(List<InspectionData> inspections)
        {
            HandleInspectionsListReceived(inspections);
        }

        /// <summary>
        /// Sets the poiData list directly (useful for testing)
        /// </summary>
        /// <param name="poiData"></param>
        public void SetPoiData(List<Poi> poiData)
        {
            HandlePoiDataListReceived(poiData);
        }

        /// <summary>
        /// Sets DateTime and Timer data directly (useful for testing)
        /// </summary>
        public void SetDateTimeAndTimer()
        {
            UpdateTimerUI();
            UpdateDateUI();
        }
    }
}