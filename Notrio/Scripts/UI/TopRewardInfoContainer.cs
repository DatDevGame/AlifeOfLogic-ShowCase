using System.Collections;
using System.Collections.Generic;
using Takuzu;
using Takuzu.Generator;
using UnityEngine;

public class TopRewardInfoContainer : MonoBehaviour {

    [Header("Config")]
    [SerializeField]
    private int amountTopInfoDisplay = 6;

    [SerializeField]
    private int columnNumber = 3;

    [SerializeField]
    private float timeDelaySwitchInfo = 2;

    [Header("Reference")]
    [SerializeField]
    private RectTransform snapPoint;

    [SerializeField]
    private GameObject topRewardInfoObject;

    [SerializeField]
    private RectTransform topRewardInfoLine;

    [SerializeField]
    public RectTransform detailRuleLine;

    [HideInInspector]
    public int currentShowIndex = 0;
    private int offset = 25;

    private bool isFullSize = true;
    public bool IsFullSize
    {
        get
        {
            return isFullSize;
        }
        set
        {
            if(value != isFullSize)
            {
                isFullSize = value;
                ChangeSizeState(isFullSize);
            }
        }
    }

    private int baseReward;
    private RectTransform[] infoLineList;
    private TopTournamentRewardInfo[] topRewardInfoList;
    private RectTransform rectTrans;
    private Vector3 originalPos;
    private Coroutine autoScrollCR;
    private Level currentLevel = Level.UnGraded;
    private Size currentSize;

	void Awake () 
    {
        rectTrans = GetComponent<RectTransform>();
        originalPos = transform.GetComponent<RectTransform>().localPosition;
        Init();
    }

    private void OnEnable()
    {
        currentShowIndex = 0;
        rectTrans.localPosition = new Vector3(originalPos.x, originalPos.y, originalPos.z);
        ChangeSizeState(isFullSize);
    }

    void Init()
    {
        currentShowIndex = 0;
        topRewardInfoList = new TopTournamentRewardInfo[amountTopInfoDisplay];
        int rowNumber = (amountTopInfoDisplay + columnNumber - 1) / columnNumber;
        infoLineList = new RectTransform[rowNumber];

        infoLineList[0] = topRewardInfoLine;
        infoLineList[0].parent = transform;
        infoLineList[0].localScale = Vector3.one;
        infoLineList[0].SetSiblingIndex(0);

        if (infoLineList.Length >= 2)
        {
            for (int i = 1; i < rowNumber; i++)
            {
                infoLineList[i] = Instantiate(topRewardInfoLine.gameObject).GetComponent<RectTransform>();
                infoLineList[i].parent = transform;
                infoLineList[i].localScale = Vector3.one;
                infoLineList[i].SetSiblingIndex(0);
            }
        }

        int topIndex = amountTopInfoDisplay;
        Sprite topIcon;
        for (int i = 0; i < rowNumber; i++)
        {
            for (int x = 0; x < columnNumber; x++)
            {
                topIcon = Resources.Load<Sprite>("topicon/" + string.Format("icon-top-{0}", (topIndex < 6) ? topIndex : 10));
                int coinReward = 500;
                GameObject topInfoObject = Instantiate(topRewardInfoObject);
                RectTransform topInfoRect = topInfoObject.GetComponent<RectTransform>();
                TopTournamentRewardInfo topTournamentInfo = topInfoObject.GetComponent<TopTournamentRewardInfo>();
                topInfoRect.parent = infoLineList[i].transform;
                topInfoRect.SetSiblingIndex(0);
                topInfoRect.localScale = Vector3.one;
                if (topIndex < 6)
                    topTournamentInfo.SetInfo(topIndex, coinReward, topIcon);
                else
                    topTournamentInfo.SetInfo(10, coinReward, topIcon);
                topTournamentInfo.IsFullSize = isFullSize;
                topRewardInfoList[topIndex - 1] = topTournamentInfo;
                topIndex--;
            }
        }
        if(currentLevel != Level.UnGraded)
        {
            UpdateCoinReward(currentSize, currentLevel);
        }
    }

    public void ChangeSizeState(bool isFullSize)
    {
        if (autoScrollCR != null)
            StopCoroutine(autoScrollCR);
        for (int i = 0; i < topRewardInfoList.Length; i++)
            topRewardInfoList[i].IsFullSize = isFullSize;

        if (isFullSize)
        {
            StartCoroutine(CR_BackToOriginalPos());
        }
        else
        {
            autoScrollCR = StartCoroutine(CR_AutoScrollVertical());
        }
    }

    IEnumerator CR_BackToOriginalPos()
    {
        detailRuleLine.gameObject.SetActive(true);
        float speed = 1 / 0.15f;
        float value = 0;
        Vector3 start = rectTrans.localPosition;
        while(value < 1)
        {
            value += Time.deltaTime * speed;
            rectTrans.localPosition = Vector3.Lerp(start, new Vector3(originalPos.x, originalPos.y + 200, originalPos.z), value);
            yield return null;
        }
        currentShowIndex = 0;
    }

    IEnumerator CR_AutoScrollVertical()
    {
        yield return new WaitForEndOfFrame();
        detailRuleLine.gameObject.SetActive(false);
        float speed = 1 / 0.15f;
        float value = 0;
        Vector3 start = rectTrans.localPosition;
        while (value < 1)
        {
            value += Time.deltaTime * speed;
            rectTrans.localPosition = Vector3.Lerp(start, new Vector3(originalPos.x, originalPos.y, originalPos.z), value);
            yield return null;
        }
        currentShowIndex = 0;
        speed = 1 / 0.4f;
        value = 0;
        while (!isFullSize)
        {
            if (currentShowIndex >= 0)
                yield return new WaitForSeconds(timeDelaySwitchInfo);
            currentShowIndex++;
            if (currentShowIndex < infoLineList.Length)
            {
                float distance = 50;
                if (currentShowIndex > 0)
                    distance = infoLineList[currentShowIndex - 1].sizeDelta.y / 2 + offset + infoLineList[currentShowIndex].sizeDelta.y / 2;
                Vector3 startPos = rectTrans.localPosition;
                Vector3 endPos = new Vector3(originalPos.x, rectTrans.localPosition.y - distance, originalPos.z);
                while (value < 1)
                {
                    value += Time.deltaTime * speed;
                    rectTrans.localPosition = Vector3.Lerp(startPos, endPos, value);
                    yield return null;
                }
                value = 0;
            }
            else
            {
                Vector3 startPos = rectTrans.localPosition;
                Vector3 endPos = new Vector3(originalPos.x, rectTrans.localPosition.y - infoLineList[currentShowIndex - 1].sizeDelta.y, originalPos.z);
                while (value < 1)
                {
                    value += Time.deltaTime * speed;
                    rectTrans.localPosition = Vector3.Lerp(startPos, endPos,value);
                    yield return null;
                }
                value = 0;
                currentShowIndex = -1;
                rectTrans.localPosition = new Vector3(originalPos.x, originalPos.y + 50, originalPos.z);
            }
        }
    }

    public void UpdateCoinReward(Size size, Level level)
    {
        currentLevel = level;
        currentSize = size;
        if (topRewardInfoList == null)
            return;

        for (int i = 0; i < topRewardInfoList.Length; i++)
        {
            int rewardCoin = CoinManager.Instance.GetTopRewardCoin(size, level, i);
            topRewardInfoList[i].SetCoinNumber(rewardCoin);
        }
    }
}
