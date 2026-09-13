#define FIREBASE
#undef FIREBASE

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if FIREBASE
    using Firebase.Extensions;
#endif

namespace MeowStudio.Analytics
{
    public class FirebaseService
    {
        private bool initialiationFinished = false;

        public async UniTask Initialize()
        {
#if FIREBASE
            Print("Start initialize Firebase");

            if (Application.platform == RuntimePlatform.IPhonePlayer)
                Application.targetFrameRate = 60;

            Dictionary<string, object> defaults = new Dictionary<string, object>();
            AddDefaultRemoteValues(default);

            await Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults)
              .ContinueWithOnMainThread(task =>
              {
                  FetchDataAsync();
              });
            await UniTask.WaitUntil(()=> initialiationFinished);
#endif
        }
        private void AddDefaultRemoteValues(Dictionary<string, object> dict)
        {
            dict = new Dictionary<string, object>();
            dict.Add(FirebaseRemoteParam.remote_param_1.ToString(), "remote_param_1");
        }

#if FIREBASE
        private Task FetchDataAsync()
        {
            Print("Fetching data...");
            Task fetchTask =
            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                TimeSpan.Zero);
            return fetchTask.ContinueWithOnMainThread(FetchComplete);
        }
        void FetchComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                Print("Fetch canceled");
            }
            else if (fetchTask.IsFaulted)
            {
                Print("Fetch encountered an error");
            }
            else if (fetchTask.IsCompleted)
            {
                Print("Fetch completed successfully!");
            }

            var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case Firebase.RemoteConfig.LastFetchStatus.Success:
                    Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        Print(string.Format("Remote data loaded and ready (last fetch time {0})", info.FetchTime));
                    });
                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case Firebase.RemoteConfig.FetchFailureReason.Error:
                            Print("Fetch failed for unknown reason");
                            break;
                        case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                            Print("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Pending:
                    Print("Latest Fetch call still pending.");
                    break;
            }
            initialiationFinished = true;
        }

        public float GetFloatValue(FirebaseRemoteParam paramType)
        {
            string resultString = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(paramType.ToString()).StringValue;

            string[] numbers = resultString.Split('.');
            float result = int.Parse(numbers[0]);
            if (numbers.Length > 1)
                result += (float)int.Parse(numbers[1]) / 10;

            Print($"Get float value '{paramType}' = {result}");
            return result;
        }
        public int GetIntValue(FirebaseRemoteParam paramType)
        {
            int result = (int)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(paramType.ToString()).LongValue;
            Print($"Get int value '{paramType}' = {result}");
            return result;
        }
        public string GetStringValue(FirebaseRemoteParam paramType)
        {
            string result = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(paramType.ToString()).StringValue;
            Print($"Get string value '{paramType}' = {result}");
            return result;
        }
        public bool GetBoolValue(FirebaseRemoteParam paramType)
        {
            string stringResult = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                .GetValue(paramType.ToString()).StringValue.ToString();
            bool result = stringResult.Contains("true") || stringResult.Contains("True");
            Print($"Get bool value '{paramType}' = {result}");
            return result;
        }

        public void LogEvent(FirebaseEventType eventType)
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventType.ToString());
            Print($"Firebase log event '{eventType}'");
        }
        public void LogEvent(FirebaseEventType eventType, int value)
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent($"{eventType}_{value}");
            Print($"Firebase log event '{eventType}' value {value}");
        }
        public void LogEvent(FirebaseEventType eventType, string value)
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent($"{eventType}_{value}");
            Print($"Firebase log event '{eventType}' value {value}");
        }
        public void LogEvent(string value)
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(value);
            Print($"Firebase log event '{value}'");
        }
#endif

        private bool logEnabled = false;
        private void Print(string message)
        {
            Debug.Log(message);
        }
    }
}
public enum FirebaseRemoteParam
{
    remote_param_1
}
public enum FirebaseEventType
{
    event_1
}
