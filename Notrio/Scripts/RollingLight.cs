using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RollingLight : MonoBehaviour {
    public enum State
    {
        Idle,
        Rolling,
        Winning
    }

    public State RollingState
    {
        get
        {
            return rollingState;
        }

        set
        {
            rollingStateChanged(value, rollingState);
            rollingState = value;
        }
    }

    private State rollingState = State.Idle;
    private Coroutine animationCR;

    public Action<State, State> rollingStateChanged = delegate { };

    [Header("Rolling Lights Config")]
    public float rollingStateDelayTime = 0.5f;
    public float winningStateDelayTime = 0.3f;
    public Sprite lightOnSprite;
    public Sprite lightOffSprite;

    [Header("UI references")]
    public Image[] lights;


    private void Start()
    {
        rollingStateChanged += OnRollingStateChanged;
    }
    private void OnDestroy()
    {
        rollingStateChanged -= OnRollingStateChanged;
    }

    private void OnRollingStateChanged(State newVal, State oldVal)
    {
        if (animationCR != null)
            StopCoroutine(animationCR);
        switch (newVal)
        {
            case State.Idle:
                break;
            case State.Rolling:
                animationCR = StartCoroutine(lightRollingCR());
                break;
            case State.Winning:
                animationCR = StartCoroutine(lightWinningCR());
                break;
            default:
                break;
        }
    }

    private IEnumerator lightRollingCR()
    {
        if(lights.Length == 0)
        {
            Debug.Log("No Rolling light");
        }
        else
        {
            int lightOnIndex = 0;
            while (true)
            {
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].sprite = i == lightOnIndex ? lightOnSprite : lightOffSprite;
                }
                lightOnIndex = (lightOnIndex + 1) % lights.Length;
                yield return new WaitForSeconds(rollingStateDelayTime);
            }
        }
    }

    private IEnumerator lightWinningCR()
    {
        if (lights.Length == 0)
        {
            Debug.Log("No Rolling light");
        }
        else
        {
            bool lightOn = true;
            while (true)
            {
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].sprite = lightOn ? lightOnSprite : lightOffSprite;
                }
                lightOn = !lightOn;
                yield return new WaitForSeconds(winningStateDelayTime);
            }
        }
    }
}
