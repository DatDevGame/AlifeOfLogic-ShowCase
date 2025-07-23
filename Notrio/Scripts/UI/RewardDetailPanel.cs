using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;
using System;

namespace Takuzu
{
    public class RewardDetailPanel : OverlayPanel
    {
        public UiGroupController controller;
        public Text title;
        public Text coinAmount;
        public Image coinIcon;
        public Text message;
        public Button shareButton;
        public Button closeButton;
        public GameObject flyingCoin;
		[HideInInspector]
		public OverlayEffect overlayEffect;
		[HideInInspector]
        public CoinDisplayer menuCoinDisplayer;
		[HideInInspector]
        public CoinDisplayer ingameCoinDisplayer;
        public Text rankDescription;
        public Text challengeName;
        public Text dateText;
        public Image laurels;
        public Image background;
        public PositionAnimation messageGroupAnim;
		[HideInInspector]
        public Canvas canvas;
		[HideInInspector]
        public CameraController camController;
        public GameObject challengeDetailGroup;
        public Color defaultAccentColor;
        public Graphic[] graphicsToApplyAccentColor;

        private int rewardedCoin;

		private void Awake() 
        {
			if(UIReferences.Instance!=null){
				UpdateReferences();
			};
			UIReferences.UiReferencesUpdated += UpdateReferences;
            CloudServiceManager.onPlayerDbSyncEnd += OnLoginReward;
            CoinManager.onDailyChallengeReward += OnDailyChallengeReward;
            CoinManager.onInviatationCodeVerifiedSuccessfully += OnInvivitationCodeVerifiedSuccessfully;
            //ConfirmPolicyPanelController.CheckConfirmPolicyComplete += OnCheckConfirmPolicyComplete;
            CoinManager.onFinishTutorialFirstTimeReward += OnTutorialCompleteFirstTime;
            //ECAsPanelController.onGameShared += OnShareGameURL;
        }

        private void UpdateReferences()
		{
			canvas = UIReferences.Instance.overlayCanvas;
			camController = UIReferences.Instance.mainCameraController;
			overlayEffect = UIReferences.Instance.overlayEffect;
			ingameCoinDisplayer = UIReferences.Instance.gameUiPlayUI.CoinDisplayer;
			menuCoinDisplayer = UIReferences.Instance.gameUiHeaderUI.CoinDisplayer;
		}

		private void OnDestroy() {
			UIReferences.UiReferencesUpdated -= UpdateReferences;
            CloudServiceManager.onPlayerDbSyncEnd -= OnLoginReward;
            CoinManager.onDailyChallengeReward -= OnDailyChallengeReward;
            CoinManager.onInviatationCodeVerifiedSuccessfully -= OnInvivitationCodeVerifiedSuccessfully;
            //ConfirmPolicyPanelController.CheckConfirmPolicyComplete -= OnCheckConfirmPolicyComplete;
            CoinManager.onFinishTutorialFirstTimeReward -= OnTutorialCompleteFirstTime;
            //ECAsPanelController.onGameShared -= OnShareGameURL;
        }

        private void OnTutorialCompleteFirstTime(int v)
        {
            if (PlayerDb.GetInt(PlayerDb.FINISH_TUTORIAL_REWARD_KEY, 0) == 1)
            {
                Debug.Log("Show tutorial reward");
                OnTutorialReward();
            }
        }

        private void OnInvivitationCodeVerifiedSuccessfully()
        {
            PlayerPrefs.SetInt("ENTER_GAME_CODE_KEY", 1);
            ECAsPanelController.onCodeRewardClaimed(ECAsPanelController.RewardType.RedeemCodeReward);
            string title = I2.Loc.ScriptLocalization.ACTIVE_CODE_REWARD_TITLE;
            string msg = I2.Loc.ScriptLocalization.ACTIVE_CODE_REWARD_MSG;
            int rewardAmount = CoinManager.Instance.rewardProfile.rewardOnEnterCode;
            CoinManager.Instance.AddCoins(rewardAmount);
            SetTitle(title);
            SetMessage(msg);
            SetCoin(rewardAmount);
            SetBg(Background.Get("bg-reward"));
            Show(false);
        }

        //private void OnShareGameURL(){
        //    string title = "Game Shared Reward";
        //    string msg = "Invitation reward msg";
        //    int rewardAmount = CoinManager.Instance.rewardProfile.rewardOnShareCode;
        //    CoinManager.Instance.AddCoins(rewardAmount);
        //    SetTitle(title);
        //    SetMessage(msg);
        //    SetCoin(rewardAmount);
        //    SetBg(Background.Get("bg-reward"));
        //    Show(false);
        //}

