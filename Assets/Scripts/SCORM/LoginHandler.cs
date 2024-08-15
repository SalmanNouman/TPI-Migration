using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using VARLab.CloudSave;
using VARLab.DeveloperTools;
using VARLab.SCORM;
using PlayFab;
using PlayFab.ClientModels;
using VARLab.Analytics;

namespace VARLab.DLX
{
    /// <summary>
    ///     This class aims to integrate three key CORE systems:
    ///     * SCORM
    ///     * Cloud Save
    ///     * CORE Analytics
    /// </summary>
    public class LoginHandler : MonoBehaviour
    {
        protected static readonly List<string> Logs = new() { };

        public static LoginHandler Instance;

        public string Username = "Development";
        public string DisplayName = "Development";

        public UnityEvent<string> LoginCompleted;

        public bool Initialized { get; protected set; } = false;
        public bool ScormLoginReceived { get; protected set; } = false;

        [Tooltip("The username used to login to the Analytics platform")]
        public string PlayFabUsername = "Development";

        [Header("Analytics Event Callbacks")]
        public UnityEvent LoginCompletedEvent;
        public UnityEvent<string> PlayFabResponse;

        // Constants for Analytics Event
        private const string EventNameLogin = "user_login";
        private const string EventKeyUsername = "username";

        // If there was anything heavier in this method, like loading large resources, it could then execute a Coroutine            
        // This method also assumes that Awake() has been executed, as per the AfterSceneLoad initialization type

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void LoadLoginHandler()
        {
            CommandInterpreter.Instance?.Add(new ScormCommand(GetInfo));

            Instance = GetInstance();

            if (ScormManager.Instance)
            {
                ScormManager.ScormMessageReceived.AddListener(Instance.HandleScormMessage);
            }

            if (ScormManager.Initialized)
            {
                Instance.AssignLoginId();
            }
            

            Instance.StartCoroutine(CheckDeployment());
        }

        /// <summary>
        ///     Attempts to find an existing LoginHandler in the scene, 
        ///     and if one does not exist, it is created
        /// </summary>
        /// <returns>An instance of a LoginHandler</returns>
        protected static LoginHandler GetInstance()
        {
            LoginHandler existing = FindObjectOfType<LoginHandler>();
            if (existing)
            {
                DontDestroyOnLoad(existing.gameObject);
                return existing;
            }

            return CreateInstance();
        }

        /// <summary>
        ///     Creates a new instance of a <see cref="LoginHandler"/>
        ///     and attaches it to a GameObject that is placed in DontDestroyOnLoad
        /// </summary>
        /// <returns>An instance of a LoginHandler</returns>
        protected static LoginHandler CreateInstance()
        {
            GameObject loginObject = new("Login Handler");
            DontDestroyOnLoad(loginObject);
            return loginObject.AddComponent<LoginHandler>();
        }

        protected static IEnumerator CheckDeployment()
        {
#if UNITY_EDITOR
            Logs.Add($"SCORM is unavailable in the Unity Editor. Using default login ID: '{Instance.Username}'");
            Instance.HandleScormMessage(ScormManager.Event.Initialized);
#endif
            yield return null;
        }

        public void HandleScormMessage(ScormManager.Event response)
        {
            // Must be an 'Initialized' event to load username
            if (response == ScormManager.Event.Initialized)
            {
                ScormLoginReceived = true;
                Logs.Add($"SCORM initialized event received at {DateTime.Now}");

                AssignLoginId();
                LoginCompleted?.Invoke(Username);

                // Send login event to analytics
                SendLoginEvent();
            }
            else
            {
                Logs.Add($"SCORM commit event received at {DateTime.Now}");
            }
        }

        public void AssignLoginId()
        {
            try
            {
                if (ScormManager.Initialized)
                {
                    Username = ScormManager.GetLearnerId();
                    DisplayName = ScormManager.GetLearnerName();
                }
            }
            catch (NullReferenceException e)
            {
                Logs.Add($"{e.Source} : {e.Message}");
                Debug.Log("SCORM was not able to properly initialize");
            }
        }
        /// <summary>
        ///     Sends a login event to the analytics platform.
        /// </summary>
        public void SendLoginEvent()
        {
            CoreAnalytics.Initialize();

            CoreAnalytics.CustomEvent(
                EventNameLogin,
                EventKeyUsername,
                Username,
                ClientEventResponseHandler,
                GenericErrorHandler
            );
        }

        /// <summary>
        ///     Callback used for the PlayFab login event in order to capture the response from PlayFab.
        /// </summary>
        /// <param name="result">Response from the PlayFab 
        public void PlayFabLoginResultHandler(LoginResult result)
        {
            PlayFabResponse?.Invoke($"User {PlayFabUsername} ({result.PlayFabId}) logged in at {Timestamp()}");
            LoginCompletedEvent?.Invoke();
        }

        /// <summary>
        ///     Callback used for the analytics event response in order to capture the response from the analytics service.
        /// </summary>
        /// <param name="response">Response from the analytics service</param>
        private void ClientEventResponseHandler(WriteEventResponse response)
        {
            if (response.Request is WriteClientPlayerEventRequest request)
            {
                Logs.Add($"{request.EventName} sent successfully at {DateTime.Now}");
            }
        }
        /// <summary>
        ///     Error handler callback for all analytics events. The response is simply
        ///     passed to the logs, which can be used to log the error or handle it elsewhere.
        /// </summary>
        /// <param name="error">Response payload containing the error that occurred.</param>
        private void GenericErrorHandler(PlayFabError error)
        {
            Logs.Add($"Error: {error.ErrorMessage}");
        }
        /// <summary>
        ///     Error handler callback for all PlayFab events. The response is simply
        ///     passed to the PlayFabResponse event, which can be used to log the error or handle it elsewhere.
        /// </summary>
        /// <param name="error">Response payload containing the error that occurred.</param>
        private void PlayFabGenericErrorHandler(PlayFabError error)
        {
            PlayFabResponse?.Invoke(error.ErrorMessage);
        }
        /// <summary>
        ///     Generates a timestamp in the form HH:MM:SS for the time 
        ///     since the application launched.
        /// </summary>
        /// <returns>Timestamp as string</returns>
        public static string Timestamp()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(Time.time);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
        /// <summary>
        ///     Returns a formatted string with information about the SCORM context
        /// </summary>
        /// <returns>
        ///     Formatted string with SCORM info, learner ID, and name
        /// </returns>
        public static string GetInfo()
        {
            if (!Instance) { return "SCORM Login Handler not properly configured."; }

            StringBuilder builder = new();

            builder.Append($"SCORM Info:\n");
            builder.Append($"* Loaded:\t\t{ScormManager.Initialized}\n");
            builder.Append($"* Learner:\t\t{Instance.Username}\n");
            builder.Append($"* Name:\t\t{Instance.DisplayName}\n");

            if (Logs.Count > 0)
            {
                builder.Append($"* Logs:\n\t{string.Join("\n\n\t", Logs)}");
            }

            return builder.ToString();
        }
    }
}