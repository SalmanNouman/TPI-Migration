using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    public class ProgressBuilder : MonoBehaviour
    {
        [Tooltip("Progress panel title")]
        [SerializeField] private string title;

        [Tooltip("Item 1 label")]
        [SerializeField] private string itemOne;

        [Tooltip("Item 2 line 1 label")]
        [SerializeField] private string itemTwoLineOne;

        [Tooltip("Item 2 line 2 label")]
        [SerializeField] private string itemTwoLineTwo;

        [Tooltip("Item 3 label")]
        [SerializeField] private string itemThree;

        // Labels
        private Label titleLabel;
        private Label itemOneLabel;
        private Label itemTwoLineOneLabel;
        private Label itemTwoLineTwoLabel;
        private Label itemThreeLabel;
        private Label progressOneLabel;
        private Label progressTwoLabel;
        private Label progressThreeLabel;

        // root visual element
        private VisualElement root;

        [HideInInspector]
        public bool IsOpen = false;

        private int inspectablePois = 0;
        private int inspectedPois = 0;
        private int totalInspections = 0;

        private void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;

            GetReferences();
            UpdateLabels();
        }

        /// <summary>
        /// Runs every frame and updates the timer when the Inspection review window is open.
        /// </summary>
        private void Update()
        {
            if (IsOpen)
            {
                progressThreeLabel.text = TimerManager.Instance.ConvertTimeSpanToString();
            }
        }

        /// <summary>
        /// Gets reference of all UI elements that need to be updated.
        /// </summary>
        private void GetReferences()
        {
            titleLabel = root.Q<Label>("Title");
            itemOneLabel = root.Q<Label>("TaskTitle");
            itemTwoLineOneLabel = root.Q<Label>("FirstLabel");
            itemTwoLineTwoLabel = root.Q<Label>("SecondLabel");
            itemThreeLabel = root.Q<Label>("TimeRunningLabel");
            progressOneLabel = root.Q<Label>("ProgressLabel");
            progressTwoLabel = root.Q<Label>("NumberLabel");
            progressThreeLabel = root.Q<Label>("ElapsedTimeLabel");
        }

        /// <summary>
        /// Set the text to all the labels in the progress indicator.
        /// </summary>
        private void UpdateLabels()
        {
            UIHelper.SetElementText(titleLabel, title);
            UIHelper.SetElementText(itemOneLabel, itemOne);
            UIHelper.SetElementText(itemTwoLineOneLabel, itemTwoLineOne);
            UIHelper.SetElementText(itemTwoLineTwoLabel, itemTwoLineTwo);
            UIHelper.SetElementText(itemThreeLabel, itemThree);

            UIHelper.SetElementText(progressOneLabel, "0 / 00");
            UIHelper.SetElementText(progressTwoLabel, "0");
            UIHelper.SetElementText(progressThreeLabel, "0hr00mins");
        }

        /// <summary>
        /// Called from inspection review OnShow and OnHide to update the 
        /// timer, inspections, and locations inspected.
        /// </summary>
        /// <param name="open"></param>
        public void ProgressOpen(bool open)
        {
            IsOpen = open;
            UIHelper.SetElementText(progressOneLabel, $"{inspectedPois} / {inspectablePois}");
            UIHelper.SetElementText(progressTwoLabel, totalInspections.ToString());
        }

        /// <summary>
        /// Gets the total number of POIs that have inspectables.
        /// </summary>
        /// <param name="count">Total POIs that contain inspectables</param>
        public void GetPoiCount(int count)
        {
            inspectablePois = count;
        }

        /// <summary>
        /// Updated the number of POIs that have been interacted.
        /// </summary>
        /// <param name="count">Inspected POIs</param>
        public void UpdateInspectedPois(int count)
        {
            inspectedPois = count;
        }

        /// <summary>
        /// Gets the inspection list count.
        /// </summary>
        /// <param name="count">Number of inspections recorded</param>
        public void GetInspectionsCount(List<InspectionData> list)
        {
            totalInspections = list.Count;

            if (IsOpen)
            {
                UIHelper.SetElementText(progressTwoLabel, totalInspections.ToString());
            }
        }
    }
}
