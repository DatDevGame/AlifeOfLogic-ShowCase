using EasyMobile;
using GameSparks.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using Takuzu.Generator;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Takuzu
{
    public class CoinManager : MonoBehaviour
    {
        public static Action<int> onLoginReward = delegate { };
        public static Action onInviatationCodeVerifiedSuccessfully = delegate{};
        public static Action<int> onWatchingAdReward = delegate { };
        public static Action<int> onDailyChallengeReward = delegate { };
        public static Action<int> onWeeklyChallengeReward = delegate { };
        public static Action<int> onFinishTutorialFirstTimeReward = delegate { };
        public static Action<RollingItem.RollingItemData> onRewarded = delegate { };
        public static CoinManager Instance;
        public static Action<string> LowOnCoins = delegate { };
        private static bool inSufficentCoins = false;
        public static bool InSufficentCoins 
        { 
            set 
                {
                    if (value == true && inSufficentCoins == false)
                        LowOnCoins(InsufficentCoinsReason); 
                    inSufficentCoins = value; 
                } 
            get 
                {
                    return inSufficentCoins;
                } 
        }

        public static string InsufficentCoinsReason = "";
        [SerializeField]
        private int coins;
        public int Coins
        {
            get
            {
                return coins;
            }
            private set
            {
                //Debug.Log("Update Coins, old:: " + coins + " ,new:: " + value);
                coins = value;
                PlayerDb.SetString(encryptedPpkKey, Crypto.Encrypt(Coins.ToString(), cryptoKey));
                CoinsUpdated(value);
            }
        }

        public static event Action<int> CoinsUpdated = delegate { };

        [SerializeField]
        int initialCoins = 0;
        public static float rewardDelayDuration = 3;
        public static float rewardAnimationDuration = 4;

        // key name to store high score in PlayerPrefs
        public const string COINS_KEY = "COINS";
        public const string TRANSACTION_KEY = "COIN_TRANSACTION";

        public CryptoKey cryptoKey;
        public ItemPriceProfile itemPrice;
        public RewardCoinProfile rewardProfile;
        public const string luckySpinDataKey = "LuckySpinData";
        public List<RollingItem.RollingItemData> rollingItemDatas { get {
                List<string> luckySpinDatastrs = CloudServiceManager.Instance.appConfig.GetStringList(luckySpinDataKey);
                if (luckySpinDatastrs == null || luckySpinDatastrs.Count == 0)
                {
                    luckySpinDatastrs = new List<string>()
                    {
                        "c-1-coinSp-20",
                        "c-5-coinSp-60",
                        "c-10-coinSp-200",
                        "c-15-coinSp-320",
                        "c-20-coinSp-350",
                        "c-25-coinSp-285",
                        "c-30-coinSp-160",
                        "c-35-coinSp-125",
                        "c-40-coinSp-105",
                        "c-45-coinSp-85",
                        "c-50-coinSp-55",
                        "c-55-coinSp-45",
                        "c-60-coinSp-35",
                        "c-65-coinSp-30",
                        "c-70-coinSp-15",
                        "c-75-coinSp-6",
                        "c-80-coinSp-5",
                        "c-100-coinSp-1",
                        "e-1-energySp-1",
                        "e-2-energySp-1",
                        "e-3-energySp-1",
                        "e-4-energySp-1",
                        "e-5-energySp-2",
                        "e-6-energySp-50",
                        "e-7-energySp-80",
                        "e-8-energySp-110",
                        "e-9-energySp-140",
                        "e-10-energySp-155",
                        "e-11-energySp-170",
                        "e-12-energySp-155",
                        "e-13-energySp-110",
                        "e-14-energySp-100",
                        "e-15-energySp-85",
                        "e-16-energySp-65",
                        "e-17-energySp-60",
                        "e-18-energySp-45",
                        "e-19-energySp-45",
                        "e-20-energySp-40",
                        "e-21-energySp-40",
                        "e-22-energySp-30",
                        "e-23-energySp-20",
                        "e-24-energySp-15",
                        "e-25-energySp-9",
                        "e-26-energySp-7",
                        "e-27-energySp-5",
                        "e-28-energySp-2",
                        "e-29-energySp-2",
                        "e-30-energySp-1"
                    };
                }
                return luckySpinDatastrs.ConvertAll(new Converter<string, RollingItem.RollingItemData>(String2RollingItem));} }
        public static RollingItem.RollingItemData String2RollingItem(string s)
        {
            return new RollingItem.RollingItemData() { rewardCoins = (s.Split('-')[0] == "c"), amount = int.Parse(s.Split('-')[1]), bgSprite = Background.Get(s.Split('-')[2]), f = float.Parse(s.Split('-')[3]) };
        }

        public List<int> topRewardCoinList;

        private string encryptedPpkKey;
        private string encryptedInitialCoin;
        private string encryptedTransactionKey;
        public static bool luckySpinnerRewardCoin;

        private void OnEnable()
        {
            CloudServiceManager.onLoginGameSpark += OnLoginGameSpark;
            CloudServiceManager.onInvitationVerifiedSuccessfully += OnInvitationCodeVerifiedSuccessfully;
            CloudServiceManager.onConfigLoaded += OnConfigLoaded;
            CloudServiceManager.onPlayerDbSyncSucceed += OnSyncSucceed;
            PlayerDb.Resetted += OnPlayerDbReset;
            PlayerDb.RequestDecrypt += OnPlayerDbRequestDecrypt;
            PlayerDb.RequestEncrypt += OnPlayerDbRequestEncrypt;
            Judger.onCoinGained += OnCoinGained;
            LogicalBoard.onCellAboutToReveal += OnCellAboutToRevealed;
            LogicalBoard.onPuzzleReseted += OnPuzzleReseted;
            VisualBoard.onCellFlagged += OnCellFlagged;
            LogicalBoard.onCellUndone += OnCellUndo;
            Advertising.RewardedAdCompleted += OnRewardedAdCompleted;
            TutorialManager4.onFinishTutorialFirstTime += OnFinishTutorialFirstTime;
        }

        private void OnDisable()
        {
            CloudServiceManager.onLoginGameSpark -= OnLoginGameSpark;
            CloudServiceManager.onInvitationVerifiedSuccessfully -= OnInvitationCodeVerifiedSuccessfully;
            CloudServiceManager.onConfigLoaded -= OnConfigLoaded;
            CloudServiceManager.onPlayerDbSyncSucceed -= OnSyncSucceed;
            PlayerDb.Resetted -= OnPlayerDbReset;
            PlayerDb.RequestDecrypt -= OnPlayerDbRequestDecrypt;
            PlayerDb.RequestEncrypt -= OnPlayerDbRequestEncrypt;
            Judger.onCoinGained -= OnCoinGained;
            LogicalBoard.onCellAboutToReveal -= OnCellAboutToRevealed;
            LogicalBoard.onPuzzleReseted -= OnPuzzleReseted;
            VisualBoard.onCellFlagged -= OnCellFlagged;
            LogicalBoard.onCellUndone -= OnCellUndo;
            Advertising.RewardedAdCompleted -= OnRewardedAdCompleted;
            TutorialManager4.onFinishTutorialFirstTime -= OnFinishTutorialFirstTime;
        }

        public void OnInvitationCodeVerifiedSuccessfully()
        {
            onInviatationCodeVerifiedSuccessfully();
        }

        void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                encryptedPpkKey = Crypto.Encrypt(COINS_KEY, cryptoKey);
                encryptedInitialCoin = Crypto.Encrypt(initialCoins.ToString(), cryptoKey);
                encryptedTransactionKey = Crypto.Encrypt(TRANSACTION_KEY, cryptoKey);
                ItemPriceProfile.active = itemPrice;
                RewardCoinProfile.active = rewardProfile;
                InitBaseCoinRewardList();
                LoadCoin();
            }
        }

        void Start()
        {
            if (CloudServiceManager.Instance.appConfig != null)
            {
                ApplyConfig(CloudServiceManager.Instance.appConfig);
            }
        }

        void InitBaseCoinRewardList()
        {
            topRewardCoinList = new List<int>();
            int[] baseRewardList = { 10, 15, 20, 30, 45 };
            int[] sizeRewardList = { 0, 5, 10, 15 };
            int[] rankRewardList = { 8, 4, 2, 1, 1, 1, 1, 1, 1, 1 };
            for (int level = 0; level < 5; level++)
            {
                for (int size = 0; size < 4; size++)
                {
                    for (int top = 0; top < 10; top++)
                    {
                        topRewardCoinList.Add((((baseRewardList[level] + sizeRewardList[size]) * rankRewardList[top]) / 5) * 5);
                    }
                }
            }
        }

        public int GetTopRewardCoin(Size size, Level level,int top)
        {
            int indexLevel = (int)level - 1;
            int indexSize = 0;
            switch (size)
            {
                case Size.Six:
                    indexSize = 0;
                    break;
                case Size.Eight:
                    indexSize = 1;
                    break;
                case Size.Ten:
                    indexSize = 2;
                    break;
                case Size.Twelve:
                    indexSize = 3;
                    break;
            }
            int rewardIndex = indexLevel * 40 + indexSize * 10 + top;
            rewardIndex = Mathf.Min(rewardIndex, topRewardCoinList.Count - 1);
            return topRewardCoinList[rewardIndex];
        }

        public void AddCoins(int amount, string note = "")
        {
            if (amount < 0)
            {
                throw new ArgumentException("amount < 0");
            }
            Coins += amount;
            // Store new coin value
            AddTransaction(amount);
        }

        public void RemoveCoins(int amount, string note = "")
        {
            if (amount < 0)
            {
                throw new ArgumentException("amount < 0");
            }
            Coins -= amount;
            //set coin trans
            AddTransaction(-amount);
        }

        public void AddTransaction(int amount)
        {
            int trans = GetTransaction();
            trans += amount;
            PlayerDb.SetString(encryptedTransactionKey, Crypto.Encrypt(trans.ToString(), cryptoKey));
        }

        public int GetTransaction()
        {
            string transString = PlayerDb.GetString(encryptedTransactionKey, null);
            int trans = 0;
            try
            {
                trans = int.Parse(Crypto.Decrypt(transString, cryptoKey));
            }
            catch
            {
                trans = 0;
            }
            return trans;
        }

        public void ResetCoinTransaction()
        {
            Debug.Log("Reset TRANSACTION");
            PlayerDb.DeleteKey(encryptedTransactionKey);
        }

        public void LoadCoin()
        {
            string coinsString = PlayerDb.GetString(encryptedPpkKey, encryptedInitialCoin);
            try
            {
                Coins = int.Parse(Crypto.Decrypt(coinsString, cryptoKey));
            }
            catch
            {
                Debug.Log("Fail to load coins amount from playerDB");
                Coins = 0;
            }
        }

        private void OnSyncSucceed()
        {
            ResetCoinTransaction();
            LoadCoin();
        }

        private void OnLoginGameSpark(GameSparks.Api.Responses.AuthenticationResponse response)
        {
            if (!response.HasErrors && response.NewPlayer == true)
            {
                AddCoins(rewardProfile.rewardOnFbLogin);
                print(string.Format("Reward {0} coin(s) by login to FB", rewardProfile.rewardOnFbLogin));
                onLoginReward(rewardProfile.rewardOnFbLogin);
                PlayerDb.Save();
            }
        }

        private void OnFinishTutorialFirstTime(float f)
        {
            //we delay raising the event here instead of delay showing the panel because there is no reward panel in Tutorial scene
            //so we must wait until the Main scene is loaded
            CoroutineHelper.Instance.PostponeActionUntil(() =>
            {
                onFinishTutorialFirstTimeReward(rewardProfile.rewardOnFinishTutorialFirstTime);
                PlayerDb.Save();
            },
            () =>
            {
                return SceneManager.GetActiveScene().name.Equals("Main");
            });

        }

        private void OnPlayerDbReset()
        {
            Coins = 0;
        }

        private void OnCoinGained(int amount)
        {
            if(amount >=0 )
                AddCoins(amount);
            else
                RemoveCoins(-amount);
            if (amount > 0 && PuzzleManager.currentIsChallenge)
            {
                //bool isDaily = PuzzleManager.currentPuzzleId.StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX);
                //Action<int> a;
                //if (isDaily)
                //{
                //    a = onDailyChallengeReward;
                //}
                //else
                //{
                //    a = onWeeklyChallengeReward;
                //}

                //a(amount);
                PlayerDb.Save();
            }
        }

        public void ClampReward(int amount)
        {
            Debug.Log("Call reward");
            AddCoins(amount);
            Action<int> a;
            a = onDailyChallengeReward;
            a(amount);
            PlayerDb.Save();
        }

        private void OnCellAboutToRevealed(Index2D i)
        {
            if(StoryPuzzlesSaver.Instance.MaxNode >=0)
                RemoveCoins(itemPrice.revealPowerup * Powerup.Instance.CountRevealPerGame);
        }

        private void OnPuzzleReseted()
        {
            Powerup.Instance.CountRevealPerGame = Powerup.Instance.CountUndoPerGame = 0;
            RemoveCoins(itemPrice.clearPowerup);
        }

        private void OnCellFlagged(Index2D i)
        {
            RemoveCoins(itemPrice.flagPowerup);
            if (Coins < itemPrice.flagPowerup)
            {
                Powerup.Instance.SetType("none");
            }
        }

        private void OnCellUndo(Index2D i)
        {
            if (StoryPuzzlesSaver.Instance.MaxNode >= 0)
                RemoveCoins(itemPrice.undoPowerup /** Powerup.Instance.CountUndoPerGame*/);
        }

        public void FakeAdReward()
        {
            if (GameManager.CurrentPurposeRewardAd == PurposeRewardAd.GetItem)
            {
                List<RollingItem.RollingItemData> currentData = rollingItemDatas.FindAll(item => item.rewardCoins == luckySpinnerRewardCoin);
                RollingItem.RollingItemData data = RandomRollingDataWithF(currentData);
                onRewarded(data);
            }
        }

        public void CompleteAdReward(RollingItem.RollingItemData itemData)
        {
            if (itemData.rewardCoins)
            {
                AddCoins(itemData.amount);
                print(string.Format("REWARD {0} COINS BY WATCHING AD", itemData.amount));
                onWatchingAdReward(itemData.amount);
            }
            else
            {
                EnergyManager.Instance.AddEnergy(itemData.amount);
            }
        }

        private void OnRewardedAdCompleted(RewardedAdNetwork network, AdPlacement location)
        {
            List<RollingItem.RollingItemData> currentData = rollingItemDatas.FindAll(item => item.rewardCoins == luckySpinnerRewardCoin);
            RollingItem.RollingItemData data = RandomRollingDataWithF(currentData);
            onRewarded(data);
        }

        private void OnRewardedAdCompleted()
        {
            if (GameManager.CurrentPurposeRewardAd == PurposeRewardAd.GetItem)
            {
                List<RollingItem.RollingItemData> currentData = rollingItemDatas.FindAll(item => item.rewardCoins == luckySpinnerRewardCoin);
                RollingItem.RollingItemData data = RandomRollingDataWithF(currentData);
                onRewarded(data);
            }
        }

        private RollingItem.RollingItemData RandomRollingDataWithF(List<RollingItem.RollingItemData> datas)
        {
            List<float> randomRange = new List<float>();
            float max = 0;
            foreach (var item in datas)
            {
                randomRange.Add(item.f + max);
                max += item.f;
            }
            float rd = UnityEngine.Random.Range(0, max);
            int i = 0;
            while (i < randomRange.Count)
            {
                if (randomRange[i] > rd)
                    break;
                i++;
            }
            return datas[i];
        }

        private void OnConfigLoaded(GSData config)
        {
            ApplyConfig(config);
        }

        private void ApplyConfig(GSData config)
        {
            int? loginReward = config.GetInt("loginReward");
            if (loginReward.HasValue)
                rewardProfile.rewardOnFbLogin = loginReward.Value;
            int? adReward = config.GetInt("adReward");
            if (adReward.HasValue)
                rewardProfile.rewardOnWatchingAd = adReward.Value;
            int? tutorialReward = config.GetInt("tutorialReward");
            if (tutorialReward.HasValue)
                rewardProfile.rewardOnFinishTutorialFirstTime = tutorialReward.Value;

            int? revealPrice = config.GetInt("revealPrice");
            if (revealPrice.HasValue)
                itemPrice.revealPowerup = revealPrice.Value;
            int? undoPrice = config.GetInt("undoPrice");
            if (undoPrice.HasValue)
                itemPrice.undoPowerup = undoPrice.Value;

            int? shareCodeReward = config.GetInt("game_shared_rw_coins");
            if (shareCodeReward.HasValue)
                rewardProfile.rewardOnShareCode = shareCodeReward.Value;

            int? enterCodeReward = config.GetInt("invitation_rw_coins");
            if (enterCodeReward.HasValue)
                rewardProfile.rewardOnEnterCode = enterCodeReward.Value;

            for (int level = 0; level < 5; level++)
            {
                for (int size = 0; size < 4; size++)
                {
                    for (int top = 0; top < 10; top++)
                    {
                        int rewardIndex = level * 40 + size * 10 + top;
                        int? topreward = config.GetInt(String.Format("tournamentRewardFor_level{0}_size{1}_rank{2}",
                            level + 1, 6 + size * 2, top + 1));
                        if (topreward.HasValue)
                        {
                            topRewardCoinList[rewardIndex] = topreward.Value;
                        }
                    }
                }
            }
        }

        private void OnPlayerDbRequestDecrypt(Dictionary<string, object> d)
        {
            if (d.ContainsKey(encryptedPpkKey))
            {
                d.Remove(encryptedPpkKey);
                d[COINS_KEY] = Coins;
            }
            if (d.ContainsKey(encryptedTransactionKey))
            {
                d.Remove(encryptedTransactionKey);
                d[TRANSACTION_KEY] = GetTransaction();
            }
        }

        private void OnPlayerDbRequestEncrypt(Dictionary<string, object> d)
        {
            if (d.ContainsKey(COINS_KEY))
            {
                d[encryptedPpkKey] = Crypto.Encrypt(d[COINS_KEY].ToString(), cryptoKey);
                d.Remove(COINS_KEY);
            }
            if (d.ContainsKey(TRANSACTION_KEY))
            {
                d.Remove(TRANSACTION_KEY);
            }
        }
    }
}
