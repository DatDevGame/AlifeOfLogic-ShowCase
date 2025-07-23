using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;
using System;
using Takuzu.Generator;
using Pinwheel;
using UnityEngine.SceneManagement;
using System.Globalization;

[RequireComponent(typeof(OverlayGroupController))]
public class TounamentsPanel : MonoBehaviour
{
    public static event System.Action<int> HighlightSubscription = delegate { };
    [Header("UI References")]
    //public List<Image> characterImgs;
    public string CountDownUIGuide;
    public string TournamentUIGuide;
    [HideInInspector]
    public GameObject countDownReferenceObj;
    public GameObject tournamentReferenceObj;
    [HideInInspector]
    public Transform tournamentContainer;
    //public Button homeBtn;
    public OverlayGroupController controller;
    public GameObject dailyTournamentContainer;
    //public ParalaxBg paralaxBg;
    public SnappingScroller dailyScroller;

    public Transform content;
    public float sideUIShowDelay = 0.5f;
    public Vector2 panelSize = new Vector2(550, 230);

    public Color activeColor = new Color(0.3764706f, 0.7450981f, 0.683715f, 1);
    public Color inacitveColor = new Color(0.7843137f, 0.7843137f, 0.7843137f, 1);
    public Color currentSolvedTabBtnColor;
    public Color currentTabBtnColor;
    public Color currentTabTextColor;
    public Color defaultTabBtnColor;
    public Color defaultTabTextColor;

    [HideInInspector]
    public LeaderBoardScreenUI leaderboardUI;

    public bool showOnStart = false;

    public PositionAnimation entries;

    public RectTransform rectTransIndicator;

    public float indicatorAdjustValueY = 50;

    private TournamentDataRequest.ChallengeMode currentChallengeMode = TournamentDataRequest.ChallengeMode.Daily;

    public TournamentDataRequest.ChallengeMode CurrentChallengeMode
    {
        get
        {
            return currentChallengeMode;
        }

        set
        {
            currentChallengeMode = value;
            UpdateTournamentUI();
        }
    }
    private bool sidePanelShowing = false;
    private float lastHide = 0;
    private bool wasSetUpParalax = false;
    private ConfirmationDialog confirmDialog;
    private List<TournamentDetailPanel> tournamentDetailPanelList;

    public static bool IsTournamentScene { get { return SceneManager.GetActiveScene().name == "Tournament"; } }

    private void Start()
    {
#if UNITY_EDITOR
        float screenRate = 1f / Camera.main.aspect;
#else
        float screenRate = (float)Screen.height / Screen.width;
#endif
        if (screenRate >= 2)
        {
            rectTransIndicator.localPosition = new Vector3(rectTransIndicator.localPosition.x,
                rectTransIndicator.localPosition.y + indicatorAdjustValueY, rectTransIndicator.localPosition.z);
        }
        countDownReferenceObj = UIReferences.Instance.gameUiClockUI.gameObject;
        //homeBtn.onClick.AddListener(delegate
        //{
        //    if(SceneManager.GetActiveScene().name == "Tournament")
        //    {
        //        SceneLoadingManager.Instance.LoadMainScene();
        //    }
        //    else
        //    {
        //        HideIfNot();
        //    }
        //});
        UpdateTournamentUI();
        if (PuzzleManager.Instance.challengeIds.Count > 0)
        {
            UpdateButtonEvent();
        }
        PuzzleManager.onChallengeListChanged += OnDailyPuzzleListChanged;
        GameManager.GameStateChanged += OnGameStateChanged;
        dailyScroller.onSnapIndexChanged += OnDailySnapIndexChanged;
        //dailyScroller.onScrollingViewPositionChanged += OnScrollingPositionChanged;
        CloudServiceManager.onPlayerDbSyncEnd += OnPlayerDbSyncEnd;
        if (showOnStart)
            ShowIfNot();



        //private void OnScrollingPositionChanged(float progress)
        //{
        //    paralaxBg.Progress = progress;
        //}
    }

    private void Awake()
    {
        if (UIReferences.Instance != null)
        {
            UpdateReferences();
        }
        UIReferences.UiReferencesUpdated += UpdateReferences;
        tournamentDetailPanelList = new List<TournamentDetailPanel>();
    }

    private void UpdateReferences()
    {
        leaderboardUI = UIReferences.Instance.overlayLeaderBoardScreenUI;
        tournamentContainer = UIReferences.Instance.gameUiTournamentContainer;
        confirmDialog = UIReferences.Instance.overlayConfirmDialog;
    }

    private void OnDestroy()
    {
        PuzzleManager.onChallengeListChanged -= OnDailyPuzzleListChanged;
        GameManager.GameStateChanged -= OnGameStateChanged;
        dailyScroller.onSnapIndexChanged -= OnDailySnapIndexChanged;
        //dailyScroller.onScrollingViewPositionChanged -= OnScrollingPositionChanged;
        UIReferences.UiReferencesUpdated -= UpdateReferences;
        CloudServiceManager.onPlayerDbSyncEnd -= OnPlayerDbSyncEnd;
    }

