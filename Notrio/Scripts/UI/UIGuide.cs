using System;
using System.Collections;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;
using UnityEngine.UI;

public class UIGuide : MonoBehaviour
{
    public static UIGuide instance;

    [Serializable]
    public class UIGuideInformation
    {
        public string SaveKey = "SaveKey";
        public bool matchXPosition = true;
        public bool matchYPosition = true;
        public Vector3 transformOffset = Vector3.zero;
        public UiGroupController controller;
        public string message= "";
        public Transform container = null;
        [HideInInspector]
        public bool isShowing = false;
        public List<Image> BgImage = new List<Image>();
        [HideInInspector]
        public GameObject targetObject;
        [HideInInspector]
        public Button clickableButton;
        public Coroutine highLightCR = null;
        [HideInInspector]
        public float lastShowTime;
        public List<GameState> gameState;
        [HideInInspector]
        public GameObject highLightTarget;
        [HideInInspector]
        public float bubleTextWidth = 400;

        public UIGuideInformation(string saveKey, List<Image> maskedImage, GameObject targetObject, GameObject highLightTarget, List<GameState> gameState)
        {
            this.SaveKey = saveKey;
            this.BgImage = maskedImage;
            this.targetObject = targetObject;
            this.gameState = gameState;
            this.highLightTarget = highLightTarget;
        }
        public UIGuideInformation(string saveKey, List<Image> maskedImage, GameObject targetObject, GameObject highLightTarget, GameState gameState)
        {
            this.SaveKey = saveKey;
            this.BgImage = maskedImage;
            this.targetObject = targetObject;
            this.gameState = new List<GameState>();
            this.gameState.Add(gameState);
            this.highLightTarget = highLightTarget;
        }
    }

