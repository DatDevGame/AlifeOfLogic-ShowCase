using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Takuzu
{
    public class ConfirmationDialog : OverlayPanel
    {
        public UiGroupController controller;
        public Text title;
        public Text message;
        public Button closeButton;
        public Button yesButton;
        public Button noButton;
        public Text yesLabel;
        public Text noLabel;
        public RectTransform footer;
        public GameObject bg;
        public GameObject bgNoFooter;
        public Vector2 footerSize1Button;
        public Vector2 footerSize2Button;

        public static string YES_LABEL_DEFAULT { get { return I2.Loc.ScriptLocalization.YES; } }
        public static string NO_LABEL_DEFAULT { get { return I2.Loc.ScriptLocalization.NO; } }

        private Action confirmedAction;
        private Action declineAction;
        private Action cancelAction;
        private bool showCancelButton = true;

        public override void Show()
        {
            footer.gameObject.SetActive(confirmedAction != null || declineAction != null);
            bg.SetActive(confirmedAction != null || declineAction != null);
            bgNoFooter.SetActive(confirmedAction == null && declineAction == null);
            yesButton.gameObject.SetActive(confirmedAction != null);
            noButton.gameObject.SetActive(declineAction != null);
            closeButton.gameObject.SetActive(showCancelButton);

            footer.sizeDelta =
                (confirmedAction != null && declineAction != null) ? footerSize2Button :
                (confirmedAction != null || declineAction != null) ? footerSize1Button : Vector2.zero;

            controller.ShowIfNot();
            IsShowing = true;
            transform.BringToFront();
            onPanelStateChanged(this, true);
        }

        public override void Hide()
        {
            controller.HideIfNot();
            //gameObject.SetActive(false);
            IsShowing = false;
            confirmedAction = null;
            declineAction = null;
            cancelAction = null;
            //transform.SendToBack();
            onPanelStateChanged(this, false);
        }

        private void Awake()
        {
            closeButton.onClick.AddListener(delegate
            {
                if (cancelAction != null)
                    cancelAction();
                Hide();
            });

            yesButton.onClick.AddListener(delegate
            {
                if (confirmedAction != null)
                    confirmedAction();
                Hide();
            });

            noButton.onClick.AddListener(delegate
            {
                if (declineAction != null)
                    declineAction();
                Hide();
            });
            GameManager.ForceOutInGamScene += OnForceOutInGameScene;
            GameManager.GameStateChanged += OnGameStateChanged;
        }


        private void OnDestroy()
        {
            GameManager.ForceOutInGamScene -= OnForceOutInGameScene;
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        void OnForceOutInGameScene()
        {
            if (IsShowing)
                Hide();
        }

        private void OnGameStateChanged(GameState arg1, GameState arg2)
        {
            if (IsShowing)
                Hide();
        }

        public void Show(string t, string msg, Action confirm = null, Action decline = null, Action cancel = null)
        {
            title.text = t.ToUpper();
            message.text = msg;
            confirmedAction = confirm;
            declineAction = decline;
            yesLabel.text = YES_LABEL_DEFAULT.ToUpper();
            noLabel.text = NO_LABEL_DEFAULT.ToUpper();
            cancelAction = cancel;
            Show();
        }

        public void Show(string t, string msg, string yesLabelText, string noLabelText, Action confirm = null, Action decline = null, Action cancel = null, bool showCancelButton = true)
        {
            this.showCancelButton = showCancelButton;
            Show(t, msg, yesLabelText, confirm, noLabelText, decline, cancel);
        }
        public void Show(string t, string msg, string yesLabelText, Action confirm, string noLabelText, Action decline, Action cancel)
        {
            title.text = t.ToUpper();
            message.text = msg;
            confirmedAction = confirm;
            declineAction = decline;
            yesLabel.text = yesLabelText.ToUpper();
            noLabel.text = noLabelText.ToUpper();
            cancelAction = cancel;
            Show();
        }
    }
}