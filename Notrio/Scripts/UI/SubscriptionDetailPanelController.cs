using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyMobile;
using System;
using System.Text.RegularExpressions;
using TMPro;

namespace Takuzu
{
    public class SubscriptionDetailPanelController : MonoBehaviour
    {
        public static Action OnShow = delegate { };
        public static Action OnHide = delegate { };

        public const string FIRST_SUBSCRIPTION_PANEL_KEY = "FIRST_SUBSCRIPTION_PANEL__SAVE_KEY";
        [SerializeField] private RectTransform rectContainer;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform subscriptionButtonsGroup;
        [SerializeField] private SubscribeButtonController subscribeButton;
        [SerializeField] private Button restoreBtn;

        [SerializeField] private Text restoreTxt;
        [SerializeField] private Button privacyBtn;
        [SerializeField] private Button termBtn;
        public Text subscriptionTitle;
        public Text subscriptionDescribe;
        public Text payOneTimeTitle;
        public TextMeshProUGUI payOneTimeDescribe;
        public GameObject subscribedWeeklyBtn;
        public SubscribeButtonController weeklyBtnController;
        public GameObject purchasedFullGameBtnObj;
        public GameObject purchaseFullGameBtnObj;
        public Button purchaseFullGameBtn;
        public GameObject purchaseFullGameWithOriginalPriceGroup;

        public GameObject purchasedFullGameWithDiscountBtnObj;
        public GameObject purchaseFullGameWithDiscountBtnObj;
        public Button purchaseFullGameWithDiscount;
        public GameObject purchaseFullGameWithDiscountPriceGroup;
        public Text discountAmountTxt;
        public Text discountTimeTxt;
        public Image darkCover;
        public GameObject systemBtnGroup;

        [Header("Config")]
        [SerializeField] private float timeDisplay = 0.12f;
        [SerializeField] private Color normalBtnColor;
        [SerializeField] private Color subscribedBtnColor;

        public SubscriptionDetailPanelController(bool isShowing)
        {
            this.IsShowing = isShowing;

        }
        public bool IsShowing { get; private set; }
        private Coroutine displayCR;
        private VerticalLayoutGroup subscriptionGroupLayout;
        private Vector3 showPos;
        private Vector3 hidePos;
        private void Start()
        {
            subscriptionGroupLayout = subscriptionButtonsGroup.GetComponent<VerticalLayoutGroup>();
            showPos = rectContainer.localPosition;
            hidePos = showPos;
            hidePos.y = rectContainer.localPosition.y - rectContainer.rect.size.y;
            discountTimeTxt.gameObject.SetActive(false);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            darkCover.color = new Color(0, 0, 0, 0);
            darkCover.gameObject.SetActive(false);
            rectContainer.gameObject.SetActive(false);
            rectContainer.localPosition = hidePos;
            closeButton.onClick.AddListener(delegate
            {
                Hide();
            });

            restoreBtn.onClick.AddListener(() =>
            {
                InAppPurchaser.Instance.RestorePurchase();
            });

            privacyBtn.onClick.AddListener(() =>
            {
                Application.OpenURL(AppInfo.Instance.PRIVACY_POLICY_LINK);
            });

            termBtn.onClick.AddListener(() =>
            {
                Application.OpenURL(AppInfo.Instance.TERMS_OF_SERVICE_LINK);
            });

            //InAppPurchaser.SubscriptionPack weeklyPack = InAppPurchaser.Instance.subscriptionPacks[0];
            bool isWeeklySubscribed = InAppPurchaser.StaticIsSubscibed();
            weeklyBtnController.SetData(isWeeklySubscribed, isWeeklySubscribed ? subscribedBtnColor : normalBtnColor);
            weeklyBtnController.subscribeBtn.onClick.AddListener(() =>
            {
                //InAppPurchaser.Instance.Purchase(weeklyPack.productName);
            });

            purchaseFullGameBtn.onClick.AddListener(() =>
            {
                if (InAppPurchaser.Instance == null)
                    return;
                InAppPurchaser.Instance.PurchaseFullGame();
            });

            purchaseFullGameWithDiscount.onClick.AddListener(() =>
            {
                if (InAppPurchaser.Instance == null)
                    return;
                InAppPurchaser.Instance.PurchaseFullGameWithDiscount();
                StartCoroutine(UpdateDiscountGroup());
            });

            //payOneTimeDescribe.text = string.Format(I2.Loc.ScriptLocalization.ONE_TIME_PAYMENT_DESCRIPTION, InAppPurchaser.Instance.OneTimePurchaseProductPriceString);
            //subscriptionDescribe.text = string.Format(I2.Loc.ScriptLocalization.SUBSCRIBE_WEEKLY_DESCRIPTION, InAppPurchaser.Instance.GetPriceSubscription(weeklyPack.productName));
            if (isWeeklySubscribed)
            {
                subscribedWeeklyBtn.gameObject.SetActive(true);
                weeklyBtnController.gameObject.SetActive(false);
            }
            else
            {
                weeklyBtnController.gameObject.SetActive(true);
                subscribedWeeklyBtn.gameObject.SetActive(false);
            }

#if UNITY_ANDROID
            systemBtnGroup.gameObject.SetActive(false);
            restoreBtn.gameObject.SetActive(false);
            restoreTxt.color = new Color(1, 1, 1, 0);
#endif
            InAppPurchasing.PurchaseCompleted += OnPurchaseCompleted;
            InAppPurchasing.RestoreCompleted += OnRestoreCompleted;

            GameManager.GameStateChanged += OnGameStateChanged;

            StartCoroutine(UpdateDiscountGroup());
            StartCoroutine(ResetSpacingLayout());
            AdjustUI();
        }

