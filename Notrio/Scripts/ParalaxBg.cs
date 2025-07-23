using System.Collections;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;

public class ParalaxBg : MonoBehaviour {
    public List<Transform> paralaxImgs = new List<Transform>();
    public int numberOfBgs = 5;
    private float progress = 0;
    public float Progress { set { progress = value; UpdateParalaxBgs(); } get { return progress; } }

    private List<SpriteRenderer> nearBgRender;
    private List<SpriteRenderer> farBgRender;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        nearBgRender = new List<SpriteRenderer>();
        farBgRender = new List<SpriteRenderer>();

        for (int j = 0; j < paralaxImgs[0].childCount; j++)
        {
            farBgRender.Add(paralaxImgs[0].GetChild(j).GetComponent<SpriteRenderer>());
        }

        for (int j = 0; j < paralaxImgs[1].childCount; j++)
        {
            nearBgRender.Add(paralaxImgs[1].GetChild(j).GetComponent<SpriteRenderer>());
        }
    }

    private void OnEnable()
    {
        GameManager.GameStateChanged += OnGameStateChange;
    }

    private void OnDisable()
    {
        GameManager.GameStateChanged -= OnGameStateChange;
    }

    private void OnGameStateChange(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing && oldState == GameState.Prepare)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(CR_FadeBackground(false));
        }

        if (newState == GameState.Prepare)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(CR_FadeBackground(true));
        }
    }

    IEnumerator CR_FadeBackground(bool isFadeIn)
    {
        SpriteRenderer farSprite = FindBackGroundDisplaying(farBgRender);
        SpriteRenderer nearSprite = FindBackGroundDisplaying(nearBgRender);
        float value = 0;
        Color startColor = farSprite.color;
        Color endColor = isFadeIn ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);
        while (value < 1)
        {
            value += Time.deltaTime;
            nearSprite.color = farSprite.color = Color.Lerp(startColor, endColor, value);
            yield return null;
        }
    }

    private SpriteRenderer FindBackGroundDisplaying(List<SpriteRenderer> list)
    {
        if (list.Count != 0)
        {
            SpriteRenderer spriteRender = list[0];
            float minX = Mathf.Abs(Camera.main.transform.position.x - spriteRender.transform.position.x);
            for (int i = 1; i < list.Count; i++)
            {
                float value = Mathf.Abs(Camera.main.transform.position.x - list[i].transform.position.x);
                if (value < minX)
                {
                    spriteRender = list[i];
                    minX = value;
                }
            }
            return spriteRender;
        }
        else
            return null;
    }
    public void Init()
    {
        for (int i = 0; i < paralaxImgs.Count; i++)
        {
            float bgW = paralaxImgs[i].GetChild(0).GetComponent<SpriteRenderer>().sprite.bounds.size.x * paralaxImgs[i].lossyScale.x - 0.05f;
            for (int j = 0; j < paralaxImgs[i].childCount; j++)
            {
                paralaxImgs[i].GetChild(j).position += (bgW * j - bgW * numberOfBgs / 2 + bgW / 2) * Vector3.right;
            }
        }
    }

    private void UpdateParalaxBgs()
    {
        for (int i = 0; i < paralaxImgs.Count; i++)
        {
            float bgW = paralaxImgs[i].GetChild(0).GetComponent<SpriteRenderer>().sprite.bounds.size.x * paralaxImgs[i].lossyScale.x - 0.05f;
            paralaxImgs[i].transform.position = new Vector3(bgW*numberOfBgs/2 - bgW / 2 - bgW* (numberOfBgs -1)* Progress + transform.position.x, paralaxImgs[i].transform.position.y, paralaxImgs[i].transform.position.z);   
        }
    }
}
