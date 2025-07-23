using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameSparks.Core;
using GameSparks.Api.Responses;
using System.Globalization;
using System.Linq;


namespace Takuzu
{
    public class LeaderboardBuilder : MonoBehaviour
    {
        public const string LB_EXP_KEY = "LB_EXP";
        public const string LB_DAILY_KEY = "LB_DAILY";
        public const string LB_WEEKLY_KEY = "LB_WEEKLY";

        public const string LB_EXP_COUNTRY_KEY = "LB_EXP_COUNTRY";
        public const string LB_DAILY_COUNTRY_KEY = "LB_DAILY_COUNTRY";
        public const string LB_WEEKLY_COUNTRY_KEY = "LB_WEEKLY_COUNTRY";

        public const string TYPE_EXP = "EXP";
        public const string TYPE_DAILY = "DAILY";
        public const string TYPE_WEEKLY = "WEEKLY";
        public string TYPE_CURRENT;

        public const int GROUP_BY_INDIVIDUAL = 0;
        public const int GROUP_BY_FRIENDS = 1;
        public const int GROUP_BY_COUNTRY = 2;
        public int GROUP_CURRENT;

        public const string NO_CONNECTION_MSG = "Can't connect to server.";
        public const string NO_DATA_MSG = "There's no record yet.";

        public GameObject entryTemplate;
        public ListView listView;
        public GameObject currentPlayerEntryTemplate;
        public GameObject currentPlayerEntryRoot;
        public Text listViewMessageText;
        public Text currentPlayerEntryMessageText;

        private string listViewMessage;

        public string ListViewMessage
        {
            get
            {
                return listViewMessageText != null ? listViewMessageText.text : string.Empty;
            }
            set
            {
                if (listViewMessageText == null)
                    return;

                listViewMessage = value;
                if (string.IsNullOrEmpty(value))
                {
                    listViewMessageText.CrossFadeAlpha(0, 0.5f, false);
                    CoroutineHelper.Instance.DoActionDelay(() =>
                        {
                            if (string.IsNullOrEmpty(listViewMessage))
                                listViewMessageText.text = listViewMessage;
                        }, 0.5f);
                }
                else
                {
                    listViewMessageText.text = value;
                    listViewMessageText.CrossFadeAlpha(1, 0.5f, false);
                }
            }
        }

        private string currentPlayerEntryMessage;

        public string CurrentPlayerEntryMessage
        {
            get
            {
                return currentPlayerEntryMessageText != null ? currentPlayerEntryMessageText.text : string.Empty;
            }
            set
            {
                if (currentPlayerEntryMessageText == null)
                    return;
                currentPlayerEntryMessage = value;
                if (string.IsNullOrEmpty(value))
                {
                    currentPlayerEntryMessageText.CrossFadeAlpha(0, 0.5f, false);
                    CoroutineHelper.Instance.DoActionDelay(() =>
                        {
                            if (string.IsNullOrEmpty(currentPlayerEntryMessage))
                                currentPlayerEntryMessageText.text = currentPlayerEntryMessage;
                        }, 0.5f);
                }
                else
                {
                    currentPlayerEntryMessageText.text = value;
                    currentPlayerEntryMessageText.CrossFadeAlpha(1, 0.5f, false);
                }
            }
        }

        public Text leaderboardName;
        public int maxRank;

        public bool isLoadingCurrentPlayerEntry;
        public bool isLoadingListviewFirstEntry;
        public bool isLoadingMore;



        private string entryInfo;
        private string entrySecondaryInfo;
        private Sprite entrySecondaryIcon;
        private bool entryHasSecondaryInfo;
        private object entryPrimaryData;
        private Dictionary<string, string> lbNameDict;

        private System.Action<GSData> getInfoAction;
        private Dictionary<string, System.Action<GSData>> getInfoActionDict;

        private System.Action<GSEnumerable<LeaderboardDataResponse._LeaderboardData>> readLbDataAction;
        private Dictionary<string, System.Action<GSEnumerable<LeaderboardDataResponse._LeaderboardData>>> readLbDataActionDict;

        private System.Action<GSData> readCurrentRankAction;
        private Dictionary<string, System.Action<GSData>> readCurrentRankActionDict;

        private bool useCountryLb;
        private Dictionary<string, bool> useCountryLbDict;


        static List<Texture2D> flags;
        public string lbShortCode;
        public bool social;
        private bool initialized;
        private int currentEntryCount;
        private string currentRequestId;
        private int currentRank;

