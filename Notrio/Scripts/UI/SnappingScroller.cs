using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Takuzu {
	public class SnappingScroller : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
		public enum LockDirection {
			None,
			Left,
			Right,
			Both
		}

		public System.Action<int, int> onSnapIndexChanged = delegate { };
		public System.Action<float> onScrollingViewPositionChanged = delegate { };
		public RectTransform content;
		public RectTransform snappingPoint;
        public bool usedInOverlayPanel = false;
		public float sensivity;
		public float minDeltaX;
		public float maxVoidOffset;
		public float snappingSpeed;
		public float fastDragVelocity;
		public bool fitOneElement;
		public bool fastDragOnly;
		public LockDirection lockDirection;
		public bool resetScrollPosOnClearElements;
		public RectTransform[] presetElements;
        public bool isScrolling = false;

		private int snapIndex = -1;
		public int SnapIndex {
			get {
				return snapIndex;
			}
			set {
				int oldValue = snapIndex;
				int newValue = value;
				snapIndex = newValue;
				if (oldValue != newValue)
					onSnapIndexChanged (newValue, oldValue);
			}
		}

		private Vector2 snapPos;
		private float dragTime;
		private float dragBeginTime;
		private float dragVelocity;
		private float dragBeginX;
		private float dragLengthX;
        private bool lockDrag;
        private bool enableFindSnap;
		public float RelativeNormalizedScrollPos {
			get {
				float pos = -(content.anchoredPosition.x / content.rect.width);
				if (float.IsNaN (pos))
					return 0;
				return pos;
			}
		}
		public int ElementCount {
			get {
				return content.childCount;
			}
		}

		bool isDragging;
		RectTransform selfRt;
		float canvasRefResolutionX;
		float canvasRefResolutionY;
		private Vector2 lastAnchorPosition = Vector2.zero;

		private void Awake () {
			selfRt = GetComponent<RectTransform> ();
			Vector2 refResolution = selfRt.GetComponentInParent<CanvasScaler> ().referenceResolution;
			//canvasRefResolutionX = refResolution.x;
			//canvasRefResolutionY = refResolution.y;
			canvasRefResolutionX = CanvasScalerHelper.currentReferenceResolution.x;
			canvasRefResolutionY = CanvasScalerHelper.currentReferenceResolution.y;
          

            for (int i = 0; i < presetElements.Length; ++i) {
				AddElement (presetElements[i]);
			}

			ScreenManager.onScreenResolutionChanged += OnResolutionChanged;
        }
		
		public float GetCurrentPosition(){
			return Mathf.Clamp01((-content.anchoredPosition).x / (content.rect.width - selfRt.rect.width));
		}

        private void OnDestroy () {
			ScreenManager.onScreenResolutionChanged -= OnResolutionChanged;
        }

        private void OnResolutionChanged (Vector2 res) {
			if (fitOneElement) {
				List<Transform> elements = content.GetAllChildren ();
				for (int i = 0; i < elements.Count; ++i) {
					ResizeElement (elements[i] as RectTransform);
				}
				LayoutRebuilder.ForceRebuildLayoutImmediate (content);
			}
        }

        private void ResizeAllElements () {
			if (fitOneElement) {
				List<Transform> elements = content.GetAllChildren ();
				for (int i = 0; i < elements.Count; ++i) {
					ResizeElement (elements[i] as RectTransform);
				}
				LayoutRebuilder.ForceRebuildLayoutImmediate (content);
			}
		}

		private void Update () {
            lockDrag = CheckLockDrag();

			float oldResX = canvasRefResolutionX;
			canvasRefResolutionX = CanvasScalerHelper.currentReferenceResolution.x;

			float oldResY = canvasRefResolutionY;
			canvasRefResolutionY = CanvasScalerHelper.currentReferenceResolution.y;

			if (oldResX != canvasRefResolutionX ||
				oldResY != canvasRefResolutionY)
				ResizeAllElements ();

			if (ElementCount > 0 && SnapIndex < 0) {
				SnapIndex = 0;
			}
			if (SnapIndex >= ElementCount) {
				SnapIndex = Mathf.Clamp (SnapIndex, 0, ElementCount - 1);
				GetSnapPos ();
			}
			if (!isDragging || lockDrag)
            {
				Snap ();
            }
            if ((content.anchoredPosition - lastAnchorPosition).magnitude > 0)
            {
                onScrollingViewPositionChanged(Mathf.Clamp01((-content.anchoredPosition).x / (content.rect.width - selfRt.rect.width)));
                isScrolling = true;
            }
            else if (!isDragging)
            {
                isScrolling = false;
            }
			lastAnchorPosition = content.anchoredPosition;
		}

		public void OnBeginDrag (PointerEventData data) {
			if (lockDirection == LockDirection.Both) {
				isDragging = false;
				return;
			}

			float x = data.delta.x;
			if (x < 0 && lockDirection == LockDirection.Left) {
				isDragging = false;
				return;
			}

			if (x > 0 && lockDirection == LockDirection.Right) {
				isDragging = false;
				return;
			}

            if (lockDrag)
                return;
            isDragging = true;
			dragBeginTime = Time.time;
			dragBeginX = data.position.x;
		}

		public void OnDrag (PointerEventData data) {
            if (lockDirection == LockDirection.Both) {
				isDragging = false;
				return;
			}
			float x = data.delta.x;
			if (x < 0 && lockDirection == LockDirection.Left) {
				isDragging = false;
				return;
			}
			if (x > 0 && lockDirection == LockDirection.Right) {
				isDragging = false;
				return;
			}

            if (lockDrag)
                return;
            if (!isDragging)
                return;

            float delta = data.delta.x * canvasRefResolutionX / Screen.width;
			float voidOffset = 0;
			if (content.anchoredPosition.x >= 0) {
				voidOffset = content.anchoredPosition.x + delta * sensivity;
			} else {
				voidOffset = selfRt.rect.width - content.rect.width - content.anchoredPosition.x - delta * sensivity;
			}
			float deceleration = 1;
			if (maxVoidOffset == 0) {
				if (voidOffset >= maxVoidOffset)
					deceleration = 0;
			} else {
				deceleration = Mathf.Clamp01 (1 - voidOffset / maxVoidOffset);
			}
			if (Mathf.Abs (delta) >= minDeltaX)
				content.anchoredPosition = content.anchoredPosition + (delta * sensivity * deceleration * Vector2.right);
			if (deceleration == 0)
				SnapImmediately ();
		}

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                isDragging = false;
                FindSnapElement();
                SnapImmediately();
            }
        }

        public void OnEndDrag (PointerEventData data) {
            if (!isDragging)
                return;
			isDragging = false;
			int lastSnapIndex = SnapIndex;
			if (lockDirection == LockDirection.Both) {
				isDragging = false;
				return;
			}
			float x = data.delta.x;
			if (x < 0 && lockDirection == LockDirection.Left) {
				isDragging = false;
				return;
			}
			if (x > 0 && lockDirection == LockDirection.Right) {
				isDragging = false;
				return;
			}

            if (lockDrag)
                return;

			dragTime = Time.time - dragBeginTime;
			dragLengthX = data.position.x - dragBeginX;
			dragVelocity = dragLengthX / dragTime;
			if (Mathf.Abs (dragVelocity) >= fastDragVelocity) {
				if (lastSnapIndex == SnapIndex) {
					SnapIndex -= (int) Mathf.Sign (dragVelocity);
					SnapIndex = Mathf.Clamp (SnapIndex, 0, content.childCount - 1);
				}
			} else if (!fastDragOnly) {
				FindSnapElement ();
			}

		}

        public bool overrideLockScroll = false;
        [HideInInspector]
        public bool lockScrollOR = false;

        bool CheckLockDrag()
        {
            bool overlayIsShown = false;
            if (!usedInOverlayPanel)
            {
                foreach (var intance in OverlayUIController.overlayUIControllerIntances)
                {
                    if (intance.darkenImage.GetComponent<CanvasGroup>().blocksRaycasts)
                    {
                        overlayIsShown = true;
                        break;
                    }
                }
            }

            if (overrideLockScroll)
                return lockScrollOR;

            if (GameManager.Instance.GameState == GameState.Prepare)
            {
                if(!enableFindSnap)
                {
                    enableFindSnap = true;
                }
                if (!overlayIsShown)
                    return false;
                else
                    return true;
            }
            else
            {
                if(enableFindSnap)
                {
                    FindSnapElement();
                    enableFindSnap = false;
                }
                return true;
            }

        }

		public void Snap () {
			if (SnapIndex == -1)
				return;
			GetSnapPos ();
			content.anchoredPosition = Vector2.MoveTowards (content.anchoredPosition, snapPos, snappingSpeed * Time.smoothDeltaTime / 0.017f);
		}

		public void SnapImmediately () {
			if (SnapIndex == -1)
				return;
			GetSnapPos ();
			content.anchoredPosition = snapPos;
        }

        private void FindSnapElement () {
			int tmpSnapIndex = -1;
			List<Transform> element = content.transform.GetAllChildren ();
			float snapPointX = snappingPoint.position.x;
			float minDistance = float.MaxValue;
			float distance;

			for (int i = 0; i < element.Count; ++i) {
				distance = Mathf.Abs (snapPointX - element[i].transform.position.x);
				if (distance < minDistance) {
					minDistance = distance;
					tmpSnapIndex = i;
				}
			}

			SnapIndex = tmpSnapIndex;
		}

		public void AddElement (RectTransform rt) {
			rt.pivot = new Vector2 (0, 0.5f);
			if (fitOneElement) {
				ResizeElement (rt);
			}
			rt.SetParent (content.transform, false);

		}

		public void ResizeElement (RectTransform rt) {
			float x = Screen.width * canvasRefResolutionY / Screen.height;
			rt.sizeDelta = new Vector2 (
				x,
				rt.sizeDelta.y);
		}

		public void ClearElement () {
			content.transform.ClearAllChildren ();
			if (resetScrollPosOnClearElements) {
				ResetScrollPos ();
			}
		}

		public void GetSnapPos () {
			float x = 0;
			for (int i = 0; i < SnapIndex; ++i) {
				RectTransform rt = content.GetChild (i) as RectTransform;
				x -= rt.rect.width;
			}
			snapPos = new Vector2 (x, content.anchoredPosition.y);
		}

		public void ResetScrollPos () {
			content.anchoredPosition = Vector2.zero;
		}
	}
}