using System;
using System.Collections;
using System.Collections.Generic;
using EasyMobile;
using Takuzu;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class EnergyDisplayer : MonoBehaviour
{
    public Text energyText;
    public Text timeLeftText;
    public GameObject timeLeftObj;
    public GameObject infinityIcon;
    public static int offset;
    private bool isInfiniteEnergy;
    // Use this for initialization

    void Start()
    {
        energyText = GetComponent<Text>();
        InAppPurchasing.PurchaseCompleted += OnPurchaseCompleted;
        InAppPurchasing.RestoreCompleted += OnRestoreCompleted;
        GameManager.GameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        InAppPurchasing.PurchaseCompleted -= OnPurchaseCompleted;
        InAppPurchasing.RestoreCompleted -= OnRestoreCompleted;
        GameManager.GameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Prepare || newState == GameState.Playing)
        {
            if (EnergyManager.Instance != null)
            {
                isInfiniteEnergy = EnergyManager.Instance.AlwaysMaxEnergy();
            }
        }
    }

    void OnPurchaseCompleted(IAPProduct product)
    {
        if (EnergyManager.Instance != null)
        {
            isInfiniteEnergy = EnergyManager.Instance.AlwaysMaxEnergy();
        }
    }

    void OnRestoreCompleted()
    {
        if (EnergyManager.Instance != null)
        {
            isInfiniteEnergy = EnergyManager.Instance.AlwaysMaxEnergy();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateText_CR());
        if (EnergyManager.Instance != null)
        {
            isInfiniteEnergy = EnergyManager.Instance.AlwaysMaxEnergy();
        }
    }

    private IEnumerator UpdateText_CR()
    {
        while (true)
        {
            UpdateText();
            yield return new WaitForSecondsRealtime(1);
        }
    }

    private void UpdateText()
    {
        if (EnergyManager.Instance == null)
            return;

        if (isInfiniteEnergy)
        {
            //Infinite energy
            if (timeLeftObj != null && timeLeftObj.activeSelf)
                timeLeftObj.SetActive(false);
            if (energyText != null && energyText.enabled)
                energyText.enabled = false;
            if (infinityIcon != null && infinityIcon.activeSelf == false)
                infinityIcon.gameObject.SetActive(true);
            return;
        }
        if (infinityIcon != null && infinityIcon.activeSelf)
            infinityIcon.gameObject.SetActive(false);
        if (energyText != null && energyText.enabled == false)
            energyText.enabled = true;

        energyText.text = String.Format("{0}/{1}", EnergyManager.Instance.CurrentEnergy + offset, EnergyManager.Instance.MaxEnergy);
        float timeLeft = EnergyManager.Instance.IncreaseEnergyInterval - (float)(TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds - EnergyManager.Instance.lastTime);
        if (timeLeftObj != null)
            if (timeLeftObj.activeSelf != EnergyManager.Instance.CurrentEnergy < EnergyManager.Instance.MaxEnergy)
                timeLeftObj.SetActive(EnergyManager.Instance.CurrentEnergy < EnergyManager.Instance.MaxEnergy);
        if (timeLeftText != null)
            timeLeftText.text = String.Format("{0:00}:{1:00}", (int)timeLeft / 60, (int)timeLeft % 60);
    }

    public void ShowEarnCoinsAnim(Transform transform, int amount, GameObject coinTemplate)
    {
        StopCoroutine("EarnCoinsAnim");
        StartCoroutine(EarnCoinsAnim(transform, amount, coinTemplate));
    }

    private IEnumerator EarnCoinsAnim(Transform transform, int amount, GameObject coinTemplate)
    {
        GameObject coin = Instantiate(coinTemplate, transform);
        float animationTime = 0;
        Vector3 normalScale = coin.transform.localScale;
        while (animationTime < 1)
        {
            yield return new WaitForEndOfFrame();
            animationTime += Time.deltaTime;
            coin.transform.position = Vector3.Lerp(transform.position, this.transform.position, animationTime);
            coin.transform.localScale = normalScale * (1 - animationTime);
        }
        DestroyImmediate(coin);
        energyText.text = String.Format("{0}/{1}", EnergyManager.Instance.CurrentEnergy, EnergyManager.Instance.MaxEnergy + amount);
    }
}
