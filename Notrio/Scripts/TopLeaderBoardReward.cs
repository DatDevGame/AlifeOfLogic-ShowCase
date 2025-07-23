using System.Collections.Generic;
using UnityEngine;
using Takuzu.Generator;
using Takuzu;
using GameSparks.Api.Responses;
using System;
using GameSparks.Core;

public class TopLeaderBoardReward : MonoBehaviour {
    public static TopLeaderBoardReward Instance;
    [SerializeField]
    public List<RewardInformation> rewards = new List<RewardInformation>();
    public static System.Action<List<RewardInformation>> topChallengeRewardListChanged = delegate { };
    private static string leaderBoardLevelScriptDataKey = "LEVEL";
    private static int topRanksForReward = 10;
    private static string topChallengeRewardPrefix = "TOP_CHALLENGE_REWARD";
    private static string topChallengeRewardSetup = "TOP_CHALLENGE_REWARD_SETUP";
    public static string DAILY_LB_PREFIX = "LB_DAILY_LV";
    public static string WEEKLY_LB_PREFIX = "LB_WEEKLY_LV";
   
    public static bool rewardReceived = false;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            DestroyImmediate(gameObject);
        PuzzleManager.onChallengeListChanged += OnChallengeListChanged;
    }

    private void OnDestroy()
    {
        PuzzleManager.onChallengeListChanged += OnChallengeListChanged;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnChallengeListChanged(List<Puzzle> ps)
    {
        RequestReward();
    }

    private void OnRewardRequestResponse(LogEventResponse obj)
    {
        if (obj.ScriptData == null)
            return;
        Instance.rewards.Clear();
        List<GSData> rewardList = obj.ScriptData.GetGSDataList("rewards");
        if (rewardList != null)
        {
            foreach (var item in rewardList)
            {
                Debug.Log("Add rewards");
                int? rewardAmount = item.GetInt("coinAmount");
                if (!rewardAmount.HasValue)
                    continue;

                if (item.GetString("rewardId").StartsWith(DAILY_LB_PREFIX) || item.GetString("rewardId").StartsWith(WEEKLY_LB_PREFIX))
                {
                    Instance.rewards.Add(new RewardInformation
                    {
                        rewardId = item.GetString("rewardId"),
                        coinAmount = rewardAmount.Value,
                        uiTitle = item.GetString("uiTitle"),
                        uiBody = item.GetString("uiBody"),
                        rewardName = item.GetString("rewardName"),
                        summary = item.GetString("summary"),
                        bgName = item.GetString("bgName"),
                        rankDescription = item.GetString("rankDescription"),
                        challengeName = item.GetString("challengeName"),
                        accentColor = item.GetString("accentColor"),
                        openDetailPanel = (bool)item.GetBoolean("openDetailPanel"),
                        date = item.GetString("date")
                    });
                }
                else {
                    Instance.rewards.Add(new RewardInformation
                    {
                        rewardId = item.GetString("rewardId"),
                        coinAmount = rewardAmount.Value,
                        uiTitle = item.GetString("uiTitle"),
                        uiBody = item.GetString("uiBody"),
                        rewardName = item.GetString("rewardName"),
                        summary = item.GetString("summary"),
                        bgName = item.GetString("bgName"),
                        accentColor = item.GetString("accentColor"),
                        openDetailPanel = (bool)item.GetBoolean("openDetailPanel"),
                        date = item.GetString("date")
                    });
                }
            }
        }
        rewardReceived = true;
        topChallengeRewardListChanged(Instance.rewards);
    }

    internal static void RefreshRewards()
    {
        Instance.RequestReward();
    }

    private void RequestReward()
    {
        rewardReceived = false;
        CloudServiceManager.Instance.RequestReward(OnRewardRequestResponse);
    }

    public static string getTopChallengeRewardCode(string prefix, int level)
    {
        return topChallengeRewardPrefix + "_" + prefix + ""+ level;
    }

    public static string getTopChallengeRewardSetupCode(string prefix, int level)
    {
        return topChallengeRewardSetup + "_" + prefix + "" + level;
    }

    public static void getReward(RewardInformation rewardInformation, Action<LogEventResponse> callback)
    {
        CloudServiceManager.Instance.ClaimReward(rewardInformation.rewardId, callback);
    }
}

[Serializable]
public struct RewardInformation
{
    public string rewardId;
    public int coinAmount;
    public string uiTitle;
    public string uiBody;
    public string rewardName;
    public string summary;
    public string bgName;
    public string rankDescription;
    public string challengeName;
    public string accentColor;
    public bool openDetailPanel;
    public string date;
}
