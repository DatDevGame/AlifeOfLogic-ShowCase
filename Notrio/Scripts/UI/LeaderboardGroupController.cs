using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class LeaderboardGroupController : OverlayPanel
    {
        public UiGroupController controller;
        public RectTransform container;
        public float referenceHeight = 1136;
        public SnappingScroller scroller;
        public Button[] closeButtons;
        public LeaderboardController[] lbControllers;

        string lbName;

        public const int EXP_LB_INDEX = 0;
        public const int DAILY_LB_INDEX = 1;
        public const int WEEKLY_LB_INDEX = 2;

        public override void Show()
        {
            controller.ShowIfNot();
            IsShowing = true;
            transform.BringToFront();
            onPanelStateChanged(this, true);
            for (int i = 0; i < lbControllers.Length; ++i)
            {
                if (lbControllers[i].listView.DataCount == 0)
                    lbControllers[i].Reload();
                else
                    lbControllers[i].TryReloadLb();
            }
        }

        public void Show(int lbIndex)
        {
            scroller.SnapIndex = lbIndex;
            if (controller.isShowing == true)
                scroller.Snap();
            else
                scroller.SnapImmediately();

            Show();
        }

        public override void Hide()
        {
            controller.HideIfNot();
            IsShowing = false;
            onPanelStateChanged(this, false);
            for (int i = 0; i < lbControllers.Length; ++i)
            {
                lbControllers[i].deactivateTime = Time.time;
            }
            //CoroutineHelper.Instance.DoActionDelay(
            //    () =>
            //    {
            //        if (controller.isShowing != true)
            //        {
            //            LeaderboardBuilder.UnloadAllFlags();
            //        }
            //    },
            //    LeaderboardController.refreshThresholdMinutes * 2);
        }

        private void Start()
        {
            container.sizeDelta = new Vector2(Camera.main.aspect * referenceHeight, referenceHeight);

            for (int i = 0; i < closeButtons.Length; ++i)
            {
                closeButtons[i].onClick.AddListener(delegate
                {
                    Hide();
                });
            }
        }
    }
}