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

        [Tooltip("Sort Button One label")]
        [SerializeField] private string sortButtonOneLabel;

        [Tooltip("Sort Button Two label")]
        [SerializeField] private string sortButtonTwoLabel;

        [Tooltip("Sort Button Three label")]
        [SerializeField] private string sortButtonThreeLabel;

        private VisualElement root;
        private VisualElement inspectionWindowTabContent;

        public TemplateContainer LogContainer;

        public VisualElement ContentContainer;
        private VisualElement emptyContainer;
        private VisualElement emptyContainerIcon;
        private Label emptyContainerLabel;

        //Add references to the Sort Button Container and Buttons
        public VisualElement SortBtnContainer;
        private Button sortBtn1;
        private Button sortBtn2;
        private Button sortBtn3;
        
        
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

            SortBtnContainer = LogContainer.Q<VisualElement>("SortButtonContainer");
            sortBtn1 = LogContainer.Q<Button>("SortBtnOne");
            sortBtn2 = LogContainer.Q<Button>("SortBtnTwo");
            sortBtn3 = LogContainer.Q<Button>("SortBtnThree");

            SetContent();
        }

        private void SetContent()
        {
            UIHelper.SetElementText(emptyContainerLabel, label);
            UIHelper.SetElementSprite(emptyContainerIcon, icon);
            UIHelper.SetElementText(sortBtn1, sortButtonOneLabel);
            UIHelper.SetElementText(sortBtn2, sortButtonTwoLabel);
            UIHelper.SetElementText(sortBtn3, sortButtonThreeLabel);
        }

        public void TabSelected()
        {
            inspectionWindowTabContent.Clear();
            inspectionWindowTabContent.Add(LogContainer);
            UIHelper.Hide(SortBtnContainer);
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
