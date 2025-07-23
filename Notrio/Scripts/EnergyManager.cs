using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;
using GameSparks.Core;
using Takuzu.Generator;

public class EnergyManager : MonoBehaviour
{

    public static EnergyManager Instance;
    public static System.Action<int, int> currentEnergyChanged = delegate { };
    public static System.Action<int, int> maxEnergyChanged = delegate { };
    public static System.Action<int, int, Puzzle> lowEnergy = delegate { };
    private int defaultMultiplayerEnergyCost = 1;
    public int MultiplayerEnergyCost
    {
        get
        {
            if (CloudServiceManager.Instance != null && CloudServiceManager.Instance.appConfig != null)
            {
                return CloudServiceManager.Instance.appConfig.GetInt(MULTIPLAYER_ENERGY_COST_KEY) ?? defaultMultiplayerEnergyCost;
            }
            return defaultMultiplayerEnergyCost;
        }
    }
    public int[] TournamentEnergyCost { set { PlayerPrefs.SetString(tournamentEneryCostsKey, JsonUtility.ToJson(new EnergyCostList() { energyCosts = value })); } get { return JsonUtility.FromJson<EnergyCostList>(PlayerPrefs.GetString(tournamentEneryCostsKey, JsonUtility.ToJson(new EnergyCostList()))).energyCosts; } }
    public int[] StoryModeEnergyCost { set { PlayerPrefs.SetString(storymodeEnergyCostsKey, JsonUtility.ToJson(new EnergyCostList() { energyCosts = value })); } get { return JsonUtility.FromJson<EnergyCostList>(PlayerPrefs.GetString(storymodeEnergyCostsKey, JsonUtility.ToJson(new EnergyCostList()))).energyCosts; } }

    public class EnergyCostList
    {
        public int[] energyCosts = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
    }

    private double timeLeft, originalTimeleft;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    #region privateData
    private int currentEnergy = 0;
    private int lowEnergyIndex = 5;
    private string PLAYER_ENERGY_KEY = "PLAYER_ENERGY_KEY";
    private string PLAYER_LAST_INCREASE_TIME_KEY = "PLAYER_LAST_INCREASE_TIME_KEY";
    private string IncreaseEnergyInterval_KEY = "IncreaseEnergyInterval_KEY";
    private string IncreaseEnergyAmount_KEY = "IncreaseEnergyAmount_KEY";
    private string maxEnergy_KEY = "maxEnergy_KEY";
    private string tournamentEneryCostsKey = "TOURNAMENT_ENERGYCOSTS";
    private string storymodeEnergyCostsKey = "STORYMODE_ENERGYCOSTS";
    private string MULTIPLAYER_ENERGY_COST_KEY = "MULTIPLAYER_ENERGY__COST_KEY";
    #endregion

    #region SetGetRegion
    public int CurrentEnergy
    {
        get
        {
            if (AlwaysMaxEnergy())
                return MaxEnergy;
            return currentEnergy;
        }

        set
        {
            // no change current energy while subscribing
            if (AlwaysMaxEnergy())
            {
                currentEnergy = PlayerPrefs.GetInt(PLAYER_ENERGY_KEY, MaxEnergy);
                return;
            }
            if (value <= 0)
                value = 0;
            int oldValue = currentEnergy;
            currentEnergy = value;
            PlayerPrefs.SetInt(PLAYER_ENERGY_KEY, value);
            currentEnergyChanged(value, oldValue);
        }
    }

    public bool AlwaysMaxEnergy()
    {
        //if (InAppPurchaser.StaticIsSubscibed())
        //    return true;
        //if (InAppPurchaser.Instance.IsOneTimePurchased())
        //    return true;
        //return false;
        return true;
    }

    public int LowEnergyIndex
    {
        get
        {
            return lowEnergyIndex;
        }
    }

    public int MaxEnergy
    {
        get
        {
            return PlayerPrefs.GetInt(maxEnergy_KEY, 50);
        }

        set
        {
            if (value <= 1)
                value = 1;
            int oldValue = MaxEnergy;
            PlayerPrefs.SetInt(maxEnergy_KEY, value);
            maxEnergyChanged(value, oldValue);
        }
    }

    public float IncreaseEnergyInterval
    {
        get
        {
            return PlayerPrefs.GetFloat(IncreaseEnergyInterval_KEY, 600);
        }

        set
        {
            PlayerPrefs.SetFloat(IncreaseEnergyInterval_KEY, value);
        }
    }

    public int IncreaseEnergyAmount
    {
        get
        {
            return PlayerPrefs.GetInt(IncreaseEnergyAmount_KEY, 1);
        }

        set
        {
            PlayerPrefs.SetInt(IncreaseEnergyAmount_KEY, value);
        }
    }
    #endregion

