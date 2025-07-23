using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;
using Takuzu;
using System;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(TutorialOverlayUIController))]
public class RUSurePanelController : MonoBehaviour {
    private TutorialOverlayUIController mTutorialOverlayUIController;
    public Button yesButton;
    public Button noButton;
    public PositionAnimation panelAnimation;
    public GameObject container;
    private bool isShowing = false;
	// Use this for initialization
	void Start () {
        mTutorialOverlayUIController = GetComponent<TutorialOverlayUIController>();
        mTutorialOverlayUIController.tuttorialManager.onSkipButtonClicked += OnSkipButtonClick;
        yesButton.onClick.AddListener(delegate
        {
            if (SceneLoadingManager.Instance != null)
                SceneLoadingManager.Instance.LoadMainScene();
            else
                SceneManager.LoadScene("Main");
        });
        noButton.onClick.AddListener(delegate
        {
            Hide();
        });
	}

    private void OnDestroy()
    {
        mTutorialOverlayUIController.tuttorialManager.onSkipButtonClicked -= OnSkipButtonClick;
    }

    private void OnSkipButtonClick()
    {
        Show();
    }

    public void Show()
    {
        if (isShowing)
            return;
        isShowing = true;
        container.SetActive(true);
        mTutorialOverlayUIController.ShowingPanel++;
        panelAnimation.Play(panelAnimation.curves[0]);
    }

    public void Hide()
    {
        if (!isShowing)
            return;
        isShowing = false;
        mTutorialOverlayUIController.ShowingPanel--;
        panelAnimation.Play(panelAnimation.curves[1]);
        CoroutineHelper.Instance.DoActionDelay(()=>{
            container.SetActive(false);
        }, panelAnimation.duration);
    }

}
