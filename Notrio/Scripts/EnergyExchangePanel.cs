using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;
using System;
using GameSparks.Core;
using EasyMobile;

public class EnergyExchangePanel : OverlayPanel
{
    [SerializeField]
    private Color activeColor;

    [SerializeField]
    private Color inActiveColor;

    [Header("UI Preferences")]
    public RectTransform container;
    public Button exchangeBtn;
    public Button moreCoinBtn;

    public Button luckySpinnerBtn;
    public Button luckySpinnerInactiveBtn;
    public Button infiniteEnergyBtn;

    public Button closeBtn;
    public Text coinConsumText;
    public Text energyExchangeText;
    public Text timeTillNextSpinnerText;
    public Text luckyBtnText;
    public Text exchangeDescriptionText;
    public Image exchangeIconImg;
    public Sprite exchangeIconActive;
    public Sprite exchangeIconInActive;
    public int UseCoinAmount { get { return CloudServiceManager.Instance.appConfig.GetInt("energyExchangeCost") ?? 15; } }
    public int EnergyAmount { get { return CloudServiceManager.Instance.appConfig.GetInt("energyExchange") ?? 4; } }

    [HideInInspector]
    public HeaderUI headerUI;

    public OverlayGroupController controller;

    private int thresholdEnergyToHideRwBtn { set { PlayerPrefs.SetInt("ThresholdEnergyToHideRwBtn", value); } get { return PlayerPrefs.GetInt("ThresholdEnergyToHideRwBtn", -1); } }
    private int rewardedEnergyAmount { set { PlayerPrefs.SetInt("RewardedEnergyAmount", value); } get { return PlayerPrefs.GetInt("RewardedEnergyAmount", 4); } }

    [HideInInspector]
    public bool enableRewardEnergyBtn = false;

    private Coroutine checkUICR;

