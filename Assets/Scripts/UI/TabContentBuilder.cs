using UnityEngine;
using UnityEngine.UIElements;
using VARLab.Velcro;

namespace VARLab.DLX
{
    public class TabContentBuilder : MonoBehaviour
    {
        [Tooltip("Reference to the Tab Content template")]
        [SerializeField] private VisualTreeAsset tabContent;

        [Tooltip("Empty container icon")]
        [SerializeField] private Sprite icon;

        [Tooltip("Empty container label")]
        [SerializeField] private string label;

        private VisualElement root;
        private VisualElement inspectionWindowTabContent;

        public TemplateContainer LogContainer;

        public VisualElement ContentContainer;
        private VisualElement emptyContainer;
        private VisualElement emptyContainerIcon;
        private Label emptyContainerLabel;

        private void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            LogContainer = tabContent.Instantiate();

            LogContainer.style.flexGrow = 1;
            ContentContainer = LogContainer.Q<ScrollView>("ScrollView");
            emptyContainer = LogContainer.Q<VisualElement>("EmptyContainer");
            emptyContainerIcon = LogContainer.Q<TemplateContainer>().Q<VisualElement>("Icon");
            emptyContainerLabel = LogContainer.Q<TemplateContainer>().Q<Label>("Label");

            inspectionWindowTabContent = root.Q<VisualElement>("TabContent");

            SetContent();
        }

        private void SetContent()
        {
            UIHelper.SetElementText(emptyContainerLabel, label);
            UIHelper.SetElementSprite(emptyContainerIcon, icon);
        }

        public void TabSelected()
        {
            inspectionWindowTabContent.Clear();
            inspectionWindowTabContent.Add(LogContainer);
        }

        public void DisplayEmptyLogMessage()
        {
            UIHelper.Show(emptyContainer);
            UIHelper.Hide(ContentContainer);
        }

        public void HideEmptyLogMessage()
        {
            UIHelper.Show(ContentContainer);
            UIHelper.Hide(emptyContainer);
        }
    }
}
