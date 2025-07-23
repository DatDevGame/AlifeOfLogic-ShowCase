using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinwheel;
using System;
using System.Linq;

namespace Takuzu
{
    public class OverlayGroupController : UiGroupController
    {
        public RectTransform container;
        public Vector2 hidePostion = new Vector2(0, -1136);
        public Vector2 showPosition = Vector2.zero;
        public bool dontDeactivateOnHidden;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            anim = new List<ProceduralAnimation>();
            if (group != null)
                anim.AddRange(group.GetComponents<ProceduralAnimation>());
            if (container != null)
                anim.AddRange(container.GetComponents<ProceduralAnimation>());
            anim.Distinct();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            if (!dontDeactivateOnHidden)
                container.gameObject.SetActive(false);
        }

        protected override void ShowBeginAction()
        {
            base.ShowBeginAction();
            container.anchoredPosition = showPosition;
            container.gameObject.SetActive(true);
        }

        protected override void HideCompletedAction()
        {
            base.HideCompletedAction();
            container.anchoredPosition = hidePostion;
            if (!dontDeactivateOnHidden)
                container.gameObject.SetActive(false);
        }
    }
}