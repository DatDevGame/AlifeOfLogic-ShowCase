using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;
using EasyMobile;
using System;
using System.Globalization;
using UnityEngine.SceneManagement;
using LionStudios.Suite.Analytics;
using Takuzu.Generator;
using static StoryPuzzlesSaver;

namespace Takuzu
{
    public class WinMenu : OverlayPanel
    {
        public Text ribbonText;
        public string UIGuideControllKey = "StoryMode";
        public ClockController clockController;
        public UiGroupController controller;
        public CustomProgressBar progressBar;
        public GameObject solvedIcon;
        public Image solvedImgIcon;
        public GameObject currentIcon;
        public Color currentColor;
        public Color solvedColor;
        public GameObject container;
        public Text playTimeText;
        public Text expText;
        public Text coinText;
        public RectTransform infoGroup;
        public Text infoText1;
        public Text infoText2;
        public Text infoText3;
        public Slider expSlider;
        public Image rankIcon;
        public Button shareButton;
        public Button homeButton;
        public Image homeIcon;
        public Text homeText;
        public Button nextButton;
        public Button multiplayerModeAgainBtn;
        public Text againText;
        public Animator againTxtAnimator;
        public Button waitingPlayerButton;
        public CanvasGroup buttonGroup;
        public GameObject dateGroup;
        public GameObject levelProgressGroup;
        public Text dateText;
        public Text ageText;
        public Text betCoinTxt;
        public Text nextPuzzleCostText;
        public Text rematchEnergyCost;
        public GameObject betCoinContainer;
        [HideInInspector]
        public ConfirmationDialog dialog;
        [HideInInspector]
        public CameraController camController;

        public GameObject screenShotCover;
        public DayNightAdapter[] dayNightAdapter;
        [HideInInspector]
        public RewardDetailPanel rewardDetailPanel;
        private PlayUI playUI;
        public GameObject levelInfoContainer;

        [Header("Multiplayer Result")]
        public GameObject multiplayerInfoContainer;
        public RawImage userAvatar;
        public RawImage opponentAvatar;
        public Text userName;
        public Text opponenName;
        public Text userLevel;
        public Text opponentLevel;
        public Text multiplayerResult;

#pragma warning disable IDE1006 // Naming Styles
        public Color color { get; set; }
        //dont rename, it is to use with DayLightAdapter
#pragma warning restore IDE1006 // Naming Styles

        [Header("Replay")]
        public RawImage boardScreenshot;
        public ClipPlayerUI clipPlayer;
        public RawImage clipPlayerRaw;
        public float gifRecordingDelay;
        public GameObject screenshotButtonGroup;
        public Button pngButton;
        public Image pngButtonBackground;
        public GameObject pngGroup;
        public Button gifButton;
        public Image gifButtonBackground;
        public GameObject gifGroup;
        public Color buttonActiveColor;
        public Color buttonInactiveColor;
        [HideInInspector]
        public TaskPanel taskPanel;
        private AnimatedClip clip;
        private string gifUrl;
        private string gifPath;

        [Header("On puzzle solved")]
        public float showDelayOnPuzzleSolved;
        public PositionAnimation ribbonAnim;

        private Judger.JudgingResult stat;
        private bool statAnimDone;
        private bool ribbonHidden = true;
        private bool countDownForScreeenShotIsDone = false;
        private bool isOpponnentDisconnected = false;
        private bool isPlayerDisconnected = false;

        private RenderTexture screenShotRT;
        private Texture2D convertedScreenShot;

        public override void Show()
        {
            if (PuzzleManager.currentIsMultiMode)
            {
                multiplayerInfoContainer.SetActive(true);
                clockController.gameObject.SetActive(false);
                levelInfoContainer.gameObject.SetActive(false);
                SetMultiplayerResult();
            }
            else
            {
                multiplayerInfoContainer.SetActive(false);
                clockController.gameObject.SetActive(true);
                levelInfoContainer.gameObject.SetActive(true);
            }
            screenshotButtonGroup.SetActive(!PuzzleManager.currentIsChallenge && !PuzzleManager.currentIsMultiMode);
            //Checking if this is multiplayer mode then wait for the result
            //Display winning losing
            againTxtAnimator.enabled = false;
            againText.color = Color.white;
            clockController.ResetTimeUI();
            int nodeIndex = StoryPuzzlesSaver.GetIndexNode(PuzzleManager.currentLevel, PuzzleManager.currentSize);
            int realAge = PuzzleManager.Instance.ageList[nodeIndex];
            float progress = ((float)StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex)) / ((float)StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
            int nextPuzzleOffSet = StoryPuzzlesSaver.Instance.GetCurrentPuzzleIndesOffset(nodeIndex);
            if (!PuzzleManager.currentIsChallenge && !PuzzleManager.currentIsMultiMode)
            {
                nextButton.gameObject.SetActive((progress < 1 && nodeIndex == StoryPuzzlesSaver.Instance.MaxNode + 1)
                    || (nodeIndex <= StoryPuzzlesSaver.Instance.MaxNode && nextPuzzleOffSet != 0));
            }
            else
            {
                nextButton.gameObject.SetActive(false);
            }
            if (PuzzleManager.currentIsMultiMode || PuzzleManager.currentIsChallenge)
                progressBar.Hide();
            else
                progressBar.Show();
            if (multiplayerModeAgainBtn != null)
                multiplayerModeAgainBtn.gameObject.SetActive(PuzzleManager.currentIsMultiMode && !isOpponnentDisconnected && !isPlayerDisconnected);
            if (waitingPlayerButton != null)
                waitingPlayerButton.gameObject.SetActive(false);
            if (!PuzzleManager.currentIsChallenge)
                nextPuzzleCostText.text = EnergyManager.Instance.GetCostByLevel(PuzzleManager.Instance.GetPuzzleById(StoryPuzzlesSaver.Instance.GetNextPuzzle()), EnergyManager.Instance.StoryModeEnergyCost).ToString();
            //homeButton.gameObject.SetActive(PuzzleManager.currentIsChallenge || (StoryPuzzlesSaver.Instance.HasNextPuzzle(realAge)));
            homeButton.gameObject.SetActive(true);
            //homeIcon.gameObject.SetActive(!string.IsNullOrEmpty(PuzzleManager.nextPuzzleId));
            //homeText.gameObject.SetActive(string.IsNullOrEmpty(PuzzleManager.nextPuzzleId));
            rematchEnergyCost.text = EnergyManager.Instance.MultiplayerEnergyCost.ToString();
            homeIcon.gameObject.SetActive(nextButton.gameObject.activeSelf || multiplayerModeAgainBtn.gameObject.activeSelf);
            homeText.gameObject.SetActive(!homeIcon.gameObject.activeSelf);
            homeText.text = PuzzleManager.currentIsChallenge ? I2.Loc.ScriptLocalization.EXIT.ToUpper() : I2.Loc.ScriptLocalization.HOME.ToUpper();

            countDownForScreeenShotIsDone = false;
            screenShotCover.gameObject.SetActive(false);
            IsShowing = true;
            controller.ShowIfNot();
            transform.BringToFront();
            onPanelStateChanged(this, true);
            if (!MultiplayerSession.playerWin && PuzzleManager.currentIsMultiMode)
            {
                Debug.LogWarning("Adjust position winmenu");
                container.transform.localPosition += Vector3.up * 60;
            }

            for (int i = 0; i < dayNightAdapter.Length; ++i)
            {
                dayNightAdapter[i].Adapt();
            }

            if (!PuzzleManager.currentIsMultiMode || (PuzzleManager.currentIsMultiMode && MultiplayerSession.playerWin))
                ShowRibbonIfNot();
            SelectPngGroup();

            CoroutineHelper.Instance.DoActionDelay(
                () =>
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(infoGroup);
                }, 0);
        }

