using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyMobile;
using UnityEngine.UI;
using Pinwheel;
using System;

namespace Takuzu
{
    public class CoinShopUI : OverlayPanel
    {
        public UiGroupController controller;
        public Button closeButton;

        [Header("Free coin")]
        public Text timeTillNextSpinnerText;
        public Button luckySpinnerBtn;
        public Button luckySpinnerInactiveBtn;

        [Header("Background")]
        public Image fullBg;
        public List<Image> framentedBg;
		[HideInInspector]
        public RollPanelUI rollPanelUI;

        public override void Show()
        {
            UpdateFreeCoinBtns();
            controller.ShowIfNot();
            IsShowing = true;
            transform.BringToFront();
            onPanelStateChanged(this, true);
        }

        private void UpdateFreeCoinBtns()
        {
            //luckySpinnerBtn.gameObject.SetActive((UIReferences.Instance.overlayRollPanelUI.timeUntilNextSpinner < 0 && UIReferences.Instance.overlayRollPanelUI.numberOfSpinLeft > 0) && IsRewardedAdReady);
            //luckySpinnerInactiveBtn.gameObject.SetActive(!luckySpinnerBtn.gameObject.activeSelf);
        }

        public override void Hide()
        {
            controller.HideIfNot();
            IsShowing = false;
            onPanelStateChanged(this, false);
        }

        private void Awake()
        {
			if(UIReferences.Instance!=null){
				UpdateReferences();
			}
			UIReferences.UiReferencesUpdated += UpdateReferences;
            GameManager.ForceOutInGamScene += OnForceOutInGameScene;
            MatchingPanelController.ShowMatchingPanelEvent += OnShowMatchingPanelEvent;
        }

        private void OnDestroy()
        {
            UIReferences.UiReferencesUpdated -= UpdateReferences;
            GameManager.ForceOutInGamScene -= OnForceOutInGameScene;
            MatchingPanelController.ShowMatchingPanelEvent -= OnShowMatchingPanelEvent;
        }

        private void UpdateReferences()
		{
			rollPanelUI = UIReferences.Instance.overlayRollPanelUI;
		}

        private void OnShowMatchingPanelEvent()
        {
            if (IsShowing)
                Hide();
        }

        void OnForceOutInGameScene()
        {
            if (IsShowing)
                Hide();
        }

        public void Start()
        {
            luckySpinnerBtn.onClick.AddListener(() =>
            {
                if (UIReferences.Instance.overlayRollPanelUI.timeUntilNextSpinner < 0)
                {
                    CoinManager.luckySpinnerRewardCoin = true;
                    CoinManager.onRewarded += OnAdReward;
                    if (AdsFrequencyManager.Instance.showAdsInGame)
                    {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                        if (Application.internetReachability != NetworkReachability.NotReachable && Advertising.IsRewardedAdReady())
                            Advertising.ShowRewardedAd();              
#else
                        CoinManager.Instance.FakeAdReward();
#endif
                    }
                    else
                    {
                        CoinManager.Instance.FakeAdReward();
                    }
                };
            });
            closeButton.onClick.AddListener(delegate
                {
                    Hide();
                });
        }
        private void OnAdReward(RollingItem.RollingItemData itemData)
        {
            CoinManager.onRewarded -= OnAdReward;
            UIReferences.Instance.overlayRollPanelUI.itemData = itemData;
            UIReferences.Instance.overlayRollPanelUI.Show();
        }
        private void Update()
        {
            if (UIReferences.Instance.overlayRollPanelUI.numberOfSpinLeft > 0 
                && UIReferences.Instance.overlayRollPanelUI.timeUntilNextSpinner > 0)
                timeTillNextSpinnerText.text = String.Format("{0}", new TimeSpan(0, 0, UIReferences.Instance.overlayRollPanelUI.timeUntilNextSpinner));
            else
            {
                timeTillNextSpinnerText.text = I2.Loc.ScriptLocalization.Unavailable_;
            }
            UpdateFreeCoinBtns();
        }

        public bool IsRewardedAdReady
        {
            get
            {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR

            if (Application.internetReachability == NetworkReachability.NotReachable)
                return false;

            if (!AdsFrequencyManager.Instance.IsAppropriateFrequencyForVideo())
                return false;

            if (!Advertising.IsRewardedAdReady())
                return false;

            return true;
#else
                return true;
#endif
            }
        }

        public void UseFullBackground()
        {
            fullBg.gameObject.SetActive(true);
            for (int i = 0; i < framentedBg.Count; ++i)
            {
                framentedBg[i].gameObject.SetActive(false);
            }
        }

        public void UseFragmentedBackground()
        {
            fullBg.gameObject.SetActive(false);
            for (int i = 0; i < framentedBg.Count; ++i)
            {
                framentedBg[i].gameObject.SetActive(true);
            }
        }
    }
}