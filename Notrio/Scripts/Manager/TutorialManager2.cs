using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Pinwheel;

namespace Takuzu
{
    [Obsolete("Use TutorialManager3 instead")]
    public class TutorialManager2 : MonoBehaviour
    {
        #region variables
        [Header("General")]
        public Button closeButton;
        public GridLayoutGroup gridLayoutGroup;
        public TutorialCellIllustrator board;
        public CanvasGroup panelGroup;
        public ColorAnimation panelAnim;
        [Space]
        public Button nextButton;
        public Image nextButtonBg;
        public Text nextButtonText;
        public Color nextButtonBgColor;
        public CanvasGroup nextButtonGroup;
        [Space]
        public Button prevButton;
        public Image prevButtonBg;
        public Text prevButtonText;
        public Color prevButtonBgColor;
        public CanvasGroup prevButtonGroup;
        [Space]
        public Color buttonInactiveColor;
        public Color blueCellColor;
        public Color redCellColor;
        public Color grayCellColor;

        public Text instructionText;

        public string Instruction
        {
            get
            {
                return instructionText.text;
            }
            set
            {
                instructionText.text = value;
            }
        }

        private Action prevAction;
        private Action PrevAction
        {
            get
            {
                return prevAction;
            }
            set
            {
                prevAction = value;
                SetPrevButtonInteractable(prevAction != null);
            }
        }

        private Action nextAction;
        private Action NextAction
        {
            get
            {
                return nextAction;
            }
            set
            {
                nextAction = value;
                SetNextButtonInteractable(nextAction != null);
            }
        }

        [Header("Introduction")]
        public List<TutorialCellIllustrator> redCellsIntro;
        public List<TutorialCellIllustrator> blueCellsIntro;
        public float animIntervalIntro;
        public float colorTweenDurationIntro;
        public bool isIntroAnimStopped;

        [Header("Rule 1")]
        public List<TutorialCellIllustrator> redCellsRule1;
        public List<TutorialCellIllustrator> blueCellsRule1;
        public float animIntervalRule1;
        public float colorTweenDurationRule1;
        public bool isRule1AnimStopped;

        [Header("Rule 2")]
        public List<TutorialCellIllustrator> redCellRule2;
        public List<TutorialCellIllustrator> blueCellsRule2;
        public List<TutorialCellIllustrator> grayCellRule2;
        public Vector2 blueCellPivotRule2;
        public Vector2 redCellPivotRule2;
        public float cellOffsetYRule2;
        public float animIntervalRule2;
        public float animCycleDelayRule2;
        public bool isRule2AnimStopped;

        [Header("Rule 3")]
        public List<TutorialCellIllustrator> redCellRule3;
        public List<TutorialCellIllustrator> blueCellRule3;
        public List<TutorialCellIllustrator> grayCellStep1Rule3;
        public List<TutorialCellIllustrator> grayCellStep2Rule3;
        public List<TutorialCellIllustrator> firstColumnRule3;
        public List<TutorialCellIllustrator> secondColumnRule3;
        public List<TutorialCellIllustrator> firstRowRule3;
        public List<TutorialCellIllustrator> secondRowRule3;
        public Vector2 boardTargetPosRule3;
        public Vector2 columnPivotRule3;
        public float columnOffsetRule3;
        public Vector2 rowPivotRule3;
        public float rowOffsetRule3;
        public float cellOffsetRule3;
        public float animIntervalRule3;
        public float animCycleDelayRule3;
        public bool isRule3AnimStopped;

        [Header("Walkthrough")]
        public string puzzle;
        public string solution;
        public int size;
        public GameObject highlighterTemplate;
        public GameObject revealButtonHighlighter;
        public GameObject undoButtonHighlighter;
        public GameObject coinHighlighter;
        public Button revealButton;
        public Button undoButton;
        public CanvasGroup revealGroup;
        public CanvasGroup undoGroup;
        public Text coinText;
        public ItemPriceProfile price;
        public Button nextButton2;
        public Button skipButton;
        public Button finishButton;
        public Text instructionText2;
        public ColorAnimation characterGroupAnim;
        public PositionAnimation skipButtonAnim;
        public PositionAnimation coinGroupAnim;
        public PositionAnimation revealGroupAnim;
        public PositionAnimation undoGroupAnim;
        public AnimController revealHighlightAnim;
        public AnimController undoHighlightAnim;
        public AnimController coinHighlightAnim;
        public GameObject looseCoinPrefab;
        public ParticleSystem leavesParticle;
        public RectTransform panelRt;
        public Vector2 panelTargetPos;
        public float panelMovementSpeed;
        public Vector2 panelTargetSize;
        public float panelResizeSpeed;
        public ColorAnimation boardFadeAnim;
        public ColorAnimation panelHeaderFadeAnim;