        public override void Hide()
        {
            //* Clean up generated texture2d and release render texture
            if (!PuzzleManager.currentIsMultiMode)
            {
                if (this.screenShotRT != null)
                    this.screenShotRT.Release();
                if (this.convertedScreenShot != null)
                    Destroy(this.convertedScreenShot);
                this.boardScreenshot.texture = null;
            }

            IsShowing = false;
            controller.HideIfNot();
            StopAllCoroutines();
            onPanelStateChanged(this, false);
            playUI.DisplayHeaderGroupButton(true);
            HideRibbonIfNot();
        }

        private void OnEnable()
        {
            Judger.onPreJudging += OnPreJudging;
            Judger.onJudgingCompleted += OnJudgingCompleted;
            MultiplayerRoom.LoadedMultiplayerPuzzle += OnLoadedMultiplayerPuzzle;
            MultiplayerRoom.OpponentReady += OnOpponentReady;
            MultiplayerManager.OpponentDisconnected += OnOpponentDisconnected;
            MatchingPanelController.DeclineMatchingEvent += OnDeclineMatchingEvent;
        }

        private void OnDisable()
        {
            Judger.onPreJudging -= OnPreJudging;
            Judger.onJudgingCompleted -= OnJudgingCompleted;
            MultiplayerRoom.LoadedMultiplayerPuzzle -= OnLoadedMultiplayerPuzzle;
            MultiplayerRoom.OpponentReady -= OnOpponentReady;
            MultiplayerManager.OpponentDisconnected -= OnOpponentDisconnected;
            MatchingPanelController.DeclineMatchingEvent -= OnDeclineMatchingEvent;
        }

        private void OnDestroy()
        {
            DestroyScreenshot();
            UIReferences.UiReferencesUpdated -= UpdateReferences;
        }

        public void OnDeclineMatchingEvent()
        {
            multiplayerModeAgainBtn.gameObject.SetActive(false);
            homeIcon.gameObject.SetActive(false);
            homeText.gameObject.SetActive(true);
            homeText.text = I2.Loc.ScriptLocalization.EXIT.ToUpper();
        }

        void OnOpponentDisconnected()
        {
            isOpponnentDisconnected = true;
            //multiplayerModeAgainBtn.gameObject.SetActive(false);
            //homeIcon.gameObject.SetActive(false);
            //homeText.gameObject.SetActive(true);
            //homeText.text = "EXIT";
        }

        void OnOpponentReady()
        {
            againTxtAnimator.enabled = false;
        }

        void OnLoadedMultiplayerPuzzle()
        {
            if (IsShowing)
                Hide();
        }

        private void UpdateReferences()
        {
            dialog = UIReferences.Instance.overlayConfirmDialog;
            camController = UIReferences.Instance.mainCameraController;
            rewardDetailPanel = UIReferences.Instance.overlayRewardDetailPanel;
            taskPanel = UIReferences.Instance.overlayTaskPanel;
            playUI = UIReferences.Instance.gameUiPlayUI;

        }

