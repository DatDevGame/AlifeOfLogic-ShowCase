using GameSparks.Api.Responses;
using GameSparks.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using Takuzu;
using Takuzu.Generator;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;

public class LeaderBoardScreenUI : OverlayPanel
{
    [Header("UI References")]
    static List<Texture2D> flags;
    public string challengeId = "";
    public GameObject playerEntry;
    public GameObject loginEntry;
    public Puzzle currentChallenge;
    public ListView listView;
    public OverlayGroupController controller;
    public GameObject currentPlayerEntryRoot;
    public AnimController loadingBarAnimation;
    public Text title;
    public Text subTitle;
    public Button loginButton;

    [SerializeField] private List<SizeSelectionTab.TabButton> sizeTabButtons;

    public Text lbDetailInfor;
    public Button invidualButton;
    public Button friendButton;
    public Button countryButton;

    public Button closeButton;
    [HideInInspector]
    public Sprite[] topIconSprite;

    public static string playerDefaultName
    {
        get { return I2.Loc.ScriptLocalization.GUEST; }
    }

    public static string currentPlayerNameDefault
    {
        get { return I2.Loc.ScriptLocalization.ME; }
    }

    //public ColorAnimation darkentAnimation;
    //public Image darkenImage;

    public LeaderboardEntry currentEntry;

    private TournamentDataRequest.LeaderboardType currentLBType = TournamentDataRequest.LeaderboardType.Exp;
    private TournamentDataRequest.LeaderboardGroup currentGroupType = TournamentDataRequest.LeaderboardGroup.Normal;
    [SerializeField] private Color bgActiveColor;
    [SerializeField] private Color bgInactiveColor;
    [SerializeField] private Color txtActiveColor;
    [SerializeField] private Color txtInactiveColor;

    public TournamentDataRequest.LeaderboardType CurrentLBType
    {
        get
        {
            return currentLBType;
        }

        set
        {
            currentLBType = value;
        }
    }

    public TournamentDataRequest.LeaderboardGroup CurrentGroupType
    {
        get
        {
            return currentGroupType;
        }

        set
        {
            var oldValue = currentGroupType;
            currentGroupType = value;
            if (currentGroupType != oldValue)
                UpdateLeaderBoard();
        }
    }

    public void SetChallengeId(string id = "ExpLB")
    {
        challengeId = id;
    }

    private class RequestLeadboardData
    {
        int callbackCount = 0;
        List<LeaderboardDataResponse._LeaderboardData> leaderboardData;
        GSData allEntriesResponse;
        GSData leaderboardCountResponse;
        GSData allRankResponse;
        Action<List<LeaderboardDataResponse._LeaderboardData>, GSData, GSData, GSData> callback;
        public void StartRequest(TournamentDataRequest.LeaderboardGroup currentGroupType, TournamentDataRequest.LeaderboardType currentLBType, string challengeId, Action<List<LeaderboardDataResponse._LeaderboardData>, GSData, GSData, GSData> callback)
        {
            this.callback = callback;
            TournamentDataRequest.RequestTournamentLeaderboard(currentGroupType, currentLBType, (leaderboardData) =>
            {
                this.leaderboardData = leaderboardData;
                CheckAndCallback();
            }, challengeId);
            TournamentDataRequest.RequestTournamentPlayerCount(TournamentDataRequest.LeadboardPlayerCountType.DailyWeeklyCount, (allEntriesResponse) =>
            {
                this.allEntriesResponse = allEntriesResponse;
                CheckAndCallback();
            });
            TournamentDataRequest.RequestTournamentPlayerCount(TournamentDataRequest.LeadboardPlayerCountType.AllLeaderboardCount, (leaderboardCountResponse) =>
            {
                this.leaderboardCountResponse = leaderboardCountResponse;
                CheckAndCallback();
            });
            TournamentDataRequest.RequestTournamentPlayerCount(TournamentDataRequest.LeadboardPlayerCountType.AllLeaderboardEntriesCount, (allRankResponse) =>
            {
                this.allRankResponse = allRankResponse;
                CheckAndCallback();
            });
        }