        public LogicalBoardTutorial lb;
        private Dictionary<string, GameObject> hltDict;
        private bool isTapOnNext;
        private bool isTapOnEnterWalkthrough;
        private bool isRevealButtonClicked;
        private bool isUndoButtonClicked;
        private bool isSetAnyValue;
        private bool isPuzzleSolved;
        #endregion

        private void Start()
        {
            closeButton.onClick.AddListener(delegate
            {
                if (SceneLoadingManager.Instance != null)
                    SceneLoadingManager.Instance.LoadMainScene();
                else
                    SceneManager.LoadScene("Main");
            });

            nextButton.onClick.AddListener(delegate
            {
                if (NextAction != null)
                    NextAction();
            });

            prevButton.onClick.AddListener(delegate
            {
                if (PrevAction != null)
                    PrevAction();
            });

            skipButton.onClick.AddListener(delegate
            {
                if (SceneLoadingManager.Instance != null)
                    SceneLoadingManager.Instance.LoadMainScene();
                else
                    SceneManager.LoadScene("Main");
            });

            finishButton.onClick.AddListener(delegate
            {
                if (SceneLoadingManager.Instance != null)
                    SceneLoadingManager.Instance.LoadMainScene();
                else
                    SceneManager.LoadScene("Main");
            });

            PrevAction = null;
            NextAction = null;
            Intro();
        }

        private void SetNextButtonInteractable(bool i)
        {
            nextButtonBg.color = i ? nextButtonBgColor : buttonInactiveColor;
            nextButton.interactable = i;
            nextButtonGroup.blocksRaycasts = i;
        }

        private void SetPrevButtonInteractable(bool i)
        {
            prevButtonBg.color = i ? prevButtonBgColor : buttonInactiveColor;
            prevButton.interactable = i;
            prevButtonGroup.blocksRaycasts = i;
        }

        private void Intro()
        {
            isIntroAnimStopped = false;
            StartCoroutine(CrIntroAnim());
            PrevAction = null;
            NextAction = IntroToRule1;
            Instruction =
                        "<b>TKZ</b> is a puzzle game " +
                        "in which you have to fill a NxN board " +
                        "with BLUEs and REDs " +
                        "to satisfy 3 simple rules";
            nextButtonText.text = "NEXT";
        }

        private IEnumerator CrIntroAnim()
        {
            board.targetPos = board.initPos;
            for (int i = 0; i < redCellsIntro.Count; ++i)
            {
                redCellsIntro[i].targetPos = redCellsIntro[i].initPos;
            }
            for (int i = 0; i < blueCellsIntro.Count; ++i)
            {
                blueCellsIntro[i].targetPos = blueCellsIntro[i].initPos;
            }
            while (!isIntroAnimStopped)
            {
                for (int i = 0; i < redCellsIntro.Count; ++i)
                {
                    redCellsIntro[i].targetColor = redCellColor;
                }
                for (int i = 0; i < blueCellsIntro.Count; ++i)
                {
                    blueCellsIntro[i].targetColor = blueCellColor;
                }
                yield return new WaitForSeconds(animIntervalIntro);
                for (int i = 0; i < redCellsIntro.Count; ++i)
                {
                    redCellsIntro[i].targetColor = grayCellColor;
                }
                for (int i = 0; i < blueCellsIntro.Count; ++i)
                {
                    blueCellsIntro[i].targetColor = grayCellColor;
                }
                yield return new WaitForSeconds(animIntervalIntro);
            }

            for (int i = 0; i < redCellsIntro.Count; ++i)
            {
                redCellsIntro[i].targetColor = redCellColor;
            }
            for (int i = 0; i < blueCellsIntro.Count; ++i)
            {
                blueCellsIntro[i].targetColor = blueCellColor;
            }
        }

