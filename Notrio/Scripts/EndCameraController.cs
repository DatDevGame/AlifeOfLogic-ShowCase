using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public struct Conversation
{
    [SerializeField]
    public string text;

    [SerializeField]
    public float timeShow;
}

    [System.Serializable]
public struct Picture
{
    [SerializeField]
    public Transform trans;

    [SerializeField]
    public float timeMove;

    [SerializeField]
    public AnimationCurve aniCurve;

    [SerializeField]
    public float timeStay;

    [SerializeField]
    public Conversation[] conversations;
}

public class EndCameraController : MonoBehaviour {

    [SerializeField]
    private Picture[] pictures;

    [SerializeField]
    AnimationCurve swingCurve;

    [SerializeField]
    private float timeZoomOut = 0.8f;

    [SerializeField]
    private float targetOrthoSize = 7;

    [SerializeField]
    private Vector3 targetZoomPos = new Vector3(1.25f, 1.82f, -10f);

    [SerializeField]
    AnimationCurve zoomCurve;

    [SerializeField]
    float swingIndex = 0.4f;

    [SerializeField]
    AnimationCurve endCurve;

    [SerializeField]
    private Text text1, text2;

    [SerializeField]
    private CanvasGroup dialogCanvasGroup, finishGroup;

    [SerializeField]
    private Text dialogText;

    private void Start()
    {
        Application.targetFrameRate = 60;
        StartCoroutine(CR_MoveCamera());
    }

    IEnumerator CR_FadeDialog(string conver,float timeOut)
    {
        float value = 0;
        float speed = 1 / 0.25f;
        dialogText.text = conver;
        while (value < 1)
        {
            value += Time.deltaTime * speed;
            dialogCanvasGroup.alpha = Mathf.Lerp(0, 1, value);
            yield return null;
        }
        yield return new WaitForSeconds(timeOut);
        value = 0;
        while (value < 1)
        {
            value += Time.deltaTime * speed;
            dialogCanvasGroup.alpha = Mathf.Lerp(1, 0, value);
            yield return null;
        }
    }

    IEnumerator RunConversation(Conversation[] conver)
    {
        for(int i = 0; i < conver.Length; i ++)
        {
            yield return StartCoroutine(CR_FadeDialog(conver[i].text, conver[i].timeShow));
        }
    }

    IEnumerator CR_FadeEndText()
    {
        float value = 0;
        float speed = 1 / 0.75f;
        Color startColor = text1.color;
        Color endColor = text1.color;
        endColor.a = 1;
        while (value < 1)
        {
            value += Time.deltaTime * speed;
            text1.color = Color.Lerp(startColor, endColor, value);
            yield return null;
        }

        startColor = text2.color;
        endColor = text2.color;
        endColor.a = 1;
        value = 0;
        while (value < 1)
        {
            value += Time.deltaTime * speed;
            text2.color = Color.Lerp(startColor, endColor, value);
            yield return null;
        }
    }

    IEnumerator CR_Zoom()
    {
        float value = 0;
        float speed = 1 / timeZoomOut;
        float curSize = Camera.main.orthographicSize;
        Vector3 curPos = transform.position;
        while(value < 1)
        {
            value += Time.deltaTime * speed;
            Camera.main.orthographicSize = Mathf.Lerp(curSize, targetOrthoSize, zoomCurve.Evaluate(value));
            transform.position = Vector3.Lerp(curPos, targetZoomPos, zoomCurve.Evaluate(value));
            yield return null;
        }
    }
    IEnumerator CR_SwingCamera()
    {
        Vector3 centerPos = transform.position;
        Vector3 topPos = centerPos;
        Vector3 botPos = centerPos;
        topPos.y = centerPos.y + swingIndex;
        botPos.y = centerPos.y - swingIndex;
        int count = 5;
        float subValue = swingIndex / count;
        bool isMoveUp = true;
        while(count > 0)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = isMoveUp ? topPos: botPos;
            endPos.y = isMoveUp ? endPos.y - (swingIndex - count * subValue) : endPos.y + (swingIndex - count * subValue);
            float value = 0;
            float speed = 1;
            while(value < 1)
            {
                value += Time.deltaTime * speed;
                transform.position = Vector3.Lerp(startPos, endPos, swingCurve.Evaluate(value));
                yield return null;
            }
            count--;
            isMoveUp = !isMoveUp;
            yield return null;
        }

        float value1 = 0;
        float speed1 = 1;
        Vector3 startPos1 = transform.position;
        while (value1 < 1)
        {
            value1 += Time.deltaTime * speed1;
            transform.position = Vector3.Lerp(startPos1, centerPos, swingCurve.Evaluate(value1));
            yield return null;
        }

        yield return new WaitForSeconds(1);
    }

    IEnumerator CR_MoveCamera()
    {
        yield return new WaitForSeconds(2);
        Conversation[] conver = new Conversation[2];
        conver[0].text = "Finally, after all this time";
        conver[0].timeShow = 2;
        conver[1].text = "I have completed my adventure!";
        conver[1].timeShow = 2;
        StartCoroutine(RunConversation(conver));
        yield return StartCoroutine(CR_SwingCamera());
        bool isGoIn = true;
        bool isCheck = false;
        Vector3 prePos = pictures[0].trans.position;
        for (int i = 0; i < pictures.Length; i++)
        {
            float value = 0;
            Vector3 startPos = transform.position;
            Vector3 endPos = pictures[i].trans.position;
            float speed = 2;
            if (i != 0)
                isGoIn = false;
            //Debug.Log(Vector3.Distance(transform.position, endPos));
            while (Vector3.Distance(transform.position, endPos) != 0)
            {
                if(isGoIn)
                {
                    if (Vector3.Distance(transform.position, endPos) < 1.5f)
                    {
                        speed = Mathf.Clamp(speed - Time.deltaTime * 1.5f, 0.5f, 2);
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, prePos) > 1.5f && Vector3.Distance(transform.position, prePos) < 3)
                    {
                        speed = Mathf.Clamp(speed + Time.deltaTime * 1.5f, 0.5f, 2);
                        isCheck = true;
                    }
                    else
                    {
                        if (isCheck)
                        {
                            isGoIn = true;
                            isCheck = false;
                        }
                    }
                }
                value += Time.deltaTime * speed;
                transform.position = Vector3.MoveTowards(transform.position, endPos, Time.deltaTime * speed);
                yield return null;
            }
            prePos = endPos;
        }

        yield return new WaitForSeconds(1);
        for (int i = pictures.Length - 2; i >= 0;i--)
        {
            float value = 0;
            float speed = 1 / pictures[i].timeMove;
            Vector3 startPos = transform.position;
            Vector3 endPos = pictures[i].trans.position;
            while (value < 1)
            {
                value += Time.deltaTime * speed;
                transform.position = Vector3.Lerp(startPos, endPos, pictures[i].aniCurve.Evaluate(value));
                yield return null;
            }
            StartCoroutine(RunConversation(pictures[i].conversations));
            yield return new WaitForSeconds(pictures[i].timeStay);
        }
        yield return StartCoroutine(CR_Zoom());
        yield return StartCoroutine(CR_FadeEndText());
        StartCoroutine(CR_FadeButton());
    }

    IEnumerator CR_FadeButton()
    {
        float value = 0;
        while (value < 1)
        {
            value += Time.deltaTime * 2;
            finishGroup.alpha = Mathf.Lerp(0, 1, value);
            yield return null;
        }
    }

    public void Reload()
    {
        SceneManager.LoadScene("SampleScene");
    }

}
