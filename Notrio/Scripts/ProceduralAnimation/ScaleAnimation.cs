using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel
{
    [AddComponentMenu("Pinwheel/Animate/Scale Animation")]
    public class ScaleAnimation : ProceduralAnimation
    {
        public CurveTuple[] curves;
        public bool isRelative;
        public Vector3 initialScale;

        public void Reset()
        {
            isRelative = true;
            initialScale = Vector3.one;
            duration = 0.3f;
            curves = new CurveTuple[1];
            curves[0].x = AnimationCurve.Linear(0, 0, 1, 1);
            curves[0].y = AnimationCurve.Linear(0, 0, 1, 1);
            curves[0].z = AnimationCurve.Linear(0, 0, 1, 1);
        }

        public override void Play(int curveIndex)
        {
            StopAllCoroutines();
            CurveTuple c = curves[curveIndex];
            StartCoroutine(CrPlay(c, duration, isRelative));
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

        private IEnumerator CrPlay(CurveTuple c, float duration, bool isRelative)
        {
            Vector3 multiplier = isRelative ? initialScale : Vector3.one;
            float time = 0;
            float x, y, z;
            while (time <= duration)
            {
                x = c.x.Evaluate(time / duration);
                y = c.y.Evaluate(time / duration);
                z = c.z.Evaluate(time / duration);
                transform.localScale = Vector3.Scale(new Vector3(x, y, z), multiplier);
                if (time == duration)
                    break;
                time = Mathf.MoveTowards(time, duration, Time.smoothDeltaTime);
                yield return null;
            }
        }

    }
}