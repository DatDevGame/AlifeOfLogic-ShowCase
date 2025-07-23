using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;
using System;
using UnityEngine.UI;

public class SkinShopOverlayUI : OverlayPanel
{

    public OverlayGroupController controller;
    public ListView listView;
    public GameObject boardTemplate;
    public Button closeButton;
    public override void Hide()
    {
        controller.HideIfNot();
        IsShowing = false;
        onPanelStateChanged(this, false);
    }

    public override void Show()
    {
        controller.ShowIfNot();
        IsShowing = true;
        transform.BringToFront();
        onPanelStateChanged(this, true);
        UpdateSkinShopUI();
    }

    public string samplePuzzle = "";
    public string sampleSolution = "";
    private bool skinPreviewCreated = false;

    private void UpdateSkinShopUI()
    {
        if (skinPreviewCreated)
        {
            //Clear previewImage
            foreach (var data in listView.Data)
            {
                (data as SkinShopEntry.SkinEntryData).renderTexture = null;
            }
            return;
        }
        listView.ClearData();
        List<SkinShopEntry.SkinEntryData> skinEntryDatas = new List<SkinShopEntry.SkinEntryData>();
        for (int i = 0; i < SkinManager.Instance.availableSkin.Count; i++)
        {
            skinEntryDatas.Add(new SkinShopEntry.SkinEntryData()
            {
                scriptableObject = SkinManager.Instance.availableSkin[i],
                previewBoardTemplate = boardTemplate,
                samplePuzzle = samplePuzzle,
                sampleSolution = sampleSolution,
                instantiatePos = new Vector3(i * 100, 1000, 0),
                index = i
            });
        }
        listView.AppendData(skinEntryDatas);
        skinPreviewCreated = true;
    }

    private void Start()
    {
        listView.displayDataAction += OnDisplayDataAction;
        closeButton.onClick.AddListener(Hide);
        GameManager.GameStateChanged += OnGameStateChanged;
        GameManager.ForceOutInGamScene += OnForceOutInGameScene;
    }

    private void OnDestroy()
    {
        listView.displayDataAction -= OnDisplayDataAction;
        GameManager.GameStateChanged -= OnGameStateChanged;
        GameManager.ForceOutInGamScene -= OnForceOutInGameScene;
    }


    void OnForceOutInGameScene()
    {
        if (IsShowing)
            Hide();
    }

    private void OnGameStateChanged(GameState arg1, GameState arg2)
    {
        if (IsShowing)
            Hide();
    }

    private void OnDisplayDataAction(GameObject item, object Data)
    {
        if (IsShowing == false)
            return;
        SkinShopEntry shopEntry = item.GetComponent<SkinShopEntry>();
        if(shopEntry.CurrentData != Data)
            shopEntry.ClearRenderTexture();
        shopEntry.UpdateUI((SkinShopEntry.SkinEntryData)Data);
    }
}
