using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyMobile;

namespace Takuzu
{
    public class Logger : MonoBehaviour, ILogger
    {
        public bool logInfo;
        public bool logWarning;
        public bool logError;

        public FPSCounter fpsCounter;

        public List<string> logs;
        public List<string> rawLogs;
        public List<LogType> logsType;

        public ILogHandler logHandler { get; set; }
        public bool logEnabled { get; set; }
        public LogType filterLogType { get; set; }

        private ILogHandler defaultLogHandler = Debug.unityLogger.logHandler;

        public bool showInfo = true;
        public bool showWarning = true;
        public bool showError = true;

        private void Awake()
        {
#if !UNITY_EDITOR
            Debug.unityLogger.logHandler = this;
#endif
            logs = new List<string>();

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            Debug.unityLogger.logHandler = defaultLogHandler;
        }

        public void SendLog()
        {
            if (logs == null && logs.Count == 0)
                return;

            System.Text.StringBuilder s = new System.Text.StringBuilder();
            for (int i = logs.Count - 1; i >= 0; --i)
            {
                string type = string.Format("({0})", logsType[i] == LogType.Log ? "I" : logsType[i] == LogType.Warning ? "W" : "E");
                string log = logs[i]
                    .Replace("<color=white>", string.Empty)
                    .Replace("<color=yellow>", string.Empty)
                    .Replace("<color=red>", string.Empty)
                    .Replace("</color>", string.Empty);
                s.Append(type).Append(" >> ").Append(log).Append("\n");
            }

            string content = s.ToString();
            content = WWW.EscapeURL(content).Replace("+", "%20");
            string mail = "tntam.developing@gmail.com";
            string userName = WWW.EscapeURL(CloudServiceManager.playerName ?? "").Replace("+", "%20");
            string time = WWW.EscapeURL(DateTime.Now.ToString()).Replace("+", "%20");
            string url = string.Format("mailto:{0}?subject={1}&body={2}", mail, "[" + userName + "]%20" + "PlayerDb%20" + time, content);
            Application.OpenURL(url);
        }

        private void OnGUI()
        {
            float width = Screen.width * 0.2f;
            float height = Screen.height * 0.05f;
            Rect r = new Rect(Screen.width / 2 - width / 2, 0, width, height);
            if (GUI.Button(r, "Show logs"))
            {
                ShowLog();
            }
        }

        public void ShowLog()
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            for (int i = 0; i < logs.Count; ++i)
            {
                string type = string.Format("({0})", logsType[i] == LogType.Log ? "I" : logsType[i] == LogType.Warning ? "W" : "E");
                string log = logs[i]
                    .Replace("<color=white>", string.Empty)
                    .Replace("<color=yellow>", string.Empty)
                    .Replace("<color=red>", string.Empty)
                    .Replace("</color>", string.Empty);
                s.Append(type).Append("> ").Append(log).Append("\n");
            }

            EasyMobile.NativeUI.AlertPopup.Alert("Logs", s.ToString());
        }

        public bool IsLogTypeAllowed(LogType logType)
        {
            if (logType == LogType.Log && logInfo)
                return true;
            if (logType == LogType.Warning && logWarning)
                return true;
            if ((logType == LogType.Error || logType == LogType.Exception) && logError)
                return true;
            return false;
        }

        public void Log(LogType logType, object message)
        {
            if (IsLogTypeAllowed(logType))
            {
                logs.Add(string.Format("<color={0}> {1} </color>", logType == LogType.Log ? "white" : logType == LogType.Warning ? "yellow" : "red", message.ToString()));
                logsType.Add(logType);

                if (logType == LogType.Error || logType == LogType.Exception)
                {
                    NativeUI.AlertPopup.Alert(logType.ToString(), message.ToString());
                }
            }
        }

        public void Log(LogType logType, object message, UnityEngine.Object context)
        {
            Log(logType, message);
        }

        public void Log(LogType logType, string tag, object message)
        {
            Log(logType, message);
        }

        public void Log(LogType logType, string tag, object message, UnityEngine.Object context)
        {
            Log(logType, message);
        }

        public void Log(object message)
        {
            Log(LogType.Log, message);
        }

        public void Log(string tag, object message)
        {
            Log(LogType.Log, message);
        }

        public void Log(string tag, object message, UnityEngine.Object context)
        {
            Log(LogType.Log, message);
        }

        public void LogError(string tag, object message)
        {
            Log(LogType.Error, message);
        }

        public void LogError(string tag, object message, UnityEngine.Object context)
        {
            Log(LogType.Error, message);
        }

        public void LogException(Exception exception)
        {
            Log(LogType.Exception, exception.ToString());
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            Log(LogType.Exception, exception.ToString());
            defaultLogHandler.LogException(exception, context);
        }

        public void LogFormat(LogType logType, string format, params object[] args)
        {
            Log(logType, string.Format(format, args));
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            Log(logType, string.Format(format, args));
            defaultLogHandler.LogFormat(logType, context, format, args);
        }

        public void LogWarning(string tag, object message)
        {
            Log(LogType.Warning, message.ToString());
        }

        public void LogWarning(string tag, object message, UnityEngine.Object context)
        {
            Log(LogType.Warning, message.ToString());
        }
    }
}