using System;
using System.Collections;
using System.Collections.Generic;
using GameSparks.Api.Responses;
using GameSparks.Core;
using Takuzu;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;
using Takuzu.Generator;
using EasyMobile;

public class DailyWeeklyChallengePanelUI : MonoBehaviour
{
    public Action ChallengeStateChanged = delegate { };
    [Header("UI Preferences")]
    [Header("Panel")]
    public Image bgImg;
    public Sprite iosPanelSprite;
    public Sprite androidPanelSprite;
    [Header("Tournament")]
    public GameObject tournamentContainer;
    public Text challengeNameText;
    public Text timeLeftText;
    public Text numberOfPlayersText;
    public Button tournamentBtn;
    [Header("Tournament On Android")]
    public GameObject tournamentContainer_Android;
    public Text challengeNameText_Android;
    public Text timeLeftText_Android;
    public Text numberOfPlayersText_Android;
    public Button tournamentBtn_Android;

    [Header("Multiplayer")]
    public GameObject multiplayerContainer;
    public Text statisticsText;
    public Text multiplayerNameText;
    public Button multiplayerBtn;

    [HideInInspector]
    public TounamentsPanel tounamentsPanel;
    public PositionAnimation positionAnimation;
    public bool openTournamentScene = false;
    public string TournamentSceneName = "Tournament";

    public string weeklyChallengeString;
    public string dailyChallengeString
    {
        get
        {
            return I2.Loc.ScriptLocalization.Daily_Tournament.ToUpper();
        }
    }
    public string UIGuideSaveKey;
    public static string UIGuideNewTournamentAvailableSaveKeyPrefix = "Daily_Tournament_New_Tournament_Available";
    public static string UIGuideMultiplayerAvailableSaveKey = "Multiplayer_Available";

    [HideInInspector]
    public bool isShown = true;
    private TournamentDataRequest.ChallengeMode currentChallengeState = TournamentDataRequest.ChallengeMode.Daily;

    public TournamentDataRequest.ChallengeMode CurrentChallengeState
    {
        get
        {
            return currentChallengeState;
        }

        set
        {
            currentChallengeState = value;
            ChallengeStateChanged();
        }
    }
    private void Awake()
    {
        if (UIReferences.Instance != null)
        {
            UpdateReferences();
        }

        // hide multiplayer button on Android
        bool isAndroid = Application.platform == RuntimePlatform.Android;
        tournamentContainer_Android.SetActive(isAndroid);
        tournamentContainer.SetActive(!isAndroid);
        multiplayerContainer.SetActive(!isAndroid);
        bgImg.sprite = isAndroid ? androidPanelSprite : iosPanelSprite;
        UIReferences.UiReferencesUpdated += UpdateReferences;
    }

    private void UpdateTournamentActiveState(int currentNode)
    {
        bool active = currentNode >= 0;
        gameObject.SetActive(active);
        bool isAndroid = Application.platform == RuntimePlatform.Android;
        if (active)
        {
            if (!isAndroid)
            {
                CoroutineHelper.Instance.PostponeActionUntil(() =>
                {
                    CoroutineHelper.Instance.DoActionDelay(() =>
                    {
                        if (gameObject && !UIGuide.instance.IsGuildeShown(UIGuideMultiplayerAvailableSaveKey))
                        {
                            List<Image> maskedObject = StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].GetComponents<Image>().ToList();
                            List<Image> tournamentPanelMaskedImage = maskedObject;
                            tournamentPanelMaskedImage.Add(multiplayerBtn.GetComponent<Image>());
                            UIGuide.UIGuideInformation tournamentPanelUIGuideInformation = new UIGuide.UIGuideInformation(UIGuideMultiplayerAvailableSaveKey, tournamentPanelMaskedImage, StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject, multiplayerBtn.gameObject, GameState.Prepare)
                            {
                                message = I2.Loc.ScriptLocalization.UIGUI_MULTIPLAYER,
                                clickableButton = multiplayerBtn,
                                bubleTextWidth = 440
                            };
                            Vector3[] worldConners = new Vector3[4];
                            StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].rectTransform.GetWorldCorners(worldConners);
                            tournamentPanelUIGuideInformation.transformOffset = new Vector3(0, (worldConners[1].y - worldConners[0].y) * 0.8f, 0);
                            UIGuide.instance.HighLightThis(tournamentPanelUIGuideInformation);
                        }
                    }, 0.4f);
                }, () => GameManager.Instance.GameState.Equals(GameState.Prepare));
            }

