using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;

[CreateAssetMenu(fileName = "NewBoardSkin",
menuName = "ScriptableObj/BoardSkin", order = 1)]
public class SkinScriptableObject : ScriptableObject {
    public string name = "BoardSkinName";
    public Sprite zeroSprite;
    public Color zeroTintColor = Color.white;
    public Sprite oneSprite;
    public Color oneTintColor = Color.white;
    public bool supportNumber = false;

    public string saveKey = "save";
    private static string skinPurchasedPostFixed = "_SKIN_PURCHASED";
    private string skinPurchasedKey 
    {
        get
        {
            return saveKey + skinPurchasedPostFixed;
        }

    }
    public bool purchased
    {
        set
        {
            PlayerPrefs.SetInt(skinPurchasedKey, value?1:0);
        }
        get
        {
            return PlayerPrefs.GetInt(skinPurchasedKey) == 1;
        }
    }
    public bool isFree = false;

    public int price
    {
        get
        {
            return CloudServiceManager.Instance.appConfig.GetInt(string.Format("{0}SkinPrice", saveKey))??299;
        }
    }
}
