using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using EasyMobile;
using System;

namespace Takuzu
{
    public class SettingPanel : OverlayPanel
    {
        public UiGroupController controller;
        public Button closeButton;
        public Button removeAdButton;
        public Image removeAdBackground;
        public Color removeAdBackgroundActiveColor;
        public Color removeAdBackgroundInactiveColor;
        public Button restorePurchaseButton;
        public Toggle soundToggle;
        public Toggle musicToggle;
        public Button tutorialButton;
        public Button creditButton;
        public Button moreGameButton;
        public GameObject loginContainer;
        public GameObject logonContainer;
        public Button loginBtn;
        public Button logoutBtn;
        public Button changeLanguageButton;
        public Button inviteButton;
        public Text versionText;
        public Button policyBtn;
        public Button termsBtn;
        public string moreGameLink;
		[HideInInspector]
        public CreditPanel creditPanel;

		private void Awake() {
			if(UIReferences.Instance!=null){
				UpdateReferences();
			}
			UIReferences.UiReferencesUpdated += UpdateReferences;
            CloudServiceManager.onPlayerDbSyncEnd += EnableConnectButtons;
            SocialManager.onFailConnectFb += EnableConnectButtons;

#if UNITY_ANDROID
            restorePurchaseButton.gameObject.SetActive(false);
#endif
        }

        void EnableConnectButtons()
        {
            loginBtn.interactable = true;
            logoutBtn.interactable = true;
            inviteButton.interactable = true;
        }

        void DisableConnectButtons()
        {
            loginBtn.interactable = false;
            logoutBtn.interactable = false;
            inviteButton.interactable = false;
        }

        IEnumerator CR_DelayEnableConnectBtn()
        {
            DisableConnectButtons();
            yield return new WaitForSeconds(1);
            EnableConnectButtons();
        }
		private void UpdateReferences()
		{
			creditPanel = UIReferences.Instance.overlayCreditPanel;
		}

        public override void Show()
        {
            controller.ShowIfNot();
            IsShowing = true;
            transform.BringToFront();
            onPanelStateChanged(this, true);
            UpdateButtons();
            UpdateSocialGroup(SocialManager.Instance.IsLoggedInFb);
        }

        public override void Hide()
        {
            controller.HideIfNot();
            IsShowing = false;
            onPanelStateChanged(this, false);
            StopUpdateButtons();
        }

        private void UpdateButtons()
        {
            InvokeRepeating("UpdateRemoveAdsButton", 0.15f, 1);
            soundToggle.isOn = !SoundManager.Instance.IsSoundMuted();
            musicToggle.isOn = !SoundManager.Instance.IsMusicMuted();
            UpdateNotificationButton();
        }

        private void StopUpdateButtons()
        {
            CancelInvoke("UpdateRemoveAdsButton");
        }

