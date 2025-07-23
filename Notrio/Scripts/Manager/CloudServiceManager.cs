using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameSparks;
using GameSparks.Core;
using GameSparks.Api;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.Api.Messages;
using Takuzu.Generator;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if EM_ONESIGNAL
using OneSignalPush;
#endif

namespace Takuzu
{
    public class CloudServiceManager : MonoBehaviour
    {
        public static CloudServiceManager Instance { get; private set; }
        /// <summary>
        /// Is user logged in GS with their FB account?
        /// </summary>
        public static bool IsLoggedInToGameSpark { get; private set; }

        /// <summary>
        /// Event fired after logged in GS with FB account.
        /// </summary>
        public static Action<AuthenticationResponse> onLoginGameSpark = delegate { };

        public static Action onInvitationVerifiedSuccessfully = delegate {};

        /// <summary>
        /// Event fired after logged in GS with device id (annonymous).
        /// </summary>
        public static Action<AuthenticationResponse> onLoginGameSparkAsGuest = delegate { };

        /// <summary>
        /// Event fired after logged in GS, using FB or device id.
        /// </summary>
        public static Action onGamesparkAuthenticated = delegate { };

        internal void RequestLeaderboardData(string shortCode, object resultCallBack)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Event fired after receive challenge puzzle from server.
        /// </summary>
        public static Action<List<Puzzle>> onChallengeDataReceived = delegate { };

        public static Action<LeaderboardDataResponse> onLeaderBoardDataReceived = delegate { };
        /// <summary>
        /// Event fired after a LB submission being rejected by the server.
        /// </summary>
        public static Action<float> onSubmitSolvingTimeCancelled = delegate { };

        /// <summary>
        /// Event fired after data syncing successful.
        /// </summary>
        public static Action onPlayerDbSyncSucceed = delegate { };

        /// <summary>
        /// Event fired after data syncing failed, with the error string.
        /// </summary>
        public static Action<string> onPlayerDbSyncFailed = delegate { };

        /// <summary>
        /// Event fired when data syncing started.
        /// </summary>
        public static Action onPlayerDbSyncBegin = delegate { };

        /// <summary>
        /// Event fired when data syncing ended (not sure if it succeed or not!).
        /// </summary>
        public static Action onPlayerDbSyncEnd = delegate { };

        /// <summary>
        /// Event fired when app config is loaded from local storaged or pulled from server.
        /// </summary>
        public static Action<GSData> onConfigLoaded = delegate { };

        public static string gsAuthToken;
        public static string playerId;
        public static string playerName;
        public static string countryId = null;
        public static string countryName = null;
        public static bool isGuest = true;

        public const string GET_CHALLENGE_KEY = "GET_CHALLENGE";
        public const string PLAYERDB_DATA_KEY = "PLAYERDB_DATA";
        public const string SYNC_PLAYERDB_REMOTE_KEY = "SYNC_PLAYERDB_REMOTE";

        public const string SYNC_PLAYERDB_KEY = "SYNC_PLAYERDB";
        public const string UPLOAD_PLAYERDB_KEY = "UPLOAD_PLAYERDB";
        public const string LAST_MODIFY_ATTRIBUTE = "LAST_MODIFY";
        public const string SAVE_PLAYERDB_INFO_KEY = "SAVE_PLAYERDB_INFO";
        public const string UPLOAD_ID_ATTRIBUTE = "UPLOAD_ID";
        public const string GET_COUNTRY_TEAM_KEY = "GET_COUNTRY_TEAM";
        public const string SUBMIT_DAILY_SOLVING_TIME_KEY = "SUBMIT_DAILY_SOLVING_TIME_PREPROCESS";
        public const string SUBMIT_WEEKLY_SOLVING_TIME_KEY = "SUBMIT_WEEKLY_SOLVING_TIME_PREPROCESS";
        public const string SOLVING_TIME_ATTRIBUTE = "SOLVING_TIME";
        public const string CHALLENGE_ID_ATTRIBUTE = "CHALLENGE_ID";
        public const string CHALLENGE_TYPE_ATTRIBUTE = "CHALLENGE_TYPE";
        public const string SUBMIT_COUNTRY_EXP_KEY = "SUBMIT_COUNTRY_EXP";
        public const string SUBMIT_COUNTRY_DAILY_SOLVING_TIME_KEY = "SUBMIT_COUNTRY_DAILY_SOLVING_TIME";
        public const string SUBMIT_COUNTRY_WEEKLY_SOLVING_TIME_KEY = "SUBMIT_COUNTRY_WEEKLY_SOLVING_TIME";
        public const string EXP_ATTRIBUTE = "EXP";
        public const string LEVEL_ATTRIBUTE = "LEVEL";
        public const string SIZE_ATTRIBUTE = "SIZE";
        public const string GET_AD_FREQUENCY_CAP_KEY = "GET_AD_FREQUENCY_CAP";
        public const string SUBMIT_ONESIGNAL_ID_KEY = "SUBMIT_ONESIGNAL_ID";
        public const string ONESIGNAL_ID_ATTRIBUTE = "ONESIGNAL_ID";
        public const string GET_REWARD_KEY = "GET_REWARD";
        public const string CLAIM_REWARD_KEY = "CLAIM_REWARD";
        public const string REWARD_ID_ATTRIBUTE = "REWARD_ID";
        public const string GET_APP_CONFIG_KEY = "GET_APP_CONFIG";
        public const string SUBMIT_DAILY_WEEKLY_ENTRY_KEY = "SUBMIT_DAILY_WEEKLY_CHALLENGE_ENTRY";
        public const string GET_CHALLENGE_ENTRIES_COUNT_KEY = "GET_CHALLENGE_ENTRIES_COUNT";
        public const string GET_ALL_LB_COUNT_KEY = "GET_ALL_LB_COUNT";
        public const string GET_ALL_CURRENT_RANK_KEY = "GET_ALL_CURRENT_RANK";
        public const string PLAYER_MAX_LEVEL_ATTRIBUTE = "PLAYER_MAX_LEVEL";
        public const string FIRST_INSTALLATION = "FIRST_INSTALLATION";
        public const string DELETE_PLAYER_KEY = "DELETE_PLAYER";
        public const string DELETE_PLAYER_LB_ENTRY_KEY = "DELETE_PLAYER_LB_ENTRY_KEY";
        public const string DELETE_PLAYER_LB_ENTRY_ID_KEY = "DELETE_PLAYER_LB_ENTRY_ID_KEY";
        public const string LOGOUT_GAMESPARK_KEY = "LOGOUT_GAMESPARK";
        public const string GET_INVITATION_CODE_EVENT_KEY = "GET_INVITATION_CODE";
        public const string VERIFY_INVITATION_CODE_EVENT_KEY = "VERIFY_INVITATION_CODE";
        public const string UPDATE_INVITATION_CODE_REF_EVENT_KEY = "UPDATE_INVITATION_CODE_OWNER";
        public const string INVITATION_CODE_ATTRIBUTE_KEY = "INVITATION_CODE";
        public const string MANUALLY_SET_REWARD = "MANUALLY_SET_REWARD";
        private const string INVITATION_CODE_PLAYERPREF_SAVE_KEY = "UNLOCK_INVITATION_CODE_PLAYERPREF_SAVE_KEY";
        private const string INVITATION_DYNAMICLINK_PLAYERPREF_SAVE_KEY = "UNLOCK_INVITATION_DYNAMICLINK_PLAYERPREF_SAVE_KEY";
        private const string GET_YD_TOP_ENTRIES_EVENT_KEY = "GET_YD_TOP_LEADERBOARD";
        private const string LB_SHORT_CODE_ATTRIBUTE_KEY = "LB_SHORT_CODE";
        private const string TOP_ENTRIES_DATA_LIST_NAME = "top_entries";

