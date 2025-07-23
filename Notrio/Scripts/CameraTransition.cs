using System.Collections;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum MoveType
{
    Top,Bot,Left,Right,None
}

[System.Serializable]
public struct ZoomEffect
{
    [SerializeField]
    public bool useZoom;

    [SerializeField]
    public float targetorthoSize;

    [SerializeField]
    public float startTime;

    [SerializeField]
    public float duration;
}

[System.Serializable]
public struct CharacterInfo
{
    public Sprite sprite;
    public Vector3 pos;
    public Vector3 scale;
}


[System.Serializable]
public struct BackgroundInfo
{
    public Sprite sprite;
    public Vector3 startPos;
    public Vector3 endPos;
    public float timeMove;
    public AnimationCurve curveAniMove;
}

[System.Serializable]
public struct TransitionEffect
{
    [SerializeField]
    public bool useFadeEffect;

    [SerializeField]
    public MoveType moveEffect;

    [SerializeField]
    public float timeTrans;
}

[System.Serializable]
public class ConversationInfo
{
    [SerializeField]
    public string text;

    [SerializeField]
    public string textLocalizationKey = "";

    [SerializeField]
    public float delaytime;

    [SerializeField]
    public float duration;

    public string Text
    {
        get
        {
            if (string.IsNullOrEmpty(textLocalizationKey))
            {
                return text;
            }
            else
            {
                return I2.Loc.LocalizationManager.GetTranslation(textLocalizationKey);
            }
        }
    }
}

[System.Serializable]
public struct PopupInfo
{
    [SerializeField]
    public bool usePopup;

    [SerializeField]
    public Vector3 pos;

    [SerializeField]
    public ConversationInfo[] converInfo;
}

[System.Serializable]
public struct SceneData
{
    [SerializeField]
    public TransitionEffect transitionEffect;

    [SerializeField]
    public ZoomEffect zoomEffect;

    [SerializeField]
    public BackgroundInfo bgInfo;

    [SerializeField]
    public CharacterInfo charInfo;

    [SerializeField]
    public PopupInfo popupInfo;
}

[System.Serializable]
public struct SceneElement
{
    [SerializeField]
    public SpriteRenderer bgRender;

    [SerializeField]
    public SpriteRenderer charRender;
}

public class CameraTransition : MonoBehaviour {
    [Header("Reference Objects")]
    [SerializeField]
    private SceneData[] sceneData;

    [SerializeField]
    private SceneElement[] sceneElement;

    [SerializeField]
    private GameObject popupObject;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private Text popupText;

    [SerializeField]
    private Image fadableImg;

    [SerializeField]
    private Image topImg;

    [SerializeField]
    private Image botImg;

    [SerializeField]
    private Text endText_1, endText_2;

    [Header("Config")]
    [SerializeField]
    private bool useFadePopup;

    [SerializeField]
    private AnimationCurve moveVerCurve;

    [SerializeField]
    private ConversationInfo[] conversationInfo;

    private CanvasGroup popupGroup;
    private Vector2 cameraSize;
    private float bgWidth;
    private float distance;
    int curIndex = 0;
    int nextIndex = 1;
    bool isNext = true;


    private void Awake()
    {
        cameraSize.x = Camera.main.orthographicSize * (float)Screen.width / Screen.height;
        cameraSize.y = Camera.main.orthographicSize;
        bgWidth = 20.5f / 2;
        distance = -(bgWidth - cameraSize.x);
        curIndex = 0;
        nextIndex = 1;
        popupGroup = popupObject.GetComponent<CanvasGroup>();
        topImg.rectTransform.sizeDelta = botImg.rectTransform.sizeDelta = new Vector2(topImg.rectTransform.sizeDelta.x, 300);
    }

    private void Start()
    {
        if (!useFadePopup)
            popupObject.transform.localScale = Vector3.zero;
        else
            popupGroup.alpha = 0;
        StartCoroutine(MoveBackground());

        if(SoundManager.Instance != null)
        {
            SoundManager.Instance.StopMenuBackgroundMusic();
        }
    }

