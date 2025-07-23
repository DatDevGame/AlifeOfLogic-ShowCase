using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyUI : MonoBehaviour {
    public Image currentEnergy;
    public RectTransform parentRt;
    public Color EmptyColor;
    public Color FullColor;
    public Text energyText;
    public float updateTime = 0.5f;

    private bool isChanging = false;
    private void Start()
    {
        EnergyManager.currentEnergyChanged += OnCurrentEnergyChanged;
        EnergyManager.maxEnergyChanged += OnMaxEnergyChanged;

        UpdateEnergyBar();

    }
    private void OnDestroy()
    {
        EnergyManager.currentEnergyChanged -= OnCurrentEnergyChanged;
        EnergyManager.maxEnergyChanged -= OnMaxEnergyChanged;

    }

    private void OnMaxEnergyChanged(int newMaxE, int oldMaxE)
    {
        UpdateEnergyBar();
    }

  
    private void OnCurrentEnergyChanged(int newE, int oldE)
    {
        UpdateEnergyBar();
    }

    private void UpdateEnergyBar()
    {
        if(!isChanging)
            StartCoroutine(DelayResponeToChange());
        
    }

    private IEnumerator DelayResponeToChange()
    {
        isChanging = true;
        yield return new WaitForEndOfFrame();
        float currentE = EnergyManager.Instance.CurrentEnergy;
        float maxE = EnergyManager.Instance.MaxEnergy;
        float percent = (Mathf.Min(currentE, maxE)) / maxE;

        float currentPercent = currentEnergy.rectTransform.anchoredPosition.x / parentRt.sizeDelta.x;

        float t = 0;
        while (t < updateTime)
        {
            UpdateSlide(Mathf.Lerp(currentPercent, percent, t/updateTime));
            UpdateColor(Mathf.Lerp(currentPercent, percent, t / updateTime), EmptyColor, FullColor);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        UpdateSlide(percent);
        UpdateColor(percent, EmptyColor, FullColor);
        UpdateText((int)currentE, (int)maxE);

        isChanging = false;
    }

    private void UpdateText(int currentE, int maxE)
    {
        energyText.text = String.Format("{0}/{1}", currentE, maxE);
    }

    private void UpdateColor(float percent, Color emptyColor, Color fullColor)
    {
        currentEnergy.color = Color.Lerp(emptyColor, fullColor, percent);
    }

    private void UpdateSlide(float percent)
    {
        currentEnergy.rectTransform.anchoredPosition = new Vector2(percent* parentRt.sizeDelta.x, currentEnergy.rectTransform.anchoredPosition.y);
    }
}
