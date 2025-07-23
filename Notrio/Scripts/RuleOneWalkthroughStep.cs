using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinwheel;
using UnityEngine.UI;
using System;

[System.Serializable]
public struct StepTutorial
{
    public string[] intereactCell;
    public int[] valueInteractCell;
    public bool isActiveRow;
    public int[] setRows;
    public bool isActiveColoumns;
    public int[] setColumns;
}

namespace Takuzu
{
    public class RuleOneWalkthroughStep : WalkthroughStep
    {
        public PositionAnimation cameraPositionAnimation;
        public string puzzle = ".00111000.1.011.";
        public string solution = "1001110000110110";
        public StepTutorial[] stepTutorialList;
        private LogicalBoardTutorial m_lb;
        [Header("UI References")]
        public UIScriptAnimationManager Header1TextGameObject;
        public UIScriptAnimationManager Header2TextGameObject;
        public UIScriptAnimationManager InstructionTextGameObject;
        public UIScriptAnimationManager InstructionText2GameObject;
        public UIScriptAnimationManager InstructionText3GameObject;
        public Text ruleTitle;
        public Text instructionText1;
        public Text instructionText2;
        [Header("Config")]
        private Coroutine checkStateCoroutine, checkTouchCoroutine;
        private int currentStep = 0;
        private bool isPassStep = false;

        public void Awake()
        {
            ruleTitle.text = string.Format(I2.Loc.ScriptLocalization.RULE_NAME.ToUpper(), 1);
        }

        public void OnEnable()
        {
            LogicalBoard.onCellClicked += OnCellClicked;
        }

        public void OnDisable()
        {
            LogicalBoard.onCellClicked -= OnCellClicked;
        }

        public void OnCellClicked(Index2D index2D)
        {
            if (checkTouchCoroutine != null)
                StopCoroutine(checkTouchCoroutine);
            checkTouchCoroutine = StartCoroutine(CR_WaitCheckCompleteStep());
        }

        public override void StartWalkthrough(TutorialManager4 tutorialManager)
        {
            base.StartWalkthrough(tutorialManager);
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(CR_RuleOneWalkthrough());
        }

