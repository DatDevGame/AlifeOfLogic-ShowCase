using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameSparks.Api.Responses;
using GameSparks.Core;
using GameSparks.Api.Messages;
using Pinwheel;

namespace Takuzu
{
    public class RewardUI : MonoBehaviour
    {
        public Button rewardButton;
        public CanvasGroup rewardButtonGroup;
        public GameObject rewardButtonBadge;
        public Text badgeNumber;
        public AnimController[] badgeAnims;
        public RewardDetailPanel rewardDetailPanel;
        public string defaultBgName;
        public string tutorialRewardBgName;
        public string loginRewardBgName;
        public string watchingAdRewardBgName;
        public string dailyChallengeBgName;
        public string weeklyChallengeBgName;
        public List<string> bgNames;
        public float challengeRewardShowDelay;
        public bool willShowPanel;
        private List<GSData> rewards;

        [Header("Reward button show/hide anim")]
        public bool isRewardButtonShowing;
        public bool isBadgeShowing;
        public float sizingSpeed;

        private Vector2 rewardButtonOriginalSize;
        private Vector2 badgeOriginalSize;
        private RectTransform rewardButtonRt;
        private RectTransform badgeRt;

        private void Awake()
        {
            rewardButtonRt = rewardButton.GetComponent<RectTransform>();
            rewardButtonOriginalSize = rewardButtonRt.sizeDelta;

            badgeRt = rewardButtonBadge.GetComponent<RectTransform>();
            badgeOriginalSize = badgeRt.sizeDelta;

            rewardButton.onClick.AddListener(delegate
                {
                    //listRewardPanel.Show();
                    if (rewards == null || rewards.Count == 0)
                    {
                        return;
                    }
                    GSData r = rewards[0];
                    rewards.RemoveAt(0);
                    ClaimReward(r);
                });
        }

        private void Start()
        {
            CloudServiceManager.onLoginGameSpark += OnLoginGameSparks;
            NotificationManager.onNotificationReceived += OnNotificationReceived;
            //CoinManager.onLoginReward += OnLoginReward;
            CoinManager.onWatchingAdReward += OnWatchingAdReward;
            //CoinManager.onDailyChallengeReward += OnDailyChallengeReward;
            //CoinManager.onWeeklyChallengeReward += OnWeeklyChallengeReward;
            //CoinManager.onFinishTutorialFirstTimeReward += OnTutorialReward;
            //SocialManager.onFbLogout += OnFbLogout;
            OverlayPanel.onPanelStateChanged += OnPanelStateChanged;
        }

        private void OnDestroy()
        {
            Debug.Log("DEstroy");
            CloudServiceManager.onLoginGameSpark -= OnLoginGameSparks;
            NotificationManager.onNotificationReceived -= OnNotificationReceived;
            //CoinManager.onLoginReward -= OnLoginReward;
            CoinManager.onWatchingAdReward -= OnWatchingAdReward;
            //CoinManager.onDailyChallengeReward -= OnDailyChallengeReward;
            //CoinManager.onWeeklyChallengeReward -= OnWeeklyChallengeReward;
            //CoinManager.onFinishTutorialFirstTimeReward -= OnTutorialReward;
            //SocialManager.onFbLogout -= OnFbLogout;
            OverlayPanel.onPanelStateChanged -= OnPanelStateChanged;
        }