            CoroutineHelper.Instance.PostponeActionUntil(() =>
            {
                CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    if (gameObject && !UIGuide.instance.IsGuildeShown(UIGuideSaveKey))
                    {
                        StoryLevelContainer.instance.scroller.SnapIndex = 0;
                        StoryLevelContainer.instance.scroller.Snap();
                        List<Image> maskedObject = StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].GetComponents<Image>().ToList();

                        List<Image> tournamentPanelMaskedImage = maskedObject;
                        //tournamentPanelMaskedImage.AddRange(gameObject.GetComponentsInChildren<Image>().ToList());
                        tournamentPanelMaskedImage.Add(isAndroid ? tournamentBtn_Android.GetComponent<Image>() : tournamentBtn.GetComponent<Image>());
                        UIGuide.UIGuideInformation tournamentPanelUIGuideInformation = new UIGuide.UIGuideInformation(UIGuideSaveKey, tournamentPanelMaskedImage, StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject, isAndroid ? tournamentBtn_Android.gameObject : tournamentBtn.gameObject, GameState.Prepare)
                        {
                            message = I2.Loc.ScriptLocalization.UIGUIDE_TOURNAMENT_PANEL,
                            clickableButton = isAndroid ? tournamentBtn_Android : tournamentBtn,
                            bubleTextWidth = 440
                        };

                        Vector3[] worldConners = new Vector3[4];
                        StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].rectTransform.GetWorldCorners(worldConners);
                        tournamentPanelUIGuideInformation.transformOffset = new Vector3(0, (worldConners[1].y - worldConners[0].y) * 0.8f, 0);

                        UIGuide.instance.HighLightThis(tournamentPanelUIGuideInformation);
                    }
                }, 0.25f);
            }, () => GameManager.Instance.GameState.Equals(GameState.Prepare));
        }
    }

    private void UpdateReferences()
    {
        tounamentsPanel = UIReferences.Instance.overlayTournamentsPanel;
    }

    private void Start()
    {
        UpdateTournamentActiveState(StoryPuzzlesSaver.Instance.MaxNode);
        PuzzleManager.onChallengeListChanged += OnChallengeListChagned;
        ChallengeStateChanged += OnChallengeStateChanged;
        GameManager.GameStateChanged += OnGameStateChanged;
        CloudServiceManager.onPlayerDbSyncEnd += OnPlayerDBSyncEnd;
        PlayerDb.Resetted += OnPlayerDBSyncEnd;
        StoryPuzzlesSaver.NewMaxDifficultAchieved += OnMaxDifficultyAchieved;
        InitPanel();
    }

    private void OnDestroy()
    {
        PuzzleManager.onChallengeListChanged -= OnChallengeListChagned;
        ChallengeStateChanged -= OnChallengeStateChanged;
        GameManager.GameStateChanged -= OnGameStateChanged;
        UIReferences.UiReferencesUpdated -= UpdateReferences;
        CloudServiceManager.onPlayerDbSyncEnd -= OnPlayerDBSyncEnd;
        PlayerDb.Resetted -= OnPlayerDBSyncEnd;
        StoryPuzzlesSaver.NewMaxDifficultAchieved -= OnMaxDifficultyAchieved;
    }

    private void OnMaxDifficultyAchieved(Level level)
    {
        if (level == Level.Easy)
            return;

        //CoroutineHelper.Instance.PostponeActionUntil(() =>
        //{
        //    CoroutineHelper.Instance.DoActionDelay(() => {
        //        string saveKey = string.Format("{0}{1}", UIGuideNewTournamentAvailableSaveKeyPrefix, level);
        //        if (gameObject && !UIGuide.instance.IsGuildeShown(saveKey))
        //        {
        //            List<Image> maskedObject = StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].GetComponents<Image>().ToList();

        //            List<Image> tournamentPanelMaskedImage = maskedObject;
        //            //tournamentPanelMaskedImage.AddRange(gameObject.GetComponentsInChildren<Image>().ToList());
        //            tournamentPanelMaskedImage.Add(tournamentBtn.GetComponent<Image>());
        //            UIGuide.UIGuideInformation tournamentPanelUIGuideInformation = new UIGuide.UIGuideInformation(saveKey, tournamentPanelMaskedImage, StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject, tournamentBtn.gameObject, GameState.Prepare)
        //            {
        //                message = I2.Loc.ScriptLocalization.UIGUIDE_NEW_TOURNAMENT_UNLOCKED,
        //                clickableButton = tournamentBtn,
        //                bubleTextWidth = 440
        //            };

        //            Vector3[] worldConners = new Vector3[4];
        //            StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].rectTransform.GetWorldCorners(worldConners);
        //            tournamentPanelUIGuideInformation.transformOffset = new Vector3(0, (worldConners[1].y - worldConners[0].y) * 0.6f, 0);

        //            UIGuide.instance.HighLightThis(tournamentPanelUIGuideInformation);
        //        }
        //    }, 1.5f);
        //}, () => GameManager.Instance.GameState.Equals(GameState.Prepare));
    }

    private void OnPlayerDBSyncEnd()
    {
        if (GameManager.Instance.GameState == GameState.Prepare)
        {
            UpdateTournamentActiveState(StoryPuzzlesSaver.Instance.MaxNode);
            if (positionAnimation.gameObject.activeSelf)
                positionAnimation.Play(positionAnimation.curves[0]);
            statisticsText.text = String.Format("{0} {1}   {2} {3}", I2.Loc.ScriptLocalization.WIN, PlayerInfoManager.Instance.winNumber, I2.Loc.ScriptLocalization.LOSE, PlayerInfoManager.Instance.loseNumber);
        }
    }

    private void OnChallengeListChagned(List<Puzzle> obj)
    {
        UpdateTextInfor();
    }

    private void OnGameStateChanged(GameState arg1, GameState arg2)
    {
        if (arg1.Equals(GameState.Playing))
            HideIfNot();
        else if (arg1.Equals(GameState.Prepare))
        {
            UpdateTournamentActiveState(StoryPuzzlesSaver.Instance.MaxNode);
            ShowIfNot();
        }
    }

    private void ShowIfNot()
    {
        if (!isShown && gameObject.activeInHierarchy)
            positionAnimation.Play(positionAnimation.curves[0]);
        isShown = true;
        tournamentBtn_Android.interactable = tournamentBtn.interactable = true;
    }

    private void HideIfNot()
    {
        if (isShown && gameObject.activeInHierarchy)
            positionAnimation.Play(positionAnimation.curves[1]);
        isShown = false;
        tournamentBtn_Android.interactable = tournamentBtn.interactable = false;
    }
    private void Update()
    {
        UpdateTimeLeft();
    }

    private void UpdateTimeLeft()
    {
        TimeSpan timeSpanLeft = CurrentChallengeState == TournamentDataRequest.ChallengeMode.Daily ? PuzzleManager.Instance.timeToEndOfTheDay : PuzzleManager.Instance.timeToEndOfTheWeek;
        if (Application.internetReachability != NetworkReachability.NotReachable && PuzzleManager.Instance.challengeIds != null && PuzzleManager.Instance.challengeIds.Count > 0)
            timeLeftText_Android.text = timeLeftText.text = String.Format("{0:00}:{1:00}:{2:00}", timeSpanLeft.Hours, timeSpanLeft.Minutes, timeSpanLeft.Seconds);
        else
        {
            timeLeftText_Android.text = timeLeftText.text = "--:--:--:--";
            numberOfPlayersText_Android.text = numberOfPlayersText.text = "?";
        }
    }

    private void OnChallengeStateChanged()
    {
        UpdateTextInfor();
    }

    private void UpdateTextInfor()
    {
        challengeNameText_Android.text = challengeNameText.text = CurrentChallengeState == TournamentDataRequest.ChallengeMode.Daily ? dailyChallengeString :
            CurrentChallengeState == TournamentDataRequest.ChallengeMode.Weekly ? weeklyChallengeString : "?";
        TournamentDataRequest.RequestTournamentPlayerCount(TournamentDataRequest.LeadboardPlayerCountType.DailyWeeklyCount, (response) =>
        {
            if (response == null)
                return;
            if (!this)
                return;
            int? numberOfDailyPlayers = response.GetInt(TournamentDataRequest.DAILY_KEY);
            int? numberOfWeeklyPlayers = response.GetInt(TournamentDataRequest.WEEKLY_KEY);
            numberOfPlayersText_Android.text = numberOfPlayersText.text = CurrentChallengeState == TournamentDataRequest.ChallengeMode.Daily ? (String.IsNullOrEmpty(numberOfDailyPlayers.ToString()) ? "?" : numberOfDailyPlayers.ToString()) :
                CurrentChallengeState == TournamentDataRequest.ChallengeMode.Weekly ? (String.IsNullOrEmpty(numberOfWeeklyPlayers.ToString()) ? "?" : numberOfWeeklyPlayers.ToString()) : "?";
        });

        statisticsText.text = String.Format("{0} {1}   {2} {3}", I2.Loc.ScriptLocalization.WIN, PlayerInfoManager.Instance.winNumber, I2.Loc.ScriptLocalization.LOSE, PlayerInfoManager.Instance.loseNumber);
        multiplayerNameText.text = I2.Loc.ScriptLocalization.MULTIPLAYER.ToUpper();
    }

    private void InitPanel()
    {
        tournamentBtn.onClick.AddListener(delegate
        {
            tounamentsPanel.CurrentChallengeMode = CurrentChallengeState;
            if (openTournamentScene)
            {
                if (Application.internetReachability != NetworkReachability.NotReachable && PuzzleManager.Instance.challengeIds != null && PuzzleManager.Instance.challengeIds.Count > 0)
                    SceneLoadingManager.Instance.LoadTournamentScene();
                else
                    InGameNotificationPopup.Instance.confirmationDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), I2.Loc.ScriptLocalization.TOURNAMENT_CONNECTION_ERROR, I2.Loc.ScriptLocalization.OK, "", () => { });
            }
            else
            {
                tounamentsPanel.ShowIfNot();
            }
        });

        tournamentBtn_Android.onClick.AddListener(delegate
        {
            tounamentsPanel.CurrentChallengeMode = CurrentChallengeState;
            if (openTournamentScene)
            {
                if (Application.internetReachability != NetworkReachability.NotReachable && PuzzleManager.Instance.challengeIds != null && PuzzleManager.Instance.challengeIds.Count > 0)
                    SceneLoadingManager.Instance.LoadTournamentScene();
                else
                    InGameNotificationPopup.Instance.confirmationDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), I2.Loc.ScriptLocalization.TOURNAMENT_CONNECTION_ERROR, I2.Loc.ScriptLocalization.OK, "", () => { });
            }
            else
            {
                tounamentsPanel.ShowIfNot();
            }
        });

        multiplayerBtn.onClick.AddListener(delegate
        {
            if (openTournamentScene)
            {
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    if (Application.internetReachability != NetworkReachability.NotReachable)
                    {
#if !UNITY_EDITOR
                        if (GameServices.IsInitialized())
                        {
                            SceneLoadingManager.Instance.LoadMultiPlayerScene();
                        }
                        else
                        {
#if UNITY_IOS
                            var alert = NativeUI.Alert(I2.Loc.ScriptLocalization.NO_GAME_CENTER_TITLE, I2.Loc.ScriptLocalization.NO_GAME_CENTER_MSG, I2.Loc.ScriptLocalization.CANCEL);
#else
                            GameServices.Init();
#endif
                        }
#else
                        SceneLoadingManager.Instance.LoadMultiPlayerScene();
#endif
                    }
                }
                else
                    InGameNotificationPopup.Instance.confirmationDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), I2.Loc.ScriptLocalization.NO_INTERNET_CONNECTION, I2.Loc.ScriptLocalization.OK, "", () => { });
            }
            else
            {
                tounamentsPanel.ShowIfNot();
            }
        });
        UpdateTextInfor();
    }
}