    IEnumerator FadeImg(Image img, float fadeTime, bool isFadeIn)
    {
        float value = 0;
        float speed = 1 / fadeTime;
        Color startColor = img.color;
        Color endColor = isFadeIn ? new Color(0, 0, 0, 1) : new Color(0, 0, 0, 0);
        while(value < 1)
        {
            value += Time.deltaTime;
            img.color = Color.Lerp(startColor, endColor, value);
            yield return null;
        }
    }

    IEnumerator ScaleImg(Image img, float scaleTime, bool isScaleUp)
    {
        float value = 0;
        float speed = 1 / scaleTime;
        Vector2 startScale = img.rectTransform.sizeDelta;
        Vector2 endScale = isScaleUp ? new Vector2(img.rectTransform.sizeDelta.x, 300) : new Vector2(img.rectTransform.sizeDelta.x, 150);
        while (value < 1)
        {
            value += Time.deltaTime;
            img.rectTransform.sizeDelta = Vector3.Lerp(startScale, endScale, moveVerCurve.Evaluate(value));
            yield return null;
        }
    }

    IEnumerator CR_ShowConversation(PopupInfo popupInfo)
    {
        if (!useFadePopup)
        {
            popupObject.transform.localPosition = popupInfo.pos;
            popupObject.transform.localScale = Vector3.zero;
            for (int i = 0; i < popupInfo.converInfo.Length; i++)
            {
                yield return new WaitForSeconds(popupInfo.converInfo[i].delaytime);
                popupText.text = popupInfo.converInfo[i].Text;
                float value = 0;
                float speed = 1 / 0.2f;
                Vector3 startScale = popupObject.transform.localScale;
                Vector3 endScale = Vector3.one;
                while (value < 1)
                {
                    value += Time.deltaTime * speed;
                    popupObject.transform.localScale = Vector3.Lerp(startScale, endScale, value);
                    yield return null;
                }
                yield return new WaitForSeconds(popupInfo.converInfo[i].duration);
                value = 0;
                startScale = popupObject.transform.localScale;
                endScale = Vector3.zero;
                while (value < 1)
                {
                    value += Time.deltaTime * speed;
                    popupObject.transform.localScale = Vector3.Lerp(startScale, endScale, value);
                    yield return null;
                }
            }
        }
        else
        {
            popupObject.transform.localPosition = popupInfo.pos;
            popupGroup.alpha = 0;
            for (int i = 0; i < popupInfo.converInfo.Length; i++)
            {
                yield return new WaitForSeconds(popupInfo.converInfo[i].delaytime);
                popupText.text = popupInfo.converInfo[i].Text;
                float value = 0;
                float speed = 1 / 0.5f;
                float startAlpha = popupGroup.alpha;
                float endAlpha = 1;
                while (value < 1)
                {
                    value += Time.deltaTime * speed;
                    popupGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, value);
                    yield return null;
                }
                yield return new WaitForSeconds(popupInfo.converInfo[i].duration);
                value = 0;
                startAlpha = popupGroup.alpha;
                endAlpha = 0;
                while (value < 1)
                {
                    value += Time.deltaTime * speed;
                    popupGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, value);
                    yield return null;
                }
            }
        }
    }

    IEnumerator CR_ShowConversation(ConversationInfo[] converInfo)
    {
        popupGroup.alpha = 0;
        for (int i = 0; i < converInfo.Length; i++)
        {
            yield return new WaitForSeconds(converInfo[i].delaytime);
            popupText.text = converInfo[i].Text;
            float value = 0;
            float speed = 1 / 0.6f;
            float startAlpha = popupGroup.alpha;
            float endAlpha = 1;
            while (value < 1)
            {
                value += Time.deltaTime * speed;
                popupGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, value);
                yield return null;
            }
            yield return new WaitForSeconds(converInfo[i].duration);
            value = 0;
            startAlpha = popupGroup.alpha;
            endAlpha = 0;
            while (value < 1)
            {
                value += Time.deltaTime * speed;
                popupGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, value);
                yield return null;
            }
        }
    }

    IEnumerator FadeText(Text text, float fadeTime, bool isFadeIn)
    {
        float value = 0;
        float speed = 1 / fadeTime;
        Color startColor = text.color;
        Color endColor = isFadeIn ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);
        while (value < 1)
        {
            value += Time.deltaTime;
            text.color = Color.Lerp(startColor, endColor, value);
            yield return null;
        }
    }

    IEnumerator MoveBackground()
    {
        SceneData curSceneData, nextSceneData;
        curSceneData = sceneData[0];
        nextSceneData = sceneData[1];
        sceneElement[curIndex].bgRender.transform.position = curSceneData.bgInfo.startPos;
        sceneElement[curIndex].charRender.sprite = curSceneData.charInfo.sprite;
        sceneElement[curIndex].charRender.transform.localPosition = curSceneData.charInfo.pos;
        sceneElement[curIndex].charRender.transform.localScale = curSceneData.charInfo.scale;
        sceneElement[curIndex].charRender.sortingOrder = 1;
        sceneElement[nextIndex].bgRender.transform.position = GetHidePos(nextSceneData.bgInfo, 
            nextSceneData.transitionEffect.moveEffect);
        if (nextSceneData.transitionEffect.useFadeEffect)
        {
            sceneElement[nextIndex].bgRender.color = new Color(1, 1, 1, 0);
            sceneElement[nextIndex].charRender.color = new Color(1, 1, 1, 0);
        }
        sceneElement[nextIndex].charRender.sprite = nextSceneData.charInfo.sprite;
        sceneElement[nextIndex].charRender.transform.localPosition = nextSceneData.charInfo.pos;
        sceneElement[nextIndex].charRender.transform.localScale = nextSceneData.charInfo.scale;
        yield return new WaitForSeconds(1);
        StartCoroutine(CR_FadeMusic(1.5f, true));
        StartCoroutine(ScaleImg(topImg, 1.5f, false));
        yield return StartCoroutine(ScaleImg(botImg, 1.5f, false));
        //yield return StartCoroutine(FadeImg(fadableImg, 1, false));
        StartCoroutine(CR_ShowConversation(conversationInfo));
        for (int i = 2; i < sceneData.Length + 2; i++)
        {
            float value = 0;
            float speed = 1 / curSceneData.bgInfo.timeMove;
            Vector3 startPos = curSceneData.bgInfo.startPos;
            Vector3 endPos = curSceneData.bgInfo.endPos;
            sceneElement[curIndex].bgRender.sortingOrder = 0;
            sceneElement[curIndex].charRender.sortingOrder = 1;
            sceneElement[nextIndex].bgRender.sortingOrder = -2;
            sceneElement[nextIndex].charRender.sortingOrder = -1;
            float curCamSize = Camera.main.orthographicSize;

            float valueZoom = 0;
            float speedZoom = 1 / curSceneData.zoomEffect.duration;
            float t = 0;
            if (curSceneData.popupInfo.usePopup)
            {
                popupObject.transform.parent = sceneElement[curIndex].bgRender.transform;
                StartCoroutine(CR_ShowConversation(curSceneData.popupInfo));
            }
            while (value < 1)
            {
                value += Time.deltaTime * speed;
                t += Time.deltaTime;
                sceneElement[curIndex].bgRender.transform.position = Vector3.Lerp(startPos, 
                    endPos, curSceneData.bgInfo.curveAniMove.Evaluate(value));
                if(curSceneData.zoomEffect.useZoom && t >= curSceneData.zoomEffect.startTime)
                {
                    valueZoom += Time.deltaTime * speedZoom;
                    Camera.main.orthographicSize = Mathf.Lerp(curCamSize, curSceneData.zoomEffect.targetorthoSize, valueZoom);
                }
                yield return null;
            }

            value = 0;
            speed = 1 / nextSceneData.transitionEffect.timeTrans;
            startPos = sceneElement[nextIndex].bgRender.transform.position;
            endPos = nextSceneData.bgInfo.startPos;
            if (!nextSceneData.transitionEffect.useFadeEffect)
            {
                sceneElement[nextIndex].bgRender.sortingOrder = 2;
                sceneElement[nextIndex].charRender.sortingOrder = 3;
            }
            Color curSceneStartColor, curSceneEndColor, nextSceneStartColor, nextSceneEndColor;
            curSceneStartColor = sceneElement[curIndex].bgRender.color;
            curSceneEndColor = new Color(1, 1, 1, 0);
            nextSceneStartColor = sceneElement[nextIndex].bgRender.color;
            nextSceneEndColor = new Color(1, 1, 1, 1);


            if (i < sceneData.Length + 1)
            {
                while (value < 1)
                {
                    value += Time.deltaTime * speed;
                    sceneElement[nextIndex].bgRender.transform.position = Vector3.Lerp(startPos, 
                        endPos, moveVerCurve.Evaluate(value));

                    if(nextSceneData.transitionEffect.useFadeEffect)
                    {
                        sceneElement[curIndex].bgRender.color = Color.Lerp(curSceneStartColor, curSceneEndColor, value);
                        sceneElement[curIndex].charRender.color = Color.Lerp(curSceneStartColor, curSceneEndColor, value);
                        sceneElement[nextIndex].bgRender.color = Color.Lerp(nextSceneStartColor, nextSceneEndColor, value);
                        sceneElement[nextIndex].charRender.color = Color.Lerp(nextSceneStartColor, nextSceneEndColor, value);
                    }
                    yield return null;
                }
            }
            int temp = nextIndex;
            nextIndex = curIndex;
            curIndex = temp;

            if (i < sceneData.Length + 1)
            {
                curSceneData = nextSceneData;
                if (i < sceneData.Length)
                {
                    nextSceneData = sceneData[i];
                    sceneElement[nextIndex].bgRender.sprite = sceneData[i].bgInfo.sprite;
                }
                sceneElement[nextIndex].charRender.sprite = nextSceneData.charInfo.sprite;
                sceneElement[nextIndex].charRender.transform.localPosition = nextSceneData.charInfo.pos;
                sceneElement[nextIndex].charRender.transform.localScale = nextSceneData.charInfo.scale;
                sceneElement[nextIndex].bgRender.transform.position = GetHidePos(nextSceneData.bgInfo,
                    nextSceneData.transitionEffect.moveEffect);
                if (nextSceneData.transitionEffect.useFadeEffect)
                {
                    sceneElement[nextIndex].bgRender.color = new Color(1, 1, 1, 0);
                    sceneElement[nextIndex].charRender.color = new Color(1, 1, 1, 0);
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
        //yield return StartCoroutine(FadeImg(fadableImg, 2, true));
        StartCoroutine(CR_FadeMusic(2.5f, false));
        StartCoroutine(ScaleImg(topImg, 1.5f, true));
        yield return StartCoroutine(ScaleImg(botImg, 1.5f, true));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeText(endText_1, 0.75f, true));
        yield return StartCoroutine(FadeText(endText_1, 0.75f, false));
        if (SceneLoadingManager.Instance != null)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayMenuBackgroundMusic();
            SceneLoadingManager.Instance.LoadMainScene();
        }
    }

    IEnumerator CR_FadeMusic(float timeFade, bool isFadeIn)
    {
        if (isFadeIn)
        {
            audioSource.volume = 0;
            audioSource.Play();
        }

        float startVol = isFadeIn ? 0 : 1;
        float endVol = isFadeIn ? 1 : 0;
        float speed = timeFade != 0 ? 1 / timeFade : 1000;
        float value = 0;
        while(value < 1)
        {
            value += Time.deltaTime * speed;
            audioSource.volume = Mathf.Lerp(startVol, endVol, value);
            yield return null;
        }
        if (!isFadeIn)
            audioSource.Stop();
    }

    private Vector3 GetHidePos(BackgroundInfo bgInfo, MoveType type)
    {
        switch(type)
        {
            case MoveType.Bot:
                return new Vector3(bgInfo.startPos.x, -bgWidth - cameraSize.y, 0);
            case MoveType.Top:
                return new Vector3(bgInfo.startPos.x, bgWidth + cameraSize.y, 0);
            case MoveType.Left:
                return new Vector3(-bgWidth - cameraSize.x, bgInfo.startPos.y, 0);
            case MoveType.Right:
                return new Vector3(bgWidth + cameraSize.x, bgInfo.startPos.y, 0);
            case MoveType.None:
                return bgInfo.startPos;
            default:
                return bgInfo.startPos;    
        }
    }

    public void Reload()
    {
        SceneManager.LoadScene("EndingScene");
    }

}