        private void Rule1()
        {
            isRule1AnimStopped = false;
            StartCoroutine(CrRule1Anim());
            PrevAction = Rule1ToIntro;
            NextAction = Rule1ToRule2;
            Instruction =
                        "<b>Rule #1</b>\n" +
                        "There is no 3 cells of the same value " +
                        "next to each others";
            nextButtonText.text = "NEXT";
        }

        private IEnumerator CrRule1Anim()
        {
            board.targetPos = board.initPos;
            for (int i = 0; i < redCellsRule1.Count; ++i)
            {
                redCellsRule1[i].targetPos = redCellsRule1[i].initPos;
            }
            for (int i = 0; i < blueCellsRule1.Count; ++i)
            {
                blueCellsRule1[i].targetPos = blueCellsRule1[i].initPos;
            }
            while (!isRule1AnimStopped)
            {
                for (int i = 0; i < redCellsRule1.Count; ++i)
                {
                    redCellsRule1[i].targetColor = grayCellColor;
                }
                for (int i = 0; i < blueCellsRule1.Count; ++i)
                {
                    blueCellsRule1[i].targetColor = blueCellColor;
                }
                yield return new WaitForSeconds(animIntervalRule1);

                for (int i = 0; i < redCellsRule1.Count; ++i)
                {
                    redCellsRule1[i].targetColor = redCellColor;
                }
                for (int i = 0; i < blueCellsRule1.Count; ++i)
                {
                    blueCellsRule1[i].targetColor = grayCellColor;
                }
                yield return new WaitForSeconds(animIntervalRule1);

                for (int i = 0; i < redCellsRule1.Count; ++i)
                {
                    redCellsRule1[i].targetColor = redCellColor;
                }
                for (int i = 0; i < blueCellsRule1.Count; ++i)
                {
                    blueCellsRule1[i].targetColor = blueCellColor;
                }
                yield return new WaitForSeconds(animIntervalRule1);
            }

            for (int i = 0; i < redCellsRule1.Count; ++i)
            {
                redCellsRule1[i].targetColor = redCellColor;
            }
            for (int i = 0; i < blueCellsRule1.Count; ++i)
            {
                blueCellsRule1[i].targetColor = blueCellColor;
            }
        }

        private void IntroToRule1()
        {
            isIntroAnimStopped = true;
            StopAllCoroutines();
            Rule1();
        }

        private void Rule1ToIntro()
        {
            isRule1AnimStopped = true;
            StopAllCoroutines();
            Intro();
        }

        private void Rule1ToRule2()
        {
            isRule1AnimStopped = true;
            StopAllCoroutines();
            Rule2();
        }

        private void Rule2ToRule1()
        {
            isRule2AnimStopped = true;
            StopAllCoroutines();
            Rule1();
        }

        private void Rule2()
        {
            isRule2AnimStopped = false;
            StartCoroutine(CrRule2Anim());
            PrevAction = Rule2ToRule1;
            NextAction = Rule2ToRule3;
            Instruction =
                        "<b>Rule #2</b>\n" +
                        "Each row/column must have " +
                        "an equal number of BLUEs and REDs";
            nextButtonText.text = "NEXT";
        }

