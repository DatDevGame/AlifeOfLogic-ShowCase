using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;
using System;
using GameSparks.Core;
using EasyMobile;

public class RulePopUpController : MonoBehaviour {

    public CanvasGroup rulePopupGroup;
    public Text rulePopupText;

    private RectTransform rulePopupRect;
    private bool isShowingRulePopup = false;
    private Coroutine rulePopupCoroutine;

    void Start ()
    {
        rulePopupRect = rulePopupGroup.gameObject.GetComponent<RectTransform>();
        rulePopupGroup.alpha = 0;
        isShowingRulePopup = false;
    }

    public void FadeRulePopUp(bool isFadeIn, float seconds)
    {
        if (isFadeIn != isShowingRulePopup)
        {
            isShowingRulePopup = isFadeIn;

            if (rulePopupCoroutine != null)
                StopCoroutine(rulePopupCoroutine);
            rulePopupCoroutine = StartCoroutine(CR_FadeCanvasGroup(rulePopupGroup, isFadeIn, seconds));
        }
    }

    IEnumerator CR_FadeCanvasGroup(CanvasGroup group, bool isFadeIn, float seconds)
    {
        float end = isFadeIn ? 1 : 0;
        if (seconds > 0)
        {
            float speed = 1 / seconds;
            float value = 0;
            float start = group.alpha;
            while (value < 1)
            {
                value += Time.deltaTime * speed;
                group.alpha = Mathf.Lerp(start, end, value);
                yield return null;
            }
        }
        group.alpha = end;
    }

    public void UpdateText(string msg, Vector2 size)
    {
        rulePopupRect.sizeDelta = size;
        rulePopupText.text = msg;
    }
}
