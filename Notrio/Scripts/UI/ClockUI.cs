using System.Collections;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;

public class ClockUI : MonoBehaviour {

    public UiGroupController controller;
    public float showDelay;

    private void Awake()
    {
        GameManager.GameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.GameStateChanged -= OnGameStateChanged;
    }

    private void Start()
    {
        controller.ShowIfNot();
    }

    private void OnGameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing)
        {
            controller.HideIfNot();
        }
        else if (newState == GameState.Prepare)
        {
            CoroutineHelper.Instance.DoActionDelay(
                () =>
                {
                    if (GameManager.Instance.GameState == GameState.Prepare)
                        controller.ShowIfNot();
                },
                showDelay);
        }
    }
}