        private IEnumerator CrRule2Anim()
        {
            board.targetPos = board.initPos;
            for (int i = 0; i < redCellRule2.Count; ++i)
            {
                redCellRule2[i].targetColor = redCellColor;
                redCellRule2[i].targetPos = redCellRule2[i].initPos;
            }
            for (int i = 0; i < blueCellsRule2.Count; ++i)
            {
                blueCellsRule2[i].targetColor = blueCellColor;
                blueCellsRule2[i].targetPos = blueCellsRule2[i].initPos;
            }
            for (int i = 0; i < grayCellRule2.Count; ++i)
            {
                grayCellRule2[i].targetColor = grayCellColor;
                grayCellRule2[i].targetPos = grayCellRule2[i].initPos;
            }
            while (!isRule2AnimStopped)
            {
                for (int i = 0; i < Mathf.Min(redCellRule2.Count, blueCellsRule2.Count); ++i)
                {
                    blueCellsRule2[i].targetPos = blueCellPivotRule2 + Vector2.down * i * cellOffsetYRule2;
                    redCellRule2[i].targetPos = redCellPivotRule2 + Vector2.down * i * cellOffsetYRule2;
                    yield return new WaitForSeconds(animIntervalRule2);
                }

                yield return new WaitForSeconds(animCycleDelayRule2);

                for (int i = 0; i < Mathf.Min(redCellRule2.Count, blueCellsRule2.Count); ++i)
                {
                    blueCellsRule2[i].targetPos = blueCellsRule2[i].initPos;
                    redCellRule2[i].targetPos = redCellRule2[i].initPos;
                    yield return new WaitForSeconds(animIntervalRule2);
                }

                yield return new WaitForSeconds(animCycleDelayRule2);
            }
        }

        private void Rule2ToRule3()
        {
            isRule2AnimStopped = true;
            StopAllCoroutines();
            Rule3();
        }

        private void Rule3ToRule2()
        {
            isRule3AnimStopped = true;
            StopAllCoroutines();
            Rule2();
        }

        private void Rule3()
        {
            isRule3AnimStopped = false;
            StartCoroutine(CrRule3Anim());
            PrevAction = Rule3ToRule2;
            NextAction = EnterWalkthrough;
            Instruction =
                        "<b>Rule #3</b>\n" +
                        "Each row/column are unique";
            nextButtonText.text = "LET'S TRY";
        }

        private IEnumerator CrRule3Anim()
        {
            yield return null;
            board.targetPos = boardTargetPosRule3;
            for (int i = 0; i < redCellRule3.Count; ++i)
            {
                redCellRule3[i].targetPos = redCellRule3[i].initPos;
            }
            for (int i = 0; i < blueCellRule3.Count; ++i)
            {
                blueCellRule3[i].targetPos = blueCellRule3[i].initPos;
            }
            while (!isRule3AnimStopped)
            {
                for (int i = 0; i < redCellRule3.Count; ++i)
                {
                    redCellRule3[i].targetColor = redCellColor;
                }
                for (int i = 0; i < blueCellRule3.Count; ++i)
                {
                    blueCellRule3[i].targetColor = blueCellColor;
                }
                for (int i = 0; i < grayCellStep1Rule3.Count; ++i)
                {
                    grayCellStep1Rule3[i].targetColor = grayCellColor;
                }
                for (int i = 0; i < firstRowRule3.Count; ++i)
                {
                    firstRowRule3[i].targetPos = firstRowRule3[i].initPos;
                }
                for (int i = 0; i < secondRowRule3.Count; ++i)
                {
                    secondRowRule3[i].targetPos = secondRowRule3[i].initPos;
                }
                yield return new WaitForSeconds(animCycleDelayRule3 / 2);
                for (int i = 0; i < firstColumnRule3.Count; ++i)
                {
                    firstColumnRule3[i].targetPos = columnPivotRule3 + Vector2.down * i * cellOffsetRule3;
                    yield return new WaitForSeconds(animIntervalRule3);
                }
                for (int i = 0; i < secondColumnRule3.Count; ++i)
                {
                    secondColumnRule3[i].targetPos = columnPivotRule3 + Vector2.right * columnOffsetRule3 + Vector2.down * i * cellOffsetRule3;
                    yield return new WaitForSeconds(animIntervalRule3);
                }
                yield return new WaitForSeconds(animCycleDelayRule3);

                for (int i = 0; i < redCellRule3.Count; ++i)
                {
                    redCellRule3[i].targetColor = redCellColor;
                }
                for (int i = 0; i < blueCellRule3.Count; ++i)
                {
                    blueCellRule3[i].targetColor = blueCellColor;
                }
                for (int i = 0; i < grayCellStep2Rule3.Count; ++i)
                {
                    grayCellStep2Rule3[i].targetColor = grayCellColor;
                }
                for (int i = 0; i < firstColumnRule3.Count; ++i)
                {
                    firstColumnRule3[i].targetPos = firstColumnRule3[i].initPos;
                }
                for (int i = 0; i < secondColumnRule3.Count; ++i)
                {
                    secondColumnRule3[i].targetPos = secondColumnRule3[i].initPos;
                }
                yield return new WaitForSeconds(animCycleDelayRule3 / 2);
                for (int i = 0; i < firstRowRule3.Count; ++i)
                {
                    firstRowRule3[i].targetPos = rowPivotRule3 + Vector2.right * i * cellOffsetRule3;
                    yield return new WaitForSeconds(animIntervalRule3);
                }
                for (int i = 0; i < secondRowRule3.Count; ++i)
                {
                    secondRowRule3[i].targetPos = rowPivotRule3 + Vector2.up * rowOffsetRule3 + Vector2.right * i * cellOffsetRule3;
                    yield return new WaitForSeconds(animIntervalRule3);
                }
                yield return new WaitForSeconds(animCycleDelayRule3);
            }
        }

