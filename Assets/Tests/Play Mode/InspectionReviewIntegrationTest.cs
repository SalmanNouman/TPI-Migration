using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.DLX;

namespace Tests.PlayMode
{
    public class InspectionReviewIntegrationTest
    {
        // Inspection review window
        private UIDocument inspectionReviewDoc;
        private InspectionReviewBuilder inspectionReviewBuilder;
        private VisualElement root;

        private const string SceneName = "InspectionReviewTestScene";

        /// <summary>
        /// Loads the inspection review test scene
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

            inspectionReviewBuilder = GameObject.FindAnyObjectByType<InspectionReviewBuilder>();
            inspectionReviewDoc = inspectionReviewBuilder.GetComponent<UIDocument>();

            root = inspectionReviewDoc.rootVisualElement;

            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
        }

        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator InspectionReviewWindowIsHiddenOnStart()
        {
            // Arrange
            var expectedResult = DisplayStyle.None;

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.AreEqual(expectedResult.ToString().Trim(), root.style.display.ToString().Trim());
        }

        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator InspectionReviewWindowDisplayStyleIsFlex()
        {
            // Arrange
            var expectedResult = DisplayStyle.Flex;

            // Act
            inspectionReviewBuilder.Show();

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.AreEqual(expectedResult.ToString().Trim(), root.style.display.ToString().Trim());
        }

        [UnityTest, Order(3)]
        [Category("BuildServer")]
        public IEnumerator TabsAreNotNull()
        {
            // Arrange
            Button tabOne;
            Button tabTwo;
            Button tabThree;

            // Act
            tabOne = root.Q<Button>("TabOne");
            tabTwo = root.Q<Button>("TabTwo");
            tabThree = root.Q<Button>("TabThree");

            yield return null;

            // Assert
            Assert.IsNotNull(tabOne);
            Assert.IsNotNull(tabTwo);
            Assert.IsNotNull(tabThree);
        }

        [UnityTest, Order(4)]
        [Category("BuildServer")]
        public IEnumerator ProgressIndicatorIsNotNull()
        {
            // Arrange
            VisualElement progressIndicator;

            // Act
            progressIndicator = root.Q<VisualElement>("ProgressIndicator");

            yield return null;

            // Assert
            Assert.IsNotNull(progressIndicator);
        }

        [UnityTest, Order(5)]
        [Category("BuildServer")]
        public IEnumerator SelectedTabHasTheCorrectClass()
        {
            // Arrange
            Button tabOne;
            Button tabTwo;
            Button tabThree;

            tabOne = root.Q<Button>("TabOne");
            tabTwo = root.Q<Button>("TabTwo");
            tabThree = root.Q<Button>("TabThree");
            string selectedTab = "navigation-horizontal-button-selected";

            // Act
            inspectionReviewBuilder.Show();
            inspectionReviewBuilder.SelectTab(tabOne);
            tabOne.ClassListContains(selectedTab);


            yield return null;

            // Assert
            Assert.IsTrue(tabOne.ClassListContains(selectedTab));
            Assert.IsFalse(tabTwo.ClassListContains(selectedTab));
            Assert.IsFalse(tabThree.ClassListContains(selectedTab));
        }

        [UnityTest, Order(6)]
        [Category("BuildServer")]
        public IEnumerator UnselectedTabsHaveTheCorrectClass()
        {
            // Arrange
            Button tabOne;
            Button tabTwo;
            Button tabThree;

            tabOne = root.Q<Button>("TabOne");
            tabTwo = root.Q<Button>("TabTwo");
            tabThree = root.Q<Button>("TabThree");
            string selectedTab = "navigation-horizontal-button-selected";

            // Act
            inspectionReviewBuilder.Show();
            inspectionReviewBuilder.SelectTab(tabTwo);
            tabOne.ClassListContains(selectedTab);


            yield return null;

            // Assert
            Assert.IsFalse(tabOne.ClassListContains(selectedTab));
            Assert.IsTrue(tabTwo.ClassListContains(selectedTab));
            Assert.IsFalse(tabThree.ClassListContains(selectedTab));
        }

        [UnityTest, Order(7)]
        [Category("BuildServer")]
        public IEnumerator OnHideDisplayStyleIsNone()
        {
            // Arrange
            var expectedResult = DisplayStyle.None;

            // Act
            inspectionReviewBuilder.Hide();

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.AreEqual(expectedResult.ToString().Trim(), root.style.display.ToString().Trim());
        }

        [UnityTest, Order(8)]
        [Category("BuildServer")]
        public IEnumerator TabOneEventInvokedWhenTabOneButtonClicked()
        {
            // Arrange
            bool tabOneInvoked = false;
            bool tabTwoInvoked = false;
            bool tabThreeInvoked = false;

            Button tabOne = root.Q<Button>("TabOne");
            Button tabTwo = root.Q<Button>("TabTwo");
            Button tabThree = root.Q<Button>("TabThree");

            inspectionReviewBuilder.OnTabOneSelected.AddListener(() => tabOneInvoked = true);
            inspectionReviewBuilder.OnTabTwoSelected.AddListener(() => tabTwoInvoked = true);
            inspectionReviewBuilder.OnTabThreeSelected.AddListener(() => tabThreeInvoked = true);

            // Act
            inspectionReviewBuilder.Show();
            var e1 = new NavigationSubmitEvent() { target = tabOne };
            tabOne.SendEvent(e1);
            var e2 = new NavigationSubmitEvent() { target = tabTwo };
            tabTwo.SendEvent(e2);
            var e3 = new NavigationSubmitEvent() { target = tabThree };
            tabThree.SendEvent(e3);

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.IsTrue(tabOneInvoked);
            Assert.IsTrue(tabTwoInvoked);
            Assert.IsTrue(tabThreeInvoked);
        }
    }
}
