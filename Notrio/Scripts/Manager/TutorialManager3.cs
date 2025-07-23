using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;
using System;
using UnityEngine.SceneManagement;

namespace Takuzu
{
    public class TutorialManager3 : MonoBehaviour
    {
        public static Action onFinishTutorialFirstTime = delegate
        {
        };

        #region variables

        [Header("General")]
        public SnappingScroller introScroller;
        public GameObject introScrollerContainer;
        public ColorAnimation introScrollerFadingAnim;
        public CanvasGroup introScrollerGroup;
        public TutorialCellIllustrator board;
        public Button skipButton;
        public Text skipText;
        public Graphic skipUnderline;
        public Color skipIntroColor;
        public Color skipWalkthroughColor;
        public SpriteRenderer background;
        public Sprite introBackground;
        public Sprite walkthroughBackgroundDay;
        public Sprite walkthroughBackgroundNight;
        public float bgBlendSpeed;
        [Space]
        public Color blueCellColor;
        public Color redCellColor;
        public Color grayCellColor;

        public Text instructionText;

        public string Instruction
        {
            get
            {
                return instructionText.text ?? string.Empty;
            }
            set
            {
                //instructionText.text = value;
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

        [Header("Rule 2")]
        public List<TutorialCellIllustrator> redCellRule2;
        public List<TutorialCellIllustrator> blueCellsRule2;
        public List<TutorialCellIllustrator> grayCellRule2;
        public Vector2 blueCellPivotRule2;
        public Vector2 redCellPivotRule2;
        public float cellOffsetYRule2;
        public float animIntervalRule2;
        public float animCycleDelayRule2;

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

        [Header("Walkthrough invite")]
        public List<TutorialCellIllustrator> cellWalkthroughInvite;

        [Header("Walkthrough")]
        public Button enterWalkthroughButton;
        public PositionAnimation cameraSlidingAnim;
        public ColorAnimation scrollerFadingAnim;
        public SnappingScroller walkthroughScroller;
        public GameObject walkthroughScrollerIndexIndicator;
        public GameObject walkthroughScrollerContainer;
        [HideInInspector]
        public string puzzle;
        [HideInInspector]
        public string solution;
        [HideInInspector]
        public int size;
        public GameObject highlighterTemplate;
        public GameObject revealButtonHighlighter;
        public GameObject undoButtonHighlighter;
        public GameObject coinHighlighter;
        public Text timerText;
        public System.Diagnostics.Stopwatch stopwatch;
        public Button revealButton;
        public Button undoButton;
        public CanvasGroup revealGroup;
        public CanvasGroup undoGroup;
        public Text coinText;
        public ItemPriceProfile price;
        public Button finishButton;
        public PositionAnimation skipButtonAnim;
        public PositionAnimation headerAnim;
        public PositionAnimation revealGroupAnim;
        public PositionAnimation undoGroupAnim;
        public AnimController revealHighlightAnim;
        public AnimController undoHighlightAnim;
        public AnimController coinHighlightAnim;
        public GameObject looseCoinPrefab;
        public ParticleSystem leavesParticle;
        public ColorAnimation boardFadeAnim;
        public Text revealInstruction;
        public Text undoInstruction;
        public Text challengePlayerSolveInstruction;
        public Text finishInstruction;
        public GameObject swipeIndicator;
        public Text stepText;
        public PositionAnimation ribbonAnim;
        public AnimController[] swipeIndicatorAnim;

        public LogicalBoardTutorial lb;
        private Dictionary<string, GameObject> hltDict;
        private bool isSwipeNext;
        private bool isTapOnEnterWalkthrough;
        private bool isRevealButtonClicked;
        private bool isUndoButtonClicked;
        private bool isSetAnyValue;
        private bool isPuzzleSolved;

        #endregion

        private void Start()
        {
            skipButton.onClick.AddListener(delegate
                {
                    Skip();
                });

            enterWalkthroughButton.onClick.AddListener(delegate
                {
                    EnterWalkthrough();
                });

            introScroller.onSnapIndexChanged += OnSnapIndexChanged;

            CharacterFacialAnim facialAnim = FindObjectOfType<CharacterFacialAnim>();
            if (facialAnim != null)
            {
                facialAnim.dontLookYet = true;
            }

            Intro();
        }

        private void OnSnapIndexChanged(int n, int o)
        {
            StopAllCoroutines();
            if (n == 0)
            {
                Intro();
            }
            else if (n == 1)
            {
                Rule1();
            }
            else if (n == 2)
            {
                Rule2();
            }
            else if (n == 3)
            {
                Rule3();
            }
            else if (n == 4)
            {
                WalkthroughInvite();
            }
        }

        private void SetWalkthroughCanSwipeNext(bool i)
        {
            if (i == true)
                walkthroughScroller.lockDirection = SnappingScroller.LockDirection.Right;
            else
                walkthroughScroller.lockDirection = SnappingScroller.LockDirection.Both;

        }

        private void Intro()
        {
            MaterialPropertyBlock p = new MaterialPropertyBlock();
            background.GetPropertyBlock(p);
            p.SetTexture("_MainTex", introBackground.texture);
            p.SetFloat("_BlendFraction", 0);
            background.SetPropertyBlock(p);
            introScrollerContainer.SetActive(true);
            walkthroughScrollerContainer.SetActive(false);
            skipText.color = skipIntroColor;
            skipUnderline.color = skipIntroColor;
            StartCoroutine(CrIntroAnim());
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
            StartCoroutine(CrRule1Anim());
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
            while (true)
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
        }

        private void Rule2()
        {
            StartCoroutine(CrRule2Anim());
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
            while (true)
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

        private void Rule3()
        {
            StartCoroutine(CrRule3Anim());
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
            while (true)
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

        private void WalkthroughInvite()
        {
            StartCoroutine(CrWalkthroughInviteAnim());
        }

        private IEnumerator CrWalkthroughInviteAnim()
        {
            yield return null;
            board.targetPos = board.initPos;
            for (int i = 0; i < cellWalkthroughInvite.Count; ++i)
            {
                cellWalkthroughInvite[i].targetPos = cellWalkthroughInvite[i].initPos;
                cellWalkthroughInvite[i].targetColor = cellWalkthroughInvite[i].initColor;
                yield return null;
            }
        }

        private void EnterWalkthrough()
        {
            InitWalkthrough();
            StartCoroutine(CrWalkthrough());
        }

        private void InitWalkthrough()
        {
            introScrollerGroup.blocksRaycasts = false;
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

            introScrollerFadingAnim.Play(AnimConstant.OUT);
            CoroutineHelper.Instance.DoActionDelay(
                () =>
                {
                    introScrollerContainer.SetActive(false);
                    introScroller.onSnapIndexChanged = null;
                },
                introScrollerFadingAnim.duration);
        }

        private void OnWalkthroughScrollerSnapIndexChanged(int n, int o)
        {
            isSwipeNext = true;
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
                return PlayerDb.HasKey(PlayerDb.FINISH_TUTORIAL_KEY) || PlayerPrefs.HasKey(PlayerDb.FINISH_TUTORIAL_KEY);
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
            coinText.text = "0";
            revealGroup.gameObject.SetActive(false);
            undoGroup.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(true);
            skipText.color = skipWalkthroughColor;
            skipUnderline.color = skipWalkthroughColor;
            SetWalkthroughCanSwipeNext(false);
            isSwipeNext = false;

            boardFadeAnim.Play(AnimConstant.OUT);
            Sprite targetSprite = PersonalizeManager.NightModeEnable ? walkthroughBackgroundNight : walkthroughBackgroundDay;
            float blend = 0;
            MaterialPropertyBlock p = new MaterialPropertyBlock();
            background.GetPropertyBlock(p);
            p.SetTexture("_MainTex", introBackground.texture);
            p.SetTexture("_SecondaryTex", targetSprite.texture);
            p.SetFloat("_BlendFraction", blend);
            background.SetPropertyBlock(p);
            while (true)
            {
                blend += Time.deltaTime * bgBlendSpeed;
                p.SetFloat("_BlendFraction", blend);
                background.SetPropertyBlock(p);
                if (blend >= 1)
                    break;
                yield return null;
            }
            p.SetFloat("_BlendFraction", 1);
            background.SetPropertyBlock(p);

            headerAnim.Play(AnimConstant.IN);
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            yield return new WaitForSeconds(1);
            walkthroughScrollerContainer.SetActive(true);
            walkthroughScroller.onSnapIndexChanged += OnWalkthroughScrollerSnapIndexChanged;
            scrollerFadingAnim.Play(AnimConstant.IN);
            cameraSlidingAnim.Play(AnimConstant.IN);

            CharacterFacialAnim facialAnim = FindObjectOfType<CharacterFacialAnim>();
            if (facialAnim != null)
            {
                facialAnim.dontLookYet = false;
            }

            lb.InitPuzzle(puzzle, solution);
            lb.SetInteractableIndex();
            yield return null;
            VisualBoard.Instance.ShowSymbols();

            yield return new WaitForSeconds(0.5f);
            isSwipeNext = false;
            //            Instruction =
            //                "1B and 1E must be RED, " +
            //            "using Rule #1:\n" +
            //            "There is no 3 cells of the same value next to each others";

            HighLightCell("1B", "1E");
            SetWalkthroughCanSwipeNext(false);
            bool isSwipeIndicatorShown = false;
            while (!isSwipeNext)
            {
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("1B"), LogicalBoard.VALUE_ONE);
                lb.SetValueNoInteract(s2i("1E"), LogicalBoard.VALUE_ONE);
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("1B"), LogicalBoard.VALUE_EMPTY);
                lb.SetValueNoInteract(s2i("1E"), LogicalBoard.VALUE_EMPTY);

                if (!isSwipeIndicatorShown)
                {
                    yield return new WaitForSeconds(1f);
                    SetWalkthroughCanSwipeNext(true);
                    ShowSwipeIndicator();
                    isSwipeIndicatorShown = true;
                }
            }
            isSwipeNext = false;
            SetWalkthroughCanSwipeNext(false);
            HideSwipeIndicator();
            isSwipeIndicatorShown = false;

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
            //            Instruction =
            //                "Similarly, " +
            //            "these highlighted cells must be BLUE. " +
            //            "Tap them once to fill in BLUE";
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
                    walkthroughScroller.SnapIndex = 1;
                    return completed;
                });
            lb.LockMatchedCell();
            walkthroughScroller.SnapIndex = 2;

            lb.SetInteractableIndex(
                s2i("2A"),
                s2i("2D"),
                s2i("5A"));
            HighLightCell("2A", "2D", "5A");
            //            Instruction =
            //                "Same with these cells. " +
            //            "Tap them twice to fill in RED";
            yield return new WaitUntil(() =>
                {
                    bool completed =
                        lb.GetValue(s2i("2A")) == LogicalBoard.VALUE_ONE &&
                        lb.GetValue(s2i("2D")) == LogicalBoard.VALUE_ONE &&
                        lb.GetValue(s2i("5A")) == LogicalBoard.VALUE_ONE;

                    return completed;
                });
            lb.LockMatchedCell();
            walkthroughScroller.SnapIndex = 3;

            isSwipeNext = false;
            //            Instruction =
            //                "Nice! " +
            //            "Look at 1A, it must be BLUE " +
            //            "because there's already " +
            //            "3 RED cells " +
            //            "on the first column. " +
            //            "Remember Rule #2: " +
            //            "BLUEs = REDs";
            HighLightCell("1A");
            SetWalkthroughCanSwipeNext(false);
            while (!isSwipeNext)
            {
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("1A"), LogicalBoard.VALUE_ZERO);
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("1A"), LogicalBoard.VALUE_EMPTY);

