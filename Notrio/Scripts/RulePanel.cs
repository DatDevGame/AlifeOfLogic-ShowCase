using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;
using System;
using GameSparks.Core;
using EasyMobile;

public class RulePanel : OverlayPanel
{
    private static string rule1Message { get { return I2.Loc.ScriptLocalization.Rule_Description_1; } }
    private static string rule2Message { get { return I2.Loc.ScriptLocalization.Rule_Description_2; } }
    private static string rule3Message { get { return I2.Loc.ScriptLocalization.Rule_Description_3; } }
    public Button closeButton;
    public Image[] ruleBtnImg;
    public Image[] ruleIconBtnImg;
    public Sprite[] ruleValidIcon;
    public Sprite[] ruleInvalidIcon;
    [Space]
    private ErrorsDisplayer errorDisplayer;
    public OverlayGroupController controller;

    private Coroutine showErrorCoroutine;
    private bool[] preErrorList = new bool[3] { false, false, false };
    private List<int> errorShowList = new List<int>();
    private int currentErrorIndex = -1;

    public Text rule1Txt;
    public Text rule2Txt;
    public Text rule3Txt;

    [Header("Config")]
    [SerializeField]
    private float timeShowRule = 6f;

    [SerializeField]
    private Color errorColor;

    [SerializeField]
    private Color noErrorColor;

    private RulePopUpController rulePopUpController;
    private PlayUI playUI;

    private void Awake()
    {
        if (UIReferences.Instance != null)
        {
            OnUiReferencesUpdated();
        }
        UIReferences.UiReferencesUpdated += OnUiReferencesUpdated;
        ErrorsDisplayer.UpdateRuleViolate += UpdateRuleViolate;
        GameManager.GameStateChanged += OnGameStateChanged;
        GameManager.ForceOutInGamScene += OnForceOutInGameScene;

        rule1Txt.text = rule1Message;
        rule2Txt.text = String.Format(rule2Message, 0, 1);
        rule3Txt.text = rule3Message;
    }

    private void OnDestroy()
    {
        UIReferences.UiReferencesUpdated -= OnUiReferencesUpdated;
        ErrorsDisplayer.UpdateRuleViolate -= UpdateRuleViolate;
        GameManager.GameStateChanged -= OnGameStateChanged;
        GameManager.ForceOutInGamScene -= OnForceOutInGameScene;
    }

    void OnForceOutInGameScene()
    {
        if (IsShowing)
            Hide();
    }

    void OnUiReferencesUpdated()
    {
        rulePopUpController = UIReferences.Instance.rulePopUpController;
        errorDisplayer = UIReferences.Instance.errorDisplay;
        playUI = UIReferences.Instance.gameUiPlayUI;
    }

    private void OnGameStateChanged(GameState newState,GameState oldState)
    {
        if (newState == GameState.Prepare || newState == GameState.GameOver)
        {
            if (rulePopUpController != null)
            {
                rulePopUpController.FadeRulePopUp(false, 0);
                ResetError();
            }
        }
    }

    void UpdateRuleViolate(bool[] ruleError)
    {
        for(int i = 0;i< ruleError.Length;i++)
        {
            if(preErrorList[i] != ruleError[i])
            {
                if (ruleError[i])
                {
                    if (!errorShowList.Contains(i))
                    {
                        errorShowList.Add(i);
                    }
                }
                else
                {
                    errorShowList.Remove(i);
                }
                preErrorList[i] = ruleError[i];
            }

            if (ruleError[i])
            {
                ruleBtnImg[i].color = errorColor;
                ruleIconBtnImg[i].sprite = ruleInvalidIcon[i];
            }
            else
            {
                ruleBtnImg[i].color = noErrorColor;
                ruleIconBtnImg[i].sprite = ruleValidIcon[i];
            }
        }

        if(errorShowList.Count <= 0)
        {
            int newIndex = -1;
            for (int i = 0; i < preErrorList.Length; i++)
            {
                if (preErrorList[i])
                    newIndex = i;
            }
            if (newIndex != -1)
                errorDisplayer.SwitchRuleIcon(newIndex, true);
            else
                errorDisplayer.SwitchRuleIcon(3, false);
            rulePopUpController.FadeRulePopUp(false, 0.5f);
        }
        else
        {
            if (UIGuide.instance.IsGuildeShown(playUI.guideFirstErrorSaveKey))
            {
                rulePopUpController.FadeRulePopUp(true, 0.5f);
            }
            if (showErrorCoroutine != null)
                StopCoroutine(showErrorCoroutine);
            showErrorCoroutine = StartCoroutine(CR_UpdateErrorPopUp());
        }
    }

    public void ResetError()
    {
        currentErrorIndex = -1;
        preErrorList = new bool[3] { false, false, false };
        errorShowList.Clear();
        for (int i = 0; i < 3; i++)
        {
            ruleBtnImg[i].color = noErrorColor;
        }
        if (rulePopUpController != null)
            rulePopUpController.FadeRulePopUp(false, 0.5f);
    }

    void Start ()
    {
        closeButton.onClick.AddListener(delegate
        {
            Hide();
        });
    }

    public override void Show()
    {
        if (showErrorCoroutine != null)
            StopCoroutine(showErrorCoroutine);
        errorShowList.Clear();
        rulePopUpController.FadeRulePopUp(false, 0.1f);
        controller.ShowIfNot();
        IsShowing = true;
        transform.BringToFront();
        onPanelStateChanged(this, true);
    }

    public override void Hide()
    {
        controller.HideIfNot();
        IsShowing = false;
        onPanelStateChanged(this, false);
    }

    IEnumerator CR_UpdateErrorPopUp()
    {
        while (errorShowList.Count > 0)
        {
            currentErrorIndex = errorShowList[0];
            errorDisplayer.SwitchRuleIcon(currentErrorIndex, true);
            switch (currentErrorIndex)
            {
                case 0:
                    rulePopUpController.UpdateText(rule1Message, new Vector2(400, 75));
                    break;
                case 1:
                    rulePopUpController.UpdateText(String.Format(rule2Message, 0, 1), new Vector2(400, 75));
                    break;
                case 2:
                    rulePopUpController.UpdateText(rule3Message, new Vector2(400, 75));
                    break;
            }
            yield return new WaitForSeconds(timeShowRule);
            errorShowList.Remove(currentErrorIndex);
        }

        int count = 0;
        for(int i = 0; i < preErrorList.Length;i++)
        {
            if (preErrorList[i])
                count++;
        }

        if (count == 0)
        {
            currentErrorIndex = -1;
            errorDisplayer.SwitchRuleIcon(3, false);
        }
        rulePopUpController.FadeRulePopUp(false, 0.5f);
    }
}
