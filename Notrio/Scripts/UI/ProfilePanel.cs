using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using GameSparks.Core;
using GameSparks.Api.Responses;
using System.Threading;
using Pinwheel;
using Takuzu.Generator;

namespace Takuzu
{
    public class ProfilePanel : OverlayPanel
    {
        public UiGroupController controller;
        public GameObject container;
        public RawImage avatar;
        public Mask avatarMask;
        public Texture2D defaultAvatar;
        public Text playerName;
        public Text rank;
        public Image rankIcon;
        public Text exp;
        public Slider expSlider;
        public Button closeButton;
        public Button loginButton;
        public Button logoutButton;
        public Button inviteButton;
        public Button syncButton;
        public AnimController syncAnim;
        public ColorAnimation syncResultAnim;
        public Image syncIcon;
        public Color syncIconInitColor;
        public CanvasGroup socialButtonGroup;
        [HideInInspector]
        public ConfirmationDialog dialog;

        [Header("Animation")]
        public float animationSpeed;

        [Header("All time")]
        public Text allTimePlayedValue;
        public Text allTimeSolvedValue;
        public Image allTimePlayedChartPiece;
        public Image allTimeSolvedChartPiece;

        [Header("Daily challenge")]
        public Text dailyTotalValue;
        public Text dailyPlayedValue;
        public Text dailySolvedValue;
        public Image dailyTotalChartPiece;
        public Image dailyPlayedChartPiece;
        public Image dailySolvedChartPiece;

        [Header("Weekly challenge")]
        public Text weeklyTotalValue;
        public Text weeklyPlayedValue;
        public Text weeklySolvedValue;
        public Image weeklyTotalChartPiece;
        public Image weeklyPlayedChartPiece;
        public Image weeklySolvedChartPiece;

        private int allTimePlayedCount;
        private int allTimeSolvedCount;
        private float allTimeSolvedChartFillAmount;

        private int dailyTotalCount;
        private int dailyPlayedCount;
        private int dailySolvedCount;
        private float dailyPlayedChartFillAmount;
        private float dailySolvedChartFillAmount;

        private int weeklyTotalCount;
        private int weeklyPlayedCount;
        private int weeklySolvedCount;
        private float weeklyPlayedChartFillAmount;
        private float weeklySolvedChartFillAmount;

        public const string GUEST_NAME = "GUEST";
        public const string AVATAR_LOCAL_PATH_KEY = "AVATAR_LOCAL_PATH";
        public const string PLAYER_NAME_KEY = "PLAYER_NAME";

        public override void Show()
        {
            controller.ShowIfNot();
            IsShowing = true;
            transform.BringToFront();
            UpdateExpInfo();
            UpdateDetailInfo();
            onPanelStateChanged(this, true);

            // syncIcon.color = syncIconInitColor;
            // syncAnim.transform.rotation = Quaternion.identity;
            // if (CloudServiceManager.isSyncing)
            //     syncAnim.Play();
        }

        public override void Hide()
        {
            controller.HideIfNot();
            IsShowing = false;
            onPanelStateChanged(this, false);
        }

        private void Awake()
        {
            // if (UIReferences.Instance != null)
            // {
            //     UpdateReferences();
            // }
            // UIReferences.UiReferencesUpdated += UpdateReferences;

            // SocialManager.onFbLogin += OnFbLogin;
            // SocialManager.onFbLogout += OnFbLogout;
            // CloudServiceManager.onLoginGameSpark += OnLoginGameSpark;
            // PlayerDb.Resetted += OnPlayerDbResetted;
            // CloudServiceManager.onPlayerDbSyncBegin += OnSyncBegin;
            // CloudServiceManager.onPlayerDbSyncEnd += OnSyncEnd;
            // CloudServiceManager.onPlayerDbSyncSucceed += OnSyncSucceed;
            // CloudServiceManager.onPlayerDbSyncFailed += OnSyncFailed;
        }

        private void UpdateReferences()
        {
            // dialog = UIReferences.Instance.overlayConfirmDialog;
        }

