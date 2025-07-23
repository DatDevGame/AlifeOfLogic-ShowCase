using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace Takuzu
{
    public class InAppPurchaser : MonoBehaviour
    {
        public static InAppPurchaser Instance { get; private set; }

        public static string SaveKeySuffixes = "SaveKey";

        [System.Serializable]
        public struct CoinPack
        {
            public string productName;
            public string priceString;
            public int coinValue;
        }

        [System.Serializable]
        public struct SubscriptionPack
        {
            public string productName;
            public string priceString;
        }

        [Header("Remove Ads products")]
        public string removeAds = "Remove_Ads";
        public string removeAdsPrice;

        [Header("Coin pack products")]
        public CoinPack[] coinPacks;

        [Header("Supscription pack products")]
        public SubscriptionPack[] subscriptionPacks;

        [Header("Full game purchased product")]
        public string oneTimePurchaseProductName;
        [SerializeField] private string oneTimePurchaseProductPriceString;
        [SerializeField] private float oneTimePurchaseProductPrice;
        public string OneTimePurchaseProductPriceString
        {
            get
            {
                if (InAppPurchasing.IsInitialized() == false)
                    return GetCachePriceString(this.oneTimePurchaseProductName, this.oneTimePurchaseProductPriceString);
                return "";
            }
        }
        public float OneTimePurchaseProductPrice
        {
            get
            {
                if (InAppPurchasing.IsInitialized() == false)
                    return GetCachePrice(this.oneTimePurchaseProductName, this.oneTimePurchaseProductPrice);
                return 0;
            }
        }

        [Header("Full game purchased discount product")]
        public string oneTimePurchaseDiscountProductName;
        [SerializeField] private string oneTimePurchaseDiscountPriceString;
        [SerializeField] private float oneTimePurchaseDiscountPrice;
        public string OneTimePurchaseProductDiscountPriceString
        {
            get
            {
                if (InAppPurchasing.IsInitialized() == false)
                    return GetCachePriceString(this.oneTimePurchaseDiscountProductName, this.oneTimePurchaseDiscountPriceString);
                return "";
            }
        }
        public float OneTimePurchaseProductDiscountPrice
        {
            get
            {
                if (InAppPurchasing.IsInitialized() == false)
                    return GetCachePrice(this.oneTimePurchaseDiscountProductName, this.oneTimePurchaseDiscountPrice);
                return 0;
            }
        }

        private string ProductPriceStringCacheSaveKey(string productName)
        {
            return string.Format("{0}_Cache_Price_String", productName);
        }

        private void UpdateCachePriceString(string productName, string priceString)
        {
            PlayerPrefs.SetString(ProductPriceStringCacheSaveKey(productName), priceString);
        }

        private string GetCachePriceString(string productName, string defaultPriceString)
        {
            return PlayerPrefs.GetString(ProductPriceStringCacheSaveKey(productName), defaultPriceString);
        }

        private string ProductPriceCacheSaveKey(string productName)
        {
            return string.Format("{0}_Cache_Price", productName);
        }

        private void UpdateCachePrice(string productName, float price)
        {
            PlayerPrefs.SetFloat(ProductPriceCacheSaveKey(productName), price);
        }

        private float GetCachePrice(string productName, float defaultPrice)
        {
            return PlayerPrefs.GetFloat(ProductPriceCacheSaveKey(productName), defaultPrice);
        }

        public void PurchaseFullGame()
        {
            if (InAppPurchasing.IsInitialized() == false)
                return;
            InAppPurchasing.Purchase(oneTimePurchaseProductName);
        }

        bool productIsOwn = false;
        bool discountProductIsOwn = false;
        public bool IsOneTimePurchased()
        {
            if (InAppPurchasing.IsInitialized() == false)
                return CacheProductIsOwn(oneTimePurchaseProductName) || CacheProductIsOwn(oneTimePurchaseDiscountProductName);
            if (productIsOwn || discountProductIsOwn)
                return true;

            productIsOwn = InAppPurchasing.IsProductOwned(oneTimePurchaseProductName);
            UpdateCacheProductIsOwn(oneTimePurchaseProductName, productIsOwn);

            discountProductIsOwn = InAppPurchasing.IsProductOwned(oneTimePurchaseDiscountProductName);
            UpdateCacheProductIsOwn(oneTimePurchaseDiscountProductName, discountProductIsOwn);

            return productIsOwn || discountProductIsOwn;
        }

        public void PurchaseFullGameWithDiscount()
        {
            if (InAppPurchasing.IsInitialized() == false)
                return;
            GetDiscountTimeLeft(info =>
            {
                if (info.timeLeft.TotalSeconds < 5)
                {
                    //* Discount nearly end
                    //! Not allow to purchase discount product
                    //InAppPurchasing.Purchase(oneTimePurchaseProductName);
                    return;
                }
                InAppPurchasing.Purchase(oneTimePurchaseDiscountProductName);
            });
        }

        public void GetDiscountTimeLeft(Action<CloudServiceManager.OneTimePurchasingDiscountInfo> callback)
        {
            if (CloudServiceManager.Instance == null)
                callback(CloudServiceManager.DefaultDiscountInfo);
            else
                CloudServiceManager.Instance.GetOneTimePurchasingDiscountTimeLeft(info =>
                {
                    callback(info);
                });
        }

        private bool CacheProductIsOwn(string productName)
        {
            return PlayerPrefs.GetInt(productName, 0) == 1;
        }

        private void UpdateCacheProductIsOwn(string productName, bool isOwned)
        {
            PlayerPrefs.SetInt(productName, isOwned ? 1 : 0);
        }

        public static bool StaticIsSubscibed()
        {
            if (Instance == null)
                return GetCacheSubscription();
            return Instance.IsSubscibed();
        }

        public bool IsSubscibed()
        {
            bool isSubscribed = false;
            foreach (var subPack in Instance.subscriptionPacks)
            {
                if (IsValidSubscription(subPack.productName))
                {
                    isSubscribed = true;
                    break;
                }
            }
            UpdateSubscriptionCache(isSubscribed);
            return isSubscribed;
        }

        private static string SubscriptionCacheSaveKey = "SUBSCRIPTION_CACHE";
        private static void UpdateSubscriptionCache(bool isSubscribed)
        {
            PlayerPrefs.SetInt(SubscriptionCacheSaveKey, isSubscribed ? 1 : 0);
        }

        private static bool GetCacheSubscription()
        {
            return PlayerPrefs.GetInt(SubscriptionCacheSaveKey, 0) == 0;
        }

        /*
            Clear subscription info cache when subscription information may change
            Cache subscription info
            Cache will be cleared when
                - OnPurchased
                - OnRestorePurchased
                - Subscribed but expired
        */
        private static Dictionary<string, SubscriptionInfoWrapper> subscriptionInfoDict = new Dictionary<string, SubscriptionInfoWrapper>();
        public static bool IsValidSubscription(string subscriptionName)
        {
            //if (InAppPurchasing.IsInitialized() == false)
            //    GetCacheSubscriptionIsValid(subscriptionName);

            if (subscriptionInfoDict.ContainsKey(subscriptionName) == false)
                subscriptionInfoDict.Add(subscriptionName, null);
            SubscriptionInfoWrapper subInfo = subscriptionInfoDict[subscriptionName];
            bool validSubscription = false;
            if (subInfo != null)
            {
                validSubscription = subInfo.isTrial() || (subInfo.isSubscribed() && subInfo.isExpired() == false);
                // Debug.LogFormat("Subscription Info : Trial {0}, Subscribed {1}, Expired {2}, AutoRenewing {3}, IntroductoryPeriod {4}, Cancelled {5}",
                // subInfo.isTrial(), subInfo.isSubscribed(), subInfo.isExpired(), subInfo.isAutoRenewing(), subInfo.isIntroductoryPricePeriod(), subInfo.isCancelled());
            }
            //UpdateCacheSubscriptionIsValid(subscriptionName, validSubscription);
            //* Clear cache when it's expired if the current subscription is subscribed and not expired
            if (validSubscription)
                WaitUntilExpiredAndClearSubscriptionValidCache(subscriptionName, subInfo);

            return validSubscription;
        }

        //* Make sure that only one timer is running per subscription
        private static Dictionary<string, WaitForSecondRealTimeRunner> expiredWaitingCoroutine = new Dictionary<string, WaitForSecondRealTimeRunner>();
        private static void WaitUntilExpiredAndClearSubscriptionValidCache(string subscriptionName, SubscriptionInfoWrapper subscriptionInfo)
        {
            if (Instance == null)
                return;
            if (expiredWaitingCoroutine.ContainsKey(subscriptionName) && expiredWaitingCoroutine[subscriptionName].IsRunning)
                return;
            if (expiredWaitingCoroutine.ContainsKey(subscriptionName))
                expiredWaitingCoroutine.Remove(subscriptionName);
            expiredWaitingCoroutine.Add(subscriptionName, new WaitForSecondRealTimeRunner(Instance,
            () => subscriptionInfoDict.Remove(subscriptionName),
            (float)subscriptionInfo.getRemainingTime().TotalSeconds));
        }
        private class WaitForSecondRealTimeRunner
        {
            private MonoBehaviour mono;
            private bool isRunning = false;
            public bool IsRunning { get { return this.isRunning; } }

            public WaitForSecondRealTimeRunner(MonoBehaviour mono, Action action, float time)
            {
                this.mono = mono;
                isRunning = true;
                mono.StartCoroutine(WaitForSecondRealTimeCR(() =>
                {
                    this.isRunning = false;
                    action.Invoke();
                }, time));
            }

            private IEnumerator WaitForSecondRealTimeCR(Action action, float timeSec)
            {
                yield return new WaitForSecondsRealtime(timeSec);
                try
                {
                    action.Invoke();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Error on coroutine: " + e.ToString());
                }
            }
        }


        private interface SubscriptionInfoWrapper
        {
            bool isSubscribed();
            bool isExpired();
            TimeSpan getRemainingTime();
            bool isTrial();
            bool isCancelled();
            bool isIntroductoryPricePeriod();
            bool isAutoRenewing();
        }

        private class EditorSubscriptionInfoWrapper : SubscriptionInfoWrapper
        {
            static float generatedTime = -1;

            public EditorSubscriptionInfoWrapper()
            {
                if (generatedTime == -1)
                    generatedTime = Time.time;
            }

            public TimeSpan getRemainingTime()
            {
                return TimeSpan.FromSeconds(180 - (Time.time - generatedTime));
            }

            public bool isAutoRenewing()
            {
                return false;
            }

            public bool isCancelled()
            {
                return false;
            }

            public bool isExpired()
            {
                return getRemainingTime().TotalSeconds < 0;
            }

            public bool isIntroductoryPricePeriod()
            {
                return false;
            }

            public bool isSubscribed()
            {
                return true;
            }

            public bool isTrial()
            {
                return false;
            }
        }

        //private class EMSubscriptionInfoWrapper : SubscriptionInfoWrapper
        //{
        //    SubscriptionInfo subscriptionInfo;

        //    public EMSubscriptionInfoWrapper(SubscriptionInfo subscriptionInfo)
        //    {
        //        this.subscriptionInfo = subscriptionInfo;
        //    }

        //    public TimeSpan getRemainingTime()
        //    {
        //        return this.subscriptionInfo.getRemainingTime();
        //    }

        //    public bool isAutoRenewing()
        //    {
        //        return this.isAutoRenewing();
        //    }

        //    public bool isCancelled()
        //    {
        //        return this.subscriptionInfo.isCancelled() == Result.True;
        //    }

        //    public bool isExpired()
        //    {
        //        return this.subscriptionInfo.isExpired() == Result.True;
        //    }

        //    public bool isIntroductoryPricePeriod()
        //    {
        //        return this.subscriptionInfo.isIntroductoryPricePeriod() == Result.True;
        //    }

        //    public bool isSubscribed()
        //    {
        //        return this.subscriptionInfo.isSubscribed() == Result.True;
        //    }

        //    public bool isTrial()
        //    {
        //        return this.subscriptionInfo.isFreeTrial() == Result.True;
        //    }
        //}

        //private static bool GetCacheSubscriptionIsValid(string subscriptionName)
        //{
        //    return PlayerPrefs.GetInt(subscriptionName + SaveKeySuffixes, 0) == 1;
        //}

        //private static void UpdateCacheSubscriptionIsValid(string subscriptionName, bool isValid)
        //{
        //    PlayerPrefs.SetInt(subscriptionName + SaveKeySuffixes, isValid ? 1 : 0);
        //}

        //void Awake()
        //{
        //    if (Instance == null)
        //    {
        //        Instance = this;
        //    }
        //    else
        //    {
        //        DestroyImmediate(gameObject);
        //        return;
        //    }
        //    DontDestroyOnLoad(gameObject);
        //}

#if EASY_MOBILE

        void OnEnable()
        {
            InAppPurchasing.PurchaseCompleted += OnPurchaseCompleted;
            InAppPurchasing.RestoreCompleted += OnRestoreCompleted;
            InAppPurchasing.RestoreFailed += OnRestoreFailed;
        }

        void OnDisable()
        {
            InAppPurchasing.PurchaseCompleted -= OnPurchaseCompleted;
            InAppPurchasing.RestoreCompleted -= OnRestoreCompleted;
            InAppPurchasing.RestoreFailed -= OnRestoreFailed;
        }

        // Successful purchase handler
        void OnPurchaseCompleted(IAPProduct product)
        {
            string name = product.Name;

            if (name.Equals(removeAds))
            {
                // Purchase of Remove Ads
                Advertising.RemoveAds();
            }
            else
            {
                // Purchase of coin packs
                foreach (CoinPack pack in coinPacks)
                {
                    if (pack.productName.Equals(name))
                    {
                        // Grant the user with their purchased coins
                        CoinManager.Instance.AddCoins(pack.coinValue);
                        break;
                    }
                }
            }

            subscriptionInfoDict.Clear();
        }

        // Successful purchase restoration handler
        void OnRestoreCompleted()
        {
            NativeUI.Alert("Restoration Completed", "Your in-app purchases were restored successfully.");

            subscriptionInfoDict.Clear();
        }

        // Failed purchase restoration handler
        void OnRestoreFailed()
        {
            NativeUI.Alert("Restoration Failed", "Please try again later.");
        }

#endif

        // Buy an IAP product using its name
        public void Purchase(string productName)
        {
#if EASY_MOBILE
            if (InAppPurchasing.IsInitialized())
            {
                InAppPurchasing.Purchase(productName);
            }
            else
            {
                NativeUI.Alert("Service Unavailable", "Please check your internet connection.");
            }
#endif
        }

        // Restore purchase
        public void RestorePurchase()
        {
#if EASY_MOBILE
            if (InAppPurchasing.IsInitialized())
            {
                InAppPurchasing.RestorePurchases();
            }
            else
            {
                NativeUI.Alert("Service Unavailable", "Please check your internet connection.");
            }
#endif
        }

        public string GetPriceCoinPack(string name)
        {
            string price = "$1.49";
            for (int i = 0; i < coinPacks.Length; i++)
            {
                if (name.Equals(coinPacks[i].productName))
                {
                    price = coinPacks[i].priceString;
#if EM_UIAP && !UNITY_EDITOR
                    ProductMetadata data = InAppPurchasing.GetProductLocalizedData(name);

                    if (data != null)
                    {
                        price = data.localizedPriceString;
                    }
#endif
                    return price;
                }
            }
            return price;
        }

        public string GetPriceSubscription(string name)
        {
            string price = "$1.49";
            for (int i = 0; i < subscriptionPacks.Length; i++)
            {
                if (name.Equals(subscriptionPacks[i].productName))
                {
                    price = subscriptionPacks[i].priceString;
#if EM_UIAP && !UNITY_EDITOR
                    ProductMetadata data = InAppPurchasing.GetProductLocalizedData(name);

                    if (data != null)
                    {
                        price = data.localizedPriceString;
                    }
#endif
                    return price;
                }
            }
            return price;
        }

        public string GetLocalizePackName(int packIndex)
        {
            if (packIndex < subscriptionPacks.Length)
            {
                switch (packIndex)
                {
                    case 0:
                        return I2.Loc.ScriptLocalization.SUBSCRIBE_PACK_NAME_1.ToUpper();
                    case 1:
                        return I2.Loc.ScriptLocalization.SUBSCRIBE_PACK_NAME_2.ToUpper();
                    case 2:
                        return I2.Loc.ScriptLocalization.SUBSCRIBE_PACK_NAME_3.ToUpper();
                    default:
                        return null;
                }
            }
            else
                return null;
        }

        public bool CheckTrialStateOfSubscribePack(string subscriptionName)
        {
            if (InAppPurchasing.IsInitialized())
            {
                if (InAppPurchasing.IsProductOwned(subscriptionName))
                {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                    return true;
#else
                    return true;
#endif
                }
                else
                    return true;
            }
            else
            {
                return false;
            }
        }

        public TimeSpan GetTrialTimeSpan(string subscriptionName)
        {
            if (CloudServiceManager.Instance == null)
                return GetCacheTrialTimeSpan(subscriptionName);
            if (CloudServiceManager.Instance.appConfig == null)
                return GetCacheTrialTimeSpan(subscriptionName);
            if (CloudServiceManager.Instance.appConfig.ContainsKey(GetTrialTimeSpanCacheSaveKey(subscriptionName)) == false)
                return GetCacheTrialTimeSpan(subscriptionName);

            TimeSpan trialTS = TimeSpan.FromDays(CloudServiceManager.Instance.appConfig.GetFloat(GetTrialTimeSpanCacheSaveKey(subscriptionName)) ?? GetCacheTrialTimeSpan(subscriptionName).TotalDays);
            UpdateCacheTrialTimeSpan(subscriptionName, trialTS);
            return trialTS;
        }

        private string GetTrialTimeSpanCacheSaveKey(string subscriptionName)
        {
            return string.Format("Subscription_{0}_Trial_Time", subscriptionName);
        }

        private void UpdateCacheTrialTimeSpan(string subscriptionName, TimeSpan timeSpan)
        {
            PlayerPrefs.SetFloat(GetTrialTimeSpanCacheSaveKey(subscriptionName), (float)timeSpan.TotalDays);
        }

        private TimeSpan GetCacheTrialTimeSpan(string subscriptionName)
        {
            if (PlayerPrefs.HasKey(GetTrialTimeSpanCacheSaveKey(subscriptionName)) == false)
                return new TimeSpan(3, 0, 0, 0);
            return TimeSpan.FromDays(PlayerPrefs.GetFloat(GetTrialTimeSpanCacheSaveKey(subscriptionName)));
        }
    }
}

