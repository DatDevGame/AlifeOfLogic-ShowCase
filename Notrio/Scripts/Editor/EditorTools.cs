using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Takuzu
{
    public class EditorTools
    {
        [MenuItem("Tools/Reset PlayerPrefs", false)]
        public static void ResetPlayerPref()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("*** PlayerPrefs was reset! ***");
        }

        [MenuItem("Tools/Reset PlayerDb", false)]
        public static void ResetPlayerDb()
        {
            PlayerDb.ResetInPlayerPrefNoPlaymode();
            Debug.Log("*** PlayerDb was reset! ***");
        }

        [MenuItem("Tools/Reset GamesparkId", false)]
        public static void ResetGamesparkId()
        {
            PlayerPrefs.SetString("gamesparks.userid", "123456");
            Debug.Log("*** GamesparkId was reset! ***");
        }

        [MenuItem("Tools/Open PlayerDb directory")]
        public static void OpenPlayerDbDirectory()
        {
            System.Diagnostics.Process.Start(Application.persistentDataPath);
        }

        [MenuItem("Tools/Count GameObject")]
        public static void CountGameObject()
        {
            Scene s = SceneManager.GetActiveScene();
            GameObject[] root = s.GetRootGameObjects();
            int sum = 0;
            for (int i = 0; i < root.Length; ++i)
            {
                sum += 1 + root[i].transform.childCount;
            }

            Debug.Log("Total game object: " + sum);
        }

        [MenuItem("Tools/Change UserId Gamespark", false)]
        public static void ChangeUserIdGS()
        {
            PlayerPrefs.SetString("gamesparks.userid", "gamespark123");
            Debug.Log("*** UserIdGS was changed ***");
        }
    }
}