                if (!isSwipeIndicatorShown)
                {
                    yield return new WaitForSeconds(1f);
                    SetWalkthroughCanSwipeNext(true);
                    ShowSwipeIndicator();
                    isSwipeIndicatorShown = true;
                }
            }
            walkthroughScroller.SnapIndex = 4;
            isSwipeNext = false;
            SetWalkthroughCanSwipeNext(false);
            HideSwipeIndicator();
            isSwipeIndicatorShown = false;

            lb.SetValueNoInteract(s2i("1A"), LogicalBoard.VALUE_ZERO);
            lb.LockMatchedCell();

            lb.SetInteractableIndex(
                s2i("1F"),
                s2i("6C"));
            HighLightCell("1F", "6C");
            //            Instruction =
            //                "Let's fill in these cells.\n" +
            //            "(It's easy, right?)";
            yield return new WaitUntil(() =>
                {
                    bool completed =
                        lb.GetValue(s2i("1F")) == LogicalBoard.VALUE_ONE &&
                        lb.GetValue(s2i("6C")) == LogicalBoard.VALUE_ONE;
                    walkthroughScroller.SnapIndex = 4;
                    return completed;
                });
            lb.LockMatchedCell();
            walkthroughScroller.SnapIndex = 5;

            isSwipeNext = false;
            //            Instruction =
            //                "Good! " +
            //            "Here's an advanced tip, " +
            //            "5F CANNOT be RED, " +
            //            "because it will create " +
            //            "a BLUE-BLUE-BLUE pattern " +
            //            "on 5C 5D 5E\n" +
            //            "(Rule #1: No triple)";
            HighLightCell("5F");
            SetWalkthroughCanSwipeNext(false);
            while (!isSwipeNext)
            {
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("5F"), LogicalBoard.VALUE_ZERO);
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("5F"), LogicalBoard.VALUE_EMPTY);
                walkthroughScroller.SnapIndex = 5;
                if (!isSwipeIndicatorShown)
                {
                    yield return new WaitForSeconds(2);
                    SetWalkthroughCanSwipeNext(true);
                    ShowSwipeIndicator();
                    isSwipeIndicatorShown = true;
                }
            }
            walkthroughScroller.SnapIndex = 6;
            isSwipeNext = false;
            lb.SetValueNoInteract(s2i("5F"), LogicalBoard.VALUE_ZERO);
            lb.LockMatchedCell();

            lb.SetInteractableIndex(
                s2i("3F"));
            HighLightCell("3F");
            Instruction =
                "OK, let's apply that trick to find 3F";
            SetWalkthroughCanSwipeNext(false);
            HideSwipeIndicator();
            isSwipeIndicatorShown = false;

            yield return new WaitUntil(() =>
                {
                    bool completed =
                        lb.GetValue(s2i("3F")) == LogicalBoard.VALUE_ONE;

                    return completed;
                });
            lb.LockMatchedCell();
            walkthroughScroller.SnapIndex = 7;

            //            Instruction =
            //                "Very good! Here are some coins for the next step";
            coinText.text = (price.revealPowerup + price.undoPowerup).ToString();
            //coinGroupAnim.Play(AnimConstant.IN);
            HighLightCell("coin");
            coinHighlightAnim.Play();
            isSwipeNext = false;
            if (!isSwipeIndicatorShown)
            {
                yield return new WaitForSeconds(3);
                SetWalkthroughCanSwipeNext(true);
                ShowSwipeIndicator();
                isSwipeIndicatorShown = true;
            }
            yield return new WaitUntil(() =>
                {
                    return isSwipeNext;
                });
            walkthroughScroller.SnapIndex = 8;
            isSwipeNext = false;
            SetWalkthroughCanSwipeNext(false);
            HideSwipeIndicator();
            isSwipeIndicatorShown = false;

            coinHighlightAnim.Stop();

            revealGroup.gameObject.SetActive(true);
            //            Instruction = string.Format(
            //                "You can reveal a cell " +
            //                "by tapping on the Hint button " +
            //                "on the lower-right corner.\n" +
            //                "It costs you {0} coins each time.\n" +
            //                "Tap to see how it work.", price.revealPowerup);
            revealInstruction.text = revealInstruction.text
                .Replace("#PRICE", price.revealPowerup.ToString())
                .Replace("#S", price.revealPowerup >= 2 ? "s" : string.Empty);
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
            walkthroughScroller.SnapIndex = 9;

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
            walkthroughScroller.SnapIndex = 10;

            lb.SetInteractableIndex();
            undoGroup.gameObject.SetActive(true);
            //            Instruction = string.Format(
            //                "You can undo previous moves " +
            //                "by tapping on the Undo button " +
            //                "on the lower-right corner.\n" +
            //                "It costs you {0} coin each time.\n" +
            //                "Tap to see how it work.", price.undoPowerup);
            undoInstruction.text = undoInstruction.text
                .Replace("#PRICE", price.undoPowerup.ToString())
                .Replace("#S", price.undoPowerup >= 2 ? "s" : string.Empty);
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
            walkthroughScroller.SnapIndex = 11;
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
            walkthroughScroller.SnapIndex = 12;
            LogicalBoardTutorial.onPuzzleValidated -= OnPuzzleValidated;

            //            Instruction = string.Format(
            //                "Congratulation!\n" +
            //                "You've completed " +
            //                "the tutorial.\n" +
            //                "Tap on Finish " +
            //                "to {0}.",
            //                HadFinishTutorialBefore ? "go back" : "continue");
            finishInstruction.text = finishInstruction.text.Replace("#ACTION", HadFinishTutorialBefore ? "go back" : "continue");
            walkthroughScrollerIndexIndicator.SetActive(false);
            stepText.gameObject.SetActive(false);
            skipButton.interactable = false;
            skipButtonAnim.Play(AnimConstant.OUT);
            revealGroupAnim.Play(AnimConstant.OUT);
            undoGroupAnim.Play(AnimConstant.OUT);
            //coinGroupAnim.Play(AnimConstant.OUT);
            headerAnim.Play(AnimConstant.OUT);
            SetWalkthroughCanSwipeNext(false);

            leavesParticle.Clear();
            leavesParticle.Play();
            ribbonAnim.Play(AnimConstant.IN);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver, true);

            finishButton.onClick.AddListener(delegate
                {
                    if (!HadFinishTutorialBefore)
                        onFinishTutorialFirstTime();

                    PlayerDb.SetInt(PlayerDb.FINISH_TUTORIAL_KEY, 1);
                    PlayerPrefs.SetInt(PlayerDb.FINISH_TUTORIAL_KEY, 1);

                    if (SceneLoadingManager.Instance != null)
                        SceneLoadingManager.Instance.LoadMainScene();
                    else
                        SceneManager.LoadScene("Main");
                });

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
                "Uh oh... looks like there're some errors." +
                "\nCan you fix them?";
            if (errorCells.Count > 0)
            {
                challengePlayerSolveInstruction.text = errorText;
            }
            else
            {
                if (challengePlayerSolveInstruction.text.Equals(errorText))
                    challengePlayerSolveInstruction.text =
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
            isSwipeNext = true;
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

        private void ShowSwipeIndicator()
        {
            swipeIndicator.SetActive(true);
            for (int i = 0; i < swipeIndicatorAnim.Length; ++i)
            {
                swipeIndicatorAnim[i].Play();
            }
        }

        private void HideSwipeIndicator()
        {
            for (int i = 0; i < swipeIndicatorAnim.Length; ++i)
            {
                swipeIndicatorAnim[i].Stop();
            }
        }

        private void Skip()
        {
            if (!HadFinishTutorialBefore)
                onFinishTutorialFirstTime();

            PlayerDb.SetInt(PlayerDb.FINISH_TUTORIAL_KEY, 1);
            PlayerPrefs.SetInt(PlayerDb.FINISH_TUTORIAL_KEY, 1);

            if (SceneLoadingManager.Instance != null)
                SceneLoadingManager.Instance.LoadMainScene();
            else
                SceneManager.LoadScene("Main");
        }

        private void Update()
        {
            if (stopwatch != null && timerText != null)
            {
                timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds);
            }

            if (stepText != null)
            {
                stepText.text = string.Format("{0}|{1}",
                    walkthroughScroller.SnapIndex + 1,
                    walkthroughScroller.ElementCount);
            }
        }
    }
}
