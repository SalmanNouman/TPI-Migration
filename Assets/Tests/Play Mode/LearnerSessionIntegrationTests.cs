using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.DLX;
using VARLab.SCORM;

namespace Tests.PlayMode
{
    /// <summary>
    ///     Tests for integrating CloudSave and SCORM services through the 
    ///     boilerplate code provided in the DLX template
    /// </summary>
    [TestFixture]
    public class LearnerSessionIntegrationTests
    {
        private const string TestUsername = "TestUsername";

        private LearnerSessionHandler learnerSession;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // overwrites the static Analytics handler 
            LearnerSessionHandler.Analytics = new MockAnalyticsWrapper();

            learnerSession = Object.FindAnyObjectByType<LearnerSessionHandler>();

            if (!learnerSession)
            {
                Debug.Log($"Creating new {nameof(LearnerSessionHandler)}");
                learnerSession = new GameObject().AddComponent<LearnerSessionHandler>();
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Object.Destroy(learnerSession.gameObject);
        }

        /// <summary>
        ///     LoginHandler Instance is initialized on application startup, 
        ///     therefore it should be non-null at any point during runtime
        /// </summary>
        [Test]
        public void LoginHandler_Startup_ShouldInitialize()
        {
            // Assert
            Assert.IsNotNull(LearnerSessionHandler.Instance);
        }

        /// <summary>
        ///     Expects that when the LoginHandler receives an "Initialized" event from SCORM, 
        ///     the LoginCompleted event is then invoked for other objects to listen for
        /// </summary>
        [UnityTest]
        public IEnumerator LoginHandler_Login_ShouldExecuteCallback()
        {
            // Arrange
            bool usernameMatch = false;
            string nameExpected = TestUsername;
            string nameActual = string.Empty;

            learnerSession.SessionStarted.AddListener(call: (username) => { nameActual = username; });
            learnerSession.Username = nameExpected;

            // Act
            yield return null;
            learnerSession.HandleScormMessage(ScormManager.Event.Initialized);

            // Wait for at most 60 frames to ensure that coroutines are able to run
            int frame = 0;
            while (frame < 60 && !usernameMatch)
            {
                frame++;
                usernameMatch = string.Equals(nameExpected, nameActual);
                yield return null;
            }

            Debug.Log($"Waited {frame} frames before completing");

            // Assert
            Assert.That(LearnerSessionHandler.Analytics is MockAnalyticsWrapper);
            Assert.IsTrue(learnerSession.ScormLoginReceived);
            Assert.IsTrue(usernameMatch);

            Object.Destroy(learnerSession.gameObject);
        }
    }


    /// <summary>
    ///     A mocked analytics library used for testing
    /// </summary>
    internal class MockAnalyticsWrapper : IAnalyticsWrapper
    {
        private string id;

        public void Initialize()
        {
            Debug.Log("Mock initialize analytics system");
        }

        public void Login(string username, System.Action<string> successCallback, System.Action<string> errorCallback)
        {
            id = username.GetHashCode().ToString();

            if (!string.IsNullOrWhiteSpace(username))
            {
                successCallback?.Invoke(id);
            }
            else
            {
                errorCallback?.Invoke("Generic error");
            }
        }
    }

}