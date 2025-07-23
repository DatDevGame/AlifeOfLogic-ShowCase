using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinwheel;
using System;
using UnityEngine.UI;

namespace Takuzu
{
    public class RuleThreeWalkthroughStep : WalkthroughStep
    {
        public PositionAnimation cameraPositionAnimation;
        private LogicalBoardTutorial m_lb;
        public string puzzle = "100111000.1.0110";
        public string solution = "1001110000110110";
        public string intereactCell1 = "2B";
        public int correctIntereactableCell1Value = LogicalBoard.VALUE_ONE;
        public string intereactCell2 = "2D";
        public int correctIntereactableCell2Value = LogicalBoard.VALUE_ZERO;
        public List<int> setInactiveRows = new List<int>();
        public List<int> setInactiveColumns = new List<int>();
        protected string violateRule1Text = "Violated rule #1";
        protected string violateRule2Text = "Violated rule #2";
        protected string violateRule3Text = "Violated rule #3";
        protected string adviceRuleText = "This will make row 4 same as row 1.";
        [Header("UI references")]
        public UIScriptAnimationManager header1GameObject;
        public UIScriptAnimationManager header2GameObject;
        public UIScriptAnimationManager instructionGameObject;
        public UIScriptAnimationManager instructionGameObject1;
        public UIScriptAnimationManager instructionGameObject2;
        public Text ruleTitle;
        public Text instruction2Text;
        public UIScriptAnimationManager skipBtn;
        public RectTransform rectTransInstruction2;
        private Coroutine checkStateCoroutine;
        public TutorialCompletePanel tutorialComplatePanel;

        private Coroutine differentRowsHintCR;
        private int preCellValue1 = 100;
        private int preCellValue2 = 100;
        private Coroutine switchCoroutine1;
        private Coroutine switchCoroutine2;
        private Coroutine checkGameStateCoroutine;
        private bool isPassStep = false;

        private void OnEnable()
        {
            LogicalBoard.onCellClicked += OnCellClicked;
        }

        private void OnDisable()
        {
            LogicalBoard.onCellClicked -= OnCellClicked;
        }

        public void Awake()
        {
            ruleTitle.text = String.Format(I2.Loc.ScriptLocalization.RULE_NAME.ToUpper(), 3);
            violateRule1Text = I2.Loc.ScriptLocalization.Three_Number;
            violateRule3Text = I2.Loc.ScriptLocalization.Must_Different;
            adviceRuleText = I2.Loc.ScriptLocalization.Will_Same;
        }

        void OnCellClicked(Index2D index2D)
        {
            if (checkGameStateCoroutine != null)
                StopCoroutine(checkGameStateCoroutine);
            checkGameStateCoroutine = StartCoroutine(CR_WaitCheckComplete());
        }

        IEnumerator CR_WaitCheckComplete()
        {
            yield return new WaitForSeconds(0.3f);
            isPassStep = Rule3ConditionIsFullFill();
        }

        public override void StartWalkthrough(TutorialManager4 tutorialManager)
        {
            base.StartWalkthrough(tutorialManager);
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(CR_RuleThreeWalkthrough());
        }

        private IEnumerator CR_RuleThreeWalkthrough()
        {
            yield return null;
            Index2D[] intereactableIndexes = null;
            m_lb = base.tutorialManager.RequestNewLogicalBoard(puzzle, solution, intereactableIndexes);

            yield return new WaitUntil(() =>
            {
                return !cameraPositionAnimation.isAnimationRunning;
            });
            header1GameObject.FadeIn(0.5f);
            yield return new WaitForSeconds(0.25f);
            header2GameObject.FadeIn(0.5f);
            yield return new WaitForSeconds(0.5f);
            //yield return StartCoroutine(CR_WaitNext(2.5f, true));
            if (VisualBoard.Instance != null)
            {
                VisualBoard.Instance.SetInActiveRows(setInactiveRows.ToArray());
                VisualBoard.Instance.SetInActiveColumns(setInactiveColumns.ToArray());
            }
            differentRowsHintCR = StartCoroutine(CR_CheckUniqueRowsCondition(setInactiveRows, setInactiveColumns));
            yield return new WaitForSeconds(0.6f);
            instructionGameObject.FadeIn(0.5f);

            Index2D indexintereactCell1 = base.tutorialManager.s2i(intereactCell1, (int)Mathf.Sqrt(puzzle.Length));
            Index2D index2D = base.tutorialManager.s2i(intereactCell2, (int)Mathf.Sqrt(puzzle.Length));
            checkStateCoroutine = StartCoroutine(CR_CheckChangeCellValue(new List<Index2D>() { indexintereactCell1, index2D },
                new List<string>() { intereactCell1, intereactCell2 }));

            m_lb.SetInteractableIndex(
                indexintereactCell1,
                index2D
            );

            yield return new WaitUntil(() =>
            {
                return Rule3ConditionIsFullFill();
            });
            tutorialComplatePanel.StartRecordingGif();
            //if (SoundManager.Instance)
            //SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver, true);
            StartCoroutine(DelayHideInstruction(instructionGameObject, 0));
            if (skipBtn.gameObject.activeInHierarchy)
                skipBtn.FadeOut(0.5f);
            m_lb.SetInteractableIndex();
            if (VisualBoard.Instance != null)
                VisualBoard.Instance.ClearInActiveCells();
            if (VisualBoard.Instance != null)
                VisualBoard.Instance.HideHandUI();

            if (checkStateCoroutine != null)
                StopCoroutine(checkStateCoroutine);
            instructionGameObject.FadeOut(0.4f);
            header2GameObject.FadeOut(0.5f);
            header1GameObject.FadeOut(0.5f);
            yield return new WaitForSeconds(0.5f);
            if (SoundManager.Instance)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver, true);
                SoundManager.Instance.PlaySoundDelay(0, SoundManager.Instance.confetti, true);
            }
            TutorialManager4.Instance.PlayLeavesParticle(Mathf.Infinity);
            instructionGameObject1.FadeIn(0.3f);
            yield return new WaitForSeconds(0.75f);
            instructionGameObject1.FadeOut(0.3f);

