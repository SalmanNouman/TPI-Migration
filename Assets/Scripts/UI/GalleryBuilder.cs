using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    /// <summary>
    /// This class is responsible for building the image gallery in the inspection review window.
    /// It handles the image sort buttons to filter compliant and non-compliant images, the pop-up
    /// display and removing images with or without removing the inspection.
    /// </summary>
    public class GalleryBuilder : TabContentBuilder
    {
        #region Fields
        [Tooltip("Reference to the Gallery Row UI document")]
        [SerializeField] private VisualTreeAsset galleryRow;

        [Tooltip("Reference to the Gallery Image UI document")]
        [SerializeField] private VisualTreeAsset galleryImage;

        // Confirmation Modal pop up serialized objects
        [SerializeField] private ConfirmDialogSO photoDeleteDialogSO;

        private List<Texture2D> textures;

        private List<InspectablePhoto> photos;

        private Dictionary<string, List<InspectablePhoto>> photosByLocation;

        private const int ResWidth = 770;
        private const int ResHeight = 486;

        // Flag to track if the log should also be deleted
        private bool deleteInspectionLogFlag = false;

        private Dictionary<string, TemplateContainer> imageContainers = new Dictionary<string, TemplateContainer>();
        private Dictionary<string, TemplateContainer> rowContainers = new Dictionary<string, TemplateContainer>();

        private NotificationSO notification;

        public UnityEvent<NotificationSO> OnPhotoDeleteNotification;

        #endregion

        #region UnityEvents
        /// <summary>
        /// Event triggered when a photo is deleted
        /// </summary>
        [Header("Gallery Builder Events"), Space(10f)]
        public UnityEvent<string> DeletePhoto;

        /// <summary>
        /// Event triggered when the gallery is updated
        /// </summary>
        public UnityEvent SaveGallery;

        /// <summary>
        /// Event triggered when an inspection is deleted with the photo.
        /// </summary>
        public UnityEvent<string> DeleteInspection;

        /// <summary>
        /// Event triggered to show a confirmation dialog.
        /// </summary>
        public UnityEvent<ConfirmDialogSO> OnShowConfirmationDialog;

        public UnityEvent<InspectablePhoto> OnImageClicked;

        #endregion

        /// <summary>
        /// This is used to store the data for the delete button. It stores the name and the
        /// location of the photo so we can delete it from the inspection list.
        /// </summary>
        private struct BtnData
        {
            public string Name;
            public string Location;
        }

        /// <summary>
        /// Used to get the list of photos from the image handler. It is triggered every time
        /// the photo list is changed.
        /// </summary>
        /// <param name="photosList">List of inspectable photos</param>
        public void GetPhotoList(List<InspectablePhoto> photosList)
        {
            photos = new List<InspectablePhoto>();
            photosByLocation = new Dictionary<string, List<InspectablePhoto>>();
            photos.Clear();
            photos = photosList;

            PopulatePhotosByLocationDictionary();
        }

        /// <summary>
        /// Builds the gallery when the image gallery tab is clicked.
        /// </summary>
        public void BuildGallery()
        {
            textures = new List<Texture2D>();
            DeletePhoto ??= new();
            SaveGallery ??= new();
            DeleteInspection ??= new();
            OnShowConfirmationDialog ??= new();
            OnImageClicked ??= new();
            // Create a new instance of NotificationSO to be reused
            notification = ScriptableObject.CreateInstance<NotificationSO>();
            OnPhotoDeleteNotification ??= new UnityEvent<NotificationSO>();

            if (photos == null || photos.Count <= 0)
            {
                DisplayEmptyLogMessage();
            }
            else
            {
                HideEmptyLogMessage();
                BuildAllRows();
            }
        }

        /// <summary>
        /// The gallery has 1 row per location, this will build all the rows for locations that contain photos.
        /// </summary>
        private void BuildAllRows()
        {
            ContentContainer.Clear();
            imageContainers.Clear();
            rowContainers.Clear();
            foreach (string location in photosByLocation.Keys)
            {
                TemplateContainer row = PopulateRow(location);

                if (row != null)
                {
                    ContentContainer.Add(row);
                    rowContainers.Add(location, row);
                }
            }
        }

        /// <summary>
        /// This creates a dictionary where the key is the location and the 
        /// value is a list of photos, this will sort the photos by location to add them to the gallery rows.
        /// </summary>
        private void PopulatePhotosByLocationDictionary()
        {
            photosByLocation.Clear();

            foreach (var photo in photos)
            {
                if (photosByLocation.ContainsKey(photo.Location))
                {
                    photosByLocation[photo.Location].Add(photo);
                }
                else
                {
                    photosByLocation.Add(photo.Location, new List<InspectablePhoto> { photo });
                }
            }
        }

        /// <summary>
        /// Sets up all the images for a given location.
        /// </summary>
        /// <param name="location">POI of the image.</param>
        /// <returns></returns>
        private TemplateContainer PopulateRow(string location)
        {
            TemplateContainer rowContainer = galleryRow.Instantiate();
            VisualElement imageRow = rowContainer.Q<VisualElement>("ImageScroll");

            Label locationLabel = rowContainer.Q<Label>("LocationLabel");

            UIHelper.SetElementText(locationLabel, location);

            foreach (var photo in photosByLocation[location])
            {
                // instantiate the image container
                TemplateContainer image = galleryImage.Instantiate();

                // Load the photo texture to the image background
                Texture2D tex = new(ResWidth, ResHeight);
                tex.LoadImage(photo.Data);
                textures.Add(tex);

                image.Q<TemplateContainer>().Q<VisualElement>("Image").style.backgroundImage = tex;

                // Reference of image button that will be used to display the pop-up
                Button imageBtn = image.Q<TemplateContainer>().Q<Button>("ImageButton");
                imageBtn.clicked += () => DisplayPopUp(photo);

                // Delete button reference
                Button deleteBtn = image.Q<TemplateContainer>().Q<Button>("DeleteButton");

                // Set up name and timestamp
                Label objName = image.Q<TemplateContainer>().Q<Label>("Primary");
                string objN = photo.ParseNameFromID(photo.Id);
                UIHelper.SetElementText(objName, objN);

                Label timeStampLabel = image.Q<TemplateContainer>().Q<Label>("TagText");
                UIHelper.SetElementText(timeStampLabel, photo.Timestamp);

                // Create button data to store photo information
                BtnData btnData = new BtnData
                {
                    Name = photo.Id,
                    Location = location
                };

                // add listeners to delete button
                deleteBtn.clicked += () =>
                {
                    // Set up the confirmation dialog actions
                    photoDeleteDialogSO.SetPrimaryAction(() => DeletePhotoConfirmed(btnData.Name, btnData.Location));
                    photoDeleteDialogSO.SetToggleAction((val) => deleteInspectionLogFlag = val);

                    // Show the confirmation dialog
                    OnShowConfirmationDialog?.Invoke(photoDeleteDialogSO);
                };

                imageRow.Add(image);
                imageContainers.Add(photo.Id, image);
            }

            // Checks if image row already has children
            if (imageRow.childCount <= 0)
            {
                rowContainer = null;
            }

            return rowContainer;
        }

        private void DisplayPopUp(InspectablePhoto photo)
        {
            OnImageClicked?.Invoke(photo);
        }

        /// <summary>
        /// Destroys the textures created to populate the gallery images.
        /// </summary>
        public void DestroyTextures()
        {
            if (textures == null) { return; }

            if (textures.Count > 0 || textures != null)
            {
                foreach (var tex in textures)
                {
                    Destroy(tex);
                }
            }
        }

        /// <summary>
        /// Deletes a photo and optionally its associated inspection based on the checkbox state
        /// </summary>
        /// <param name="photoId">ID of the photo to delete</param>
        /// <param name="location">Location of the photo</param>
        public void DeletePhotoConfirmed(string photoId, string location)
        {
            // First remove the image from the UI immediately for instant feedback
            RemovePhotoFromTemplateContainer(photoId, location);

            // Trigger the DeletePhoto event with the photo ID
            DeletePhoto?.Invoke(photoId);

            // Create a notification to inform the user about the deletion
            notification.NotificationType = NotificationType.Success;
            notification.Alignment = Align.FlexStart;
            notification.FontSize = FontSize.Medium;

            // If the checkbox was checked, also delete the associated inspection
            if (deleteInspectionLogFlag)
            {
                DeleteInspection?.Invoke(photoId);
                notification.Message = "Photo and inspection deleted";
            }
            else
            {
                notification.Message = "Photo deleted";
            }

            // Show the notification
            OnPhotoDeleteNotification?.Invoke(notification);

            // Reset the flag for future use
            deleteInspectionLogFlag = false;

            SaveGallery?.Invoke();
        }

        /// <summary>
        /// Removes an image from the UI immediately without waiting for the gallery to rebuild
        /// </summary>
        /// <param name="photoId">ID of the photo to remove</param>
        /// <param name="location">Location of the photo</param>
        public void RemovePhotoFromTemplateContainer(string photoId, string location)
        {
            // Check if we have this image container
            if (imageContainers.TryGetValue(photoId, out TemplateContainer imageContainer))
            {
                // Get the parent row
                if (rowContainers.TryGetValue(location, out TemplateContainer rowContainer))
                {
                    // Get the image scroll container
                    VisualElement imageRow = rowContainer.Q<VisualElement>("ImageScroll");

                    // Remove the image from its parent
                    imageRow.Remove(imageContainer);

                    // Remove from our tracking dictionary
                    imageContainers.Remove(photoId);

                    // If the row is now empty, remove it too
                    if (imageRow.childCount == 0)
                    {
                        ContentContainer.Remove(rowContainer);
                        rowContainers.Remove(location);

                        // If all photos are removed, show empty message
                        if (ContentContainer.childCount == 0)
                        {
                            DisplayEmptyLogMessage();
                        }
                    }
                }
            }
        }
    }
}