        public static void LoadFlags()
        {
            flags = new List<Texture2D>();
            flags.AddRange(Resources.LoadAll<Texture2D>("flags"));
        }

        public static void UnloadAllFlags()
        {
            List<Texture2D> tmp = flags;
            flags = new List<Texture2D>();
            CoroutineHelper.Instance.ForeachPerFrame(
                (t) =>
                {
                    Resources.UnloadAsset(t);
                    Debug.Log("Unload flag " + t.name);
                },
                tmp);
        }

        private void Update()
        {
            if (currentPlayerEntryRoot.transform.childCount > 1)
            {
                Destroy(currentPlayerEntryRoot.transform.GetChild(0).gameObject);
            }
        }

        public static Texture2D GetFlag(string code)
        {
            if (flags == null)
                flags = new List<Texture2D>();
            Texture2D t = flags.Find((tex) =>
                {
                    return tex.name.Equals(code);
                });
            if (t == null)
            {
                t = Resources.Load<Texture2D>(string.Format("{0}/{1}", "flags", code));
                if (t != null)
                    flags.Add(t);
            }
            return t;
        }

        private void Awake()
        {
            Init();

            SocialManager.onFbLogout += OnFacebookLogout;
        }

        private void OnDestroy()
        {
            SocialManager.onFbLogout -= OnFacebookLogout;
        }

        private void OnFacebookLogout()
        {
            ClearEntries();
            ClearCurrentPlayerEntry();
        }

        public void Init()
        {
            lbShortCode = LB_EXP_KEY;
            TYPE_CURRENT = TYPE_EXP;
            GROUP_CURRENT = GROUP_BY_INDIVIDUAL;

            lbNameDict = new Dictionary<string, string>();
            lbNameDict.Add(LB_EXP_KEY, "TOP SOLVERS");
            lbNameDict.Add(LB_DAILY_KEY, "DAILY CHALLENGE");
            lbNameDict.Add(LB_WEEKLY_KEY, "WEEKLY CHALLENGE");
            lbNameDict.Add(LB_EXP_COUNTRY_KEY, "TOP COUNTRIES");
            lbNameDict.Add(LB_DAILY_COUNTRY_KEY, "DAILY CHALLENGE");
            lbNameDict.Add(LB_WEEKLY_COUNTRY_KEY, "WEEKLY CHALLENGE");

            getInfoActionDict = new Dictionary<string, System.Action<GSData>>();
            getInfoActionDict.Add(LB_EXP_KEY, GetExpInfoString);
            getInfoActionDict.Add(LB_DAILY_KEY, GetSolvingTimeInfoString);
            getInfoActionDict.Add(LB_WEEKLY_KEY, GetSolvingTimeInfoString);
            getInfoActionDict.Add(LB_EXP_COUNTRY_KEY, GetCountryExpInfoString);
            getInfoActionDict.Add(LB_DAILY_COUNTRY_KEY, GetCountrySolvingTimeInfoString);
            getInfoActionDict.Add(LB_WEEKLY_COUNTRY_KEY, GetCountrySolvingTimeInfoString);

            readLbDataActionDict = new Dictionary<string, System.Action<GSEnumerable<LeaderboardDataResponse._LeaderboardData>>>();
            readLbDataActionDict.Add(LB_EXP_KEY, ReadIndividualLbData);
            readLbDataActionDict.Add(LB_DAILY_KEY, ReadIndividualLbData);
            readLbDataActionDict.Add(LB_WEEKLY_KEY, ReadIndividualLbData);
            readLbDataActionDict.Add(LB_EXP_COUNTRY_KEY, ReadCountryLbData);
            readLbDataActionDict.Add(LB_DAILY_COUNTRY_KEY, ReadCountryLbData);
            readLbDataActionDict.Add(LB_WEEKLY_COUNTRY_KEY, ReadCountryLbData);

            readCurrentRankActionDict = new Dictionary<string, System.Action<GSData>>();
            readCurrentRankActionDict.Add(LB_EXP_KEY, ReadCurrentPlayerRankData);
            readCurrentRankActionDict.Add(LB_DAILY_KEY, ReadCurrentPlayerRankData);
            readCurrentRankActionDict.Add(LB_WEEKLY_KEY, ReadCurrentPlayerRankData);
            readCurrentRankActionDict.Add(LB_EXP_COUNTRY_KEY, ReadCurrentTeamRankData);
            readCurrentRankActionDict.Add(LB_DAILY_COUNTRY_KEY, ReadCurrentTeamRankData);
            readCurrentRankActionDict.Add(LB_WEEKLY_COUNTRY_KEY, ReadCurrentTeamRankData);

            useCountryLbDict = new Dictionary<string, bool>();
            useCountryLbDict.Add(LB_EXP_KEY, false);
            useCountryLbDict.Add(LB_DAILY_KEY, false);
            useCountryLbDict.Add(LB_WEEKLY_KEY, false);
            useCountryLbDict.Add(LB_EXP_COUNTRY_KEY, true);
            useCountryLbDict.Add(LB_DAILY_COUNTRY_KEY, true);
            useCountryLbDict.Add(LB_WEEKLY_COUNTRY_KEY, true);

            initialized = true;
        }

