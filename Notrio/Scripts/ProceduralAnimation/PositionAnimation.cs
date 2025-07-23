using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Pinwheel
{
    [AddComponentMenu("Pinwheel/Animate/Position Animation")]
    public class PositionAnimation : ProceduralAnimation
    {
        public CurveTuple[] curves;
        public bool isRelative;
        public PositionAnimationType type;

        private Vector3 origin;
        private RectTransform rectTransform;
        private Action<Vector3> positioningAction = delegate { };
		public bool isAnimationRunning = false;
        public bool twoWayAnimation = false;
        private float lastTime = 0;
        public void Reset()
        {
            duration = 0.3f;
            curves = new CurveTuple[1];
            curves[0].x = AnimationCurve.Linear(0, 0, 1, 0);
            curves[0].y = AnimationCurve.Linear(0, 0, 1, 0);
            curves[0].z = AnimationCurve.Linear(0, 0, 1, 0);

            if (GetComponent<RectTransform>() != null)
            {
                type = PositionAnimationType.RectTransform;
            }
        }
		private void Awake() {
			Init();
		}

        public void Init()
        {
            if (type == PositionAnimationType.RectTransform)
            {
                rectTransform = GetComponent<RectTransform>();
                positioningAction = PositioningRectTransform;
                origin = rectTransform.anchoredPosition3D;
            }
            else
            {
                positioningAction = PositioningTransform;
                origin = transform.localPosition;
            }
        }

        public override void Play(int curveIndex)
        {
            StopAllCoroutines();
            CurveTuple c = curves[twoWayAnimation ? 0 : curveIndex];
            StartCoroutine(CrPlay(c, duration, isRelative, curveIndex == 0 ));
        }

        public void Play(int curveIndex, float duration)
        {
            StopAllCoroutines();
            CurveTuple c = curves[curveIndex];
            StartCoroutine(CrPlay(c, duration, isRelative));
        }

        public void Play(int curveIndex, float duration, bool isRelative)
        {
            StopAllCoroutines();
            CurveTuple c = curves[curveIndex];
            StartCoroutine(CrPlay(c, duration, isRelative));
        }

        public void Play(CurveTuple c)
        {
            StopAllCoroutines();
            StartCoroutine(CrPlay(c, duration, isRelative));
        }

        public void Play(CurveTuple c, float duration)
        {
            StopAllCoroutines();
            StartCoroutine(CrPlay(c, duration, isRelative));
        }

        public void Play(CurveTuple c, float duration, bool isRelative)
        {
            StopAllCoroutines();
            StartCoroutine(CrPlay(c, duration, isRelative));
        }

        private IEnumerator CrPlay(CurveTuple c, float duration, bool isRelative, bool direction = true)
        {
			isAnimationRunning = true;
            if (twoWayAnimation)
            {
                Vector3 factor = isRelative ? origin : Vector3.zero;
                float time = twoWayAnimation ? lastTime : 0;
                float x, y, z;
                while (time <= duration && time >= 0)
                {
                    x = c.x.Evaluate(time / duration);
                    y = c.y.Evaluate(time / duration);
                    z = c.z.Evaluate(time / duration);
                    positioningAction(new Vector3(x, y, z) + factor);
                    time += Time.deltaTime * (direction ? 1 : -1);
                    time = Mathf.Clamp(time, 0, duration);
                    lastTime = time;
                    yield return null;
                }
            }
            else
            {
                Vector3 factor = isRelative ? origin : Vector3.zero;
                float time = 0;
                float x, y, z;
                while (time <= duration)
                {
                    x = c.x.Evaluate(time / duration);
                    y = c.y.Evaluate(time / duration);
                    z = c.z.Evaluate(time / duration);
                    positioningAction(new Vector3(x, y, z) + factor);

                    if (time == duration)
                        break;

                    time = Mathf.MoveTowards(time, duration, Time.smoothDeltaTime);
                    yield return null;
                }
            }
            isAnimationRunning = false;
        }

        private void PositioningRectTransform(Vector3 position)
        {
            rectTransform.anchoredPosition3D = position;
        }

        private void PositioningTransform(Vector3 position)
        {
            transform.localPosition = position;
        }

		public void SetOrigin(Vector2 position){
			origin = transform.localPosition;
			origin.x = position.x;
			origin.y = position.y;
		}
    }

    public enum PositionAnimationType
    {
        Transform, RectTransform
    }


}