        public void StartRequest(TournamentDataRequest.LeaderboardGroup currentGroupType, TournamentDataRequest.LeaderboardType currentLBType, string challengeId, Action<List<LeaderboardDataResponse._LeaderboardData>, TournamentDataRequest.LeaderboardGroup, TournamentDataRequest.LeaderboardType> callback)
        {
            TournamentDataRequest.RequestTournamentLeaderboard(currentGroupType, currentLBType, (leaderboardData) =>
            {
                callback(leaderboardData, currentGroupType, currentLBType);
            }, challengeId, true);
        }

        private void CheckAndCallback()
        {
            callbackCount++;
            if (callbackCount == 4)
                callback(leaderboardData, allEntriesResponse, leaderboardCountResponse, allRankResponse);
        }
    }

    public void SetTitle(string titleText)
    {
        title.text = titleText;
    }

    private void UpdateLeaderBoard()
    {
        SetCurrentEntriesLocalData();
        if (challengeId == "ExpLB")
            CurrentLBType = TournamentDataRequest.LeaderboardType.Exp;
        else
        {
            Debug.Log("LDB challengeId = " + challengeId);
            currentChallenge = PuzzleManager.Instance.GetChallengeById(challengeId);
            CurrentLBType = TournamentDataRequest.LeaderboardType.SolvingTime;
        }
        listView.ClearData();
        currentPlayerEntryRoot.transform.ClearAllChildren();
        loadingBarAnimation.Play();
        var requestLBData = new RequestLeadboardData();
        requestLBData.StartRequest(CurrentGroupType, CurrentLBType, challengeId, (leaderboardData, allEntriesResponse, leaderboardCountResponse, allRankResponse) =>
        {
            listView.ClearData();
            currentPlayerEntryRoot.transform.ClearAllChildren();
            loadingBarAnimation.Stop();
            if (!this)
                return;
            if (leaderboardData == null || allEntriesResponse == null || leaderboardCountResponse == null || allRankResponse == null)
                return;
            string lbShortCode = TournamentDataRequest.GetLbShortCode(CurrentGroupType, CurrentLBType, challengeId);
            string lbShortCodeWithSocialPreFix = TournamentDataRequest.GetLbShortCode(CurrentGroupType, CurrentLBType, challengeId, true);

            GSData playedData = allEntriesResponse.GetGSDataList("ChallengesCount").Find(item => (item.GetString("ChallengeId").Equals(challengeId)));
            int? played = playedData != null ? playedData.GetInt("COUNT") : 0;
            int? ranked = leaderboardCountResponse.GetInt(lbShortCode);

            GSData rankData = allRankResponse.GetGSData(lbShortCodeWithSocialPreFix);
            int? playerRank = rankData != null ? rankData.GetInt("rank") : -1;
            lbDetailInfor.text = String.Format("ChallengeId: {0}\n played: {1}, ranked: {2}, currentRank: {3}", challengeId, played ?? 0, ranked ?? 0, playerRank ?? -1);

            if (rankData == null)
                rankData = new GSData();

            long? currentSolveT = CurrentGroupType == TournamentDataRequest.LeaderboardGroup.Contries ? rankData.GetLong("LAST-AVG_SOLVING_TIME") : rankData.GetLong("SOLVING_TIME");
            TimeSpan currentTimeSpan = TimeSpan.FromSeconds(((float)(currentSolveT ?? 0)) / 1000);
            string currentRimeSpanStr = String.Format("{1:00}:{2:00}:{3:00}", currentTimeSpan.Days, currentTimeSpan.Hours, currentTimeSpan.Minutes, currentTimeSpan.Seconds);
            int? currentExp = CurrentGroupType == TournamentDataRequest.LeaderboardGroup.Contries ? rankData.GetInt("LAST-AVG_EXP") : PlayerInfoManager.Instance.expProfile.ToTotalExp(PlayerInfoManager.Instance.info);
            string currentPrimaryInforTextStr = currentSolveT.HasValue ? ((CurrentGroupType == TournamentDataRequest.LeaderboardGroup.Contries ? "Avg. " : "|  ") + currentRimeSpanStr)
            : currentExp.HasValue ? ((CurrentGroupType == TournamentDataRequest.LeaderboardGroup.Contries ? "Avg. " : "|  ") + currentExp.ToString() + " EXP") : "";
            if (currentLBType == TournamentDataRequest.LeaderboardType.SolvingTime && !currentSolveT.HasValue)
            {
                currentPrimaryInforTextStr = "";
            }

            LeaderboardEntryParsedData currentParsedData = new LeaderboardEntryParsedData
            {
                debugId = System.Guid.NewGuid().ToString(),
                playerId = CurrentGroupType == TournamentDataRequest.LeaderboardGroup.Contries ? CloudServiceManager.countryId : CloudServiceManager.playerId,
                rank = playerRank ?? -1,
                secondaryInfo = CurrentGroupType == TournamentDataRequest.LeaderboardGroup.Contries ? "" : StoryPuzzlesSaver.Instance.MaxNode.ToString(),
                primaryInfo = currentPrimaryInforTextStr,
                hasSecondaryInfo = !currentPrimaryInforTextStr.Equals(""),
                destroyAvatarOnDispose = true,
                isCurrentPlayerEntry = true,
                primaryInfoData = null
            };
            currentParsedData.topIcon = GetFrameByRank(currentParsedData.rank);

            if (CurrentGroupType == TournamentDataRequest.LeaderboardGroup.Contries)
            {
                if (!string.IsNullOrEmpty(CloudServiceManager.countryName))
                {
                    Texture2D flag = GetFlag(CloudServiceManager.countryName.ToLower());
                    if (flag != null)
                        currentParsedData.avatar = flag;
                }
                currentParsedData.isLoadingAvatar = false;
                currentParsedData.destroyAvatarOnDispose = false;
            }
            else
            {
                CloudServiceManager.Instance.RequestAvatarForPlayer(CloudServiceManager.playerId, (response) =>
                {
                    if (response == null)
                        return;
                    if (response.HasErrors)
                        return;
                    string url = response.ScriptData.GetString("FbAvatarUrl");
                    if (string.IsNullOrEmpty(url))
                        return;
                    CloudServiceManager.Instance.DownloadLbAvatar(url, currentParsedData);
                });
            }
            currentEntry.SetData(currentParsedData);
            if (leaderboardData == null)
                return;


            List<LeaderboardEntryParsedData> parsedDataCollection = GetParsedDataCollection(leaderboardData, playerRank);

            listView.AppendData(parsedDataCollection);
            listView.onReachLastElement += () =>
            {
                loadingBarAnimation.Play();
                requestLBData.StartRequest(CurrentGroupType, CurrentLBType, challengeId, (extraLeaderboardData, requestedGroup, requestedType) =>
                {
                    loadingBarAnimation.Stop();
                    if (CurrentGroupType == requestedGroup && CurrentLBType == requestedType)
                    {
                        List<LeaderboardEntryParsedData> extraEntries = GetParsedDataCollection(extraLeaderboardData, playerRank);
                        if (extraEntries.Count == 0)
                            return;
                        if ((listView.Data[listView.Data.Count - 1] as LeaderboardEntryParsedData).rank >= extraEntries[0].rank)
                            return;
                        listView.AppendData(extraEntries);
                    }
                });
            };

            listView.onStopScrolling += () =>
            {
                for (int i = listView.fromIndex; i < listView.toIndex; i++)
                {
                    if (string.IsNullOrEmpty((listView.Data[i] as LeaderboardEntryParsedData).avatarUrl) == false)
                        CloudServiceManager.Instance.DownloadLbAvatar((listView.Data[i] as LeaderboardEntryParsedData).avatarUrl, listView.Data[i] as LeaderboardEntryParsedData);
                }
            };
        });

    }

