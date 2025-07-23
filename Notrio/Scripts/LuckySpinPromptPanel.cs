using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;
using System;
using GameSparks.Core;
using EasyMobile;

public class LuckySpinPromptPanel : OverlayPanel
{
    public static string IsGetCountDownTickKey = "IS_GET_COUNT_DOWN_TICK_KEY";
    public static string IsFirstShowLuckyKey = "IS_FIRST_SHOW_LUCKY_KEY";

    [Header("UI Preferences")]
    public ClockController clockController;
    public Button luckySpinnerBtn;
    public Button luckySpinnerInactiveBtn;
    public Button closeBtn;
    private Text luckyBtnRemainText;
    private Button luckySpinBtn;
    private OverlayUIController overlayUIController;
    private RollPanelUI rollPanelUI;
    private UIGuide uiGui;

    public bool enableLuckySpinPrompt
    {
        get
        {
            if (StoryPuzzlesSaver.Instance == null)
                return false;
            else
            {
                return StoryPuzzlesSaver.Instance.GetMaxLevel() > startLuckyPromptAtLevel;
            }
        }
    }

    public bool IsReadyLuckySpin
    {
        get
        {
            return rollPanelUI.numberOfSpinLeft > 0 && rollPanelUI.timeUntilNextSpinner <= 0;
        }
    }

    private int startLuckyPromptAtLevel { set { PlayerPrefs.SetInt("StartLuckyPromptAtLevel", value); } get { return PlayerPrefs.GetInt("StartLuckyPromptAtLevel", 4); } }
    private Coroutine checkLuckyCoroutine;

    [HideInInspector]
    public HeaderUI headerUI;

    public OverlayGroupController controller;

    private void Awake()
    {
        if (UIReferences.Instance != null)
        {
            UpdateReferences();
        }
        UIReferences.UiReferencesUpdated += UpdateReferences;
    }

    private void UpdateReferences()
    {
        headerUI = UIReferences.Instance.gameUiHeaderUI;
        overlayUIController = UIReferences.Instance.overlayUIController;
        rollPanelUI = UIReferences.Instance.overlayRollPanelUI;
        uiGui = UIReferences.Instance.uiGui;
        luckyBtnRemainText = UIReferences.Instance.ecasPanelController.tabList[0].info;

        rollPanelUI.finishSpinner += ResetCountDowntPlayerPrefs;
    }

    void ResetCountDowntPlayerPrefs()
    {
        HideMainLuckySpinButton();
    }

    private void Start()
    {
        UpdateButtonUI();
        luckySpinnerBtn.onClick.AddListener(() =>
        {
            ActiveLuckySpin();
        });

        closeBtn.onClick.AddListener(delegate
        {
            Hide();
        });
        CloudServiceManager.onConfigLoaded += OnConfigLoaded;

        InvokeRepeating("CheckLuckySpinState", 0.1f, 1);
    }

    private void OnDisable()
    {
        CancelInvoke("CheckLuckySpinState");
        CancelInvoke("UpdateButtonUI");
    }

    private void OnDestroy()
    {
        UIReferences.UiReferencesUpdated -= UpdateReferences;
        rollPanelUI.finishSpinner -= ResetCountDowntPlayerPrefs;
        CloudServiceManager.onConfigLoaded -= OnConfigLoaded;
        StopDelayShowLuckyPanel();

        CancelInvoke("CheckLuckySpinState");
        CancelInvoke("UpdateButtonUI");
    }

    private void OnConfigLoaded(GSData config)
    {
        int? levelStartCheck = config.GetInt("startLuckyPromptAtLevel");
        if (levelStartCheck.HasValue)
            startLuckyPromptAtLevel = levelStartCheck.Value;
    }

    public void ActiveLuckySpin()
    {
        if (UIReferences.Instance.overlayRollPanelUI.timeUntilNextSpinner < 0)
        {
            GameManager.CurrentPurposeRewardAd = PurposeRewardAd.GetItem;
            CoinManager.luckySpinnerRewardCoin = true;
            if (AdsFrequencyManager.Instance.showAdsInGame)
            {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                if (Application.internetReachability != NetworkReachability.NotReachable && Advertising.IsRewardedAdReady())
                {
                    CoinManager.onRewarded += OnAdReward;
#if UNITY_IOS
                    Time.timeScale = 0;
                    AudioListener.pause = true;
#endif
                    Advertising.ShowRewardedAd();
                }          
#else
                CoinManager.onRewarded += OnAdReward;
                CoinManager.Instance.FakeAdReward();
#endif
            }
            else
            {
                CoinManager.onRewarded += OnAdReward;
                CoinManager.Instance.FakeAdReward();
            }
        }
    }

