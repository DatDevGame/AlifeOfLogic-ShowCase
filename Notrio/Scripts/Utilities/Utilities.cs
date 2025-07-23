using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using Takuzu.Generator;

namespace Takuzu
{
    public static class Utilities
    {
        public static IEnumerator CRWaitForRealSeconds(float time)
        {
            float start = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup < start + time)
            {
                yield return null;
            }
        }

        // Opens a specific scene
        public static void GoToScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void RateApp()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    Application.OpenURL(AppInfo.Instance.APPSTORE_LINK);
                    break;

                case RuntimePlatform.Android:
                    Application.OpenURL(AppInfo.Instance.PLAYSTORE_LINK);
                    break;
            }
        }

        public static void ShowMoreGames()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    Application.OpenURL(AppInfo.Instance.APPSTORE_HOMEPAGE);
                    break;

                case RuntimePlatform.Android:
                    Application.OpenURL(AppInfo.Instance.PLAYSTORE_HOMEPAGE);
                    break;
            }
        }

        public static void OpenFacebookPage()
        {
            Application.OpenURL(AppInfo.Instance.FACEBOOK_LINK);
        }

        public static void OpenTwitterPage()
        {
            Application.OpenURL(AppInfo.Instance.TWITTER_LINK);
        }

        public static void ContactUs()
        {
            string email = AppInfo.Instance.SUPPORT_EMAIL;
            string subject = EscapeURL(AppInfo.Instance.APP_NAME + " [" + Application.version + "] Support");
            string body = EscapeURL("");
            Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
        }

        public static string EscapeURL(string url)
        {
            return WWW.EscapeURL(url).Replace("+", "%20");
        }

        public static int[] GenerateShuffleIndices(int length)
        {
            int[] array = new int[length];

            // Populate array
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }

            // Shuffle
            for (int j = 0; j < array.Length; j++)
            {
                int tmp = array[j];
                int randomPos = UnityEngine.Random.Range(j, array.Length);
                array[j] = array[randomPos];
                array[randomPos] = tmp;
            }

            return array;
        }

        /// <summary>
        /// Stores a DateTime as string to PlayerPrefs.
        /// </summary>
        /// <param name="time">Time.</param>
        /// <param name="ppkey">Ppkey.</param>
        public static void StoreTime(string ppkey, DateTime time)
        {
            PlayerPrefs.SetString(ppkey, time.ToBinary().ToString());
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets the stored string in the PlayerPrefs and converts it to a DateTime.
        /// If no time stored previously, defaultTime is returned.
        /// </summary>
        /// <returns>The time.</returns>
        /// <param name="ppkey">Ppkey.</param>
        public static DateTime GetTime(string ppkey, DateTime defaultTime)
        {
            string storedTime = PlayerPrefs.GetString(ppkey, string.Empty);

            if (!string.IsNullOrEmpty(storedTime))
                return DateTime.FromBinary(Convert.ToInt64(storedTime));
            else
                return defaultTime;
        }

        public static Vector3 ToWorldPosition(this Vector2 mousePosition)
        {
            return mousePosition.ToWorldPosition(Camera.main);
        }

        public static Vector3 ToWorldPosition(this Vector2 mousePosition, Camera cam)
        {
            return cam.ScreenToWorldPoint(mousePosition);
        }

        public static Index2D ToIndex2D(this Vector2 mousePosition)
        {
            return mousePosition.ToIndex2D(Camera.main);
        }

        public static Index2D ToIndex2D(this Vector2 mousePosition, Camera cam)
        {
            Vector2 worldPoint = mousePosition.ToWorldPosition();
            return new Index2D(Mathf.RoundToInt(worldPoint.y), Mathf.RoundToInt(worldPoint.x));
        }

        public static int[] GetIndicesArray(int length)
        {
            int[] indices = new int[length];
            for (int i = 0; i < length; ++i)
            {
                indices[i] = i;
            }
            return indices;
        }

        public static Index2D[] GetIndicesArray2D(int row, int column)
        {
            Index2D[] indices = new Index2D[row * column];
            for (int i = 0; i < row; ++i)
            {
                for (int j = 0; j < column; ++j)
                {
                    indices[i * row + j] = new Index2D(i, j);
                }
            }
            return indices;
        }

        public static Index2D[] GetShuffleIndicesArray2D(int row, int column)
        {
            Index2D[] indices = GetIndicesArray2D(row, column);
            for (int i = 0; i < indices.Length - 1; ++i)
            {
                int j = UnityEngine.Random.Range(0, indices.Length);
                Index2D tmp = indices[i];
                indices[i] = indices[j];
                indices[j] = tmp;
            }
            return indices;
        }

        public static int[] GetShuffleIndicesArray(int length)
        {
            int[] indices = GetIndicesArray(length);
            for (int i = 0; i < length - 1; ++i)
            {
                int j = UnityEngine.Random.Range(0, length);
                int tmp = indices[i];
                indices[i] = indices[j];
                indices[j] = tmp;
            }
            return indices;
        }

        public static void Fill<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = value;
            }
        }

        public static void Fill<T>(this T[,] array2D, T value)
        {
            for (int i = 0; i < array2D.GetLength(0); ++i)
            {
                for (int j = 0; j < array2D.GetLength(1); ++j)
                {
                    array2D[i, j] = value;
                }
            }
        }

        public static void Fill<T>(this T[][] jaggedArray, T value)
        {
            for (int i = 0; i < jaggedArray.Length; ++i)
            {
                for (int j = 0; j < jaggedArray[i].Length; ++j)
                {
                    jaggedArray[i][j] = value;
                }
            }
        }

        public static List<T> ToList<T>(this T[] array)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < array.Length; ++i)
            {
                list.Add(array[i]);
            }
            return list;
        }

        public static Vector2 ToScreenPosition(this Vector3 worldPoint)
        {
            return worldPoint.ToScreenPosition(Camera.main);
        }

        public static Vector2 ToScreenPosition(this Vector3 worldPoint, Camera cam)
        {
            return cam.WorldToScreenPoint(worldPoint);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        public static void ClearAllChildren(this Transform t)
        {
            int childCount = t.childCount;
            for (int i = childCount - 1; i >= 0; --i)
            {
                UnityEngine.GameObject.DestroyImmediate(t.GetChild(i).gameObject);
            }
        }

        public static List<Transform> GetAllChildren(this Transform t)
        {
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < t.childCount; ++i)
            {
                children.Add(t.GetChild(i));
            }
            return children;
        }

        #region Country code <==> coutry name
        private static CountryCodeMapper countryCodeMapper;

        private static void InitCountryCodeToName()
        {
            countryCodeMapper = Resources.Load<CountryCodeMapper>("CountryCodeMapper");
        }

        public static string CountryNameFromCode(string code)
        {
            if (countryCodeMapper == null)
            {
                InitCountryCodeToName();
            }
            return countryCodeMapper.ToEnglishName(code);
        }

        public static CountryCodeMapper GetCountryCodeMapper()
        {
            if (countryCodeMapper == null)
            {
                InitCountryCodeToName();
            }
            return countryCodeMapper;
        }
        #endregion

        #region Difficulty internal name <==> display name
        private static DifficultyNameMapper difficultyNameMapper;
        private static DifficultyNameMapper DifficultyMapper
        {
            get
            {
                if (difficultyNameMapper==null)
                {
                    difficultyNameMapper = Resources.Load<DifficultyNameMapper>("DifficultyNameMapper");
                }
                return difficultyNameMapper;
            }
        }

        public static string GetDifficultyDisplayName(Level l)
        {
            return DifficultyMapper.ToDisplayName(l);
        }

        #endregion
        public static void BringToFront(this Transform t)
        {
            t.SetAsLastSibling();
        }

        public static void SendToBack(this Transform t)
        {
            t.SetAsFirstSibling();
        }

        public static int ToMondayBased(this DayOfWeek d)
        {
            if (d == DayOfWeek.Sunday)
                return 6;
            else
                return (int)d - 1;
        }

        public static void RemovePeekAll<T>(this Stack<T> s, T value)
        {
            while (s.Count > 0 && s.Peek().Equals(value))
            {
                s.Pop();
            }
        }

        public static Texture2D ToTexture2D(this Sprite s)
        {
            Texture2D a = new Texture2D((int)s.rect.width, (int)s.rect.height);
            Color[] c = s.texture.GetPixels(
                            (int)s.textureRect.x,
                            (int)s.textureRect.y,
                            (int)s.textureRect.width,
                            (int)s.textureRect.height);
            a.SetPixels(c);
            a.Apply();
            return a;
        }

        public static string GetLocalizePackNameByLevel(Level level)
        {
            for(int i = 0; i < PuzzleManager.Instance.levelInfors.Count;i++)
            {
                if(PuzzleManager.Instance.levelInfors[i].level.Equals(level))
                {
                    switch (i)
                    {
                        case 1:
                            return I2.Loc.ScriptLocalization.PACK_NAME_1.ToUpper();
                        case 2:
                            return I2.Loc.ScriptLocalization.PACK_NAME_2.ToUpper();
                        case 3:
                            return I2.Loc.ScriptLocalization.PACK_NAME_3.ToUpper();
                        case 4:
                            return I2.Loc.ScriptLocalization.PACK_NAME_4.ToUpper();
                        case 5:
                            return I2.Loc.ScriptLocalization.PACK_NAME_5.ToUpper();
                    }
                }
            }
            Debug.LogWarning("missing level = " + level);
            return "????";
        }

        public static string GetLocalizePackNameByIndex(int index)
        {
            switch (index)
            {
                case 1:
                    return I2.Loc.ScriptLocalization.PACK_NAME_1.ToUpper();
                case 2:
                    return I2.Loc.ScriptLocalization.PACK_NAME_2.ToUpper();
                case 3:
                    return I2.Loc.ScriptLocalization.PACK_NAME_3.ToUpper();
                case 4:
                    return I2.Loc.ScriptLocalization.PACK_NAME_4.ToUpper();
                case 5:
                    return I2.Loc.ScriptLocalization.PACK_NAME_5.ToUpper();
                default:
                    Debug.LogWarning("missing index = " + index);
                    return "???";
            }
        }

        public static string GetLocalizePackNameByName(string name)
        {
            for (int i = 0; i < PuzzleManager.Instance.levelInfors.Count; i++)
            {
                if (PuzzleManager.Instance.levelInfors[i].levelName.ToUpper().Equals(name.ToUpper()))
                {
                    switch (i)
                    {
                        case 1:
                            return I2.Loc.ScriptLocalization.PACK_NAME_1;
                        case 2:
                            return I2.Loc.ScriptLocalization.PACK_NAME_2;
                        case 3:
                            return I2.Loc.ScriptLocalization.PACK_NAME_3;
                        case 4:
                            return I2.Loc.ScriptLocalization.PACK_NAME_4;
                        case 5:
                            return I2.Loc.ScriptLocalization.PACK_NAME_5;
                    }
                }
            }
            Debug.LogWarning("missing name = " + name);
            return "????";
        }
    }
}