    private TournamentDetailPanel tournamentDetailPanel;
    internal void SetTournamentDetailPanel(TournamentDetailPanel tournamentDetailPanel)
    {
        this.tournamentDetailPanel = tournamentDetailPanel;
    }

    private List<LeaderboardEntryParsedData> GetParsedDataCollection(List<LeaderboardDataResponse._LeaderboardData> leaderboardData, int? playerRank)
    {
        List<LeaderboardEntryParsedData> parsedDataCollection = new List<LeaderboardEntryParsedData>();

        foreach (var item in leaderboardData)
        {
            if (CurrentGroupType == TournamentDataRequest.LeaderboardGroup.Contries)
            {
                GSData baseData = item.BaseData;
                long? solveT = baseData.GetLong("LAST-AVG_SOLVING_TIME");
                TimeSpan timeSpan = TimeSpan.FromSeconds(((float)(solveT ?? 0)) / 1000);
                string timeSpanStr = String.Format("{1:00}:{2:00}:{3:00}", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                int? exp = baseData.GetInt("LAST-AVG_EXP");
                string primaryInforTextStr = solveT.HasValue ? ("Avg.\n" + timeSpanStr) : exp.HasValue ? ("Avg.\n" + exp.ToString() + " EXP") : "NaN";
                int rank = baseData.GetInt("rank") ?? -1;
                string countryCode = baseData.GetString("teamName") ?? "";
                LeaderboardEntryParsedData parsedData = new LeaderboardEntryParsedData
                {
                    debugId = System.Guid.NewGuid().ToString(),
                    playerId = baseData.GetString("teamId"),
                    rank = baseData.GetInt("rank") ?? -1,
                    playerName = Utilities.CountryNameFromCode(countryCode),
                    secondaryIcon = null,
                    primaryInfo = primaryInforTextStr,
                    hasSecondaryInfo = false,
                    destroyAvatarOnDispose = true,
                    isCurrentPlayerEntry = rank != -1 && rank == (playerRank ?? -1),
                    primaryInfoData = null
                };
                parsedData.topIcon = GetFrameByRank(parsedData.rank);
                Texture2D flag = GetFlag(countryCode.ToLower());
                if (flag != null)
                    parsedData.avatar = flag;
                parsedData.isLoadingAvatar = false;
                parsedData.destroyAvatarOnDispose = false;
                parsedDataCollection.Add(parsedData);
            }
            else
            {
                long? solveT = item.BaseData.GetLong("SOLVING_TIME");
                TimeSpan timeSpan = TimeSpan.FromSeconds(((float)(solveT ?? 0)) / 1000);
                string timeSpanStr = String.Format("{1:00}:{2:00}:{3:00}", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                int? exp = item.BaseData.GetInt("EXP");
                string primaryInforTextStr = solveT.HasValue ? timeSpanStr : exp.HasValue ? exp.ToString() + " EXP" : "NaN";
                int currentAge = item.BaseData.GetInt("PLAYER_MAX_LEVEL") ?? -1;
                int rank = (int)(item.Rank ?? -1);

                int currentNode = 0;
                for (int i = 0; i < PuzzleManager.Instance.ageList.Count; i++)
                {
                    if (PuzzleManager.Instance.ageList[i] <= currentAge)
                        currentNode = i;
                }

                LeaderboardEntryParsedData parsedData = new LeaderboardEntryParsedData
                {
                    debugId = System.Guid.NewGuid().ToString(),
                    playerId = item.UserId,
                    rank = rank,
                    playerName = item.UserName != "" ? item.UserName : playerDefaultName,
                    secondaryIcon = null,
                    secondaryInfo = "loading",
                    primaryInfo = primaryInforTextStr,
                    hasSecondaryInfo = true,
                    destroyAvatarOnDispose = true,
                    isCurrentPlayerEntry = rank != -1 && rank == (playerRank ?? -1),
                    primaryInfoData = null
                };
                parsedData.topIcon = GetFrameByRank(parsedData.rank);
                if (parsedData.isCurrentPlayerEntry)
                {
                }

                if (string.IsNullOrEmpty(item.UserName) == false)
                {
                    CloudServiceManager.Instance.RequestAvatarForPlayer(item.UserId, (response) =>
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
        }
        return parsedDataCollection;
    }

    private void SetCurrentEntriesLocalData()
    {
        int? currentExp = CurrentGroupType == TournamentDataRequest.LeaderboardGroup.Contries ? 0 : PlayerDb.GetInt("LAST-EXP", 0);
        LeaderboardEntryParsedData currentParsedData = new LeaderboardEntryParsedData
        {
            debugId = System.Guid.NewGuid().ToString(),
            secondaryInfo = CurrentGroupType == TournamentDataRequest.LeaderboardGroup.Contries ? "" : StoryPuzzlesSaver.Instance.MaxNode.ToString(),
            hasSecondaryInfo = CurrentGroupType != TournamentDataRequest.LeaderboardGroup.Contries,
            destroyAvatarOnDispose = true,
            isCurrentPlayerEntry = true,
            primaryInfoData = null
        };
        if (CurrentGroupType == TournamentDataRequest.LeaderboardGroup.Contries)
        {
            if (!string.IsNullOrEmpty(CloudServiceManager.countryName))
            {
                Texture2D flag = GetFlag(CloudServiceManager.countryName.ToLower());
                if (flag != null)
                    currentParsedData.avatar = flag;
            }
            currentParsedData.isLoadingAvatar = false;
            currentParsedData.destroyAvatarOnDispose = false;
        }
        currentEntry.SetData(currentParsedData);
    }

    private void Start()
    {
        LoadTopFrameIcon();
        listView.displayDataAction += OnDisplayDataAction;
        loginButton.onClick.AddListener(delegate
        {
            
        });
        switch (CurrentGroupType)
        {
            case TournamentDataRequest.LeaderboardGroup.Normal:
                subTitle.text = I2.Loc.ScriptLocalization.ALL_PLAYER;
                break;
            case TournamentDataRequest.LeaderboardGroup.Social:
                subTitle.text = I2.Loc.ScriptLocalization.Friends;
                break;
            case TournamentDataRequest.LeaderboardGroup.Contries:
                subTitle.text = I2.Loc.ScriptLocalization.Countries;
                break;
            default:
                break;
        }
        invidualButton.onClick.AddListener(delegate
        {
            CurrentGroupType = TournamentDataRequest.LeaderboardGroup.Normal;
            subTitle.text = I2.Loc.ScriptLocalization.ALL_PLAYER;
        });
        friendButton.onClick.AddListener(delegate
        {
            CurrentGroupType = TournamentDataRequest.LeaderboardGroup.Social;
            subTitle.text = I2.Loc.ScriptLocalization.Friends;
        });
        countryButton.onClick.AddListener(delegate
        {
            CurrentGroupType = TournamentDataRequest.LeaderboardGroup.Contries;
            subTitle.text = I2.Loc.ScriptLocalization.Countries;
        });

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
        UpdatePlayerEntryGroup();
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

        UpdateView();


        if (this.sizeTabButtons.Count == 0)
            return;

        //Change tab
        new SizeSelectionTab(new SizeSelectionTab.Config()
        {
            bgActiveColor = this.bgActiveColor,
            bgInactiveColor = this.bgInactiveColor,
            txtActiveColor = this.txtActiveColor,
            txtInactiveColor = this.txtInactiveColor
        },
        tournamentDetailPanel.challengeIdInPackList.IndexOf(tournamentDetailPanel.currentChallengeId),
        this.sizeTabButtons,
        (index) =>
        {
            //Update leaderboard
            string tournamentId = tournamentDetailPanel.challengeIdInPackList[index];
            Puzzle p = PuzzleManager.Instance.GetChallengeById(tournamentId);
            SetTitle(Takuzu.Utilities.GetLocalizePackNameByLevel(p.level));
            SetChallengeId(tournamentId);

            UpdateView();
        });
    }

    private void UpdateView()
    {
        StartCoroutine(UpdateDataCR());
        UpdatePlayerEntryGroup();
    }

    private void UpdatePlayerEntryGroup()
    {
        playerEntry.SetActive(SocialManager.Instance.IsLoggedInFb);
        loginEntry.SetActive(!SocialManager.Instance.IsLoggedInFb);
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
