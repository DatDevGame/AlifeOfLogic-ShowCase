using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopTournamentRewardInfo : MonoBehaviour {

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
                StartCoroutine(CR_ChangeSizeState());
            }
        }
    }

    public Image topIcon;
    public Image topFrame;
    public Text topNameTxt;
    public Text coinRewardInfo;

    private float defaultIconSize = 35;
    private float fullIconSize = 50;
    private Vector2 originalSize;

    public void SetInfo(int topIndex, int coinNumber, Sprite icon = null)
    {
        if (icon != null)
            topFrame.sprite = icon;
        topNameTxt.text = "Top " + topIndex;
        coinRewardInfo.text = coinNumber.ToString();
    }

    public void SetCoinNumber(int coinNumber)
    {
        coinRewardInfo.text = coinNumber.ToString();
    }

    IEnumerator CR_ChangeSizeState()
    {
        yield return new WaitForEndOfFrame();
        Vector2 startSize = topIcon.rectTransform.sizeDelta;
        Vector2 endSize = (isFullSize) ? Vector2.one * fullIconSize : Vector2.one * defaultIconSize;
        topNameTxt.gameObject.SetActive(isFullSize);
        float value = 0;
        float speed = 1 / 0.2f;
        while(value < 1)
        {
            value += Time.deltaTime * speed;
            topIcon.rectTransform.sizeDelta = Vector2.Lerp(startSize, endSize, value);
            yield return null;
        }
    }
}
