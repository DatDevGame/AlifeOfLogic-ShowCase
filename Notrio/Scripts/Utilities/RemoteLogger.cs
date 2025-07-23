using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Messages;
using GameSparks.Api.Requests;

namespace Takuzu
{
    public class RemoteLogger : MonoBehaviour
    {
        public static RemoteLogger Instance { get; private set; }

        public void OnEnable()
        {
            ScriptMessage.Listener += OnScriptMessageReceive;
        }

        public void OnDisable()
        {
            ScriptMessage.Listener -= OnScriptMessageReceive;
        }

        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this;
        }

        private void OnScriptMessageReceive(ScriptMessage m)
        {
            string type = m.Data.GetString("type");
            string log = m.Data.GetString("log");
            string src = m.Data.GetString("src");
            string colorCode =
                src.Equals("editor") ? "green" :
                src.Equals("android") ? "blue" :
                src.Equals("ios") ? "cyan" : "gray";

            Debug.LogFormat("{0} <b><color={1}>Remote (from {2}): {3} </color></b>",type, colorCode, src.ToUpper(), log);
        }

        public void Log(string log, LogType type = LogType.Log)
        {
#if ((UNITY_ANDROID || UNITY_IOS)) && !UNITY_EDITOR

            string logTypePrefix =
                type == LogType.Log ? "<b><color=gray>(i)</color></b>" :
                type == LogType.Warning ? "<b><color=orange>(w)</color></b>" :
                type == LogType.Error ? "<b><color=red>(e)</color></b>" : "";
            string src = "unknown";
#if UNITY_EDITOR
            src = "editor";
#elif UNITY_ANDROID
            src = "android";
#elif UNITY_IOS
            src = "ios";
#endif

            new LogEventRequest()
                .SetEventKey("REMOTE_LOG")
                .SetEventAttribute("LOG", log)
                .SetEventAttribute("SOURCE", src)
                .SetEventAttribute("TYPE", logTypePrefix)
                .Send((response) =>
                {
                    if (response.HasErrors)
                        Debug.LogWarning(response.Errors.JSON);
                });
#elif UNITY_EDITOR
            if (type == LogType.Log)
                Debug.Log(log);
            else if (type == LogType.Warning)
                Debug.LogWarning(log);
            else if (type == LogType.Error)
                Debug.LogError(log);
#endif
        }
    }
}