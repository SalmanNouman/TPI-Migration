using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    public class PopUpBuilder : MonoBehaviour, IUserInterface
    {
        public VisualElement Root;
        private VisualElement imageContainer;
        private List<InspectablePhoto> photos;

        // Labels
        private Label locationLabel;
        private Label objectLabel;
        private Label timeStampLabel;

        // Buttons
        private Button closeButton;

        // image set up
        private Texture2D photoTexture;
        private const int ResWidth = 770;
        private const int ResHeight = 486;

        [Header("Events")]
        [Tooltip("Invoked when window is displayed")]
        public UnityEvent OnShow;

        [Tooltip("Invoked when Hide is called.")]
        public UnityEvent OnHide;

        private void Awake()
        {
            Root = GetComponent<UIDocument>().rootVisualElement;

            photos = new List<InspectablePhoto>();

            UIHelper.Hide(Root);

            GetAllReferences();

            OnShow ??= new();
            OnHide ??= new();
        }

        public void Show()
        {
            UIHelper.Show(Root);
            OnShow?.Invoke();
        }

        public void Hide()
        {
            UIHelper.Hide(Root);
            OnHide?.Invoke();
            Destroy(photoTexture);
        }

        public void HandleDisplayUI(InspectablePhoto photo)
        {
            SetUIElements(photo);

            Show();
        }

        public void HandleDisplayUIFromInspectionLog(string objectId)
        {
            InspectablePhoto photo = photos.Find(x => x.Id == objectId);

            SetUIElements(photo);

            Show();
        }

        private void SetUIElements(InspectablePhoto photo)
        {
            photoTexture = new(ResWidth, ResHeight);
            photoTexture.LoadImage(photo.Data);

            imageContainer.style.backgroundImage = photoTexture;

            string locationName = PoiList.GetPoiName(photo.Location);
            UIHelper.SetElementText(locationLabel, locationName);
            UIHelper.SetElementText(objectLabel, photo.ParseNameFromID(photo.Id));
            UIHelper.SetElementText(timeStampLabel, photo.Timestamp);
        }

        private void GetAllReferences()
        {
            imageContainer = Root.Q<VisualElement>("Image");
            locationLabel = Root.Q<Label>("Primary");
            objectLabel = Root.Q<Label>("Secondary");
            timeStampLabel = Root.Q<Label>("TagText");
            closeButton = Root.Q<TemplateContainer>().Q<Button>("CloseBtn");

            closeButton.clicked += Hide;
        }

        public void GetPhotosList(List<InspectablePhoto> list)
        {
            photos = list;
        }
    }
}