        private void EnterWalkthrough()
        {
            //HidePanel();
            InitWalkthrough();
            StartCoroutine(CrWalkthrough());
        }

        private void HidePanel()
        {
            panelGroup.blocksRaycasts = false;
            panelAnim.Play(AnimConstant.OUT);
        }

        private void InitWalkthrough()
        {
            puzzle = "1....0.1.....11.010.1....0......00..";
            solution = "101100110010011001001101100110010011";
            size = 6;

            GameObject hltRoot = new GameObject("HltRoot");
            hltRoot.transform.position = Vector3.zero;
            hltDict = new Dictionary<string, GameObject>();
            for (int i = 0; i < 6; ++i)
            {
                for (int j = 0; j < 6; ++j)
                {
                    GameObject g = Instantiate(highlighterTemplate, new Vector3(j, i, 0), Quaternion.identity);
                    g.transform.SetParent(hltRoot.transform, true);
                    g.transform.localScale = Vector3.one;
                    g.SetActive(false);
                    string s = i2s(new Index2D(i, j));
                    hltDict.Add(s, g);
                }
            }

            hltDict.Add("revealButton", revealButtonHighlighter);
            hltDict.Add("undoButton", undoButtonHighlighter);
            hltDict.Add("coin", coinHighlighter);
            revealButtonHighlighter.SetActive(false);
            undoButtonHighlighter.SetActive(false);
            coinHighlighter.SetActive(false);

            revealButton.onClick.AddListener(RevealButtonClicked);
            undoButton.onClick.AddListener(UndoButtonClicked);
            //nextButton = nextButton2;
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(TapOnNext);
            nextButtonText.text = "NEXT";
            //instructionText = instructionText2;
            closeButton.onClick.RemoveAllListeners();
            prevButton.gameObject.SetActive(false);
        }

        private string i2s(Index2D i)
        {
            string s = string.Format(
                "{0}{1}",
                size - i.row,
                (char)(65 + i.column));
            return s;
        }

        private Index2D s2i(string s)
        {
            return new Index2D()
            {
                row = size - int.Parse(s[0].ToString()),
                column = s[1] - 65
            };
        }

        public bool HadFinishTutorialBefore
        {
            get
            {
                return PlayerPrefs.HasKey("FINISH_TUTORIAL");
            }
        }

        public void RevealButtonClicked()
        {
            isRevealButtonClicked = true;
        }

        public void UndoButtonClicked()
        {
            isUndoButtonClicked = true;
        }

        private IEnumerator CrWalkthrough()
        {
            //characterGroupAnim.Play(AnimConstant.IN);
            coinText.text = "0";
            revealGroup.gameObject.SetActive(false);
            undoGroup.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(true);
            skipButtonAnim.Play(AnimConstant.IN);
            finishButton.gameObject.SetActive(false);
            SetNextButtonInteractable(false);
            isTapOnNext = false;
            lb.InitPuzzle(puzzle, solution);
            lb.SetInteractableIndex();

            panelHeaderFadeAnim.Play(AnimConstant.OUT);
            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                panelHeaderFadeAnim.gameObject.SetActive(false);
            }, panelHeaderFadeAnim.duration);