        private void OnPanelStateChanged(OverlayPanel p, bool isShowing)
        {
            if (p == rewardDetailPanel && !isShowing)
            {
                CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    Background.Unload(loginRewardBgName);
                    Background.Unload(watchingAdRewardBgName);
                    for (int i = 0; i < bgNames.Count; ++i)
                    {
                        Background.Unload(bgNames[i]);
                    }
                },
                rewardDetailPanel.controller.MaxDuration + 0.5f);
            }
        }

        private void OnLoginGameSparks(AuthenticationResponse response)
        {
            if (response.HasErrors)
                return;
            CloudServiceManager.Instance.RequestReward((r) =>
                {
                    if (r.HasErrors)
                    {
                        return;
                    }
                    rewards = r.ScriptData.GetGSDataList("rewards");
                    rewardButtonBadge.SetActive(true);
                    if (rewardButtonBadge.activeInHierarchy)
                    {
                        for (int i = 0; i < badgeAnims.Length; ++i)
                        {
                            if (!badgeAnims[i].isPlaying)
                                badgeAnims[i].Play();
                        }
                    }
                });
        }

        private void OnFbLogout()
        {
            rewards = new List<GSData>();
        }

        private void Update()
        {
            rewardButtonGroup.interactable = HasUnclaimedReward();
            SetBadgeActive(HasUnclaimedReward());
            SetRewardButtonActive(HasUnclaimedReward());
            if (rewardButtonBadge.activeInHierarchy && isBadgeShowing)
            {
                for (int i = 0; i < badgeAnims.Length; ++i)
                {
                    if (!badgeAnims[i].isPlaying)
                        badgeAnims[i].Play();
                }
            }

            int badgeNum = rewards != null ? rewards.Count : 0;
            badgeNumber.text = badgeNum > 0 ? badgeNum.ToString() : "";


            rewardButtonRt.sizeDelta = Vector2.MoveTowards(
                rewardButtonRt.sizeDelta,
                isRewardButtonShowing ? rewardButtonOriginalSize : Vector2.zero,
                sizingSpeed);

            badgeRt.sizeDelta = Vector2.MoveTowards(
                badgeRt.sizeDelta,
                isBadgeShowing ? badgeOriginalSize : Vector2.zero,
                sizingSpeed);

        }

        public void SetBadgeActive(bool active)
        {
            isBadgeShowing = active;
        }

        public void SetRewardButtonActive(bool active)
        {
            isRewardButtonShowing = active;
        }

        public bool HasUnclaimedReward()
        {
            return rewards != null && rewards.Count > 0;
        }

        private void OnNotificationReceived(string msg, Dictionary<string, object> data)
        {
            if (!data.ContainsKey("rewardId"))
                return;
            GSData d = new GSData(data);
            if (rewards == null)
                rewards = new List<GSData>();
            rewards.Add(d);
            if (rewardButtonBadge.activeInHierarchy)
            {
                for (int i = 0; i < badgeAnims.Length; ++i)
                {
                    if (!badgeAnims[i].isPlaying)
                        badgeAnims[i].Play();
                }
            }
        }

#if UNITY_EDITOR
        public void OnReceiveFakeRewardData(GSData d)
        {
            if (rewards == null)
                rewards = new List<GSData>();
            rewards.Add(d);
            if (rewardButtonBadge.activeInHierarchy)
            {
                for (int i = 0; i < badgeAnims.Length; ++i)
                {
                    if (!badgeAnims[i].isPlaying)
                        badgeAnims[i].Play();
                }
            }
        }
