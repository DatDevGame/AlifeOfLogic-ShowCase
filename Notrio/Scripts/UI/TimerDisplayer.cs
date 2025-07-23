using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;
using System;

[RequireComponent(typeof(Text))]
public class TimerDisplayer : MonoBehaviour {
	[HideInInspector]
    public Timer timer;
    public Text text;
	private void Awake() {
		if(UIReferences.Instance!=null){
			UpdateReferences();
		}
		UIReferences.UiReferencesUpdated += UpdateReferences;
	}

	private void UpdateReferences()
	{
		timer = UIReferences.Instance.timer;
	}

	private void OnDestroy() {
		UIReferences.UiReferencesUpdated -= UpdateReferences;
	}
    private void Reset()
    {
        text = GetComponent<Text>();
    }

    private void Update()
    {
        text.text = timer.GetTimeString();
    }
}
