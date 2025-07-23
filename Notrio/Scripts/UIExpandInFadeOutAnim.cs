using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;

public class UIExpandInFadeOutAnim : UIInOutAnim {

    List<Image> images = new List<Image>();
    public bool IsFadeIn { get; private set; }
    public bool IsShowAtStart;
    public AnimationCurve curve;
    void Start()
    {
        images.Add(gameObject.GetComponent<Image>());
        images.AddRange(gameObject.GetComponentsInChildren<Image>());
        if (!IsShowAtStart)
        {
            for (int i = 0; i < images.Count; i++)
            {
                if (images[i])
                {
                    images[i].transform.localScale = new Vector3(curve.Evaluate(0), 1, 1);
                }
            }
            IsFadeIn = false;
        }
        else
        {
            IsFadeIn = true;
        }
    }

    public override void FadeIn(float duration)
    {
        if (!IsFadeIn)
        {
            base.FadeIn(duration);
            StartCoroutine(CR_FadeIn(duration));
            IsFadeIn = true;
        }
    }
    public override void FadeOut(float duration)
    {
        if (IsFadeIn)
        {
            base.FadeOut(duration);
            IsFadeIn = false;
        }
    }
    private IEnumerator CR_FadeIn(float duration)
    {
        float timeLeft = duration;
        yield return new WaitForSeconds(duration / 4);
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            for (int i = 0; i < images.Count; i++)
            {
                if (images[i])
                {
                    images[i].transform.localScale = new Vector3(curve.Evaluate(1 - (timeLeft / duration)), 1,1);
                }
            }
            yield return new WaitForEndOfFrame();
        }
        for (int i = 0; i < images.Count; i++)
        {
            if (images[i])
            {
                images[i].transform.localScale = Vector3.one;
            }
        }
        yield return null;
    }
}
