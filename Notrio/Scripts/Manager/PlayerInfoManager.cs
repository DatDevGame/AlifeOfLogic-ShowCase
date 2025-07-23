using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Takuzu.Generator;

namespace Takuzu
{
    public class PlayerInfoManager : MonoBehaviour
    {
        public static PlayerInfoManager Instance { get; set; }
        public const string WIN_NUMBER_KEY = "PLAYED-WIN_NUMBER_KEY";
        public const string LOSE_NUMBER_KEY = "PLAYED-LOSE_NUMBER_KEY";
        public const string EXP_KEY = "EXP";
        public static Action<PlayerInfo, bool> onInfoLoaded = delegate { };
        public static Action<PlayerInfo, PlayerInfo> onInfoUpdated = delegate { };
        public static Action<PlayerInfo, PlayerInfo> onLevelUp = delegate { };


        public CryptoKey cryptoKey;
        public ExpProfile expProfile;

        //[HideInInspector]
        public PlayerInfo info;
        public int winNumber
        {
            get
            {
                return PlayerDb.GetInt(WIN_NUMBER_KEY, 0);
            }
            private set
            {
                PlayerDb.SetInt(WIN_NUMBER_KEY, value);
                PlayerDb.Save();
            }
        }

        public int loseNumber
        {
            get
            {
                return PlayerDb.GetInt(LOSE_NUMBER_KEY, 0);
            }
            private set
            {
                PlayerDb.SetInt(LOSE_NUMBER_KEY, value);
                PlayerDb.Save();
            }
        }

        private string encryptedKey;

        private void OnEnable()
        {
            Judger.onExpGained += OnExpGained;
            PlayerDb.Resetted += OnPlayerDbReset;
            PlayerDb.RequestDecrypt += OnPlayerDbRequestDecrypt;
            PlayerDb.RequestEncrypt += OnPlayerDbRequestEncrypt;
            CloudServiceManager.onPlayerDbSyncSucceed += OnSyncSucceed;
        }

        private void OnDisable()
        {
            Judger.onExpGained -= OnExpGained;
            PlayerDb.Resetted -= OnPlayerDbReset;
            PlayerDb.RequestDecrypt -= OnPlayerDbRequestDecrypt;
            PlayerDb.RequestEncrypt -= OnPlayerDbRequestEncrypt;
            CloudServiceManager.onPlayerDbSyncSucceed -= OnSyncSucceed;
        }

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ExpProfile.active = expProfile;
                encryptedKey = Crypto.Encrypt(EXP_KEY, cryptoKey);
            }
        }

        private void Start()
        {
            LoadInfo();
        }

        /// <summary>
        /// Load and decrypt player info
        /// </summary>
        public void LoadInfo(bool start = true)
        {
            int exp = 0;
            try
            {
                string expStringEncrypted = PlayerDb.GetString(encryptedKey, Crypto.Encrypt("0", cryptoKey));
                string expString = Crypto.Decrypt(expStringEncrypted, cryptoKey);
                int.TryParse(expString, out exp);
            }
            catch
            {
                exp = 0;
            }
            //int exp = PlayerDb.GetInt(EXP_KEY, 0);
            winNumber = PlayerDb.GetInt(WIN_NUMBER_KEY, 0);
            loseNumber = PlayerDb.GetInt(LOSE_NUMBER_KEY, 0);
            info = ExpProfile.active.FromTotalExp(exp);
            onInfoLoaded(info, start);
        }

        public void UpdateWinLoseNumber(bool isWin)
        {
            if (isWin)
                winNumber++;
            else
                loseNumber++;
            PlayerDb.Save();
        }
        /// <summary>
        /// Encrypt and save player info to disk
        /// </summary>
        public void SaveInfo()
        {
            int exp = ExpProfile.active.ToTotalExp(info);
            string encryptedExp = Crypto.Encrypt(exp.ToString(), cryptoKey);
            PlayerDb.SetString(encryptedKey, encryptedExp);
            //PlayerDb.SetInt(EXP_KEY, exp);
        }

        private void OnExpGained(int exp)
        {
            PlayerInfo oldInfo = info;
            info.exp += exp;
            if (info.level < expProfile.exp.Count - 1)
            {
                if (info.exp >= expProfile.exp[info.level])
                {
                    info.exp -= expProfile.exp[info.level];
                    info.level += 1;
                    onLevelUp(info, oldInfo);
                }
            }
            onInfoUpdated(info, oldInfo);
            SaveInfo();
        }

        private void OnPlayerDbReset()
        {
            info = expProfile.FromTotalExp(0);
            onInfoLoaded(info, true);
        }

        private void OnSyncSucceed()
        {
            LoadInfo(false);
        }

        private void OnPlayerDbRequestDecrypt(Dictionary<string,object> d)
        {
            if (d.ContainsKey(encryptedKey))
            {
                d[EXP_KEY] = expProfile.ToTotalExp(info);
                d.Remove(encryptedKey);
            }
        }

        private void OnPlayerDbRequestEncrypt(Dictionary<string, object> d)
        {
            if (d.ContainsKey(EXP_KEY))
            {
                d[encryptedKey] = Crypto.Encrypt(d[EXP_KEY].ToString(), cryptoKey);
                d.Remove(EXP_KEY);
            }
        }
    }
}