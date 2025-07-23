using System;
using System.Collections;
using System.Collections.Generic;
using GameSparks.Api.Responses;
using GameSparks.Core;
using Pinwheel;
using Takuzu;
using Takuzu.Generator;
using UnityEngine;
using UnityEngine.UI;

public class TopPlayerPanel : OverlayPanel
{
    [System.Serializable]
    public struct TabSizeButtons
    {
        public Button button;
        public Text sizeTxt;
        public Image bg;
    }

    [Header("UI References")]
    public List<TabSizeButtons> sizeTabButtons;
    public Text subTitle;

    public Color tabTextActiveColor;
    public Color tabTextInActiveColor;
    public Color tabBgActiveColor;
    public Color tabBgInActiveColor;
    private int currentSizeIndex;
    private int currentLevelIndex;

    static List<Texture2D> flags;
    public string challengeId = "";
    public Puzzle currentChallenge;
    public ListView listView;
    public OverlayGroupController controller;
    public AnimController loadingBarAnimation;
    public Text title;

    public Button closeButton;
    [HideInInspector]
    public Sprite[] topIconSprite;

    public static string playerDefaultName
    {
        get { return I2.Loc.ScriptLocalization.GUEST; }
    }

    public void SetChallengeId(string id = "ExpLB")
    {
        challengeId = id;
    }

    public void SetTitle(string titleText)
    {
        title.text = titleText;
    }

    private class YDLBRequest{
        public string lbCode = "";
        private List<GSData> results;
        private bool finished = false;
        private bool requestIsSent = false;
        private List<Action<List<GSData>>> callbacks = new List<Action<List<GSData>>>();

        public void SendRequest(Action<List<GSData>> cb ,bool reRequest = false){
            callbacks.Add(cb);
            if(finished == true && reRequest == false)
            {
                CallbackFetchedResultToAll();
                return;
            }

            if(requestIsSent == true)
                return;

            RequestDataFromCloud();
        }

        public void ClearCallbackList()
        {
            callbacks.Clear();
        }

        private void RequestDataFromCloud()
        {
            finished = false;
            requestIsSent = true;
            CloudServiceManager.Instance.GetYDTopEntries(lbCode, results =>{
                if(results == null)
                {
                    RequestDataFromCloud();
                    return;
                }
                this.results = results;
                finished = true;
                requestIsSent = false;
                CallbackFetchedResultToAll();
            });
        }

        private void CallbackFetchedResultToAll()
        {
            foreach (var callback in callbacks)
            {
                callback(results);
            }
            callbacks.Clear();
        }
    }
    Dictionary<string, YDLBRequest> requestDictionary = new Dictionary<string, YDLBRequest>();
    private int[] availableLeaderboardSize = new int[]{6,8,10,12};
    private void LoadYDLeaderboard(int level, int sizeIndex, Action<List<GSData>> callback)
    {
        string lbCode = string.Format("LB_DAILY_LV{0}_SIZE{1}",level, availableLeaderboardSize[sizeIndex]);
        YDLBRequest request;
        if(requestDictionary.ContainsKey(lbCode))
        {
            request = requestDictionary[lbCode];
        }
        else
        {
            request = new YDLBRequest()
            {
                lbCode = lbCode
            };
            requestDictionary.Add(lbCode,request);
        }
        request.SendRequest(result =>{
            if(level == currentLevelIndex || sizeIndex == currentLevelIndex)
            {
                callback(result);
            }
        });
    }

    private void UpdateLeaderBoard()
    {
        if(IsShowing == false)
            return;
        listView.ClearData();
        loadingBarAnimation.Play();
        LoadYDLeaderboard(currentLevelIndex, currentSizeIndex, result =>{
            loadingBarAnimation.Stop();
            listView.ClearData();
            if(this == null)
                return;
            if(gameObject == null)
                return;
            List<LeaderboardEntryParsedData> parsedDataCollection = GetParsedDataCollection(result);
            listView.AppendData(parsedDataCollection);
        });
    }

