using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Takuzu
{
    public class UiGroupController : UIBehaviour
    {
        public bool bypassTransformAnimationOnAndroid;
        public bool bypassTransformAnimationOnIos;
        public List<ProceduralAnimation> anim;
        public CanvasGroup group;
        public bool autoOrdering = true;
        public bool? isShowing;
        [HideInInspector]
        private float maxDuration = -1;

        private float yieldDelay = 0.1f;

        public UnityEvent onShowBegin, onShowCompleted;
        public UnityEvent onHideBegin, onHideCompleted;

        protected List<System.Type> transformAnimationType { get {if (bypassTransformAnimationOnAndroid)
                    return new List<System.Type>()
                {
                    typeof(ScaleAnimation),
                    typeof(PositionAnimation),
                    typeof(RotateAnimation)
                };
                else if (bypassTransformAnimationOnIos)
                    return new List<System.Type>()
                {
                    typeof(ScaleAnimation),
                    typeof(PositionAnimation),
                    typeof(RotateAnimation)
                };
                else
                    return new List<System.Type>();
            } }

        public float MaxDuration
        {
            get
            {
                if(maxDuration==-1)
                    GetMaxDuration();
                return maxDuration;
            }

            set
            {
                maxDuration = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            GetMaxDuration();
        }

        protected virtual void ShowBeginAction()
        {
            onShowBegin.Invoke();
            group.blocksRaycasts = true;
            group.interactable = false;
            if (autoOrdering)
                transform.BringToFront();
        }

        protected virtual void ShowCompletedAction()
        {
            onShowCompleted.Invoke();
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        protected virtual void HideBeginAction()
        {
            onHideBegin.Invoke();
            group.blocksRaycasts = true;
            group.interactable = false;
        }

        protected virtual void HideCompletedAction()
        {
            onHideCompleted.Invoke();
            group.blocksRaycasts = false;
            group.interactable = false;
            if (autoOrdering)
                transform.SendToBack();
        }

        public virtual void Show()
        {
            ShowBeginAction();
            isShowing = true;
            PlayAnimIn();
            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                if (group != null && isShowing == true)
                {
                    ShowCompletedAction();
                }
            },
            MaxDuration + yieldDelay);
        }

        public virtual void ShowIfNot()
        {
            if (isShowing != true)
                Show();
        }

        public virtual void Hide()
        {
            HideBeginAction();
            isShowing = false;
            PlayAnimOut();
            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                if (group != null && isShowing == false)
                {
                    HideCompletedAction();
                }
            },
            MaxDuration + yieldDelay);
        }

        public virtual void HideIfNot()
        {
            if (isShowing != false)
                Hide();
        }

        private void PlayAnimIn()
        {
            for (int i = 0; i < anim.Count; ++i)
            {
                if (!transformAnimationType.Contains(anim[i].GetType()))
                {
                    if (anim[i].gameObject.activeInHierarchy)
                        anim[i].Play(AnimConstant.IN);
                }
            }
        }

        private void PlayAnimOut()
        {
            for (int i = 0; i < anim.Count; ++i)
            {
                if (!transformAnimationType.Contains(anim[i].GetType()))
                {
                    if (anim[i].gameObject.activeInHierarchy)
                        anim[i].Play(AnimConstant.OUT);
                }
            }
        }

        private void GetMaxDuration()
        {
            MaxDuration = 0;
            for (int i = 0; i < anim.Count; ++i)
            {
                MaxDuration = anim[i].duration > MaxDuration ? anim[i].duration : MaxDuration;
            }
        }
    }
}