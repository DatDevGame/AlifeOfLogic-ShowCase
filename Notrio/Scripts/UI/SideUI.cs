using EasyMobile;
using GameSparks.Api.Responses;
using GameSparks.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu.Generator;

[System.Serializable]
public struct ButtonInfo
{
    public bool IsUse;
    public Button button;
}

namespace Takuzu
{
    public class SideUI : MonoBehaviour
    {
        public static Action<string> claimedReward = delegate { };
        public const string UiGuideEarnCoinSaveKey = "UIGUI_EARN_COIN_KEY";
        public UiGroupController controller;
        public ButtonInfo settingButtonInfo;
        [HideInInspector]
        public SettingPanel settingGroup;
        public ButtonInfo rewardButtonInfo;
        [HideInInspector]
        public RewardDetailPanel rewardDetailPanel;
        public ButtonInfo expLBButtonInfo;
        [HideInInspector]
        public LeaderBoardScreenUI leaderBoardScreen;
        public ButtonInfo tipsButtonInfo;
        public ButtonInfo TutorialButtonInfo;
        public ButtonInfo earnCoinsPanelInfo;
        public ButtonInfo premiumSubscriptionInfo;
        [HideInInspector]
        public TipsPanel tipsPanel;
        public float showDelay = 0.5f;
        public string UIGuideTipsSaveKey;
        public string UIGuideRewardsSaveKey;
        public string UIGUideExpLeaderboardSaveKey;
        public string UIGuideLuckySpinSaveKey;
        public string UIGuidePremiumSubscription;

        private float gameStateChangeTime;
        private float lastHide = 0;
        private const string luckySpineerOpenTimeKey = "luckySpinnerOpenTime";
        private const string luckySpineerCloseTimeKey = "luckySpinnerCloseTime";
        private int luckySpinnerOpenTime { get { return CloudServiceManager.Instance.appConfig.GetInt(luckySpineerOpenTimeKey) ?? 18; } }
        private int luckySpinnerCloseTime { get { return CloudServiceManager.Instance.appConfig.GetInt(luckySpineerCloseTimeKey) ?? 19; } }


        private EnergyExchangePanel energyExchangePanel;
        private bool wasShowLuckySpinAtFirstTime = false;

        private void Awake()
        {
            if (UIReferences.Instance != null)
            {
                UpdateReferences();
            }
            UIReferences.UiReferencesUpdated += UpdateReferences;
            GameManager.GameStateChanged += OnGameStateChanged;
            TopLeaderBoardReward.topChallengeRewardListChanged += OnTopChallengeRewardListChanged;
            CloudServiceManager.onPlayerDbSyncEnd += OnPlayerDbSyncEnd;
        }

        private void UpdateReferences()
        {
            settingGroup = UIReferences.Instance.overlaySettingPanel;
            rewardDetailPanel = UIReferences.Instance.overlayRewardDetailPanel;
            leaderBoardScreen = UIReferences.Instance.overlayLeaderBoardScreenUI;
            tipsPanel = UIReferences.Instance.overlayTipsPanel;
            energyExchangePanel = UIReferences.Instance.overlayEnergyExchangePanel;
        }

        private void OnTopChallengeRewardListChanged(List<RewardInformation> obj)
        {
            if (rewardButtonInfo.IsUse)
            {
                SetActiveRewardBtn();
                rewardButtonInfo.button.onClick.RemoveAllListeners();
                rewardButtonInfo.button.onClick.AddListener(delegate
                {
                    SideButtonRewardListener();
                });
            }
        }

        private void SideButtonRewardListener()
        {
            if (!TopLeaderBoardReward.rewardReceived || Application.internetReachability == NetworkReachability.NotReachable)
                //InGameNotificationPopup.Instance.ShowToast("Can not get Reward datas, Please check your internet connection", 5);
                InGameNotificationPopup.Instance.confirmationDialog.Show(I2.Loc.ScriptLocalization.ATTENTION, I2.Loc.ScriptLocalization.NO_INTERNET_CONNECTION, I2.Loc.ScriptLocalization.OK, "", () => { });
            else
            {
                ShowRewardDetailPanel();
            }
        }

        private void ShowRewardDetailPanel()
        {
            RewardInformation rewardInformation = TopLeaderBoardReward.Instance.rewards[0];
            TopLeaderBoardReward.getReward(rewardInformation, onGetRewardCallBack);
        }