            boardFadeAnim.Play(AnimConstant.OUT);
            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                boardFadeAnim.gameObject.SetActive(false);
            }, boardFadeAnim.duration);

            yield return new WaitForSeconds(boardFadeAnim.duration);

            while (panelRt.sizeDelta != panelTargetSize || panelRt.anchoredPosition != panelTargetPos)
            {
                panelRt.sizeDelta = Vector2.MoveTowards(panelRt.sizeDelta, panelTargetSize, panelResizeSpeed * Time.smoothDeltaTime);
                panelRt.anchoredPosition = Vector2.MoveTowards(panelRt.anchoredPosition, panelTargetPos, panelMovementSpeed * Time.smoothDeltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
            isTapOnNext = false;
            Instruction =
                "1B and 1E must be RED, " +
                "using Rule #1:\n" +
                "There is no 3 cells of the same value next to each others";

            HighLightCell("1B", "1E");
            SetNextButtonInteractable(false);
            while (!isTapOnNext)
            {
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("1B"), LogicalBoard.VALUE_ONE);
                lb.SetValueNoInteract(s2i("1E"), LogicalBoard.VALUE_ONE);
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("1B"), LogicalBoard.VALUE_EMPTY);
                lb.SetValueNoInteract(s2i("1E"), LogicalBoard.VALUE_EMPTY);
                SetNextButtonInteractable(true);
            }
            isTapOnNext = false;
            SetNextButtonInteractable(false);

            lb.SetValueNoInteract(s2i("1B"), LogicalBoard.VALUE_ONE);
            lb.SetValueNoInteract(s2i("1E"), LogicalBoard.VALUE_ONE);
            lb.LockMatchedCell();
            lb.SetInteractableIndex(
                s2i("2C"),
                s2i("3B"),
                s2i("4A"),
                s2i("4D"),
                s2i("5C"),
                s2i("6B"));
            Instruction =
                "Similarly, " +
                "these highlighted cells must be BLUE. " +
                "Tap them once to fill in BLUE";
            HighLightCell("2C", "3B", "4A", "4D", "5C", "6B");
            yield return new WaitUntil(() =>
            {
                bool completed =
                    lb.GetValue(s2i("2C")) == LogicalBoard.VALUE_ZERO &&
                    lb.GetValue(s2i("3B")) == LogicalBoard.VALUE_ZERO &&
                    lb.GetValue(s2i("4A")) == LogicalBoard.VALUE_ZERO &&
                    lb.GetValue(s2i("4D")) == LogicalBoard.VALUE_ZERO &&
                    lb.GetValue(s2i("5C")) == LogicalBoard.VALUE_ZERO &&
                    lb.GetValue(s2i("6B")) == LogicalBoard.VALUE_ZERO;

                return completed;
            });
            lb.LockMatchedCell();

            lb.SetInteractableIndex(
                s2i("2A"),
                s2i("2D"),
                s2i("5A"));
            HighLightCell("2A", "2D", "5A");
            Instruction =
                "Same with these cells. " +
                "Tap them twice to fill in RED";
            yield return new WaitUntil(() =>
            {
                bool completed =
                    lb.GetValue(s2i("2A")) == LogicalBoard.VALUE_ONE &&
                    lb.GetValue(s2i("2D")) == LogicalBoard.VALUE_ONE &&
                    lb.GetValue(s2i("5A")) == LogicalBoard.VALUE_ONE;

                return completed;
            });
            lb.LockMatchedCell();

            isTapOnNext = false;
            Instruction =
                "Nice! " +
                "Look at 1A, it must be BLUE " +
                "because there's already " +
                "3 RED cells " +
                "on the first column. " +
                "Remember Rule #2: " +
                "BLUEs = REDs";
            HighLightCell("1A");
            SetNextButtonInteractable(false);
            while (!isTapOnNext)
            {
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("1A"), LogicalBoard.VALUE_ZERO);
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("1A"), LogicalBoard.VALUE_EMPTY);
                SetNextButtonInteractable(true);
            }
            isTapOnNext = false;
            SetNextButtonInteractable(false);
            lb.SetValueNoInteract(s2i("1A"), LogicalBoard.VALUE_ZERO);
            lb.LockMatchedCell();

            lb.SetInteractableIndex(
                s2i("1F"),
                s2i("6C"));
            HighLightCell("1F", "6C");
            Instruction =
                "Let's fill in these cells.\n" +
                "(It's easy, right?)";
            yield return new WaitUntil(() =>
            {
                bool completed =
                    lb.GetValue(s2i("1F")) == LogicalBoard.VALUE_ONE &&
                    lb.GetValue(s2i("6C")) == LogicalBoard.VALUE_ONE;

                return completed;
            });
            lb.LockMatchedCell();

            isTapOnNext = false;
            Instruction =
                "Good! " +
                "Here's an advanced tip, " +
                "5F CANNOT be RED, " +
                "because it will create " +
                "a BLUE-BLUE-BLUE pattern " +
                "on 5C 5D 5E\n" +
                "(Rule #1: No triple)";
            HighLightCell("5F");
            SetNextButtonInteractable(false);
            while (!isTapOnNext)
            {
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("5F"), LogicalBoard.VALUE_ZERO);
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("5F"), LogicalBoard.VALUE_EMPTY);
                SetNextButtonInteractable(true);
            }
            isTapOnNext = false;
            lb.SetValueNoInteract(s2i("5F"), LogicalBoard.VALUE_ZERO);
            lb.LockMatchedCell();

            lb.SetInteractableIndex(
                s2i("3F"));
            HighLightCell("3F");
            Instruction =
                "OK, let's apply that trick to find 3F";
            SetNextButtonInteractable(false);
            yield return new WaitUntil(() =>
            {
                bool completed =
                    lb.GetValue(s2i("3F")) == LogicalBoard.VALUE_ONE;

                return completed;
            });
            lb.LockMatchedCell();

            Instruction =
                "Very good! Here are some coins for the next step";
            coinText.text = (price.revealPowerup + price.undoPowerup).ToString();
            coinGroupAnim.Play(AnimConstant.IN);
            HighLightCell("coin");
            coinHighlightAnim.Play();
            SetNextButtonInteractable(true);
            isTapOnNext = false;
            yield return new WaitUntil(() =>
            {
                return isTapOnNext;
            });
            isTapOnNext = false;
            SetNextButtonInteractable(false);
            coinHighlightAnim.Stop();

            revealGroup.gameObject.SetActive(true);
            Instruction = string.Format(
                "You can reveal a cell " +
                "by tapping on the Hint button " +
                "on the lower-right corner.\n" +
                "It costs you {0} coins each time.\n" +
                "Tap to see how it work.", price.revealPowerup);
            revealGroupAnim.Play(AnimConstant.IN);
            HighLightCell("revealButton");
            revealHighlightAnim.Play();

            isRevealButtonClicked = false;
            yield return new WaitUntil(() =>
            {
                return isRevealButtonClicked;
            });
            isRevealButtonClicked = false;
            lb.RevealRandom();
            PlayLooseCoinAnim(price.revealPowerup, (revealGroup.transform as RectTransform));
            revealGroup.interactable = false;
            revealGroup.blocksRaycasts = false;
            revealButton.interactable = false;
            revealHighlightAnim.Stop();
            coinText.text = price.undoPowerup.ToString();

            yield return new WaitUntil(() =>
            {
                return !LogicalBoardTutorial.Instance.isPlayingRevealAnim;
            });

            HighLightCell();
            Instruction =
                "Try fill in any cell";
            lb.SetInteractableIndex(
                s2i("2E"),
                s2i("2F"),
                s2i("3D"),
                s2i("3E"),
                s2i("5D"),
                s2i("5E"),
                s2i("6D"),
                s2i("6E"));
            isSetAnyValue = false;
            LogicalBoardTutorial.onCellValueSet += CellValueSet;
            yield return new WaitUntil(() =>
            {
                return isSetAnyValue;
            });
            isSetAnyValue = false;
            LogicalBoardTutorial.onCellValueSet -= CellValueSet;

            lb.SetInteractableIndex();
            undoGroup.gameObject.SetActive(true);
            Instruction = string.Format(
                "You can undo previous moves " +
                "by tapping on the Undo button " +
                "on the lower-right corner.\n" +
                "It costs you {0} coin each time.\n" +
                "Tap to see how it work.", price.undoPowerup);
            HighLightCell("undoButton");
            undoGroupAnim.Play(AnimConstant.IN);
            undoHighlightAnim.Play();
            isUndoButtonClicked = false;
            yield return new WaitUntil(() =>
            {
                return isUndoButtonClicked;
            });
            isUndoButtonClicked = false;
            lb.Undo();
            PlayLooseCoinAnim(price.undoPowerup, (undoGroup.transform as RectTransform));
            undoGroup.interactable = false;
            undoGroup.blocksRaycasts = false;
            undoButton.interactable = false;
            undoHighlightAnim.Stop();

            coinText.text = "0";
            lb.SetInteractableIndex(
                s2i("2E"),
                s2i("2F"),
                s2i("3D"),
                s2i("3E"),
                s2i("5D"),
                s2i("5E"),
                s2i("6D"),
                s2i("6E"));

            HighLightCell();
            LogicalBoardTutorial.onPuzzleValidated += OnPuzzleValidated;
            Instruction =
                "Can you complete this level?";
            isPuzzleSolved = false;
            LogicalBoardTutorial.onPuzzleSolved += OnPuzzleSolved;
            yield return new WaitUntil(() =>
            {
                return isPuzzleSolved;
            });
            isPuzzleSolved = false;
            LogicalBoardTutorial.onPuzzleSolved -= OnPuzzleSolved;
            lb.LockMatchedCell();

            LogicalBoardTutorial.onPuzzleValidated -= OnPuzzleValidated;

            Instruction = string.Format(
                "Congratulation!\n" +
                "You've completed " +
                "the tutorial.\n" +
                "Tap on Finish " +
                "to {0}.",
                HadFinishTutorialBefore ? "go back" : "continue");
            //revealGroup.gameObject.SetActive(false);
            //undoGroup.gameObject.SetActive(false);
            //skipButton.gameObject.SetActive(false);
            skipButton.interactable = false;
            skipButtonAnim.Play(AnimConstant.OUT);
            revealGroupAnim.Play(AnimConstant.OUT);
            undoGroupAnim.Play(AnimConstant.OUT);
            coinGroupAnim.Play(AnimConstant.OUT);
            //nextButton.gameObject.SetActive(false);
            //finishButton.gameObject.SetActive(true);
            SetNextButtonInteractable(true);
            nextButtonText.text = "FINISH";
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(delegate
            {
                if (SceneLoadingManager.Instance != null)
                    SceneLoadingManager.Instance.LoadMainScene();
                else
                    SceneManager.LoadScene("Main");
            });
            PlayerPrefs.SetInt("FINISH_TUTORIAL", 1);
            leavesParticle.Play();
        }

        private void HighLightCell(params string[] s)
        {
            foreach (var e in hltDict)
            {
                e.Value.SetActive(false);
            }

            for (int i = 0; i < s.Length; ++i)
            {
                if (hltDict.ContainsKey(s[i]))
                {
                    hltDict[s[i]].SetActive(true);
                }
            }
        }

        private void CellValueSet(Index2D index, int value)
        {
            isSetAnyValue = true;
        }

        private void OnPuzzleValidated(ICollection<Index2D> errorCells)
        {
            string errorText =
                "You have got some error," +
                " fix it now.";
            if (errorCells.Count > 0)
            {
                Instruction = errorText;
            }
            else
            {
                if (Instruction.Equals(errorText))
                    Instruction =
                        "Almost there, " +
                        "can you finish it?";
            }
        }

        private void OnPuzzleSolved()
        {
            isPuzzleSolved = true;
        }

        public void TapOnNext()
        {
            isTapOnNext = true;
        }

        private void PlayLooseCoinAnim(int amount, RectTransform parent)
        {
            GameObject g = Instantiate(looseCoinPrefab);
            g.transform.SetParent(parent, false);
            (g.transform as RectTransform).anchoredPosition = Vector2.zero;
            g.GetComponentInChildren<Text>().text = string.Format("-{0}", amount);

            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                if (g != null)
                    Destroy(g);
            }, 1);
        }
    }
}