using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;
using System;
using Pinwheel;
[RequireComponent(typeof(PositionAnimation))]
public class PackGroupController : MonoBehaviour {
    private PositionAnimation positionAnimation;
    private bool isHiding =false;
	// Use this for initialization
	void Start () {
        positionAnimation = GetComponent<PositionAnimation>();
        GameManager.GameStateChanged += OnGameStateChanged;
	}
    private void OnDestroy()
    {
        GameManager.GameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newGameState, GameState arg2)
    {
        if (newGameState.Equals(GameState.Prepare))
        {
            if(isHiding)
                Show();
        }
        else
        {
            if(!isHiding)
                Hide();
        }
    }

    private void Show()
    {
        StartCoroutine(DelayShow(1));
        
    }
    private void Hide()
    {
        StartCoroutine(DelayHide(0));
    }
    IEnumerator DelayShow(float duration)
    {
        isHiding = false;
        yield return new WaitForSeconds(duration);
        positionAnimation.Play(positionAnimation.curves[0]);
    }
    IEnumerator DelayHide(float duration)
    {
        isHiding = true;
        yield return new WaitForSeconds(duration);
        positionAnimation.Play(positionAnimation.curves[1]);
    }
}
