using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScriptAnimationManager : MonoBehaviour {
    [HideInInspector]
    public UIInOutAnim[] animationList;
    // Use this for initialization
    private bool isShown = false;
	void Awake () {
        animationList = GetComponents<UIInOutAnim>();
	}
    public void FadeIn(float duration)
    {
        if (isShown)
            return;
        isShown = true;
        foreach (var item in animationList)
        {
            item.FadeIn(duration);
        }
    }
    public void FadeOut(float duration)
    {
        if (!isShown)
            return;
        isShown = false;
        foreach (var item in animationList)
        {
            item.FadeOut(duration);
        }
    }
}
