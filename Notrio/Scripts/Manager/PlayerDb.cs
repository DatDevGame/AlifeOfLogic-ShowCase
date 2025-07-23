using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using System;
using System.Linq;
using GameSparks.Core;

namespace Takuzu
{
    [AddComponentMenu("")]
    public class PlayerDb : MonoBehaviour
    {
        public static PlayerDb Instance { get; private set; }

        public static event Action Resetted = delegate { };

        /// <summary>
        /// Event fired before send data to server, to decrypt data to send to server
        /// </summary>
        public static event Action<Dictionary<string, object>> RequestDecrypt = delegate { };

        /// <summary>
        /// Event fired after receive sync result from server, to encrypt data and save to storage
        /// </summary>
        public static event Action<Dictionary<string, object>> RequestEncrypt = delegate { }; 

        private static Dictionary<string, string> data;
        private static Dictionary<string, string> Data
        {
            get
            {
                if (data == null)
                    Init();
                return data;
            }
            set
            {
                data = value;
            }
        }

        public const string PLAYER_PREFS_KEY = "PLAYER_DB";
        public const string PLAYER_ID_KEY = "PLAYER_ID";
        public const string UP_TO_DATE_KEY = "UP_TO_DATE";
        public const string RECORDS_KEY = "RECORDS";
        public const string FINISH_TUTORIAL_KEY = "FINISH_TUTORIAL";
        public const string FINISH_TUTORIAL_REWARD_KEY = "FINISH_TUTORIAL_REWARD";
        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
                Save();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
                Save();
        }

        private void OnApplicationQuit()
        {
            //* Save and write all to disk is a long operation
            //* only set string and let Unity handle save data to disk
            Save(false);
        }

        public static void Init()
        {
            if (Instance == null)
            {
                GameObject g = new GameObject("PlayerDbInstance");
                Instance = g.AddComponent<PlayerDb>();
                DontDestroyOnLoad(Instance.gameObject);
            }

            RawData raw = new RawData();
            try
            {
                string rawDataJson = PlayerPrefs.GetString(PLAYER_PREFS_KEY, RawData.DEFAULT_JSON);
                raw = RawData.FromString(rawDataJson);
            }
            catch
            {
                raw = RawData.DefaultData;
            }
            finally
            {
                Data = RawData.ToDictionary(raw);
            }
        }

        public static void Save(bool saveImmediatelyToDisk = true)
        {
            //Debug.Log("Saving PlayerDB");
            string json = JsonUtility.ToJson(RawData.FromDictionary(Data));
            PlayerPrefs.SetString(PLAYER_PREFS_KEY, json);
            if(saveImmediatelyToDisk)
                PlayerPrefs.Save();
        }

        public static int GetInt(string key, int defaultValue)
        {
            string s = Get(key);
            int value;
            if (int.TryParse(s, out value))
            {
                return value;
            }
            return defaultValue;
        }

        public static void SetInt(string key, int value)
        {
            Set(key, value.ToString());
        }

        public static string GetString(string key, string defaultValue)
        {
            string value = Get(key);
            return value ?? defaultValue;
        }

        public static void SetString(string key, string value)
        {
            Set(key, value);
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            string s = Get(key);
            bool value;
            if (bool.TryParse(s, out value))
                return value;
            else
                return defaultValue;
        }

        public static void SetBool(string key, bool value)
        {
            Set(key, value.ToString());
        }

        public static float GetFloat(string key, float defaultValue)
        {
            string s = Get(key);
            float value;
            if (float.TryParse(s, out value))
            {
                return value;
            }
            return defaultValue;
        }

        public static void SetFloat(string key, float value)
        {
            Set(key, value.ToString());
        }

        private static string Get(string key)
        {
            string value = null;
            Data.TryGetValue(key, out value);
            return value;
        }

        private static void Set(string key, string value)
        {
            string oldValue = null;
            Data.TryGetValue(key, out oldValue);
            Data[key] = value;

            if (value != null && !value.Equals(oldValue))
                SetUpToDate(false);
        }

        public static bool HasKey(string key)
        {
            return Data.ContainsKey(key);
        }

        public static void DeleteKey(string key)
        {
            Data.Remove(key);
            SetUpToDate(false);
        }

        public static void DeleteAllKey()
        {
            Data.Clear();
            SetUpToDate(false);
        }

        public static void GetAllRecords(Dictionary<string, string> container)
        {
            container = new Dictionary<string, string>(Data);
        }

        public static int CountKeyStartWith(string pattern)
        {
            //return Data.Where(p => p.Key.StartsWith(pattern)).Count();
            string[] keys = Data.Keys.ToArray();
            int count = 0;
            for (int i = 0; i < keys.Length; ++i)
            {
                if (keys[i].StartsWith(pattern))
                    count += 1;
            }
            return count;
        }

        public static void SetUpToDate(bool isUpToDate)
        {
            Data[UP_TO_DATE_KEY] = isUpToDate.ToString();
        }

        public static bool IsUpToDate()
        {
            return GetBool(UP_TO_DATE_KEY, false);
        }

        public static void SetPlayerIdIfNotExists(string playerId)
        {
            if (string.IsNullOrEmpty(GetString(PLAYER_ID_KEY, string.Empty)))
                SetString(PLAYER_ID_KEY, playerId);
        }

        public static void Reset()
        {
            Data = null;
            PlayerPrefs.DeleteKey(PLAYER_PREFS_KEY);
            Resetted();
        }

#if UNITY_EDITOR
        public static void ResetInPlayerPrefNoPlaymode()
        {
            PlayerPrefs.DeleteKey(PLAYER_PREFS_KEY);
        }
#endif
        
        public static GSData ToGSData()
        {
            Dictionary<string, object> baseData = Data.ToDictionary(p => p.Key.Clone().ToString(), p => p.Value.Clone());
            RequestDecrypt(baseData);

            string coin = "0";
            if (baseData.ContainsKey(CoinManager.COINS_KEY))
                coin = baseData[CoinManager.COINS_KEY].ToString();
            string trans = "0";
            if (baseData.ContainsKey(CoinManager.TRANSACTION_KEY))
                trans = baseData[CoinManager.TRANSACTION_KEY].ToString();

            string s = string.Format("Send: COIN {0}, TRANSACTION {1}", coin, trans);
            Debug.Log(s);

            return new GSData(baseData);
        }

        public static void FromGSData(GSData d)
        {
            Dictionary<string, object> baseData = new Dictionary<string, object>(d.BaseData);

            string coin = "0";
            if (baseData.ContainsKey(CoinManager.COINS_KEY))
                coin = baseData[CoinManager.COINS_KEY].ToString();
            string trans = "0";
            if (baseData.ContainsKey(CoinManager.TRANSACTION_KEY))
                trans = baseData[CoinManager.TRANSACTION_KEY].ToString();

            string s = string.Format("Receive: COIN {0}, TRANSACTION {1}", coin, trans);
            Debug.Log(s);

            RequestEncrypt(baseData);
            Data = baseData.ToDictionary(p => p.Key, p => p.Value != null ? p.Value.ToString() : string.Empty);
        }
    }
}