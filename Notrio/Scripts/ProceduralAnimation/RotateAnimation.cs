using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel
{
    [AddComponentMenu("Pinwheel/Animate/Rotate Animation")]
    public class RotateAnimation : ProceduralAnimation
    {
        public CurveTuple[] curves;
        public bool isRelative;

        public void Reset()
        {
            duration = 0.3f;
            curves = new CurveTuple[1];
            curves[0].x = AnimationCurve.Linear(0, 0, 1, 0);
            curves[0].y = AnimationCurve.Linear(0, 0, 1, 0);
            curves[0].z = AnimationCurve.Linear(0, 0, 1, 0);
        }

        public override void Play(int curveIndex)
        {
            StopAllCoroutines();
            CurveTuple c = curves[curveIndex];
            StartCoroutine(CrRotate(c, duration, isRelative));
        }

        public void Play(int curveIndex, float duration)
        {
            StopAllCoroutines();
            CurveTuple c = curves[curveIndex];
            StartCoroutine(CrRotate(c, duration, isRelative));
        }

        public void Play(int curveIndex, float duration, bool isRelative)
        {
            StopAllCoroutines();
            CurveTuple c = curves[curveIndex];
            StartCoroutine(CrRotate(c, duration, isRelative));
        }

        public void Play(CurveTuple c)
        {
            StopAllCoroutines();
            StartCoroutine(CrRotate(c, duration, isRelative));
        }

        public void Play(CurveTuple c, float duration)
        {
            StopAllCoroutines();
            StartCoroutine(CrRotate(c, duration, isRelative));
        }

        public void Play(CurveTuple c, float duration, bool isRelative)
        {
            StopAllCoroutines();
            StartCoroutine(CrRotate(c, duration, isRelative));
        }
        private IEnumerator CrRotate(CurveTuple c, float duration, bool isRelative)
        {
            Vector3 factor = isRelative ? transform.localEulerAngles : Vector3.zero;
            float time = 0;
            float x, y, z;
            while (time <= duration)
            {
                x = c.x.Evaluate(time / duration);
                y = c.y.Evaluate(time / duration);
                z = c.z.Evaluate(time / duration);
                transform.localEulerAngles = new Vector3(x, y, z) + factor;
                if (time == duration)
                    break;
                time = Mathf.MoveTowards(time, duration, Time.smoothDeltaTime);
                yield return null;
            }
        }

    }
}
