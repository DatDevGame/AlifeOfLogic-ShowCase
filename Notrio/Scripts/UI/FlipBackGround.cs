using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    public class FlipBackGround
    {
        public const string resourcesPath = "bgflip/";
        public static string currentFlipBgName = "";

        private static Sprite loadedMainBg;
        private static Sprite LoadedMainBg
        {
            get
            {
                return loadedMainBg;
            }

            set
            {
                loadedMainBg = value;
            }
        }

        private static List<Sprite> loadedBg;
        private static List<Sprite> LoadedBg
        {
            get
            {
                if (loadedBg == null)
                    loadedBg = new List<Sprite>();
                return loadedBg;
            }

            set
            {
                loadedBg = value;
            }
        }

        public static Sprite GetMainSprite(string spriteName)
        {
            if (currentFlipBgName.Equals(spriteName) && LoadedMainBg != null)
            {
                currentFlipBgName = spriteName;
                return LoadedMainBg;
            }
            else
            {
                LoadedMainBg = Resources.Load<Sprite>(resourcesPath + spriteName);
                if(LoadedMainBg == null)
                {
                    if (OndemandResourceLoader.IsBundleLoaded("textures"))
                    {
                        LoadedMainBg = OndemandResourceLoader.GetAssetBundle("textures").LoadAsset<Sprite>(spriteName);
                    }
                    else
                    {
                        OndemandResourceLoader.GetAssetBundleWithCallback("textures", (ab) =>
                        {
                            LoadedMainBg = ab.LoadAsset<Sprite>(spriteName);
                        });
                    }
                }
            }
            currentFlipBgName = spriteName;
            return LoadedMainBg;
        }

        public static List<Sprite> GetSubSprites(string spriteName)
        {
            if (currentFlipBgName.Equals(spriteName) && LoadedBg.Count > 0)
            {
                currentFlipBgName = spriteName;
                return LoadedBg;
            }
            else
            {
                LoadedBg = new List<Sprite>(Resources.LoadAll<Sprite>(resourcesPath + spriteName));
                if (LoadedBg == null || LoadedBg.Count <= 0)
                {
                    if (OndemandResourceLoader.IsBundleLoaded("textures"))
                    {
                        LoadedBg = new List<Sprite>(OndemandResourceLoader.GetAssetBundle("textures").LoadAssetWithSubAssets<Sprite>(spriteName));
                    }
                    else
                    {
                        OndemandResourceLoader.GetAssetBundleWithCallback("textures", (ab) =>
                        {
                            LoadedBg = new List<Sprite>(ab.LoadAssetWithSubAssets<Sprite>(spriteName));
                        });
                    }
                }
            }
            currentFlipBgName = spriteName;
            return LoadedBg;
        }

        public static void UnloadSubSprites()
        {
            List<Sprite> bg = LoadedBg;
            for (int i = 0; i < bg.Count; ++i)
            {
                try
                {
                    Resources.UnloadAsset(LoadedBg[i].texture);
                    bg[i] = null;
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }
            Resources.UnloadUnusedAssets();
            LoadedBg.RemoveAll(s => s == null);
        }

        public static void UnloadMainSprite()
        {
            if (loadedMainBg != null)
            {
                Resources.UnloadUnusedAssets();
                Resources.UnloadAsset(loadedMainBg.texture);
            }
        }
    }
}
