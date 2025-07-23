using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;

public class RollingItem: MonoBehaviour{
    [Serializable]
    public class RollingItemData
    {
        public bool rewardCoins;
        public int amount;
        public Sprite bgSprite;
        public float f;
    }

    public RollingItemData data = new RollingItemData { rewardCoins = true};
    [Header("UI Preferences")]
    public Image bg;
    public Text amount;

    public Color coinTextColor;
    public Color energyTextColor;
    
    internal void UpdateData(RollingItemData data)
    {
        this.data = data;
        UpdateUI();
    }

    private void UpdateUI()
    {
        //Update data
        if (data.rewardCoins)
        {
            amount.text = String.Format("+{0}",data.amount);
            amount.color = coinTextColor;
            bg.sprite = data.bgSprite;
        }
        else
        {
            amount.text = String.Format("+{0}", data.amount);
            amount.color = energyTextColor;
            bg.sprite = data.bgSprite;
        }
    }
}
