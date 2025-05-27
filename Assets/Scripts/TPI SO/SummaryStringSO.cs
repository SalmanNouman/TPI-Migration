using UnityEngine;

namespace VARLab.DLX
{
    [CreateAssetMenu(fileName = "SummaryStringSO", menuName = "ScriptableObjects/SummarySO")]
    public class SummaryStringSO : ScriptableObject
    {
        [Header("Window Title")]
        public string InspectionSummary;

        [Header("Overview")]
        public string InspectionDate;
        public string Facility;
        public string TattooAndPiercingStudio;
        public string TotalTime;
        public string NonCompliances;
        public string Compliances;
        public string Locations;

        [Header("Downloads")]
        public string DownloadInfo;
        public string PDFContains;
        public string InspectionLog;
        public string ActivityLog;
        public string ZIPContains;
        public string PhotoGallery;

        [Header("Locations")]
        public string LocationsNotInspected;

        [Header("Buttons")]
        public string PrimaryButton;
        public string SecondaryButton;
    }
}
