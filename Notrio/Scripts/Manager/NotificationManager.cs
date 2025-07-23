using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Core;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using UnityEngine.SceneManagement;
using System;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace Takuzu
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance;
        public static System.Action<string, Dictionary<string, object>, bool> onNotificationOpened = delegate
        {
        };
        public static System.Action<string, Dictionary<string, object>> onNotificationReceived = delegate
        {
        };

        public float promptIntervalDay = 1;
        public int promptCountToDisallowAsking = 1;
        public Dictionary<string, object> tags;
        public ConfirmationDialog confirmDialog;

        public const string TAG_PLAYER_NAME = "playerName";
        public const string TAG_REGION = "region";
        public const string REGION_AMERICA = "regionAmerica";
        public const string REGION_AFRICA_EUROPE_ASIA = "regionAfricaEuropeAsia";
        public const string REGION_EAST_ASIA = "regionEastAsia";
        public const string TAG_DAILY_NOTIFICATION_EXCLUDE = "dailyChallengeNotificationExclude";
        public const string TAG_WEEKLY_NOTIFICATION_EXCLUDE = "weeklyChallengeNotificationExclude";
        public const string YES = "yes";
        public const string NO = "no";
        public const string LAST_PROMPT_TIME_KEY = "NOTIFICATION_LAST_PROMPT_TIME";
        public const string PROMPT_COUNT_KEY = "NOTIFICATION_PROMPT_COUNT";
        public const string ASKING_ALLOWED_KEY = "NOTIFICATION_ASKING_ALLOWED";
        public const string PROMPT_RESULT_KEY = "NOTIFICATION_PROMPT_RESULT";

        public static bool isInitialized;

        public OSNotificationPermission PushPermission
        {
            get
            {
                OSNotificationPermission permission = OneSignal.GetPermissionSubscriptionState().permissionStatus.status;
                return permission;
            }
        }

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public void Init()
        {
#if EASY_MOBILE
            if (isInitialized)
                return;
            OneSignal.StartInit(EM_Settings.Notifications.OneSignalAppId)
               .HandleNotificationOpened(HandleNotificationOpened)
               .HandleNotificationReceived(HandleNotificationReceived)
               .InFocusDisplaying(OneSignal.OSInFocusDisplayOption.None)
               .Settings(new Dictionary<string, bool>
                {
                    { OneSignal.kOSSettingsAutoPrompt, false }
                })
               .EndInit();
            //OneSignal.GetTags(OnTagReceived);
            isInitialized = true;
#endif
        }

        private static void HandleNotificationOpened(OSNotificationOpenedResult result)
        {
            OSNotificationPayload payload = result.notification.payload;
            Dictionary<string, object> additionalData = payload.additionalData;
            string message = payload.body;
            //string actionID = result.action.actionID;
            bool isAppInFocus = result.notification.isAppInFocus;

            onNotificationOpened(message, additionalData, isAppInFocus);
        }

        private static void HandleNotificationReceived(OSNotification notification)
        {
            OSNotificationPayload payload = notification.payload;
            Dictionary<string, object> additionalData = payload.additionalData;
            string message = payload.body;

            onNotificationReceived(message, additionalData);
        }

        private void Start()
        {
            CloudServiceManager.onLoginGameSparkAsGuest += OnLoginGameSparkAsGuest;
            CloudServiceManager.onLoginGameSpark += OnLoginGamespark;
            CloudServiceManager.onConfigLoaded += OnConfigLoaded;
            SocialManager.onFbLogout += OnFbLogout;
            Notifications.RemoteNotificationOpened += OnNotificationOpen;
            SceneManager.sceneLoaded += OnSceneLoaded;
            GameManager.GameStateChanged += OnGameStateChanged;

#if EM_ONESIGNAL
            Init();
#endif
        }

        private void OnGameStateChanged(GameState arg1, GameState arg2)
        {
            if(arg1 == GameState.Prepare)
            {
                if (StoryPuzzlesSaver.Instance != null && StoryPuzzlesSaver.Instance.MaxNode >= 0)
                    StartCoroutine(SetNotificationPermisionDelayCR());
            }
        }

        private void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            StartCoroutine(CrOnSceneLoaded(s, m));
            if (StoryPuzzlesSaver.Instance!= null && StoryPuzzlesSaver.Instance.MaxNode >= 0)
                StartCoroutine(SetNotificationPermisionDelayCR());
        }

        private IEnumerator SetNotificationPermisionDelayCR()
        {
            yield return new WaitForSeconds(1);
            SetNotificationPermision();
        }

        private IEnumerator CrOnSceneLoaded(Scene s, LoadSceneMode m)
        {
            //We have to find the dialog since it does not remain across scenes
            yield return null;
            confirmDialog = FindObjectOfType<ConfirmationDialog>();
        }

        private void OnDestroy()
        {
            CloudServiceManager.onLoginGameSparkAsGuest -= OnLoginGameSparkAsGuest;
            CloudServiceManager.onLoginGameSpark -= OnLoginGamespark;
            CloudServiceManager.onConfigLoaded -= OnConfigLoaded;
            SocialManager.onFbLogout -= OnFbLogout;
            Notifications.RemoteNotificationOpened -= OnNotificationOpen;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        private void OnConfigLoaded(GSData config)
        {
            float? remotePromptIntervalDay = config.GetFloat("notificationPromptIntervalDay");
            if (remotePromptIntervalDay.HasValue)
            {
                promptIntervalDay = remotePromptIntervalDay.Value;
            }

            int? remotePromptCountToDisallowAsking = config.GetInt("notificationPromptCountToDisallowAsking");
            if (remotePromptCountToDisallowAsking.HasValue)
            {
                promptCountToDisallowAsking = remotePromptCountToDisallowAsking.Value;
            }
        }

        private void OnNotificationOpen(EasyMobile.RemoteNotification remoteNotification)
        {
            OnNotificationOpen(remoteNotification.content.body, remoteNotification.actionId, remoteNotification.content.userInfo, remoteNotification.isAppInForeground);
        }

        private void OnNotificationOpen(string message, string buttonActionId, Dictionary<string, object> data, bool isAppFocus)
        {
            string actionId;
            //if (data.TryGetValue("actionId", out actionId))
            //{
            //}
        }

        public void SetPlayerName(string name)
        {
            if (isInitialized)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    OneSignal.SendTag(TAG_PLAYER_NAME, name);
                }
                else
                {
                    OneSignal.DeleteTag(TAG_PLAYER_NAME);
                }
            }
        }

        private void OnLoginGameSparkAsGuest(AuthenticationResponse response)
        {
            if (response.HasErrors)
                return;

            GSData accountDetail = response.ScriptData.GetGSData("accountDetail");
            GSData location = accountDetail.GetGSData("location");
            if (location != null)
            {
                float? longditute = location.GetFloat("longditute");
                if (longditute.HasValue)
                {
                    SetPlayerRegionBasedOnLongditute(longditute.Value);
                }
            }
        }

        private void OnLoginGamespark(AuthenticationResponse response)
        {
            if (response.HasErrors)
                return;

            GSData accountDetail = response.ScriptData.GetGSData("accountDetail");
            GSData location = accountDetail.GetGSData("location");
            if (location != null)
            {
                float? longditute = location.GetFloat("longditute");
                if (longditute.HasValue)
                {
                    SetPlayerRegionBasedOnLongditute(longditute.Value);
                }
            }

            string playerName = response.DisplayName;
            if (!string.IsNullOrEmpty(playerName))
                SetPlayerName(playerName);

            if (isInitialized)
            {
                TryRePrompt();
            }
        }

        /// <summary>
        /// Reprompt user after login if they switch to another device
        /// </summary>
        private void TryRePrompt()
        {
            StartCoroutine(CrTryRePrompt());
        }

        private IEnumerator CrTryRePrompt()
        {
            //Syncing is delay for 1 frame after OnLoginGamespark callback is fired, 
            //we wait for another frame to ensure this would run after the syncing process is started
            yield return null;
            yield return null;
            yield return new WaitUntil(() => { return CloudServiceManager.isSyncing == false; });
            yield return new WaitForSeconds(0.5f);
            SetNotificationPermision();
        }

        private void OnFbLogout()
        {
            SetPlayerName(string.Empty);
        }

        private void SetPlayerRegionBasedOnLongditute(float longditute)
        {
            if (longditute < -20)
            {
                OneSignal.SendTag(TAG_REGION, REGION_AMERICA);
            }
            else if (longditute >= -20 && longditute <= 130)
            {
                OneSignal.SendTag(TAG_REGION, REGION_AFRICA_EUROPE_ASIA);
            }
            else if (longditute > 130)
            {
                OneSignal.SendTag(TAG_REGION, REGION_EAST_ASIA);
            }
        }

        private void SetNotificationPermision()
        {
            try
            {
                OSPermissionSubscriptionState state = OneSignal.GetPermissionSubscriptionState();
                if (state != null)
                {
                    CloudServiceManager.Instance.SubmitOneSignalId(state.subscriptionStatus.userId);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.ToString());
            }

#if UNITY_IOS && !UNITY_EDITOR
            IosAcceptNotificationCallback();
#endif
            EasyMobile.Notifications.Init();
        }

        private void IosAcceptNotificationCallback()
        {
            OneSignal.PromptForPushNotificationsWithUserResponse((isAccepted) =>
            {
                //Prompt to ios user via native popup
            });
        }
    }
}