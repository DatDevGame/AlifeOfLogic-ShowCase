using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;
using Takuzu;
using System;

public class TutorialOverlayUIController : MonoBehaviour {
    public TutorialManager4 tuttorialManager;
    [Header("Local references")]
    public Image darkenImage;
    public ColorAnimation darkenImageAnimation;
    private int showingPanel = 0;
    private bool isShowing = false;

    public int ShowingPanel
    {
        get
        {
            return showingPanel;
        }

        set
        {
            showingPanel = value;
            if(showingPanel<= 0)
            {
                showingPanel = 0;
                Hide();
            }
            else
            {
                Show();
            }
        }
    }

    private void Start()
    {
        OverlayPanel.onPanelStateChanged += OnPanelStateChanged;
        darkenImage.enabled = false;
    }

    private void OnDestroy() {
        OverlayPanel.onPanelStateChanged -= OnPanelStateChanged;
    }

	private void OnPanelStateChanged(OverlayPanel OverlayPanel, bool show)
	{
		ShowingPanel += show? 1: -1;
        darkenImage.transform.SetAsLastSibling();
        OverlayPanel.transform.SetAsLastSibling();
	}

	public void Show()
    {
        if (isShowing)
            return;
        isShowing = true;
        darkenImage.enabled = true;
        darkenImageAnimation.Play(darkenImageAnimation.gradients[0]);
    }

    public void Hide()
    {
        if (!isShowing)
            return;
        isShowing = false;
        darkenImageAnimation.Play(darkenImageAnimation.gradients[1]);
        CoroutineHelper.Instance.DoActionDelay(
                    () =>
                    {
                        darkenImage.enabled = false;
                    },
                    darkenImageAnimation.duration);
    }
}