        private void OnDestroy()
        {
            // SocialManager.onFbLogin -= OnFbLogin;
            // SocialManager.onFbLogout -= OnFbLogout;
            // CloudServiceManager.onLoginGameSpark -= OnLoginGameSpark;
            // PlayerDb.Resetted -= OnPlayerDbResetted;
            // CloudServiceManager.onPlayerDbSyncBegin -= OnSyncBegin;
            // CloudServiceManager.onPlayerDbSyncEnd -= OnSyncEnd;
            // CloudServiceManager.onPlayerDbSyncSucceed -= OnSyncSucceed;
            // CloudServiceManager.onPlayerDbSyncFailed -= OnSyncFailed;
            // UIReferences.UiReferencesUpdated -= UpdateReferences;
        }

        private void Start()
        {
            // playerName.text = GUEST_NAME;
            // UpdateChartImmediately();

            // closeButton.onClick.AddListener(delegate
            //     {
            //         Hide();
            //     });

            // loginButton.onClick.AddListener(delegate
            //     {
            //         SocialManager.Instance.LoginFb();
            //     });

            // logoutButton.onClick.AddListener(delegate
            //     {
            //         if (!PlayerDb.IsUpToDate())
            //         {
            //             dialog.Show(
            //                 I2.Loc.ScriptLocalization.ATTENTION,
            //                 "Lost Data",
            //                 I2.Loc.ScriptLocalization.SYNC, () =>
            //                 {
            //                     syncButton.onClick.Invoke();
            //                 },
            //                 I2.Loc.ScriptLocalization.LOG_OUT, () =>
            //                 {
            //                     SocialManager.Instance.LogoutFB();
            //                 },
            //                 null);
            //         }
            //         else
            //         {
            //             SocialManager.Instance.LogoutFB();
            //         }
            //     });

            // inviteButton.onClick.AddListener(delegate
            //     {
            //         SocialManager.Instance.InviteFriend();
            //     });

            // syncButton.onClick.AddListener(delegate
            //     {
            //         if (CloudServiceManager.IsLoggedInToGameSpark)
            //         {
            //             CloudServiceManager.Instance.SyncPlayerDb();
            //         }
            //         else
            //         {
            //             syncAnim.Play();
            //             CloudServiceManager.Instance.LoginToGameSpark(
            //                 (r) =>
            //                 {
            //                     syncAnim.Stop();
            //                 });
            //         }
            //     });

            // if (CloudServiceManager.IsLoggedInToGameSpark)
            // {
            //     loginButton.gameObject.SetActive(false);
            //     playerName.text = CloudServiceManager.playerName;
            //     PlayerPrefs.SetString(PLAYER_NAME_KEY, CloudServiceManager.playerName);
            //     if (gameObject.activeInHierarchy)
            //         StartCoroutine(LoadAvatar());
            // }
            // else if (SocialManager.Instance.IsLoggedInFb)
            // {
            //     OnFbLogin(true);
            // }

            // socialButtonGroup.blocksRaycasts = true;
        }

        private void Update()
        {
            // if (!IsShowing)
            //     return;

            // if (SocialManager.Instance.IsLoggedInFb)
            // {
            //     loginButton.gameObject.SetActive(false);
            //     logoutButton.gameObject.SetActive(true);
            //     inviteButton.gameObject.SetActive(true);
            //     syncButton.gameObject.SetActive(true);
            // }
            // else
            // {
            //     loginButton.gameObject.SetActive(true);
            //     logoutButton.gameObject.SetActive(false);
            //     inviteButton.gameObject.SetActive(false);
            //     syncButton.gameObject.SetActive(false);
            // }


            // UpdateChartAnim();
            // //UpdateChartImmediately();
        }