    private List<LeaderboardEntryParsedData> GetParsedDataCollection(List<GSData> leaderboardData)
    {
        List<LeaderboardEntryParsedData> parsedDataCollection = new List<LeaderboardEntryParsedData>();

        foreach (var item in leaderboardData)
        {
            long? solveT = item.GetLong("solvingTime");
            TimeSpan timeSpan = TimeSpan.FromSeconds(((float)(solveT ?? 0)) / 1000);
            string timeSpanStr = String.Format("{1:00}:{2:00}:{3:00}", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            string primaryInforTextStr = solveT.HasValue ? timeSpanStr : "NaN";
            int currentAge = item.GetInt("maxLevel") ?? -1;
            int rank = (int)(item.GetInt("rank") ?? -1);
            string playerId = item.GetString("playerId");
            string playerName = item.GetString("playerName");

            int currentNode = 0;
            for (int i = 0; i < PuzzleManager.Instance.ageList.Count; i++)
            {
                if (PuzzleManager.Instance.ageList[i] <= currentAge)
                    currentNode = i;
            }

            LeaderboardEntryParsedData parsedData = new LeaderboardEntryParsedData
            {
                debugId = System.Guid.NewGuid().ToString(),
                playerId = playerId,
                rank = rank,
                playerName = playerName != "" ? playerName: playerDefaultName,
                secondaryIcon = null,
                secondaryInfo = "loading",
                primaryInfo = primaryInforTextStr,
                hasSecondaryInfo = true,
                destroyAvatarOnDispose = true,
                isCurrentPlayerEntry = false,
                primaryInfoData = null
            };
            parsedData.topIcon = GetFrameByRank(parsedData.rank);
            if (string.IsNullOrEmpty(playerName) == false){
                CloudServiceManager.Instance.RequestAvatarForPlayer(playerId, (response) =>
                {
                    if (response.HasErrors)
                        return;
                    string url = response.ScriptData.GetString("FbAvatarUrl");
                    string age = response.ScriptData.GetString("AGE");
                    string maxProgressNode = response.ScriptData.GetString("MAX_PROGRESS_NODE");
                    if (maxProgressNode != null)
                    {
                        parsedData.secondaryInfo = maxProgressNode;
                    }
                    else
                    {
                        if (parsedData.secondaryInfo == "loading")
                        {
                            parsedData.secondaryInfo = "no data";
                        }
                    }
                    if (string.IsNullOrEmpty(url))
                        return;
                    parsedData.avatarUrl = url;
                    if (listView.scrolling == false)
                        CloudServiceManager.Instance.DownloadLbAvatar(url, parsedData);
                });
            }
            else
            {
                parsedData.secondaryInfo = currentNode.ToString();
            }

            parsedDataCollection.Add(parsedData);
        }
        return parsedDataCollection;
    }

    private void Start()
    {
        LoadTopFrameIcon();
        listView.displayDataAction += OnDisplayDataAction;
        closeButton.onClick.AddListener(delegate
        {
            Hide();
        });
        CloudServiceManager.onGamesparkAuthenticated += OnGamesparkAuthenticated;
    }

    private void OnGamesparkAuthenticated()
    {
        if (IsShowing)
            Hide();
    }

    public override void Hide()
    {
        loadingBarAnimation.Stop();
        controller.HideIfNot();
        IsShowing = false;
        onPanelStateChanged(this, false);
    }

    private void OnDestroy()
    {
        listView.displayDataAction -= OnDisplayDataAction;
        CloudServiceManager.onGamesparkAuthenticated -= OnGamesparkAuthenticated;

    }

    private void OnDisplayDataAction(GameObject element, object data)
    {
        UpdateEntriesUI(element, (LeaderboardEntryParsedData)data);
    }

    private void UpdateEntriesUI(GameObject element, LeaderboardEntryParsedData data)
    {
        LeaderboardEntry leaderboardEntry = element.GetComponent<LeaderboardEntry>();
        leaderboardEntry.SetData(data);
    }

    public override void Show()
    {
        controller.ShowIfNot();
        IsShowing = true;
        transform.BringToFront();
        onPanelStateChanged(this, true);
        StartCoroutine(UpdateDataCR());
    }

    private IEnumerator UpdateDataCR()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        UpdateLeaderBoard();
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
        sizeTabButtons[0].button.onClick.AddListener(() => SelectSizeTab(0));
        sizeTabButtons[1].button.onClick.AddListener(() => SelectSizeTab(1));
        sizeTabButtons[2].button.onClick.AddListener(() => SelectSizeTab(2));
        sizeTabButtons[3].button.onClick.AddListener(() => SelectSizeTab(3));
        SelectSizeTab(0);

        
    }

    public void SetLbLevel(int lv)
    {
        currentLevelIndex = lv;
        subTitle.text = Takuzu.Utilities.GetLocalizePackNameByIndex(currentLevelIndex);
    }

    private void SelectSizeTab(int index)
    {
        for (int i = 0; i < sizeTabButtons.Count; i++)
        {
            sizeTabButtons[i].bg.color = tabBgInActiveColor;
            sizeTabButtons[i].sizeTxt.color = tabTextInActiveColor;
        }
        sizeTabButtons[index].bg.color = tabBgActiveColor;
        sizeTabButtons[index].sizeTxt.color = tabTextActiveColor;
        currentSizeIndex = index;
        UpdateLeaderBoard();
    }

    private void LoadTopFrameIcon()
    {
        topIconSprite = new Sprite[10];
        for (int i = 0; i < 10; i++)
        {
            topIconSprite[i] = Resources.Load<Sprite>("topicon/icon-top-" + (i + 1));
        }
    }

    private Sprite GetFrameByRank(int rank)
    {
        if (rank > 0 && rank <= 10)
        {
            return topIconSprite[rank - 1];
        }
        else
        {
            return null;
        }
    }
}
