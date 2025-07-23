using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyMobile;
using GameSparks.Core;
using System;

namespace Takuzu
{
    public class AdsFrequencyManager : MonoBehaviour
    {
        public static AdsFrequencyManager Instance { get; private set; }

        public float interstitialFrequencyCapSeconds = 120;
        public float videoFrequencyCapSeconds = 120;
        public float playingFrequencyCapSeconds = 420;
        public bool showAdsInGame = true;

        private float lastInterstitialShowTime;
        private float lastVideoShowTime;
        private float lastPlayingShowTime;

        public const string INTERSTITIAL_FREQUENCY_CAP_KEY = "INTERSTITIAL_FREQUENCY_CAP";
        public const string VIDEO_FREQUENCY_CAP_KEY = "VIDEO_FREQUENCY_CAP";
        public const string PLAYING_FREQUENCY_CAP_KEY = "PLAYING_FREQUENCY_CAP";

        private void OnEnable()
        {
            Advertising.InterstitialAdCompleted += OnInterstitialAdCompleted;
            Advertising.RewardedAdCompleted += OnRewardedAdCompleted;
            Advertising.RewardedAdSkipped += OnRewardedAdSkip;

            CloudServiceManager.onConfigLoaded += OnConfigLoaded;
        }

        private void OnDisable()
        {
            Advertising.InterstitialAdCompleted -= OnInterstitialAdCompleted;
            Advertising.RewardedAdCompleted -= OnRewardedAdCompleted;
            Advertising.RewardedAdSkipped -= OnRewardedAdSkip;

            CloudServiceManager.onConfigLoaded -= OnConfigLoaded;
        }

        private void OnRewardedAdSkip(RewardedAdNetwork arg1, AdPlacement arg2)
        {
#if UNITY_IOS
            Time.timeScale = 1;
            AudioListener.pause = false;
#endif
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                DestroyImmediate(this);
                return;
            }

            lastInterstitialShowTime = float.MinValue;
            lastVideoShowTime = float.MinValue;
        }

        private void Start()
        {
            if (CloudServiceManager.Instance.appConfig != null)
            {
                ApplyConfig(CloudServiceManager.Instance.appConfig);
            }
        }

        private int minLevelAllowedToShowAd
        {
            get
            {
                return PlayerPrefs.GetInt("MinLevelAllowedToShowAdCache", 2);
            }
            set
            {
                PlayerPrefs.SetInt("MinLevelAllowedToShowAdCache", value);
            }
        }
        private int MinLevelAllowedToShowAd
        {
            get
            {
                if(CloudServiceManager.Instance == null)
                    return minLevelAllowedToShowAd;
                if(CloudServiceManager.Instance.appConfig == null)
                    return minLevelAllowedToShowAd;
                minLevelAllowedToShowAd = CloudServiceManager.Instance.appConfig.GetInt("startShowAdsAtMilestone") ?? minLevelAllowedToShowAd;
                return minLevelAllowedToShowAd;
            }
        }

        private bool allowToPlayAdsMileStoneCheck
        {
            get
            {
                return PlayerPrefs.GetInt("AllowToPlayAdsMileStoneCheck", 0) == 1;
            }
            set
            {
                PlayerPrefs.SetInt("AllowToPlayAdsMileStoneCheck", value ? 1 : 0);
            }
        }
        public bool AllowToPlayAdsMiltStoneCheck {
            get 
            { 
                if(StoryPuzzlesSaver.Instance == null)
                    return this.allowToPlayAdsMileStoneCheck;
                this.allowToPlayAdsMileStoneCheck = StoryPuzzlesSaver.Instance.MaxNode + 2 >= MinLevelAllowedToShowAd;
                return this.allowToPlayAdsMileStoneCheck;
            } 
        }

        private void OnRewardedAdCompleted(RewardedAdNetwork arg1, AdPlacement arg2)
        {
#if UNITY_IOS
            Time.timeScale = 1;
            AudioListener.pause = false;
#endif
            lastVideoShowTime = Time.time;
        }

        private void OnRewardedAdCompleted()
        {
#if UNITY_IOS
            Time.timeScale = 1;
            AudioListener.pause = false;
#endif
            lastVideoShowTime = Time.time;
        }


        private void OnInterstitialAdCompleted(InterstitialAdNetwork arg1, AdPlacement arg2)
        {
#if UNITY_IOS
            Time.timeScale = 1;
            AudioListener.pause = false;
#endif
            lastInterstitialShowTime = Time.time;
        }

        private void OnInterstitialAdCompleted()
        {
#if UNITY_IOS
            Time.timeScale = 1;
            AudioListener.pause = false;
#endif
            lastInterstitialShowTime = Time.time;
        }

        public bool IsAppropriateFrequencyForInterstitial()
        {
            if (!showAdsInGame)
                return false;
            return Time.time - Mathf.Max(lastInterstitialShowTime, lastVideoShowTime) > interstitialFrequencyCapSeconds && AllowToPlayAdsMiltStoneCheck;
        }

        public bool IsAppropriateFrequencyForVideo()
        {
            if (!showAdsInGame)
                return false;
            return Time.time - lastVideoShowTime > videoFrequencyCapSeconds;
        }

        public bool IsAppropriateFrequencyForPlayingModeAd()
        {
            if (!showAdsInGame)
                return false;
#if UNITY_EDITOR
            return Time.time - lastPlayingShowTime > playingFrequencyCapSeconds;
#elif UNITY_ANDROID || UNITY_IOS
            return Time.time - lastPlayingShowTime > playingFrequencyCapSeconds && IsAppropriateFrequencyForInterstitial();
#endif
        }

        public void SetLastPlayingShowTime(float time)
        {
            lastPlayingShowTime = time;
        }

        private void OnConfigLoaded(GSData config)
        {
            ApplyConfig(config);
        }

        private void ApplyConfig(GSData config)
        {
            float? iCap = config.GetFloat("interstitialFrequencyCapSeconds");
            if (iCap.HasValue)
                interstitialFrequencyCapSeconds = iCap.Value;
            float? vCap = config.GetFloat("videoFrequencyCapSeconds");
            if (vCap.HasValue)
                videoFrequencyCapSeconds = vCap.Value;
            float? pCap = config.GetFloat("playingFrequencyCapSeconds");
            if (pCap.HasValue)
                playingFrequencyCapSeconds = pCap.Value;
            bool? showAds = config.GetBoolean("showAds");
            if (showAds.HasValue)
                showAdsInGame = showAds.Value;
        }
    }
}