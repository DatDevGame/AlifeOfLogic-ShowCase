using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using Takuzu.Generator;
using EasyMobile;
using System;
using LionStudios.Suite.Analytics;

namespace Takuzu
{
    public enum PurposeRewardAd
    {
        GetItem,
        PlayDailyPuzzle,
        UnlockDailyChapter
    }

    public enum GameState
    {
        Startup,
        Prepare,
        Playing,
        Paused,
        PreGameOver,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public static event Action<GameState, GameState> GameStateChanged = delegate { };

        public static event Action ForceOutInGamScene = delegate { };

        private static bool isRestart;

        public static PurposeRewardAd CurrentPurposeRewardAd = PurposeRewardAd.GetItem;

        public GameState GameState
        {
            get
            {
                return _gameState;
            }
            private set
            {
                if (value != _gameState)
                {
                    GameState oldState = _gameState;
                    _gameState = value;

                    //* do not sleep while in play mode
                    if (_gameState == GameState.Playing)
                        Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    else
                        Screen.sleepTimeout = SleepTimeout.SystemSetting;

                    GameStateChanged(_gameState, oldState);
                }
            }
        }

        public static int GameCount
        {
            get { return _gameCount; }
            private set { _gameCount = value; }
        }

        private static int _gameCount = 0;

        [Header("Set the target frame rate for this game")]
        [Tooltip("Use 60 for games requiring smooth quick motion, set -1 to use platform default frame rate")]
        public int targetFrameRate = 30;

        [Header("Current game state")]
        [SerializeField]
        private GameState _gameState = GameState.Startup;
        private string currentDailyPuzzleId;
        private ConfirmationDialog confirmDialog;

        void OnEnable()
        {
            GameStateChanged += OnGameStateChanged;
            Advertising.RewardedAdCompleted += OnRewardedAdCompleted;
            Judger.onJudgingCompleted += OnJudgingCompleted;
            UIReferences.UiReferencesUpdated += UpdateReferences;
            InAppPurchasing.PurchaseCompleted += OnPurchasedCompleted;
            InAppPurchasing.RestoreCompleted += OnPurchaseRestored;
            SubscriptionDetailPanelController.OnShow += OnSubscriptionPanelShow;
            SubscriptionDetailPanelController.OnHide += OnSubscriptionPanelHide;
        }

        void OnDisable()
        {
            GameStateChanged -= OnGameStateChanged;
            Advertising.RewardedAdCompleted -= OnRewardedAdCompleted;
            Judger.onJudgingCompleted -= OnJudgingCompleted;
            UIReferences.UiReferencesUpdated -= UpdateReferences;
            InAppPurchasing.PurchaseCompleted -= OnPurchasedCompleted;
            InAppPurchasing.RestoreCompleted -= OnPurchaseRestored;
            SubscriptionDetailPanelController.OnShow -= OnSubscriptionPanelShow;
            SubscriptionDetailPanelController.OnHide -= OnSubscriptionPanelHide;
        }

        void UpdateReferences()
        {
            confirmDialog = UIReferences.Instance.overlayConfirmDialog;
        }
        private void OnJudgingCompleted(Judger.JudgingResult result)
        {
            GameOver();
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }

            Application.targetFrameRate = targetFrameRate;
        }

