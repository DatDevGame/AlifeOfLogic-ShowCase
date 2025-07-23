using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Takuzu
{
    public class ListView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action onStopScrolling = delegate { };
        public Action onReachLastElement = delegate { };
        public Action<GameObject, object> displayDataAction = delegate { };
        public GameObject elementTemplate;
        public float elementHeight;
        [Range(0f, 1f)]
        public float decelerationRate;
        public bool sendEventDataToParent;

        private int elementCount;
        private float maxScrollDelta;
        private List<object> data;
        private List<RectTransform> elements;
        private RectTransform selfRt;
        private bool isDragging;
        private float canvasRefResolutionY;

        //[HideInInspector]
        public int fromIndex = -1;
        //[HideInInspector]
        public int toIndex = -1;
        private float delta;

        IBeginDragHandler[] parentBeginDragHandler;
        IDragHandler[] parentDragHandler;
        IEndDragHandler[] parentEndDragHandler;

        public bool initialized;

        public int DataCount
        {
            get
            {
                return data != null ? data.Count : 0;
            }
        }

        public List<object> Data
        {
            get
            {
                if (data == null)
                    data = new List<object>();
                return data;
            }
        }

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            if (initialized)
                return;
            selfRt = transform as RectTransform;
            canvasRefResolutionY = selfRt.GetComponentInParent<CanvasScaler>().referenceResolution.y;
            elementCount = 2 + (int)(selfRt.rect.height / elementHeight);
            data = new List<object>();
            elements = new List<RectTransform>();
            //fromIndex = -1;
            //toIndex = -1;
            maxScrollDelta = elementHeight;

            parentBeginDragHandler = transform.parent.GetComponentsInParent<IBeginDragHandler>();
            parentDragHandler = transform.parent.GetComponentsInParent<IDragHandler>();
            parentEndDragHandler = transform.parent.GetComponentsInParent<IEndDragHandler>();

            for (int i = 0; i < elementCount; ++i)
            {
                GameObject g = Instantiate(elementTemplate);
                g.transform.SetParent(transform, false);
                g.SetActive(false);
                AddElement(g.transform as RectTransform);
            }

            initialized = true;
        }

        [HideInInspector]
        public bool scrolling = true;
        private void Update()
        {
            canvasRefResolutionY = CanvasScalerHelper.currentReferenceResolution.y;
            ActivateElementsHasData();
            DeactivateElementsNoData();
            if (fromIndex != -1 && toIndex != -1)
            {
                for (int i = 0; i < elementCount && i < data.Count; ++i)
                {
                    displayDataAction(elements[i].gameObject, data[i + fromIndex]);
                }
            }

            DragElements(delta);
            RecycleElements(delta);

            if (!isDragging)
            {
                delta = delta * (1 - decelerationRate);
                if (Mathf.Abs(delta) <= maxScrollDelta * 0.025f)
                {
                    delta = 0;
                }
            }


            if (delta > 0)
            {
                if(scrolling == false)
                    scrolling = true;
            }
            else
            {
                if (scrolling == true)
                {
                    onStopScrolling();
                    scrolling = false;
                }
            }
        }

        private void ActivateElementsHasData()
        {
            for (int i = 0; i < data.Count && i < elements.Count; ++i)
            {
                if (!elements[i].gameObject.activeInHierarchy)
                {
                    elements[i].gameObject.SetActive(true);
                    //displayDataAction(elements[i].gameObject, data[i]);
                }
            }
        }

        private void DeactivateElementsNoData()
        {
            for (int i = data.Count; i < elements.Count; ++i)
            {
                if (elements[i].gameObject.activeInHierarchy)
                {
                    elements[i].gameObject.SetActive(false);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            if (sendEventDataToParent)
            {
                SendBeginDragEventToParent(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (data.Count >= elementCount)
            {
                isDragging = true;
                delta = eventData.delta.y * canvasRefResolutionY / Screen.height;
                delta = Mathf.Sign(delta) * Mathf.Min(Mathf.Abs(delta), maxScrollDelta);
            }
            else
            {
                isDragging = false;
            }

            if (sendEventDataToParent)
            {
                SendDragEventToParent(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            if (sendEventDataToParent)
            {
                SendEndDragEventToParent(eventData);
            }
        }

        private void DragElements(float delta)
        {
            bool canDrag = false;
            if (delta > 0)
            {
                canDrag =
                    toIndex < data.Count - 1 ||
                    (toIndex == data.Count - 1 && elements[elementCount - 1].anchoredPosition.y < -(selfRt.rect.height - elementHeight));

                if (toIndex == data.Count - 1)
                {
                    delta = Mathf.Min(
                        delta,
                        -(selfRt.rect.height - elementHeight) - elements[elementCount - 1].anchoredPosition.y);
                }
            }
            else if (delta < 0)
            {
                canDrag =
                    fromIndex > 0 ||
                    (fromIndex == 0 && elements[0].anchoredPosition.y > 0);

                if (fromIndex == 0)
                {
                    delta = Mathf.Max(delta, -elements[0].anchoredPosition.y);
                }
            }

            if (!canDrag)
                return;
            for (int i = 0; i < elements.Count; ++i)
            {
                elements[i].anchoredPosition += Vector2.up * delta;
            }
        }

        private void RecycleElements(float delta)
        {
            if (delta > 0 && toIndex < data.Count - 1)
            {
                for (int i = 0; i < elements.Count; ++i)
                {
                    bool outUpperBound = elements[i].anchoredPosition.y > elementHeight;
                    if (outUpperBound)
                    {
                        RectTransform e = elements[i];
                        LayoutElementAsLast(elements[i]);
                        fromIndex += 1;
                        toIndex += 1;
                        //displayDataAction(e.gameObject, data[toIndex]);
                        if (toIndex == data.Count - 1)
                        {
                            onReachLastElement();
                        }
                    }
                }
            }
            else if (delta < 0 && fromIndex > 0)
            {
                for (int i = elements.Count - 1; i >= 0; --i)
                {
                    bool outLowerBound = elements[i].anchoredPosition.y < -selfRt.rect.height;
                    if (outLowerBound)
                    {
                        RectTransform e = elements[i];
                        LayoutElementAtFirst(elements[i]);
                        fromIndex -= 1;
                        toIndex -= 1;
                        //displayDataAction(e.gameObject, data[fromIndex]);
                    }
                }
            }
        }

        public void AppendData<T>(ICollection<T> newData)
        {
            if (data == null)
                data = new List<object>();
            if (newData.Count == 0)
                return;
            //data.AddRange(newData);
            IEnumerator i = newData.GetEnumerator();
            while (i.MoveNext())
            {
                data.Add(i.Current);
            }

            if (toIndex - fromIndex < elementCount - 1)
            {
                fromIndex = 0;
                toIndex = Mathf.Min(elements.Count, data.Count) - 1;
            }

        }

        public void ClearData()
        {
            if (data == null)
                return;
            for (int i = 0; i < data.Count; ++i)
            {
                if (data[i] is IDisposable)
                {
                    (data[i] as IDisposable).Dispose();
                }
            }
            data.Clear();
            fromIndex = -1;
            toIndex = -1;
            if (elements != null && elements.Count > 0)
            {
                Vector2 translation = elements[0].anchoredPosition;
                for (int i = 0; i < elements.Count; ++i)
                {
                    elements[i].anchoredPosition -= translation;
                }
            }
            delta = 0;
        }

        private void AddElement(RectTransform e)
        {
            if (elements == null)
                elements = new List<RectTransform>();
            e.SetParent(transform, false);
            e.anchorMax = new Vector2(0, 1);
            e.anchorMin = new Vector2(0, 1);
            e.pivot = new Vector2(0, 1);

            elements.Add(e);
            float posY = -elementHeight * (elements.Count - 1);
            e.anchoredPosition = new Vector2(0, posY);
            e.sizeDelta = new Vector2(selfRt.rect.width, elementHeight);
        }

        private void LayoutElementAsLast(RectTransform e)
        {
            if (e == elements[0])
            {
                elements.RemoveAt(0);
            }
            float posY = elements[elements.Count - 1].anchoredPosition.y - elementHeight;
            e.anchoredPosition = new Vector2(0, posY);
            elements.Add(e);
        }

        private void LayoutElementAtFirst(RectTransform e)
        {
            if (e == elements[elements.Count - 1])
            {
                elements.RemoveAt(elements.Count - 1);
            }
            float posY = elements[0].anchoredPosition.y + elementHeight;
            e.anchoredPosition = new Vector2(0, posY);
            elements.Insert(0, e);
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
    }
}