﻿using UnityEngine;
using System.Collections;
using System;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace Takuzu
{
    public class AdDisplayer : MonoBehaviour
    {

        public static bool IsAllowToShowAd()
        {
            bool monthlyPurchased = InAppPurchaser.StaticIsSubscibed();

            if (monthlyPurchased || InAppPurchaser.Instance.IsOneTimePurchased())
                return false;
            return true;
        }

        public enum BannerAdPos
        {
            Top,
            Bottom
        }

        public static AdDisplayer Instance { get; private set; }

        [Header("Banner Ad Display Config")]
        [Tooltip("Whether or not to show banner ad")]
        public bool showBannerAd = true;
        public BannerAdPos bannerAdPosition = BannerAdPos.Bottom;

        [Header("Interstitial Ad Display Config")]
        [Tooltip("Whether or not to show interstitial ad")]
        public bool showInterstitialAd = true;
        [Tooltip("Show interstitial ad every [how many] games")]
        public int gamesPerInterstitial = 3;
        [Tooltip("How many seconds after game over that interstitial ad is shown")]
        public float showInterstitialDelay = 2f;

        [Header("Rewarded Ad Display Config")]
        [Tooltip("Check to allow watching ad to earn coins")]
        public bool watchAdToEarnCoins = true;
        [Tooltip("How many coins the user earns after watching a rewarded ad")]
        public int rewardedCoins = 50;

        void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

#if EASY_MOBILE
        public static event System.Action CompleteRewardedAdToRecoverLostGame;
        public static event System.Action CompleteRewardedAdToEarnCoins;

        private static int gameCount = 0;
        
        void OnEnable()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
        }

        void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        void Start()
        {
            // Show banner ad
            if (showBannerAd && !Advertising.IsAdRemoved())
            {
                Advertising.ShowBannerAd(bannerAdPosition == BannerAdPos.Top ? BannerAdPosition.Top : BannerAdPosition.Bottom);
            }
        }

        void OnGameStateChanged(GameState newState, GameState oldState)
        {       
            if (newState == GameState.GameOver)
            {
                // Show interstitial ad
                if (AdDisplayer.IsAllowToShowAd() && showInterstitialAd && !Advertising.IsAdRemoved())
                {
                    gameCount++;

                    if (gameCount >= gamesPerInterstitial)
                    {
                        if (Advertising.IsInterstitialAdReady())
                        {
                            // Show default ad after some optional delay
                            StartCoroutine(ShowInterstitial(showInterstitialDelay));

                            // Reset game count
                            gameCount = 0;
                        }
                    }
                }
            }
        }

        IEnumerator ShowInterstitial(float delay = 0f)
        {        
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            if (AdDisplayer.IsAllowToShowAd() && Advertising.IsInterstitialAdReady() && !Advertising.IsAdRemoved() && InAppPurchaser.Instance != null
                && !InAppPurchaser.Instance.IsSubscibed() && Application.internetReachability != NetworkReachability.NotReachable)
            {
#if UNITY_IOS
             Time.timeScale = 0;
             AudioListener.pause = true;
#endif
                Advertising.ShowInterstitialAd();
            }
        }

        public bool CanShowRewardedAd()
        {
            return Advertising.IsRewardedAdReady();
        }

        public void ShowRewardedAdToRecoverLostGame()
        {
            if (CanShowRewardedAd())
            {
                Advertising.RewardedAdCompleted += OnCompleteRewardedAdToRecoverLostGame;
                if (Advertising.IsRewardedAdReady())
                {
#if UNITY_IOS
             Time.timeScale = 0;
             AudioListener.pause = true;
#endif
                    Advertising.ShowRewardedAd();
                }
            }
        }

        void OnCompleteRewardedAdToRecoverLostGame(RewardedAdNetwork adNetwork, AdPlacement location)
        {
            // Unsubscribe
            Advertising.RewardedAdCompleted -= OnCompleteRewardedAdToRecoverLostGame;

            // Fire event
            if (CompleteRewardedAdToRecoverLostGame != null)
            {
                CompleteRewardedAdToRecoverLostGame();
            }
        }

        public void ShowRewardedAdToEarnCoins()
        {
            if (CanShowRewardedAd())
            {
                Advertising.RewardedAdCompleted += OnCompleteRewardedAdToEarnCoins;
                if (Advertising.IsRewardedAdReady())
                {
#if UNITY_IOS
             Time.timeScale = 0;
             AudioListener.pause = true;
#endif
                    Advertising.ShowRewardedAd();
                }
            }
        }

        void OnCompleteRewardedAdToEarnCoins(RewardedAdNetwork adNetwork, AdPlacement location)
        {
            // Unsubscribe
            Advertising.RewardedAdCompleted -= OnCompleteRewardedAdToEarnCoins;

            // Fire event
            if (CompleteRewardedAdToEarnCoins != null)
            {
                CompleteRewardedAdToEarnCoins();
            }
        }
#endif
    }
}
