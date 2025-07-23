using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Takuzu.Generator;

namespace Takuzu
{
    public class PlayerDbViewer : EditorWindow
    {
        Dictionary<string, string> d;
        Vector2 scrollPos;
        CryptoKey cryptoKey;

        [MenuItem("Tools/PlayerDb Viewer")]
        public static void ShowWindow()
        {
            PlayerDbViewer w = GetWindow<PlayerDbViewer>();
            w.Show();
        }

        private void GetData()
        {
            string json = PlayerPrefs.GetString(PlayerDb.PLAYER_PREFS_KEY, RawData.DEFAULT_JSON);
            RawData raw = RawData.FromString(json);
            d = RawData.ToDictionary(raw);
        }

        private void OnEnable()
        {
            GetData();
        }

        private void Awake()
        {
            string path = EditorPrefs.GetString("Takuzu-PlayerDbViewer-key", "");
            cryptoKey = AssetDatabase.LoadAssetAtPath<CryptoKey>(path);
        }

        private void OnDestroy()
        {
            if (cryptoKey != null)
                EditorPrefs.SetString("Takuzu-PlayerDbViewer-key", AssetDatabase.GetAssetPath(cryptoKey));
        }

        private void OnGUI()
        {
            cryptoKey = EditorGUILayout.ObjectField("Key", cryptoKey, typeof(CryptoKey), false) as CryptoKey;
            if (GUILayout.Button("Refresh"))
            {
                GetData();
            }

            if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button("GetJson"))
                {
                    GameSparks.Core.GSData d = PlayerDb.ToGSData();
                    Debug.Log(d.JSON);
                }
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (var k in d.Keys)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(k, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(d[k], GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Decrypt", GUILayout.Width(100)))
                {
                    try
                    {
                        string key = Crypto.Decrypt(k, cryptoKey);
                        string value = Crypto.Decrypt(d[k], cryptoKey);
                        EditorUtility.DisplayDialog("Info", key + "\n" + value, "OK");
                    }
                    catch { }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}