        private IEnumerator DisableLogging()
        {
            yield return null;
            Debug.unityLogger.logEnabled = false;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // Use this for initialization
        void Start()
        {
            // Initial setup
            ScoreManager.Instance.Reset();

            PrepareGame();
        }

        // Make initial setup and preparations before the game can be played
        public void PrepareGame()
        {
            GameState = GameState.Prepare;

            // Automatically start the game if this is a restart.
            if (isRestart)
            {
                isRestart = false;
                StartGame();
            }
        }

        // A new game official starts
        public void StartGame()
        {
            GameState = GameState.Playing;
        }

        public void PlayAPuzzle(string id, bool ignoreEnergy = false)
        {
            if (ignoreEnergy || EnergyManager.Instance.PlayPuzzle(id))
            {
                EnterGamePlay(id);
                if (PuzzleManager.Instance.IsChallenge(id))
                {
                    if (PuzzleManager.Instance.GetDailyChallengeIdString() != null)
                    {
                        CloudServiceManager.Instance.SubmitDailyChallengeEntry(id);
                    }
                    else if (PuzzleManager.Instance.GetWeeklyChallengeIdString() != null)
                    {
                        CloudServiceManager.Instance.SubmitWeeklyChallengeEntry(id);
                    }
                }
            }
            else
            {
                PrepareGame();
            }
        }

        public void PlayFreePuzzle(string id)
        {
            EnterGamePlay(id);
            if (PuzzleManager.Instance.IsChallenge(id))
            {
                if (PuzzleManager.Instance.GetDailyChallengeIdString() != null)
                {
                    CloudServiceManager.Instance.SubmitDailyChallengeEntry(id);
                }
                else if (PuzzleManager.Instance.GetWeeklyChallengeIdString() != null)
                {
                    CloudServiceManager.Instance.SubmitWeeklyChallengeEntry(id);
                }
            }
        }

        public bool currentTournamentPuzzleIsSubscribed = false;
        public void PlayADailyTournamentPuzzle(string id, bool isSubscribe)
        {
            currentTournamentPuzzleIsSubscribed = isSubscribe;
            currentDailyPuzzleId = id;
            //* Allow to play without charging energy */
            if (isSubscribe)
            {
                //* Player have subscribed =>> allow them to play with though ad or charge energy */
                PlayFreePuzzle(id);
                return;
            }
            if (AdsFrequencyManager.Instance.showAdsInGame == false)
            {
                //* Check if this build allow to show ad in game */
                //* If not allow player to play puzzle without charge energy*/
                PlayFreePuzzle(id);
                return;
            }

            //* Allow to play but charge energy */
            if (EnergyManager.Instance.IsEnoughEnergy(id) == false)
            {
                //* Call this function only to show insufficient energy prompt */
                EnergyManager.Instance.PlayPuzzle(id);
                return;
            }
            if (PuzzleManager.Instance.IsDailyChapterUnlocked(id))
            {
                //* Player have played this puzzle before which means they have already watched ad for this puzzle once */
                //* No need to show ad again, allow them to play puzzle <<Cost energy still>> */
                PlayAPuzzle(id);
                return;
            }
            if (IsRewardedAdReady == false)
            {
                //* Can't show rewarded ad */
                confirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION, I2.Loc.ScriptLocalization.FAIL_LOAD_AD_MSG, I2.Loc.ScriptLocalization.OK.ToUpper(), "", () => { });
                return;
            }
            if (Application.isEditor)
            {
                //* Allow to play without watch ad in editor mode */
                PlayAPuzzle(id);
                return;
            }
            if (AdDisplayer.IsAllowToShowAd() == false)
            {
                PlayAPuzzle(id);
                return;
            }
            //* Show ad and wait until it finished then enter the game from the callback handler method bellow */
#if UNITY_IOS
            //* Temporary pause time on ios devices while showing ad */
            Time.timeScale = 0;
            AudioListener.pause = true;
#endif
            CurrentPurposeRewardAd = PurposeRewardAd.PlayDailyPuzzle;
            Advertising.ShowRewardedAd();
        }

        public void UnlockTournamentChapter(string id, bool isSubscribe)
        {
            currentTournamentPuzzleIsSubscribed = isSubscribe;
            currentDailyPuzzleId = id;

            //* Allow to unlock tournament chapter if player subscribed */
            if (isSubscribe)
            {
                PuzzleManager.Instance.UnlockDailyChapter(id);
                return;
            }

            //* Allow to unlock tournament chapter in editor */
            if (Application.isEditor)
            {
                PuzzleManager.Instance.UnlockDailyChapter(id);
                return;
            }

            if (IsRewardedAdReady == false)
            {
                //* Can't show rewarded ad */
                confirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION, I2.Loc.ScriptLocalization.FAIL_LOAD_AD_MSG, I2.Loc.ScriptLocalization.OK.ToUpper(), "", () => { });
                return;
            }

