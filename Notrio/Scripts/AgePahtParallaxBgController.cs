using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;
using System;

public class AgePahtParallaxBgController : MonoBehaviour
{
    public SnappingScroller scroller;
    public RectTransform BgContent;
    public List<Image> bgImages;

    public Color[] colors = new Color[5];
    public float currentBgposition {
        get {
                float pos = -(BgContent.anchoredPosition.x / BgContent.rect.width);
				if (float.IsNaN (pos))
					return 0;
				return pos;
            }
    }

    private void Start()
    {
        UpdateBgPosition();
    }

    private void UpdateBgPosition()
    {
        BgContent.anchoredPosition = new Vector2(-scroller.RelativeNormalizedScrollPos*BgContent.rect.width , BgContent.anchoredPosition.y);

        float color1Position = Mathf.Clamp(scroller.RelativeNormalizedScrollPos*5, 0 , 4);
        float color2Position = Mathf.Clamp(color1Position + 1, 0, 4);

        Color accentColor1 = colors[(int)color1Position];
        Color accentColor2 = colors[(int)color2Position];

        Color c = Color.Lerp(accentColor1,accentColor2, color1Position - ((int) color1Position));
        foreach (var img in bgImages)
        {
            img.color = c;
        }
    }
    void Update()
    {
        if(currentBgposition != scroller.RelativeNormalizedScrollPos)
        {
            UpdateBgPosition();
        }
    }
}
