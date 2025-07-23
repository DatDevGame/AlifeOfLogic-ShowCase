using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public static float msec;
    public static float fps;
    public static float avgFps
    {
        get
        {
            return (minFps + maxFps) / 2;
        }
    }

    public static float maxFps = Mathf.NegativeInfinity;
    public static float minFps = Mathf.Infinity;
    public bool showFPS;
    public Text text;
    float deltaTime = 0.0f;

    public void Awake()
    {
        StartCoroutine(ResetCounter(3));
    }

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        msec = deltaTime * 1000.0f;
        fps = 1.0f / deltaTime;

        minFps = fps < minFps ? fps : minFps;
        maxFps = fps > maxFps ? fps : maxFps;

        if (text != null)
        {
            if (showFPS)
                text.text = ((int)fps).ToString();
            else
                text.gameObject.SetActive(false);
        }
    }

    public IEnumerator ResetCounter(float period)
    {
        while (true)
        {
            maxFps = Mathf.NegativeInfinity;
            minFps = Mathf.Infinity;
            yield return new WaitForSeconds(period);
        }

    }
}