        private void Start()
        {
            closeButton.onClick.AddListener(delegate
            {
                Hide();
            });

            removeAdButton.onClick.AddListener(delegate
            {
                if (UIReferences.Instance == null)
                    return;
                if (UIReferences.Instance.subscriptionDetailPanel == null)
                    return;
                UIReferences.Instance.subscriptionDetailPanel.Show();
            });

            restorePurchaseButton.onClick.AddListener(delegate
            {
                InAppPurchaser.Instance.RestorePurchase();
            });

            soundToggle.onValueChanged.AddListener((isOn) =>
            {
                SoundManager.Instance.SetSoundMute(!isOn);
            });

            musicToggle.onValueChanged.AddListener((isOn) =>
            {
                SoundManager.Instance.SetMusicMute(!isOn);
            });

            tutorialButton.onClick.AddListener(delegate
            {
                SceneLoadingManager.Instance.LoadTutorialScene();
            });

            moreGameButton.onClick.AddListener(delegate
            {
#if UNITY_IOS
                if (AppInfo.Instance != null)
                    Application.OpenURL(AppInfo.Instance.APPSTORE_HOMEPAGE);
#else
                if (AppInfo.Instance != null)
                    Application.OpenURL(AppInfo.Instance.PLAYSTORE_HOMEPAGE);
#endif
            });

            creditButton.onClick.AddListener(delegate
            {
                ShowCredit();
            });

            loginBtn.onClick.AddListener(delegate
            {
                DisableConnectButtons();
            });
            logoutBtn.onClick.AddListener(delegate
            {
                StartCoroutine(CR_DelayEnableConnectBtn());
                SocialManager.Instance.LogoutFB();
            });
            inviteButton.onClick.AddListener(delegate
            {
                SocialManager.Instance.InviteFriend();
            });
            changeLanguageButton.onClick.AddListener(() =>
            {
                LanguageSettingOverlayUI.Instance.Show();
                Hide();
            });

            if(LanguageSettingOverlayUI.Instance != null)
            {
                LanguageSettingOverlayUI.Instance.closeBtn.onClick.AddListener(() =>
                {
                    Show();
                });
            }

            policyBtn.onClick.AddListener(() =>
            {
                Application.OpenURL(AppInfo.Instance.PRIVACY_POLICY_LINK);
            });

            termsBtn.onClick.AddListener(() =>
            {
                Application.OpenURL(AppInfo.Instance.TERMS_OF_SERVICE_LINK);
            });

            versionText.text = String.Format("{0} {1}", I2.Loc.ScriptLocalization.VERSION, Application.version);
            SocialManager.onFbLogin += OnFBLogin;
            SocialManager.onFbLogout += OnFBLogout;
        }

        private void OnDestroy()
        {
            SocialManager.onFbLogin -= OnFBLogin;
            SocialManager.onFbLogout -= OnFBLogout;
			UIReferences.UiReferencesUpdated -= UpdateReferences;
            CloudServiceManager.onPlayerDbSyncEnd -= EnableConnectButtons;
            SocialManager.onFailConnectFb -= EnableConnectButtons;
        }

        private void OnFBLogout()
        {
            UpdateSocialGroup(false);
        }

        private void OnFBLogin(bool logon)
        {
            UpdateSocialGroup(logon);
            if (!logon)
            {
                EnableConnectButtons();
            }
        }

        private void UpdateSocialGroup(bool isLogon)
        {
            loginContainer.SetActive(!isLogon);
            logonContainer.SetActive(isLogon);
        }

        private void UpdateRemoveAdsButton()
        {
            if (AdDisplayer.IsAllowToShowAd() == false)
            {
                removeAdButton.interactable = false;
                removeAdBackground.raycastTarget = false;
                removeAdBackground.color = removeAdBackgroundInactiveColor;
            }
            else
            {
                removeAdButton.interactable = true;
                removeAdBackground.raycastTarget = false;
                removeAdBackground.color = removeAdBackgroundActiveColor;
            }
        }

        private void UpdateNotificationButton()
        {
            //if (NotificationManager.Instance.PushPermission == OSNotificationPermission.Authorized)
            //{
            //    dailyNotificationToggle.interactable = true;
            //    weeklyNotificationToggle.interactable = true;
            //    dailyNotificationToggle.isOn = false;
            //    weeklyNotificationToggle.isOn = false;
            //    notificationInstructionText.gameObject.SetActive(false);
            //}
            //else
            //{
            //    dailyNotificationToggle.interactable = false;
            //    weeklyNotificationToggle.interactable = false;
            //    dailyNotificationToggle.isOn = false;
            //    weeklyNotificationToggle.isOn = false;
            //    if (NotificationManager.Instance.PushPermission == OSNotificationPermission.Denied)
            //    {
            //        notificationInstructionText.gameObject.SetActive(true);
            //    }
            //}
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus && enabled)
            {
                UpdateNotificationButton();
            }
        }

        private void ShowCredit()
        {
            Hide();
            creditPanel.callingSource = this;
            creditPanel.Show();
        }
    }
}