        public void SetupParameters(string lbType, int groupBy)
        {
            TYPE_CURRENT = lbType;
            GROUP_CURRENT = groupBy;
            social = groupBy == GROUP_BY_FRIENDS;
            if (groupBy == GROUP_BY_COUNTRY)
            {
                lbShortCode =
                    lbType.Equals(TYPE_EXP) ? LB_EXP_COUNTRY_KEY :
                    lbType.Equals(TYPE_DAILY) ? LB_DAILY_COUNTRY_KEY :
                    lbType.Equals(TYPE_WEEKLY) ? LB_WEEKLY_COUNTRY_KEY :
                    string.Empty;
            }
            else
            {
                lbShortCode =
                    lbType.Equals(TYPE_EXP) ? LB_EXP_KEY :
                    lbType.Equals(TYPE_DAILY) ? LB_DAILY_KEY :
                    lbType.Equals(TYPE_WEEKLY) ? LB_WEEKLY_KEY :
                    string.Empty;
            }
        }

        public void RequestLeaderboardData(int entryCount = 10)
        {
            if (!initialized)
                Init();
            if (leaderboardName != null)
            {
                string lbName = "LEADERBOARD";
                lbNameDict.TryGetValue(lbShortCode, out lbName);
                leaderboardName.text = lbName;
            }
            bool errorWillRetry = false;
            if (!getInfoActionDict.TryGetValue(lbShortCode, out getInfoAction))
                errorWillRetry = true;
            if (!readCurrentRankActionDict.TryGetValue(lbShortCode, out readCurrentRankAction))
                errorWillRetry = true;
            if (!readLbDataActionDict.TryGetValue(lbShortCode, out readLbDataAction))
                errorWillRetry = true;
            if (!useCountryLbDict.TryGetValue(lbShortCode, out useCountryLb))
                errorWillRetry = true;
            if (errorWillRetry)
            {
                float delay = 0.5f;
                Debug.Log("Error when init parameter from dictionary, retry in " + delay + " second(s)");
                CoroutineHelper.Instance.DoActionDelay(() =>
                    {
                        RequestLeaderboardData();
                    }, delay);
                return;
            }

            listView.displayDataAction = DisplayData;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                ListViewMessage = NO_CONNECTION_MSG;

                if (!CloudServiceManager.isGuest)
                {
                    CurrentPlayerEntryMessage = NO_CONNECTION_MSG;
                }
            }
            else
            {
                CurrentPlayerEntryMessage = string.Empty;
                ListViewMessage = string.Empty;
                isLoadingListviewFirstEntry = true;
                CloudServiceManager.Instance.RequestLeaderboardData(lbShortCode, social, entryCount, 0, OnRequestLeaderboardData);

                if (!CloudServiceManager.isGuest && SocialManager.Instance.IsLoggedInFb)
                {
                    isLoadingCurrentPlayerEntry = true;
                    CloudServiceManager.Instance.RequestCurrentPlayerRank(lbShortCode, social, useCountryLb, OnRequestCurrentPlayerRank);
                }
            }
        }

        public void RequestCurrentPlayerRank()
        {
            if (!CloudServiceManager.isGuest && SocialManager.Instance.IsLoggedInFb)
            {
                CurrentPlayerEntryMessage = string.Empty;
                isLoadingCurrentPlayerEntry = true;
                CloudServiceManager.Instance.RequestCurrentPlayerRank(lbShortCode, social, useCountryLb, OnRequestCurrentPlayerRank);
            }
            else
            {
                isLoadingCurrentPlayerEntry = false;
            }
        }