        private string GetCreationDate(string id)
        {
            string[] s = id.Split('-');
            int y = int.Parse(s[1]);
            int m = int.Parse(s[2]);
            int d = int.Parse(s[3]);
            System.DateTime date = new System.DateTime(y, m, d);

            return date.ToShortDateString();
        }

        private void OnDailyChallengeReward(int coin)
        {
            string title = I2.Loc.ScriptLocalization.AWESOME;
            string msg = I2.Loc.ScriptLocalization.REWARD_COMPLETE_TOURNAMENT;

            SetTitle(title);
            SetMessage(msg);
            SetCoin(coin);
            SetBg(Background.Get("bg-reward"));

            string dateStr = GetCreationDate(PuzzleManager.currentPuzzleId);
            string difficulty = Utilities.GetDifficultyDisplayName(PuzzleManager.currentLevel);
            string sizeStr = string.Format("{0}x{0}", (int)PuzzleManager.currentSize);
            Debug.Log("Reward");
            CoroutineHelper.Instance.PostponeActionUntil(() =>
            {
                CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    Show(false);
                }, 0.2f);
            },
                () => GameManager.Instance.GameState == GameState.Prepare);
        }

        public void Show(bool showShareButton, bool showChallengeDetail = false)
        {
            CoinDisplayer coinDisplayer = GameManager.Instance.GameState == GameState.Prepare ?
                menuCoinDisplayer : ingameCoinDisplayer;
            coinDisplayer.offset -= rewardedCoin;

            //block the button to wait for message animation to complete
            closeButton.interactable = false;
            CoroutineHelper.Instance.DoActionDelay(
                () =>
                {
                    closeButton.interactable = true;
                }, 3);
            CoroutineHelper.Instance.DoActionDelay(
                () =>
                {
                    coinAmount.SetAllDirty();
                    coinAmount.gameObject.SetActive(true);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(coinAmount.rectTransform.parent as RectTransform);
                }, 0);

            controller.ShowIfNot();
            IsShowing = true;
            shareButton.gameObject.SetActive(showShareButton);
            ShowChallengeDetail(showChallengeDetail);
            transform.BringToFront();
            onPanelStateChanged(this, true);
            messageGroupAnim.Play(AnimConstant.IN);
            StartCoroutine(CR_DelayPlayConfetti());
        }

        IEnumerator CR_DelayPlayConfetti()
        {
            yield return new WaitForSeconds(0.5f);
            SoundManager.Instance.PlaySound(SoundManager.Instance.highRewarded, true);
            SoundManager.Instance.PlaySoundDelay(0.5f, SoundManager.Instance.confetti, true);
            camController.PlayLeavesParticle(3);
        }

        public override void Hide()
        {
            controller.HideIfNot();
            IsShowing = false;
            onPanelStateChanged(this, false);
        }
        
        private void Start()
        {
            closeButton.onClick.AddListener(delegate
            {
                PlayCoinFlyingAnimAndHide();
                TopLeaderBoardReward.RefreshRewards();
                //Hide();
            });

            shareButton.onClick.AddListener(delegate
            {
                SocialManager.Instance.NativeShareScreenshot();
            });
        }
        
        public void SetTitle(string s)
        {
            title.text = s.ToUpper();
        }

        public void SetBg(Sprite s)
        {
            background.sprite = s;
        }

        public void SetCoin(long amount)
        {
            coinAmount.text = amount.ToString();
            rewardedCoin = (int)amount;
            coinAmount.gameObject.SetActive(false);
        }

        public void SetMessage(string s)
        {
            message.text = s;
        }

        public void SetDateText(string d)
        {
            dateText.text = d;
        }

        private void ShowChallengeDetail(bool show)
        {
            challengeDetailGroup.SetActive(show);
        }

        private void PlayCoinFlyingAnimAndHide()
        {
            StartCoroutine(CrPlayCoinFlyingAnimAndHide());
        }

        private IEnumerator CrPlayCoinFlyingAnimAndHide()
        {
            Hide();
            yield return null;
            int coin = rewardedCoin;
            overlayEffect.StartPointMarker.position = coinIcon.transform.position;
            Vector2 startPoint = overlayEffect.StartPointMarker.anchoredPosition;
            overlayEffect.endPointMarker.position = GameManager.Instance.GameState == GameState.Prepare ?
                menuCoinDisplayer.icon.transform.position :
                ingameCoinDisplayer.icon.transform.position;
            Vector2 endPoint =	overlayEffect.endPointMarker.anchoredPosition;

            int count = UnityEngine.Random.Range(5, 10 + 1) + coin / 20;
            for (int i = 0; i < count; ++i)
            {
                GameObject g = Instantiate(flyingCoin);
                g.transform.SetParent(overlayEffect.FlyingCoinRoot, false);
                g.transform.position = startPoint;
                RectTransform rt = g.transform as RectTransform;
                float size = UnityEngine.Random.Range(25, 60);
                rt.sizeDelta = size * Vector2.one;
                CoinFlyingEffect e = g.GetComponent<CoinFlyingEffect>();
                e.startPoint = startPoint;
                e.endPoint = endPoint;
                e.UpdateNormal();

                yield return null;
                yield return null;
            }
            yield return new WaitForSeconds(1f);
            CoinDisplayer coinDisplayer = GameManager.Instance.GameState == GameState.Prepare ?
                menuCoinDisplayer : ingameCoinDisplayer;
            float animMaxDuration = 3;
            int minCoinOffset = UnityEngine.Random.Range(1, 2 + 1);
            int coinOffset = Mathf.Max(minCoinOffset, (int)(coin * Time.smoothDeltaTime / animMaxDuration));
            CoroutineHelper.Instance.RepeatUntil(
                () =>
                {
                    coinDisplayer.offset = (int)Mathf.MoveTowards(coinDisplayer.offset, 0, coinOffset);
                    SoundManager.Instance.PlaySound(SoundManager.Instance.tick, true);
                },
                0,
                () => coinDisplayer.offset == 0);

        }

        public void SetAccentColor(Color c)
        {
            for (int i=0;i<graphicsToApplyAccentColor.Length;++i)
            {
                if (graphicsToApplyAccentColor[i] != null)
                    graphicsToApplyAccentColor[i].color = c;
            }
        }

        public void UseDefaultAccentColor()
        {
            SetAccentColor(defaultAccentColor);
        }

        public void SetLaurelsActive(bool active)
        {
            laurels.gameObject.SetActive(active);
        }

        public void SetRankDescription(string rankDescription)
        {
            this.rankDescription.text = rankDescription;
        }

        public void SetChallengeName(string challengeName)
        {
            this.challengeName.text = challengeName;
        }

        private void OnLoginReward()
        {
            if (CloudServiceManager.isGuest)
                return;
            if (PlayerDb.HasKey("LOGIN-REWARD") || PlayerPrefs.HasKey("LOGIN-REWARD"))
                return;
            PlayerDb.SetInt("LOGIN-REWARD", 1);
            PlayerPrefs.SetInt("LOGIN-REWARD", 1);
            PlayerDb.Save();
            string title = I2.Loc.ScriptLocalization.CONNECTED;
            string msg = string.Format(I2.Loc.ScriptLocalization.REWARD_LOGIN, CloudServiceManager.playerName ?? "");
            CoinManager.Instance.AddCoins(CoinManager.Instance.rewardProfile.rewardOnFbLogin);
            SetTitle(title);
            SetMessage(msg);
            SetCoin(CoinManager.Instance.rewardProfile.rewardOnFbLogin);
            SetBg(Background.Get("bg-reward"));
            Show(false);
        }

        private void OnTutorialReward()
        {
            if (PlayerPrefs.HasKey("TUTORIAL-REWARD"))
                return;
            PlayerPrefs.SetInt("TUTORIAL-REWARD", 1);
            string title = I2.Loc.ScriptLocalization.WELCOME;
            string msg = string.Format(I2.Loc.ScriptLocalization.REWARD_COMPLETE_TUTORIAL);

            SetTitle(title);
            SetMessage(msg);
            CoinManager.Instance.AddCoins(CoinManager.Instance.rewardProfile.rewardOnFinishTutorialFirstTime);
            SetCoin(CoinManager.Instance.rewardProfile.rewardOnFinishTutorialFirstTime);
            SetBg(Background.Get("bg-reward"));
            Show(false);
        }

        public override void Show()
        {
            Show(true);
        }

#if UNITY_EDITOR
        GUIStyle style;
        private void OnDrawGizmos()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                return;
            if (Camera.current != null && Vector3.Distance(Camera.current.transform.position, background.transform.position) < 100)
            {
                if (style == null)
                {
                    style = new GUIStyle(UnityEditor.EditorStyles.miniLabel);
                }

                style.normal.textColor = Color.magenta;
                style.alignment = TextAnchor.MiddleCenter;
                UnityEditor.Handles.Label(background.transform.position, "<Background image\nis assigned\nat runtime>", style);
            }
        }    
#endif
    }
}