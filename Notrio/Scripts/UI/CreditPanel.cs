using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class CreditPanel : OverlayPanel
    {
        public UiGroupController controller;
        public Button closeButton;

        public OverlayPanel callingSource;

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
            if (callingSource != null)
            {
                callingSource.Show();
                callingSource = null;
            }
        }

        private void Start()
        {
            closeButton.onClick.AddListener(delegate
            {
                Hide();
            });
        }
    }
}