    private void OnPlayerDbSyncEnd()
    {
        UpdateButtonEvent();
    }

    private void OnGameStateChanged(GameState newState, GameState arg2)
    {
        if (PuzzleManager.Instance.challengeIds.Count > 0)
        {
            UpdateButtonEvent();
        }
        if (!showOnStart)
            return;
        if (newState == GameState.Playing)
        {
            HideIfNot();
        }
        else if (newState == GameState.Prepare)
        {
            CoroutineHelper.Instance.DoActionDelay(
                () =>
                {
                    if (GameManager.Instance.GameState == GameState.Prepare)
                        ShowIfNot();
                },
                0.3f);
        }
    }

    private void OnWeeklySnapIndexChanged(int arg1, int arg2)
    {
        UpdateTournamentUI();
    }

    private void OnDailySnapIndexChanged(int arg1, int arg2)
    {
        UpdateTournamentUI();
    }

    private void OnDailyPuzzleListChanged(List<Puzzle> obj)
    {
        UpdateButtonEvent();
    }

    private void UpdateButtonEvent()
    {
        //Update daily container
        bool error = false;
        tournamentDetailPanelList.Clear();
        for (int level = 0; level < dailyScroller.presetElements.Length; level++)
        {
            List<string> challengeIdList = new List<string>();
            for (int size = 6; size <= 12; size += 2)
            {
                Puzzle puzzle = PuzzleManager.Instance.challengePuzzles.Find(item =>
                    (PuzzleManager.Instance.challengeIds[PuzzleManager.Instance.challengePuzzles.IndexOf(item)].StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX) && (int)item.size == size && (int)item.level == level + 1));
                if (puzzle == null)
                {
                    error = true;
                    break;
                }
                challengeIdList.Add(PuzzleManager.Instance.GetChallengeId(puzzle.puzzle));
            }
            if (error)
                break;

            TournamentDetailPanel tournamentDetailPanel = dailyScroller.presetElements[level].GetComponent<TournamentDetailPanel>();
            tournamentDetailPanelList.Add(tournamentDetailPanel);
            tournamentDetailPanel.sovledCurrentTabBtnColor = currentSolvedTabBtnColor;
            tournamentDetailPanel.defaultTabBtnColor = defaultTabBtnColor;
            tournamentDetailPanel.currentTabBtnColor = currentTabBtnColor;
            tournamentDetailPanel.defaultTabTextColor = defaultTabTextColor;
            tournamentDetailPanel.currentTabTextColor = currentTabTextColor;
            for (int x = 0; x < tournamentDetailPanel.SizeTabList.Length; x++)
            {
                int index = x;
                tournamentDetailPanel.SizeTabList[x].onClick.RemoveAllListeners();
                tournamentDetailPanel.SizeTabList[x].onClick.AddListener(delegate
                {
                    tournamentDetailPanel.SwitchSizeTab(index);
                });
            }
            tournamentDetailPanel.SetupDetailPanel(challengeIdList);
            //tournamentDetailPanel.playButton.interactable = (int)puzzle.level <= (int)StoryPuzzlesSaver.Instance.GetMaxDifficultLevel();
            //tournamentDetailPanel.lockedImg.SetActive((int)puzzle.level > (int)StoryPuzzlesSaver.Instance.GetMaxDifficultLevel());

            //bool solved = PuzzleManager.Instance.IsPuzzleSolved(PuzzleManager.Instance.GetChallengeId(puzzle.puzzle));
            //tournamentDetailPanel.unsolvedIcon.SetActive(!solved);
            //tournamentDetailPanel.solvedIcon.SetActive(solved);

            tournamentDetailPanel.playButton.GetComponent<Image>().color = tournamentDetailPanel.playButton.interactable ? activeColor : inacitveColor;
            tournamentDetailPanel.playButton.onClick.RemoveAllListeners();
            tournamentDetailPanel.playButton.onClick.AddListener(delegate
            {
                tournamentDetailPanel.CheckSubscriptionToSetUI();

                Puzzle p = PuzzleManager.Instance.GetChallengeById(tournamentDetailPanel.currentChallengeId);

                if (PuzzleManager.Instance.timeToEndOfTheDay.TotalMinutes < (CloudServiceManager.Instance.appConfig.GetInt("minutestillendtournament") ?? 30))
                {
                    UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION,
                    String.Format(I2.Loc.ScriptLocalization.TOURNAMENT_ABOUT_TO_END, PuzzleManager.Instance.timeToEndOfTheDay.TotalMinutes), I2.Loc.ScriptLocalization.OK, "", () =>
                        {
                            GameManager.Instance.PlayADailyTournamentPuzzle(tournamentDetailPanel.currentChallengeId, InAppPurchaser.Instance.IsSubscibed());
                        });
                }
                else
                {
                    GameManager.Instance.PlayADailyTournamentPuzzle(tournamentDetailPanel.currentChallengeId, InAppPurchaser.Instance.IsSubscibed());
                }
            });

