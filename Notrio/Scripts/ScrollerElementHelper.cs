using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    public class ScrollerElementHelper : MonoBehaviour
    {
        public SnappingScroller scroller;
        public CanvasGroup group;
        public Gradient blend;
        public int index;

        private void Update()
        {
            if (scroller == null)
                return;
            group.alpha = blend.Evaluate(Mathf.Abs(index - scroller.RelativeNormalizedScrollPos * scroller.ElementCount)).a;
        }
    }
}