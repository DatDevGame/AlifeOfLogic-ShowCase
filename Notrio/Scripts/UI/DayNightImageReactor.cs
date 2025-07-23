using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;
using System;

[RequireComponent(typeof(Image))]
public class DayNightImageReactor : MonoBehaviour {
    public Sprite daySprite;
    public Sprite nightSprite;
    [HideInInspector]
    public Image target;

    // Use this for initialization
    void Start () {
        target = GetComponent<Image>();
        Adapt();
        PersonalizeManager.onNightModeChanged += OnNightModeChanged;
	}

    private void OnDestroy()
    {
        PersonalizeManager.onNightModeChanged -= OnNightModeChanged;
    }

    private void OnNightModeChanged(bool obj)
    {
        Adapt();
    }

    private void Adapt() {
        Sprite currentSprite = PersonalizeManager.NightModeEnable ? nightSprite : daySprite;
        target.sprite = currentSprite;
    }
}
