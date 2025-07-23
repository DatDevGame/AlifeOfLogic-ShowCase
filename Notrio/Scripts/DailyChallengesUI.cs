using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;
using GameSparks.Api.Responses;
using GameSparks.Core;
using System;
using Takuzu.Generator;

public class DailyChallengesUI : MonoBehaviour {

    [Header("DailyChallengeUIState")]
    private DailyChallenges.ChallengeMode currentChallengeMode = DailyChallenges.ChallengeMode.Daily;
    public Action<DailyChallenges.ChallengeMode, DailyChallenges.ChallengeMode> ChallengeModeChange = delegate { };

    [Header("UI References")]
    public GameObject PuzzleSolvedIcon;
    public Button dailyChallengeTab;
    public Button weeklyChallengeTab;
    public Button playButton;
    public Button closeButton;

    public Transform weeklyChallengeContainer;
    public Transform dailyChallengeContainer;
    public SnappingScroller weeklyScroller;
    public SnappingScroller dailyScroller;

    public OverlayGroupController controller;

    [Header("Tab Button Color")]
    public Image dailyButtonBg;
    public Image weeklyButtonBg;
    public Image dailyButtonIcon;
    public Image weekylyButtonIcon;
    public Color iconActiveColor;
    public Color iconInActiveColor;
    public Color bgActiveColor;
    public Color bgInactiveColor;


    public DailyChallenges.ChallengeMode CurrentChallengeMode
    {
        get
        {
            return currentChallengeMode;
        }

        set
        {
            DailyChallenges.ChallengeMode oldChallenge = currentChallengeMode;
            currentChallengeMode = value;
            ChallengeModeChange(currentChallengeMode, oldChallenge);
        }
    }

    private void Start()
    {
        DailyChallenges.RetriveAllLBData += OnRetriveAllChallengesLeaderBoards;
        ChallengeModeChange += OnChallengeModeChanged;
        dailyChallengeTab.onClick.RemoveAllListeners();
        dailyChallengeTab.onClick.AddListener(delegate
        {
            CurrentChallengeMode = DailyChallenges.ChallengeMode.Daily;
        });
        weeklyChallengeTab.onClick.RemoveAllListeners();
        weeklyChallengeTab.onClick.AddListener(delegate
        {
            CurrentChallengeMode = DailyChallenges.ChallengeMode.Weekly;
        });
        closeButton.onClick.AddListener(delegate
        {
            controller.HideIfNot();
        });
        CurrentChallengeMode = DailyChallenges.ChallengeMode.Daily;
        dailyScroller.onSnapIndexChanged += OnScrollerIndexChanged;
        weeklyScroller.onSnapIndexChanged += OnScrollerIndexChanged;
    }

    private void OnDestroy()
    {
        DailyChallenges.RetriveAllLBData -= OnRetriveAllChallengesLeaderBoards;
        ChallengeModeChange -= OnChallengeModeChanged;
        dailyScroller.onSnapIndexChanged -= OnScrollerIndexChanged;
        weeklyScroller.onSnapIndexChanged -= OnScrollerIndexChanged;
    }

    private void OnScrollerIndexChanged(int arg1, int arg2)
    {
        UpdateDailyChallengePanelUI();
    }

    private void OnChallengeModeChanged(DailyChallenges.ChallengeMode newChallengeNode, DailyChallenges.ChallengeMode oldChallengeMode)
    {
        UpdateDailyChallengePanelUI();
    }

    private void UpdateDailyChallengePanelUI()
    {
        StopCoroutine("WaitForDailyChallengeData");
        StartCoroutine(WaitForDailyChallengeData());
    }

    private IEnumerator WaitForDailyChallengeData()
    {
        if (!DailyChallenges.gotAllResponses)
            yield return new WaitUntil(() => { return DailyChallenges.gotAllResponses; });
        InitAllChallengePanel();

        Puzzle currentPuzzle = GetCurrentPuzzle();
        if (CurrentChallengeMode == DailyChallenges.ChallengeMode.Daily)
        {
            dailyScroller.gameObject.SetActive(true);
            weeklyScroller.gameObject.SetActive(false);

            dailyButtonBg.color = bgActiveColor;
            weeklyButtonBg.color = bgInactiveColor;
            dailyButtonIcon.color = iconActiveColor;
            weekylyButtonIcon.color = iconInActiveColor;
        }
        else if (CurrentChallengeMode == DailyChallenges.ChallengeMode.Weekly)
        {
            dailyScroller.gameObject.SetActive(false);
            weeklyScroller.gameObject.SetActive(true);

            dailyButtonBg.color = bgInactiveColor;
            weeklyButtonBg.color = bgActiveColor;
            dailyButtonIcon.color = iconInActiveColor;
            weekylyButtonIcon.color = iconActiveColor;
        }
        if (currentPuzzle != null)
        {
            string currentPuzzleId = PuzzleManager.Instance.GetChallengeId(currentPuzzle.puzzle);
            UpdatePuzzleSolvedIcon(currentPuzzleId);
            UpdatePlayButtonListener(currentPuzzleId, currentPuzzle);
        }
        yield return null;
    }

