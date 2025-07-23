using EasyMobile;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class HeaderUI : MonoBehaviour
    {
        public RawImage avatar;
        public Mask avatarMask;
        public Image rankIcon;
        public Slider expSlider;
        public Button profileButton;
        public Button coinShopButton;
        public Button energyShopButton;
        [HideInInspector]
        public CoinShopUI coinShop;
        [HideInInspector]
        public EnergyExchangePanel energyExchangePanel;
        [HideInInspector]
        public ProfilePanel profilePanel;
        public UiGroupController controller;
        public Texture2D defaultAvatar;
        public float showDelay;

        [Header("Debug")]
        public bool enableRandomRewardButton;
        public Button randomRewardButton;
        public CoinDisplayer CoinDisplayer;
        public EnergyDisplayer energyDisplayer;

        public const string GUEST_NAME = "GUEST";
        public const string AVATAR_LOCAL_PATH_KEY = "AVATAR_LOCAL_PATH";
        public const string PLAYER_NAME_KEY = "PLAYER_NAME";

        private void Awake()
        {
            if (UIReferences.Instance != null)
            {
                UpdateReferences();
            }
            UIReferences.UiReferencesUpdated += UpdateReferences;

            CloudServiceManager.onLoginGameSpark += OnLoginGameSpark;
            SocialManager.onFbLogin += OnFbLogin;
            SocialManager.onFbLogout += OnFbLogout;
            GameManager.GameStateChanged += OnGameStateChanged;
            PlayerInfoManager.onInfoLoaded += OnPlayerInfoLoaded;
            PlayerInfoManager.onInfoUpdated += OnPlayerInfoUpdated;
            InAppPurchasing.PurchaseCompleted += OnPurchaseCompleted;
            InAppPurchasing.RestoreCompleted += OnRestoreCompleted;
        }

        private void UpdateReferences()
        {
            coinShop = UIReferences.Instance.overlayCoinShopUI;
            energyExchangePanel = UIReferences.Instance.overlayEnergyExchangePanel;
            profilePanel = UIReferences.Instance.overlayProfilePanel;
        }

        private void OnDestroy()
        {
            CloudServiceManager.onLoginGameSpark -= OnLoginGameSpark;
            SocialManager.onFbLogin -= OnFbLogin;
            SocialManager.onFbLogout -= OnFbLogout;
            GameManager.GameStateChanged -= OnGameStateChanged;
            PlayerInfoManager.onInfoLoaded -= OnPlayerInfoLoaded;
            PlayerInfoManager.onInfoUpdated -= OnPlayerInfoUpdated;
            UIReferences.UiReferencesUpdated -= UpdateReferences;
            InAppPurchasing.PurchaseCompleted -= OnPurchaseCompleted;
            InAppPurchasing.RestoreCompleted -= OnRestoreCompleted;
        }

        private void Update()
        {
            randomRewardButton.gameObject.SetActive(!CloudServiceManager.isGuest && enableRandomRewardButton);
        }

        private void OnEnable()
        {
            if (EnergyManager.Instance != null && energyShopButton != null)
                energyShopButton.interactable = EnergyManager.Instance.AlwaysMaxEnergy() == false;
        }

        private void Start()
        {
            UpdateExpInfo();

            randomRewardButton.onClick.AddListener(delegate
            {
                if (!enableRandomRewardButton)
                    return;
                randomRewardButton.interactable = false;
                CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    randomRewardButton.interactable = true;
                }, 1);

                string eventKey = "RANDOM_REWARD";
                new LogEventRequest()
                .SetEventKey(eventKey)
                .Send((r) =>
                {
                    Debug.Log(r.JSONString);
#if UNITY_EDITOR
                    RewardUI rewardUI = FindObjectOfType<RewardUI>();
                    GameSparks.Core.GSData rewardData = r.ScriptData.GetGSData("rewardData");
                    rewardUI.OnReceiveFakeRewardData(rewardData);
#endif
                });
            });

            coinShopButton.onClick.AddListener(delegate
            {
                coinShop.Show();
            });
            energyShopButton.onClick.AddListener(delegate
            {
                energyExchangePanel.Show();
            });
            profileButton.onClick.AddListener(delegate
            {
                profilePanel.Show();
            });

            if (CloudServiceManager.IsLoggedInToGameSpark)
            {
                PlayerPrefs.SetString(PLAYER_NAME_KEY, CloudServiceManager.playerName);
                // if (gameObject.activeInHierarchy)
                //     StartCoroutine(LoadAvatar());
            }
            else if (SocialManager.Instance.IsLoggedInFb)
            {
                OnFbLogin(true);
            }

            controller.ShowIfNot();
        }

        private void OnFbLogin(bool loggedIn)
        {
            if (loggedIn)
            {
                string avatarLocalPath = PlayerPrefs.GetString(AVATAR_LOCAL_PATH_KEY, "");
                if (File.Exists(avatarLocalPath))
                {
                    Texture2D avatarTex = new Texture2D(180, 180);
                    avatarTex.LoadImage(File.ReadAllBytes(avatarLocalPath));
                    avatarTex.Apply();
                    avatar.texture = avatarTex;
                    avatar.enabled = true;
                    avatarMask.enabled = true;
                }
                else
                {
                    //avatar.texture = defaultAvatar;
                    avatar.enabled = false;
                    avatarMask.enabled = false;
                }
            }
            else
            {
                //avatar.texture = defaultAvatar;
                avatar.enabled = false;
                avatarMask.enabled = false;
            }
        }

        private void OnLoginGameSpark(AuthenticationResponse response)
        {
            // if (gameObject.activeInHierarchy)
            //     StartCoroutine(LoadAvatar());
        }

        private void OnFbLogout()
        {
            ResetHeader();
            PlayerPrefs.DeleteKey(AVATAR_LOCAL_PATH_KEY);
            PlayerPrefs.DeleteKey(PLAYER_NAME_KEY);
        }

        public void ResetHeader()
        {
            Destroy(avatar.texture);
            //avatar.texture = defaultAvatar;
            //avatar.RecalculateMasking();
            avatar.enabled = false;
            avatarMask.enabled = false;
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing)
            {
                if (EnergyManager.Instance != null && energyShopButton != null)
                    energyShopButton.interactable = EnergyManager.Instance.AlwaysMaxEnergy() == false;
                controller.HideIfNot();
            }
            else
            if (newState == GameState.Prepare)
            {
                if (EnergyManager.Instance != null && energyShopButton != null)
                    energyShopButton.interactable = EnergyManager.Instance.AlwaysMaxEnergy() == false;

                CoroutineHelper.Instance.DoActionDelay(
                    () =>
                    {
                        if (GameManager.Instance.GameState == GameState.Prepare)
                            controller.ShowIfNot();
                    },
                    showDelay);
            }
        }

        void OnPurchaseCompleted(IAPProduct product)
        {
            if (EnergyManager.Instance != null && energyShopButton != null)
                energyShopButton.interactable = EnergyManager.Instance.AlwaysMaxEnergy() == false;
        }

        void OnRestoreCompleted()
        {
            if (EnergyManager.Instance != null && energyShopButton != null)
                energyShopButton.interactable = EnergyManager.Instance.AlwaysMaxEnergy() == false;
        }

        private void OnPlayerInfoLoaded(PlayerInfo info, bool start)
        {
            UpdateExpInfo();
        }

        private void OnPlayerInfoUpdated(PlayerInfo newInfo, PlayerInfo oldInfo)
        {
            UpdateExpInfo();
        }

        private void UpdateExpInfo()
        {
            PlayerInfo info = PlayerInfoManager.Instance.info;
            int expToLevelUp = ExpProfile.active.exp[info.level];
            expSlider.maxValue = expToLevelUp;
            expSlider.value = info.exp;
            rankIcon.sprite = ExpProfile.active.icon[PlayerInfoManager.Instance.info.level];
            rankIcon.color = rankIcon.sprite == null ? Color.clear : Color.white;
        }
    }
}