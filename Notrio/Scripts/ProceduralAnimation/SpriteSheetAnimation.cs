using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace Pinwheel
{
    [AddComponentMenu("Pinwheel/Animate/Sprite Sheet Animation")]
    public class SpriteSheetAnimation : ProceduralAnimation
    {
        public Component target;
        public AnimationCurve[] indexVariant;
        public Sprite[] sprites;

        public override void Play(int curveIndex)
        {
            if (sprites.Length == 0)
                return;
            StopAllCoroutines();
            AnimationCurve c = indexVariant[curveIndex];
            StartCoroutine(CrPlay(c));
        }

        private IEnumerator CrPlay(AnimationCurve c)
        {
            int i;
            int maxIndex = sprites.Length - 1;
            PropertyInfo spriteProps = target.GetType().GetProperty("sprite");
            float time = 0;
            while (time <= duration)
            {
                i = Mathf.RoundToInt(c.Evaluate(time / duration) * maxIndex);
                if (spriteProps != null)
                {
                    Sprite s = sprites[i];
                    spriteProps.SetValue(target, s, null);
                }

                if (time == duration)
                    break;
                time = Mathf.MoveTowards(time, duration, Time.smoothDeltaTime);
                yield return null;
            }
        }
    }
}