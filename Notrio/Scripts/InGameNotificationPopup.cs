using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Pinwheel;
using Takuzu;

public class InGameNotificationPopup : MonoBehaviour {
    public static InGameNotificationPopup Instance;
    public Text toastText;
    public PositionAnimation animationController;
	[HideInInspector]
	public ConfirmationDialog confirmationDialog;
    private bool isShown = false;
    private void Awake()
    {
		if(UIReferences.Instance!=null){
			UpdateReferences();
		}
		UIReferences.UiReferencesUpdated += UpdateReferences;
		
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

	private void UpdateReferences()
	{
		confirmationDialog = UIReferences.Instance.overlayConfirmDialog;
	}

	private void OnDestroy() {
		UIReferences.UiReferencesUpdated += UpdateReferences;
	}

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void ShowToast(string toastStr, float duration)
    {
        gameObject.SetActive(true);
        if (isShown)
            return;
        isShown = true;
        Debug.Log(toastStr);
        toastText.text = toastStr;
        animationController.Play(animationController.curves[0]);
        StartCoroutine(AutoHideToast(duration));
    }

    private IEnumerator AutoHideToast(float duration)
    {
        yield return new WaitForSeconds(duration);
        animationController.Play(animationController.curves[1]);
        isShown = false;
        yield return new WaitForSeconds(animationController.duration);
        gameObject.SetActive(false);
    }
}