        private void OnFbLogin(bool loggedIn)
        {
            // if (loggedIn)
            // {
            //     loginButton.gameObject.SetActive(false);
            //     inviteButton.gameObject.SetActive(true);
            //     logoutButton.gameObject.SetActive(true);
            //     playerName.text = PlayerPrefs.GetString(PLAYER_NAME_KEY, GUEST_NAME);
            //     string avatarLocalPath = PlayerPrefs.GetString(AVATAR_LOCAL_PATH_KEY, "");
            //     if (File.Exists(avatarLocalPath))
            //     {
            //         Texture2D avatarTex = new Texture2D(180, 180);
            //         avatarTex.LoadImage(File.ReadAllBytes(avatarLocalPath));
            //         avatarTex.Apply();
            //         avatar.texture = avatarTex;
            //         avatar.enabled = true;
            //         avatarMask.enabled = true;
            //     }
            //     else
            //     {
            //         //avatar.texture = defaultAvatar;
            //         avatar.enabled = false;
            //         avatarMask.enabled = false;
            //     }
            // }
            // else
            // {
            //     playerName.text = GUEST_NAME;
            //     loginButton.gameObject.SetActive(true);
            //     inviteButton.gameObject.SetActive(false);
            //     logoutButton.gameObject.SetActive(false);
            //     //avatar.texture = defaultAvatar;
            //     avatar.enabled = false;
            //     avatarMask.enabled = false;
            // }
        }

        private void OnFbLogout()
        {
            // playerName.text = GUEST_NAME;
            // PlayerPrefs.DeleteKey(AVATAR_LOCAL_PATH_KEY);
            // PlayerPrefs.DeleteKey(PLAYER_NAME_KEY);
            // if (avatar.texture != defaultAvatar)
            //     Destroy(avatar.texture);
            // avatar.enabled = false;
            // avatarMask.enabled = false;
            //avatar.texture = defaultAvatar;
            //avatar.RecalculateMasking();
            //ResetCharts();
        }

        private IEnumerator LoadAvatar()
        {
            yield return null;
            // string avatarUrl = string.Format("https://graph.facebook.com/{0}/picture?type={1}",
            //                        SocialManager.Instance.AccessTokenFb.UserId,
            //                        "large");
            // WWW w = new WWW(avatarUrl);
            // yield return w;
            // if (string.IsNullOrEmpty(w.error))
            // {
            //     if (avatar.texture != defaultAvatar)
            //         Destroy(avatar.texture);
            //     avatar.texture = w.texture;
            //     avatar.enabled = true;
            //     avatarMask.enabled = true;
            //     string path = Application.persistentDataPath;
            //     byte[] avatarBytes = w.texture.EncodeToPNG();
            //     bool saveAvatarCompleted = false;
            //     string avatarLocalPath = Path.Combine(path, "avatar.png");
            //     PlayerPrefs.SetString(AVATAR_LOCAL_PATH_KEY, avatarLocalPath);
            //     Thread t = new Thread(() =>
            //         {
            //             File.WriteAllBytes(avatarLocalPath, avatarBytes);
            //             saveAvatarCompleted = true;
            //         });
            //     t.Start();
            //     yield return new WaitUntil(() =>
            //         {
            //             return saveAvatarCompleted == true;
            //         });
            // }
            // else
            // {
            //     //avatar.texture = defaultAvatar;
            //     avatar.enabled = false;
            //     avatarMask.enabled = false;
            // }
        }

        private IEnumerator CrRequestNameRepeating(float interval)
        {
            yield return null;
            // bool willRetry = true;
            // bool isRequesting = false;
            // while (willRetry)
            // {
            //     if (!isRequesting)
            //     {
            //         yield return new WaitForSeconds(interval);
            //         if (Application.internetReachability != NetworkReachability.NotReachable)
            //         {
            //             isRequesting = true;
            //             print("request user name");
            //             SocialManager.Instance.RequestUserName(
            //                 (n) =>
            //                 {
            //                     playerName.text = n;
            //                     PlayerPrefs.SetString(PLAYER_NAME_KEY, n);
            //                     willRetry = false;
            //                     isRequesting = false;
            //                 },
            //                 (n) =>
            //                 {
            //                     willRetry = true;
            //                     isRequesting = false;
            //                 });
            //         }
            //     }
            //     yield return null;
            // }
        }

        private void OnLoginGameSpark(AuthenticationResponse response)
        {
            // loginButton.gameObject.SetActive(false);
            // if (response.HasErrors)
            // {
            //     StartCoroutine(CrRequestNameRepeating(5));
            // }
            // else
            // {
            //     playerName.text = response.DisplayName;
            //     PlayerPrefs.SetString(PLAYER_NAME_KEY, response.DisplayName);
            // }

            // if (gameObject.activeInHierarchy)
            //     StartCoroutine(LoadAvatar());
        }

