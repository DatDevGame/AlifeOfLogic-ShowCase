using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using GameSparks.Core;

namespace Takuzu
{
    public class DebugCanvas : MonoBehaviour
    {
       

        public RectTransform container;
        public ScrollRect scrollRect;
        public GameObject settingGroup;
        public Logger logger;
        public Toggle iToggle;
        public Toggle wToggle;
        public Toggle eToggle;
        public Toggle fpsToggle;
        public Button showButton;
        public Button mailButton;
        public Button clearButton;
        public Text logText;
        public Text buildText;
        public Image gsConnect;
        public Text gsGuest;
        public Text fpsText;

        public bool isShow = false;
        string buildTime;

        private void Awake()
        {
            if (FindObjectOfType<DebugCanvas>() != this)
                Destroy(gameObject);
            else
                DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            clearButton.onClick.AddListener(delegate
            {
                logger.logs = new List<string>();
                logger.logsType = new List<LogType>();
            });

            showButton.onClick.AddListener(delegate
            {
                if (!isShow)
                {
                    container.pivot = new Vector2(0.5f, 0);
                }
                else
                {
                    container.pivot = new Vector2(0.5f, 1);
                }
                container.anchoredPosition = Vector2.zero;
                isShow = !isShow;
                settingGroup.SetActive(isShow);
                gsConnect.gameObject.SetActive(isShow);
                gsGuest.gameObject.SetActive(isShow);
                buildText.gameObject.SetActive(isShow);
                scrollRect.gameObject.SetActive(isShow);
                this.enabled = isShow;
            });

            mailButton.onClick.AddListener(delegate
            {
                logger.SendLog();
            });

            buildTime = Resources.Load<TextAsset>("build").text;
        }

        private void Update()
        {
            StringBuilder s = new StringBuilder();
            if (logger.logs != null)
            {
                for (int i = 0; i < logger.logs.Count; ++i)
                {
                    LogType lt = logger.logsType[i];
                    if ((lt == LogType.Log && iToggle.isOn) ||
                        (lt == LogType.Warning && wToggle.isOn) ||
                        ((lt == LogType.Error || lt == LogType.Exception) && eToggle.isOn))
                    {
                        s.Append(">>  ").Append(logger.logs[i]).Append("\n");
                    }
                }
            }

            logText.text = s.ToString();

            Color c =
                    GS.Authenticated ? Color.blue :
                    GS.Available ? Color.cyan :
                    Color.red;
            gsConnect.color = c;
            gsGuest.enabled = CloudServiceManager.isGuest;

            buildText.text = buildTime;
            fpsText.enabled = fpsToggle.isOn;
        }
    }
}