using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    public class Background
    {
        public const string resourcesPath = "bg/";

        private static List<Sprite> loadedBg;
        private static List<Sprite> LoadedBg
        {
            get
            {
                if (loadedBg == null)
                    loadedBg = new List<Sprite>();
                return loadedBg;
            }
        }

        public static Sprite Get(string spriteName)
        {
            Sprite bg = LoadedBg.Find(s => s.name.Equals(spriteName));
            if (bg == null)
            {
                bg = Resources.Load<Sprite>(resourcesPath + spriteName);
                if (bg != null)
                    LoadedBg.Add(bg);
                else{
                    if(OndemandResourceLoader.IsBundleLoaded("textures")){
                        AssetBundle ab = OndemandResourceLoader.GetAssetBundle("textures");
                        Debug.Log(ab);
                        bg = ab.LoadAsset<Sprite>(spriteName);
                        LoadedBg.Add(bg);
                    }else{
                        OndemandResourceLoader.GetAssetBundleWithCallback("textures", (ab) => {
                            LoadedBg.Add(ab.LoadAsset<Sprite>(spriteName));
                        });
                    }
                }
            }
            return bg;
        }

        public static void Unload(string spriteName)
        {
            List<Sprite> bg = LoadedBg.FindAll(s => s.name.Equals(spriteName));
            for (int i = 0; i < bg.Count; ++i)
            {
                try
                {
                    Resources.UnloadAsset(bg[i].texture);
                    Resources.UnloadUnusedAssets();
                    bg[i] = null;
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }
            LoadedBg.RemoveAll(s => s == null);
        }
    }
}