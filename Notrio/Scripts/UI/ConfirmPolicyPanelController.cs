using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyMobile;

namespace Takuzu
{
    public class ConfirmPolicyPanelController : OverlayPanel
    {
        public static event System.Action CheckConfirmPolicyComplete = delegate { };

        public UiGroupController controller;
        public Button policyBtn;
        public Button termsBtn;
        public Button acceptBtn;

        public static string CONFIRM_POLICY_KEY = "CONFIRM_POLICY_SAVE_KEY";

        private void Start()
        {
            StartCoroutine(CR_CheckRegion());
            acceptBtn.onClick.AddListener(delegate
            {
                PlayerPrefs.SetInt(CONFIRM_POLICY_KEY, 1);
                CheckConfirmPolicyComplete();
                Hide();
            });

            policyBtn.onClick.AddListener(() =>
            {
                Application.OpenURL(AppInfo.Instance.PRIVACY_POLICY_LINK);
            });

            termsBtn.onClick.AddListener(() =>
            {
                Application.OpenURL(AppInfo.Instance.TERMS_OF_SERVICE_LINK);
            });
        }

        IEnumerator CR_CheckRegion()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (PlayerPrefs.GetInt(CONFIRM_POLICY_KEY, 0) == 0)
            {
                yield return new WaitForSeconds(1);
                Show();
            }
            else
            {
                CheckConfirmPolicyComplete();
            }
        }

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
    }
}