        private void onGetRewardCallBack(LogEventResponse obj)
        {
            if (obj.HasErrors)
                Debug.Log(obj.Errors.JSON);
            else
            {
                RewardInformation rewardInformation = TopLeaderBoardReward.Instance.rewards[0];
                if (rewardInformation.rewardId.StartsWith(TopLeaderBoardReward.DAILY_LB_PREFIX) || rewardInformation.rewardId.StartsWith(TopLeaderBoardReward.WEEKLY_LB_PREFIX))
                    ShowTopDailyReward(rewardInformation);
                else
                    ShowOtherReward(rewardInformation);
                TopLeaderBoardReward.RefreshRewards();
            }
        }

        private void ShowOtherReward(RewardInformation rewardInformation)
        {
            if (rewardInformation.rewardId.StartsWith("invitation_"))
            {
                CoinManager.Instance.AddCoins(CoinManager.Instance.rewardProfile.rewardOnShareCode);
                ECAsPanelController.onCodeRewardClaimed(ECAsPanelController.RewardType.ShareCodeReward);
                rewardDetailPanel.SetTitle(I2.Loc.ScriptLocalization.SHARE_CODE_REWARD_TITLE);
                rewardDetailPanel.SetCoin(CoinManager.Instance.rewardProfile.rewardOnShareCode);
                rewardDetailPanel.SetBg(Background.Get("bg-reward"));
                rewardDetailPanel.SetAccentColor(new Color(0.9450981f, 0.7176471f, 0.007843138f));
                rewardDetailPanel.SetMessage(I2.Loc.ScriptLocalization.SHARE_CODE_REWARD_MSG);
                rewardDetailPanel.SetChallengeName(rewardInformation.challengeName);
                rewardDetailPanel.SetLaurelsActive(false);
                rewardDetailPanel.SetDateText(rewardInformation.date);

                rewardDetailPanel.Show(false);
            }
            if (rewardInformation.rewardId.StartsWith("welcome_"))
            {
                CoinManager.Instance.AddCoins(CoinManager.Instance.rewardProfile.rewardOnEnterCode);
                ECAsPanelController.onCodeRewardClaimed(ECAsPanelController.RewardType.RedeemCodeReward);
                rewardDetailPanel.SetTitle(I2.Loc.ScriptLocalization.ACTIVE_CODE_REWARD_TITLE);
                rewardDetailPanel.SetCoin(CoinManager.Instance.rewardProfile.rewardOnEnterCode);
                rewardDetailPanel.SetBg(Background.Get("bg-reward"));
                rewardDetailPanel.SetAccentColor(new Color(0.9450981f, 0.7176471f, 0.007843138f));
                rewardDetailPanel.SetMessage(I2.Loc.ScriptLocalization.ACTIVE_CODE_REWARD_MSG);
                rewardDetailPanel.SetChallengeName(rewardInformation.challengeName);
                rewardDetailPanel.SetLaurelsActive(false);
                rewardDetailPanel.SetDateText(rewardInformation.date);

                rewardDetailPanel.Show(false);
            }

        }

        public void ShowTopDailyReward(RewardInformation rewardInformation)
        {
            string title = I2.Loc.ScriptLocalization.TOURNAMENT_REWARD;
            string msg = I2.Loc.ScriptLocalization.REWARD_TOP_TOURNAMENT;

            rewardDetailPanel.SetTitle(title);

            rewardDetailPanel.SetCoin(rewardInformation.coinAmount);
            int rank = -1;
            int.TryParse(rewardInformation.rankDescription, out rank);
            rewardDetailPanel.SetBg(Background.Get("bg-reward"));
            string rankDescription = rewardInformation.rankDescription;
            switch (rank)
            {
                case 1:
                    rewardDetailPanel.SetBg(Background.Get("challenge-daily-winner"));
                    rewardDetailPanel.SetAccentColor(new Color(0.9450981f, 0.7176471f, 0.007843138f));
                    rankDescription = "1ST";
                    break;
                case 2:
                    rewardDetailPanel.SetBg(Background.Get("challenge-daily-2nd"));
                    rewardDetailPanel.SetAccentColor(new Color(0.7294118f, 0.7294118f, 0.7294118f));
                    rankDescription = "2ND";
                    break;
                case 3:
                    rewardDetailPanel.SetBg(Background.Get("challenge-daily-3rd"));
                    rewardDetailPanel.SetAccentColor(new Color(0.9294118f, 0.509804f, 0.05490196f));
                    rankDescription = "3RD";
                    break;
                default:
                    rewardDetailPanel.SetBg(Background.Get("challenge-daily-top10"));
                    rewardDetailPanel.SetAccentColor(new Color(1f, 1f, 1f));
                    rankDescription = String.Format("{0}TH", rank);
                    break;
            }
            rewardDetailPanel.SetMessage(msg);
            rewardDetailPanel.SetChallengeName(GetLocalizeTopDailyChallengeName(rewardInformation.challengeName));

            rewardDetailPanel.SetRankDescription(rankDescription);
            rewardDetailPanel.SetLaurelsActive(!string.IsNullOrEmpty(rankDescription));

            DateTime dateTime = Convert.ToDateTime(rewardInformation.date);
            rewardDetailPanel.SetDateText(dateTime.ToString("d"));

            rewardDetailPanel.Show(true, true);
            CoinManager.Instance.AddCoins(rewardInformation.coinAmount);
        }

