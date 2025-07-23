using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinwheel;
using UnityEngine.UI;

namespace Takuzu
{
    public class RuleTwoWalkthroughStep : WalkthroughStep
    {
        public PositionAnimation cameraPositionAnimation;
        private LogicalBoardTutorial m_lb;
        public string puzzle = ".00111000.1.0110";
        public string solution = "1001110000110110";
        public StepTutorial[] stepTutorialList;
        private int currentStep = 0;
        private bool isPassStep = false;
        [Header("UI reference")]
        public UIScriptAnimationManager Header1GameObject;
        public UIScriptAnimationManager Header2GameObject;
        public UIScriptAnimationManager InstructionGameObject;
        public UIScriptAnimationManager Instruction2GameObject;
        public UIScriptAnimationManager Instruction3GameObject;
        public UIScriptAnimationManager InstructionPairObject;
        public Text ruleTitle;
        public Text headerTxt2;
        public Text instructionText1;
        public Text instructionText3;
        public Text instructionPairTxt;
        private Coroutine checkStateCoroutine, checkTouchCoroutine;
        private Coroutine showCoroutine;

        public void OnEnable()
        {
            LogicalBoard.onCellClicked += OnCellClicked;
            ruleTitle.text = string.Format(I2.Loc.ScriptLocalization.RULE_NAME.ToUpper(), 2);
            headerTxt2.text = string.Format(I2.Loc.ScriptLocalization.Rule_Description_2, 0, 1);
            instructionPairTxt.text = string.Format(I2.Loc.ScriptLocalization.Pair_Notice, 1);
        }

        public void OnDisable()
        {
            LogicalBoard.onCellClicked -= OnCellClicked;
        }

        //public void Awake()
        //{
        //    transform.localScale = ((float)Screen.height / Screen.width >= 1.95f ? 1 : 1.15f) * Vector3.one;
        //}

        public void OnCellClicked(Index2D index2D)
        {
            if (checkTouchCoroutine != null)
                StopCoroutine(checkTouchCoroutine);
            checkTouchCoroutine = StartCoroutine(CR_WaitCheckCompleteStep());
        }

