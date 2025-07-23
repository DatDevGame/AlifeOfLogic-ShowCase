using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace SupportService
{
    public class AppsflyerLogger : ILogger
    {
        private readonly string errorEventKey = "error";

        public void LogError(string message)
        {
            Debug.LogError(message);
            Dictionary<string, string> extraData = new Dictionary<string, string> { { "message", message } };

            AppsFlyer.trackRichEvent(errorEventKey, extraData);

            Dictionary<string, object> unityDictionaryData = new Dictionary<string, object>();
            foreach (var item in extraData) unityDictionaryData.Add(item.Key, item.Value);
            AnalyticsEvent.Custom(errorEventKey, unityDictionaryData);
        }
    }
}