        private void Awake()
        {
            ResetDisconnectedState();

            if (UIReferences.Instance != null)
            {
                UpdateReferences();
            }
            UIReferences.UiReferencesUpdated += UpdateReferences;

            shareButton.onClick.AddListener(delegate
                {
                    if (pngGroup.activeInHierarchy)
                    {
                        ShareNewPng();
                    }
                    else if (gifGroup.activeInHierarchy)
                    {
                        if (string.IsNullOrEmpty(gifUrl))
                            ShareNewGif();
                        else
                            ShareCachedGif();
                    }

                });

            homeButton.onClick.AddListener(delegate
                {
                    GoBackHandle();
                });
            if (multiplayerModeAgainBtn != null)
            {
                multiplayerModeAgainBtn.onClick.AddListener(() =>
                {
                    if (CoinManager.Instance.Coins < 0)
                    {
                        UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), String.Format(I2.Loc.ScriptLocalization.OWE_COIN,
                        Mathf.Abs(CoinManager.Instance.Coins)), I2.Loc.ScriptLocalization.OK.ToUpper(), "", () =>
                            {
                                OnDeclineMatchingEvent();
                                SetPlayerDisconnected();
                                MultiplayerManager.Instance.LeaveRoom();
                            });
                        return;
                    }

                    if (EnergyManager.Instance.CurrentEnergy < EnergyManager.Instance.MultiplayerEnergyCost)
                    {
                        UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), string.Format(I2.Loc.ScriptLocalization.NOT_ENOUGH_ENERGY, EnergyManager.Instance.MultiplayerEnergyCost),
                        I2.Loc.ScriptLocalization.OK.ToUpper(), "", () =>
                        {
                            OnDeclineMatchingEvent();
                            SetPlayerDisconnected();
                            MultiplayerManager.Instance.LeaveRoom();
                        });
                        return;
                    }

                    if (!MultiplayerManager.Instance.room.WaitAllPlayerInfomation())
                    {
                        return;
                    }

