using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;
using EasyMobile;
using GameSparks.Core;

public class RollPanelUI : OverlayPanel {

    [Header("UI References")]
    public LevierButton levierButton;
    public RollingWindow rollingWindow;
    public Transform spawnPoint;
    public OverlayGroupController controller;
    public Button closeBtn;

    [HideInInspector]
    public RollingItem.RollingItemData itemData;
    public int minimumTimeBeforeAbleToWatchAdForCoins { set { PlayerPrefs.SetInt("w4ceminimumtime", value); } get { return PlayerPrefs.GetInt("w4ceminimumtime", 3600); } }
    public int minimumTimeBeforeAbleToWatchOutEnergyAdForCoins { set { PlayerPrefs.SetInt("w4ceminimumtimeOutEnergy", value); } get { return PlayerPrefs.GetInt("w4ceminimumtimeOutEnergy", 600); } }
    public int minimumTimeBeforeShowW4CNotifications { set { PlayerPrefs.SetInt("w4cnotificationtime", value); } get { return PlayerPrefs.GetInt("w4cnotificationtime", 720); } }
    public int turnOffluckyRollNotificationTime { set { PlayerPrefs.SetInt("turnoffluckyrollnotificationtime", value); } get { return PlayerPrefs.GetInt("turnoffluckyrollnotificationtime", 22); } }
    public int turnOnluckyRollNotificationTime { set { PlayerPrefs.SetInt("turnonluckyrollnotificationtime", value); } get { return PlayerPrefs.GetInt("turnonluckyrollnotificationtime", 8); } }
    private bool hasRolled = false;

    public Action finishSpinner = delegate { };

    private int mLastTime = -1;
    private int mLastTimeOutEnergy = -1;
    private bool canHide;

    private void Start()
    {
        levierButton.buttonClicked += OnLevierClicked;
        CloudServiceManager.onConfigLoaded += OnConfigLoaded;
        closeBtn.onClick.AddListener(() =>
        {
            //Hide();
        });
        //rollingWindow.UpdateUI();
        closeBtn.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        levierButton.buttonClicked -= OnLevierClicked;
        CloudServiceManager.onConfigLoaded -= OnConfigLoaded;
    }
    private void OnConfigLoaded(GSData config)
    {
        int? w4ceminimumtime = config.GetInt("w4ceminimumtime");
        if (w4ceminimumtime.HasValue)
            minimumTimeBeforeAbleToWatchAdForCoins = w4ceminimumtime.Value;
        int? w4ceminimumtimeOutEnergy = config.GetInt("w4ceminimumtimeOutEnergy");
        if (w4ceminimumtimeOutEnergy.HasValue)
            minimumTimeBeforeAbleToWatchOutEnergyAdForCoins = w4ceminimumtimeOutEnergy.Value;
        int? turnoffluckyrollnotificationtime = config.GetInt("turnoffluckyrollnotificationtime");
        if (turnoffluckyrollnotificationtime.HasValue)
            turnOffluckyRollNotificationTime = turnoffluckyrollnotificationtime.Value;
        int? turnonluckyrollnotificationtime = config.GetInt("turnonluckyrollnotificationtime");
        if (turnonluckyrollnotificationtime.HasValue)
            turnOnluckyRollNotificationTime = turnonluckyrollnotificationtime.Value;
    }

    public void ResetLuckyLastTime()
    {
        mLastTime = -1;
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan untilNow = DateTime.UtcNow - epoch;
        PlayerPrefs.SetInt("LuckyRollLastTime", (int)untilNow.TotalSeconds);
    }

    public void AddLastTimeAtNow(int seconds)
    {
        mLastTime = -1;
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan untilNow = DateTime.UtcNow - epoch;
        PlayerPrefs.SetInt("LuckyRollLastTime", (int)untilNow.TotalSeconds + seconds);
    }

    private IEnumerator RewardAnimationCR(RollingItem.RollingItemData itemData)
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan untilNow = DateTime.UtcNow - epoch;
        PlayerPrefs.SetInt("LuckyRollLastTime",(int) untilNow.TotalSeconds);