        IEnumerator CR_WaitCheckCompleteStep()
        {
            yield return new WaitForSeconds(0.5f);
            CheckCompleteStep(currentStep);
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
                            instructionText1.text = string.Format(I2.Loc.ScriptLocalization.Unequal, 0, 1);
                        }
                        else
                        {
                            //instructionText3.text = "You are violating rule 2!";
                            instructionText3.text = I2.Loc.ScriptLocalization.Violate_Rule_2;
                        }
                    }
                    else
                    {
                        if (currentStep == 0)
                        {
                            //instructionText1.text = string.Format("To satisfy this rule,\n square {0} must be a 1.", stepTutorialList[currentStep].intereactCell[0]);
                            instructionText1.text = string.Format(I2.Loc.ScriptLocalization.Instruction_Rule2_1, stepTutorialList[currentStep].intereactCell[0], 1);
                        }
                        else
                        {
                            //instructionText3.text = string.Format("Can you fill square {0}?", stepTutorialList[currentStep].intereactCell[0]);
                            instructionText3.text = string.Format(I2.Loc.ScriptLocalization.Instruction_Rule_2_2, stepTutorialList[currentStep].intereactCell[0]);
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

        public override void StartWalkthrough(TutorialManager4 tutorialManager)
        {
            base.StartWalkthrough(tutorialManager);
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(CR_RuleTwoWalkthrough());
        }

        private IEnumerator CR_RuleTwoWalkthrough()
        {
            yield return null;
            Index2D[] intereactableIndexes = null;
            m_lb = base.tutorialManager.RequestNewLogicalBoard(puzzle, solution, intereactableIndexes);

            yield return new WaitUntil(() =>
            {
                return !cameraPositionAnimation.isAnimationRunning;
            });

            Header1GameObject.FadeIn(0.5f);
            yield return new WaitForSeconds(0.25f);
            Header2GameObject.FadeIn(0.5f);

            StartCoroutine(CR_StateRowsAndColumns(0.5f, currentStep));
            //instructionText1.text = string.Format("To satisfy this rule,\n square {0} must be a 1.", stepTutorialList[currentStep].intereactCell[0]);
            instructionText1.text = string.Format(I2.Loc.ScriptLocalization.Instruction_Rule2_1, stepTutorialList[currentStep].intereactCell[0], 1);
            yield return new WaitForSeconds(0.6f);
            InstructionGameObject.FadeIn(0.5f);
            yield return new WaitForSeconds(0.75f);
            List<Index2D> index2DList = new List<Index2D>();
            for (int i = 0; i < stepTutorialList[currentStep].intereactCell.Length; i++)
            {
                index2DList.Add(base.tutorialManager.s2i(stepTutorialList[currentStep].intereactCell[i], (int)Mathf.Sqrt(puzzle.Length)));
            }
            checkStateCoroutine = StartCoroutine(CR_CheckChangeCellValue(index2DList, stepTutorialList[currentStep].intereactCell.ToList()));
            yield return new WaitForSeconds(0.25f);
            m_lb.SetInteractableIndex(index2DList.ToArray());
            yield return new WaitUntil(() => isPassStep);

            if (checkStateCoroutine != null)
                StopCoroutine(checkStateCoroutine);
            if (checkTouchCoroutine != null)
                StopCoroutine(checkTouchCoroutine);

            InstructionGameObject.FadeOut(0.3f);
            yield return new WaitForSeconds(0.4f);
            InstructionPairObject.FadeIn(0.5f);
            yield return new WaitForSeconds(2.5f);
            InstructionPairObject.FadeOut(0.3f);
            yield return new WaitForSeconds(0.4f);
            VisualBoard.Instance.SetInActiveRows(stepTutorialList[1].setRows);
            //Complete step 1 of rule 2
            isPassStep = false;
            currentStep++;
            InstructionGameObject.FadeOut(0.25f);
            yield return new WaitForSeconds(0.3f);
            //instructionText3.text = string.Format("Can you fill square {0}?", stepTutorialList[currentStep].intereactCell[0]);
            instructionText3.text = string.Format(I2.Loc.ScriptLocalization.Instruction_Rule_2_2, stepTutorialList[currentStep].intereactCell[0]);
            index2DList.Clear();
            for (int i = 0; i < stepTutorialList[currentStep].intereactCell.Length; i++)
            {
                index2DList.Add(base.tutorialManager.s2i(stepTutorialList[currentStep].intereactCell[i], (int)Mathf.Sqrt(puzzle.Length)));
            }
            VisualBoard.Instance.SetActiveColumns(stepTutorialList[currentStep].setColumns);
            checkStateCoroutine = StartCoroutine(CR_CheckChangeCellValue(index2DList, stepTutorialList[currentStep].intereactCell.ToList()));
            yield return new WaitForSeconds(0.25f);
            Instruction3GameObject.FadeIn(0.5f);
            m_lb.SetInteractableIndex(index2DList.ToArray());
            yield return new WaitUntil(() => isPassStep);

            if (checkStateCoroutine != null)
                StopCoroutine(checkStateCoroutine);
            if (checkTouchCoroutine != null)
                StopCoroutine(checkTouchCoroutine);

            if (VisualBoard.Instance != null)
                VisualBoard.Instance.HideHandUI();
            if (checkStateCoroutine != null)
                StopCoroutine(checkStateCoroutine);
            if (VisualBoard.Instance != null)
                VisualBoard.Instance.ClearInActiveCells();

            InstructionGameObject.FadeOut(0.4f);
            Instruction3GameObject.FadeOut(0.3f);
            Header1GameObject.FadeOut(0.5f);
            Header2GameObject.FadeOut(0.5f);
            yield return new WaitForSeconds(0.5f);
            //base.tutorialManager.PlayLeavesParticle(1.5f);
            if (SoundManager.Instance)
                SoundManager.Instance.PlaySound(SoundManager.Instance.rewarded, true);
            Instruction2GameObject.FadeIn(0.3f);

            yield return new WaitForSeconds(0.75f);
            Instruction2GameObject.FadeOut(0.3f);

            #region Analytics Event
            AlolAnalytics.Tutorial(4);
            #endregion

            yield return new WaitForSeconds(0.3f);
            base.isFinished = true;
            gameObject.SetActive(false);
        }

        IEnumerator DelayHideInstruction(UIScriptAnimationManager uiAni, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            uiAni.FadeOut(0.5f);
        }

        IEnumerator DelayShowInstruction(UIScriptAnimationManager uiAni, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            uiAni.FadeIn(0.5f);
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
    }
}