        public const string AGE_ATTRIBUTE = "AGE";
        public float syncIntervalMinutes;
        public ConfirmationDialog dialog;

        //private string currentSyncingPhase = "";

        private float lastSyncTime = 0;
        public static bool isSyncing = false;
        public static bool isLoggingIn = false;
        public GSData appConfig;
        private bool sendingDeleteRequest = false;
        private string oldId;
        //Texture2D t;
        //GUIStyle style;
        //private void OnGUI()
        //{
        //    Color c =
        //        GS.Authenticated ? Color.blue :
        //        GS.Available ? Color.cyan :
        //        Color.red;
        //    Rect rt = new Rect(0, 0, Screen.height * 0.025f, Screen.height * 0.025f);

        //    if (t == null)
        //        t = new Texture2D(1, 1);
        //    t.SetPixels(new Color[] { c });
        //    t.Apply();
        //    GUI.DrawTexture(rt, t);

        //    if (style == null)
        //        style = new GUIStyle();
        //    style.normal.textColor = Color.white;
        //    style.fontSize = (int)(Screen.height * 0.03f);
        //    style.alignment = TextAnchor.LowerRight;
        //    Rect r = new Rect(0, Screen.height - 100, Screen.width, 100);
        //    GUI.Label(r, currentSyncingPhase, style);

        //    if (GS.Authenticated && isGuest)
        //    {
        //        GUI.Label(rt, "G", style);
        //    }
        //}

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
                isGuest = true;
#if UNITY_EDITOR
                //FB SDK does not auto logging in at startup like one mobile
                //So we consider the data on the machine at that time as 'orphaned'
                PlayerDb.DeleteKey(PlayerDb.PLAYER_ID_KEY);
#endif

                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            StartCoroutine(CrOnSceneLoaded(s, m));
        }

        /// <summary>
        /// Confirmation dialog does not remain across scenes
        /// We need to do this in case of displaying "can't connect" message
        /// </summary>
        private IEnumerator CrOnSceneLoaded(Scene _, LoadSceneMode __)
        {
            yield return null;
            dialog = FindObjectOfType<ConfirmationDialog>();
        }

        private void Start()
        {
            LoadLocalAppConfig();
            TryLogin();
            StartCoroutine(CrSyncScheduled());
        }

        private void OnEnable()
        {
            CloudServiceManager.onConfigLoaded += OnConfigLoaded;
            CloudServiceManager.onLoginGameSparkAsGuest += this.OnLoginToGameSparkAsGuest;
            CloudServiceManager.onPlayerDbSyncBegin += OnSyncBegin;
            CloudServiceManager.onPlayerDbSyncEnd += OnSyncEnd;
            GS.GameSparksAvailable += OnGsAvailable;
            GS.GameSparksAuthenticated += OnGsAuthenticated;
            SocialManager.onFbLogin += OnFbLogin;
            SocialManager.onFbLogout += OnFbLogout;
            Judger.onExpGained += OnExpGained;
            PlayerInfoManager.onInfoUpdated += OnPlayerInfoUpdated;
            GameManager.GameStateChanged += OnGameStateChanged;
            PlayerInfoManager.onInfoLoaded += OnInfoLoaded;
        }

        private void OnDisable()
        {
            CloudServiceManager.onConfigLoaded -= OnConfigLoaded;
            CloudServiceManager.onLoginGameSparkAsGuest -= this.OnLoginToGameSparkAsGuest;
            CloudServiceManager.onPlayerDbSyncBegin -= OnSyncBegin;
            CloudServiceManager.onPlayerDbSyncEnd -= OnSyncEnd;
            GS.GameSparksAvailable -= OnGsAvailable;
            GS.GameSparksAuthenticated -= OnGsAuthenticated;
            SocialManager.onFbLogin -= OnFbLogin;
            SocialManager.onFbLogout -= OnFbLogout;
            Judger.onExpGained -= OnExpGained;
            PlayerInfoManager.onInfoUpdated -= OnPlayerInfoUpdated;
            GameManager.GameStateChanged -= OnGameStateChanged;
            PlayerInfoManager.onInfoLoaded -= OnInfoLoaded;
        }

        private void OnSyncBegin()
        {
            isSyncing = true;
        }

        private void OnSyncEnd()
        {
            isSyncing = false;
        }

        private void OnConfigLoaded(GSData config)
        {
            Debug.Log("Apply config: " + config.JSON);
            float? interval = config.GetFloat("syncInterval");
            if (interval.HasValue)
                syncIntervalMinutes = interval.Value;
        }

        private IEnumerator DelayCheckingVersionCR()
        {
            yield return null;
            yield return null;
#if UNITY_IOS
            if (string.IsNullOrEmpty(appConfig.GetString("minimumIOSAcceptedVersion")) == false)
            {
                Debug.Log("Checking for minimumIOSAcceptedVersion " + appConfig.GetString("minimumIOSAcceptedVersion"));
                if (CheckingForMinAcceptedVersion(appConfig.GetString("minimumIOSAcceptedVersion")) == false)
                {
                    versionWarningStickyPopupEnabled = true;
                    ShowVersionWarningPopUp();
                    yield break;
                }
            }
#else
            if (string.IsNullOrEmpty(appConfig.GetString("minimumAndroidAcceptedVersion")) == false)
            {
                Debug.Log("Checking for minimumAndroidAcceptedVersion " + appConfig.GetString("minimumAndroidAcceptedVersion"));
                if (CheckingForMinAcceptedVersion(appConfig.GetString("minimumAndroidAcceptedVersion")) == false)
                {
                    versionWarningStickyPopupEnabled = true;
                    ShowVersionWarningPopUp();
                    yield break;
                }
            }
#endif

            if (VersionWarningCover == null)
                yield break;
            VersionWarningCover.gameObject.SetActive(false);
            AudioListener.pause = false;
        }

        private bool versionWarningStickyPopupEnabled = false;
        private void ShowVersionWarningPopUp()
        {
            if (versionWarningStickyPopupEnabled == false)
                return;
            VersionWarningCover.gameObject.SetActive(true);
            EasyMobile.NativeUI.AlertPopup popup = EasyMobile.NativeUI.Alert(I2.Loc.ScriptLocalization.REQUIRE_UPGRADE_VERSION_TITLE, I2.Loc.ScriptLocalization.REQUIRE_UPGRADE_VERSION_MSG, I2.Loc.ScriptLocalization.OK.ToUpper());
            if (popup != null)
            {
                popup.OnComplete += (value) => {
                    if(Application.platform == RuntimePlatform.IPhonePlayer)
                        Application.OpenURL(AppInfo.Instance.APPSTORE_LINK);
                    else
                        Application.OpenURL(AppInfo.Instance.PLAYSTORE_LINK);
                };
            }
            Debug.LogError("Please Update To Newer version of this game!!!");
            AudioListener.pause = true;
        }

        private void OnApplicationFocus(bool focus)
        {
            if(focus)
            {
                ShowVersionWarningPopUp();
            }
        }

        public GameObject VersionWarningCover;

