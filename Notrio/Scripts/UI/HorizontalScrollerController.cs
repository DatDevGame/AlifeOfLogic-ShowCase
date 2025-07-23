using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalScrollerController : MonoBehaviour {
    [Header("Config")]
    [SerializeField]
    private float displayWidth = 0;
    [SerializeField]
    private float timeScroll = 8;

    [Header("References")]
    public Text contentTxt;

    private Vector3 originalPos;
    private Vector3 endPos;
    private Coroutine scrollCR;


    private void OnEnable()
    {
        if (scrollCR != null)
            StopCoroutine(scrollCR);
        scrollCR = StartCoroutine(CR_Scroll());
    }

    private void OnDisable()
    {
        if (scrollCR != null)
            StopCoroutine(scrollCR);
    }
    
    IEnumerator CR_Scroll()
    {
        yield return new WaitForEndOfFrame();
        originalPos = new Vector3(displayWidth, contentTxt.rectTransform.localPosition.y, contentTxt.rectTransform.localPosition.z);
        contentTxt.rectTransform.localPosition = originalPos;
        endPos = new Vector3(-contentTxt.rectTransform.sizeDelta.x, contentTxt.rectTransform.localPosition.y, contentTxt.rectTransform.localPosition.z);
        float value = 0;
        float speed = 1 / timeScroll;
        while (true)
        {
            while (value < 1)
            {
                value += Time.deltaTime * speed;
                contentTxt.rectTransform.localPosition = Vector3.Lerp(originalPos, endPos, value);
                yield return null;
            }
            contentTxt.rectTransform.localPosition = originalPos;
            value = 0;
        }
    }

}
