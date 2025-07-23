using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Takuzu
{
    public class SimpleVerticalScroller : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action onReachEndOfList = delegate { };

        public RectTransform content;
        public float sensitive;
        public float maxVoidOffset;
        public float snapSpeed;
        public float endDragDeceleration;
        public bool sendDragEventToParent;

        public int ElementCount
        {
            get
            {
                return content.childCount;
            }
        }
#pragma warning disable 0414
        bool isDragging;
        float remainVelocity;
#pragma warning restore 0414
#pragma warning disable 0649
        float voidOffset;
#pragma warning restore 0649

        RectTransform selfRt;
        float canvasRefResolutionY;

        float lastVoidOffset;

        float dragBeginTime;
        //[SerializeField]
        Rect visibleRect;

        IBeginDragHandler[] parentBeginDragHandler;
        IDragHandler[] parentDragHandler;
        IEndDragHandler[] parentEndDragHandler;

        private void Awake()
        {
            selfRt = GetComponent<RectTransform>();
            canvasRefResolutionY = selfRt.GetComponentInParent<CanvasScaler>().referenceResolution.y;
            parentBeginDragHandler = transform.parent.GetComponentsInParent<IBeginDragHandler>();
            parentDragHandler = transform.parent.GetComponentsInParent<IDragHandler>();
            parentEndDragHandler = transform.parent.GetComponentsInParent<IEndDragHandler>();
        }

        private void Start()
        {
            content.anchorMax = new Vector2(1, 1);
            content.anchorMin = new Vector2(0, 1);
            content.pivot = new Vector2(0, 1);
            content.anchoredPosition = new Vector2(0, 0);
        }

        public void OnBeginDrag(PointerEventData data)
        {
            isDragging = true;
            dragBeginTime = Time.time;
            if (sendDragEventToParent)
            {
                SendBeginDragEventToParent(data);
            }
        }

        public void OnDrag(PointerEventData data)
        {
            float deceleration = 1;
            if (maxVoidOffset == 0)
            {
                if (voidOffset > maxVoidOffset)
                    deceleration = 0;
            }
            else
            {
                deceleration = Mathf.Clamp01(1 - voidOffset / maxVoidOffset);
            }
            float delta = data.delta.y * canvasRefResolutionY / Screen.height;
            content.anchoredPosition = content.anchoredPosition + (delta * sensitive * deceleration * Vector2.up);

            if (sendDragEventToParent)
            {
                SendDragEventToParent(data);
            }
        }

        public void OnEndDrag(PointerEventData data)
        {
            isDragging = false;
            if (sendDragEventToParent)
            {
                SendEndDragEventToParent(data);
            }
            float dragDuration = Time.time - dragBeginTime;
            remainVelocity = data.delta.y / dragDuration;
        }

        private void Update()
        {
            Rect visibleRect = selfRt.rect;

        }

        private void SendBeginDragEventToParent(PointerEventData data)
        {
            for (int i = 0; i < parentBeginDragHandler.Length; ++i)
            {
                parentBeginDragHandler[i].OnBeginDrag(data);
            }
        }

        private void SendDragEventToParent(PointerEventData data)
        {
            for (int i = 0; i < parentDragHandler.Length; ++i)
            {
                parentDragHandler[i].OnDrag(data);
            }
        }

        private void SendEndDragEventToParent(PointerEventData data)
        {
            for (int i = 0; i < parentEndDragHandler.Length; ++i)
            {
                parentEndDragHandler[i].OnEndDrag(data);
            }
        }

        public void ResetScrollPos()
        {
            content.anchoredPosition = Vector2.zero;
        }
    }
}