    private void InitAllChallengePanel()
    {
        foreach (var lbShortCode in DailyChallenges.instance.allChallengesLeaderBoardEntryDictionary.Keys)
        {
            Transform container = null;
            string prefix = "";
            if (lbShortCode.StartsWith(DailyChallenges.DAILY_LB))
            {
                container = dailyChallengeContainer;
                prefix = DailyChallenges.DAILY_LB;
            }
            else if (lbShortCode.StartsWith(DailyChallenges.WEEKLY_LB))
            {
                container = weeklyChallengeContainer;
                prefix = DailyChallenges.WEEKLY_LB;
            }
            if (container == null)
                break;
            GameObject go = container.GetChild(int.Parse(lbShortCode.Replace(prefix,"")) - 1).gameObject;
            if (go == null)
                break;

            GSEnumerable<LeaderboardDataResponse._LeaderboardData> leaderboardDatas;
            if (!DailyChallenges.instance.allChallengesLeaderBoardEntryDictionary.TryGetValue(lbShortCode, out leaderboardDatas))
                break;

            ChallengePanelVer2 challengePanel = go.GetComponent<ChallengePanelVer2>();
            challengePanel.SettupLeaderBoard(leaderboardDatas);

            PuzzleSolvedIcon.SetActive(PuzzleManager.Instance.IsPuzzleSolved(DailyChallenges.instance.GetPuzzleIdFromLBShortCode(lbShortCode)));
        }
    }

    private Puzzle GetCurrentPuzzle()
    {
        Puzzle currentChallenge = null;
        foreach (var lbShortCode in DailyChallenges.instance.allChallengesLeaderBoardEntryDictionary.Keys)
        {
            if(lbShortCode.StartsWith(DailyChallenges.DAILY_LB) && CurrentChallengeMode == DailyChallenges.ChallengeMode.Daily)
            {
                int scrollerIndex = Mathf.Max(dailyScroller.SnapIndex, 0)%dailyScroller.ElementCount;
                if ((int)DailyChallenges.instance.GetLBLevel(lbShortCode) - 1 == scrollerIndex)
                {
                    return currentChallenge = PuzzleManager.Instance.GetChallengeById(DailyChallenges.instance.GetPuzzleIdFromLBShortCode(lbShortCode));
                }
            }
            else if (lbShortCode.StartsWith(DailyChallenges.WEEKLY_LB) && CurrentChallengeMode == DailyChallenges.ChallengeMode.Weekly)
            {
                int scrollerIndex = Mathf.Max(weeklyScroller.SnapIndex, 0)%weeklyScroller.ElementCount;
                if ((int) DailyChallenges.instance.GetLBLevel(lbShortCode) - 1 == scrollerIndex)
                {
                    return currentChallenge = PuzzleManager.Instance.GetChallengeById(DailyChallenges.instance.GetPuzzleIdFromLBShortCode(lbShortCode));
                }
            }
        }
        return currentChallenge;
    }

    private void UpdatePlayButtonListener(string puzzleId, Puzzle puzzle)
    {
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(delegate
        {
            if (puzzle.level <= StoryPuzzlesSaver.Instance.GetMaxDifficultLevel())
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.button);
                controller.HideIfNot();
                GameManager.Instance.PlayAPuzzle(puzzleId);
            }
            else
            {
                Debug.Log("Your level is not high enough for this challenge");
            }
        });
        
    }

    private void UpdatePuzzleSolvedIcon(string puzzleId)
    {
        PuzzleSolvedIcon.SetActive(PuzzleManager.Instance.IsPuzzleSolved(puzzleId));
    }

    private void OnRetriveAllChallengesLeaderBoards()
    {
        UpdateDailyChallengePanelUI();
    }
}
