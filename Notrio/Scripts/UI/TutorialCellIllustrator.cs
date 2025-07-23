using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class TutorialCellIllustrator : MonoBehaviour
    {
        public RectTransform rt;
        public Image img;
        public Image digitImg;
        public Color initColor;
        public float colorTweenSpeed;
        public Vector2 initPos;
        public float movementSpeed;

        public Color targetColor;
        public Vector2 targetPos;

        private void Reset()
        {
            rt = GetComponent<RectTransform>();
            img = GetComponent<Image>();

            if (img != null)
                initColor = img.color;

            if (rt != null)
                initPos = rt.anchoredPosition;

            targetColor = initColor;
            targetPos = initPos;
            
            if (transform.childCount>0)
            {
                Transform child = transform.GetChild(0);
                if (child!=null)
                {
                    digitImg = child.GetComponent<Image>();
                }
            }

            colorTweenSpeed = 1;
            movementSpeed = 500;
        }

        private void Awake()
        {
            if (img != null)
                initColor = img.color;

            if (rt != null)
                initPos = rt.anchoredPosition;

            targetColor = initColor;
            targetPos = initPos;
        }

        private void Update()
        {
            if (img)
                img.color = Vector4.MoveTowards(img.color, targetColor, colorTweenSpeed * Time.smoothDeltaTime);

            if (digitImg)
            {
                Color c = digitImg.color;
                c.a = img.color.a;
                digitImg.color = Vector4.MoveTowards(digitImg.color, c, colorTweenSpeed * Time.smoothDeltaTime);
            }

            if (rt)
                rt.anchoredPosition = Vector2.MoveTowards(rt.anchoredPosition, targetPos, movementSpeed * Time.smoothDeltaTime);
        }
    }
}