            #region Analytics Event
            AlolAnalytics.CompletedTutorialMain();
            #endregion

            yield return new WaitForSeconds(0.5f);
            base.isFinished = true;
            gameObject.SetActive(false);


        }

        private IEnumerator CR_CheckUniqueRowsCondition(List<int> setInactiveRows, List<int> setInactiveColumns)
        {
            yield return null;
            ContentSizeFitter fitter = rectTransInstruction2.GetComponent<ContentSizeFitter>();
            string preIns = "";
            float preSizeY = rectTransInstruction2.sizeDelta.y;
            bool isShowViolate = true;
            while (true)
            {
                if (Rule3ConditionIsFullFill())
                {
                    if (switchCoroutine1 != null)
                        StopCoroutine(switchCoroutine1);

                    if (switchCoroutine2 != null)
                        StopCoroutine(switchCoroutine2);

                    skipBtn.gameObject.SetActive(false);
                    instructionGameObject2.FadeOut(0.3f);
                    StopCoroutine(differentRowsHintCR);
                    instructionGameObject.FadeOut(0.3f);
                }
                else
                {
                    if (VisualBoard.Instance.currentErrorCells != null && VisualBoard.Instance.currentErrorCells.Count > 0)
                    {
                        if (!isShowViolate)
                        {
                            isShowViolate = true;
                            instructionGameObject.FadeOut(0.3f);
                            //instructionGameObject2.transform.localScale = new Vector3(0, 1, 1);
                            //yield return new WaitForSeconds(0.4f);
                            //instructionGameObject2.FadeIn(0.3f);
                            if (switchCoroutine2 != null)
                                StopCoroutine(switchCoroutine2);
                            switchCoroutine1 = StartCoroutine(DelaySwitchText(isShowViolate));
                        }

                        if (base.tutorialManager.lb.violateRule3)
                        {
                            VisualBoard.Instance.StopHighlightErrorCells();
                            instruction2Text.text = violateRule3Text;
                        }
                        else if (base.tutorialManager.lb.violateRule1)
                        {
                            if (m_lb.GetValue(base.tutorialManager.s2i(intereactCell2, (int)Mathf.Sqrt(puzzle.Length))) == -1
                                || m_lb.GetValue(base.tutorialManager.s2i(intereactCell1, (int)Mathf.Sqrt(puzzle.Length))) == -1)
                            {
                                VisualBoard.Instance.StopHighlightErrorCells();
                                instruction2Text.text = adviceRuleText;
                            }
                            else
                            {
                                instruction2Text.text = violateRule1Text;
                            }
                        }
                        else if (base.tutorialManager.lb.violateRule2)
                        {
                            VisualBoard.Instance.StopHighlightErrorCells();
                            instruction2Text.text = violateRule2Text;
                        }
                    }
                    else
                    {
                        if (isShowViolate)
                        {
                            isShowViolate = false;
                            instructionGameObject2.FadeOut(0.3f);
                            //instructionGameObject.transform.localScale = new Vector3(0, 1, 1);
                            //yield return new WaitForSeconds(0.4f);
                            //instructionGameObject.FadeIn(0.3f);
                            if (switchCoroutine1 != null)
                                StopCoroutine(switchCoroutine1);
                            switchCoroutine2 = StartCoroutine(DelaySwitchText(isShowViolate));
                        }
                    }
                }

                yield return new WaitForEndOfFrame();

                // resize size of instruction if its width is too long.
                if (!preIns.Equals(instruction2Text.text))
                {
                    if (instruction2Text.text.Equals(violateRule3Text))
                    {
                        rectTransInstruction2.sizeDelta = new Vector2(rectTransInstruction2.sizeDelta.x, 50);
                    }
                    if (instruction2Text.text.Equals(adviceRuleText))
                    {
                        rectTransInstruction2.sizeDelta = new Vector2(rectTransInstruction2.sizeDelta.x, 75);
                    }
                    if (instruction2Text.text.Equals(violateRule1Text))
                    {
                        rectTransInstruction2.sizeDelta = new Vector2(rectTransInstruction2.sizeDelta.x, 75);
                    }
                    preIns = instruction2Text.text;
                }
            }
        }

        IEnumerator DelaySwitchText(bool isViolate)
        {
            if (isViolate)
            {
                instructionGameObject2.transform.localScale = new Vector3(0, 1, 1);
                yield return new WaitForSeconds(0.4f);
                instructionGameObject2.FadeIn(0.3f);
            }
            else
            {
                instructionGameObject.transform.localScale = new Vector3(0, 1, 1);
                yield return new WaitForSeconds(0.4f);
                instructionGameObject.FadeIn(0.3f);
            }
        }

        private bool Rule3ConditionIsFullFill()
        {
            return m_lb.GetValue(base.tutorialManager.s2i(intereactCell1, (int)Mathf.Sqrt(puzzle.Length))) == correctIntereactableCell1Value &&
                m_lb.GetValue(base.tutorialManager.s2i(intereactCell2, (int)Mathf.Sqrt(puzzle.Length))) == correctIntereactableCell2Value;
        }

        private bool IsChangeCellValue()
        {
            bool isChange = false;
            if (m_lb.GetValue(base.tutorialManager.s2i(intereactCell1, (int)Mathf.Sqrt(puzzle.Length))) != preCellValue1)
            {
                preCellValue1 = m_lb.GetValue(base.tutorialManager.s2i(intereactCell1, (int)Mathf.Sqrt(puzzle.Length)));
                isChange = true;
            }

            if (m_lb.GetValue(base.tutorialManager.s2i(intereactCell2, (int)Mathf.Sqrt(puzzle.Length))) != preCellValue2)
            {
                preCellValue2 = m_lb.GetValue(base.tutorialManager.s2i(intereactCell2, (int)Mathf.Sqrt(puzzle.Length)));
                isChange = true;
            }
            return isChange;
        }
        IEnumerator DelayHideInstruction(UIScriptAnimationManager uiAni, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            uiAni.FadeOut(0.5f);
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
                        {
                            List<Index2D> showHandIndexes = new List<Index2D>();
                            bool violatedOtherRules = false;
                            if (m_lb.GetValue(base.tutorialManager.s2i(intereactCell1, (int)Mathf.Sqrt(puzzle.Length))) == LogicalBoard.VALUE_ZERO)
                            {
                                violatedOtherRules = true;
                                showHandIndexes.Add(base.tutorialManager.s2i(intereactCell1, (int)Mathf.Sqrt(puzzle.Length)));
                            }
                            if (m_lb.GetValue(base.tutorialManager.s2i(intereactCell2, (int)Mathf.Sqrt(puzzle.Length))) == LogicalBoard.VALUE_ONE)
                            {
                                violatedOtherRules = true;
                                showHandIndexes.Add(base.tutorialManager.s2i(intereactCell2, (int)Mathf.Sqrt(puzzle.Length)));
                            }
                            if (!violatedOtherRules)
                            {
                                if (m_lb.GetValue(base.tutorialManager.s2i(intereactCell1, (int)Mathf.Sqrt(puzzle.Length))) == LogicalBoard.VALUE_EMPTY)
                                    showHandIndexes.Add(base.tutorialManager.s2i(intereactCell1, (int)Mathf.Sqrt(puzzle.Length)));
                                if (m_lb.GetValue(base.tutorialManager.s2i(intereactCell2, (int)Mathf.Sqrt(puzzle.Length))) == LogicalBoard.VALUE_EMPTY)
                                    showHandIndexes.Add(base.tutorialManager.s2i(intereactCell2, (int)Mathf.Sqrt(puzzle.Length)));
                            }
                            if (showHandIndexes.Count > 0)
                                VisualBoard.Instance.ShowHandUI(showHandIndexes);
                        }
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }
}
