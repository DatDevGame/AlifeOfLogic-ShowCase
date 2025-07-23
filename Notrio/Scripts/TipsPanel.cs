using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;
using UnityEngine.UI;

public class TipsPanel : OverlayPanel
{
    [Header("UI References")]
    public Text title;
    public Transform container;
    public SnappingScroller scroller;
    public GameObject tipTemplate;
    public GameObject boardTemplate;
    public RawImage target;
    private List<TipDetailPanel> tipDetailPanels = new List<TipDetailPanel>();
    public Button nextButton;
    public Button backButton;
    public GameObject navigationGroup;
    public Button closeButton;
    public OverlayGroupController controller;

    private void Awake()
    {
        GameManager.ForceOutInGamScene += OnForceOutInGameScene;
        GameManager.GameStateChanged += OnGameStateChanged;
    }

    void OnForceOutInGameScene()
    {
        if (IsShowing)
            Hide();
    }

    void OnGameStateChanged(GameState newState, GameState oldState)
    {
        if (GameManager.Instance.GameState == GameState.Prepare || GameManager.Instance.GameState == GameState.GameOver)
        {
            if (IsShowing)
                Hide();
        }
    }

    private void Start()
    {
        backButton.onClick.AddListener(() =>
        {
            scroller.SnapIndex -= 1;
            scroller.Snap();
        });
        nextButton.onClick.AddListener(() =>
        {
            scroller.SnapIndex += 1;
            scroller.Snap();
        });
        scroller.onSnapIndexChanged += OnSnapIndexChanged;
        TipsManager.Instance.UpdateTipsList += OnTipListUpdated;
        OnTipListUpdated();
        closeButton.onClick.AddListener(delegate
        {
            foreach (var tipDetail in tipDetailPanels)
            {
                tipDetail.RequestRunning = false;
            }
            Hide();
        });

    }

    public void ShowThisTip(TipInformationScriptableObject tipInformationScriptableObject, int tipIndex, string id, Action<string> callback)
    {
        ClearTipsObject();

        scroller.presetElements = new RectTransform[1];
        GameObject go = Instantiate(tipTemplate, container);
        GameObject board = Instantiate(boardTemplate, UIReferences.Instance.tipsBoardContainer);
        scroller.AddElement(go.transform as RectTransform);
        scroller.presetElements[0] = go.transform as RectTransform;
        TipDetailPanel tipDetail = go.GetComponent<TipDetailPanel>();
        tipDetail.tipInformation = tipInformationScriptableObject;
        tipDetail.cameraController = board.GetComponent<BoardInstanceCameraController>();
        tipDetail.lb = board.GetComponent<BoardLogical>();
        tipDetail.boardVisualizer = board.GetComponent<BoardVisualizer>();
        tipDetailPanels.Add(tipDetail);
        tipDetail.puzzleImage = target;
        tipDetail.RequestRunning = true;
        tipDetail.InitBoard();
        //title.text = tipDetail.tipInformation.tipTitle.ToUpper();
        title.text = String.Format("{0} #{1}", I2.Loc.ScriptLocalization.TIP.ToUpper(), tipIndex + 1);

        bool callbacked = false;
        closeButton.onClick.AddListener(delegate
        {
            if (!callbacked)
            {
                tipDetail.RequestRunning = false;
                Hide();
                if (callback != null)
                    callback(id);
                callbacked = true;
                TipsManager.Instance.UpdateTipsList();
            }
        });
        navigationGroup.SetActive(false);
        controller.ShowIfNot();
        IsShowing = true;
        transform.BringToFront();
        onPanelStateChanged(this, true);
        TipsManager.Instance.MarkAsShownTip(tipInformationScriptableObject);
        scroller.lockDirection = SnappingScroller.LockDirection.Both;
    }

    public override void Show()
    {
        controller.ShowIfNot();
        IsShowing = true;
        transform.BringToFront();
        onPanelStateChanged(this, true);
        UpdateUI();
        RunAnimtion();
    }

