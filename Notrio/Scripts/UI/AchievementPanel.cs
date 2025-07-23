using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu.Achievements
{
    public class AchievementPanel : OverlayPanel
    {
        public OverlayGroupController controller;
        public Image badge;
        public Text bodyText;
        public Button closeButton;

        public override void Show()
        {
            controller.ShowIfNot();
            IsShowing = true;
            transform.BringToFront();
            onPanelStateChanged(this, true);
        }

        public override void Hide()
        {
            controller.HideIfNot();
            IsShowing = false;
            onPanelStateChanged(this, false);
        }

        private void Start()
        {
            closeButton.onClick.AddListener(delegate
            {
                Hide();
            });
        }

        public void SetAchievementInfo(AchievementInfo info)
        {
            bodyText.text = JsonUtility.ToJson(info).Replace(",", ",\n");
            badge.sprite = info.Badge;
        }

    }
}