using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;

public class ClockController : MonoBehaviour {

	public List<TimerCharacter> timerCharacterList;

    [SerializeField]
    private bool IsCountEndOfDay = true;

    void Update ()
    {
        if (IsCountEndOfDay)
            UpdateTimeLeft();
    }

    public void UpdateTime(TimeSpan t)
    {
        UpdateDiggits(t);
    }

    public void ResetTimeUI()
    {
        for (int i = 0; i < timerCharacterList.Count; i += 2)
        {
            if (i + 1 > timerCharacterList.Count || i + 2 > timerCharacterList.Count)
                return;
            timerCharacterList[i + 1].text1.text = "0";
            timerCharacterList[i + 1].text2.text = "0";
            timerCharacterList[i].text1.text = "0";
            timerCharacterList[i].text2.text = "0";
        }
    }

    private void UpdateTimeLeft()
    {
        UpdateDiggits(PuzzleManager.Instance.timeToEndOfTheDay);
    }

    private void UpdateDiggits(TimeSpan t)
    {
		for (int i = 0; i < timerCharacterList.Count; i+=2)
        {
			if (i + 1 > timerCharacterList.Count || i + 2 > timerCharacterList.Count)
                return;
            int time = t.Days;
            switch (i/2)
            {
                case 3:
                    time = t.Days;
                    break;
                case 2:
                    time = t.Hours;
                    break;
                case 1:
                    time = t.Minutes;
                    break;
                case 0:
                    time = t.Seconds;
                    break;
                default:
                    break;
            }
			timerCharacterList[i + 1].SetNewText((time / 10).ToString());
			timerCharacterList[i].SetNewText((time % 10).ToString());
        }
    }
}
