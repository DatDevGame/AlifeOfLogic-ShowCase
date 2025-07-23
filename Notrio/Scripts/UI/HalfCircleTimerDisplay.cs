using System.Collections;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;
using UnityEngine.UI;

public class HalfCircleTimerDisplay : MonoBehaviour {

    public Text hourText;
    public Text minuteText;
    public Text secondText;

    [HideInInspector]
    public Timer timer;

    private void Awake()
    {
        if (UIReferences.Instance != null)
        {
            UpdateReferences();
        }
        UIReferences.UiReferencesUpdated += UpdateReferences;
    }

    private void UpdateReferences()
    {
        timer = UIReferences.Instance.timer;
    }

    private void OnDestroy()
    {
        UIReferences.UiReferencesUpdated -= UpdateReferences;
    }
    private void Reset()
    {
        hourText = GetComponent<Text>();
        minuteText = GetComponent<Text>();
        secondText = GetComponent<Text>();
    }

    private void Update()
    {
        string[] timerString = timer.GetTimeString().Split(':');
        hourText.text = timerString[0];
        minuteText.text = timerString[1];
        secondText.text = timerString[2];
    }
}
