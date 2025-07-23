using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;

public class CoinEnergyRewardAnimation : MonoBehaviour {
    public static CoinEnergyRewardAnimation Instance;
    [Header("UI refereces")]
    public OverlayEffect overlayEffect;
    public GameObject flyingCoin;
    public GameObject flyingEnergy;
    public Transform enegyTf;
    public bool IsRunning { get; private set; }

    Transform startTf;

    private void Awake()
    {
        if (Instance != null)
            DestroyImmediate(Instance.gameObject);
        Instance = this;
        IsRunning = false;
    }

    public void StartAnimation(Transform startTf, int amount, bool rewardCoin)
    {
        this.startTf = startTf;
        StopAllCoroutines();
        StartCoroutine(CrPlayCoinFlyingAnimAndHide(amount, rewardCoin));
    }

    private IEnumerator CrPlayCoinFlyingAnimAndHide(int rewardedCoin, bool rewardCoin)
    {
        IsRunning = true;
        CoinDisplayer ingameCoinDisplayer = UIReferences.Instance.gameUiPlayUI.CoinDisplayer;
        CoinDisplayer menuCoinDisplayer = UIReferences.Instance.gameUiHeaderUI.CoinDisplayer;
        CoinDisplayer coinDisplayer = GameManager.Instance.GameState == GameState.Prepare ? menuCoinDisplayer : ingameCoinDisplayer;
        if (rewardCoin)
        {
            coinDisplayer.offset -= rewardedCoin;
        }
        else
        {
            EnergyDisplayer.offset -= rewardedCoin;
        }
        yield return null;
        int coin = rewardedCoin;
        overlayEffect.StartPointMarker.position = startTf.position;
        Vector2 startPoint = overlayEffect.StartPointMarker.anchoredPosition;
        overlayEffect.endPointMarker.position = GameManager.Instance.GameState == GameState.Prepare ?
            (rewardCoin ? menuCoinDisplayer.icon.transform.position : enegyTf.position) :
            (rewardCoin ? ingameCoinDisplayer.icon.transform.position : UIReferences.Instance.energyIconIngame.position);
        Vector2 endPoint = overlayEffect.endPointMarker.anchoredPosition;
        int count = UnityEngine.Random.Range(5, 10 + 1) + coin / 20;
        for (int i = 0; i < count; ++i)
        {
            GameObject g = Instantiate(rewardCoin ? flyingCoin: flyingEnergy);
            g.transform.SetParent(overlayEffect.FlyingCoinRoot, false);
            g.transform.position = startPoint;
            RectTransform rt = g.transform as RectTransform;
            float size = UnityEngine.Random.Range(25, 60);
            rt.sizeDelta = size * Vector2.one;
            CoinFlyingEffect e = g.GetComponent<CoinFlyingEffect>();
            e.startPoint = startPoint;
            e.endPoint = endPoint;
            e.UpdateNormal();

            yield return null;
            yield return null;
        }
        float animMaxDuration = 3;
        int minCoinOffset = UnityEngine.Random.Range(1, 2 + 1);
        int coinOffset = Mathf.Max(minCoinOffset, (int)(coin * Time.smoothDeltaTime / animMaxDuration));
        //Debug.Log(coinOffset);
        if (rewardCoin)
        {
            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                CoroutineHelper.Instance.RepeatUntil(
                () =>
                {
                    coinDisplayer.offset = (int)Mathf.MoveTowards(coinDisplayer.offset, 0, coinOffset);
                    SoundManager.Instance.PlaySound(SoundManager.Instance.tick, true);
                },
                0,
                () => coinDisplayer.offset == 0);
            }, 1);
            
        }
        else
        {
            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                CoroutineHelper.Instance.RepeatUntil(
                () =>
                {
                    EnergyDisplayer.offset = (int)Mathf.MoveTowards(EnergyDisplayer.offset, 0, coinOffset);
                    SoundManager.Instance.PlaySound(SoundManager.Instance.tick, true);
                },
                0,
                () => EnergyDisplayer.offset == 0);
            }
            , 1);
        }
        IsRunning = false;
    }
}
