using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;
using System;

[RequireComponent(typeof(Text))]
public class CoinDisplayer : MonoBehaviour
{
    public Image icon;
    public Text text;
    public int offset;

    private void Reset()
    {
        text = GetComponent<Text>();
    }

    private void Update()
    {
        int n = CoinManager.Instance.Coins + offset;
        text.text = n.ToString();
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
        int n = Mathf.Max(0, CoinManager.Instance.Coins + amount);
        text.text = n.ToString();
    }
}