            if (AdDisplayer.IsAllowToShowAd() == false)
            {
                PuzzleManager.Instance.UnlockDailyChapter(id);
                return;
            }

#if UNITY_IOS
            //* Temporary pause time on ios devices while showing ad */
            Time.timeScale = 0;
            AudioListener.pause = true;
#endif
            CurrentPurposeRewardAd = PurposeRewardAd.UnlockDailyChapter;
            Advertising.ShowRewardedAd();
        }

        //* this handle the callback from watch rewarded ad to play the game */
        private void OnRewardedAdCompleted(RewardedAdNetwork arg1, AdPlacement arg2)
        {
            if (CurrentPurposeRewardAd == PurposeRewardAd.UnlockDailyChapter)
            {
                PuzzleManager.Instance.UnlockDailyChapter(currentDailyPuzzleId);
                return;
            }

            if (CurrentPurposeRewardAd == PurposeRewardAd.PlayDailyPuzzle)
            {
                PuzzleManager.Instance.UnlockDailyChapter(currentDailyPuzzleId);
                PlayAPuzzle(currentDailyPuzzleId);
                return;
            }
        }

        public void EnterGamePlay(string id)
        {
            PuzzleManager.Instance.SelectPuzzle(id);
            StartGame();
        }

        // Called when the player died
        public void GameOver()
        {
            GameState = GameState.GameOver;
            GameCount++;
        }

        // Start a new game
        public void RestartGame(float delay = 0)
        {
            isRestart = true;
            StartCoroutine(CRRestartGame(delay));
        }

        IEnumerator CRRestartGame(float delay = 0)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void PauseGame()
        {
            GameState = GameState.Paused;
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            PlayerDb.Save();
            bool isExitFromLevel = newState == GameState.Prepare &&
                (oldState == GameState.Paused || oldState == GameState.GameOver);
            bool isSelectNextLevel =
                newState == GameState.Playing && oldState == GameState.GameOver;
            bool isMultiplayerMode = MultiplayerManager.Instance != null;

            if ((isExitFromLevel || isSelectNextLevel) && !isMultiplayerMode)
            {
                if (AdDisplayer.IsAllowToShowAd() && Advertising.IsInterstitialAdReady() && AdsFrequencyManager.Instance.IsAppropriateFrequencyForInterstitial()
                     && !Advertising.IsAdRemoved() && InAppPurchaser.Instance != null && !InAppPurchaser.Instance.IsSubscibed() && Application.internetReachability != NetworkReachability.NotReachable)
                {
#if UNITY_IOS
                    Time.timeScale = 0;
                    AudioListener.pause = true;
#endif
                    Advertising.ShowInterstitialAd();
                }
            }
            UpdateBannerAdDisplay();
        }

        private void OnPurchasedCompleted(IAPProduct obj)
        {
            UpdateBannerAdDisplay();
        }

        private void OnPurchaseRestored()
        {
            UpdateBannerAdDisplay();
        }

        private void OnSubscriptionPanelShow()
        {
            UpdateBannerAdDisplay();
        }

        private void OnSubscriptionPanelHide()
        {
            UpdateBannerAdDisplay();
        }

        private void UpdateBannerAdDisplay()
        {
            if (BannerAdShowConditionsCheck())
                Advertising.ShowBannerAd(BannerAdPosition.Bottom);
            else
                Advertising.HideBannerAd();
        }

        private bool BannerAdShowConditionsCheck()
        {
            if (UIReferences.Instance != null
            && UIReferences.Instance.subscriptionDetailPanel != null
            && UIReferences.Instance.subscriptionDetailPanel.IsShowing)
                return false;
            if (GameState != GameState.Playing)
                return false;
            if (GameManager.Instance == null)
                return false;
            if (MultiplayerManager.Instance != null)
                return false;
            if (AdDisplayer.IsAllowToShowAd() == false)
                return false;
            if (Advertising.IsAdRemoved())
                return false;
            if (InAppPurchaser.Instance.IsSubscibed())
                return false;
            if (AdsFrequencyManager.Instance == null)
                return false;
            if (AdsFrequencyManager.Instance.AllowToPlayAdsMiltStoneCheck == false)
                return false;
            return true;
        }

        private static string MAX_UNFOCUS_SESSION_TIME_KEY = "maxUnfocusSessionAllowedTime";
        private static float defaultMaxUnfocusSessionTime = 7200;
        private float startUnFocusTime = 0;

        private void OnApplicationFocus(bool focus)
        {
            if (_gameState == GameState.Playing || _gameState == GameState.Paused)
            {
                if (focus)
                {
                    float maxUnfocusSessionTimeAllowed = (CloudServiceManager.Instance.appConfig.GetFloat(MAX_UNFOCUS_SESSION_TIME_KEY) ?? defaultMaxUnfocusSessionTime);
                    if (maxUnfocusSessionTimeAllowed >= 0 && (Time.realtimeSinceStartup - startUnFocusTime) > maxUnfocusSessionTimeAllowed)
                    {
                        ForceOutInGamScene();
                        Invoke("PrepareGame", 0.05f);
                    }
                }
                else
                {
                    startUnFocusTime = Time.realtimeSinceStartup;
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // if(pauseStatus == false)
            //     ShowInterstitialAdOnPause();
        }

        private void ShowInterstitialAdOnPause()
        {
            if (MultiplayerManager.Instance != null)
                return;
            if (AdDisplayer.IsAllowToShowAd() == false)
                return;
            if (Advertising.IsAdRemoved())
                return;
            if (InAppPurchaser.Instance == null || InAppPurchaser.Instance.IsSubscibed())
                return;
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return;
            if (AdsFrequencyManager.Instance.IsAppropriateFrequencyForInterstitial() == false)
                return;
            if (Advertising.IsInterstitialAdReady() == false)
                return;
#if UNITY_IOS
            Time.timeScale = 0;
            AudioListener.pause = true;
#endif
            Advertising.ShowInterstitialAd();
        }

        public bool IsRewardedAdReady
        {
            get
            {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            if (Advertising.IsRewardedAdReady() && Application.internetReachability != NetworkReachability.NotReachable)
            {
                return true;
            }
            else
            {
                return false;
            }
#else
                return true;
#endif
            }
        }
    }
}