        private bool CheckingForMinAcceptedVersion(string acceptedMinVersion)
        {
            string[] acceptedMinVersionNumbers = acceptedMinVersion.Split('.');
            string[] currentVersionNumbers = Application.version.Split('.');

            int versionNumberLength = Mathf.Min(acceptedMinVersionNumbers.Length, currentVersionNumbers.Length);

            for(int i = 0; i < versionNumberLength; i++){
                int checkCurrent = 0;
                int checkAccept = 0;
                Int32.TryParse(currentVersionNumbers[i], out checkCurrent);
                Int32.TryParse(acceptedMinVersionNumbers[i], out checkAccept);
                Debug.LogFormat("Compare {0} - {1}", checkCurrent, checkAccept);
                if(checkCurrent < checkAccept)
                    return false;
                if(checkCurrent > checkAccept)
                    return true;
            }
            if(acceptedMinVersionNumbers.Length > currentVersionNumbers.Length)
                return false;

            return true;
        }

        /// <summary>
        /// Load the app config from local storage, typically, this is the config we pulled from server in last session
        /// This config will override the default config packed with the app
        /// </summary>
        private void LoadLocalAppConfig()
        {
            try
            {
                string json = PlayerPrefs.GetString(GET_APP_CONFIG_KEY, "");
                Dictionary<string, object> d;
                if (!string.IsNullOrEmpty(json))
                    d = GSJson.From(json) as Dictionary<string, object>;
                else
                    d = new Dictionary<string, object>();
                GSData config = new GSData(d);
                appConfig = config;
                onConfigLoaded(config);
                StartCoroutine(DelayCheckingVersionCR());
            }
            catch (Exception e)
            {
                Debug.LogError("(HARMLESS) Failed to load local app config. " + e.ToString());
                //Something goes wrong with the config json, we need to delete it to avoid exception in later session
                PlayerPrefs.DeleteKey(GET_APP_CONFIG_KEY);
            }
        }

        /// <summary>
        /// Save app config to local storage.
        /// </summary>
        private void SaveLocalAppConfig(GSData config)
        {
            try
            {
                string json = GSJson.To(config);
                PlayerPrefs.SetString(GET_APP_CONFIG_KEY, json);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save local app config: " + e.ToString());
            }
        }

