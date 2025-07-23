using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class ErrorsDisplayer : MonoBehaviour
    {
        public static event System.Action<bool[]> UpdateRuleViolate = delegate { };
        public Animator rule1ErrorAnimator;
        public Button rule1Button;
        public Image rule1Image;
        [Space]
        [HideInInspector]
        public Animator rule2ErrorAnimator;
        [HideInInspector]
        public Button rule2Button;
        [Space]
        [HideInInspector]
        public Animator rule3ErrorAnimator;
        [HideInInspector]
        public Button rule3Button;
        [Space]
        [SerializeField]
        private Sprite[] ruleIcon;
        private RulePanel rulePanel;
        private PlayUI playUI;

        public static string ON_ERROR_STATE_KEY = "Rule1ErrorAnimationClip";
        public static string ON_NO_ERROR_STATE_KEY = "Rule1NoErrorAnimationClip";

        public static string RULE_1_DESCRIPTION = "We can't have three same numbers\nnext to each other.";
        public static string RULE_2_DESCRIPTION = "Each rows and columns must have\nthe same number of 0s and 1s.";
        public static string RULE_3_DESCRIPTION = "No identical rows or columns.";


        private int descriptionIsShown = -1;

        private void Awake()
        {
            if(UIReferences.Instance != null)
            {
                OnUiReferencesUpdated();
            }
            UIReferences.UiReferencesUpdated += OnUiReferencesUpdated;
        }

        private void OnDestroy()
        {
            UIReferences.UiReferencesUpdated -= OnUiReferencesUpdated;
        }

        void OnUiReferencesUpdated()
        {
            rulePanel = UIReferences.Instance.rulePanel;
            playUI = UIReferences.Instance.gameUiPlayUI;
        }

        void Start()
        {
            rule1Button.onClick.AddListener(delegate
            {
                rulePanel.Show();
            });
        }

        private void OnRule3ViolatedButtonClick()
        {
            InGameNotificationPopup.Instance.confirmationDialog.Show("Rule 3", RULE_3_DESCRIPTION, "OK", "", () => { }, null, null, false);
        }

        private void OnRule2ViolatedButtonClick()
        {
            InGameNotificationPopup.Instance.confirmationDialog.Show("Rule 2", RULE_2_DESCRIPTION, "OK", "", () => { }, null, null, false);
        }

        private void OnRule1ViolatedButtonClick()
        {
            InGameNotificationPopup.Instance.confirmationDialog.Show("Rule 1", RULE_1_DESCRIPTION, "OK", "", () => { }, null, null, false);
        }

        public void OnEnable()
        {
            ResetAnimationStates();
            LogicalBoard.onPuzzleValidated += OnPuzzleValidated;
        }
        
        public void OnDisable()
        {
            LogicalBoard.onPuzzleValidated -= OnPuzzleValidated;
        }

        private void OnPuzzleValidated(ICollection<Index2D> errorCells)
        {
            //for (int i = 0; i < 3; i++)
            //{
            //    StartErrorAnimation(i, LogicalBoard.Instance.currentErrorsState[i]);
            //}
            if (checkErrorCoroutine != null)
                StopCoroutine(checkErrorCoroutine);
            checkErrorCoroutine = StartCoroutine(CR_CheckAllRuleError());
        }

        private bool[] errorAnimationStates         = new bool[] { false, false, false };
        private Coroutine[] animationCRs = new Coroutine[] { null, null, null};
        private Coroutine checkErrorCoroutine;

        private void StartErrorAnimation(int i, bool error)
        {
            if (error == errorAnimationStates[i])
                return;
            if(animationCRs[i]!= null)
                StopCoroutine(animationCRs[i]);

            animationCRs[i] = StartCoroutine(AnimationCoroutine(i));    
        }

        [HideInInspector]
        public int lastedErrorIndex = -1;
        IEnumerator AnimationCoroutine(int ruleIndex)
        {
            yield return new WaitForSeconds(1);
            bool error = LogicalBoard.Instance.currentErrorsState[ruleIndex];
            if (error != errorAnimationStates[ruleIndex])
            {
                string animationKey = error ? ON_ERROR_STATE_KEY : ON_NO_ERROR_STATE_KEY;
                switch (ruleIndex)
                {
                    case 0:
                        rule1ErrorAnimator.Play(animationKey);
                        break;
                    case 1:
                        rule2ErrorAnimator.Play(animationKey);
                        break;
                    case 2:
                        rule3ErrorAnimator.Play(animationKey);
                        break;
                }
                if (error)
                    lastedErrorIndex = ruleIndex;
                else
                {
                    int errorCount = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        if (errorAnimationStates[i] == true)
                        {
                            errorCount++;
                        }
                    }
                    if (errorCount == 0)
                    {
                        lastedErrorIndex = -1;
                    }
                }
            }
            errorAnimationStates[ruleIndex] = error;
        }

        IEnumerator CR_CheckAllRuleError()
        {
            yield return new WaitForSeconds(1);
            bool[] error = new bool[3];
            int countError = 0;
            for (int i = 0; i < 3; i++)
            {
                error[i] = LogicalBoard.Instance.currentErrorsState[i];
                errorAnimationStates[i] = error[i];
                if (error[i])
                {
                    lastedErrorIndex = i;
                    countError++;
                }
            }

            if(countError > 0)
            {
                if(!UIGuide.instance.IsGuildeShown(playUI.guideFirstErrorSaveKey))
                {
                    playUI.ShowGuideFirstError();
                }
                //rule1Image.sprite = ruleIcon[lastedErrorIndex];
                //rule1ErrorAnimator.Play(ON_ERROR_STATE_KEY);
            }
            else
            {
                //rule1Image.sprite = ruleIcon[0];
                lastedErrorIndex = -1;
                //rule1ErrorAnimator.Play(ON_NO_ERROR_STATE_KEY);
            }

            UpdateRuleViolate(errorAnimationStates);
        }

        public void SwitchRuleIcon(int ruleIndex,bool isError)
        {
            rule1Image.sprite = ruleIcon[ruleIndex];
            if (isError)
                rule1ErrorAnimator.Play(ON_ERROR_STATE_KEY);
            else
                rule1ErrorAnimator.Play(ON_NO_ERROR_STATE_KEY);
        }

        private void ResetAnimationStates()
        {
            rule1Image.sprite = ruleIcon[3];
            rule1ErrorAnimator.Play(ON_NO_ERROR_STATE_KEY, -1, 1);
            //rule2ErrorAnimator.Play(ON_NO_ERROR_STATE_KEY, -1, 1);
            //rule3ErrorAnimator.Play(ON_NO_ERROR_STATE_KEY, -1, 1);
            if (UIReferences.Instance != null)
                UIReferences.Instance.rulePanel.ResetError();
        }

    }
}