    private void Awake()
    {
        if (UIReferences.Instance != null)
        {
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
        CloudServiceManager.onConfigLoaded -= OnConfigLoaded;
        MatchingPanelController.ShowMatchingPanelEvent -= OnShowMatchingPanelEvent;
    }

    private void OnDisable()
    {
        StopCheckUI();
    }

    private void OnShowMatchingPanelEvent()
    {
        if (IsShowing)
            Hide();
    }

    private void UpdateReferences()
    {
        headerUI = UIReferences.Instance.gameUiHeaderUI;
    }

    private void Start()
    {
        enableRewardEnergyBtn = false;
        UpdateButtonUI();

        exchangeBtn.onClick.AddListener(delegate
        {
            if (CoinManager.Instance.Coins >= UseCoinAmount)
            {
                CoinManager.Instance.RemoveCoins(UseCoinAmount);
                EnergyManager.Instance.AddEnergy(EnergyAmount);
                CoinEnergyRewardAnimation.Instance.StartAnimation(exchangeBtn.transform as RectTransform, EnergyAmount, false);
            }
            else
            {
                headerUI.coinShop.Show();
            }
        });

        luckySpinnerBtn.onClick.AddListener(() =>
        {
            RewardDirectlyEnergy();
        });
        closeBtn.onClick.AddListener(delegate
        {
            Hide();
        });

        moreCoinBtn.onClick.AddListener(delegate
        {
            headerUI.coinShop.Show();
        });
        infiniteEnergyBtn.onClick.AddListener(delegate
        {
            UIReferences.Instance.subscriptionDetailPanel.Show();
        });
        CloudServiceManager.onConfigLoaded += OnConfigLoaded;
    }

    public void ActiveLuckySpin()
    {
        if (UIReferences.Instance.overlayRollPanelUI.timeUntilNextSpinner < 0)
        {
            GameManager.CurrentPurposeRewardAd = PurposeRewardAd.GetItem;
            CoinManager.luckySpinnerRewardCoin = false;
            CoinManager.onRewarded += OnAdReward;
            if (AdsFrequencyManager.Instance.showAdsInGame)
            {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                if (Application.internetReachability != NetworkReachability.NotReachable && Advertising.IsRewardedAdReady())
                {
                    Advertising.ShowRewardedAd();  
                }            
#else
                CoinManager.Instance.FakeAdReward();
#endif
            }
            else
            {
                CoinManager.Instance.FakeAdReward();
            }

        }
    }

    public void RewardDirectlyEnergy()
    {
        CoinManager.luckySpinnerRewardCoin = false;
        CoinManager.onRewarded += OnDirectAdReward;
        if (AdsFrequencyManager.Instance.showAdsInGame)
        {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                if (Application.internetReachability != NetworkReachability.NotReachable && Advertising.IsRewardedAdReady())
                {
                    Advertising.ShowRewardedAd();  
                }            
#else
            FakeDirectAdReward();
#endif
        }
        else
        {
            FakeDirectAdReward();
        }
    }

    private void OnDirectAdReward(RollingItem.RollingItemData itemData)
    {
        CoinManager.onRewarded -= OnDirectAdReward;
        UIReferences.Instance.overlayRollPanelUI.UpdateTimeUntilNextSpinnerOutEnergy();
        CoinEnergyRewardAnimation.Instance.StartAnimation(exchangeBtn.transform, rewardedEnergyAmount, false);
        EnergyManager.Instance.AddEnergy(rewardedEnergyAmount);
        enableRewardEnergyBtn = false;
    }

    private void FakeDirectAdReward()
    {
        CoinManager.onRewarded -= OnDirectAdReward;
        UIReferences.Instance.overlayRollPanelUI.UpdateTimeUntilNextSpinnerOutEnergy();
        CoinEnergyRewardAnimation.Instance.StartAnimation(exchangeBtn.transform, rewardedEnergyAmount, false);
        EnergyManager.Instance.AddEnergy(rewardedEnergyAmount);
        enableRewardEnergyBtn = false;
    }

    private void OnAdReward(RollingItem.RollingItemData itemData)
    {
        CoinManager.onRewarded -= OnAdReward;
        UIReferences.Instance.overlayRollPanelUI.itemData = itemData;
        UIReferences.Instance.overlayRollPanelUI.Show();
    }

    private void UpdateButtonUI()
    {
        luckyBtnText.text = string.Format(I2.Loc.ScriptLocalization.WATCH_ADS_FOR_ENERGY, rewardedEnergyAmount);
        luckySpinnerBtn.gameObject.SetActive(false);
        container.sizeDelta = new Vector2(container.sizeDelta.x, 340);

        if (UIReferences.Instance.overlayRollPanelUI.timeUntilNextWatchAd4Energy < 0 && UIReferences.Instance.overlayRollPanelUI.numberOfWatchAd4Energy > 0 && IsRewardedAdReady)
        {
            int amount = (thresholdEnergyToHideRwBtn < 0) ? EnergyManager.Instance.MaxEnergy : thresholdEnergyToHideRwBtn;
            if (EnergyManager.Instance.CurrentEnergy < amount)
            {
                luckySpinnerBtn.gameObject.SetActive(true);
                container.sizeDelta = new Vector2(container.sizeDelta.x, 510);
            }
            else
            {
                if (enableRewardEnergyBtn)
                {
                    luckySpinnerBtn.gameObject.SetActive(true);
                    container.sizeDelta = new Vector2(container.sizeDelta.x, 510);
                }
            }
        }
        bool canExchange = CoinManager.Instance.Coins >= UseCoinAmount ? true : false;
        exchangeBtn.interactable = canExchange;
        exchangeBtn.image.color = canExchange ? activeColor : inActiveColor;
        exchangeIconImg.sprite = canExchange ? exchangeIconActive : exchangeIconInActive;

        exchangeDescriptionText.gameObject.SetActive(canExchange);
        moreCoinBtn.gameObject.SetActive(!canExchange);
    }

    void OnForceOutInGameScene()
    {
        if (IsShowing)
            Hide();
    }

    void StartCheckUI()
    {
        if (checkUICR != null)
            StopCoroutine(checkUICR);
        checkUICR = StartCoroutine(CR_CheckUI());
    }

    void StopCheckUI()
    {
        if (checkUICR != null)
            StopCoroutine(checkUICR);
    }

    IEnumerator CR_CheckUI()
    {
        while (true)
        {
            if (UIReferences.Instance.overlayRollPanelUI.numberOfSpinLeft > 0
            && UIReferences.Instance.overlayRollPanelUI.timeUntilNextSpinner > 0)
                timeTillNextSpinnerText.text = String.Format("{0}", new TimeSpan(0, 0, UIReferences.Instance.overlayRollPanelUI.timeUntilNextSpinner));
            else
            {
                timeTillNextSpinnerText.text = I2.Loc.ScriptLocalization.Unavailable_;
            }

            UpdateButtonUI();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
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

    public override void Show()
    {
        if (CoinManager.Instance.Coins < UseCoinAmount)
        {
            CoinManager.InsufficentCoinsReason = "Energy_Exchange";
            CoinManager.InSufficentCoins = true;
        }
        else
        {
            CoinManager.InsufficentCoinsReason = "";
            CoinManager.InSufficentCoins = false;
        }
        coinConsumText.text = UseCoinAmount.ToString();
        energyExchangeText.text = EnergyAmount.ToString();
        controller.ShowIfNot();
        IsShowing = true;
        transform.BringToFront();
        onPanelStateChanged(this, true);
        StartCheckUI();
    }

    public override void Hide()
    {
        StopCheckUI();
        controller.HideIfNot();
        IsShowing = false;
        onPanelStateChanged(this, false);
    }

    private void OnConfigLoaded(GSData config)
    {
        int? thresholdEnergy = config.GetInt("thresholdEnergyToHideRewardBtn");
        if (thresholdEnergy.HasValue)
        {
            thresholdEnergyToHideRwBtn = thresholdEnergy.Value;
        }

        int? energyAmount = config.GetInt("rewardedEnergyAmountAfterWatchAds");
        if (energyAmount.HasValue)
        {
            rewardedEnergyAmount = energyAmount.Value;
        }
    }
}