    public List<UIGuideInformation> guideList;
    [HideInInspector]
    public bool isShownGuide = false;
    public bool isWaitingGuide = false;
    [HideInInspector]
    public bool isReady = false;
    public GameObject bubbleTextTemplate;
    public Image darkenImg;
    public Image UIBlocker;
    public Transform bubbleTextContainer;
    public Button UIBlockerBtn;
    public Material hightlightMat;
    public float minimalShowTime = 5;
    public List<UIGuideInformation> guideQueue = new List<UIGuideInformation>();
    private UIGuideInformation currentGuide = null;
    private bool showingQueueRunning = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            DestroyImmediate(gameObject);
        HideAll();
    }

    private void Start()
    {
        UIBlockerBtn.onClick.AddListener(() =>
        {
            InvokeCurrentHighLightButton();
            HideAndShowNext();
        });
        GameManager.GameStateChanged += OnGameStateChanged;
        GameManager.ForceOutInGamScene += OnForceOutInGameScene;
    }

    void OnForceOutInGameScene()
    {
        HideAll();
    }

    private void InvokeCurrentHighLightButton()
    {
        var mousePosition = Input.mousePosition;
        if (currentGuide != null && Time.time - currentGuide.lastShowTime > 1 && currentGuide.isShowing && currentGuide.clickableButton!=null)
            if ( RectTransformUtility.RectangleContainsScreenPoint(currentGuide.clickableButton.transform as RectTransform, Input.mousePosition, Camera.main))
            {
                currentGuide.clickableButton.onClick.Invoke();
            }
    }

    private void OnDestroy()
    {
        GameManager.GameStateChanged -= OnGameStateChanged;
        GameManager.ForceOutInGamScene -= OnForceOutInGameScene;
    }

    private void OnGameStateChanged(GameState arg1, GameState arg2)
    {
        if (currentGuide != null)
            guideQueue.Insert(0, currentGuide);
        HideAll();
        if (!showingQueueRunning)
            ShowQueue();
    }

    private void HideAndShowNext()
    {
        if (currentGuide != null && Time.time - currentGuide.lastShowTime > 1 && currentGuide.isShowing)
            HideThis(currentGuide);
    }

    private void HideAll()
    {
        for (int i = 0; i < guideList.Count; i++)
        {
            if (guideList[i].container)
                guideList[i].container.gameObject.SetActive(false);
            guideList[i].isShowing = false;
            guideList[i].highLightCR = null;
            if (guideList[i].controller)
                guideList[i].controller.HideIfNot();
            foreach (var image in guideList[i].BgImage)
            {
                if (image)
                    image.material = null;
            }
        }
        showingQueueRunning = false;
        isShownGuide = false;
        isWaitingGuide = false;
        currentGuide = null;
        UpdateDarkenImgAndBlockUI();
        isReady = true;
    }

    public void HighLightThis(UIGuideInformation UIGuideInfo)
    {
        StartCoroutine(WaitToDisableAllPopup(UIGuideInfo));
    }
    private IEnumerator WaitToDisableAllPopup(UIGuideInformation UIGuideInfo)
    {
        if (UIGuideInfo.highLightTarget == null || !UIGuideInfo.highLightTarget.activeInHierarchy)
            yield break;
        if (UIGuideInfo.highLightCR == null && !IsGuildeShown(UIGuideInfo.SaveKey))
        {
            if (guideQueue.FindIndex(item => item.SaveKey == UIGuideInfo.SaveKey) == -1 && (currentGuide== null || currentGuide.SaveKey != UIGuideInfo.SaveKey))
            {
                int index = guideList.FindIndex(item => item.SaveKey == UIGuideInfo.SaveKey);
                if (index == -1)
                    guideList.Add(UIGuideInfo);
                else
                {
                    if (guideList[index].container != null && guideList[index].container != UIGuideInfo.container)
                        DestroyImmediate(guideList[index].container.gameObject);
                    guideList[index] = UIGuideInfo;
                }

                guideQueue.Add(UIGuideInfo);
            }

            if (guideList.FindIndex(item => (item.controller != null && item.controller.isShowing == true)) == -1)
            {
                if (!showingQueueRunning)
                    ShowQueue();
            }
        }
    }

    internal bool IsGuildeShown(string uIGuideControllKey)
    {
        return PlayerDb.HasKey("UNLOCK-UIGuide_" + uIGuideControllKey) || PlayerPrefs.HasKey("UNLOCK-UIGuide_" + uIGuideControllKey);
    }

    private void SetGuideShown(string uIGuideControllKey)
    {
        PlayerDb.SetInt("UNLOCK-UIGuide_" + uIGuideControllKey, 1);
        PlayerPrefs.SetInt("UNLOCK-UIGuide_" + uIGuideControllKey, 1);
    }

    private void ShowQueue()
    {
        int indexGuide = guideQueue.FindIndex(item => item.gameState.Contains(GameManager.Instance.GameState));
        if (indexGuide != -1)
        {
            guideQueue[indexGuide].highLightCR = StartCoroutine(HigheLightCR(guideQueue[indexGuide], !showingQueueRunning));
            currentGuide = guideQueue[indexGuide];
            guideQueue.RemoveAt(indexGuide);
            showingQueueRunning = true;
        }
        else
        {
            currentGuide = null;
            showingQueueRunning = false;
            UpdateDarkenImgAndBlockUI();
        }
    }

    private IEnumerator HigheLightCR(UIGuideInformation uIGuideInformation, bool firstQ)
    {
        uIGuideInformation.isShowing = true;
        EnableUIBlocker();
        UIBlockerBtn.interactable = false;
        uIGuideInformation.lastShowTime = Time.time;
        if (firstQ)
            yield return new WaitForSeconds(0.5f);
        if (!isReady)
            yield return new WaitUntil(() => isReady);

        if(UIReferences.Instance.gameUiPackSelectionUI != null)
            yield return new WaitUntil(() => UIReferences.Instance.gameUiPackSelectionUI.scroller.isScrolling == false);

        isWaitingGuide = true;
        if (UIReferences.Instance.overlayUIController.ShowingPanelCount > 0)
        {
            darkenImg.gameObject.SetActive(false);
            yield return new WaitUntil(() => UIReferences.Instance.overlayUIController.ShowingPanelCount == 0);
            if (firstQ)
                yield return new WaitForSeconds(3);
        }
        else
        {
            if (firstQ)
                yield return new WaitForSeconds(1);
        }
        isWaitingGuide = false;


        uIGuideInformation.lastShowTime = Time.time;
        UIBlockerBtn.interactable = true;
        DisableUIBlocker();
        if (uIGuideInformation.highLightTarget && uIGuideInformation.highLightTarget.activeInHierarchy && !IsGuildeShown(uIGuideInformation.SaveKey)&& uIGuideInformation.gameState.Contains(GameManager.Instance.GameState))
        {
            uIGuideInformation.lastShowTime = Time.time;
            foreach (var image in uIGuideInformation.BgImage)
            {
                if(image)
                    image.material = hightlightMat;
            }
            if(uIGuideInformation.container == null)
            {
                GameObject bubbleText = Instantiate(bubbleTextTemplate, bubbleTextContainer);
                bubbleText.GetComponentInChildren<Text>().text = uIGuideInformation.message;
                uIGuideInformation.container = bubbleText.transform;
            }
            BubbleText bText = uIGuideInformation.container.GetComponent<BubbleText>();
            bText.SetTarget(uIGuideInformation.targetObject, uIGuideInformation.transformOffset);
            bText.SetSize(uIGuideInformation.bubleTextWidth);
            uIGuideInformation.controller = uIGuideInformation.container.GetChild(0).GetComponent<UiGroupController>();
            uIGuideInformation.container.gameObject.SetActive(true);
            Vector3 position = uIGuideInformation.container.position;
            uIGuideInformation.container.SetParent(bubbleTextContainer);
            uIGuideInformation.container.position = position;
            isShownGuide = true;
            UpdateDarkenImgAndBlockUI();
            MimicTransform mimic = uIGuideInformation.container.GetComponent<MimicTransform>();
            mimic.SetTargetTransform(uIGuideInformation.targetObject.transform);
            mimic.MimicPositionX = uIGuideInformation.matchXPosition;
            mimic.MimicPositionY = uIGuideInformation.matchYPosition;
            mimic.offset = uIGuideInformation.transformOffset;
            uIGuideInformation.controller.ShowIfNot();
            //HideDelay(uIGuideInformation, 10);
        }
        else
        {
            yield return null;
            uIGuideInformation.isShowing = false;
            uIGuideInformation.highLightCR = null;
            if(uIGuideInformation.container)
                uIGuideInformation.container.gameObject.SetActive(false);
            isShownGuide = guideList.FindIndex(item => (item.controller != null && item.controller.isShowing == true)) != -1;
            if (!guideQueue.Contains(uIGuideInformation) && !IsGuildeShown(uIGuideInformation.SaveKey))
            {
                guideQueue.Add(uIGuideInformation);
            }
            ShowQueue();
        }
    }

    private void HideThis(UIGuideInformation uIGuideInformation)
    {
        uIGuideInformation.isShowing = false;
        uIGuideInformation.controller.HideIfNot();
        SetGuideShown(uIGuideInformation.SaveKey);
        StartCoroutine(HideGuideUIDelayCR(uIGuideInformation));
        
        if(uIGuideInformation.SaveKey == "FirstError")
            UIReferences.Instance.rulePopUpController.FadeRulePopUp(true, 0.1f);
    }

    private IEnumerator HideGuideUIDelayCR(UIGuideInformation uIGuideInformation)
    {
        yield return new WaitForSeconds(uIGuideInformation.controller.MaxDuration);
        foreach (var image in uIGuideInformation.BgImage)
        {
            if(image)
                image.material = null;
        }
        uIGuideInformation.highLightCR = null;
        if (uIGuideInformation.container)
            uIGuideInformation.container.gameObject.SetActive(false);
        isShownGuide = guideList.FindIndex(item => (item.controller!=null && item.controller.isShowing == true)) != -1;
        ShowQueue();
    }

    private void UpdateDarkenImgAndBlockUI()
    {
        darkenImg.gameObject.SetActive(isShownGuide);
        UIBlocker.gameObject.SetActive(isShownGuide);
    }

    public void EnableUIBlocker()
    {
        UIBlocker.gameObject.SetActive(true);
    }

    public void DisableUIBlocker()
    {
        UIBlocker.gameObject.SetActive(false);
    }
}
