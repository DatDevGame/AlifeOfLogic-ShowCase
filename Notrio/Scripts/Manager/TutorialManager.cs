using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
namespace Takuzu
{
    [Obsolete("Use TutorialManager3 instead")]
    public class TutorialManager : MonoBehaviour
    {
        public LogicalBoardTutorial lb;
        public Text instructionText;
        public Text coinText;
        public Button revealButton;
        public CanvasGroup revealGroup;
        public Button undoButton;
        public CanvasGroup undoGroup;
        public Button skipButton;
        public Button finishButton;
        public Button nextButton;
        public Text skipButtonText;
        public SnappingScroller scroller;
        public ItemPriceProfile price;
        public GameObject hand;

        [Space]
        public GameObject highlighterTemplate;
        public GameObject revealButtonHighlighter;
        public GameObject undoButtonHighlighter;
        public GameObject coinHighlighter;

        private Dictionary<string, GameObject> hltDict;

        private bool isTapOnNext;
        private bool isTapOnEnterWalkthrough;
        private bool isRevealButtonClicked;
        private bool isUndoButtonClicked;
        private bool isSetAnyValue;
        private bool isPuzzleSolved;

        private string puzzle;
        private string solution;
        private int size;

        public bool HadFinishTutorialBefore
        {
            get
            {
                return PlayerPrefs.HasKey("FINISH_TUTORIAL");
            }
        }

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

        private void Init()
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
        }

        private void Start()
        {
            Init();

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

            StartCoroutine(CrInstructStepByStep());
        }

        public void TapOnNext()
        {
            isTapOnNext = true;
        }

