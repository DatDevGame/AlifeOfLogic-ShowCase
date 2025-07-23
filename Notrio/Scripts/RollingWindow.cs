using System;
using System.Collections;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;
using UnityEngine.UI;

public class RollingWindow : MonoBehaviour{

    [Header("UI References")]
    public Transform container;
    public GameObject rollingItemTemplate;
    public Image containerImage;
    public Sprite defaultSprite;
    public Sprite[] rollingSprites;

    [Header("Setting")]
    public float spacing = 0;
    public int numberOfItems = 7;
    public int numberOfMiddleScrollingItem = 50;
    public AnimationCurve rollingAnimationCurve;
    //-----------------------------
    public float ScrollPercent { set { value = value%1 +Mathf.Clamp01(value - 1f/itemDataList.Count); CurrentIndex = (int)(value * (itemDataList.Count)); PercentInItem = value * (itemDataList.Count) % 1; } get { return (CurrentIndex + PercentInItem)/(itemDataList.Count); } }

    public List<RollingItem.RollingItemData> itemDataList = new List<RollingItem.RollingItemData>();
    private List<GameObject> rollingItemHolderList = new List<GameObject>();
    private Coroutine rollingCoroutine;
    private float PercentInItem
    {
        get
        {
            return m_percentInItem;
        }

        set
        {
            m_percentInItem = value;
            UpdateScrollPosition();
        }
    }

    private int CurrentIndex
    {
        get
        {
            return m_currentIndex;
        }

        set
        {
            m_currentIndex = value;
            UpdateScrollItem();
        }
    }

    private float m_percentInItem = 0;
    private int m_currentIndex = 0;

    private void OnDisable()
    {
        if (rollingCoroutine != null)
        {
            StopCoroutine(rollingCoroutine);
        }
    }

    private void Start()
    {
        containerImage.sprite = defaultSprite;
        CreateNewRollingWindow();
    }

    public void CreateNewRollingWindow()
    {
        ClearDataList();
        InitRollingItem();
        SetupBaseRollItem(CoinManager.Instance.rollingItemDatas.FindAll(item => item.rewardCoins == CoinManager.luckySpinnerRewardCoin));
    }

    public void SetupBaseRollItem(List<RollingItem.RollingItemData> randomList)
    {
        SetTargetRollItem(randomList, numberOfItems + 2);
        CurrentIndex = 3;
        PercentInItem = 0;
    }

    public float GetRollingDuration()
    {
        return rollingAnimationCurve.keys[rollingAnimationCurve.keys.Length - 1].time;
    }

    public void SettupRollingWindow(RollingItem.RollingItemData targetData, List<RollingItem.RollingItemData> randomList)
    {
        SetTargetRollItem(randomList, numberOfMiddleScrollingItem , targetData);
        UpdateScrollItem();
        UpdateScrollPosition();
    }

    public void StartRolling()
    {
        if (rollingCoroutine != null)
        {
            StopCoroutine(rollingCoroutine);
        }
        rollingCoroutine = StartCoroutine(RollingCR());
    }

    private IEnumerator RollingCR()
    {
        float t = +((float)numberOfItems) / (itemDataList.Count - 1);
        float duration = rollingAnimationCurve.keys[rollingAnimationCurve.keys.Length - 1].time;
        float playSoundIndex = 0.06f;
        int count = 0;
        int lengthSprites = rollingSprites.Length;
        int index = 0;
        float currentPercent = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            ScrollPercent = rollingAnimationCurve.Evaluate(t);
            if (currentPercent + 0.05f < ScrollPercent && lengthSprites > 0)
            {
                index++;
                if (index >= lengthSprites)
                    index = 0;
                containerImage.sprite = rollingSprites[index];
                currentPercent = ScrollPercent;
            }

            if (ScrollPercent > playSoundIndex * count)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.tick, true);
                count++;
            }
            yield return null;
        }
        SoundManager.Instance.PlaySound(SoundManager.Instance.tick, true);
        SoundManager.Instance.PlaySound(SoundManager.Instance.unlock, true);
    }

    public void ClearDataList()
    {
        itemDataList.Clear();
    }

    private void SetTargetRollItem(List<RollingItem.RollingItemData> randomList, int count, RollingItem.RollingItemData targetData = null)
    {
        for (int i = 0; i < count; i++)
        {
            itemDataList.Add(randomList[UnityEngine.Random.Range(0, randomList.Count)]);
        }
        if(targetData!=null)
            itemDataList.Add(targetData);
    }

    private void InitRollingItem()
    {
        foreach (var go in rollingItemHolderList)
        {
            DestroyImmediate(go);
        }
        rollingItemHolderList.Clear();
        for (int i = 0; i < numberOfItems; i++)
        {
            GameObject go = Instantiate(rollingItemTemplate, container);
            rollingItemHolderList.Add(go);
        }
    }

    private void UpdateScrollPosition()
    {
        float templateHeight = (rollingItemTemplate.transform as RectTransform).rect.height;
        for (int i = 0; i < rollingItemHolderList.Count; i++)
        {
            RectTransform rectRT = rollingItemHolderList[i].transform as RectTransform;
            rectRT.anchoredPosition = new Vector2( rectRT.anchoredPosition.x , (i - numberOfItems/2) *(templateHeight + spacing) - PercentInItem* (templateHeight+ spacing));
        }
    }


    private void UpdateScrollItem()
    {
        int itemIndex = 0;
        for (int i = 0; i < rollingItemHolderList.Count; i++)
        {
            RollingItem rollingItem = rollingItemHolderList[i].GetComponent<RollingItem>();
            itemIndex = CurrentIndex + i - numberOfItems/2;
            rollingItem.UpdateData(itemDataList[itemIndex % itemDataList.Count]);
        }
    }
}