        string lastSave = PlayerPrefs.GetString("LuckySpinned", String.Format("{0}-{1}", ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Subtract(new DateTime(2000, 1, 1))).TotalDays, 0));
        if (int.Parse(lastSave.Split('-')[0]) == ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Subtract(new DateTime(2000, 1, 1))).TotalDays)
        {
            PlayerPrefs.SetString("LuckySpinned", String.Format("{0}-{1}", ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Subtract(new DateTime(2000, 1, 1))).TotalDays, int.Parse(lastSave.Split('-')[1]) + 1));
        }
        else
        {
            PlayerPrefs.SetString("LuckySpinned", String.Format("{0}-{1}", ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Subtract(new DateTime(2000, 1, 1))).TotalDays, 1));
        }

        mLastTime = -1;
        rollingWindow.CreateNewRollingWindow();
        hasRolled = true;
        SoundManager.Instance.PlaySound(SoundManager.Instance.buyCoin, true);
        yield return null;
        rollingWindow.SettupRollingWindow(itemData, CoinManager.Instance.rollingItemDatas.FindAll(item => item.rewardCoins == itemData.rewardCoins));
        rollingWindow.StartRolling();
        yield return new WaitForSeconds(rollingWindow.GetRollingDuration());
        //rollingWindow.CurrentState = RollingWindow.State.SlowDown;
        //yield return new WaitForSeconds(rollingWindow.decelereationTime);
        CoinEnergyRewardAnimation.Instance.StartAnimation(spawnPoint, itemData.amount, itemData.rewardCoins);
        yield return new WaitForSeconds(0.1f);
        CoinManager.Instance.CompleteAdReward(itemData);
        //ScheduleLocalNotification();
        canHide = true;
        Hide();
        finishSpinner();
    }

    public int timeUntilNextSpinner { get {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan untilNow = DateTime.UtcNow - epoch;
            if (mLastTime == -1)
                mLastTime = PlayerPrefs.GetInt("LuckyRollLastTime", 0);
            int lastTime = mLastTime;
            return minimumTimeBeforeAbleToWatchAdForCoins - ((int) untilNow.TotalSeconds - lastTime);
        } }

    public int timeUntilNextWatchAd4Energy
    {
        get
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan untilNow = DateTime.UtcNow - epoch;
            if (mLastTimeOutEnergy == -1)
                mLastTimeOutEnergy = PlayerPrefs.GetInt("WatchAd4EnergyLastTime", 0);
            int lastTime = mLastTimeOutEnergy;
            return minimumTimeBeforeAbleToWatchOutEnergyAdForCoins - ((int)untilNow.TotalSeconds - lastTime);
        }
    }

    public int numberOfSpinLeft { get {
            string lastSave = PlayerPrefs.GetString("LuckySpinned", String.Format("{0}-{1}", ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Subtract(new DateTime(2000, 1, 1))).TotalDays, 0));
            if (int.Parse(lastSave.Split('-')[0]) == ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Subtract(new DateTime(2000, 1, 1))).TotalDays)
            {
                return (CloudServiceManager.Instance.appConfig.GetInt("luckySpinPerDay") ?? 4) - int.Parse(lastSave.Split('-')[1]);
            }
            else
            {
                return (CloudServiceManager.Instance.appConfig.GetInt("luckySpinPerDay") ?? 4);
            }
    } }

    public int numberOfWatchAd4Energy
    {
        get
        {
            string lastSave = PlayerPrefs.GetString("WatchAd4EnergyData", String.Format("{0}-{1}", ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Subtract(new DateTime(2000, 1, 1))).TotalDays, 0));
            if (int.Parse(lastSave.Split('-')[0]) == ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Subtract(new DateTime(2000, 1, 1))).TotalDays)
            {
                return (CloudServiceManager.Instance.appConfig.GetInt("watchAd4EnergyPerDay") ?? 5) - int.Parse(lastSave.Split('-')[1]);
            }
            else
            {
                return (CloudServiceManager.Instance.appConfig.GetInt("watchAd4EnergyPerDay") ?? 5);
            }
        }
    }

    private void ScheduleLocalNotification()
    {
        Notifications.CancelAllPendingLocalNotifications();
        NotificationContent content = PrepareNotificationContent();
        DateTime triggerDate = DateTime.Now;
        Debug.Log(triggerDate);
        Debug.Log(triggerDate.Hour);
        triggerDate = triggerDate.AddHours(24 - turnOffluckyRollNotificationTime);
        triggerDate = triggerDate.AddMinutes(minimumTimeBeforeShowW4CNotifications);
        if (triggerDate.Hour < 24 - turnOffluckyRollNotificationTime + turnOnluckyRollNotificationTime)
        {
            triggerDate = new DateTime(triggerDate.Year, triggerDate.Month, triggerDate.Day, 24 - turnOffluckyRollNotificationTime + turnOnluckyRollNotificationTime, 0, 0);
        }
        triggerDate = triggerDate.AddHours(-(24 - turnOffluckyRollNotificationTime));
        Debug.Log(triggerDate);
        Notifications.ScheduleLocalNotification(triggerDate, content);
    }

    NotificationContent PrepareNotificationContent()
    {
        NotificationContent content = new NotificationContent();

        content.title = "Lucky Spin";
        content.subtitle = "Lucky Spin";
        content.body = "Feeling lucky? Try a Lucky Spin now!";
        content.smallIcon = "ic_stat_onesignal_default";
        content.largeIcon = "ic_onesignal_large_icon_default";

        return content;
    }
    private void OnLevierClicked()
    {
        if (hasRolled)
            return;
        StartCoroutine(RewardAnimationCR(itemData));
    }
    public static Action UsingLuckySpinner = delegate {};

    public override void Show()
    {
        UsingLuckySpinner();
        canHide = false;
        rollingWindow.CreateNewRollingWindow();
        hasRolled = false;
        controller.ShowIfNot();
        IsShowing = true;
        onPanelStateChanged(this, true);
        closeBtn.gameObject.SetActive(true);
        if (GameManager.Instance.GameState == GameState.Prepare)
            SoundManager.Instance.bgmMenuSource.Pause();

        if (GameManager.Instance.GameState == GameState.Playing)
            SoundManager.Instance.bgmIngameSource.Pause();
    }

    public override void Hide()
    {
        if (canHide)
        {
            controller.HideIfNot();
            IsShowing = false;
            onPanelStateChanged(this, false);
            closeBtn.gameObject.SetActive(false);
            canHide = false;
            if (GameManager.Instance.GameState == GameState.Prepare)
                SoundManager.Instance.bgmMenuSource.UnPause();

            if (GameManager.Instance.GameState == GameState.Playing)
                SoundManager.Instance.bgmIngameSource.UnPause();
        }
    }

    public void UpdateTimeUntilNextSpinnerOutEnergy()
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan untilNow = DateTime.UtcNow - epoch;
        PlayerPrefs.SetInt("WatchAd4EnergyLastTime", (int)untilNow.TotalSeconds);
        mLastTimeOutEnergy = -1;

        string lastSave = PlayerPrefs.GetString("WatchAd4EnergyData", String.Format("{0}-{1}", ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Subtract(new DateTime(2000, 1, 1))).TotalDays, 0));
        if (int.Parse(lastSave.Split('-')[0]) == ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Subtract(new DateTime(2000, 1, 1))).TotalDays)
        {
            PlayerPrefs.SetString("WatchAd4EnergyData", String.Format("{0}-{1}", ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Subtract(new DateTime(2000, 1, 1))).TotalDays, int.Parse(lastSave.Split('-')[1]) + 1));
        }
        else
        {
            PlayerPrefs.SetString("WatchAd4EnergyData", String.Format("{0}-{1}", ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Subtract(new DateTime(2000, 1, 1))).TotalDays, 1));
        }
    }
}
