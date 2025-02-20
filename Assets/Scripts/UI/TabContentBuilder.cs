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

        private TemplateContainer logContainer;

        private VisualElement emptyContainer;
        private VisualElement emptyContainerIcon;
        private Label emptyContainerLabel;

        private void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            logContainer = tabContent.Instantiate();

            emptyContainer = logContainer.Q<VisualElement>("EmptyContainer");
            emptyContainerIcon = logContainer.Q<TemplateContainer>().Q<VisualElement>("Icon");
            emptyContainerLabel = logContainer.Q<TemplateContainer>().Q<Label>("Label");

            inspectionWindowTabContent = root.Q<VisualElement>("TabContent");

            SetContent();
        }

        private void SetContent()
        {
            UIHelper.SetElementText(emptyContainerLabel, label);
            emptyContainerIcon.style.backgroundImage = new StyleBackground(icon);
        }

        public void TabSelected()
        {
            inspectionWindowTabContent.Clear();
            inspectionWindowTabContent.Add(logContainer);
        }

        private void DisplayEmptyLogMessage()
        {
            emptyContainer.style.display = DisplayStyle.Flex;
        }

        private void HideEmptyLogMessage()
        {
            emptyContainer.style.display = DisplayStyle.None;
        }
    }
}
