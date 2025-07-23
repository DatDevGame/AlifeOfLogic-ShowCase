using System;
using System.Collections;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;
using UnityEngine.UI;

public class TipDetailPanel : MonoBehaviour {
    [HideInInspector]
    public TipInformationScriptableObject tipInformation;
    [HideInInspector]
    public RawImage puzzleImage;
    public Text titleText;
    [HideInInspector]
    public BoardLogical lb;
    [HideInInspector]
    public BoardVisualizer boardVisualizer;
    [HideInInspector]
    public BoardInstanceCameraController cameraController;
    public Text tipInfor;
    private bool m_requestRunning = false;
    public bool runningAnimation = false;
    private Color bgColor = new Color(0, 0, 0, 0);

    public RectTransform paternContainer;

    public RectTransform paternContainerL;
    public RectTransform paternContainerR;
    public RectTransform paternContainerC;
    public GameObject patern0Template;
    public GameObject patern1Template;
    public GameObject paternNoneTemplate;

    public bool RequestRunning
    {
        get
        {
            return m_requestRunning;
        }

        set
        {
            m_requestRunning = value;
            if (m_requestRunning && !runningAnimation)
                ActiveAnimation();
            else if (!m_requestRunning && runningAnimation)
                DeactiveAnimation();
        }
    }

    private void Start()
    {
        InitBoard();
    }

    private void Update()
    {
        if (runningAnimation)
        {
            puzzleImage.texture = cameraController.GetBoardTexture(1, bgColor);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        runningAnimation = false;
    }

    public void ActiveAnimation()
    {
        if (!tipInformation)
            return;
        CoroutineHelper.Instance.PostponeActionUntil(() =>
        {
            if(this!=null && !runningAnimation)
                StartCoroutine(AnimationCR());
        }, () => this!=null && gameObject.activeInHierarchy);
       
    }

    public void InitBoard()
    {
        lb.InitPuzzle(tipInformation.InitialPuzzle, tipInformation.puzzleSolution, true);
        tipInfor.text = tipInformation.TipText;
        titleText.text = tipInformation.tipTitle.ToUpper();
        if (tipInformation.paternList.Count > 0)
        {
            paternContainer.gameObject.SetActive(true);

            paternContainerC.gameObject.SetActive(false);
            paternContainerL.gameObject.SetActive(false);
            paternContainerR.gameObject.SetActive(false);

            if (tipInformation.paternList.Count == 0)
            {
                paternContainerC.gameObject.SetActive(true);
                InstantiatePaternObject(paternContainerC, tipInformation.paternList[0]);
            }
            else
            {
                paternContainerL.gameObject.SetActive(true);
                paternContainerR.gameObject.SetActive(true);
                InstantiatePaternObject(paternContainerL, tipInformation.paternList[0]);
                InstantiatePaternObject(paternContainerR, tipInformation.paternList[1]);
            }
        }
        else
        {
            paternContainer.gameObject.SetActive(false);
        }
    }

    private void InstantiatePaternObject(Transform container, string patern)
    {
        foreach(Transform trans in container.GetAllChildren())
        {
            Destroy(trans.gameObject);
        }
        foreach (var character in patern)
        {
            switch (character) {
                case '1':
                    Instantiate(patern1Template, container);
                    break;
                case '0':
                    Instantiate(patern0Template, container);
                    break;
                default:
                    Instantiate(paternNoneTemplate, container);
                    break;
            }
        }
    }

    private IEnumerator AnimationCR()
    {
        runningAnimation = true;
        puzzleImage.enabled = true;
        //titleText.text = tipInformation.tipTitle;
        while (true)
        {
            lb.ResetBoard();
            for (int i = 0; i < tipInformation.animationTimeline.Count; i++)
            {
                if(tipInformation.animationTimeline[i].deltaTime > 0)
                    yield return new WaitForSeconds(tipInformation.animationTimeline[i].deltaTime);
                switch (tipInformation.animationTimeline[i].action)
                {
                    case TipInformationScriptableObject.ActionType.HighLight:
                        boardVisualizer.HighlightCells(BoardLogical.ss2is(tipInformation.animationTimeline[i].additionalInfor, 6), tipInformation.animationTimeline[(i + 1 + tipInformation.animationTimeline.Count) % tipInformation.animationTimeline.Count].deltaTime);
                        break;
                    case TipInformationScriptableObject.ActionType.SetOne:
                        lb.SetValuesNoInteract(BoardLogical.ss2is(tipInformation.animationTimeline[i].additionalInfor, 6), 1);
                        break;
                    case TipInformationScriptableObject.ActionType.SetZero:
                        lb.SetValuesNoInteract(BoardLogical.ss2is(tipInformation.animationTimeline[i].additionalInfor, 6), 0);
                        break;
                    case TipInformationScriptableObject.ActionType.Unset:
                        lb.SetValuesNoInteract(BoardLogical.ss2is(tipInformation.animationTimeline[i].additionalInfor, 6), -1);
                        break;
                    case TipInformationScriptableObject.ActionType.SetAtive:
                        boardVisualizer.SetActiveCells(BoardLogical.ss2is(tipInformation.animationTimeline[i].additionalInfor, 6), tipInformation.animationTimeline[(i + 1 + tipInformation.animationTimeline.Count)%tipInformation.animationTimeline.Count].deltaTime);
                        break;
                    case TipInformationScriptableObject.ActionType.ClearInactive:
                        boardVisualizer.UnSetActiveCells(tipInformation.animationTimeline[(i + 1 + tipInformation.animationTimeline.Count) % tipInformation.animationTimeline.Count].deltaTime);
                        break;
                    default:
                        break;
                }
            }
            yield return null;
        }
    }

    public void DeactiveAnimation()
    {
        StopAllCoroutines();
        runningAnimation = false;
    }
}
