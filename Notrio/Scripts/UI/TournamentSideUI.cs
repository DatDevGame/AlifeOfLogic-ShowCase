using System;
using System.Collections;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TournamentSideUI : MonoBehaviour {

    public UiGroupController controller;
    public Button homeBtn;
    public Button subscriptionBtn;
    public float showDelay;

    private Coroutine disableHomeBtnCR;

    private void Awake()
    {
        GameManager.GameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.GameStateChanged -= OnGameStateChanged;

#if UNITY_ANDROID
        StopDisableHomeBtn();
#endif
    }

#if UNITY_ANDROID
    private void OnDisable()
    {
        StopDisableHomeBtn();
    }
#endif

    private void Start()
    {
        homeBtn.onClick.AddListener(delegate
        {
            SceneLoadingManager.Instance.LoadMainScene();
        });
        if (subscriptionBtn != null)
        {
            subscriptionBtn.onClick.AddListener(() =>
            {
                if (UIReferences.Instance != null)
                    UIReferences.Instance.subscriptionDetailPanel.Show();
            });
        }
        controller.ShowIfNot();
        //DisplaySubscriptionUIGuide();
    }

    private void OnGameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing)
        {
            controller.HideIfNot();
        }
        else if (newState == GameState.Prepare)
        {
            CoroutineHelper.Instance.DoActionDelay(
                () =>
                {
                    if (GameManager.Instance.GameState == GameState.Prepare)
                    {
                        controller.ShowIfNot();
                    }
                },
                showDelay);
        }
    }
    public static string guiUISubscriptionSaveKey = "UI_GUI_SUBSCRIPTION_KEY";

    private void DisplaySubscriptionUIGuide()
    {
        CoroutineHelper.Instance.DoActionDelay(() =>
        {
            if (!PlayerPrefs.HasKey(guiUISubscriptionSaveKey))
            {
                Image btnImg = subscriptionBtn.GetComponent<Image>();
                Vector3[] worldConners = new Vector3[4];
                (btnImg.transform as RectTransform).GetWorldCorners(worldConners);
                float offsetFactor = Mathf.Abs(worldConners[1].y - worldConners[0].y);
                List<Image> detalSubButtonMaskedImage = new List<Image>();
                detalSubButtonMaskedImage.Add(btnImg);
                UIGuide.UIGuideInformation subscriptionUIGuideInformation = new UIGuide.UIGuideInformation(guiUISubscriptionSaveKey, detalSubButtonMaskedImage,
                    btnImg.gameObject, btnImg.gameObject, GameState.Prepare);
                subscriptionUIGuideInformation.clickableButton = subscriptionBtn;
                subscriptionUIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUI_SUBSCRIPTION;
                subscriptionUIGuideInformation.transformOffset = new Vector3(-5, offsetFactor * 0.75f, 0);

                UIGuide.instance.HighLightThis(subscriptionUIGuideInformation);
            }
        }, 0.2f);
    }

#if UNITY_ANDROID
    public void StartDisableHomeBtn()
    {
        if(homeBtn != null)
        {
            if (disableHomeBtnCR != null)
                StopCoroutine(disableHomeBtnCR);
            disableHomeBtnCR = StartCoroutine(CR_DisableHomeBtn());
        }
    }

    public void StopDisableHomeBtn()
    {
        if (homeBtn != null)
        {
            if (disableHomeBtnCR != null)
                StopCoroutine(disableHomeBtnCR);
        }
    }

    IEnumerator CR_DisableHomeBtn()
    {
        homeBtn.interactable = false;
        yield return new WaitForSeconds(3f);
        homeBtn.interactable = true;
    }
#endif
}
