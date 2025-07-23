using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomProgressBar : MonoBehaviour {
    public GameObject dot;
    private List<GameObject> dotList = new List<GameObject>();
    private int amountProgressDot;
    public GameObject progressBarContainer;
    [Header("Config")]
    public Color bgColor;
    public Color progressColor;

    private float progress;
    private bool showIndicator;

    private float animationDuration;

    private bool requireUpdateAfterActive = false;
    private bool requireAnimateToProgress = false;

    public void Hide()
    {
        progressBarContainer.gameObject.SetActive(false);
    }

    public void Show()
    {
        progressBarContainer.gameObject.SetActive(true);
    }

    public void SetProgress(float progress,bool showIndicator = false , int segments = 1)
    {
        this.progress = progress;
        this.showIndicator = showIndicator;
        this.amountProgressDot = segments;
        if (gameObject.activeInHierarchy)
            StartCoroutine(WaitForUnityLayout(progress, showIndicator));
        else
            requireUpdateAfterActive = true;
        ClearDots();
        CreateDots(amountProgressDot);
    }

    public void AnimateToProgress(float progress,int amountDot, float duration, bool showIndicator = false)
    {
        this.progress = progress;
        this.showIndicator = showIndicator;
        this.animationDuration = duration;
        if (gameObject.activeInHierarchy)
            StartCoroutine(AnimateToProgressCR(progress, amountDot, duration, showIndicator));
        else
            requireAnimateToProgress = true;
    }

    private IEnumerator AnimateToProgressCR(float progress, int dotsAmount,float duration , bool showIndicator)
    {
        requireAnimateToProgress = false;
        int currentDotIndex = Mathf.Max(0, (int)(progress * dotsAmount) - 1);

        Image dotImg = dotList[currentDotIndex].GetComponentInChildren<Image>();
        float value = 0;
        float speed = 1 / (duration / 2);
        Vector3 startScale = dotImg.transform.localScale;
        Vector3 endScale = dotImg.transform.localScale * 1.3f;
        while (value < 1)
        {
            value += Time.deltaTime * speed;
            dotImg.transform.localScale = Vector3.Lerp(startScale, endScale, value);
            dotImg.color = Color.Lerp(bgColor, progressColor * 1.2f, value);
            yield return null;
        }

        value = 0;
        while (value < 1)
        {
            value += Time.deltaTime * speed;
            dotImg.transform.localScale = Vector3.Lerp(endScale, startScale, value);
            dotImg.color = Color.Lerp(progressColor * 1.2f, progressColor, value);
            yield return null;
        }

        UpdateProgess(progress, amountProgressDot, showIndicator);
    }

    private void OnEnable()
    {
        if (requireAnimateToProgress)
            StartCoroutine(AnimateToProgressCR(progress, amountProgressDot, animationDuration, showIndicator));
        else if (requireUpdateAfterActive)
            StartCoroutine(WaitForUnityLayout(progress, showIndicator));
    }

    private IEnumerator WaitForUnityLayout(float progress, bool showIndicator = false)
    {
        requireUpdateAfterActive = false;
        yield return new WaitForEndOfFrame();
        UpdateProgess(progress, amountProgressDot, showIndicator);
    }

    private void UpdateProgess(float progress, int dotsAmount, bool showIndicator)
    {
        progress = Mathf.Clamp01(progress);
        int currentDotIndex = (int)(progress * dotsAmount);
        for (int i = 0; i < dotsAmount; i++)
        {
            if (i < currentDotIndex)
                dotList[i].GetComponentInChildren<Image>().color = progressColor;
            else
                dotList[i].GetComponentInChildren<Image>().color = bgColor;

        }
    }

    void ClearDots()
    {
        foreach (var d in dotList)
        {
            DestroyImmediate(d);
        }
        dotList.Clear();
    }

    void CreateDots(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject d = Instantiate(dot, progressBarContainer.transform);
            d.GetComponentInChildren<Image>().color = bgColor;
            dotList.Add(d);
        }
    }
}