        public void RequestMoreEntry(int entryCount)
        {
            isLoadingMore = true;
            CloudServiceManager.Instance.RequestLeaderboardData(lbShortCode, social, entryCount, currentEntryCount, OnRequestLeaderboardData);
        }

        private void OnRequestLeaderboardData(LeaderboardDataResponse response)
        {
            string responseLbShortCode = response.BaseData.GetString("leaderboardShortCode");
            if (!string.IsNullOrEmpty(responseLbShortCode) &&
                !responseLbShortCode.Contains(lbShortCode))
            {
                Debug.Log(string.Format("Reject LB response: response: {0} - current: {1}", responseLbShortCode, lbShortCode));
                return;
            }

            isLoadingMore = false;
            isLoadingListviewFirstEntry = false;
            GSEnumerable<LeaderboardDataResponse._LeaderboardData> data = response.Data;
            if (data == null || !data.GetEnumerator().MoveNext())
            {
                if (listView.DataCount == 0)
                    ListViewMessage = NO_DATA_MSG;
            }
            else
            {
                ListViewMessage = string.Empty;
                readLbDataAction(data);
            }

        }

        private void OnRequestCurrentPlayerRank(LeaderboardsEntriesResponse response)
        {
            isLoadingCurrentPlayerEntry = false;
            //a dirty fix for response.BaseData.GetGSData(lbShortCode) 
            //because the response contain a lb code with the syntax of <lbShortCode>.<SNAPSHOT>.<some-date> if the LB is periodly resetted
            IEnumerator<KeyValuePair<string, object>> i = response.JSONData.GetEnumerator();
            GSData data = null;
            List<object> dataList = null;
            while (i.MoveNext())
            {
                if (i.Current.Key.Contains(lbShortCode))
                {
                    dataList = i.Current.Value as List<object>;
                }
            }

            if (dataList == null || dataList.Count == 0)
            {
                if (currentPlayerEntryRoot.transform.childCount == 0)
                    CurrentPlayerEntryMessage = NO_DATA_MSG;
                else
                    CurrentPlayerEntryMessage = string.Empty;
                return;
            }
            else
            {
                data = new GSData(dataList[0] as Dictionary<string, object>);
            }

            if (data.BaseData.Count == 0 && currentPlayerEntryRoot.transform.childCount == 0)
            {
                CurrentPlayerEntryMessage = NO_DATA_MSG;
            }
            else
            {
                CurrentPlayerEntryMessage = string.Empty;
                readCurrentRankAction(data);
            }
        }

        private void ReadCurrentPlayerRankData(GSData data)
        {
            try
            {
                string playerName = data.GetString("userName");
                int rank = (int)data.GetInt("rank");
                GameObject g = Instantiate(currentPlayerEntryTemplate);
                g.transform.SetParent(currentPlayerEntryRoot.transform, false);
                LeaderboardEntry e = g.GetComponent<LeaderboardEntry>();
                g.SetActive(true);
                e.SetName(playerName);
                e.SetRank(rank);
                getInfoAction(data);
                e.SetPrimaryInfo(entryInfo);
                e.SetSecondaryInfo(entrySecondaryInfo);
                e.SetSecondaryIcon(entrySecondaryIcon);
                LeaderboardEntryParsedData parsedData = new LeaderboardEntryParsedData
                {
                    debugId = System.Guid.NewGuid().ToString(),
                    rank = rank,
                    playerName = playerName,
                    secondaryIcon = entrySecondaryIcon,
                    secondaryInfo = entrySecondaryInfo,
                    primaryInfo = entryInfo,
                    hasSecondaryInfo = entryHasSecondaryInfo,
                    destroyAvatarOnDispose = true,
                    isCurrentPlayerEntry = true,
                    primaryInfoData = entryPrimaryData,
                    displayExpSlider = TYPE_CURRENT.Equals(TYPE_EXP)

                };
                CloudServiceManager.Instance.RequestAvatarForPlayer(data.GetString("userId"), (response) =>
                    {
                        if (response.HasErrors)
                            return;
                        string url = response.ScriptData.GetString("FbAvatarUrl");
                        if (string.IsNullOrEmpty(url))
                            return;
                        CloudServiceManager.Instance.DownloadLbAvatar(url, parsedData);
                    });
                e.SetData(parsedData);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error on populate leaderboard entry: " + e.ToString());
            }
        }