    private void RunAnimtion()
    {
        int index = Mathf.Clamp(scroller.SnapIndex, 0, scroller.ElementCount - 1);
        StopAnimation();
        tipDetailPanels[index].RequestRunning = true;
    }

    public override void Hide()
    {
        StopAnimation();
        controller.HideIfNot();
        IsShowing = false;
        onPanelStateChanged(this, false);
    }

    private void StopAnimation()
    {
        foreach (var tip in tipDetailPanels)
        {
            tip.RequestRunning = false;
        }
    }

    private void OnDestroy()
    {
        scroller.onSnapIndexChanged -= OnSnapIndexChanged;
        TipsManager.Instance.UpdateTipsList -= OnTipListUpdated;
        GameManager.ForceOutInGamScene -= OnForceOutInGameScene;
        GameManager.GameStateChanged -= OnGameStateChanged;
    }

    private void OnTipListUpdated()
    {
        //UpdateUI();
    }

    private void OnSnapIndexChanged(int arg1, int arg2)
    {
        if (arg1 == arg2 || arg1 < 0 || arg2 < 0 || arg1 >= scroller.ElementCount || arg2 >= scroller.ElementCount)
            return;
        int index = Mathf.Clamp(arg1, 0, scroller.ElementCount - 1);
        foreach (var tip in tipDetailPanels)
        {
            tip.RequestRunning = false;
        }
        backButton.gameObject.SetActive(index != 0);
        nextButton.gameObject.SetActive(index != scroller.ElementCount - 1);
        //title.text = tipDetailPanels[index].tipInformation.tipTitle.ToUpper();
        title.text = String.Format("{0} #{1}", I2.Loc.ScriptLocalization.TIP.ToUpper(), index + 1);
        tipDetailPanels[index].RequestRunning = true;
    }

    private void UpdateUI()
    {
        ClearTipsObject();

        scroller.presetElements = new RectTransform[TipsManager.Instance.availabaleTips.Count];
        if (TipsManager.Instance.availabaleTips.Count == 0)
            return;

        for (int i = 0; i < TipsManager.Instance.availabaleTips.Count; i++)
        {
            GameObject go = Instantiate(tipTemplate, container);
            GameObject board = Instantiate(boardTemplate, UIReferences.Instance.tipsBoardContainer);
            board.transform.localPosition = new Vector3(i * 100, 0, 0);
            scroller.AddElement(go.transform as RectTransform);
            scroller.presetElements[i] = go.transform as RectTransform;
            TipDetailPanel tipDetail = go.GetComponent<TipDetailPanel>();
            tipDetail.puzzleImage = target;
            tipDetail.tipInformation = TipsManager.Instance.availabaleTips[i];
            tipDetail.cameraController = board.GetComponent<BoardInstanceCameraController>();
            tipDetail.lb = board.GetComponent<BoardLogical>();
            tipDetail.boardVisualizer = board.GetComponent<BoardVisualizer>();
            tipDetailPanels.Add(tipDetail);
            tipDetail.RequestRunning = false;
            TipsManager.Instance.MarkAsShownTip(TipsManager.Instance.availabaleTips[i]);
        }

        scroller.lockDirection = TipsManager.Instance.availabaleTips.Count > 1 ? SnappingScroller.LockDirection.None : SnappingScroller.LockDirection.Both;
        navigationGroup.SetActive(TipsManager.Instance.availabaleTips.Count > 1);
        scroller.SnapIndex = 0;
        scroller.SnapImmediately();
        //title.text = tipDetailPanels[scroller.SnapIndex].tipInformation.tipTitle.ToUpper();
        title.text = String.Format("{0} #{1}", I2.Loc.ScriptLocalization.TIP.ToUpper(), scroller.SnapIndex + 1);
        tipDetailPanels[scroller.SnapIndex].RequestRunning = true;
        backButton.gameObject.SetActive(false);
    }

    private void ClearTipsObject()
    {
        container.ClearAllChildren();
        scroller.ClearElement();
        tipDetailPanels.Clear();
        UIReferences.Instance.tipsBoardContainer.ClearAllChildren();
    }
}