        private void SetSpecificDetail(GSData d)
        {
            if (d == null)
            {
                rewardDetailPanel.SetLaurelsActive(false);
                rewardDetailPanel.SetChallengeName(string.Empty);
                rewardDetailPanel.SetRankDescription(string.Empty);
                rewardDetailPanel.SetDateText(string.Empty);
            }
            else
            {
                string challengeName = d.GetString("challengeName");
                rewardDetailPanel.SetChallengeName(challengeName);

                string rankDescription = d.GetString("rankDescription");
                rewardDetailPanel.SetRankDescription(rankDescription);
                rewardDetailPanel.SetLaurelsActive(!string.IsNullOrEmpty(rankDescription));

                string dateText = d.GetString("date");
                rewardDetailPanel.SetDateText(dateText);

                string accentColorHex = d.GetString("accentColor") ?? string.Empty;
                Color accent;
                if (ColorUtility.TryParseHtmlString(accentColorHex, out accent))
                {
                    rewardDetailPanel.SetAccentColor(accent);
                }
                else
                {
                    rewardDetailPanel.UseDefaultAccentColor();
                }
            }
        }

        private void OnDestroy()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
            TopLeaderBoardReward.topChallengeRewardListChanged -= OnTopChallengeRewardListChanged;
            TipsManager.Instance.UpdateTipsList -= OnTipListUpdated;
            UIReferences.UiReferencesUpdated -= UpdateReferences;
            CloudServiceManager.onPlayerDbSyncEnd -= OnPlayerDbSyncEnd;
        }



        private void OnPlayerDbSyncEnd()
        {
            if (tipsButtonInfo.IsUse)
                SetActiveTipsBtn();
        }

        private void Start()
        {
            controller.ShowIfNot();
            gameStateChangeTime = Time.time;
            if (settingButtonInfo.IsUse)
            {
                settingButtonInfo.button.onClick.AddListener(delegate
                {
                    settingGroup.Show();
                });
            }
            else
            {
                settingButtonInfo.button.gameObject.SetActive(false);
            }

            if (rewardButtonInfo.IsUse)
            {
                SetActiveRewardBtn();
                rewardButtonInfo.button.onClick.AddListener(delegate
                {
                    SideButtonRewardListener();
                });
            }
            else
            {
                rewardButtonInfo.button.gameObject.SetActive(false);
            }

            if (TutorialButtonInfo.IsUse)
            {
                TutorialButtonInfo.button.onClick.AddListener(delegate
                {
                    SceneLoadingManager.Instance.LoadTutorialScene();
                });
            }
            else
            {
                TutorialButtonInfo.button.gameObject.SetActive(false);
            }

            if (premiumSubscriptionInfo.button != null)
            {
                if (premiumSubscriptionInfo.IsUse)
                {
                    SetUIGuidePremiumSubscriptionBtn();
                    premiumSubscriptionInfo.button.onClick.AddListener(() =>
                    {
                        if (UIReferences.Instance.subscriptionDetailPanel != null)
                            UIReferences.Instance.subscriptionDetailPanel.Show();
                    });
                }
                else
                {
                    premiumSubscriptionInfo.button.gameObject.SetActive(false);
                }
            }

            if (earnCoinsPanelInfo.IsUse)
            {
                SetUIGuideEarnCoinBtn();
                earnCoinsPanelInfo.button.onClick.AddListener(delegate
                {
                    ECAsPanelController.Instance.Show();
                });
            }
            else
            {
                earnCoinsPanelInfo.button.gameObject.SetActive(false);
            }


            if (expLBButtonInfo.IsUse)
            {
                expLBButtonInfo.button.onClick.AddListener(delegate
                {
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        InGameNotificationPopup.Instance.confirmationDialog.Show(I2.Loc.ScriptLocalization.ATTENTION, I2.Loc.ScriptLocalization.NO_INTERNET_CONNECTION, I2.Loc.ScriptLocalization.OK, "", () => { });
                    }
                    else
                    {
                        leaderBoardScreen.SetChallengeId();
                        leaderBoardScreen.SetTitle(I2.Loc.ScriptLocalization.LEADERBOARD_TITLE.ToUpper());
                        leaderBoardScreen.Show();
                    }
                });
            }
            else
            {
                expLBButtonInfo.button.gameObject.SetActive(false);
            }

