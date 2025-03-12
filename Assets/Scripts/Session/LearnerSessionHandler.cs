using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using VARLab.DeveloperTools;
using VARLab.SCORM;

namespace VARLab.DLX
{
    public interface ISessionHandler
    {
        public static ISessionHandler Instance { get; set; }
    }

    /// <summary>
    ///     This class manages the "user session" during runtime by 
    ///     integrating three key CORE systems:
    ///     * SCORM - receives a unique user ID
    ///     * Cloud Save - individual save states based on user ID
    ///     * CORE Analytics - pushes user metrics to external endpoint
    /// </summary>
    public class LearnerSessionHandler : MonoBehaviour
    {
        /// <summary> A collection of log messages generated during runtime </summary>
        protected static readonly List<string> Logs = new() { };

        /// <summary> Static instance so that the LoginHandler can be accessed externally </summary>
        public static LearnerSessionHandler Instance;
        public static IAnalyticsWrapper Analytics = new CoreAnalyticsWrapper();

        // Fields tracking the current user session
        public string Username;
        public string DisplayName = "Development";

        [Header("Events")]
        public UnityEvent<string> SessionStarted = new();
        public UnityEvent<string> AnalyticsConnected = new();


        // Properties
        public bool Initialized { get; protected set; } = false;
        public bool ScormLoginReceived { get; protected set; } = false;



        /// <summary>
        ///     If there was anything heavier in this method, like loading large resources, 
        ///     it could then execute a Coroutine. This method also assumes that Awake() has 
        ///     been executed, as per the AfterSceneLoad initialization type
        /// </summary>
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
                Instance.StartCoroutine(Instance.GetUsernameCoroutine());
            }

            Instance.StartCoroutine(CheckDeploymentCoroutine());
        }

        /// <summary>
        ///     Attempts to find an existing LoginHandler in the scene, 
        ///     and if one does not exist, it is created
        /// </summary>
        /// <returns> An instance of a <see cref="LearnerSessionHandler"> </returns>
        protected static LearnerSessionHandler GetInstance()
        {
            LearnerSessionHandler existing = FindFirstObjectByType<LearnerSessionHandler>();
            if (existing)
            {
                DontDestroyOnLoad(existing.gameObject);
                return existing;
            }

            return CreateInstance();
        }

        /// <summary>
        ///     Creates a new instance of a <see cref="LearnerSessionHandler"/>
        ///     and attaches it to a GameObject that is placed in DontDestroyOnLoad
        /// </summary>
        /// <returns> An instance of a <see cref="LearnerSessionHandler"> </returns>
        protected static LearnerSessionHandler CreateInstance()
        {
            GameObject loginObject = new("Login Handler");
            DontDestroyOnLoad(loginObject);
            return loginObject.AddComponent<LearnerSessionHandler>();
        }


        /// <summary>
        ///     Receives the response from the SCORM Javascript wrapper library 
        ///     indicating it has been initialized.
        /// </summary>
        /// <remarks>
        ///     This method currently does not care abut <see cref="ScormManager.Event.Commit"/> 
        ///     events, but they are still logged.
        /// </remarks>
        /// <param name="response">
        ///     The response from the SCORM library. Expected to be <see cref="ScormManager.Event.Initialized"/>
        /// </param>
        public void HandleScormMessage(ScormManager.Event response)
        {
            // Must be an 'Initialized' event to load username
            if (response == ScormManager.Event.Initialized)
            {
                ScormLoginReceived = true;
                Logs.Add($"SCORM initialized event received at {DateTime.Now}");

                StartCoroutine(StartSessionCoroutine());
            }
            else
            {
                Logs.Add($"SCORM commit event received at {DateTime.Now}");
            }
        }

        /// <summary>
        ///     Coroutine intended to check if the application is running in the Unity Editor.
        ///     If so, a mock SCORM message is crafted to load the session using the default username.
        /// </summary>
        /// <remarks>
        ///     When running in the Unity Editor, all sessions will then save over the same file. 
        ///     If uniqueness in the Unity Editor or non-SCORM contexts (ie WebGL but not in an LMS)
        ///     is desired, then a unique value like the device GUID or MAC address can be used.
        /// </remarks>
        protected static IEnumerator CheckDeploymentCoroutine()
        {

#if UNITY_EDITOR
            Instance.Username = SystemInfo.deviceUniqueIdentifier;
            Logs.Add($"SCORM is unavailable in the Unity Editor. Using default login ID: '{Instance.Username}'");
            Instance.HandleScormMessage(ScormManager.Event.Initialized);
#endif
            yield return null;
        }

        /// <summary>
        ///     Coroutine intended to start the user session by invoking the <see cref="SessionStarted"/>
        ///     event with the current <see cref="Username"/> as the argument. This allows listeners
        ///     to read the current user name as needed.
        /// </summary>
        /// <remarks>
        ///     After the session is started, the Analytics system is then logged in
        /// </remarks>
        /// <returns></returns>
        public IEnumerator StartSessionCoroutine()
        {
            yield return GetUsernameCoroutine();

            SessionStarted?.Invoke(Username);

            yield return null;  // Wait frame to allow for other code paths to run

            // Send login event to analytics
            StartCoroutine(AnalyticsLoginCoroutine());
        }

        /// <summary>
        ///     Coroutine intended to pull the username and display name from SCORM
        /// </summary>
        public IEnumerator GetUsernameCoroutine()
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
            }

            yield return null;
        }

        /// <summary>
        ///     Sends a login event to the analytics platform.
        /// </summary>
        public IEnumerator AnalyticsLoginCoroutine()
        {
            Analytics.Initialize();
            yield return null;

            Logs.Add($"Login request sent at {DateTime.Now}");

            Analytics.Login(Username,
                successCallback: LoginResultHandler,
                errorCallback: ErrorResponseHandler);
        }

        /// <summary>
        ///     Callback used for the PlayFab login event in order to capture the response from PlayFab.
        /// </summary>
        /// <param name="result">Response from the PlayFab 
        private void LoginResultHandler(string id)
        {
            Logs.Add($"Login response received at {DateTime.Now} for user ID '{id}'");
            AnalyticsConnected?.Invoke(id);
        }

        /// <summary>
        ///     Error handler callback for all analytics events. The response is simply
        ///     passed to the logs, which can be used to log the error or handle it elsewhere.
        /// </summary>
        /// <param name="error">Message string for the error that occurred.</param>
        private void ErrorResponseHandler(string error)
        {
            Logs.Add($"Error response received at {DateTime.Now}: {error}");
#if UNITY_EDITOR
            Debug.LogWarning(error);
#endif
        }


        #region Debug

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

        #endregion
    }
}
