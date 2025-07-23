using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class LeftRightToggle : Toggle
    {
        public float movementSpeed;
        public Color onColor;
        public Color offColor;
        public Image background;
        public float colorTweenSpeed;
        public Color backgroundInactiveColor;
        public Color handleActiveColor;
        public Color handleInActiveColor;

        private void Update()
        {
            if (targetGraphic != null)
            {
                Vector2 anchorMin = new Vector2(0, 0.5f);
                Vector2 anchorMax = new Vector2(0, 0.5f);
                Vector2 pivot = new Vector2(0, 0.5f);
                targetGraphic.rectTransform.anchorMin = anchorMin;
                targetGraphic.rectTransform.anchorMax = anchorMax;
                targetGraphic.rectTransform.pivot = pivot;
                Vector2 targetPos;
                if (isOn)
                {
                    RectTransform parentRt = targetGraphic.rectTransform.parent as RectTransform;
                    float parentWidth = parentRt.rect.width;
                    float selfWidth = targetGraphic.rectTransform.rect.width;
                    float newX = parentWidth - selfWidth;
                    targetPos = new Vector2(newX, 0);
                }
                else
                {
                    targetPos = Vector2.zero;
                }
                targetGraphic.rectTransform.anchoredPosition = Vector2.MoveTowards(targetGraphic.rectTransform.anchoredPosition, targetPos, movementSpeed * Time.deltaTime);
            }
            if (background != null)
            {
                if (interactable)
                    background.color = Vector4.MoveTowards(background.color, isOn ? onColor : offColor, colorTweenSpeed * Time.deltaTime);
                else
                    background.color = Vector4.MoveTowards(background.color, backgroundInactiveColor, colorTweenSpeed * Time.deltaTime);
            }
            if (targetGraphic != null)
            {
                if (interactable)
                    targetGraphic.color = Vector4.MoveTowards(targetGraphic.color, handleActiveColor, colorTweenSpeed * Time.deltaTime);
                else
                    targetGraphic.color = Vector4.MoveTowards(targetGraphic.color, handleInActiveColor, colorTweenSpeed * Time.deltaTime);
            }
        }
    }
}