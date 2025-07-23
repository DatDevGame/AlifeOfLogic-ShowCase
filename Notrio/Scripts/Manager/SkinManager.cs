using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;
using System;

public class SkinManager : MonoBehaviour {

    public static SkinManager Instance;
    public static System.Action ActivatedSkinChanged = delegate { };
    public static System.Action<int> TemporarySkinChanged = delegate { };
    public static System.Action<SkinScriptableObject> SkinPurchased = delegate { };

    public List<SkinScriptableObject> availableSkin =
        new List<SkinScriptableObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            DestroyImmediate(gameObject);
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private string activeSkinSaveKey = "ACTIVE_SKIN_INDEX_SAVE_KEY";
    public int currentActivatedSkinIndex
    {
        set
        {
            PlayerPrefs.SetInt(activeSkinSaveKey, value);
        }
        get
        {
            return PlayerPrefs.GetInt(activeSkinSaveKey, 0);
        }
    }

    public static void ResetSkinToCurrentActive()
    {
        if (Instance == null)
            return;
        ActivatedSkinChanged();
    }

    public static void SetSkinTemporary(int index)
    {
        if (Instance == null)
            return;
        TemporarySkinChanged(index);
    }

    public static void SetActivatedSkinIndex(int index)
    {
        if (Instance == null)
            return;
        if(index >= Instance.availableSkin.Count)
            return;
        
        if( Instance.availableSkin[index].isFree ||
            Instance.availableSkin[index].purchased)
        {
            bool skinChanged = index != Instance.currentActivatedSkinIndex;
            Instance.currentActivatedSkinIndex = index;
            if (skinChanged)
                ActivatedSkinChanged();
        }
    }

    public static void PurchasingSkin(int index)
    {
        if (Instance == null)
            return;
        if (index >= Instance.availableSkin.Count)
            return;
        
        if(CoinManager.Instance.Coins >= Instance.availableSkin[index].price)
        {
            if (UIReferences.Instance != null)
            {
                UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.CONFIRMATION.ToUpper(), I2.Loc.ScriptLocalization.Purchase_Msg,
                    delegate 
                    {
                        SkinPurchased(Instance.availableSkin[index]);
                        CoinManager.Instance.RemoveCoins(Instance.availableSkin[index].price);
                        Instance.availableSkin[index].purchased = true;
                        SetActivatedSkinIndex(index);
                        if (SoundManager.Instance != null)
                            SoundManager.Instance.PlaySound(SoundManager.Instance.rewarded);
                    }, null);
            }
        }
        else
        {
            if (UIReferences.Instance != null)
            {
                UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION, string.Format(I2.Loc.ScriptLocalization.Not_Enough_Coin_For_Tile, Instance.availableSkin[index].price),
                    delegate
                    {
                        //UIReferences.Instance.overlayCoinShopUI.Show();
                    }, null);
            }
        }
    }

    public static int GetSkinIndexFromName(string name)
    {
        int index = 0;
        index = Instance.availableSkin.FindIndex(item => item.name == name);
        if (index < 0)
            index = 0;
        return index;
    }

    public static SkinScriptableObject GetActivatedSkin()
    {
        //Assume that skin list always have more than 1 skin
        //may be we need to check and return a default skin
        if(Instance.currentActivatedSkinIndex >= Instance.availableSkin.Count)
            return Instance.availableSkin[0];
        return Instance.availableSkin[Instance.currentActivatedSkinIndex];
    }

    internal static SkinScriptableObject GetSkinFromIndex(int index)
    {
        if(index >= Instance.availableSkin.Count)
            return Instance.availableSkin[0];
        return Instance.availableSkin[index];
    }
}