        private void UpdateExpInfo()
        {
            // rank.text = ExpProfile.active.rank[PlayerInfoManager.Instance.info.level];
            // rankIcon.sprite = ExpProfile.active.icon[PlayerInfoManager.Instance.info.level];
            // rankIcon.color = rankIcon.sprite == null ? Color.clear : Color.white;
            // exp.text = string.Format("{0} XP", ExpProfile.active.ToTotalExp(PlayerInfoManager.Instance.info));
            // float normValue = (float)PlayerInfoManager.Instance.info.exp / ExpProfile.active.exp[PlayerInfoManager.Instance.info.level];
            // expSlider.normalizedValue = normValue;
        }

        private void OnPlayerDbResetted()
        {
            // CoroutineHelper.Instance.DoActionDelay(() =>
            //     {
            //         UpdateExpInfo();
            //         UpdateDetailInfo();
            //     }, 0.5f);
        }

        private void OnSyncSucceed()
        {
            // if (syncResultAnim.gameObject.activeInHierarchy)
            //     syncResultAnim.Play(0);
            // CoroutineHelper.Instance.DoActionDelay(() =>
            //     {
            //         UpdateExpInfo();
            //         UpdateDetailInfo();
            //     }, 0.5f);
        }

        private void OnSyncFailed(string error)
        {
            // if (syncResultAnim.gameObject.activeInHierarchy)
            //     syncResultAnim.Play(1);
        }

        private void OnSyncBegin()
        {
            // if (!syncAnim.isPlaying && syncAnim.gameObject.activeInHierarchy)
            //     syncAnim.Play();
            // socialButtonGroup.interactable = false;
            // socialButtonGroup.blocksRaycasts = false;
        }

        private void OnSyncEnd()
        {
            // syncAnim.Stop();
            // socialButtonGroup.interactable = true;
            // socialButtonGroup.blocksRaycasts = true;
        }

        private void UpdateDetailInfo()
        {
            // UpdateAllTimeInfo();
            // UpdateDailyChallengeInfo();
            // UpdateWeeklyChallengeInfo();
        }

        private void UpdateAllTimeInfo()
        {
            // allTimePlayedCount = PlayerDb.CountKeyStartWith(PuzzleManager.PLAYED_PREFIX);
            // allTimeSolvedCount = PlayerDb.CountKeyStartWith(PuzzleManager.SOLVED_PREFIX);
            // allTimeSolvedChartFillAmount = (float)allTimeSolvedCount / allTimePlayedCount;
        }

        private void UpdateDailyChallengeInfo()
        {
            // dailyTotalCount = PuzzleManager.Instance.GetReceivedChallengeCount(PuzzleManager.DAILY_PUZZLE_PREFIX);
            // dailyPlayedCount = PlayerDb.CountKeyStartWith(string.Format("{0}{1}", PuzzleManager.PLAYED_PREFIX, PuzzleManager.DAILY_PUZZLE_PREFIX));
            // dailySolvedCount = PlayerDb.CountKeyStartWith(string.Format("{0}{1}", PuzzleManager.SOLVED_PREFIX, PuzzleManager.DAILY_PUZZLE_PREFIX));
            // dailyPlayedChartFillAmount = (float)dailyPlayedCount / dailyTotalCount;
            // dailySolvedChartFillAmount = (float)dailySolvedCount / dailyTotalCount;
        }

        private void UpdateWeeklyChallengeInfo()
        {
            // weeklyTotalCount = PuzzleManager.Instance.GetReceivedChallengeCount(PuzzleManager.WEEKLY_PUZZLE_PREFIX);
            // weeklyPlayedCount = PlayerDb.CountKeyStartWith(string.Format("{0}{1}", PuzzleManager.PLAYED_PREFIX, PuzzleManager.WEEKLY_PUZZLE_PREFIX));
            // weeklySolvedCount = PlayerDb.CountKeyStartWith(string.Format("{0}{1}", PuzzleManager.SOLVED_PREFIX, PuzzleManager.WEEKLY_PUZZLE_PREFIX));
            // weeklyPlayedChartFillAmount = (float)weeklyPlayedCount / weeklyTotalCount;
            // weeklySolvedChartFillAmount = (float)weeklySolvedCount / weeklyTotalCount;
        }