            tournamentDetailPanel.unlockButton.onClick.AddListener(delegate
            {
                GameManager.Instance.UnlockTournamentChapter(tournamentDetailPanel.currentChallengeId, InAppPurchaser.Instance.IsSubscibed());
            });

            tournamentDetailPanel.lbButton.onClick.RemoveAllListeners();
            tournamentDetailPanel.lbButton.onClick.AddListener(delegate
            {
                //Show lb
                leaderboardUI.SetTournamentDetailPanel(tournamentDetailPanel);
                leaderboardUI.Show();
            });

            tournamentDetailPanel.topPlayerButton.onClick.RemoveAllListeners();
            tournamentDetailPanel.topPlayerButton.onClick.AddListener(delegate
            {
                try
                {
                    Puzzle p = PuzzleManager.Instance.GetChallengeById(tournamentDetailPanel.currentChallengeId);
                    UIReferences.Instance.topPlayePanel.SetLbLevel((int)p.level);
                    UIReferences.Instance.topPlayePanel.Show();
                }
                catch (NullReferenceException ex)
                {
                    Debug.LogWarning(ex);
                }
            });
        }
    }

    private void UpdateTournamentUI()
    {
        int dailySnapIndex = Mathf.Clamp(dailyScroller.SnapIndex, 0, Mathf.Max(dailyScroller.ElementCount - 1, 0));
        TournamentDetailPanel dailyTournamentDetailPanel = dailyScroller.presetElements[dailySnapIndex].GetComponent<TournamentDetailPanel>();
    }

    private bool snapedToHighestTournament = false;

    internal void ShowIfNot()
    {
        if (content.parent != tournamentContainer.transform)
        {
            /*
            CoroutineHelper.Instance.PostponeActionUntil(() =>
            {
                if (countDownReferenceObj)
                {
                    List<Image> countDownMaskedImage = new List<Image>();
                    countDownMaskedImage.AddRange(countDownReferenceObj.GetComponentsInChildren<Image>().ToList());
                    countDownMaskedImage.Add(countDownReferenceObj.GetComponent<Image>());
                    UIGuide.UIGuideInformation countDownUIGuideInformation = new UIGuide.UIGuideInformation(CountDownUIGuide, countDownMaskedImage, countDownReferenceObj, countDownReferenceObj, GameState.Prepare);
                    countDownUIGuideInformation.message = "Here's the remaining time of today Tournament.";
                    UIGuide.instance.HighLightThis(countDownUIGuideInformation);
                }
                if (tournamentReferenceObj)
                {
                    List<Image> tournamentMaskedImage = new List<Image>();
                    tournamentMaskedImage.AddRange(tournamentReferenceObj.GetComponentsInChildren<Image>().ToList());
                    tournamentMaskedImage.Add(tournamentReferenceObj.GetComponent<Image>());
                    UIGuide.UIGuideInformation tournamentUIGuideInformation = new UIGuide.UIGuideInformation(TournamentUIGuide, tournamentMaskedImage, tournamentReferenceObj, tournamentReferenceObj, GameState.Prepare);
                    tournamentUIGuideInformation.message = "Here you can start playing a challenge of the Daily Tournament\nSwipe right for more.";
                    UIGuide.instance.HighLightThis(tournamentUIGuideInformation);
                }
            }, () => GameManager.Instance.GameState.Equals(GameState.Prepare));
            */
            controller.ShowIfNot();
            controller.isShowing = true;
            content.transform.SetParent(tournamentContainer.transform);
            //homeBtn.transform.parent.SetParent(tournamentContainer.transform);

            (content.transform as RectTransform).anchoredPosition = Vector2.zero;
            (content.transform as RectTransform).sizeDelta = new Vector2(0, 0);
            content.SetAsFirstSibling();
            content.gameObject.SetActive(true);

            entries.Play(entries.curves[0]);

            if (!snapedToHighestTournament)
            {
                dailyScroller.SnapIndex = (int)StoryPuzzlesSaver.Instance.GetMaxDifficultLevel() - 1;
                dailyScroller.Snap();
                snapedToHighestTournament = true;
            }
        }
    }

    internal void HideIfNot()
    {
        if (content.parent == tournamentContainer.transform)
        {
            controller.HideIfNot();
            controller.isShowing = false;
            content.transform.SetParent(transform);
            //homeBtn.transform.parent.SetParent(transform);
            (content.transform as RectTransform).anchoredPosition = Vector2.zero;
            entries.Play(entries.curves[1]);
            StartCoroutine(DelayInactive());
        }
    }

    private IEnumerator DelayInactive()
    {
        yield return new WaitForSeconds(Mathf.Max(new float[] { entries.duration }));
        content.gameObject.SetActive(false);
    }
}
