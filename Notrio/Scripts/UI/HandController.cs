using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour {

    [SerializeField]
    private bool ShowAtStart = false;

    [SerializeField]
    private float timeTap = 0.2f;

    [SerializeField]
    private float timeFade = 0.3f;

    [SerializeField]
    private float timeSwitchPos = 1;

    [SerializeField]
    private float distanceMove = 0.1f;

    [SerializeField]
    private Vector3 dirTap = Vector3.down;

    public bool IsShowing { get; set; }

    private Vector3 originalPosHand;
    protected SpriteRenderer handSpriteRender;
    private Coroutine fadeInCoroutine, fadeOutCoroutine, tapCoroutine, displayCoroutine;

	void Awake ()
    {
        handSpriteRender = transform.GetChild(0).GetComponent<SpriteRenderer>();
        originalPosHand = handSpriteRender.transform.localPosition;
        if (!ShowAtStart)
        {
            IsShowing = false;
            handSpriteRender.color = new Color(handSpriteRender.color.r, handSpriteRender.color.g,
                handSpriteRender.color.b, 0);
        }
        else
        {
            IsShowing = true;
            handSpriteRender.color = new Color(handSpriteRender.color.r, handSpriteRender.color.g,
                handSpriteRender.color.b, 1);
        }
    }

    public void ShowHand(List<Vector3> posList)
    {
        if (!IsShowing)
        {
            if (displayCoroutine != null)
                StopCoroutine(displayCoroutine);
            displayCoroutine = StartCoroutine(CR_ShowHand(posList));
            IsShowing = true;
        }
    }

    IEnumerator CR_ShowHand(List<Vector3> posList)
    {
        if (posList != null)
        {
            transform.position = posList[0];
            yield return new WaitForEndOfFrame();
            FadeIn();
            yield return new WaitForSeconds(timeFade);
            TapAnimation(dirTap);
            if (posList.Count > 1)
            {
                int i = 0;
                while(true)
                {
                    yield return new WaitForSeconds(timeSwitchPos);
                    i++;
                    if (i >= posList.Count)
                        i = 0;
                    FadeOut();
                    yield return new WaitForSeconds(timeFade);
                    transform.position = posList[i];
                    yield return new WaitForEndOfFrame();
                    FadeIn();
                    yield return new WaitForSeconds(timeFade);
                }
            }
        }
    }

    public void HideHand()
    {
        if (IsShowing)
        {
            if (displayCoroutine != null)
                StopCoroutine(displayCoroutine);

            if (tapCoroutine != null)
                StopCoroutine(tapCoroutine);

            FadeOut();
            IsShowing = false;
        }
    }

    void TapAnimation(Vector3 dir)
    {
        if (tapCoroutine != null)
            StopCoroutine(tapCoroutine);
        tapCoroutine = StartCoroutine(CR_TapAnimation(dir));
    }

    IEnumerator CR_TapAnimation(Vector3 dir)
    {
        Vector3 startPos = originalPosHand;
        Vector3 endPos = startPos + dir.normalized * distanceMove;
        float value = 0;
        bool isPressed = true;
        float speed = 1 / timeTap;
        while (true)
        {
            value += Time.deltaTime * speed;
            if(isPressed)
            {
                handSpriteRender.transform.localPosition = Vector3.Lerp(startPos, endPos, value);
            }
            else
            {
                handSpriteRender.transform.localPosition = Vector3.Lerp(endPos, startPos, value);
            }

            if (value >= 1)
            {
                isPressed = !isPressed;
                value = 0;
            }
            yield return null;
        }
    }

    void FadeOut()
    {
        if (fadeOutCoroutine != null)
            StopCoroutine(fadeOutCoroutine);
        if (fadeInCoroutine != null)
            StopCoroutine(fadeInCoroutine);
        fadeOutCoroutine = StartCoroutine(CR_Fade(false));
    }

    void FadeIn()
    {
        if (fadeInCoroutine != null)
            StopCoroutine(fadeInCoroutine);
        if (fadeOutCoroutine != null)
            StopCoroutine(fadeOutCoroutine);
        fadeInCoroutine = StartCoroutine(CR_Fade(true));
    }

    IEnumerator CR_Fade(bool isFadeIn)
    {
        Color curColor = handSpriteRender.color;
        Color targetColor = handSpriteRender.color;
        targetColor.a = isFadeIn ? 1 : 0;
        float value = 0;
        float speed = 1 / timeFade;
        while(value < 1)
        {
            value += Time.deltaTime * speed;
            handSpriteRender.color = Color.Lerp(curColor, targetColor, value);
            yield return null;
        }
    }
}
