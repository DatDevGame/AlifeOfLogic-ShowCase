using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class TaskPanel : OverlayPanel
    {
        public UiGroupController controller;
        public Text taskText;
        public Slider progressSlider;
        public Image powerByGiphy;
        public Image powerByEM;

        public override void Show()
        {
            IsShowing = true;
            controller.ShowIfNot();
            transform.BringToFront();
            onPanelStateChanged(this, true);
        }

        public override void Hide()
        {
            IsShowing = false;
            controller.HideIfNot();
            onPanelStateChanged(this, false);
        }

        public void SetTask(string t)
        {
            taskText.text = t;
        }

        public void SetProgress(float p)
        {
            progressSlider.value = p;
        }

        public void SetGiphyActive(bool active)
        {
            powerByGiphy.gameObject.SetActive(active);
        }

        public void SetEMActive(bool active)
        {
            powerByEM.gameObject.SetActive(active);
        }
    }
}