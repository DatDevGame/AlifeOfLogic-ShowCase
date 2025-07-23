using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Takuzu
{
    public class SwipeHandlerHelper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public SwipeHandler handler;

        public void OnPointerDown(PointerEventData eventData)
        {            
            handler.OnPointerDown(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            handler.OnPointerUp(eventData);
        }
    }
}