        private void OnDestroy()
        {
            InAppPurchasing.PurchaseCompleted -= OnPurchaseCompleted;
            InAppPurchasing.RestoreCompleted -= OnRestoreCompleted;

            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState newGT, GameState oldGT)
        {
            if (newGT == GameState.Prepare)
                AutoShowSubscriptionInfoPanel();
        }

        private string AUTO_SHOW_COUNT_SAVE_KEY = "SUBSCRIPTION_PANEL_AUTO_SHOW_COUNT";
        private void AutoShowSubscriptionInfoPanel()
        {
            bool isSubscribed = IsWeeklySubscribed();
            int autoShowCount = PlayerPrefs.GetInt(AUTO_SHOW_COUNT_SAVE_KEY, 0);

            if (isSubscribed)
                return;
            if (autoShowCount >= 1)
                return;
            if (CloudServiceManager.Instance == null)
                return;
            if (InAppPurchaser.Instance == null)
                return;
            if (InAppPurchaser.Instance.IsOneTimePurchased())
                return;

            int minEToShow = 5;
            if (CloudServiceManager.Instance.appConfig != null)
                minEToShow = CloudServiceManager.Instance.appConfig.GetInt("MIN_E_BEFORE_SHOW_SUBSCRIPTION_PANEL") ?? minEToShow;
            if (EnergyManager.Instance.CurrentEnergy >= minEToShow)
                return;
            //Wait a bit before show the panel
            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                if (GameManager.Instance.GameState != GameState.Prepare)
                    return;
                Show();
                autoShowCount++;
                PlayerPrefs.SetInt(AUTO_SHOW_COUNT_SAVE_KEY, autoShowCount);
            }, 1.5f);
        }

        void OnPurchaseCompleted(IAPProduct product)
        {
            CheckSubscriptionToSetUI();
        }

        void OnRestoreCompleted()
        {
            CheckSubscriptionToSetUI();
        }

        bool IsWeeklySubscribed()
        {
            return InAppPurchaser.StaticIsSubscibed();
        }

        void CheckSubscriptionToSetUI()
        {
            bool isSubscribed = InAppPurchaser.StaticIsSubscibed();
            weeklyBtnController.SetData(isSubscribed, isSubscribed ? subscribedBtnColor : normalBtnColor);

            bool isWeeklySubscribed = InAppPurchaser.StaticIsSubscibed();
            if (isWeeklySubscribed)
            {
                subscribedWeeklyBtn.gameObject.SetActive(true);
                weeklyBtnController.gameObject.SetActive(false);
            }
            else
            {
                weeklyBtnController.gameObject.SetActive(true);
                subscribedWeeklyBtn.gameObject.SetActive(false);
            }

            if (InAppPurchaser.Instance.IsOneTimePurchased())
            {
                discountTimeTxt.gameObject.SetActive(false);
                purchaseFullGameBtnObj.SetActive(false);
                purchaseFullGameWithDiscountBtnObj.SetActive(false);
                weeklyBtnController.subscribeBtn.interactable = false;
            }
            else
            {
                purchaseFullGameBtnObj.SetActive(true);
                purchaseFullGameWithDiscountBtnObj.SetActive(true);
                weeklyBtnController.subscribeBtn.interactable = true;
            }
            weeklyBtnController.subscribeBtn.targetGraphic.color = weeklyBtnController.subscribeBtn.interactable ?
                                                                    weeklyBtnController.subscribeBtn.colors.normalColor :
                                                                    weeklyBtnController.subscribeBtn.colors.disabledColor;
            purchasedFullGameBtnObj.SetActive(!purchaseFullGameBtnObj.activeSelf);
            purchasedFullGameWithDiscountBtnObj.SetActive(!purchaseFullGameWithDiscountBtnObj.activeSelf);
            StartCoroutine(ResetSpacingLayout());
        }

        private DateTime discountEndDate = DateTime.UtcNow;
        private IEnumerator UpdateDiscountGroup()
        {
            if (InAppPurchaser.Instance == null)
            {
                purchaseFullGameWithOriginalPriceGroup.SetActive(true);
                purchaseFullGameWithDiscountPriceGroup.SetActive(!purchaseFullGameWithOriginalPriceGroup.activeSelf);
                yield break;
            }
            bool timeLeftUpdated = false;
            CloudServiceManager.OneTimePurchasingDiscountInfo info = CloudServiceManager.DefaultDiscountInfo;
            InAppPurchaser.Instance.GetDiscountTimeLeft(inf =>
            {
                timeLeftUpdated = true;
                info = inf;
            });
            yield return new WaitUntil(() => timeLeftUpdated);
            Debug.Log("info.timeLeft.TotalSeconds =" + info.timeLeft.TotalSeconds);
            if (info.timeLeft.TotalSeconds > 5)
            {
                if (InAppPurchaser.Instance.IsOneTimePurchased())
                    discountTimeTxt.gameObject.SetActive(false);
                else
                    discountTimeTxt.gameObject.SetActive(true);
                purchaseFullGameWithOriginalPriceGroup.SetActive(false);
            }
            else
            {
                discountTimeTxt.gameObject.SetActive(false);
                purchaseFullGameWithOriginalPriceGroup.SetActive(true);
            }
            this.discountEndDate = info.endDate;
            float percent = (int)((1 - (InAppPurchaser.Instance.OneTimePurchaseProductDiscountPrice / InAppPurchaser.Instance.OneTimePurchaseProductPrice)) * 100);
            discountAmountTxt.text = percent.ToString() + "%";
            purchaseFullGameWithDiscountPriceGroup.SetActive(!purchaseFullGameWithOriginalPriceGroup.activeSelf);
            if (purchaseFullGameWithOriginalPriceGroup.activeSelf)
                payOneTimeDescribe.text = string.Format(I2.Loc.ScriptLocalization.ONE_TIME_PAYMENT_DESCRIPTION, InAppPurchaser.Instance.OneTimePurchaseProductPriceString);
            else
                payOneTimeDescribe.text = string.Format(I2.Loc.ScriptLocalization.ONE_TIME_PAYMENT_DISCOUNT_DESCRIPTION, InAppPurchaser.Instance.OneTimePurchaseProductPriceString, InAppPurchaser.Instance.OneTimePurchaseProductDiscountPriceString);
            StartCoroutine(ResetSpacingLayout());
        }

        public void Show()
        {
            CheckSubscriptionToSetUI();
            IsShowing = true;
            if (displayCR != null)
                StopCoroutine(displayCR);
            StartCoroutine(CR_Display(IsShowing));
            StartCoroutine(UpdateDiscountGroup());
            StartCoroutine(ResetSpacingLayout());
            OnShow();
        }

        public void Hide()
        {
            IsShowing = false;
            if (displayCR != null)
                StopCoroutine(displayCR);
            StartCoroutine(CR_Display(IsShowing));
            OnHide();
        }

        IEnumerator CR_Display(bool isShowing)
        {
            float value = 0;
            float speed = 1 / timeDisplay;
            Vector3 startPos = rectContainer.localPosition;
            Vector3 endPos = isShowing ? showPos : hidePos;
            Color startCoverColor = darkCover.color;
            Color endCoverColor = isShowing ? new Color(0, 0, 0, 0.7f) : new Color(0, 0, 0, 0);


            if (isShowing)
            {
                rectContainer.gameObject.SetActive(true);
                darkCover.gameObject.SetActive(true);
            }
            else
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            while (value < 1)
            {
                value += Time.deltaTime * speed;
                //canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, value);
                rectContainer.localPosition = Vector3.Lerp(startPos, endPos, value);
                darkCover.color = Color.Lerp(startCoverColor, endCoverColor, value);
                yield return null;
            }

            if (isShowing)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                rectContainer.gameObject.SetActive(false);
                darkCover.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            StartCoroutine(UpdateDiscountTimeLeftCR());
        }

        private IEnumerator UpdateDiscountTimeLeftCR()
        {
            bool discounted = true;
            while (true)
            {
                yield return new WaitForSeconds(1);
                if (this.discountTimeTxt == null)
                    continue;
                TimeSpan timeLeft = (this.discountEndDate - DateTime.UtcNow);
                if (timeLeft.TotalSeconds < 0)
                {
                    //Only trigger update discount group when discount count down turn from true to false
                    if (discounted)
                        StartCoroutine(UpdateDiscountGroup());
                    discounted = false;
                    continue;
                }

                discounted = true;
                this.discountTimeTxt.text = I2.Loc.ScriptLocalization.DISCOUNT_DESCRIPTION + "\n" + "<b>" + timeLeft.Days + " " + I2.Loc.ScriptLocalization.DAYS + " " + timeLeft.Hours + " " + I2.Loc.ScriptLocalization.HOURS + " " + timeLeft.Minutes + " " + I2.Loc.ScriptLocalization.MINUTES + "</b>";
            }
        }

        IEnumerator ResetSpacingLayout()
        {
            if (subscriptionGroupLayout != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(purchaseFullGameWithDiscountPriceGroup.transform as RectTransform);
                yield return null;
                LayoutRebuilder.MarkLayoutForRebuild(purchaseFullGameWithDiscountPriceGroup.transform as RectTransform);
                yield return null;
                LayoutRebuilder.MarkLayoutForRebuild(purchaseFullGameWithDiscountPriceGroup.transform as RectTransform);
                yield return null;
                LayoutRebuilder.MarkLayoutForRebuild(purchaseFullGameWithDiscountPriceGroup.transform as RectTransform);
            }
        }

        private void AdjustUI()
        {
            Vector2 offsetMax = rectContainer.offsetMax;
            Vector2 offsetMin = rectContainer.offsetMin;

            if ((float)Screen.width / Screen.height >= 0.725f)
            {
                offsetMax.x = -110;
                offsetMin.x = 110;
                rectContainer.offsetMax = offsetMax;
                rectContainer.offsetMin = offsetMin;
                return;
            }

            if ((float)Screen.width / Screen.height >= 0.64f)
            {
                offsetMax.x = -70;
                offsetMin.x = 70;
                rectContainer.offsetMax = offsetMax;
                rectContainer.offsetMin = offsetMin;
                return;
            }
        }
    }
}
