using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyMobile;
using Pinwheel;
using System;
using UnityEngine.SceneManagement;

namespace Takuzu
{
    public class PlayingModeAdUI : MonoBehaviour
    {
		[HideInInspector]
        public GameObject globalUiBlocker;
		[HideInInspector]
        public OverlayUIController overlayController;
		[HideInInspector]
        public InputHandler inputHandler;
		[HideInInspector]
        public GameObject adPreparationGroup;
		[HideInInspector]
        public Text adCountDownText;
		[HideInInspector]
        public ColorAnimation fadeAnim;
        public Color fadeOriginalColor;
		[HideInInspector]
        public ColorAnimation popupAnim;
        public int countdownSeconds = 10;
        public int lockInteractionSeconds = 3;
        public int checkIntervalSeconds = 10;

        private int remainingTimeSeconds;

        //public const string AD_PREPARATION_MESSAGE = "Time to relax!\nAn ad will be served shortly in ${a} second${b}...";

        private void OnEnable()
        {
            adPreparationGroup.gameObject.SetActive(false);
            globalUiBlocker.gameObject.SetActive(false);

            adCountDownText.text = countdownSeconds.ToString();
        }

        private void Awake()
        {
			if(UIReferences.Instance!=null){
				UpdateReferences();
			}
			UIReferences.UiReferencesUpdated += UpdateReferences;

            GameManager.GameStateChanged += OnGameStateChanged;
        }

		private void UpdateReferences()
		{
			globalUiBlocker = UIReferences.Instance.GlobleUIBlocker;
			overlayController = UIReferences.Instance.overlayUIController;
			inputHandler = UIReferences.Instance.InputHandler;
			adPreparationGroup = UIReferences.Instance.adPreparation;
			adCountDownText = UIReferences.Instance.adPreparationCountDownText;
			fadeAnim = UIReferences.Instance.adPreparationFadeAnim;
			popupAnim = UIReferences.Instance.adPreparationPopupAnim;
		}

		private void OnDestroy()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
			UIReferences.UiReferencesUpdated -= UpdateReferences;
        }

        //private void Update()
        //{
        //    if (adPreparationGroup.activeInHierarchy)
        //    {
        //        adCountDownText.enabled = overlayController.ShowingPanelCount == 0;
        //    }
        //}

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Prepare || newState == GameState.GameOver)
            {
                adPreparationGroup.gameObject.SetActive(false);
                globalUiBlocker.gameObject.SetActive(false);
                inputHandler.enabled = true;
                CancelInvoke();
                StopAllCoroutines();
            }
            if (newState == GameState.Playing && (oldState == GameState.Prepare || oldState == GameState.GameOver))
            {
                AdsFrequencyManager.Instance.SetLastPlayingShowTime(Time.time);
            }
            if (newState == GameState.Playing)
            {
                if (!SceneManager.GetActiveScene().name.Equals("Multiplayer"))
                    StartInGameIntervalAds();
                //if (!IsInvoking("CheckForShowingAd"))
                //    InvokeRepeating("CheckForShowingAd", 1, checkIntervalSeconds);
            }
        }

        private void CheckForShowingAd()
        {
//            string s = string.Format("Check for playing mode ads");
//            Debug.Log(s);

            if (AdsFrequencyManager.Instance.IsAppropriateFrequencyForPlayingModeAd())
            {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                if (Advertising.IsInterstitialAdReady())
                {
                    StartCoroutine(CrPrepareToShowAd());
                }
#elif UNITY_EDITOR
                StartCoroutine(CrPrepareToShowAd());
#endif
            }

        }

        private IEnumerator CrPrepareToShowAd(Action callback = null)
        {
            //CancelInvoke("CheckForShowingAd");
            adPreparationGroup.gameObject.SetActive(true);
            popupAnim.Play(0);
            fadeAnim.GetComponent<Image>().color = fadeOriginalColor;
            fadeAnim.Play(0);
            remainingTimeSeconds = countdownSeconds;

            while (remainingTimeSeconds > 0)
            {
                remainingTimeSeconds -= 1;
                adCountDownText.text = remainingTimeSeconds.ToString();

                if (remainingTimeSeconds <= lockInteractionSeconds)
                {
                    globalUiBlocker.gameObject.SetActive(true);
                    inputHandler.enabled = false;
                }

                yield return new WaitForSeconds(1f);
            }
            AdsFrequencyManager.Instance.SetLastPlayingShowTime(Time.time);
            if (AdDisplayer.IsAllowToShowAd() && Advertising.IsInterstitialAdReady() && !Advertising.IsAdRemoved() && InAppPurchaser.Instance != null
                && !InAppPurchaser.Instance.IsSubscibed() && Application.internetReachability != NetworkReachability.NotReachable)
            {
#if UNITY_IOS
             Time.timeScale = 0;
             AudioListener.pause = true;
#endif
                Advertising.ShowInterstitialAd();
            }

            adPreparationGroup.gameObject.SetActive(false);
            globalUiBlocker.gameObject.SetActive(false);
            inputHandler.enabled = true;
            //InvokeRepeating("CheckForShowingAd", 1, checkIntervalSeconds);
            if (callback != null)
                callback();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                adPreparationGroup.gameObject.SetActive(false);
                globalUiBlocker.gameObject.SetActive(false);
                inputHandler.enabled = true;
                CancelInvoke();
                StopAllCoroutines();
            }
            else
            {
                if (GameManager.Instance.GameState == GameState.Playing)
                {
                    //InvokeRepeating("CheckForShowingAd", 1, checkIntervalSeconds);
                    if (!SceneManager.GetActiveScene().name.Equals("Multiplayer"))
                        StartInGameIntervalAds();
                }
            }
        }

        private void StartInGameIntervalAds()
        {
            if (!Advertising.IsAdRemoved() && InAppPurchaser.Instance != null 
                && !InAppPurchaser.Instance.IsSubscibed() && Application.internetReachability != NetworkReachability.NotReachable)
            {
                StopCoroutine(InGameIntervalAdsCR());
                StartCoroutine(InGameIntervalAdsCR());
            }
        }

        private IEnumerator InGameIntervalAdsCR()
        {
            if (GameManager.Instance.GameState.Equals(GameState.Playing))
            {
                Debug.Log("Start interval Ads");
                yield return new WaitUntil(() => GameManager.Instance.GameState == GameState.Playing);
                yield return new WaitUntil(() => AdsFrequencyManager.Instance.IsAppropriateFrequencyForPlayingModeAd());
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            yield return new WaitUntil(() => Advertising.IsInterstitialAdReady());
#endif
                if (!Advertising.IsAdRemoved())
                {
                    Debug.Log("Start count down");
                    StartCoroutine(CrPrepareToShowAd(() =>
                    {
                        if (!SceneManager.GetActiveScene().name.Equals("Multiplayer")) StartInGameIntervalAds();
                    }));
                }
            }
        }
    }
}