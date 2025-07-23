using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

namespace Takuzu
{
    /// <summary>
    /// Handle player input.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler Instance { get; private set; }
        public static Action<Vector2> onMouseButtonDown = delegate { };
        public static Action<Vector2> onMouseButtonHold = delegate { };
        public static Action<Vector2> onMouseButtonUp = delegate { };
        public static Action<Vector2> onMouseClick = delegate { };

        public bool willSendEvent;
        public OverlayUIController overlayUiController;
        public ConfirmationDialog dialog;

        public bool hideCursorForScreenShot = false;
        public bool hideCursorForOpponentView = false;

        public RectTransform cursor;
        public bool activeAssistiveInput = false;
        public bool isTutorial;
        public float mouseClickMaxDistance;
        public float mouseClickMaxTime;
        public bool IsClickOnUI { get {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                foreach (Touch touch in Input.touches)
                {
                    int id = touch.fingerId;
                    if (EventSystem.current.IsPointerOverGameObject(id))
                    {
                        return true;
                    }
                 }
#else
                if (EventSystem.current.IsPointerOverGameObject())
                    return true;
#endif
                return false;
            } }
        private bool clickOnUI = false;
        private Vector2 mouseDownPos;
        private float mouseDownTime;
        private Vector3 lastTouchPostion = Vector3.zero;

        private Vector3 boundPosition;
        private Vector2 boundSize = Vector2.zero;

        private bool setActiveCursor = false;
        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this;
        }

        public Vector3 CursorPosition
        {
            get
            {
                if (cursor)
                {
                    Vector2 mousePositionV2 = RectTransformUtility.WorldToScreenPoint(Camera.main, cursor.position);
                    return activeAssistiveInput ? new Vector3(mousePositionV2.x, mousePositionV2.y, Input.mousePosition.z) : Input.mousePosition;
                }
                else
                {
                    return Vector3.zero;
                }
            }
        }

        private bool CanUseCursor
        {
            get
            {
                return cursor && boundSize != Vector2.zero && activeAssistiveInput;
            }
        }

        private void Update()
        {
            if (cursor)
            {
                if (!GameManager.Instance.GameState.Equals(GameState.Playing) || hideCursorForScreenShot)
                    cursor.gameObject.SetActive(false);
                else
                {
                    if (hideCursorForOpponentView)
                    {
                        cursor.gameObject.SetActive(false);
                    }
                    else
                    {
                        if (setActiveCursor && CanUseCursor)
                        {
                            cursor.gameObject.SetActive(true);
                        }
                        else
                        {
                            cursor.gameObject.SetActive(false);
                        }
                    }
                }
            }
            try
            {
                willSendEvent = true;
                if (!isTutorial)
                {
                    if (GameManager.Instance == null || GameManager.Instance.GameState != GameState.Playing || dialog.IsShowing)
                        willSendEvent = false;
                }

                //if (LogicalBoard.Instance.isPlayingRevealAnim)
                //    return;
                if (ScreenManager.Instance != null && !ScreenManager.Instance.IsAspectRatioSupported)
                    willSendEvent = false;

                if (overlayUiController != null && overlayUiController.ShowingPanelCount > 0)
                    willSendEvent = false;

                if (LogicalBoard.Instance.isAutoSolving)
                    willSendEvent = false;

                if (!willSendEvent)
                    return;

                //Handle multitouch
                if (Input.touchCount > 1)
                {
                    HandleMultitouch();
                    return;
                }
                Vector3 mousePosition = Input.mousePosition;
                if (CanUseCursor)
                {
                    Vector2 mousePositionV2 = RectTransformUtility.WorldToScreenPoint(Camera.main, cursor.position);
                    mousePosition = activeAssistiveInput ? new Vector3(mousePositionV2.x, mousePositionV2.y, Input.mousePosition.z) : Input.mousePosition;
                }
                    
                if (Input.GetMouseButtonDown(0))
                {
                    clickOnUI = IsClickOnUI;
                    mouseDownPos = mousePosition;
                    mouseDownTime = Time.time;
                    lastTouchPostion = Vector3.zero;
                    if(CanUseCursor)
                        RectTransformUtility.ScreenPointToWorldPointInRectangle(cursor, Input.mousePosition, Camera.main, out lastTouchPostion);
                    onMouseButtonDown(mousePosition);
                }
                if (Input.GetMouseButton(0))
                {
                    if (CanUseCursor)
                    {
                        Vector3 newTouchPosition = Vector3.zero;
                        RectTransformUtility.ScreenPointToWorldPointInRectangle(cursor, Input.mousePosition, Camera.main, out newTouchPosition);
                        float WorldCenterViewPortPointTop = boundPosition.y + boundSize.y/2;
                        float WorldCenterViewPortPointBot = boundPosition.y - boundSize.y / 2;
                        float WorldCenterViewPortPointRight = boundPosition.x + boundSize.x / 2;
                        float WorldCenterViewPortPointLeft = boundPosition.x - boundSize.x / 2;

                        cursor.position += (Vector3)((Vector2)(newTouchPosition - lastTouchPostion));
                        cursor.position = new Vector3(Mathf.Clamp(cursor.position.x, WorldCenterViewPortPointLeft, WorldCenterViewPortPointRight), Mathf.Clamp(cursor.position.y, WorldCenterViewPortPointBot, WorldCenterViewPortPointTop), cursor.position.z);
                        lastTouchPostion = newTouchPosition;
                    }
                    onMouseButtonHold(mousePosition);
                }
                if (Input.GetMouseButtonUp(0))
                {
                    onMouseButtonUp(mousePosition);
                    if (Vector2.Distance(mousePosition, mouseDownPos) <= mouseClickMaxDistance &&
                        (Time.time - mouseDownTime) < mouseClickMaxTime)
                    {
                        if (!clickOnUI)
                            onMouseClick(mousePosition);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        private float[] mTouchTimes = new float[20];
        private Vector2[] mTouchPoses = new Vector2[20];
        private void HandleMultitouch()
        {
            int touchCount = Input.touchCount;
            Touch[] touches = Input.touches;
            for (int i = 0; i < touchCount; i++)
            {
                Touch currTouch = touches[i];
                if(currTouch.phase == TouchPhase.Began)
                {
                    mTouchTimes[currTouch.fingerId] = Time.time;
                    mTouchPoses[currTouch.fingerId] = currTouch.position;
                    onMouseButtonDown(currTouch.position);
                }
                if (currTouch.phase == TouchPhase.Ended || currTouch.phase == TouchPhase.Canceled)
                {
                    onMouseButtonUp(currTouch.position);
                    if (Vector2.Distance(currTouch.position, mTouchPoses[currTouch.fingerId]) <= mouseClickMaxDistance &&
                        (Time.time - mTouchTimes[currTouch.fingerId]) < mouseClickMaxTime)
                    {
                        if (!clickOnUI)
                        {
                            onMouseClick(currTouch.position);
                            //Debug.Log("Click: " + currTouch.fingerId + " " + currTouch.position);
                        }
                    }
                }
                onMouseButtonHold(currTouch.position);
            }
        }

        public void SetCursorBound(Vector3 position, Vector2 size)
        {
            boundPosition = position;
            boundSize = size;
        }

        public void ResetAssistCursorPosition()
        {
            Vector3 WorldCenterViewPortPoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            if (cursor != null)
            {
                cursor.position = new Vector3(WorldCenterViewPortPoint.x, WorldCenterViewPortPoint.y, cursor.position.z);
            }
        }

        public void EnableCursor()
        {
            setActiveCursor = true;
        }
        public void DisbaleCursor()
        {
            setActiveCursor = false;
        }
    }
}