    #region PublicRegion
    [HideInInspector]
    public double lastTime { set { PlayerPrefs.SetString(PLAYER_LAST_INCREASE_TIME_KEY, value.ToString()); } get { return Double.Parse(PlayerPrefs.GetString(PLAYER_LAST_INCREASE_TIME_KEY, "0.0")); } }
    #endregion
    private IEnumerator ChangeEnergyLevel_CR;

    private bool wasInGame, wasSuspendGame, wasForceUpdateEnergy;
    private double startInGameTime, endInGameTime, startSuspendTime, endSuspendTime;

    private void Start()
    {
        GetPlayerDataFromLocal();
        if (!PlayerPrefs.HasKey(PLAYER_LAST_INCREASE_TIME_KEY))
        {
            PlayerPrefs.SetString(PLAYER_LAST_INCREASE_TIME_KEY, TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds.ToString());
        }
        GameManager.GameStateChanged += OnGameStateChanged;
        int lastTimeCurrentEnergy = PlayerPrefs.GetInt(PLAYER_ENERGY_KEY, MaxEnergy);
        CurrentEnergy = lastTimeCurrentEnergy + ((int)((TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds - lastTime) / IncreaseEnergyInterval)) * IncreaseEnergyAmount;
        if (lastTimeCurrentEnergy < MaxEnergy)
            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, MaxEnergy);
        else
            CurrentEnergy = lastTimeCurrentEnergy;
        lastTime = lastTime + ((int)((TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds - lastTime) / IncreaseEnergyInterval)) * IncreaseEnergyInterval;
        lowEnergy += OnLowEnergy;
    }

    private void OnEnable()
    {
        CloudServiceManager.onConfigLoaded += OnAppConfigLoadded;
    }

    private void OnDisable()
    {
        CloudServiceManager.onConfigLoaded -= OnAppConfigLoadded;
    }

    void OnGameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing && oldState == GameState.Prepare)
        {
            startInGameTime = TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds;
            timeLeft = (IncreaseEnergyInterval - (float)(TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds - EnergyManager.Instance.lastTime));
            originalTimeleft = timeLeft;
            wasInGame = true;
        }

        if (newState == GameState.Prepare)
        {
            if (wasInGame)
            {
                endInGameTime = TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds;
                lastTime = lastTime + (endInGameTime - startInGameTime) + (timeLeft - originalTimeleft);
                wasInGame = false;
            }
        }
    }

    void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            startSuspendTime = TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds;
            wasSuspendGame = true;
        }
        else
        {
            if (wasSuspendGame)
            {
                endSuspendTime = TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds;
                wasSuspendGame = false;

                double totalTimeSuspended = endSuspendTime - startSuspendTime;
                int lastTimeCurrentEnergy = PlayerPrefs.GetInt(PLAYER_ENERGY_KEY, MaxEnergy);
                CurrentEnergy = lastTimeCurrentEnergy + ((int)(totalTimeSuspended / IncreaseEnergyInterval)) * IncreaseEnergyAmount;

                if (totalTimeSuspended % IncreaseEnergyInterval - timeLeft >= 0)
                    CurrentEnergy++;

                timeLeft = timeLeft - (totalTimeSuspended % IncreaseEnergyInterval);

                if (timeLeft <= 0)
                    timeLeft += IncreaseEnergyInterval;

                if (lastTimeCurrentEnergy < MaxEnergy)
                    CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, MaxEnergy);
                else
                    CurrentEnergy = lastTimeCurrentEnergy;

                if (lastTimeCurrentEnergy < CurrentEnergy)
                    wasForceUpdateEnergy = true;

                //Debug.Log("totalTimeSuspended = " + totalTimeSuspended % IncreaseEnergyInterval);
                //Debug.Log("timeLeft = " + timeLeft);
            }
        }
    }

    private void GetPlayerDataFromLocal()
    {

        IncreaseEnergyInterval = PlayerPrefs.GetFloat(IncreaseEnergyInterval_KEY, IncreaseEnergyInterval);

        IncreaseEnergyAmount = PlayerPrefs.GetInt(IncreaseEnergyAmount_KEY, IncreaseEnergyAmount);

        MaxEnergy = PlayerPrefs.GetInt(maxEnergy_KEY, MaxEnergy);
    }

    private void OnDestroy()
    {
        GameManager.GameStateChanged -= OnGameStateChanged;
        lowEnergy -= OnLowEnergy;
    }

    private void OnLowEnergy(int current, int required, Puzzle p)
    {
        //InGameNotificationPopup.Instance.ShowToast(String.Format("Your current energy is low: {0}, require as least {1} to play this puzzle!!!", current, required), 3);
        UIReferences.Instance.overlayEnergyExchangePanel.Show();
        UIReferences.Instance.overlayEnergyExchangePanel.enableRewardEnergyBtn = true;
        //InGameNotificationPopup.Instance.confirmationDialog.Show(I2.Loc.ScriptLocalization.ATTENTION,String.Format(I2.Loc.ScriptLocalization.ENERGY_LOW, current, required),I2.Loc.ScriptLocalization.GET_ENERGY,"",()=>{
        //       UIReferences.Instance.gameUiHeaderUI.energyExchangePanel.Show();
        //});
    }

    private void OnAppConfigLoadded(GSData appConfig)
    {
        float? interval = appConfig.GetFloat("energyInterval");
        if (interval.HasValue)
        {
            IncreaseEnergyInterval = interval.Value;
        }

        int? amount = appConfig.GetInt("energyAmount");
        if (amount.HasValue)
        {
            IncreaseEnergyAmount = amount.Value;
        }

        int? max = appConfig.GetInt("maxEnergy");
        if (max.HasValue)
        {
            MaxEnergy = max.Value;
        }

        int? low = appConfig.GetInt("lowEnergyIndex");
        {
            if (low.HasValue)
                lowEnergyIndex = low.Value;
        }

        List<int> tEnergyCosts = appConfig.GetIntList("TournamentEnergyCosts");
        if (tEnergyCosts != null && tEnergyCosts.Count > 0)
        {
            TournamentEnergyCost = tEnergyCosts.ToArray();
        }
        List<int> sEnergyCosts = appConfig.GetIntList("StoryModeEnergyCosts");
        if (sEnergyCosts != null && sEnergyCosts.Count > 0)
        {
            StoryModeEnergyCost = sEnergyCosts.ToArray();
        }

    }

    private void Update()
    {
        UpdateCurrentEnergy();
    }

    private void UpdateCurrentEnergy()
    {
        if (GameManager.Instance.GameState == GameState.Prepare && !wasSuspendGame)
        {
            if (TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds - lastTime > IncreaseEnergyInterval)
            {
                lastTime = TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds;

                if (wasForceUpdateEnergy)
                {
                    wasForceUpdateEnergy = false;
                    return;
                }

                if (CurrentEnergy < MaxEnergy)
                    CurrentEnergy = Mathf.Min(CurrentEnergy + IncreaseEnergyAmount, MaxEnergy);
            }
        }
    }

    internal void AddEnergy(int v)
    {
        CurrentEnergy += v;
    }

    internal bool PlayPuzzle(string id)
    {
        Puzzle p = PuzzleManager.Instance.GetPuzzleByIdIgnoreType(id);
        int energyCost = 0;
        if (PuzzleManager.Instance.IsMultiMode(id))
        {
            energyCost = MultiplayerEnergyCost;
        }
        else
        {
            energyCost = GetCostByLevel(p, PuzzleManager.Instance.IsChallenge(id)
            ? TournamentEnergyCost : StoryModeEnergyCost);
        }

        if (CurrentEnergy - energyCost >= 0)
        {
            CurrentEnergy -= energyCost;
            currentEnergyIslow = false;
            return true;
        }
        else
        {
            lowEnergy(CurrentEnergy, energyCost, p);
            return false;
        }
    }

    internal bool IsEnoughEnergy(string id)
    {
        int energyCost = GetCostByLevel(PuzzleManager.Instance.IsChallenge(id)
            ? PuzzleManager.Instance.GetChallengeById(id) : PuzzleManager.Instance.GetPuzzleById(id), PuzzleManager.Instance.IsChallenge(id)
            ? TournamentEnergyCost : StoryModeEnergyCost);
        if (CurrentEnergy - energyCost >= 0)
            return true;
        else
            return false;
    }

    public static bool currentEnergyIslow = false;

    public int GetCostByLevel(Puzzle puzzle, int[] energyCosts)
    {
        int cost = 0;
        int size = (int)puzzle.size;
        int index = 0;

        switch (size)
        {
            case 6:
                index = 0;
                break;
            case 8:
                index = 1;
                break;
            case 10:
                index = 2;
                break;
            case 12:
                index = 3;
                break;
        }

        switch (puzzle.level)
        {
            case Level.Easy:
                cost = energyCosts[0 + index];
                break;
            case Level.Medium:
                cost = energyCosts[4 + index];
                break;
            case Level.Hard:
                cost = energyCosts[8 + index];
                break;
            case Level.Evil:
                cost = energyCosts[12 + index];
                break;
            case Level.Insane:
                cost = energyCosts[16 + index];
                break;
        }
        return cost;
    }
}