        private IEnumerator CR_RuleOneWalkthrough()
        {
            yield return null;
            Index2D[] intereactableIndexes = null;
            m_lb = base.tutorialManager.RequestNewLogicalBoard(puzzle, solution, intereactableIndexes);
            yield return new WaitUntil(() =>
            {
                return !cameraPositionAnimation.isAnimationRunning;
            });

            yield return null;
            Header1TextGameObject.FadeIn(0.5f);
            yield return new WaitForSeconds(0.25f);
            Header2TextGameObject.FadeIn(0.5f);
            m_lb.SetInteractableIndex();
            yield return new WaitForSeconds(0.6f);

            if (VisualBoard.Instance != null)
            {
                VisualBoard.Instance.SetInActiveRows(new int[6] { 0, 1, 2, 3, 4, 5 });
            }
            StartCoroutine(CR_StateRowsAndColumns(1.5f, currentStep));
            yield return new WaitForSeconds(0.6f);
            InstructionTextGameObject.FadeIn(0.5f);
            //instructionText1.text = String.Format("Square {0} must be a 0.\n Tap it once.", stepTutorialList[currentStep].intereactCell[0]);
            instructionText1.text = String.Format(I2.Loc.ScriptLocalization.Instruction_Rule_1_1, stepTutorialList[currentStep].intereactCell[0], 0);
            List<Index2D> index2DList = new List<Index2D>();
            for (int i = 0; i < stepTutorialList[currentStep].intereactCell.Length; i++)
            {
                index2DList.Add(base.tutorialManager.s2i(stepTutorialList[currentStep].intereactCell[i], (int)Mathf.Sqrt(puzzle.Length)));
            }
            yield return new WaitForSeconds(0.25f);
            m_lb.SetInteractableIndex(index2DList.ToArray());
            checkStateCoroutine = StartCoroutine(CR_CheckChangeCellValue(index2DList,
                stepTutorialList[currentStep].intereactCell.ToList()));

            yield return new WaitUntil(() => isPassStep);

            // complete step 1 of rule 1 tutorial
            if (checkStateCoroutine != null)
                StopCoroutine(checkStateCoroutine);
            if (checkTouchCoroutine != null)
                StopCoroutine(checkTouchCoroutine);

            isPassStep = false;
            InstructionTextGameObject.FadeOut(0.25f);
            yield return new WaitForSeconds(0.1f);
            VisualBoard.Instance.SetInActiveColumns(stepTutorialList[currentStep].setColumns);
            yield return new WaitForSeconds(0.1f);
            currentStep++;
            //instructionText2.text = String.Format("Similarly, square {0} must be a 1.\n Tap it twice.", stepTutorialList[currentStep].intereactCell[0]);
            instructionText2.text = String.Format(I2.Loc.ScriptLocalization.Instruction_Rule_1_2, stepTutorialList[currentStep].intereactCell[0], 1);
            StartCoroutine(CR_StateRowsAndColumns(0.3f, currentStep));
            yield return new WaitForSeconds(0.5f);
            InstructionText2GameObject.FadeIn(0.5f);
            index2DList.Clear();
            index2DList = new List<Index2D>();
            for (int i = 0; i < stepTutorialList[currentStep].intereactCell.Length; i++)
            {
                index2DList.Add(base.tutorialManager.s2i(stepTutorialList[currentStep].intereactCell[i], (int)Mathf.Sqrt(puzzle.Length)));
            }
            m_lb.SetInteractableIndex(index2DList.ToArray());
            checkStateCoroutine = StartCoroutine(CR_CheckChangeCellValue(index2DList,
                stepTutorialList[currentStep].intereactCell.ToList()));
            yield return new WaitUntil(() => isPassStep);

            //complete rule 1 tutorial
            if (checkStateCoroutine != null)
                StopCoroutine(checkStateCoroutine);
            if (checkTouchCoroutine != null)
                StopCoroutine(checkTouchCoroutine);

            StartCoroutine(DelayHideInstruction(InstructionTextGameObject, 0));
            InstructionTextGameObject.FadeOut(0.5f);
            if (VisualBoard.Instance != null)
                VisualBoard.Instance.HideHandUI();
            if (VisualBoard.Instance != null)
                VisualBoard.Instance.ClearInActiveCells();
            InstructionText2GameObject.FadeOut(0.4f);
            Header1TextGameObject.FadeOut(0.5f);
            Header2TextGameObject.FadeOut(0.5f);
            yield return new WaitForSeconds(0.5f);

            if (SoundManager.Instance)
                SoundManager.Instance.PlaySound(SoundManager.Instance.rewarded, true);
            InstructionText3GameObject.FadeIn(0.3f);
            yield return new WaitForSeconds(0.75f);
            InstructionText3GameObject.FadeOut(0.3f);

            #region Analytics Event
            AlolAnalytics.Tutorial(3);
            #endregion

            yield return new WaitForSeconds(0.3f);
            base.isFinished = true;
            gameObject.SetActive(false);


        }