            if (tipsButtonInfo.IsUse)
            {
                SetActiveTipsBtn();
                tipsButtonInfo.button.onClick.AddListener(delegate
                {
                    tipsPanel.Show();
                });
            }
            else
            {
                tipsButtonInfo.button.gameObject.SetActive(false);
            }

            TipsManager.Instance.UpdateTipsList += OnTipListUpdated;
        }
        private void SetActiveRewardBtn()

        {
            rewardButtonInfo.button.gameObject.SetActive(TopLeaderBoardReward.Instance.rewards.Count > 0);
            if (!rewardButtonInfo.button.gameObject.activeSelf)
                return;
            if (UIGuide.instance && UIGuide.instance.guideList.FindIndex(item => item.SaveKey == UIGuideRewardsSaveKey) == -1)
            {
                CoroutineHelper.Instance.PostponeActionUntil(() =>
                {
                    if (rewardButtonInfo.button)
                    {
                        List<Image> maskedObject = StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].GetComponents<Image>().ToList();

                        List<Image> rewardButtonMaskedImage = maskedObject;
                        rewardButtonMaskedImage.AddRange(rewardButtonInfo.button.gameObject.GetComponentsInChildren<Image>().ToList());
                        rewardButtonMaskedImage.Add(rewardButtonInfo.button.gameObject.GetComponent<Image>());
                        UIGuide.UIGuideInformation rewardUIGuideInformation = new UIGuide.UIGuideInformation(UIGuideRewardsSaveKey, rewardButtonMaskedImage, StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject, rewardButtonInfo.button.gameObject, GameState.Prepare);
                        rewardUIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUIDE_REWARD;
                        rewardUIGuideInformation.clickableButton = rewardButtonInfo.button;

                        Vector3[] worldConners = new Vector3[4];
                        StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].rectTransform.GetWorldCorners(worldConners);
                        rewardUIGuideInformation.bubleTextWidth = 340;
                        rewardUIGuideInformation.transformOffset = new Vector3(0, (worldConners[1].y - worldConners[0].y) * 0.8f, 0);

                        UIGuide.instance.HighLightThis(rewardUIGuideInformation);
                    }
                }, () => (GameManager.Instance.GameState.Equals(GameState.Prepare) && (StoryLevelContainer.instance.HasShownFirstInstruction || UIGuide.instance.IsGuildeShown(StoryLevelContainer.instance.UIGuideFirstPuzzleSaveKey))));
            }
        }

        private void SetUIGuideLuckySpin()
        {
            if (UIGuide.instance && UIGuide.instance.guideList.FindIndex(item => item.SaveKey == UIGuideLuckySpinSaveKey) == -1 && !UIGuide.instance.IsGuildeShown(UIGuideLuckySpinSaveKey))
            {
                Debug.Log("SetUIGuideLuckySpin");
                CoroutineHelper.Instance.PostponeActionUntil(() =>
                {
                    CoroutineHelper.Instance.DoActionDelay(() =>
                    {
                        if (earnCoinsPanelInfo.button)
                        {
                            CoroutineHelper.Instance.PostponeActionUntil(() =>
                            {
                                List<Image> maskedObject = StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].GetComponents<Image>().ToList();
                                List<Image> ButtonMaskedImage = maskedObject;
                                ButtonMaskedImage.AddRange(earnCoinsPanelInfo.button.gameObject.GetComponentsInChildren<Image>().ToList());
                                ButtonMaskedImage.Add(earnCoinsPanelInfo.button.gameObject.GetComponent<Image>());
                                UIGuide.UIGuideInformation UIGuideInformation = new UIGuide.UIGuideInformation(UIGuideLuckySpinSaveKey, ButtonMaskedImage, StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject, earnCoinsPanelInfo.button.gameObject, GameState.Prepare);
                                UIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUIDE_LOW_ENERGY;
                                UIGuideInformation.clickableButton = earnCoinsPanelInfo.button;
                                Vector3[] worldConners = new Vector3[4];
                                StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].rectTransform.GetWorldCorners(worldConners);
                                UIGuideInformation.bubleTextWidth = 370;
                                UIGuideInformation.transformOffset = new Vector3(0, (worldConners[1].y - worldConners[0].y) * 0.8f, 0);
                                UIGuide.instance.HighLightThis(UIGuideInformation);
                            }, () => StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject.activeInHierarchy);

                        }
                    }, 0.2f);
                }, () => (StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject.activeInHierarchy && GameManager.Instance.GameState.Equals(GameState.Prepare) && (StoryLevelContainer.instance.HasShownFirstInstruction || UIGuide.instance.IsGuildeShown(StoryLevelContainer.instance.UIGuideFirstPuzzleSaveKey))));
            }
        }

        private void SetUIGuidePremiumSubscriptionBtn()
        {
            //If this player is already subscibed or buy full game then return
            if (InAppPurchaser.StaticIsSubscibed())
                return;
            if (InAppPurchaser.Instance.IsOneTimePurchased())
                return;

            if (UIGuide.instance && UIGuide.instance.guideList.FindIndex(item => item.SaveKey == UIGuidePremiumSubscription) == -1 && !UIGuide.instance.IsGuildeShown(UIGuidePremiumSubscription))
            {
                CoroutineHelper.Instance.PostponeActionUntil(() =>
                {
                    CoroutineHelper.Instance.DoActionDelay(() =>
                    {
                        if (premiumSubscriptionInfo.button)
                        {
                            CoroutineHelper.Instance.PostponeActionUntil(() =>
                            {
                                Debug.Log("Show subscription highlight");
                                List<Image> maskedObject = StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].GetComponents<Image>().ToList();
                                if (maskedObject == null)
                                    return;

                                List<Image> subscriptionButtonMaskedImage = maskedObject;
                                subscriptionButtonMaskedImage.AddRange(premiumSubscriptionInfo.button.gameObject.GetComponentsInChildren<Image>().ToList());
                                subscriptionButtonMaskedImage.Add(premiumSubscriptionInfo.button.gameObject.GetComponent<Image>());

                                UIGuide.UIGuideInformation PremiumButtonUIGuideInformation = new UIGuide.UIGuideInformation(
                                    UIGuidePremiumSubscription,
                                    subscriptionButtonMaskedImage,
                                    StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject,
                                    premiumSubscriptionInfo.button.gameObject,
                                    GameState.Prepare);
                                PremiumButtonUIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUI_PREMIUM_SUBSCRIPTION;
                                PremiumButtonUIGuideInformation.clickableButton = premiumSubscriptionInfo.button;

                                Vector3[] worldConners = new Vector3[4];
                                StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].rectTransform.GetWorldCorners(worldConners);
                                PremiumButtonUIGuideInformation.bubleTextWidth = 340;
                                PremiumButtonUIGuideInformation.transformOffset = new Vector3(0, (worldConners[1].y - worldConners[0].y) * 0.8f, 0);
                                UIGuide.instance.HighLightThis(PremiumButtonUIGuideInformation);
                            }, () => StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject.activeInHierarchy);
                        }
                    }, 0.1f);
                }, () => (StoryPuzzlesSaver.Instance.MaxNode >= 0
                && GameManager.Instance.GameState.Equals(GameState.Prepare)
                && (StoryLevelContainer.instance.HasShownFirstInstruction
                || UIGuide.instance.IsGuildeShown(StoryLevelContainer.instance.UIGuideFirstPuzzleSaveKey))));
            }
        }

        private void SetUIGuideEarnCoinBtn()
        {
            if (UIGuide.instance && UIGuide.instance.guideList.FindIndex(item => item.SaveKey == UIGuideTipsSaveKey) == -1 && !UIGuide.instance.IsGuildeShown(UiGuideEarnCoinSaveKey))
            {
                Debug.Log(StoryLevelContainer.instance.HasShownFirstInstruction);
                CoroutineHelper.Instance.PostponeActionUntil(() =>
                {
                    CoroutineHelper.Instance.DoActionDelay(() =>
                    {
                        if (tipsButtonInfo.button)
                        {
                            CoroutineHelper.Instance.PostponeActionUntil(() =>
                            {
                                List<Image> maskedObject = StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].GetComponents<Image>().ToList();
                                if (maskedObject == null)
                                    return;

                                List<Image> earnCoinButtonMaskedImage = maskedObject;
                                earnCoinButtonMaskedImage.AddRange(earnCoinsPanelInfo.button.gameObject.GetComponentsInChildren<Image>().ToList());
                                earnCoinButtonMaskedImage.Add(earnCoinsPanelInfo.button.gameObject.GetComponent<Image>());
                                UIGuide.UIGuideInformation EarnCoinUIGuideInformation = new UIGuide.UIGuideInformation(UiGuideEarnCoinSaveKey, earnCoinButtonMaskedImage, StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject, earnCoinsPanelInfo.button.gameObject, GameState.Prepare);
                                EarnCoinUIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUI_ECA;
                                EarnCoinUIGuideInformation.clickableButton = earnCoinsPanelInfo.button;
                                Vector3[] worldConners = new Vector3[4];
                                StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].rectTransform.GetWorldCorners(worldConners);
                                EarnCoinUIGuideInformation.bubleTextWidth = 340;
                                EarnCoinUIGuideInformation.transformOffset = new Vector3(0, (worldConners[1].y - worldConners[0].y) * 0.8f, 0);
                                UIGuide.instance.HighLightThis(EarnCoinUIGuideInformation);
                            }, () => StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject.activeInHierarchy);

                        }
                    }, 0.1f);
                }, () => (StoryPuzzlesSaver.Instance.MaxNode >= 0 && GameManager.Instance.GameState.Equals(GameState.Prepare) && (StoryLevelContainer.instance.HasShownFirstInstruction || UIGuide.instance.IsGuildeShown(StoryLevelContainer.instance.UIGuideFirstPuzzleSaveKey))));
            }
        }

        private void SetActiveTipsBtn()
        {
            //* Tip button is replaced by subscription button => no need to show it in side panel
            // tipsButtonInfo.button.gameObject.SetActive(TipsManager.Instance.availabaleTips.Count > 0);
            // if (!tipsButtonInfo.button.gameObject.activeSelf)
            //     return;
            /*
             * High light tips button after finished puzzle 1
            if (UIGuide.instance && UIGuide.instance.guideList.FindIndex(item => item.SaveKey == UIGuideTipsSaveKey) == -1)
            {
                Debug.Log(StoryLevelContainer.instance.HasShownFirstInstruction);
                CoroutineHelper.Instance.PostponeActionUntil(() =>
                {
                    CoroutineHelper.Instance.DoActionDelay(() =>
                    {
                        if (tipsButtonInfo.button)
                        {
                            CoroutineHelper.Instance.PostponeActionUntil(() =>
                            {
                                List<Image> maskedObject = StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].GetComponents<Image>().ToList();

                                List<Image> tipsButtonMaskedImage = maskedObject;
                                tipsButtonMaskedImage.AddRange(tipsButtonInfo.button.gameObject.GetComponentsInChildren<Image>().ToList());
                                tipsButtonMaskedImage.Add(tipsButtonInfo.button.gameObject.GetComponent<Image>());
                                UIGuide.UIGuideInformation tipsUIGuideInformation = new UIGuide.UIGuideInformation(UIGuideTipsSaveKey, tipsButtonMaskedImage, StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject, tipsButtonInfo.button.gameObject, GameState.Prepare);
                                tipsUIGuideInformation.message = "Click this button to see all unlocked tips.";
                                tipsUIGuideInformation.clickableButton = tipsButtonInfo.button;
                                Vector3[] worldConners = new Vector3[4];
                                StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].rectTransform.GetWorldCorners(worldConners);
                                tipsUIGuideInformation.bubleTextWidth = 340;
                                tipsUIGuideInformation.transformOffset = new Vector3(0, (worldConners[1].y - worldConners[0].y) * 0.8f, 0);
                                UIGuide.instance.HighLightThis(tipsUIGuideInformation);
                            }, () => StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject.activeInHierarchy);

                        }
                    }, 0.1f);
                }, () => (GameManager.Instance.GameState.Equals(GameState.Prepare) && (StoryLevelContainer.instance.HasShownFirstInstruction||UIGuide.instance.IsGuildeShown(StoryLevelContainer.instance.UIGuideFirstPuzzleSaveKey))));               
            }
            */
        }

        private void OnTipListUpdated()
        {
            if (tipsButtonInfo.IsUse)
                SetActiveTipsBtn();
        }

        private void Update()
        {
            if (GameManager.Instance.GameState == GameState.Prepare)
            {
                if (!UIReferences.Instance.gameUiPackSelectionUI.scroller.isScrolling)
                {
                    if (Time.time - lastHide > showDelay)
                        controller.ShowIfNot();
                }
                else
                {
                    controller.HideIfNot();
                    lastHide = Time.time;
                }

                if (earnCoinsPanelInfo.IsUse)
                {
                    //if ((UIReferences.Instance.overlayRollPanelUI.timeUntilNextSpinner < 0 && EnergyManager.Instance.CurrentEnergy <= EnergyManager.Instance.LowEnergyIndex
                    //    && UIReferences.Instance.overlayRollPanelUI.numberOfSpinLeft > 0) && energyExchangePanel.IsRewardedAdReady)
                    //{
                    //    luckySpinEnergyBtnInfo.button.gameObject.SetActive(true);
                    //    if (!wasShowLuckySpinAtFirstTime)
                    //    {
                    //        SetUIGuideLuckySpin();
                    //        wasShowLuckySpinAtFirstTime = true;
                    //    }
                    //}
                    //else
                    //{
                    //    luckySpinEnergyBtnInfo.button.gameObject.SetActive(false);
                    //}
                }

                /*
                if (expLBButton.gameObject.activeInHierarchy && UIGuide.instance && UIGuide.instance.guideList.FindIndex(item => item.SaveKey == UIGUideExpLeaderboardSaveKey) == -1)
                {
                    CoroutineHelper.Instance.PostponeActionUntil(() =>
                    {
                        if (expLBButton)
                        {
                            List<Image> maskedObject = StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].GetComponents<Image>().ToList();

                            List<Image> lbButtonMaskedImage = maskedObject;
                            lbButtonMaskedImage.AddRange(expLBButton.gameObject.GetComponentsInChildren<Image>().ToList());
                            lbButtonMaskedImage.Add(expLBButton.gameObject.GetComponent<Image>());
                            UIGuide.UIGuideInformation lbUIGuideInformation = new UIGuide.UIGuideInformation(UIGUideExpLeaderboardSaveKey, lbButtonMaskedImage, StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].gameObject, expLBButton.gameObject, GameState.Prepare);
                            lbUIGuideInformation.message = "Click this button to open the global leaderboard.";
                            lbUIGuideInformation.clickableButton = expLBButton;

                            Vector3[] worldConners = new Vector3[4];
                            StoryLevelContainer.instance.characterImgs[StoryLevelContainer.instance.scroller.SnapIndex].rectTransform.GetWorldCorners(worldConners);
                            lbUIGuideInformation.bubleTextWidth = 340;
                            lbUIGuideInformation.transformOffset = new Vector3(0, (worldConners[1].y - worldConners[0].y) * 0.8f, 0);

                            UIGuide.instance.HighLightThis(lbUIGuideInformation);
                        }
                    }, () => (GameManager.Instance.GameState.Equals(GameState.Prepare) && StoryLevelContainer.instance.HasShownFirstInstruction));
                }
                */
            }
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            gameStateChangeTime = Time.time;
            if (newState != GameState.Prepare)
            {
                controller.HideIfNot();
                lastHide = Time.time;
            }
        }

        private string GetLocalizeTopDailyChallengeName(string challengeName)
        {
            string[] list = challengeName.Split(' ');
            list[0] = Takuzu.Utilities.GetLocalizePackNameByName(list[0]);
            list[1] = list[1].Replace("TOURNAMENT", I2.Loc.ScriptLocalization.TOURNAMENT);
            return list[0] + " " + list[1];
        }
    }
}