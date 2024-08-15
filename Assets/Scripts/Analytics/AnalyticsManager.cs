using System;
using UnityEngine;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;
using VARLab.Analytics;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    public string Username = "Development";

    [Header("Analytics Event Callbacks")]
    public UnityEvent LoginCompleted;
    public UnityEvent<string> Response;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SendLoginEvent();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SendLoginEvent()
    {
        CoreAnalytics.Initialize();
        CoreAnalytics.LoginUser(Username, LoginResultHandler, GenericErrorHandler);
    }

    public void LogCustomEvent(string eventName, string key, object value)
    {
        CoreAnalytics.CustomEvent(eventName, key, value, ClientEventResponseHandler, GenericErrorHandler);
    }

    /// <summary>
    /// Generates a timestamp string in the format HH:MM:SS.
    /// </summary>
    /// <returns>A string representing the current timestamp.</returns>
    public static string Timestamp()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(Time.time);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

    private void LoginResultHandler(LoginResult result)
    {
        Response?.Invoke($"User {Username} ({result.PlayFabId}) logged in at {Timestamp()}");
        LoginCompleted?.Invoke();
    }

    private void ClientEventResponseHandler(WriteEventResponse response)
    {
        if (response.Request is WriteClientPlayerEventRequest request)
        {
            Response?.Invoke($"{request.EventName} sent successfully at {Timestamp()}");
        }
    }

    private void GenericErrorHandler(PlayFabError error)
    {
        Response?.Invoke(error.ErrorMessage);
    }
}