        /// <summary>
        /// Request app config from server.
        /// No need to call this function, since the config is contained in the authentication response.
        /// </summary>
        private void RequestAppConfig()
        {
            new LogEventRequest()
                .SetEventKey(GET_APP_CONFIG_KEY)
                .Send((r) =>
                {
                    if (r.HasErrors)
                    {
                        Debug.LogError(r.Errors.JSON);
                        return;
                    }
                    GSData config = r.ScriptData.GetGSData("config");
                    if (config != null)
                    {
                        appConfig = config;
                        onConfigLoaded(config);
                        SaveLocalAppConfig(config);
                    }
                });
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Prepare && (oldState == GameState.Paused || oldState == GameState.GameOver))
            {
                // Wait for the next frame then try syncing (wait for player progress data to be saved to avoid data lost)
                CoroutineHelper.Instance.DoActionDelay(
                    () =>
                    {
                        TrySync();
                    }, 0);
            }
            if (newState == GameState.Playing)
            {
                TryLogin();
            }
        }

        /// <summary>
        /// Sync data if the interval is elapsed
        /// </summary>
        private void TrySync()
        {
            if (isGuest)
                return;

            float timeMinutes = (Time.time - lastSyncTime) / 60;
            if (timeMinutes > syncIntervalMinutes)
            {
                lastSyncTime = Time.time;
                Debug.Log("Sync scheduled");
                SyncPlayerDb();
            }
        }

        private IEnumerator CrSyncScheduled()
        {
            while (true)
            {
                yield return new WaitForSeconds(syncIntervalMinutes);
                if (GameManager.Instance.GameState == GameState.Prepare)
                    TrySync();
            }
        }

        private void TryLogin()
        {
            CoroutineHelper.Instance.PostponeActionUntil(
                    () =>
                    {
                        CoroutineHelper.Instance.DoActionDelay(
                            () =>
                            {
                                if (SocialManager.Instance.IsLoggedInFb && isGuest)
                                    LoginToGameSpark();
                                else if (String.IsNullOrEmpty(playerId))
                                    LoginToGameSparkAsGuest();

                            }, 0.5f);
                    },
                    () =>
                    {
                        return GS.Available;
                    });
        }

        private void OnGsAvailable(bool available)
        {
            if (GS.Authenticated && IsLoggedInToGameSpark && !isGuest)
            {
                Debug.Log("Sync on GS available");
                SyncPlayerDb();
            }
            if (!GS.Authenticated && !SocialManager.Instance.IsLoggedInFb)
            {
                LoginToGameSparkAsGuest();
            }
        }
        private bool checkedVersiononAuthenticatedRequest = false;
        private void OnGsAuthenticated(string s)
        {
            if (GS.Authenticated)
            {
                TournamentDataRequest.ClearCatchedData();
                TournamentDataRequest.UpdateData();
                onGamesparkAuthenticated();
                if(checkedVersiononAuthenticatedRequest == false)
                    StartCoroutine(DelayCheckingVersionCR());
                checkedVersiononAuthenticatedRequest = true;
            }
        }

        private void OnFbLogin(bool success)
        {
            if (!success)
            {
                return;
            }
            else
            {
                if (GS.Available && isGuest)
                {
                    LoginToGameSpark();
                    oldId = playerId;
                }
            }

        }

        private void OnFbLogout()
        {
            PlayerPrefs.DeleteKey(INVITATION_CODE_PLAYERPREF_SAVE_KEY);
            LogoutGameSpark();
        }

        public void LogoutGameSpark()
        {
            IsLoggedInToGameSpark = false;
            UpLoadLocalDb();
            new LogEventRequest()
                .SetEventKey(LOGOUT_GAMESPARK_KEY)
                .Send((response) =>
                {
                    if (response.HasErrors)
                        Debug.LogError(response.Errors.JSON);
                    GS.Reset();
                    gsAuthToken = string.Empty;
                    playerId = string.Empty;
                    playerName = string.Empty;
                    countryId = string.Empty;
                    countryName = string.Empty;
                    isGuest = true;
                    LoginToGameSparkAsGuest();
                });
        }

        /// <summary>
        /// Login to server using FB account.
        /// </summary>
        public void LoginToGameSpark(Action<AuthenticationResponse> callback = null)
        {
            isLoggingIn = true;
        }

        private void OnLoginGameSpark(AuthenticationResponse response)
        {
            if (response.HasErrors)
            {
                Debug.LogError(response.Errors.JSON);
                IsLoggedInToGameSpark = false;
                //We wait for some seconds, if still not logged in, we display the message
                CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    if (dialog != null && !GS.Authenticated)
                    {
                        dialog.Show(
                            I2.Loc.ScriptLocalization.Error,
                            I2.Loc.ScriptLocalization.CAN_NOT_CONNECT_TO_SERVER,
                            I2.Loc.ScriptLocalization.TRY_AGAIN, () => { LoginToGameSpark(); },
                            string.Empty, null,
                            null);
                    }
                    else
                    {
                        Debug.LogError("Can't connect to GS");
                    }
                }, 3);

            }
            else
            {
                Debug.Log("LOGGED IN GAMESPARK");
                IsLoggedInToGameSpark = true;
                gsAuthToken = response.AuthToken;
                playerId = response.UserId;
                playerName = response.DisplayName;
                isGuest = false;
                countryId = response.ScriptData.GetString("countryId");
                countryName = response.ScriptData.GetString("countryName");
                SubmitFbUserId();

                //if the user is new to Gamespark, they will claim the local data and upload it to server
                //otherwise, we erase the local data and pull data from server
                if (response.NewPlayer != true)
                {
                    string localPlayerId = PlayerDb.GetString(PlayerDb.PLAYER_ID_KEY, string.Empty);
                    if (!response.UserId.Equals(localPlayerId))
                    {
                        PlayerDb.Reset();
                        Debug.Log("Reset PlayerDb");
                    }
                }

                GSData config = response.ScriptData.GetGSData("config");
                if (config != null)
                {
                    appConfig = config;
                    onConfigLoaded(config);
                    SaveLocalAppConfig(config);
                }

                //Postpone syncing until the next frame to wait for other script to finish with their data
                CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    Debug.Log("Sync on login");
                    PlayerDb.SetString(PlayerDb.PLAYER_ID_KEY, response.UserId);
                    SyncPlayerDb();
                }, 0);
            }
            UpdateInvitationcodeRef();
            onLoginGameSpark(response);
        }

        private bool tryingToLoginAsGuest = false;

        /// <summary>
        /// Login to GS server using device id
        /// </summary>
        /// <param name="callback"></param>
        public void LoginToGameSparkAsGuest(Action<AuthenticationResponse> callback = null)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable || tryingToLoginAsGuest == true)
                return;
            IsLoggedInToGameSpark = false;
            new DeviceAuthenticationRequest()
                .Send((AuthenticationResponse response) =>
                {
                    tryingToLoginAsGuest = false;
                    if (response.HasErrors)
                    {
                        Debug.Log(response.Errors.JSON);
                        return;
                    }
                    GSData config = response.ScriptData.GetGSData("config");
                    if (config != null)
                    {
                        appConfig = config;
                        onConfigLoaded(config);
                        SaveLocalAppConfig(config);
                    }
                    onLoginGameSparkAsGuest(response);
                    if (callback != null)
                    {
                        callback(response);
                    }
                });
            tryingToLoginAsGuest = true;

        }

        private void OnLoginToGameSparkAsGuest(AuthenticationResponse response)
        {
            if (response.HasErrors)
            {
                Debug.Log(response.Errors.JSON);
            }
            else
            {
               
                isGuest = true;
                Debug.Log("LOGGED IN GAMESPARK AS GUEST" + response.UserId);
                GSData config = response.ScriptData.GetGSData("config");
                playerId = response.UserId;
                countryId = response.ScriptData.GetString("countryId");
                countryName = response.ScriptData.GetString("countryName");
                gsAuthToken = response.AuthToken;
                if (!PlayerPrefs.HasKey(FIRST_INSTALLATION) && !(response.NewPlayer ?? false))
                {
                    //Debug.Log("User id found - Delete this id");
                    //DeletePlayerIfExistThenRelogin();
                    DeleteLBEntry();
                }
                if (config != null)
                {
                    appConfig = config;
                    onConfigLoaded(config);
                    SaveLocalAppConfig(config);
                }
            }
        }

        private void DeleteLBEntry(string id = "")
        {
            if (string.IsNullOrEmpty(id))
                return;
            new LogEventRequest()
                .SetEventKey(DELETE_PLAYER_LB_ENTRY_KEY)
                .SetEventAttribute(DELETE_PLAYER_LB_ENTRY_ID_KEY, String.IsNullOrEmpty(id)?playerId:id)
                .SetDurable(true)
                .Send((response) =>
                {
                    if (response.HasErrors)
                    {
                        Debug.LogError(response.Errors.JSON);
                    }
                    else
                    {
                        PlayerPrefs.SetInt(FIRST_INSTALLATION, 0);
                        if (String.IsNullOrEmpty(id))
                        {
                            GS.Reset();
                            gsAuthToken = string.Empty;
                            playerId = string.Empty;
                            playerName = string.Empty;
                            countryId = string.Empty;
                            countryName = string.Empty;
                            isGuest = true;
                            TryLogin();
                        }
                    }
                });
        }

        private void DeletePlayerIfExistThenRelogin()
        {
            if (!sendingDeleteRequest)
            {
                DeleteCurrentPlayer((response) => { LoginToGameSparkAsGuest(); });
            }
        }

        private void DeleteCurrentPlayer(Action<LogEventResponse> callback = null)
        {
            Debug.Log("Delete");
            sendingDeleteRequest = true;
            new LogEventRequest()
                .SetEventKey(DELETE_PLAYER_KEY)
                .SetDurable(true)
                .Send((response) =>
                {
                    Debug.Log("Delete " + response.BaseData.JSON);
                    sendingDeleteRequest = false;
                    if (response.HasErrors)
                    {
                        Debug.LogError(response.Errors.JSON);
                    }
                    else
                    {
                        PlayerPrefs.SetInt(FIRST_INSTALLATION, 0);
                        if (callback != null)
                            callback(response);
                    }
                });
        }

        /// <summary>
        /// Get country id
        /// No need to call this because it is packed with the authentication response
        /// </summary>
        private void GetCountryTeam()
        {
            if (isGuest)
                return;
            new LogEventRequest()
                .SetEventKey(GET_COUNTRY_TEAM_KEY)
                .Send((response) =>
                {
                    if (response.HasErrors)
                    {
                        Debug.LogError(response.Errors.JSON);
                    }
                    else
                    {
                        countryId = response.ScriptData.GetString("countryId") ?? "";
                        countryName = response.ScriptData.GetString("countryName") ?? "";
                    }
                });
        }

        /// <summary>
        /// Submit FB user id, the id will be used to download their avatar image on leaderboard
        /// </summary>
        private void SubmitFbUserId()
        {
            if (isGuest)
                return;
        }

        private void SubmitFbUserIdCallback(LogEventResponse response)
        {
            if (response.HasErrors)
            {
                Debug.LogWarning(response.Errors);
            }
        }

        /// <summary>
        /// Get challenge puzzles from server
        /// </summary>
        /// <param name="callback"></param>
        public void RequestChallenge(Action<LogEventResponse> callback = null)
        {
            new LogEventRequest()
                .SetEventKey(GET_CHALLENGE_KEY)
                .SetMaxResponseTimeInMillis(15000)
                .Send((LogEventResponse response) =>
                {
                    RequestChallengeCallback(response);
                    if (callback != null)
                        callback(response);
                });
        }

        private void RequestChallengeCallback(LogEventResponse response)
        {
            if (response.HasErrors)
            {
                Debug.LogWarning("Request challenge failed");
                return;
            }
        }

        private void OnExpGained(int exp)
        {
            
        }

        private void OnPlayerInfoUpdated(PlayerInfo newInfo, PlayerInfo oldInfo)
        {
            SubmitExp(newInfo);
            SubmitCountryExp(ExpProfile.active.ToTotalExp(newInfo));
        }

        private void OnInfoLoaded(PlayerInfo Info, bool start)
        {
            if (start)
                return;
            SubmitExp(Info);
            SubmitCountryExp(ExpProfile.active.ToTotalExp(Info));
        }

        /// <summary>
        /// Submit exp to leaderboard
        /// </summary>
        /// <param name="info"></param>
        private void SubmitExp(PlayerInfo info)
        {
            //if (isGuest)
            //    return;
            //Debug.Log("Submitting exp: " + ExpProfile.active.ToTotalExp(info));
            new LogEventRequest()
                .SetDurable(true)
                .SetEventKey("SUBMIT_EXP")
                .SetEventAttribute(EXP_ATTRIBUTE, ExpProfile.active.ToTotalExp(info))
                .SetEventAttribute(PLAYER_MAX_LEVEL_ATTRIBUTE, PuzzleManager.Instance.ageList[StoryPuzzlesSaver.Instance.MaxNode < 0 ? 0 : StoryPuzzlesSaver.Instance.MaxNode])
                .Send(SubmitExpCallback);
        }

        public void SubmitAge(string v)
        {
            //Debug.Log("Submitting age: " + v);
            new LogEventRequest()
                .SetDurable(true)
                .SetEventKey("SUBMIT_AGE")
                .SetEventAttribute(AGE_ATTRIBUTE, v)
                .Send((respond)=> {
                    if (respond.HasErrors)
                    {
                        Debug.LogWarning(respond.Errors.JSON);
                    }
                });
        }

        private void SubmitExpCallback(LogEventResponse respond)
        {
            if (respond.HasErrors)
            {
                Debug.LogWarning(respond.Errors.JSON);
            }
        }

        public void RequestLeaderboardData(string lbShortCode, bool social = false, int entryCount = 10, int entryOffset = 0, Action<LeaderboardDataResponse> callback = null)
        {
            if (social && isGuest)
            {
                GSData responseData = new GSData();
                LeaderboardDataResponse r = new LeaderboardDataResponse(responseData);
                callback(r);
                return;
            }
            GSRequestData d = new GSRequestData();
            LeaderboardDataRequest request = new LeaderboardDataRequest()
                .SetLeaderboardShortCode(lbShortCode)
                .SetEntryCount(entryCount)
                .SetOffset(entryOffset)
                .SetSocial(social)
                .SetDontErrorOnNotSocial(true)
                .SetScriptData(d);

            request.Send((LeaderboardDataResponse response) =>
            {

                if (callback != null)
                    callback(response);
            });
        }

        /// <summary>
        /// Request leaderboard entry for the logged in player
        /// </summary>
        /// <param name="lbShortCode"></param>
        /// <param name="social"></param>
        /// <param name="useCountryLb"></param>
        /// <param name="callback"></param>
        public void RequestCurrentPlayerRank(string lbShortCode, bool social, bool useCountryLb, Action<LeaderboardsEntriesResponse> callback = null)
        {
            if (isGuest)
                return;
            LeaderboardsEntriesRequest request =
                new LeaderboardsEntriesRequest()
                .SetLeaderboards(new List<string>() { lbShortCode });
            if (useCountryLb)
            {
                request.SetTeamTypes(new List<string>() { "COUNTRY" });
                request.SetPlayer(countryId);
            }
            else if (social)
            {
                request.SetSocial(true);
            }
            request.Send((response) =>
            {
                if (response.HasErrors)
                {
                    Debug.LogError(response.Errors.JSON);
                }
                else
                {
                    if (callback != null)
                        callback(response);
                }
            });

        }

        internal void GetLeaderboardEntriesRequest(Action<LogEventResponse> callback = null)
        {
            new LogEventRequest()
                .SetEventKey(GET_ALL_CURRENT_RANK_KEY)
                .Send(response =>
                {
                    if (response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                    }
                    else
                    {
                        if (callback != null)
                            callback(response);
                    }
                });
        }


        public void RequestAvatarForPlayer(string playerId, Action<LogEventResponse> callback = null)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                callback(null);
                return;
            }
            if(playerUrlRequests.ContainsKey(playerId))
            {
                playerUrlRequests[playerId].GetResponse(callback);
                return;
            }

            PlayerAvatarUrlRequest rq = new PlayerAvatarUrlRequest(playerId);
            rq.GetResponse(callback);
            playerUrlRequests.Add(playerId, rq);
        }

        private Dictionary<string, PlayerAvatarUrlRequest> playerUrlRequests = new Dictionary<string, PlayerAvatarUrlRequest>();

        class PlayerAvatarUrlRequest
        {
            string id = "";
            LogEventResponse response = null;
            bool isRequesting = false;

            List<Action<LogEventResponse>> listeners = new List<Action<LogEventResponse>>();

            public PlayerAvatarUrlRequest(string id)
            {
                this.id = id;
            }

            public void GetResponse(Action<LogEventResponse> cb)
            {
                if(cb != null)
                    listeners.Add(cb);

                if(response != null)
                {
                    TryCallbackToListeners();
                    return;
                }

                if (isRequesting)
                    return;

                StartRequest();
            }
            private void StartRequest()
            {
                isRequesting = true;
                new LogEventRequest()
                .SetEventKey("GET_FB_AVATAR")
                .SetEventAttribute("PLAYER_ID", id)
                .Send((LogEventResponse res) =>
                {
                    isRequesting = false;
                    this.response = res;
                    TryCallbackToListeners();
                });
            }
            private void TryCallbackToListeners()
            {
                foreach (var cb in listeners)
                {
                    try
                    {
                        cb(this.response);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Something wrong happen when try to call back to this listener + " + ex.Message);
                    }
                }
                listeners.Clear();
            }
        }

        public void DownloadLbAvatar(string url, LeaderboardEntryParsedData parsedData)
        {
            parsedData.isLoadingAvatar = true;
            TextureDownloaderCacheManager.GetTextureDownloader(url).Get(texture =>
            {
                parsedData.isLoadingAvatar = false;
                if(texture != null)
                    parsedData.avatar = texture;
            });
        }

        // private IEnumerator CrDownloadLbAvatar(string url, LeaderboardEntryParsedData parsedData)
        // {
        //     parsedData.isLoadingAvatar = true;
        //     WWW w = new WWW(url);
        //     yield return w;
        //     parsedData.isLoadingAvatar = false;
        //     if (string.IsNullOrEmpty(w.error))
        //     {
        //         parsedData.avatar = w.texture;
        //     }
        // }

        public void DownloadMultiplayerAvatar(string url, RawImage textureAvatar)
        {
            StartCoroutine(CrDownloadMultiplayerAvatar(url, textureAvatar));
        }

        private IEnumerator CrDownloadMultiplayerAvatar(string url, RawImage textureAvatar)
        {
            WWW w = new WWW(url);
            yield return w;
            if (textureAvatar != null && string.IsNullOrEmpty(w.error))
            {
                textureAvatar.texture = w.texture;
            }
        }

        /// <summary>
        /// Submit daily challenge solving time
        /// Submission will be rejected if due date passed, or they has submitted before
        /// </summary>
        /// <param name="solvingTime"></param>
        /// <param name="callback"></param>
        public void SubmitDailySolvingTime(long solvingTime, Action<LogEventResponse> callback = null)
        {
            int level = PuzzleManager.Instance.GetDailyChallengeIdLevel();
            int size = PuzzleManager.Instance.GetDailyChallengeIdSize();
            Debug.Log(string.Format(
                "Submit daily solving time - time: {0}, id: {1}, level: {2}", solvingTime, PuzzleManager.Instance.GetDailyChallengeIdString(), level));
            new LogEventRequest()
                .SetDurable(true)
                .SetEventKey(SUBMIT_DAILY_SOLVING_TIME_KEY)
                .SetEventAttribute(SOLVING_TIME_ATTRIBUTE, solvingTime)
                .SetEventAttribute(LEVEL_ATTRIBUTE, level)
                .SetEventAttribute(SIZE_ATTRIBUTE, size)
                .SetEventAttribute(PLAYER_MAX_LEVEL_ATTRIBUTE, PuzzleManager.Instance.ageList[StoryPuzzlesSaver.Instance.MaxNode < 0 ? 0 : StoryPuzzlesSaver.Instance.MaxNode])
                .SetEventAttribute(CHALLENGE_ID_ATTRIBUTE, PuzzleManager.Instance.GetDailyChallengeIdString())
                .Send((response) =>
                {
                    Debug.Log(response.JSONData);
                    if (response.HasErrors)
                    {
                        Debug.Log(response.Errors.JSON);
                    }
                    else
                    {
                        //if (response.ScriptData.GetBoolean("cancelled") == true)
                        //{
                        //    float remainingMinutes = (float)response.ScriptData.GetFloat("remainingMinutes");
                        //    onSubmitSolvingTimeCancelled(remainingMinutes);
                        //}
                        if (callback != null)
                            callback(response);
                    }
                    Debug.Log(response.JSONString);
                });
        }

        /// <summary>
        /// Submit weekly challenge solving time
        /// Submission will be rejected if due date passed, or they has submitted before
        /// </summary>
        /// <param name="solvingTime"></param>
        /// <param name="callback"></param>
        public void SubmitWeeklySolvingTime(long solvingTime, Action<LogEventResponse> callback = null)
        {
            int level = PuzzleManager.Instance.GetWeeklyChallengeIdLevel();
            Debug.Log(string.Format(
                "Submit weekly solving time - time: {0}, id: {1}, level: {2}", solvingTime, PuzzleManager.Instance.GetWeeklyChallengeIdString(), level));
            new LogEventRequest()
                .SetDurable(true)
                .SetEventKey(SUBMIT_WEEKLY_SOLVING_TIME_KEY)
                .SetEventAttribute(SOLVING_TIME_ATTRIBUTE, solvingTime)
                .SetEventAttribute(LEVEL_ATTRIBUTE, level)
                .SetEventAttribute(PLAYER_MAX_LEVEL_ATTRIBUTE, PuzzleManager.Instance.ageList[StoryPuzzlesSaver.Instance.MaxNode < 0 ? 0: StoryPuzzlesSaver.Instance.MaxNode])
                .SetEventAttribute(CHALLENGE_ID_ATTRIBUTE, PuzzleManager.Instance.GetWeeklyChallengeIdString())
                .Send((response) =>
                {
                    if (response.HasErrors)
                    {
                        Debug.Log(response.Errors.JSON);
                    }
                    else
                    {
                        //if (response.ScriptData.GetBoolean("cancelled") == true)
                        //{
                        //    float remainingMinutes = (float)response.ScriptData.GetFloat("remainingMinutes");
                        //    onSubmitSolvingTimeCancelled(remainingMinutes);
                        //}
                        if (callback != null)
                            callback(response);
                    }

                });
        }

        /// <summary>
        /// Submit exp to country exp lb
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="callback"></param>
        public void SubmitCountryExp(int exp, Action<LogEventResponse> callback = null)
        {
            //if (isGuest)
            //    return;
            new LogEventRequest()
                .SetDurable(true)
                .SetEventKey(SUBMIT_COUNTRY_EXP_KEY)
                .SetEventAttribute(EXP_ATTRIBUTE, exp)
                .Send((response) =>
                {
                    if (response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                    }
                    if (callback != null)
                        callback(response);
                });
        }

        /// <summary>
        /// Submit solving time to country daily lb
        /// Submission will be reject if due time passed or they had submitted before
        /// </summary>
        /// <param name="solvingTime"></param>
        /// <param name="callback"></param>
        public void SubmitCountryDailySolvingTime(long solvingTime , Action<LogEventResponse> callback = null)
        {
            Debug.Log(string.Format("Submit country daily solving time: time {0}, id: {1}, level: {2}", solvingTime, PuzzleManager.Instance.GetDailyChallengeIdString(), (int)PuzzleManager.currentLevel));
            new LogEventRequest()
                .SetDurable(true)
                .SetEventKey(SUBMIT_COUNTRY_DAILY_SOLVING_TIME_KEY)
                .SetEventAttribute(SOLVING_TIME_ATTRIBUTE, solvingTime)
                .SetEventAttribute(LEVEL_ATTRIBUTE,(int) PuzzleManager.currentLevel)
                .SetEventAttribute(SIZE_ATTRIBUTE, (int) PuzzleManager.currentSize)
                .SetEventAttribute(CHALLENGE_ID_ATTRIBUTE, PuzzleManager.Instance.GetDailyChallengeIdString())
                .Send((response) =>
                {
                    if (response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                    }
                    if (callback != null)
                    {
                        callback(response);
                    }
                });
        }

        /// <summary>
        /// Submit solving time to country weekly lb
        /// Submission will be reject if due time passed or they had submitted before
        /// </summary>
        /// <param name="solvingTime"></param>
        /// <param name="callback"></param>
        public void SubmitCountryWeeklySolvingTime(long solvingTime, Action<LogEventResponse> callback = null)
        {
            Debug.Log(string.Format("Submit country weekly solving time: time {0}, id: {1}, level: {2}", solvingTime, PuzzleManager.Instance.GetWeeklyChallengeIdString(), (int)PuzzleManager.currentLevel));
            new LogEventRequest()
                .SetDurable(true)
                .SetEventKey(SUBMIT_COUNTRY_WEEKLY_SOLVING_TIME_KEY)
                .SetEventAttribute(SOLVING_TIME_ATTRIBUTE, solvingTime)
                .SetEventAttribute(LEVEL_ATTRIBUTE, (int)PuzzleManager.currentLevel)
                .SetEventAttribute(CHALLENGE_ID_ATTRIBUTE, PuzzleManager.Instance.GetWeeklyChallengeIdString())
                .Send((response) =>
                {
                    if (response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                    }
                    if (callback != null)
                    {
                        callback(response);
                    }
                });
        }

        /// <summary>
        /// Request ads cap from server
        /// No need to call this because it is packed in the authentication response
        /// </summary>
        /// <param name="callback"></param>
        public void RequestAdFrequencyCap(Action<LogEventResponse> callback = null)
        {
            new LogEventRequest()
                .SetEventKey(GET_AD_FREQUENCY_CAP_KEY)
                .Send((response) =>
                {
                    if (response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                    }
                    if (callback != null)
                        callback(response);
                });
        }



        public void SubmitDailyChallengeEntry(string challengeId)
        {
            Debug.Log("Sumit entry daily");
            new LogEventRequest()
                .SetDurable(true)
                .SetEventKey(SUBMIT_DAILY_WEEKLY_ENTRY_KEY)
                .SetEventAttribute(CHALLENGE_ID_ATTRIBUTE, challengeId)
                .SetEventAttribute(CHALLENGE_TYPE_ATTRIBUTE, "DAILY")
                .Send((response)=>
                {
                    if(response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                    }
                });
        }

        public void SubmitWeeklyChallengeEntry(string challengeId)
        {
            Debug.Log("Sumit entry weekly");
            new LogEventRequest()
                .SetDurable(true)
                .SetEventKey(SUBMIT_DAILY_WEEKLY_ENTRY_KEY)
                .SetEventAttribute(CHALLENGE_ID_ATTRIBUTE, challengeId)
                .SetEventAttribute(CHALLENGE_TYPE_ATTRIBUTE, "WEEKLY")
                .Send((response) =>
                {
                    if (response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                    }
                });
        }

        public bool IsInvitationCodeGenerated(){
            return PlayerDb.HasKey(INVITATION_CODE_PLAYERPREF_SAVE_KEY) && PlayerDb.HasKey(INVITATION_DYNAMICLINK_PLAYERPREF_SAVE_KEY);
        }

        public class InvitationCodeRequestResponse
        {
            public string iCode = "";
            public string dynamicLink = "";
        }

        public void GetInvitationCode(Action<InvitationCodeRequestResponse> callback){
            if(IsInvitationCodeGenerated())
            {
                callback(new InvitationCodeRequestResponse()
                {
                    iCode = PlayerDb.GetString(INVITATION_CODE_PLAYERPREF_SAVE_KEY, ""),
                    dynamicLink = PlayerDb.GetString(INVITATION_DYNAMICLINK_PLAYERPREF_SAVE_KEY, "")
                });
            }
            new LogEventRequest()
                .SetEventKey(GET_INVITATION_CODE_EVENT_KEY)
                .Send(response =>{
                    if(response.HasErrors){
                        Debug.LogWarning(response.Errors.JSON);
                        callback(null);
                    }
                    else{
                        GSData responseData = response.ScriptData.GetGSData("response");
                        PlayerDb.SetString(INVITATION_CODE_PLAYERPREF_SAVE_KEY, responseData.GetString("invitationCode"));
                        PlayerDb.SetString(INVITATION_DYNAMICLINK_PLAYERPREF_SAVE_KEY, responseData.GetString("invitationDynamicLink"));
                        callback(new InvitationCodeRequestResponse()
                        {
                            iCode = PlayerDb.GetString(INVITATION_CODE_PLAYERPREF_SAVE_KEY, ""),
                            dynamicLink = PlayerDb.GetString(INVITATION_DYNAMICLINK_PLAYERPREF_SAVE_KEY, "")
                        });
                    }
                });
        }

        public void VerifyInvitationCode(string invitationCode,bool manuallyReward,Action<bool> callback){
            new LogEventRequest()
                .SetEventKey(VERIFY_INVITATION_CODE_EVENT_KEY)
                .SetEventAttribute(INVITATION_CODE_ATTRIBUTE_KEY, invitationCode)
                .SetEventAttribute(MANUALLY_SET_REWARD, manuallyReward?1:0)
                .Send(response =>{
                    if(response.HasErrors){
                        Debug.LogWarning(response.Errors.JSON);
                        callback(false);
                    }
                    else{
                        bool success = response.ScriptData.GetBoolean("success")??false;
                        if(success){
                            //onInvitationVerifiedSuccessfully();
                        }
                        callback(success);
                    }
                });
        }

        public void UpdateInvitationcodeRef(){
            Debug.Log("Update invitation code ref");
            new LogEventRequest()
                .SetEventKey(UPDATE_INVITATION_CODE_REF_EVENT_KEY)
                .SetEventAttribute(INVITATION_CODE_ATTRIBUTE_KEY, PlayerDb.GetString(INVITATION_CODE_PLAYERPREF_SAVE_KEY, ""))
                .Send(response =>{
                    if(response.HasErrors){
                        Debug.LogWarning(response.Errors.JSON);
                    }
                    else{
                        Debug.Log("Update invitation code successfully");
                    }
                });
        }
        public void GetPlayerAvatar(Action<Texture> callback)
        {
            if (isGuest == true)
                return;
            if(playerAvatarDownloadRequest != null)
            {
                playerAvatarDownloadRequest.GetTexture(this, callback);
            }
            RequestAvatarForPlayer(playerId, (response) =>
            {
                if (response == null)
                    return;
                if (response.HasErrors)
                    return;
                string url = response.ScriptData.GetString("FbAvatarUrl");
                if (string.IsNullOrEmpty(url))
                    return;
                if (playerAvatarDownloadRequest == null)
                {
                    playerAvatarDownloadRequest = new AvatarDownloadRequest(url);
                }
                playerAvatarDownloadRequest.GetTexture(this, callback);
            });

        }
        private AvatarDownloadRequest playerAvatarDownloadRequest;

        private class AvatarDownloadRequest
        {
            public Texture result = null;
            private string requestUrl = "";
            public bool requesting = false;

            public List<Action<Texture>> callbacks = new List<Action<Texture>>();
            public AvatarDownloadRequest(string url)
            {
                requestUrl = url;
            }
            public void GetTexture(CloudServiceManager cloudServiceManagerInstance, Action<Texture> callback)
            {
                if(result != null && requesting == false)
                {
                    callback(result);
                    return;
                }

                callbacks.Add(callback);
                if(requesting == false)
                    StartRequest(cloudServiceManagerInstance);
            }
            private void StartRequest(CloudServiceManager cloudServiceManagerInstance)
            {
                requesting = true;
                result = null;
                cloudServiceManagerInstance.StartCoroutine(DownLoadAvatarCR());
            }
            private IEnumerator DownLoadAvatarCR()
            {
                WWW w = new WWW(requestUrl);
                yield return w;
                if (string.IsNullOrEmpty(w.error))
                {
                    this.result = w.texture;
                }
                foreach (var cb in callbacks)
                {
                    try
                    {
                        cb(result);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("Something wrong happen when try to trigger callback to requesters " + ex.Message);
                    }
                }
                callbacks.Clear();
                requesting = false;
            }
        }



        public void GetDailyWeeklyChallengeEntry(Action<LogEventResponse> callback = null)
        {
            new LogEventRequest()
                .SetEventKey(GET_CHALLENGE_ENTRIES_COUNT_KEY)
                .Send((response) =>
                {
                    if(response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                    }
                    else
                    {
                        if (callback != null)
                            callback(response);
                    }
                });
        }

        public void GetAllLeaderBoardCount(Action<LogEventResponse> callback = null)
        {
            new LogEventRequest()
                .SetEventKey(GET_ALL_LB_COUNT_KEY)
                .Send((response) =>
                {
                    if (response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                    }
                    else
                    {
                        if (callback != null)
                            callback(response);
                    }
                });
        }
        private string GET_PUZZLE_MULTIPLAYERS = "GET_PUZZLE_MULTIPLAYERS";
        private string MULTI_SIZE_ATTRIBUTE = "SIZE";
        private string MULTI_LEVEL_ATTRIBUTE = "LEVEL";
        private string MULTI_PUZZLE_OFFSET_ATTRIBUTE = "RAND";
        public void GetMultiPlayerPuzzle(int level,int size, int offset,Action<GSData> callback = null)
        {
            new LogEventRequest()
                .SetEventKey(GET_PUZZLE_MULTIPLAYERS)
                .SetEventAttribute(MULTI_LEVEL_ATTRIBUTE, level)
                .SetEventAttribute(MULTI_SIZE_ATTRIBUTE, size)
                .SetEventAttribute(MULTI_PUZZLE_OFFSET_ATTRIBUTE, offset)
                .Send((response) =>
                {
                    if(response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                        callback(null);
                    }else if (response.ScriptData.ContainsKey("error"))
                    {
                        Debug.LogWarning("Can not find suitable puzzle");
                        callback(null);
                    }
                    else
                    {
                        callback(response.ScriptData.GetGSData("puzzle"));
                    }
                });

        }

        private string GET_SERVER_TIME = "GET_SERVER_TIME";
        public void GetCurrentServerTime(Action<double> callback)
        {
            new LogEventRequest()
                .SetEventKey(GET_SERVER_TIME)
                .Send(response =>
                {
                    if (response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                    }
                    if (callback != null)
                        callback(response.ScriptData.GetDouble("time")??-1);
                });
        }

        public void GetYDTopEntries(string lbShortCode, Action<List<GSData>> callback)
        {
            new LogEventRequest()
                .SetEventKey(GET_YD_TOP_ENTRIES_EVENT_KEY)
                .SetEventAttribute(LB_SHORT_CODE_ATTRIBUTE_KEY, lbShortCode)
                .Send(response =>{
                    if(response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                        if(callback !=null)
                        {
                            callback(null);
                        }
                    }
                    else{
                        if(callback != null)
                        {
                            if(response.ScriptData.GetGSDataList(TOP_ENTRIES_DATA_LIST_NAME) == null)
                            {
                                callback(new List<GSData>());
                            }else{
                                callback(response.ScriptData.GetGSDataList(TOP_ENTRIES_DATA_LIST_NAME));
                            }
                        }
                    }
                });
        }

        /// <summary>
        /// Submit one signal id for notification when they win a challenge
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        public void SubmitOneSignalId(string id, Action<LogEventResponse> callback = null)
        {
            if (string.IsNullOrEmpty(id))
                return;
            new LogEventRequest()
                .SetDurable(true)
                .SetEventKey(SUBMIT_ONESIGNAL_ID_KEY)
                .SetEventAttribute(ONESIGNAL_ID_ATTRIBUTE, id)
                .Send((response) =>
                {
                    if (response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                    }
                    if (callback != null)
                        callback(response);
                });
        }


        List<Action<LogEventResponse>> rewardResponseCallBack = new List<Action<LogEventResponse>>();

        /// <summary>
        /// Pull all unclaimed rewards
        /// </summary>
        /// <param name="callback"></param>
        public void RequestReward(Action<LogEventResponse> callback = null)
        {
            if(rewardResponseCallBack.Count > 0)
            {
                rewardResponseCallBack.Add(callback);
                return;
            }
            rewardResponseCallBack.Add(callback);
            new LogEventRequest()
                .SetEventKey(GET_REWARD_KEY)
                .Send((response) =>
                {
                    if (response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                    }

                    foreach (var cb in rewardResponseCallBack)
                    {
                        if (cb != null)
                        {
                            cb(response);
                        }
                    }
                    rewardResponseCallBack.Clear();
                });
        }

        /// <summary>
        /// Claim a reward
        /// </summary>
        /// <param name="rewardId"></param>
        /// <param name="callback"></param>
        public void ClaimReward(string rewardId, Action<LogEventResponse> callback = null)
        {
            new LogEventRequest()
                .SetEventKey(CLAIM_REWARD_KEY)
                .SetEventAttribute(REWARD_ID_ATTRIBUTE, rewardId)
                .Send((response) =>
                {
                    if (response.HasErrors)
                    {
                        Debug.Log(response.Errors.JSON);
                    }

                    if (callback != null)
                        callback(response);
                });
        }

        /// <summary>
        /// Sync player data
        /// </summary>
        public void SyncPlayerDb()
        {
            if (isGuest)
                return;
            if (isSyncing)
                return;
            Debug.Log(PlayerDb.GetString("AGE", "-1"));
            onPlayerDbSyncBegin();
            //* Save this data to disk before sync */
            PlayerDb.Save();
            GSData playerDbData = PlayerDb.ToGSData();
            new LogEventRequest()
                .SetEventKey(SYNC_PLAYERDB_REMOTE_KEY)
                .SetEventAttribute(PLAYERDB_DATA_KEY, playerDbData.JSON)
                .Send(OnSyncSucceed, OnSyncFailed);
        }

        public void UpLoadLocalDb()
        {
            if (isGuest)
                return;
            if (isSyncing)
                return;
            Debug.Log("Upload player Data, Not Sync");
            Debug.Log(PlayerDb.GetString("AGE", "-1"));
            GSData playerDbData = PlayerDb.ToGSData();
            new LogEventRequest()
                .SetEventKey(SYNC_PLAYERDB_REMOTE_KEY)
                .SetEventAttribute(PLAYERDB_DATA_KEY, playerDbData.JSON)
                .Send(res => { Debug.Log(res.JSONData); });
        }

        const string PLAYER_ID = "PLAYER_ID";
        private void OnSyncSucceed(LogEventResponse r)
        {

            string json = r.ScriptData.GetString(PLAYERDB_DATA_KEY);
            if (json != null)
            {

                Dictionary<string, object> d = (Dictionary<string, object>)GSJson.From(json);
                GSData baseData = new GSData(d);
                string dataId = baseData.GetString(PLAYER_ID);
                if (dataId != playerId)
                {
                    OnSyncFailed("this data is not belong to this player, cancel sync operation");
                    return;
                }
                PlayerDb.FromGSData(baseData);
                onPlayerDbSyncSucceed();
                PlayerDb.SetUpToDate(true);
                PlayerDb.Save();
                Debug.Log(PlayerDb.GetString("AGE", "-1"));
            }
            else
            {
                Debug.LogError("Sync response data is null");
            }
            onPlayerDbSyncEnd();
        }

        private void OnSyncFailed(LogEventResponse r)
        {
            OnSyncFailed(r.Errors.JSON);
        }

        private void OnSyncFailed(string errorStr)
        {
            Debug.LogError(errorStr);
            onPlayerDbSyncEnd();
            onPlayerDbSyncFailed(errorStr);
        }

        private const string GET_ONE_TIME_DISCOUNT_PRODUCT_TIME_LEFT_KEY = "ONE_TIME_PURCHASING_DISCOUNT";
        private const string DISCOUNT_TIME_LEFT_KEY = "discountTimeLeft";
        private const string DISCOUNT_START_KEY = "discountStartTS";
        private const string DISCOUNT_END_KEY = "discountEndTS";
        public void GetOneTimePurchasingDiscountTimeLeft(Action<OneTimePurchasingDiscountInfo> callback)
        {
            new LogEventRequest()
            .SetEventKey(GET_ONE_TIME_DISCOUNT_PRODUCT_TIME_LEFT_KEY)
            .Send(res =>
            {
                if(res.HasErrors)
                {
                    callback(DefaultDiscountInfo);
                    return;
                }
                callback(new OneTimePurchasingDiscountInfo()
                {
                    timeLeft = TimeSpan.FromMilliseconds(res.ScriptData.GetFloat(DISCOUNT_TIME_LEFT_KEY) ?? 0),
                    startDate = GetDateTimeFromEpochTS(res.ScriptData.GetDouble(DISCOUNT_START_KEY) ?? 0),
                    endDate = GetDateTimeFromEpochTS(res.ScriptData.GetDouble(DISCOUNT_END_KEY) ?? 0)
                });
            });
        }

        private DateTime GetDateTimeFromEpochTS(double ts)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(ts);
        }

        public static OneTimePurchasingDiscountInfo DefaultDiscountInfo = new OneTimePurchasingDiscountInfo()
        {
            timeLeft = TimeSpan.FromMilliseconds(-1),
            startDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            endDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        public struct OneTimePurchasingDiscountInfo
        {
            public TimeSpan timeLeft;
            public DateTime startDate;
            public DateTime endDate;
        }
    }
}