        void CheckCompleteStep(int stepIndex)
        {
            for (int i = 0; i < stepTutorialList[currentStep].intereactCell.Length; i++)
                if (m_lb.GetValue(base.tutorialManager.s2i(stepTutorialList[currentStep].intereactCell[i], (int)Mathf.Sqrt(puzzle.Length))) != stepTutorialList[stepIndex].valueInteractCell[i])
                {
                    if (m_lb.GetValue(base.tutorialManager.s2i(stepTutorialList[currentStep].intereactCell[i], (int)Mathf.Sqrt(puzzle.Length))) != LogicalBoard.VALUE_EMPTY)
                    {
                        if (currentStep == 0)
                        {
                            //instructionText1.text = "We can't have 3 same\nnumbers next to each other!";
                            instructionText1.text = I2.Loc.ScriptLocalization.Three_Number;
                        }
                        else
                        {
                            RectTransform rect = InstructionText2GameObject.GetComponent<RectTransform>();
                            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 50);
                            //instructionText2.text = "You are violating rule 1!";
                            instructionText2.text = I2.Loc.ScriptLocalization.Violate_Rule_1;
                        }
                    }
                    else
                    {
                        if (currentStep == 0)
                        {
                            instructionText1.text = String.Format(I2.Loc.ScriptLocalization.Instruction_Rule_1_1, stepTutorialList[currentStep].intereactCell[0], 0);
                        }
                        else
                        {
                            RectTransform rect = InstructionText2GameObject.GetComponent<RectTransform>();
                            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 75);
                            instructionText2.text = String.Format(I2.Loc.ScriptLocalization.Instruction_Rule_1_2, stepTutorialList[currentStep].intereactCell[0], 1);
                        }
                    }
                    isPassStep = false;
                    return;
                }
            m_lb.SetInteractableIndex();
            isPassStep = true;
        }

        IEnumerator CR_StateRowsAndColumns(float seconds, int stepIndex)
        {
            yield return new WaitForSeconds(seconds);
            if (stepTutorialList[stepIndex].isActiveRow)
                VisualBoard.Instance.SetActiveRows(stepTutorialList[stepIndex].setRows);
            else
                VisualBoard.Instance.SetInActiveRows(stepTutorialList[stepIndex].setRows);
            if (stepTutorialList[stepIndex].isActiveColoumns)
                VisualBoard.Instance.SetActiveColumns(stepTutorialList[stepIndex].setColumns);
            else
                VisualBoard.Instance.SetInActiveRows(stepTutorialList[stepIndex].setColumns);
        }

        IEnumerator CR_CheckChangeCellValue(List<Index2D> listIndex, List<string> intereactCellList)
        {
            if (listIndex != null)
            {
                float t = 0;
                float timeNonTouch = 3;
                int[] preState = new int[intereactCellList.Count];
                for (int i = 0; i < intereactCellList.Count; i++)
                    preState[i] = m_lb.GetValue(base.tutorialManager.s2i(intereactCellList[i], (int)Mathf.Sqrt(puzzle.Length)));

                while (true)
                {
                    if (VisualBoard.Instance.GetHandState())
                    {
                        int count = 0;
                        for (int i = 0; i < intereactCellList.Count; i++)
                        {
                            if (preState[i] != m_lb.GetValue(base.tutorialManager.s2i(intereactCellList[i], (int)Mathf.Sqrt(puzzle.Length))))
                                break;
                            count++;
                        }
                        if (count < intereactCellList.Count)
                        {
                            VisualBoard.Instance.HideHandUI();
                            for (int i = 0; i < intereactCellList.Count; i++)
                                preState[i] = m_lb.GetValue(base.tutorialManager.s2i(intereactCellList[i], (int)Mathf.Sqrt(puzzle.Length)));
                        }
                    }
                    else
                    {
                        t = 0;
                        while (t < timeNonTouch)
                        {
                            t += Time.deltaTime;
                            for (int i = 0; i < intereactCellList.Count; i++)
                            {
                                if (preState[i] != m_lb.GetValue(base.tutorialManager.s2i(intereactCellList[i], (int)Mathf.Sqrt(puzzle.Length))))
                                {
                                    t = 0;
                                    for (int j = 0; j < intereactCellList.Count; j++)
                                        preState[j] = m_lb.GetValue(base.tutorialManager.s2i(intereactCellList[j], (int)Mathf.Sqrt(puzzle.Length)));
                                }
                            }
                            yield return null;
                        }

                        if (t >= timeNonTouch)
                            VisualBoard.Instance.ShowHandUI(listIndex);
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        IEnumerator CR_WaitCheckCompleteStep()
        {
            yield return new WaitForSeconds(0.5f);
            CheckCompleteStep(currentStep);
        }

        IEnumerator DelayHideInstruction(UIScriptAnimationManager uiAni, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            uiAni.FadeOut(0.5f);
        }
    }
}
