using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;

namespace Takuzu
{
    public class SwipeHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public float minPixelPerSecond;
        public Button[] buttons;
        public Action<Vector2> onSwipe = delegate { };

        private Vector2 startPoint;
        private Vector2 delta;
        private float startTime;
        private float t;
        private float v;

        public void OnPointerDown(PointerEventData eventData)
        {
            startPoint = eventData.position;
            startTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            delta = eventData.position - startPoint;
            t = Time.time - startTime;
            v = delta.magnitude / t;
            if (v >= minPixelPerSecond)
            {
                onSwipe(delta);
            }
            else
            {
                for (int i = 0; i < buttons.Length; ++i)
                {
                    RectTransform rt = buttons[i].transform as RectTransform;
                    if (RectTransformUtility.RectangleContainsScreenPoint(rt, eventData.position))
                    {
                        buttons[i].onClick.Invoke();
                    }
                }
            }
        }
    }
}