    private void OnAdReward(RollingItem.RollingItemData itemData)
    {
        CoinManager.onRewarded -= OnAdReward;
        Hide();
        UIReferences.Instance.overlayRollPanelUI.itemData = itemData;
        UIReferences.Instance.overlayRollPanelUI.Show();
    }

    private void UpdateButtonUI()
    {
        luckySpinnerBtn.gameObject.SetActive(IsRewardedAdReady && IsReadyLuckySpin);
        luckySpinnerInactiveBtn.gameObject.SetActive(!luckySpinnerBtn.gameObject.activeSelf);

        //if (!IsReadyLuckySpin)
        //{
        //    if (rollPanelUI.numberOfSpinLeft > 0 && rollPanelUI.timeUntilNextSpinner > 0)
        //    {
        //        String timeSpanString = new TimeSpan(0, 0, (int)rollPanelUI.timeUntilNextSpinner).ToString();
        //        luckyBtnRemainText.text = I2.Loc.ScriptLocalization.LUCKY_SPINNER_NEXT_SPIN + ": " + timeSpanString;
        //    }
        //    else
        //    {
        //        luckyBtnRemainText.text = I2.Loc.ScriptLocalization.Unavailable_;
        //    }
        //}
        //else
        //{
        //    if (IsRewardedAdReady)
        //        luckyBtnRemainText.text = I2.Loc.ScriptLocalization.LUCKY_SPINNER_INFO;
        //    else
        //        luckyBtnRemainText.text = I2.Loc.ScriptLocalization.Unavailable_;

        //}
    }

    public void HideMainLuckySpinButton()
    {
        PlayerPrefs.SetInt(IsGetCountDownTickKey, 0);
        rollPanelUI.ResetLuckyLastTime();
        if (IsShowing)
            Hide();
    }

    void CheckLuckySpinState()
    {
        if (IsReadyLuckySpin)
        {
            if (!GameManager.Instance.GameState.Equals(GameState.Prepare))
                return;

            if (PlayerPrefs.GetInt(IsGetCountDownTickKey, 0) == 0)
            {
                if (enableLuckySpinPrompt)
                {
                    if (overlayUIController.ShowingPanelCount > 0)
                        return;

                    if (uiGui.isShownGuide)
                        return;

                    if (uiGui.isWaitingGuide)
                        return;

                    if (!IsRewardedAdReady)
                        return;

                    PlayerPrefs.SetInt(IsGetCountDownTickKey, 1);
                    StartDelayShowLuckyPanel(1.5f);
                }
            }
        }
    }

    public void StartDelayShowLuckyPanel(float seconds)
    {
        if (checkLuckyCoroutine != null)
            StopCoroutine(checkLuckyCoroutine);
        checkLuckyCoroutine = StartCoroutine(CR_DelayShowLuckyPanel(seconds));
    }

    public void StopDelayShowLuckyPanel()
    {
        if (checkLuckyCoroutine != null)
            StopCoroutine(checkLuckyCoroutine);
    }

    IEnumerator CR_DelayShowLuckyPanel(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        yield return new WaitUntil(() => GameManager.Instance.GameState.Equals(GameState.Prepare) && overlayUIController.ShowingPanelCount <= 0 && !uiGui.isShownGuide && IsRewardedAdReady && !uiGui.isWaitingGuide);
        Show();
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
        controller.ShowIfNot();
        IsShowing = true;
        transform.BringToFront();
        onPanelStateChanged(this, true);
        closeBtn.interactable = true;
        InvokeRepeating("UpdateButtonUI", 0.1f, 1);
    }

    public override void Hide()
    {
        closeBtn.interactable = false;
        controller.HideIfNot();
        IsShowing = false;
        onPanelStateChanged(this, false);
        CancelInvoke("UpdateButtonUI");
    }
}
