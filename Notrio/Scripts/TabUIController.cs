using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabUIController : MonoBehaviour {
    public List<Button> buttons;

    public Color iconActiveColor;
    public Color iconInActiveColor;
    public Color bgActiveColor;
    public Color bgInActiveColor;

    public Image[] icons;
    public Image[] bgs;

    // Use this for initialization
    void Start () {
        foreach (var button in buttons)
        {
            button.onClick.AddListener(delegate
            {
                SetAciveButton(button);
            });
        }
	}

    private void SetAciveButton(Button btn)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            icons[i].color = iconInActiveColor;
            bgs[i].color = bgInActiveColor;
        }
        icons[buttons.IndexOf(btn)].color = iconActiveColor;
        bgs[buttons.IndexOf(btn)].color = bgActiveColor;
    }
}