        private void ReadCurrentTeamRankData(GSData data)
        {
            try
            {
                string countryCode = data.GetString("teamName");
                int rank = (int)data.GetInt("rank");

                GameObject g = Instantiate(currentPlayerEntryTemplate);
                g.transform.SetParent(currentPlayerEntryRoot.transform, false);
                LeaderboardEntry e = g.GetComponent<LeaderboardEntry>();
                g.SetActive(true);

                string countryEnglishName = Utilities.CountryNameFromCode(countryCode);
                e.SetName(countryEnglishName);

                e.SetRank(rank);
                getInfoAction(data);
                e.SetPrimaryInfo(entryInfo);
                e.SetSecondaryInfo(entrySecondaryInfo);
                e.SetSecondaryIcon(entrySecondaryIcon);
                Texture2D flag = GetFlag(countryCode.ToLower());
                if (flag != null)
                    e.SetAvatar(flag);
                LeaderboardEntryParsedData parsedData = new LeaderboardEntryParsedData
                {
                    debugId = System.Guid.NewGuid().ToString(),
                    rank = rank,
                    playerName = countryEnglishName,
                    secondaryIcon = entrySecondaryIcon,
                    secondaryInfo = entrySecondaryInfo,
                    primaryInfo = entryInfo,
                    hasSecondaryInfo = entryHasSecondaryInfo,
                    destroyAvatarOnDispose = false,
                    avatar = flag,
                    flagCode = countryCode.ToLower(),
                    isCurrentPlayerEntry = true,
                    primaryInfoData = entryPrimaryData
                };
                e.SetData(parsedData);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error on populate leaderboard entry: " + e.ToString());
            }
        }

        private void ReadIndividualLbData(GSEnumerable<LeaderboardDataResponse._LeaderboardData> data)
        {
            List<object> parsedDataCollection = new List<object>();
            foreach (var entry in data)
            {
                try
                {
                    int rank = (int)entry.Rank;
                    if (rank <= currentRank || currentRank >= maxRank)
                        continue;
                    currentEntryCount += 1;
                    currentRank = rank;
                    string playerName = entry.UserName;
                    getInfoAction(entry.BaseData);
                    LeaderboardEntryParsedData parsedData = new LeaderboardEntryParsedData
                    {
                        debugId = System.Guid.NewGuid().ToString(),
                        playerId = entry.UserId,
                        rank = rank,
                        playerName = playerName,
                        secondaryIcon = entrySecondaryIcon,
                        secondaryInfo = entrySecondaryInfo,
                        primaryInfo = entryInfo,
                        hasSecondaryInfo = entryHasSecondaryInfo,
                        destroyAvatarOnDispose = true,
                        isCurrentPlayerEntry = false,
                        primaryInfoData = entryPrimaryData
                    };
                    CloudServiceManager.Instance.RequestAvatarForPlayer(entry.UserId, (response) =>
                        {
                            if (response.HasErrors)
                                return;
                            string url = response.ScriptData.GetString("FbAvatarUrl");
                            if (string.IsNullOrEmpty(url))
                                return;
                            CloudServiceManager.Instance.DownloadLbAvatar(url, parsedData);
                        });
                    parsedDataCollection.Add(parsedData);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error on populate leaderboard entry: " + e.ToString());

                }
            }

            listView.AppendData(parsedDataCollection);
        }

        private void ReadCountryLbData(GSEnumerable<LeaderboardDataResponse._LeaderboardData> data)
        {
            List<object> parsedDataCollection = new List<object>();
            foreach (var entry in data)
            {
                try
                {
                    string countryCode = entry.GetStringValue("teamName");
                    int rank = (int)entry.Rank;
                    if (rank <= currentRank || currentRank >= maxRank)
                        return;
                    currentRank = rank;
                    currentEntryCount += 1;
                    getInfoAction(entry.BaseData);
                    string countryEnglishName = Utilities.CountryNameFromCode(countryCode);
                    LeaderboardEntryParsedData parsedData = new LeaderboardEntryParsedData()
                    {
                        debugId = System.Guid.NewGuid().ToString(),
                        playerId = entry.UserId,
                        playerName = countryEnglishName,
                        rank = rank,
                        primaryInfo = entryInfo,
                        secondaryInfo = entrySecondaryInfo,
                        secondaryIcon = entrySecondaryIcon,
                        hasSecondaryInfo = entryHasSecondaryInfo,
                        destroyAvatarOnDispose = false,
                        isCurrentPlayerEntry = false,
                        flagCode = countryCode.ToLower(),
                        primaryInfoData = entryPrimaryData
                    };

                    Texture2D flag = GetFlag(countryCode.ToLower());
                    if (flag != null)
                        parsedData.avatar = flag;
                    parsedData.isLoadingAvatar = false;
                    parsedDataCollection.Add(parsedData);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error on populate leaderboard entry: " + e.ToString());
                }
            }
            listView.AppendData(parsedDataCollection);
        }