                    UIReferences.Instance.matchingPanelController.SetTitle(I2.Loc.ScriptLocalization.REMATCH.ToUpper());
                    UIReferences.Instance.matchingPanelController.Show();
                    Hide();
                    // if (MultiplayerRoom.Instance != null)
                    // {
                    //     //MultiplayerRoom.Instance.PlayerReady();
                    //     //waitingPlayerButton.gameObject.SetActive(true);
                    //     //multiplayerModeAgainBtn.gameObject.SetActive(false);
                    //     //shareButton.interactable = true;
                    // }
                });
            }
            nextButton.onClick.AddListener(delegate
            {
                //screenShotCover.gameObject.SetActive(true);
                clipPlayer.Stop();
                Hide();
                CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    DestroyScreenshot();
                    string puzzleId = StoryPuzzlesSaver.Instance.GetNextPuzzle();
                    Debug.Log(puzzleId);
                    GameManager.Instance.PlayAPuzzle(puzzleId);

                    #region Lion Event
                    //TODO: LionAnalytics.MissionStarted
                    AlolAnalytics.MissionStarted(puzzleId);
                    #endregion
                },
                controller.MaxDuration + 0.5f);

            });
            pngButton.onClick.AddListener(delegate
                {
                    SelectPngGroup();
                });

            gifButton.onClick.AddListener(delegate
                {
                    SelectGifGroup();
                });
        }

        public void GoBackHandle()
        {
            //screenShotCover.gameObject.SetActive(true);
            isOpponnentDisconnected = false;
            clipPlayer.Stop();
            Hide();
            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                DestroyScreenshot();
            },
            controller.MaxDuration + 0.5f);
            GameManager.Instance.PrepareGame();
            if (StoryPuzzlesSaver.Instance.StoryModeIsCompleted && !PlayerPrefs.HasKey("EndGameAnimationIsShown")
            && !PuzzleManager.currentIsChallenge && StoryPuzzlesSaver.GetIndexNode(PuzzleManager.currentLevel, PuzzleManager.currentSize) == 19)
            {
                //Try to load end game animation
                SceneLoadingManager.Instance.LoadEndingScene();
                PlayerPrefs.SetInt("EndGameAnimationIsShown", 1);
            }
            //TryPromptForNotification();
        }

        private void SelectPngGroup()
        {
            gifGroup.SetActive(false);
            pngGroup.SetActive(true);
            gifButtonBackground.color = buttonInactiveColor;
            pngButtonBackground.color = buttonActiveColor;
            clipPlayer.Stop();
        }

        private void SelectGifGroup()
        {
            gifGroup.SetActive(true);
            pngGroup.SetActive(false);
            gifButtonBackground.color = buttonActiveColor;
            pngButtonBackground.color = buttonInactiveColor;
            if (clip != null)
            {
                clipPlayer.Play(clip, 0, true);
            }
        }

        public void ShareCachedGif()
        {
            EasyMobile.Sharing.ShareURL(gifUrl, AppInfo.Instance.DEFAULT_SHARE_MSG);
        }

        public void ShareNewPng()
        {
            taskPanel.Show();
            float t = 0;
            if (!countDownForScreeenShotIsDone)
            {
                CoroutineHelper.Instance.RepeatUntil(() =>
                {
                    taskPanel.SetProgress(t / 3);
                    taskPanel.SetTask(I2.Loc.ScriptLocalization.PREPARE_SCREENSHOT);
                    taskPanel.SetGiphyActive(false);
                    taskPanel.SetEMActive(true);
                    t += Time.deltaTime;
                }, Time.deltaTime, () => t > 3);
            }
            CoroutineHelper.Instance.PostponeActionUntil(() =>
            {
                countDownForScreeenShotIsDone = true;
                taskPanel.SetProgress(1);
                taskPanel.SetTask(I2.Loc.ScriptLocalization.COMPLETE_EXPORT);
                taskPanel.SetGiphyActive(false);
                taskPanel.SetEMActive(true);
                //* Convert renderTexture to texture2d
                if (this.convertedScreenShot == null)
                    this.convertedScreenShot = MultiplayerShareBgController.RenderTextureToTexture2dConvert(this.screenShotRT);
                CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    taskPanel.Hide();
                    EasyMobile.Sharing.ShareTexture2D(
                    this.convertedScreenShot,
                    string.Format("takuzu-screenshot-{0}", System.DateTime.UtcNow.Millisecond),
                    AppInfo.Instance.DEFAULT_SHARE_MSG);
                }, 0.5f);
            }, () => (t > 3 || countDownForScreeenShotIsDone == true));
        }

        public void ShareNewGif()
        {
            taskPanel.Show();
            bool completed = false;
            if (string.IsNullOrEmpty(gifPath))
            {
                Gif.ExportGif(
                    clip,
                    string.Format("takuzu-gif-{0}", System.DateTime.UtcNow.Millisecond),
                    0,
                    80,
                    System.Threading.ThreadPriority.Normal,
                    (c, progress) =>
                    {
                        taskPanel.SetProgress(progress);
                        taskPanel.SetTask(I2.Loc.ScriptLocalization.EXPORTING_GIF);
                        taskPanel.SetGiphyActive(false);
                        taskPanel.SetEMActive(true);
                    },
                    (c, path) =>
                    {
                        gifPath = path;
                        completed = true;
                    }
                );
            }
            else
            {
                completed = true;
            }

            CoroutineHelper.Instance.PostponeActionUntil(() =>
                {

                    if (string.IsNullOrEmpty(gifPath))
                    {
                        taskPanel.SetTask(I2.Loc.ScriptLocalization.EXPORTING_GIF_FAIL);
                        CoroutineHelper.Instance.DoActionDelay(() =>
                            {
                                taskPanel.SetTask(string.Empty);
                                taskPanel.Hide();
                            }, 3);
                        return;
                    }
                    else
                    {
                        if (Application.internetReachability == NetworkReachability.NotReachable)
                        {
                            taskPanel.SetTask(I2.Loc.ScriptLocalization.NO_INTERNET_FOR_SHARE);
                            taskPanel.SetProgress(0);
                            CoroutineHelper.Instance.DoActionDelay(() =>
                                {
                                    taskPanel.SetTask(string.Empty);
                                    taskPanel.Hide();
                                }, 3);
                            return;
                        }

                        GiphyUploadParams u = new GiphyUploadParams();
                        u.localImagePath = gifPath;
                        u.tags = "takuzu";
                        taskPanel.SetTask(I2.Loc.ScriptLocalization.UPLOADING_GIF);
                        taskPanel.SetGiphyActive(true);
                        taskPanel.SetEMActive(false);
                        Giphy.Upload(
                            SocialManager.giphyChannel,
                            SocialManager.giphyApiKey,
                            u,
                            (progress) =>
                            {
                                taskPanel.SetProgress(progress);
                            },
                            (url) =>
                            {
                                gifUrl = url;
                                taskPanel.SetTask(I2.Loc.ScriptLocalization.COMPLETE_EXPORT);
                                CoroutineHelper.Instance.DoActionDelay(() =>
                                    {
                                        taskPanel.SetTask(string.Empty);
                                        taskPanel.Hide();
                                        EasyMobile.Sharing.ShareURL(url, AppInfo.Instance.DEFAULT_SHARE_MSG);
                                    }, 3);
                            },
                            (error) =>
                            {
                                taskPanel.SetTask(I2.Loc.ScriptLocalization.UPLOADING_GIF_FAIL);
                                Debug.Log("Upload Gif failed: " + error);
                                CoroutineHelper.Instance.DoActionDelay(() =>
                                    {
                                        taskPanel.SetTask(string.Empty);
                                        taskPanel.Hide();
                                    }, 3);
                            });
                    }
                },
                () =>
                {
                    return completed;
                });
        }

        private void OnJudgingCompleted(Judger.JudgingResult stat)
        {
            #region Lion Event
            //TODO: LionAnalytics.MissionCompleted
            AlolAnalytics.MissionCompleted(PuzzleManager.currentPuzzleId, stat.solvingTime);
            #endregion

            //Check if this is multiplayer mode
            SetStat(stat);

            if (!PuzzleManager.currentIsChallenge && !PuzzleManager.currentIsMultiMode)
            {
                GetStoryModeBoardTexture();
            }
            else
            {
                if (PuzzleManager.currentIsChallenge)
                    GetChallengeBoardTexture();
                else
                    GetMultiplayerTexture();
            }

            CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    if (MultiplayerManager.Instance != null)
                    {
                        CoroutineHelper.Instance.PostponeActionUntil(() =>
                        {
                            if (ribbonText != null)
                            {
                                ribbonText.text = I2.Loc.ScriptLocalization.Finish.ToUpper();
                            }
                            if (!PuzzleManager.currentIsMultiMode || (PuzzleManager.currentIsMultiMode && MultiplayerSession.playerWin))
                                ShowRibbonIfNot();
                            camController.StartRecordingGif();
                        }, () => MultiplayerSession.sessionFinished);
                    }
                    else
                    {
                        if (!PuzzleManager.currentIsMultiMode || (PuzzleManager.currentIsMultiMode && MultiplayerSession.playerWin))
                            ShowRibbonIfNot();
                        camController.StartRecordingGif();
                    }

                }, gifRecordingDelay);

            CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    //we need to check this because the recorder would be stopped and the clip would be disposed (CameraController.cs)
                    //if screen resolution change during recording (occur in multiscreen mode on android 7+) 
                    if (camController.recorder.IsRecording())
                    {
                        clip = camController.StopRecordingGif();
                    }
                },
                gifRecordingDelay + camController.recorder.Length);

            CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    if (MultiplayerManager.Instance != null)
                    {
                        CoroutineHelper.Instance.PostponeActionUntil(() =>
                        {
                            if (ribbonText != null)
                            {
                                ribbonText.text = I2.Loc.ScriptLocalization.Finish.ToUpper();
                            }
                            Show();
                        }, () => MultiplayerSession.sessionFinished);
                    }
                    else
                    {
                        Show();
                    }
                },
                showDelayOnPuzzleSolved + ((!PuzzleManager.currentIsChallenge && !PuzzleManager.currentIsMultiMode) ? VisualBoard.Instance.currentTotalTimeFlip : 2.5f));

            statAnimDone = false;
            buttonGroup.blocksRaycasts = false;
            CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    PlayStatAnim();
                },
                showDelayOnPuzzleSolved + ((!PuzzleManager.currentIsChallenge && !PuzzleManager.currentIsMultiMode) ? VisualBoard.Instance.currentTotalTimeFlip : 0) + 1);

            CoroutineHelper.Instance.PostponeActionUntil(
                () =>
                {
                },
                () =>
                {
                    return statAnimDone;
                });
        }

        public void SetStat(Judger.JudgingResult stat)
        {
            this.stat = stat;
            playTimeText.text = new TimeSpan(0, 0, 0, 0).ToString();
            coinText.text = string.Format("{0}", 0);
            double preExp = PlayerInfoManager.Instance.expProfile.ToTotalExp(PlayerInfoManager.Instance.info) - stat.exp;
            PlayerInfo oldInfo = PlayerInfoManager.Instance.info.AddExp(-stat.exp);
            expText.text = string.Format("{0}", preExp);
            expSlider.normalizedValue = oldInfo.NormalizedExp;
            rankIcon.sprite = ExpProfile.active.icon[oldInfo.level];

            string info1;
            try
            {
                info1 =
                PuzzleManager.currentPuzzleId.StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX) ? "DAILY\nCHALLENGE" :
                PuzzleManager.currentPuzzleId.StartsWith(PuzzleManager.WEEKLY_PUZZLE_PREFIX) ? "WEEKLY\nCHALLENGE" :
                PuzzleManager.currentIsMultiMode ? I2.Loc.ScriptLocalization.MULTIPLAYER :
                PuzzleManager.currentPack.packName.ToUpper();
            }
            catch (Exception e)
            {
                info1 = string.Empty;
                Debug.LogWarning("Reported in WinMenu.SetStat(): " + e.ToString());
            }
            string info2;
            try
            {
                int nodeIndex = StoryPuzzlesSaver.GetIndexNode(PuzzleManager.currentLevel, PuzzleManager.currentSize);
                int preAge = nodeIndex > 0 ? PuzzleManager.Instance.ageList[nodeIndex - 1] : 0;
                int realAge = PuzzleManager.Instance.ageList[nodeIndex];
                info2 = String.Format("{0}x{0}", (int)PuzzleManager.currentSize);
                //    info2 =
                //PuzzleManager.currentIsChallenge ? PuzzleManager.Instance.GetChallengeCreationDate(PuzzleManager.currentPuzzleId).ToShortDateString() :
                //("Year " + realAge.ToString());
                //infoIcon2.sprite =
                //    PuzzleManager.currentIsChallenge ?
                //    infoIconChallenge : infoIconNormal;
            }
            catch (Exception e)
            {
                info2 = string.Empty;
                Debug.LogWarning("Reported in WinMenu.SetStat(): " + e.ToString());
            }

            string info3;
            try
            {
                info3 = Utilities.GetDifficultyDisplayName(PuzzleManager.currentLevel);
            }
            catch (Exception e)
            {
                info3 = string.Empty;
                Debug.LogWarning("Reported in WinMenu.SetStat(): " + e.ToString());
            }

            string info4;
            try
            {
                info4 = PuzzleManager.Instance.GetChallengeCreationDate(PuzzleManager.currentPuzzleId).ToShortDateString();
            }
            catch (Exception e)
            {
                info4 = string.Empty;
                Debug.LogWarning("Reported in WinMenu.SetStat(): " + e.ToString());
            }
            if (PuzzleManager.currentIsChallenge)
            {
                dateGroup.SetActive(true);
                levelProgressGroup.SetActive(false);
            }
            else
            {
                dateGroup.SetActive(false);
                levelProgressGroup.SetActive(true);
            }
            infoText1.text = Takuzu.Utilities.GetLocalizePackNameByLevel(PuzzleManager.currentLevel);
            infoText1.text = infoText1.text.Substring(0, 1) + infoText1.text.Substring(1, infoText1.text.Length - 1).ToLower();
            infoText2.text = info2;
            infoText3.text = info3;
            dateText.text = info4;
            if (MultiplayerManager.Instance == null)
            {
                infoText1.gameObject.SetActive(true);
                betCoinContainer.SetActive(false);
            }
            else
            {
                infoText1.gameObject.SetActive(false);
                betCoinContainer.SetActive(true);
                betCoinTxt.text = MultiplayerRoom.Instance.currentBetCoin.ToString();
            }
        }

        float lastProgress = 0;

        private void OnPreJudging()
        {
            int nodeIndex = PuzzleManager.currentIsChallenge ? StoryPuzzlesSaver.Instance.MaxNode : StoryPuzzlesSaver.GetIndexNode(PuzzleManager.currentLevel, PuzzleManager.currentSize);
            int realAge = PuzzleManager.Instance.ageList[nodeIndex];
            float progress = ((float)StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex)) / ((float)StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
            lastProgress = progress;
            if (MultiplayerManager.Instance == null)
            {
                progressBar.SetProgress(progress, true, StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
                solvedIcon.SetActive(progress >= 1 || nodeIndex < StoryPuzzlesSaver.Instance.MaxNode + 1);
                if (solvedIcon.activeSelf)
                    solvedImgIcon.sprite = AgePathIcon.Get(nodeIndex, true);
                currentIcon.SetActive(!solvedIcon.activeSelf);
                ageText.gameObject.SetActive(!solvedIcon.activeSelf);
                string currentMileStone = String.Format("{0}.{1}", nodeIndex + 1,
                        StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex) < StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex)
                        ? StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex) + 1 : StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
                ageText.text = currentMileStone;
                //progressBar.SetDisplayText(currentMileStone);
            }
            else
            {
                solvedIcon.SetActive(true);
                currentIcon.SetActive(!solvedIcon.activeSelf);
                ageText.gameObject.SetActive(!solvedIcon.activeSelf);
                if (MultiplayerSession.playerWin)
                    solvedImgIcon.sprite = Resources.Load<Sprite>("ageicon/ageicon-default-on");
                else
                    solvedImgIcon.sprite = Resources.Load<Sprite>("ageicon/ageicon-default-off");
            }
        }
        /*
        private void TryPromptForNotification()
        {
            if (NotificationManager.Instance.WillPromptAfterChallenge)
            {
                CoroutineHelper.Instance.PostponeActionUntil(
                        () =>
                        {
                            NotificationManager.Instance.PromptUserForNotification();
                        },
                        () =>
                        {
                            return !rewardDetailPanel.IsShowing;
                        });
            }
        }
        */

        private void GetMultiplayerTexture()
        {
            StartCoroutine(CrGetMultiplayerTexture());
        }

        private IEnumerator CrGetMultiplayerTexture()
        {
            boardScreenshot.uvRect = new Rect(0, 0, 1, 1);
            InputHandler.Instance.hideCursorForScreenShot = true;
            playUI.DisplayHeaderGroupButton(false);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            //float aspect = boardScreenshot.rectTransform.rect.width / boardScreenshot.rectTransform.rect.height;
            //boardScreenshot.texture = camController.GetBoardTexture(aspect, color);
            if (screenShotRT != null)
                screenShotRT.Release();
            this.screenShotRT = UIReferences.Instance.multiplayerShareBGController.TakeMultiplayerCapture((int)boardScreenshot.rectTransform.sizeDelta.x,
            (int)boardScreenshot.rectTransform.sizeDelta.x, MultiplayerSession.playerWin);
            boardScreenshot.texture = this.screenShotRT;
            boardScreenshot.rectTransform.sizeDelta = new Vector2(
                boardScreenshot.rectTransform.sizeDelta.x,
                boardScreenshot.rectTransform.sizeDelta.x);
            InputHandler.Instance.hideCursorForScreenShot = false;
        }

        private void GetChallengeBoardTexture()
        {
            StartCoroutine(CrGetChallengeBoardTexture());
        }

        private IEnumerator CrGetChallengeBoardTexture()
        {
            ModifyBoardScreenshotUV();
            InputHandler.Instance.hideCursorForScreenShot = true;
            playUI.DisplayHeaderGroupButton(false);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            //float aspect = boardScreenshot.rectTransform.rect.width / boardScreenshot.rectTransform.rect.height;
            //boardScreenshot.texture = camController.GetBoardTexture(aspect, color);
            if (PuzzleManager.currentIsChallenge)
            {

                this.convertedScreenShot = EasyMobile.Sharing.CaptureScreenshot();
                boardScreenshot.texture = this.convertedScreenShot;
                boardScreenshot.rectTransform.sizeDelta = new Vector2(boardScreenshot.rectTransform.sizeDelta.x, boardScreenshot.rectTransform.sizeDelta.x / Camera.main.aspect);
                InputHandler.Instance.hideCursorForScreenShot = false;
            }
        }

        void ModifyBoardScreenshotUV()
        {
            float ratio = (float)Screen.width / Screen.height;
            boardScreenshot.uvRect = new Rect(0, 0, 1, 1);
            if (ratio >= 0.73f)
            {
                boardScreenshot.uvRect = new Rect(0.1f, 0.1f, 0.8f, 0.8f);
                return;
            }

            if (ratio >= 0.65f)
            {
                boardScreenshot.uvRect = new Rect(0.05f, 0.05f, 0.9f, 0.9f);
                return;
            }
        }
        private void GetStoryModeBoardTexture()
        {
            StartCoroutine(CrGetStoryModeBoardTexture());
        }

        private IEnumerator CrGetStoryModeBoardTexture()
        {
            boardScreenshot.uvRect = new Rect(0, 0, 1, 1);
            InputHandler.Instance.hideCursorForScreenShot = true;
            playUI.DisplayHeaderGroupButton(false);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Texture bgTexture = FlipBackGround.GetMainSprite(VisualBoard.Instance.CurrentFlipSpriteName).texture;
            this.screenShotRT = UIReferences.Instance.multiplayerShareBGController.TakeStoryModeCapture(bgTexture.width, bgTexture.height, bgTexture);
            boardScreenshot.texture = this.screenShotRT;
            boardScreenshot.rectTransform.sizeDelta = new Vector2(boardScreenshot.rectTransform.sizeDelta.x, boardScreenshot.rectTransform.sizeDelta.x);
            InputHandler.Instance.hideCursorForScreenShot = false;
        }

        public void DestroyScreenshot()
        {
            if (boardScreenshot.texture != null && (PuzzleManager.currentIsChallenge || PuzzleManager.currentIsMultiMode))
            {
                Destroy(boardScreenshot.texture);
            }
            if (clip != null && !clip.IsDisposed())
            {
                clip.Dispose();
                clip = null;
            }
            if (clipPlayerRaw.texture != null)
            {
                Destroy(clipPlayerRaw.texture);
                clipPlayerRaw.texture = null;
            }
            gifUrl = null;
            gifPath = null;
        }

        private void PlayStatAnim()
        {
            bool timeAnimCompleted = false;
            bool coinAnimCompleted = false;
            bool expAnimCompleted = false;

            float buttonLockTime = Time.time;
            float maxLockTime = 10;
            buttonGroup.blocksRaycasts = false;
            CoroutineHelper.Instance.PostponeActionUntil(
                () => clockController.UpdateTime(new TimeSpan(0, 0, 0, stat.solvingTime)),
                () => IsShowing);
            CoroutineHelper.Instance.PostponeActionUntil(
                () =>
                {
                    buttonGroup.blocksRaycasts = true;
                },
                () =>
                {
                    return
                        (timeAnimCompleted && coinAnimCompleted && expAnimCompleted) ||
                    Time.time - buttonLockTime >= maxLockTime || true;
                });

            float animMaxDuration = 3;
            int time = 0;
            int minOffset = UnityEngine.Random.Range(3, 7 + 1);
            int timeOffset = Mathf.Max(minOffset, (int)(stat.solvingTime * Time.smoothDeltaTime / animMaxDuration));

            CoroutineHelper.Instance.RepeatUntil(
                () =>
                {
                    time = (int)Mathf.MoveTowards(time, stat.solvingTime, timeOffset);
                    playTimeText.text = new TimeSpan(0, 0, 0, time).ToString();
                    //if (IsShowing)
                    //    SoundManager.Instance.PlaySound(SoundManager.Instance.tick, true);
                    if (time >= stat.solvingTime)
                    {
                        timeAnimCompleted = true;
                    }
                },
                0,
                () =>
                {
                    return time >= stat.solvingTime;
                });

            int c = 0;
            int minCoinOffset = UnityEngine.Random.Range(1, 2 + 1);
            int coinOffset = Mathf.Max(minCoinOffset, (int)(stat.coin * Time.smoothDeltaTime / animMaxDuration));
            CoroutineHelper.Instance.RepeatUntil(
                () =>
                {
                    c = (int)Mathf.MoveTowards(c, Mathf.Abs(stat.coin), coinOffset);
                    coinText.text = string.Format("{0}{1}", stat.coin >= 0 ? "+" : "-", c);
                    if (IsShowing)
                        SoundManager.Instance.PlaySound(SoundManager.Instance.tick, true);
                    if (c >= Mathf.Abs(stat.coin))
                    {
                        coinAnimCompleted = true;
                    }
                },
                0,
                () =>
                {
                    return c >= Mathf.Abs(stat.coin);
                });

            if (stat.exp == 0)
                expAnimCompleted = true;
            else
            {
                CoroutineHelper.Instance.PostponeActionUntil(
                    () =>
                    {
                        int preExp = PlayerInfoManager.Instance.expProfile.ToTotalExp(PlayerInfoManager.Instance.info) - stat.exp;
                        PlayerInfo oldInfo = PlayerInfoManager.Instance.info.AddExp(-stat.exp);
                        int e = 0;
                        int minExpOffset = UnityEngine.Random.Range(1, 2 + 1);
                        int expOffset = Mathf.Max(minExpOffset, (int)(stat.exp * Time.smoothDeltaTime / animMaxDuration));
                        CoroutineHelper.Instance.RepeatUntil(
                            () =>
                            {
                                e = (int)Mathf.MoveTowards(e, stat.exp, expOffset);
                                if (expText != null)
                                    expText.text = string.Format("{0}", preExp + e);

                                PlayerInfo tmp = oldInfo.AddExp(e);
                                expSlider.normalizedValue = tmp.NormalizedExp;
                                if (rankIcon.sprite != null && tmp.level != oldInfo.level)
                                    rankIcon.sprite = ExpProfile.active.icon[tmp.level];
                                if (IsShowing && SoundManager.Instance != null)
                                    SoundManager.Instance.PlaySound(SoundManager.Instance.tick, true);
                                if (e >= stat.exp)
                                {
                                    expAnimCompleted = true;
                                    //statAnimDone = true;
                                }
                            },
                            0,
                            () =>
                            {
                                return e >= stat.exp;
                            });
                    },
                    () =>
                    {
                        return coinAnimCompleted;
                    });
            }
            int nodeIndex = PuzzleManager.currentIsChallenge ? StoryPuzzlesSaver.Instance.MaxNode : StoryPuzzlesSaver.GetIndexNode(PuzzleManager.currentLevel, PuzzleManager.currentSize);
            int realAge = PuzzleManager.Instance.ageList[nodeIndex];
            float progress = ((float)StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex)) / ((float)StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));

            //progressBar.SetProgress(lastProgress, true, StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
            //progressBar.SetProgressColor((lastProgress < 1 && nodeIndex == StoryPuzzlesSaver.Instance.MaxNode + 1) ? currentColor : solvedColor);
            //solvedIcon.SetActive(lastProgress >= 1 || nodeIndex < StoryPuzzlesSaver.Instance.MaxNode + 1);
            //if (solvedIcon.activeSelf)
            //    solvedImgIcon.sprite = AgePathIcon.Get(nodeIndex, true);
            //currentIcon.SetActive(!solvedIcon.activeSelf);
            //ageText.gameObject.SetActive(!solvedIcon.activeSelf);
            CoroutineHelper.Instance.PostponeActionUntil(() =>
            {
                if (lastProgress != progress)
                {
                    CoroutineHelper.Instance.DoActionDelay(() =>
                    {
                        statAnimDone = true;
                    }, 1);
                    progressBar.AnimateToProgress(progress, StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex), 1, true);
                }
                else
                {
                    if (MultiplayerManager.Instance == null)
                    {
                        progressBar.SetProgress(progress, true, StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
                        solvedIcon.SetActive(progress >= 1 || nodeIndex < StoryPuzzlesSaver.Instance.MaxNode + 1);
                        if (solvedIcon.activeSelf)
                            solvedImgIcon.sprite = AgePathIcon.Get(nodeIndex, true);
                        currentIcon.SetActive(!solvedIcon.activeSelf);
                        statAnimDone = true;
                    }
                    else
                    {
                        solvedIcon.SetActive(true);
                        currentIcon.SetActive(!solvedIcon.activeSelf);
                        ageText.gameObject.SetActive(!solvedIcon.activeSelf);
                        if (MultiplayerSession.playerWin)
                            solvedImgIcon.sprite = Resources.Load<Sprite>("ageicon/ageicon-default-on");
                        else
                            solvedImgIcon.sprite = Resources.Load<Sprite>("ageicon/ageicon-default-off");
                        statAnimDone = true;
                    }
                }
            }, () => true);
            //string currentMileStone = String.Format("{0}.{1}", nodeIndex + 1,
            //        StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex) < StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex)
            //        ? StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex) + 1 : StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
            //progressBar.SetDisplayText(currentMileStone);
        }

        private void ShowRibbonIfNot()
        {
            if (ribbonHidden)
            {
                ribbonAnim.Play(AnimConstant.IN);
                ribbonHidden = false;
            }
        }

        private void HideRibbonIfNot()
        {
            if (!ribbonHidden)
            {
                ribbonAnim.Play(AnimConstant.OUT);
                ribbonHidden = true;
            }
        }