        private void ResetCharts()
        {
            // allTimePlayedChartPiece.fillAmount = 0;
            // allTimeSolvedChartPiece.fillAmount = 0;

            // dailyTotalChartPiece.fillAmount = 0;
            // dailyPlayedChartPiece.fillAmount = 0;
            // dailySolvedChartPiece.fillAmount = 0;

            // weeklyTotalChartPiece.fillAmount = 0;
            // weeklyPlayedChartPiece.fillAmount = 0;
            // weeklySolvedChartPiece.fillAmount = 0;
        }

        private void UpdateChartImmediately()
        {
            // allTimePlayedChartPiece.fillAmount = 1;
            // allTimeSolvedChartPiece.fillAmount = allTimeSolvedChartFillAmount;
            // allTimePlayedValue.text = allTimePlayedCount.ToString();
            // allTimeSolvedValue.text = allTimeSolvedCount.ToString();

            // dailyTotalChartPiece.fillAmount = 1;
            // dailyPlayedChartPiece.fillAmount = dailyPlayedChartFillAmount;
            // dailySolvedChartPiece.fillAmount = dailySolvedChartFillAmount;
            // dailyTotalValue.text = dailyTotalCount.ToString();
            // dailyPlayedValue.text = dailyPlayedCount.ToString();
            // dailySolvedValue.text = dailySolvedCount.ToString();

            // weeklyTotalChartPiece.fillAmount = 1;
            // weeklyPlayedChartPiece.fillAmount = weeklyPlayedChartFillAmount;
            // weeklySolvedChartPiece.fillAmount = weeklySolvedChartFillAmount;
            // weeklyTotalValue.text = weeklyTotalCount.ToString();
            // weeklyPlayedValue.text = weeklyPlayedCount.ToString();
            // weeklySolvedValue.text = weeklySolvedCount.ToString();
        }

        private void UpdateChartAnim()
        {
            // allTimePlayedChartPiece.fillAmount = Mathf.MoveTowards(allTimePlayedChartPiece.fillAmount, 1, animationSpeed * 2);
            // allTimeSolvedChartPiece.fillAmount = Mathf.MoveTowards(allTimeSolvedChartPiece.fillAmount, allTimeSolvedChartFillAmount, animationSpeed);
            // allTimePlayedValue.text = allTimePlayedCount.ToString();
            // allTimeSolvedValue.text = allTimeSolvedCount.ToString();

            // dailyTotalChartPiece.fillAmount = Mathf.MoveTowards(dailyTotalChartPiece.fillAmount, 1, animationSpeed * 2);
            // dailyPlayedChartPiece.fillAmount = Mathf.MoveTowards(dailyPlayedChartPiece.fillAmount, dailyPlayedChartFillAmount, animationSpeed);
            // dailySolvedChartPiece.fillAmount = Mathf.MoveTowards(dailySolvedChartPiece.fillAmount, dailySolvedChartFillAmount, animationSpeed);
            // dailyTotalValue.text = dailyTotalCount.ToString();
            // dailyPlayedValue.text = dailyPlayedCount.ToString();
            // dailySolvedValue.text = dailySolvedCount.ToString();

            // weeklyTotalChartPiece.fillAmount = Mathf.MoveTowards(weeklyTotalChartPiece.fillAmount, 1, animationSpeed * 2);
            // weeklyPlayedChartPiece.fillAmount = Mathf.MoveTowards(weeklyPlayedChartPiece.fillAmount, weeklyPlayedChartFillAmount, animationSpeed);
            // weeklySolvedChartPiece.fillAmount = Mathf.MoveTowards(weeklySolvedChartPiece.fillAmount, weeklySolvedChartFillAmount, animationSpeed);
            // weeklyTotalValue.text = weeklyTotalCount.ToString();
            // weeklyPlayedValue.text = weeklyPlayedCount.ToString();
            // weeklySolvedValue.text = weeklySolvedCount.ToString();
        }
    }
}