        public void ClearCurrentPlayerEntry()
        {
            currentPlayerEntryRoot.transform.ClearAllChildren();
        }

        public void ClearEntries()
        {
            currentEntryCount = 0;
            currentRank = 0;
            //entryRoot.transform.ClearAllChildren();
            listView.ClearData();
            currentPlayerEntryRoot.transform.ClearAllChildren();
        }

        private void GetExpInfoString(GSData data)
        {
            int? expInt = data.GetInt("LAST-EXP");
            if (expInt.HasValue)
            {
                PlayerInfo info = ExpProfile.active.FromTotalExp(expInt.Value);
                string rank = ExpProfile.active.rank[info.level];
                string exp = expInt.Value.ToString();
                entryInfo = string.Format("{0}XP", exp);
                entrySecondaryInfo = rank;
                entrySecondaryIcon = ExpProfile.active.icon[info.level];
                entryHasSecondaryInfo = true;
                entryPrimaryData = expInt.Value;
            }
            else
            {
                entryInfo = "_";
                entrySecondaryInfo = "_";
                entrySecondaryIcon = null;
                entryHasSecondaryInfo = false;
                entryPrimaryData = null;
            }
        }

        private void GetSolvingTimeInfoString(GSData data)
        {
            long? tick = data.GetLong("MIN-SOLVING_TIME");
            if (tick.HasValue)
            {
                System.TimeSpan solvingTime = new System.TimeSpan(tick.Value * System.TimeSpan.TicksPerMillisecond);
                entryInfo = string.Format("{0:00}:{1:00}:{2:00}", solvingTime.Hours, solvingTime.Minutes, solvingTime.Seconds);
                entrySecondaryInfo = string.Empty;
                entrySecondaryIcon = null;
                entryHasSecondaryInfo = false;
                entryPrimaryData = solvingTime;
            }
            else
            {
                entryInfo = "_";
                entrySecondaryInfo = string.Empty;
                entrySecondaryIcon = null;
                entryHasSecondaryInfo = false;
                entryPrimaryData = null;
            }
        }

        private void GetCountryExpInfoString(GSData data)
        {
            long? exp = data.GetLong("LAST-AVG_EXP");
            if (exp.HasValue)
            {
                entryInfo = string.Format("Avg. {0}XP", exp.Value);
                PlayerInfo info = ExpProfile.active.FromTotalExp((int)exp.Value);
                string rank = ExpProfile.active.rank[info.level];
                entrySecondaryInfo = string.Empty;
                entrySecondaryIcon = null;
                entryHasSecondaryInfo = false;
                entryPrimaryData = exp.Value;
            }
            else
            {
                entryInfo = "_";
                entrySecondaryInfo = string.Empty;
                entrySecondaryIcon = null;
                entryHasSecondaryInfo = false;
                entryPrimaryData = null;
            }
        }

        private void GetCountrySolvingTimeInfoString(GSData data)
        {
            long? tick = data.GetLong("LAST-AVG_SOLVING_TIME");
            if (tick.HasValue)
            {
                System.TimeSpan solvingTime = new System.TimeSpan(tick.Value * System.TimeSpan.TicksPerMillisecond);
                entryInfo = string.Format("Avg. {0:00}:{1:00}:{2:00}", solvingTime.Hours, solvingTime.Minutes, solvingTime.Seconds);
                entrySecondaryInfo = string.Empty;
                entrySecondaryIcon = null;
                entryHasSecondaryInfo = false;
                entryPrimaryData = solvingTime;
            }
            else
            {
                entryInfo = "_";
                entrySecondaryInfo = string.Empty;
                entrySecondaryIcon = null;
                entryHasSecondaryInfo = false;
                entryPrimaryData = null;
            }
        }

        private void DisplayData(GameObject entry, object data)
        {
            LeaderboardEntry e = entry.GetComponent<LeaderboardEntry>();
            LeaderboardEntryParsedData parsedData = (LeaderboardEntryParsedData)data;
            e.SetData(parsedData);
        }
    }
}