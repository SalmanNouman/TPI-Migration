using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.DLX;

namespace Tests.PlayMode
{
    public class ActivityLogBuilderIntegrationTest
    {
        // Inspection review window
        private UIDocument inspectionReviewDoc;
        private InspectionReviewBuilder inspectionReviewBuilder;
        private ActivityLogBuilder activityLogBuilder;
        private VisualElement root;
        private List<Log> activityLog;

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
            activityLogBuilder = GameObject.FindAnyObjectByType<ActivityLogBuilder>();

            root = inspectionReviewDoc.rootVisualElement;

            activityLog = new();
            activityLog.Add(new Log(true, "Primary log one"));
            activityLog.Add(new Log(false, "Secondary log one"));
            activityLog.Add(new Log(false, "Secondary log one"));
            activityLog.Add(new Log(false, "Secondary log one"));
            activityLog.Add(new Log(true, "Primary log Two"));
            activityLog.Add(new Log(false, "Secondary log two"));
            activityLog.Add(new Log(false, "Secondary log two"));


            Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
        }

        [UnityTest, Order(1)]
        [Category("BuildServer")]
        public IEnumerator EmptyContentContainerVisibleWhenActivityLogListEmpty()
        {
            // Arrange
            var displayStyle = DisplayStyle.Flex.ToString().Trim();
            VisualElement emptyContainer = activityLogBuilder.LogContainer.Q<VisualElement>("EmptyContainer");

            // Act
            activityLogBuilder.HandleDisplayActivityLog(true);

            yield return new WaitForSeconds(0.2f); 

            // Assert
            Assert.AreEqual(displayStyle, emptyContainer.style.display.ToString().Trim());
        }

        [UnityTest, Order(2)]
        [Category("BuildServer")]
        public IEnumerator ContentContainerVisibleWhenActivityLogListEmpty()
        {
            // Arrange
            var displayStyle = DisplayStyle.Flex.ToString().Trim();

            // Act
            activityLogBuilder.HandleDisplayActivityLog(false);
            activityLogBuilder.GetActivityLog(activityLog);
            activityLogBuilder.HandleDisplayActivityLog(true);

            yield return new WaitForSeconds(0.2f);

            // Assert
            Assert.AreEqual(displayStyle, activityLogBuilder.ContentContainer.style.display.ToString().Trim());
        }
    }
}
