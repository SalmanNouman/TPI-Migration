using CORE_ExportTool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.DLX
{
    /// <summary>
    /// Manages the download of inspection reports, photos, and activity logs.
    /// Uses JSZip, BlobStream, and PDFKit for generating downloadable content.
    /// </summary>
    public class DownloadManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the Activity Log object")]
        private ActivityLog activityLog;

        [SerializeField, Tooltip("Reference to the inspections object")]
        private Inspections inspections;

        [SerializeField, Tooltip("Reference to the Image Handler object")]
        private ImageHandler imageHandler;

        // Constants
        private const string Facility = "Tattoo and Piercing Studio";
        private const string ActivityLogTitle = "ACTIVITY LOG";
        private const int ImageBatchSize = 150;

        // Lists for storing compliant and non-compliant items
        private List<string> compliantList = new();
        private List<string> nonCompliantList = new();

        // List for storing Poi data
        private List<Poi> poiDataList = new();

        // Hardcoded summary strings for now
        private readonly string[] summaryStrings = new string[]
        {
            "TATTOO PARLOR INSPECTION SIMULATION DETAILS",
            "Thank you for completing the inspection. This document includes the Inspection summary, Compliances, Non-compliances and Activity log.",
            "INSPECTION SUMMARY",
            "Inspection Date:",
            "Inspected by:",
            "Facility:",
            "Non-Compliances Reported:",
            "Compliances Reported:",
            "Locations Inspected:",
            "Total Time Taken:"
        };

        /// <summary>
        /// Event triggered when a download is initiated
        /// </summary>
        public UnityEvent DownloadTriggered;

        /// <summary>
        /// Initiates the download process for inspection reports and photos
        /// </summary>
        public void DownloadFiles()
        {
            DownloadTriggered?.Invoke();
#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(DownloadCoroutine());
            // Analytics event here
#endif
        }
        /// <summary>
        /// Coroutine that triggers all the downloads.
        /// </summary>
        /// <returns>Added a wait to space out the downloads and avoid security warnings.</returns>
        private IEnumerator DownloadCoroutine()
        {
            // Download the PDF first
            DownloadPDF();
            yield return new WaitForSeconds(2);
            // Download the zip file(s) with the photos after 2 seconds
            yield return StartCoroutine(DownloadAllPhotos());
        }

        /// <summary>
        /// Generates and downloads the PDF document.
        /// </summary>
        private void DownloadPDF()
        {
            // Create the PDF document using PDFKit
            // This will be implemented using JavaScript interop in WebGL
            // JavaScript function to create a new PDF document
            generatePdf.CreatePDFDocument();

            // Add the summary section
            AddSummary();

            // Add a new page for compliances and non-compliances
            generatePdf.AddPage();

            // Add compliances and non-compliances
            AddCompliancesAndNonCompliances();

            // Add activity log
            AddActivityLog();

            // Get the logo data
            string base64LogoData = GetLogoData("pdfLogo");

            // Add header and footer
            generatePdf.AddHeader(DateTime.Now.ToString("ddd MMM d yyyy HH:mm:ss 'EST'"), base64LogoData, Fonts.TimesRoman, "black", 10, 72);
            generatePdf.AddFooter("Centre for Virtual Reality Innovation", Fonts.TimesRoman, 9, "black", Fonts.TimesRoman, 10, "black", 72);

            // Generate the filename
            string fileName = "Inspection_Summary_" + GetLearnerName() + "_" +
                              DateTime.Now.ToString("ddMMMyyyy") + "_" +
                              DateTime.Now.ToString("HHmmss") + ".pdf";

            // Download the PDF
            generatePdf.DownloadPDF("Inspection Summary_" + GetLearnerName() + "_" +
            DateTime.Now.ToString("ddMMMyyyy") + " " + DateTime.Now.ToString("HH'H'mm'M'ss'S'") + ".pdf");
        }

        /// <summary>
        /// Downloads all the photos from the gallery in batches to prevent memory overload.
        /// Uses JSZip to create zip files and adds photos as base64 strings.
        /// </summary>
        public IEnumerator DownloadAllPhotos()
        {
            // Exit if there are no photos to download
            if (imageHandler == null || imageHandler.Photos == null || !imageHandler.Photos.Any())
            {
                yield break;
            }

            // Calculate batches
            int totalPhotos = imageHandler.Photos.Count;
            int totalBatches = Mathf.CeilToInt(totalPhotos / (float)ImageBatchSize);
            var photoList = imageHandler.Photos.ToList();

            // Sort photos by timestamp
            photoList.Sort((a, b) => string.Compare(a.Timestamp, b.Timestamp, StringComparison.Ordinal));

            // Process each batch
            for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
            {
                string zipId = $"zip_{batchIndex}";

                // Create a new zip file using JSZip
                GenerateZip.CreateZip(zipId);

                int start = batchIndex * ImageBatchSize;
                int end = Mathf.Min(start + ImageBatchSize, totalPhotos);

                // Add photos to the zip file
                for (int i = start; i < end; i++)
                {
                    var photo = photoList[i];
                    string fileName = GetSafeFileName(photo.Id, photo.Timestamp);

                    // Convert photo data to base64
                    string base64Data = Convert.ToBase64String(photo.Data);

                    // Add the photo to the zip file
                    GenerateZip.AddPhotoToZip(zipId, fileName, base64Data);
                }

                // Generate zip filename
                string zipFileName = totalBatches == 1 ? "Inspection_Photos.zip" : $"Inspection_Photos_{batchIndex + 1}.zip";

                // Download the zip file
                GenerateZip.DownloadZipFile(zipId, zipFileName);

                // Wait before processing the next batch
                yield return new WaitForSeconds(2);
            }
        }

        /// <summary>
        /// Creates a safe filename by replacing any special characters with underscores.
        /// </summary>
        /// <param name="id">The ID of the photo</param>
        /// <param name="timeStamp">The timestamp of the photo</param>
        /// <returns>A safe filename string</returns>
        private string GetSafeFileName(string id, string timeStamp)
        {
            string format = ".png";
            string fileName = $"{id}_{timeStamp}{format}";
            Regex reg = new Regex("[*'\",&#^@:;+]");
            return reg.Replace(fileName, "_");
        }

        /// <summary>
        /// Adds the summary section to the PDF document
        /// </summary>
        private void AddSummary()
        {
            // Top line
            generatePdf.AddLine("round", 72, 72, 540, 72, "#44546a");

            // Public Health Inspection Title
            generatePdf.AddContentWithXAndYPositions(summaryStrings[0], 20, Fonts.TimesRoman, "center", "#44546a", 72, 87, 0.5f);

            // Bottom Line
            generatePdf.AddLine("round", 72, 140, 540, 140, "#44546a");

            // add scenario number
            generatePdf.AddContentWithXAndYPositions("Scenario #", 10.5f, Fonts.TimesRoman, "center", "black", 72, 170, 0);

            // add message
            generatePdf.AddContentWithXAndYPositions(summaryStrings[1], 12, Fonts.TimesItalic, "center", "black", 72, 195, 3.5f);

            // add inspection summary title
            generatePdf.AddTextWithUnderline(summaryStrings[2], Fonts.TimesBold, 16, 1.5f);

            // Add inspection summary details
            CreateSummaryList();
        }

        /// <summary>
        /// Adds the activity log section to the PDF document
        /// </summary>
        public void AddActivityLog()
        {
            // Add the activity log title
            generatePdf.AddTextWithUnderline(ActivityLogTitle, Fonts.TimesBold, 16, 0.8f);

            // Check if activity log manager exists and has logs
            if (activityLog != null && activityLog.ActivityLogList != null && activityLog.ActivityLogList.Any())
            {
                // Add each log entry to the PDF
                foreach (Log log in activityLog.ActivityLogList)
                {
                    if (log.IsPrimary)
                    {
                        // Add primary log to the PDF
                        generatePdf.AddContent(log.Message, 12, Fonts.TimesBold, "left", "black");
                    }
                    // For secondary logs, smaller text
                    generatePdf.AddContent(log.Message, 11, Fonts.TimesRoman, "left", "black");
                }
            }
        }

        /// <summary>
        /// Creates the summary list with the details of the inspection
        /// </summary>
        private void CreateSummaryList()
        {
            string tempString = "";
            int count = 0;
            string learnerName = GetLearnerName();
            
            for (int i = 3; i <= 9; i++)
            {
                // Reset temp string
                tempString = "";
                
                tempString = summaryStrings[i] + " ";
                count = tempString.Length;
                
                switch (i)
                {
                    case 3: // Date
                        tempString += DateTime.Now.ToString("MMMM d, yyyy");
                        break;
                    case 4: // Learner Name
                        tempString += learnerName;
                        break;
                    case 5: // Facility
                        tempString += Facility;
                        break;
                    case 6: // Non-Compliances
                        // Count non-compliant inspections
                        int nonComplianceCount = 0;
                        if (inspections != null && inspections.InspectionsList != null)
                        {
                            nonComplianceCount = inspections.InspectionsList.Count(i => !i.IsCompliant);
                        }
                        tempString += nonComplianceCount.ToString();
                        break;
                    case 7: // Compliances
                        // Count compliant inspections
                        int complianceCount = 0;
                        if (inspections != null && inspections.InspectionsList != null)
                        {
                            complianceCount = inspections.InspectionsList.Count(i => i.IsCompliant);
                        }
                        tempString += complianceCount.ToString();
                        break;
                    case 8: // Locations Inspected
                        // Get POIs with inspectables and count those that have been interacted with
                        int inspectedCount = 0;
                        int totalCount = 0;
                        if (poiDataList != null)
                        {
                            var poisWithInspectables = poiDataList.Where(p => p.HasInspectables).ToList();
                            // Get inspected POIs (those with Interacted = true)
                            var inspectedPois = poisWithInspectables.Where(p => p.Interacted).ToList();
                            inspectedCount = inspectedPois.Count;
                            totalCount = poisWithInspectables.Count;
                        }

                        tempString += $"{inspectedCount} / {totalCount}";
                        break;
                    case 9: // Total Time
                        tempString += TimerManager.Instance != null ? TimerManager.Instance.GetElapsedTime() : "00:00:00";
                        break;
                }
                
                // Add the formatted item to the PDF
                generatePdf.AddSlicedStringFontAndColour(tempString, count, tempString.Length, Fonts.TimesBold, Fonts.TimesRoman, "black", "black", 0.8f);
            }
        }

        /// <summary>
        /// Gets the PDF logo as a base64 string
        /// </summary>
        /// <param name="resourceName">The name of the resource to load</param>
        /// <returns>A base64 string of the logo data or an empty string if there's an error</returns>
        public string GetLogoData(string resourceName)
        {
            // Check if the resourceName is valid
            if (string.IsNullOrEmpty(resourceName))
            {
                Debug.LogError("GetLogoData: Resource name is null or empty.");
                return string.Empty;
            }

            try
            {
                // Load the texture from Resources
                Texture2D logoTexture = Resources.Load<Texture2D>(resourceName);

                // Check if the texture was loaded successfully
                if (logoTexture == null)
                {
                    Debug.LogError($"GetLogoData: Unable to load resource '{resourceName}' from Resources.");
                    return string.Empty;
                }

                // Convert the texture to a byte array
                byte[] logoData = logoTexture.EncodeToPNG();

                // Check if the byte array is valid
                if (logoData == null || logoData.Length == 0)
                {
                    Debug.LogError("GetLogoData: Failed to encode texture to PNG.");
                    return string.Empty;
                }

                // Convert byte array to base64
                string base64LogoData = Convert.ToBase64String(logoData);
                return base64LogoData;
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions and log the error
                Debug.LogError($"GetLogoData: An error occurred while getting logo data. Error: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Adds the compliances and non-compliances sections to the PDF
        /// </summary>
        private void AddCompliancesAndNonCompliances()
        {
            // Generate the lists of compliant and non-compliant items
            GetCompliantAndNonCompliantList();

            // Add the compliance header
            generatePdf.AddTextWithUnderline("INSPECTION LOG: COMPLIANCES", Fonts.TimesBold, 16, 1);

            // Set font for the list
            generatePdf.SetFont(Fonts.TimesRoman, 12);

            // Add the compliance list
            generatePdf.AddNumberedListItem(compliantList);

            // Page break
            generatePdf.AddPage();

            // Add the non-compliance header
            generatePdf.AddTextWithUnderline("INSPECTION LOG: NON-COMPLIANCES", Fonts.TimesBold, 16, 1);

            // Set font for the numbered list
            generatePdf.SetFont(Fonts.TimesRoman, 12);

            // Add the compliance list
            generatePdf.AddNumberedListItem(nonCompliantList);

            generatePdf.AddPage();
        }

        /// <summary>
        /// Creates lists of compliant and non-compliant inspection items
        /// </summary>
        private void GetCompliantAndNonCompliantList()
        {
            // Clear the lists before populating them
            compliantList.Clear();
            nonCompliantList.Clear();

            // Check if inspection manager exists and has inspections
            if (inspections != null && inspections.InspectionsList != null)
            {
                foreach (var inspection in inspections.InspectionsList)
                {
                    // Get location and name information
                    string location = PoiList.GetPoiName(inspection.Obj.Location.ToString());
                    string name = inspection.Obj.Name;

                    // Create the entry text
                    string entryText = $"{location} - {name} - Visual";

                    // Add to appropriate list based on compliance status
                    if (inspection.IsCompliant)
                    {
                        compliantList.Add(entryText);
                    }
                    else
                    {
                        nonCompliantList.Add(entryText);
                    }
                }
            }

            // Sort the lists alphabetically
            compliantList.Sort();
            nonCompliantList.Sort();
        }

        /// <summary>
        /// Gets the learner name for the report
        /// </summary>
        /// <returns>The learner name</returns>
        public string GetLearnerName()
        {
            string learnerName = string.IsNullOrEmpty(LearnerSessionHandler.Instance?.DisplayName)
                ? "Instructor"
                : LearnerSessionHandler.Instance?.DisplayName;

            // Split the name on the comma, and trim any leading/trailing spaces
            string[] names = learnerName.Split(',');

            string firstName = names.Length > 1 ? names[1].Trim() : string.Empty;
            string lastName = names[0].Trim();

            learnerName = $"{firstName} {lastName}".Trim(); // Combine the trimmed names

            return learnerName;
        }

        /// <summary>
        /// Gets the Poi list data from Poi Handler
        /// <summary>
        public void SetPoiData(List<Poi> poiList)
        {
            // Check if the poiList is not null
            if (poiList == null || poiList.Count == 0)
            {
                Debug.LogWarning("GetPoiList: Poi list is null or empty.");
                return;
            }
            // Assign the poiDataList
            poiDataList = poiList;
        }
    }
}