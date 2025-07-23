using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevierButton : MonoBehaviour , IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler{

    [Header("UI References")]
    //public Transform knobTransform;
    public Transform container;
    public Transform rodTransform;

    public float SnapBackSpeed = 3;

    public AnimationCurve knowXPositionAnimationCuver;
    public System.Action buttonClicked = delegate { };

    private float levierProcess = 0;
    private Vector2 startLocalposition;
    private Vector2 currentLocalPosition;
    private bool dragging = true;
    private bool autoPullDownCoroutineIsRunning = false;

    public float LevierProcess
    {
        get
        {
            return levierProcess;
        }

        set
        {
            levierProcess = value;
            if (levierProcess >= 1)
            {
                dragging = false;
                buttonClicked();
            }
            float xProgress = levierProcess*2;
            if (xProgress > 1)
            {
                xProgress = 2 - xProgress;
            }
            //(knobTransform as RectTransform).anchoredPosition = new Vector2(knowXPositionAnimationCuver.Evaluate(xProgress) * (container as RectTransform).rect.width, (0.5f - levierProcess)*(container as RectTransform).rect.height);
            //(rodTransform as RectTransform).sizeDelta = new Vector2((rodTransform as RectTransform).sizeDelta.x, (knobTransform as RectTransform).anchoredPosition.magnitude);
            //rodTransform.localEulerAngles = new Vector3(0, 0, -Vector2.Angle((knobTransform as RectTransform).anchoredPosition, new Vector2(0, 1)));
            rodTransform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(0, -140, levierProcess));
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (autoPullDownCoroutineIsRunning)
            return;
        dragging = true;
        if (!dragging)
            return;
        startLocalposition = CalculateRectTransformPositionUtility.GetNormalizedLocalPointerPosition(eventData.position, container as RectTransform, Camera.main);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (autoPullDownCoroutineIsRunning)
            return;
        if (!dragging)
            return;
        currentLocalPosition = CalculateRectTransformPositionUtility.GetNormalizedLocalPointerPosition(eventData.position, container as RectTransform, Camera.main);
        LevierProcess = Mathf.Clamp01(-(currentLocalPosition - startLocalposition).y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (autoPullDownCoroutineIsRunning)
            return;
        StartCoroutine(AutoPullDown());
        dragging = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!autoPullDownCoroutineIsRunning)
            StartCoroutine(AutoPullDown());
    }

    private IEnumerator AutoPullDown()
    {
        autoPullDownCoroutineIsRunning = true;
        dragging = true;
        float duration = 0.2f;
        float t = duration*LevierProcess;
        while(t <= duration)
        {
            yield return null;
            LevierProcess = t / duration; 
            t += Time.deltaTime;
        }
        LevierProcess = 1;
        autoPullDownCoroutineIsRunning = false;
        dragging = false;
    }

    private void Update()
    {
        if (!dragging && LevierProcess!=0)
        {
            LevierProcess = Mathf.Clamp01(levierProcess - Time.deltaTime*SnapBackSpeed);
            //Try to snap back to default position
        }
    }
}
