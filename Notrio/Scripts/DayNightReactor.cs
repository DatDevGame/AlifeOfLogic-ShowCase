using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;

public class DayNightReactor : MonoBehaviour {

    List<Image> images = new List<Image>();
    List<Text> texts = new List<Text>();
    public Color textDayColor = new Color(0.8f, 0.8f, 0.8f, 1);
    public Color textNightColor = new Color(0.207f, 0.282f, 0.384f, 1);
    public Color panelDayColor = new Color(0.207f, 0.282f, 0.384f, 1);
    public Color panelNightColor = new Color(0.8f, 0.8f, 0.8f, 1);

    void Awake()
    {
        PersonalizeManager.onNightModeChanged += OnNightModeChanged;
        images.Add(gameObject.GetComponent<Image>());
        images.AddRange(gameObject.GetComponentsInChildren<Image>());
        texts.Add(gameObject.GetComponent<Text>());
        texts.AddRange(gameObject.GetComponentsInChildren<Text>());
        OnNightModeChanged(false);
    }
    private void OnDestroy()
    {
        PersonalizeManager.onNightModeChanged -= OnNightModeChanged;
    }
    private void OnNightModeChanged(bool enable)
    {
        bool isNightMode = PersonalizeManager.NightModeEnable;
        for (int i = 0; i < images.Count; i++)
        {
            if (images[i])
                images[i].color = isNightMode ? panelNightColor : panelDayColor;
        }
        for (int i = 0; i < texts.Count; i++)
        {
            if (texts[i])
                texts[i].color = isNightMode ? textNightColor : textDayColor;
        }
    }

    private Color GetLerpAlPhaColor(Color c, float x)
    {
        Color newColor = c;
        newColor.a = x;
        return newColor;
    }
}