#if UNITY_EDITOR
        GUIStyle style;

        private void OnDrawGizmos()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                return;
            if (Camera.current != null && Vector3.Distance(Camera.current.transform.position, boardScreenshot.transform.position) < 100)
            {
                if (style == null)
                {
                    style = new GUIStyle(UnityEditor.EditorStyles.miniLabel);
                }

                style.normal.textColor = Color.magenta;
                style.alignment = TextAnchor.MiddleCenter;
                UnityEditor.Handles.Label(boardScreenshot.transform.position, "<Background image\nis assigned\nat runtime>", style);
            }
        }
#endif

        void SetMultiplayerResult()
        {
            if (MultiplayerManager.Instance != null && UIReferences.Instance.matchingPanelController != null)
            {
                string maxNode = MultiplayerManager.Instance.playerMultiplayerInfo.playerNode.ToString();
                int maxDiff = -1;
                if (!string.IsNullOrEmpty(maxNode))
                {
                    maxNode = maxNode[0].ToString().ToUpper() + (maxNode.Length > 1 ? maxNode.Substring(1) : string.Empty);
                    int.TryParse(maxNode, out maxDiff);
                }
                string levelName = Takuzu.Utilities.GetLocalizePackNameByLevel(StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff));

                if (!isPlayerDisconnected)
                    userLevel.text = levelName.Substring(0, 1).ToUpper() + levelName.Substring(1, levelName.Length - 1).ToLower();
                else
                    userLevel.text = String.Format("<color=#FF6E6Eff>{0}</color>", I2.Loc.ScriptLocalization.DISCONNECTED);

                //userLevel.color = PuzzleManager.Instance.accentColors[Mathf.Clamp((int)StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff) - 1, 0, PuzzleManager.Instance.accentColors.Count - 1)];
                if (MultiplayerManager.Instance.avatarRawImg.texture.name.Equals("default-avatar"))
                {
                    string avatarUrl = MultiplayerManager.Instance.playerMultiplayerInfo.avatarUrl;
                    if (!string.IsNullOrEmpty(avatarUrl))
                        CloudServiceManager.Instance.DownloadMultiplayerAvatar(avatarUrl, userAvatar);
                }
                else
                {
                    userAvatar.texture = MultiplayerManager.Instance.avatarRawImg.texture;
                }

                opponenName.text = MultiplayerManager.Instance.opponentMultiplayerInfo.playerName;
                maxNode = MultiplayerManager.Instance.opponentMultiplayerInfo.playerNode.ToString();
                maxDiff = -1;
                if (!string.IsNullOrEmpty(maxNode))
                {
                    maxNode = maxNode[0].ToString().ToUpper() + (maxNode.Length > 1 ? maxNode.Substring(1) : string.Empty);
                    int.TryParse(maxNode, out maxDiff);
                }
                levelName = Takuzu.Utilities.GetLocalizePackNameByLevel(StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff));

                if (!isOpponnentDisconnected)
                    opponentLevel.text = levelName.Substring(0, 1).ToUpper() + levelName.Substring(1, levelName.Length - 1).ToLower();
                else
                    opponentLevel.text = String.Format("<color=#FF6E6Eff>{0}</color>", I2.Loc.ScriptLocalization.DISCONNECTED);

                //opponentLevel.color = PuzzleManager.Instance.accentColors[Mathf.Clamp((int)StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff) - 1, 0, PuzzleManager.Instance.accentColors.Count - 1)];
                opponentAvatar.texture = UIReferences.Instance.headerMultiplayeInfo.defaultAvatar;
                if (UIReferences.Instance.matchingPanelController.opponentAvatar.texture.name.Equals("default-avatar"))
                {
                    string avatarUrl = MultiplayerManager.Instance.opponentMultiplayerInfo.avatarUrl;
                    if (!string.IsNullOrEmpty(avatarUrl))
                        CloudServiceManager.Instance.DownloadMultiplayerAvatar(avatarUrl, opponentAvatar);
                }
                else
                {
                    opponentAvatar.texture = UIReferences.Instance.matchingPanelController.opponentAvatar.texture;
                }

                multiplayerResult.text = String.Format("{0} | {1}", MultiplayerManager.Instance.LocalUserWin, MultiplayerManager.Instance.LocalOpponentWin);
            }
        }

        public void SetPlayerDisconnected()
        {
            isPlayerDisconnected = true;
            userLevel.text = String.Format("<color=#FF6E6Eff>{0}</color>", I2.Loc.ScriptLocalization.DISCONNECTED);
            OnDeclineMatchingEvent();
        }

        public void SetOpponentDisconnected()
        {
            isOpponnentDisconnected = true;
            opponentLevel.text = String.Format("<color=#FF6E6Eff>{0}</color>", I2.Loc.ScriptLocalization.DISCONNECTED);
            OnDeclineMatchingEvent();
        }

        public void ResetDisconnectedState()
        {
            isPlayerDisconnected = false;
            isOpponnentDisconnected = false;
        }
    }
}