        private IEnumerator CrInstructStepByStep()
        {
            coinText.text = "0";
            revealGroup.gameObject.SetActive(false);
            undoGroup.gameObject.SetActive(false);
            finishButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
            isTapOnNext = false;
            isTapOnEnterWalkthrough = false;
            scroller.SnapIndex = 0;
            scroller.Snap();

            while (!isTapOnEnterWalkthrough)
            {
                if (scroller.SnapIndex == 0)
                {
                    Instruction =
                        "Takuzu is a puzzle game\n" +
                        "in which you have to fill a NxN board\n" +
                        "with BLUEs and REDs\n" +
                        "to satisfy 3 simple rules\n";
                }
                else if (scroller.SnapIndex == 1)
                {
                    hand.SetActive(false);
                    Instruction =
                        "Rule #1\n" +
                        "There is no 3 cells of the same value\n" +
                        "next to each others\n";
                }
                else if (scroller.SnapIndex == 2)
                {
                    Instruction =
                        "Rule #2\n" +
                        "Each row/column must have\n" +
                        "an equal number of BLUEs and REDs\n";
                }
                else if (scroller.SnapIndex == 3)
                {
                    Instruction =
                        "Rule #3\n" +
                        "Each row/column are unique\n";
                }
                else if (scroller.SnapIndex == 4)
                {
                    Instruction =
                        "Sound easy, eh?\n" +
                        "Let's try it!";
                }
                yield return null;
            }
            isTapOnEnterWalkthrough = false;
            scroller.gameObject.SetActive(false);

            yield return new WaitForSeconds(0.5f);
            isTapOnNext = false;
            Instruction =
                "1B and 1E must be RED,\n" +
                "using Rule #1:\n" +
                "There is no 3 cells of the same value\n next to each others";
            lb.InitPuzzle(puzzle, solution);
            lb.SetInteractableIndex();
            HighLightCell("1B", "1E");
            nextButton.gameObject.SetActive(false);
            while (!isTapOnNext)
            {
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("1B"), LogicalBoard.VALUE_ONE);
                lb.SetValueNoInteract(s2i("1E"), LogicalBoard.VALUE_ONE);
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("1B"), LogicalBoard.VALUE_EMPTY);
                lb.SetValueNoInteract(s2i("1E"), LogicalBoard.VALUE_EMPTY);
                nextButton.gameObject.SetActive(true);
            }
            isTapOnNext = false;
            nextButton.gameObject.SetActive(false);

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
                "Similarly,\n" +
                "these highlighted cells must be BLUE\n" +
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
                "Same with these cells\n" +
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
                "Nice!\n" +
                "Look at 1A, it must be BLUE\n" +
                "because there's already 3 RED cells\n" +
                "on the first column.\n" +
                "Remember Rule #2:\n" +
                "Each row/column must have an equal number of BLUEs and REDs";
            HighLightCell("1A");
            nextButton.gameObject.SetActive(false);
            while (!isTapOnNext)
            {
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("1A"), LogicalBoard.VALUE_ZERO);
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("1A"), LogicalBoard.VALUE_EMPTY);
                nextButton.gameObject.SetActive(true);
            }
            isTapOnNext = false;
            nextButton.gameObject.SetActive(false);
            lb.SetValueNoInteract(s2i("1A"), LogicalBoard.VALUE_ZERO);
            lb.LockMatchedCell();

            lb.SetInteractableIndex(
                s2i("1F"),
                s2i("6C"));
            HighLightCell("1F", "6C");
            Instruction =
                "Let's fill in the highlighted cells.\n" +
                "(You know the answer, right?)";
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
                "Excellent!\n" +
                "Here's an advanced tip,\n" +
                "5F CANNOT be RED,\n" +
                "because it might create\n" +
                "a BLUE-BLUE-BLUE pattern on 5C 5D 5E\n" +
                "(Rule #1: No triple)";
            HighLightCell("5F");
            nextButton.gameObject.SetActive(false);
            while (!isTapOnNext)
            {
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("5F"), LogicalBoard.VALUE_ZERO);
                yield return new WaitForSeconds(0.5f);
                lb.SetValueNoInteract(s2i("5F"), LogicalBoard.VALUE_EMPTY);
                nextButton.gameObject.SetActive(true);
            }
            isTapOnNext = false;
            lb.SetValueNoInteract(s2i("5F"), LogicalBoard.VALUE_ZERO);
            lb.LockMatchedCell();

            lb.SetInteractableIndex(
                s2i("3F"));
            HighLightCell("3F");
            Instruction =
                "OK, let's apply that trick to find 3F";
            nextButton.gameObject.SetActive(false);
            yield return new WaitUntil(() =>
            {
                bool completed =
                    lb.GetValue(s2i("3F")) == LogicalBoard.VALUE_ONE;

                return completed;
            });
            lb.LockMatchedCell();

            Instruction =
                "Very good!\n" +
                "Here are some coins for the next step";
            coinText.text = (price.revealPowerup + price.undoPowerup).ToString();
            HighLightCell("coin");
            nextButton.gameObject.SetActive(true);
            isTapOnNext = false;
            yield return new WaitUntil(() =>
            {
                return isTapOnNext;
            });
            isTapOnNext = false;
            nextButton.gameObject.SetActive(false);

            revealGroup.gameObject.SetActive(true);
            Instruction = string.Format(
                "You can reveal a cell\n" +
                "by tapping on the Reveal button\n" +
                "on the lower-right corner.\n" +
                "It costs you {0} coins each time.\n" +
                "Tap to see how it work.", price.revealPowerup);
            HighLightCell("revealButton");
            isRevealButtonClicked = false;
            yield return new WaitUntil(() =>
            {
                return isRevealButtonClicked;
            });
            isRevealButtonClicked = false;
            lb.RevealRandom();
            revealGroup.interactable = false;
            revealGroup.blocksRaycasts = false;
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
                "You can undo the previous action\n" +
                "by tapping on the Undo button\n" +
                "on the lower-right corner.\n" +
                "It costs you {0} coin each time.\n" +
                "Tap to see how it work.", price.undoPowerup);
            HighLightCell("undoButton");
            isUndoButtonClicked = false;
            yield return new WaitUntil(() =>
            {
                return isUndoButtonClicked;
            });
            isUndoButtonClicked = false;
            lb.Undo();
            undoGroup.interactable = false;
            undoGroup.blocksRaycasts = false;
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
                "You've completed the tutorial.\n" +
                "Tap on Finish button to {0}.",
                HadFinishTutorialBefore ? "go back" : "continue");
            revealGroup.gameObject.SetActive(false);
            undoGroup.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
            finishButton.gameObject.SetActive(true);
            PlayerPrefs.SetInt("FINISH_TUTORIAL", 1);

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

        public void RevealButtonClicked()
        {
            isRevealButtonClicked = true;
        }

        public void UndoButtonClicked()
        {
            isUndoButtonClicked = true;
        }

        public void EnterWalkthroughButtonClicked()
        {
            isTapOnEnterWalkthrough = true;
        }

        private void CellValueSet(Index2D index, int value)
        {
            isSetAnyValue = true;
        }

        private void OnPuzzleValidated(ICollection<Index2D> errorCells)
        {
            string errorText = "You have got some error, fix it now.";
            if (errorCells.Count > 0)
            {
                Instruction = errorText;
            }
            else
            {
                if (Instruction.Equals(errorText))
                    Instruction = "Almost there, can you finish it?";
            }
        }

        private void OnPuzzleSolved()
        {
            isPuzzleSolved = true;
        }
    }
}
