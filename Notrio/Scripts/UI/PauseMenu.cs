using System;
using System.Collections;
using System.Collections.Generic;
using EasyMobile;
using LionStudios.Suite.Analytics;
using Takuzu.Generator;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class PauseMenu : OverlayPanel
    {
        public UiGroupController controller;
        public Button closeButton;
        public Toggle nightModeToggle;
        public Toggle symbolToggle;
        public Toggle soundToggle;
        public Toggle musicToggle;
        public Button clearButton;
        public Button quitButton;
        public Button removeAdButton;
        public Image removeAdBackground;
        public Color removeAdBackgroundActiveColor;
        public Color removeAdBackgroundInactiveColor;
        public Text infoText1;
        public Text infoText2;
        public Text infoText3;
        public Text infoText4;
        public Image infoIcon2;
        public Sprite infoIconChallenge;
        public Sprite infoIconNormal;
        public CanvasGroup infoGroup;

        public GameObject betCoinGroup;
        public GameObject dateLevelGroup;

        [HideInInspector]
        public ConfirmationDialog confirmDialog;
        [HideInInspector]
        public PlayUI playUI;

        private void Awake()
        {
            if (UIReferences.Instance != null)
            {
                UpdateReferences();
            }
            UIReferences.UiReferencesUpdated += UpdateReferences;
            GameManager.ForceOutInGamScene += OnForceOutInGameScene;
            GameManager.GameStateChanged += OnGameStateChanged;
            PuzzleManager.onPuzzleSelected += OnPuzzleSelected;
        }

        private void UpdateReferences()
        {
            confirmDialog = UIReferences.Instance.overlayConfirmDialog;
            playUI = UIReferences.Instance.gameUiPlayUI;
        }

        private void OnDestroy()
        {
            UIReferences.UiReferencesUpdated -= UpdateReferences;
            GameManager.ForceOutInGamScene -= OnForceOutInGameScene;
            GameManager.GameStateChanged -= OnGameStateChanged;
            PuzzleManager.onPuzzleSelected -= OnPuzzleSelected;
        }

        private void OnPuzzleSelected(string id, string puzzle, string solution, string progress)
        {
            SetLevelInfo(id);
        }

        void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (IsShowing)
                Hide();
        }

        void OnForceOutInGameScene()
        {
            if (IsShowing)
                Hide();
        }

        public override void Show()
        {
            soundToggle.isOn = !SoundManager.Instance.IsSoundMuted();
            musicToggle.isOn = !SoundManager.Instance.IsMusicMuted();
            symbolToggle.isOn = PersonalizeManager.ColorBlindFriendlyModeEnable;
            nightModeToggle.isOn = PersonalizeManager.NightModeEnable;

            InvokeRepeating("UpdateRemoveAdsButton", 0.15f, 1);
            controller.ShowIfNot();
            transform.BringToFront();
            IsShowing = true;
            onPanelStateChanged(this, true);
        }

        public override void Hide()
        {
            //container.gameObject.SetActive(false);
            controller.HideIfNot();
            IsShowing = false;
            //transform.SendToBack();
            CancelInvoke("UpdateRemoveAdsButton");
            onPanelStateChanged(this, false);
        }

        private void Start()
        {
            closeButton.onClick.AddListener(delegate
                {
                    Resume();
                });

            nightModeToggle.onValueChanged.AddListener((isOn) =>
                {
                    PersonalizeManager.NightModeEnable = isOn;
                });

            symbolToggle.onValueChanged.AddListener((isOn) =>
                {
                    PersonalizeManager.ColorBlindFriendlyModeEnable = isOn;
                });

            soundToggle.onValueChanged.AddListener((isOn) =>
                {
                    SoundManager.Instance.SetSoundMute(!isOn);
                });

            musicToggle.onValueChanged.AddListener((isOn) =>
                {
                    SoundManager.Instance.SetMusicMute(!isOn);
                });

            clearButton.onClick.AddListener(delegate
                {
                    if (LogicalBoard.Instance != null && !LogicalBoard.Instance.isPlayingRevealAnim)
                    {
                        confirmDialog.Show(
                        I2.Loc.ScriptLocalization.ATTENTION,
                        I2.Loc.ScriptLocalization.RESET_BOARD_MSG, I2.Loc.ScriptLocalization.RESET,
                        delegate
                        {
                            if (MultiplayerManager.Instance == null)
                            {
                                if (AdDisplayer.IsAllowToShowAd() && Advertising.IsInterstitialAdReady() && AdsFrequencyManager.Instance.IsAppropriateFrequencyForInterstitial() && !Advertising.IsAdRemoved()
                                    && InAppPurchaser.Instance != null && !InAppPurchaser.Instance.IsSubscibed() && Application.internetReachability != NetworkReachability.NotReachable)
                                {
#if UNITY_IOS
                                    Time.timeScale = 0;
                                    AudioListener.pause = true;
#endif
                                    Advertising.ShowInterstitialAd();

                                }
                            }
                            Powerup.Instance.SetType("Clear");
                        }, "", null, null);
                    }
                });

            quitButton.onClick.AddListener(delegate
                {
                    if (LogicalBoard.Instance != null && !LogicalBoard.Instance.isPlayingRevealAnim)
                    {
                        if (PuzzleManager.currentIsMultiMode)
                        {
                            UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.CONFIRMATION, I2.Loc.ScriptLocalization.MULTIPLAYER_QUIT_WARNING,
                                I2.Loc.ScriptLocalization.YES.ToUpper(), "", () =>
                                {
                                    GameManager.Instance.PrepareGame();
                                    Hide();
                                });
                        }
                        else
                        {
                            #region Lion Event
                            //TODO: LionAnalytics.MissionAbandoned
                            AlolAnalytics.MissionAbandoned(PuzzleManager.currentPuzzleId);
                            #endregion

                            GameManager.Instance.PrepareGame();
                            Hide();
                        }
                    }
                });

            removeAdButton.onClick.AddListener(delegate
            {
                if (UIReferences.Instance == null)
                    return;
                if (UIReferences.Instance.subscriptionDetailPanel == null)
                    return;
                UIReferences.Instance.subscriptionDetailPanel.Show();
            });
        }

        public void Resume()
        {
            GameManager.Instance.StartGame();
            Hide();
        }

        private void UpdateRemoveAdsButton()
        {
            if (AdDisplayer.IsAllowToShowAd() == false)
            {
                removeAdButton.interactable = false;
                removeAdBackground.raycastTarget = false;
                removeAdBackground.color = removeAdBackgroundInactiveColor;
            }
            else
            {
                removeAdButton.interactable = true;
                removeAdBackground.raycastTarget = false;
                removeAdBackground.color = removeAdBackgroundActiveColor;
            }
        }

        private void SetLevelInfo(string id)
        {
            string info1;
            try
            {
                if (!PuzzleManager.currentIsMultiMode)
                {
                    info1 = Takuzu.Utilities.GetLocalizePackNameByLevel(PuzzleManager.currentLevel);
                    info1 = info1.Substring(0, 1).ToUpper() + info1.Substring(1, info1.Length - 1).ToLower();
                }
                else
                {
                    string multiplayer = I2.Loc.ScriptLocalization.MULTIPLAYER;
                    info1 = multiplayer.Substring(0, 1).ToUpper() + multiplayer.Substring(1, multiplayer.Length - 1).ToLower();
                }
            }
            catch (System.Exception e)
            {
                info1 = string.Empty;
                Debug.LogWarning("Reported in PlayUI.SetLevelInfo(): " + e.ToString());
            }
            string info2;
            try
            {
                int nodeIndex = StoryPuzzlesSaver.GetIndexNode(PuzzleManager.currentLevel, PuzzleManager.currentSize);
                int currentAge = PuzzleManager.Instance.ageList[nodeIndex >= 0 ? nodeIndex : 0];
                string currentMileStone = String.Format("{0}.{1}", nodeIndex + 1,
                    StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex) < StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex)
                    ? StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex) + 1 : StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
                if (nodeIndex <= StoryPuzzlesSaver.Instance.MaxNode)
                {
                    currentMileStone = String.Format("{0}.{1}", nodeIndex + 1, StoryPuzzlesSaver.Instance.GetCurrentPuzzleIndesOffset(nodeIndex) + 1);
                }
                info2 = PuzzleManager.currentIsChallenge ?
                    PuzzleManager.Instance.GetChallengeCreationDate(PuzzleManager.currentPuzzleId).ToShortDateString() : currentMileStone;
                infoIcon2.sprite =
                    PuzzleManager.currentIsChallenge ?
                    infoIconChallenge : infoIconNormal;
            }
            catch (System.Exception e)
            {
                info2 = string.Empty;
                Debug.LogWarning("Reported in PlayUI.SetLevelInfo(): " + e.ToString());
            }

            string info3;
            try
            {
                info3 = Utilities.GetDifficultyDisplayName(PuzzleManager.currentLevel);
            }
            catch (System.Exception e)
            {
                info3 = string.Empty;
                Debug.LogWarning("Reported in PlayUI.SetLevelInfo(): " + e.ToString());
            }

            infoText1.text = info1;
            infoText2.text = info2;
            infoText3.text = info3;

            if (MultiplayerManager.Instance == null)
            {
                dateLevelGroup.gameObject.SetActive(true);
                betCoinGroup.gameObject.SetActive(false);
                infoText2.gameObject.SetActive(true);
                if (infoText4 != null)
                    infoText4.gameObject.SetActive(false);
            }
            else
            {
                dateLevelGroup.gameObject.SetActive(false);
                betCoinGroup.gameObject.SetActive(true);
                infoText2.gameObject.SetActive(false);
                infoText4.gameObject.SetActive(true);
                infoText4.text = MultiplayerRoom.Instance.currentBetCoin.ToString();
            }

            CoroutineHelper.Instance.DoActionDelay(
                () =>
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(infoGroup.transform as RectTransform);
                }, 0);
        }
    }
}