#endif

        public void ShowDetailPanel(GSData d)
        {
            string title = d.GetString("uiTitle");
            if (string.IsNullOrEmpty(title))
                title = "CONGRATULATIONS!";
            string content = d.GetString("uiBody");
            long amount = d.GetLong("coinAmount").GetValueOrDefault(0);
            string bgName = d.GetString("bgName");
            Sprite icon = Background.Get(bgName);

            rewardDetailPanel.SetTitle(title);
            rewardDetailPanel.SetMessage(content);
            rewardDetailPanel.SetCoin(amount);
            rewardDetailPanel.SetBg(icon);
            SetSpecificDetail(d);
            rewardDetailPanel.Show();
        }

        private void OnLoginReward(int coin)
        {
            string title = "CONNECTED";
            string msg = string.Format("Thanks for connecting,\n{0}.\nHere's a small gift for you!", CloudServiceManager.playerName ?? "");

            rewardDetailPanel.SetTitle(title);
            rewardDetailPanel.SetMessage(msg);
            rewardDetailPanel.SetCoin(coin);
            rewardDetailPanel.SetBg(Background.Get(loginRewardBgName));
            SetSpecificDetail(null);
            rewardDetailPanel.Show(false);
        }

        private void OnWatchingAdReward(int coin)
        {
            string title = "THANKS";
            string msg = string.Format("Thank you for your time{0}.\nHere's a small gift for you!",
                             string.IsNullOrEmpty(CloudServiceManager.playerName) ? string.Empty : (",\n" + CloudServiceManager.playerName));

            rewardDetailPanel.SetTitle(title);
            rewardDetailPanel.SetMessage(msg);
            rewardDetailPanel.SetCoin(coin);
            rewardDetailPanel.SetBg(Background.Get(watchingAdRewardBgName));
            SetSpecificDetail(null);
            rewardDetailPanel.Show(false);
        }

        private void OnDailyChallengeReward(int coin)
        {
            string title = "AWESOME!";
            string msg = "You've completed the daily challenge, here's your reward!";

            rewardDetailPanel.SetTitle(title);
            rewardDetailPanel.SetMessage(msg);
            rewardDetailPanel.SetCoin(coin);
            rewardDetailPanel.SetBg(Background.Get(dailyChallengeBgName));
            SetSpecificDetail(null);

            string dateStr = GetCreationDate(PuzzleManager.currentPuzzleId);
            string difficulty = Utilities.GetDifficultyDisplayName(PuzzleManager.currentLevel);
            string sizeStr = string.Format("{0}x{0}", (int)PuzzleManager.currentSize);
            //rewardDetailPanel.SetChallengeDetail("DAILY CHALLENGE", dateStr, difficulty, sizeStr);
            willShowPanel = true;
            Debug.Log("Reward");
            CoroutineHelper.Instance.PostponeActionUntil(() =>
                {
                    CoroutineHelper.Instance.DoActionDelay(() =>
                        {
                            rewardDetailPanel.Show(true, true);
                            willShowPanel = false;
                        }, challengeRewardShowDelay);
                },
                () => GameManager.Instance.GameState == GameState.Prepare);
        }

       

        private void OnWeeklyChallengeReward(int coin)
        {
            string title = "EXCELLENT!";
            string msg = "You've completed the weekly challenge, here's your reward!";

            rewardDetailPanel.SetTitle(title);
            rewardDetailPanel.SetMessage(msg);
            rewardDetailPanel.SetCoin(coin);
            rewardDetailPanel.SetBg(Background.Get(weeklyChallengeBgName));
            SetSpecificDetail(null);

            string dateStr = GetCreationDate(PuzzleManager.currentPuzzleId);
            string difficulty = Utilities.GetDifficultyDisplayName(PuzzleManager.currentLevel);
            string sizeStr = string.Format("{0}x{0}", (int)PuzzleManager.currentSize);
            //rewardDetailPanel.SetChallengeDetail("WEEKLY CHALLENGE", dateStr, difficulty, sizeStr);
            willShowPanel = true;
            CoroutineHelper.Instance.PostponeActionUntil(() =>
                {
                    CoroutineHelper.Instance.DoActionDelay(() =>
                        {
                            rewardDetailPanel.Show(true, true);
                            willShowPanel = false;
                        }, challengeRewardShowDelay);
                },
                () => GameManager.Instance.GameState == GameState.Prepare);
        }

        private void OnTutorialReward(int coin)
        {
            string title = "WELCOME";
            string msg = "Welcome to Takuzu!\nWe have a small gift for you!";

            rewardDetailPanel.SetTitle(title);
            rewardDetailPanel.SetMessage(msg);
            rewardDetailPanel.SetCoin(coin);
            rewardDetailPanel.SetBg(Background.Get(tutorialRewardBgName));
            rewardDetailPanel.SetDateText(string.Empty);
            SetSpecificDetail(null);
            //the panel is delayed in CoinManager class
            rewardDetailPanel.Show(false);
        }

        private void ClaimReward(GSData d)
        {
            rewardButton.interactable = false;
            string rewardId = d.GetString("rewardId");
            if (string.IsNullOrEmpty(rewardId))
                return;
            CloudServiceManager.Instance.ClaimReward(rewardId, (response) =>
                {
                    rewardButton.interactable = true;
                    if (response.HasErrors)
                        return;
                    bool? succeed = response.ScriptData.GetBoolean("claimSuccess");
                    if (succeed.HasValue && succeed.Value == true)
                    {
                        int? amount = response.ScriptData.GetInt("coinAmount");
                        if (amount.HasValue)
                        {
                            CoinManager.Instance.AddCoins(amount.Value);
                            if (d.GetBoolean("openDetailPanel").GetValueOrDefault(false))
                            {
                                ShowDetailPanel(d);
                            }
                        }
                    }
                });
        }

        private string GetCreationDate(string id)
        {
            string[] s = id.Split('-');
            int y = int.Parse(s[1]);
            int m = int.Parse(s[2]);
            int d = int.Parse(s[3]);
            System.DateTime date = new System.DateTime(y, m, d);

            return date.ToShortDateString();
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
    }
}