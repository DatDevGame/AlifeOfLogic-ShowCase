using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Takuzu.Generator
{
    public class TakuzuEncryptorEditor : EditorWindow
    {
        private string srcDatabase;
        private string desDatabase;
        private CryptoKey key;

        private const string SRC_KEY = "ENCRYPTOR_SRC_DB";
        private const string DES_KEY = "ENCRYPTOR_DES_DB";
        private const string CRYPTO_KEY_KEY = "ENCRYPTOR_CRYPTO_KEY";

        [MenuItem("Tools/Takuzu Encryptor", priority = 3)]
        public static void ShowWindow()
        {
            TakuzuEncryptorEditor window = GetWindow<TakuzuEncryptorEditor>("Takuzu encryptor");
            window.Show();
        }

        private void OnEnable()
        {
            LoadParams();
        }

        private void OnDestroy()
        {
            SaveParams();
        }

        private void LoadParams()
        {
            srcDatabase = EditorPrefs.GetString(SRC_KEY);
            desDatabase = EditorPrefs.GetString(DES_KEY);
            key = AssetDatabase.LoadAssetAtPath<CryptoKey>(EditorPrefs.GetString(CRYPTO_KEY_KEY));
        }

        private void SaveParams()
        {
            EditorPrefs.SetString(SRC_KEY, srcDatabase);
            EditorPrefs.SetString(DES_KEY, desDatabase);
            EditorPrefs.SetString(CRYPTO_KEY_KEY, AssetDatabase.GetAssetPath(key));
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Source database:             " + srcDatabase);
            if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                EditorCommon.BrowseDatabase(ref srcDatabase);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Destination database:       " + desDatabase);
            if (GUILayout.Button("New", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                EditorCommon.CreateNewDatabase(ref desDatabase);
            }
            EditorGUILayout.EndHorizontal();

            key = (CryptoKey)EditorGUILayout.ObjectField("Crypto key", key, typeof(CryptoKey), false);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = key != null;
            if (GUILayout.Button("Encrypt", GUILayout.Width(150)))
            {
                Encrypt();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void Encrypt()
        {
            try
            {
                Encryptor.CloneDatabase(srcDatabase, desDatabase);
            }
            catch (System.IO.IOException)
            {
                EditorUtility.DisplayDialog("Error", "An IO error occurs, please close all connection to the source and destination database, especially in database browser software.", "OK");
            }

            List<int> idContainer = new List<int>();
            List<string> puzzleContainer = new List<string>();
            List<string> solutionContainer = new List<string>();
            try
            {
                Encryptor.GetOriginPuzzle(srcDatabase, idContainer, puzzleContainer, solutionContainer);
                Crypto.SetDefaultKey(key);
                for (int i = 0; i < idContainer.Count; ++i)
                {
                    puzzleContainer[i] = Crypto.Encrypt(puzzleContainer[i]);
                    solutionContainer[i] = Crypto.Encrypt(solutionContainer[i]);
                }
                Encryptor.SaveEncryptedPuzzle(desDatabase, idContainer, puzzleContainer, solutionContainer);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}