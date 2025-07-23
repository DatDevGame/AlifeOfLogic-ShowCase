using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Takuzu.Generator;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Takuzu
{
    public class LogicalBoard : MonoBehaviour
    {
        public static LogicalBoard Instance { get; protected set; }
        public static Action<int[][]> onPuzzleInitialized = delegate { };
        public static Action<Index2D> onCellClicked = delegate { };
        public static Action<Index2D, int> onCellValueSet = delegate { };
        public static Action<Index2D> onCellRevealed = delegate { };
        public static Action<ICollection<Index2D>> onPuzzleValidated = delegate { };
        public static Action onPuzzleSolved = delegate { };
        public static Action<int, Vector2> onRowCounted = delegate { };
        public static Action<int, Vector2> onColumnCounted = delegate { };
        public static Action onPuzzleReseted = delegate { };
        public static Action onCancelReset = delegate { };
        public static Action onSolvingError = delegate { };
        public static Action<string> onProgressReported = delegate { };
        public static Action<Index2D> onCellUndone = delegate { };
        public static Action onNoUndoAvailable = delegate { };
        public static Action<Index2D> onImmutableIndexAdded = delegate { };
        public static Action<Index2D> onCellAboutToReveal = delegate { };

        public const int VALUE_EMPTY = -1;
        public const int VALUE_ZERO = 0;
        public const int VALUE_ONE = 1;
        public const char CHAR_EMPTY = '.';
        public const char CHAR_ZERO = '0';
        public const char CHAR_ONE = '1';
        public const char CHAR_ZERO_LOCK = '3';
        public const char CHAR_ONE_LOCK = '4';

        public float validateDelay;
        public float revealAnimCycle;
        public bool isPlayingRevealAnim;
        public bool isSolvedEventLock;
        public bool isAutoSolving;
        public bool playRevealAnim;

        protected int puzzleSize;
        protected int[][] puzzle;
        protected string solution;
        protected string loadedProgress;
        [ShowInInspector] protected HashSet<Index2D> immutableIndex;
        protected Coroutine validateCoroutine;
        protected Action<Index2D> doActionOnCell = delegate { };
        protected Stack<Index2D> undoStack;
        protected string loadedImmutable;
        [HideInInspector]
        public bool renderTexturePurpose = false;
        public bool violateRule1 = false;
        public bool violateRule2 = false;
        public bool violateRule3 = false;

        private bool isOccuredBoardError = false;

        protected virtual void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this;
            doActionOnCell += SetNextValue;
        }

        protected virtual void OnEnable()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
            InputHandler.onMouseClick += OnMouseClick;
            Powerup.onPowerupChanged += OnPowerupChanged;
            VisualBoard.onInitialized += OnVisualBoardInitialized;
            VisualBoard.onHeaderInitialized += OnVisualBoardHeaderInitialized;
            VisualBoard.onPuzzleShown += OnPuzzleShown;
            VisualBoard.onPuzzleHidden += OnPuzzleHidden;
            PuzzleManager.onPuzzleSelected += OnPuzzleSelected;
            ProgressSavingScheduler.Tick += OnSchedulerTick;
        }

        protected virtual void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
            InputHandler.onMouseClick -= OnMouseClick;
            Powerup.onPowerupChanged -= OnPowerupChanged;
            VisualBoard.onInitialized -= OnVisualBoardInitialized;
            VisualBoard.onHeaderInitialized -= OnVisualBoardHeaderInitialized;
            VisualBoard.onPuzzleShown -= OnPuzzleShown;
            VisualBoard.onPuzzleHidden -= OnPuzzleHidden;
            PuzzleManager.onPuzzleSelected -= OnPuzzleSelected;
            ProgressSavingScheduler.Tick -= OnSchedulerTick;
        }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        protected virtual void OnApplicationPause(bool pause)
        {
            if (pause && (GameManager.Instance.GameState == GameState.Playing || GameManager.Instance.GameState == GameState.Paused))
            {
                ReportProgress();
            }
        }

        protected virtual void OnApplicationQuit()
        {
            if (GameManager.Instance.GameState == GameState.Playing || GameManager.Instance.GameState == GameState.Paused)
            {
                ReportProgress();
            }
        }
#endif

        private void OnSchedulerTick()
        {
            ReportProgress();
        }

        public IEnumerator AutoSolve()
        {
            isAutoSolving = true;
            for (int i = 0; i < solution.Length; ++i)
            {
                Index2D index = new Index2D(i / puzzleSize, i % puzzleSize);

                if (!IsImmutableIndex(index))
                {
                    SetValue(index, int.Parse(solution[i].ToString()));
                }
                yield return null;
            }
        }

        protected virtual void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Prepare && oldState == GameState.Paused)
            {
                ReportProgress();
            }
        }

        public void DisableInput()
        {
            IsInputEnabled = false;
        }

        public void EnableInput()
        {
            IsInputEnabled = true;
        }

        private bool IsInputEnabled = true;

        protected virtual void OnMouseClick(Vector2 mousePosition)
        {
            if (IsInputEnabled == false)
                return;
            try
            {
                Index2D index = mousePosition.ToIndex2D();
                if (!IsValidIndex(index))
                    return;
                if (IsImmutableIndex(index))
                    return;

                doActionOnCell(index);
                undoStack.Push(index);

                onCellClicked(index);
            }
            catch (Exception e)
            {
                Debug.LogError("Reported in LogicalBoard.OnMouseClick(): " + e.ToString());
            }
        }

        protected virtual void OnPowerupChanged(PowerupType newType, PowerupType oldType)
        {
            doActionOnCell = SetNextValue;
            if (newType == PowerupType.None || newType == PowerupType.Flag)
            {
                doActionOnCell = SetNextValue;
            }
            else if (newType == PowerupType.Reveal)
            {
                RevealRandom();
            }
            else if (newType == PowerupType.Clear)
            {
                ResetBoard();
            }
            else if (newType == PowerupType.Undo)
            {
                Undo();
            }
        }

        protected virtual void OnPuzzleSelected(string id, string puzzle, string solution, string progress)
        {
            renderTexturePurpose = false;
            isSolvedEventLock = false;
            isAutoSolving = false;
            undoStack = new Stack<Index2D>();
            immutableIndex = new HashSet<Index2D>();
            puzzleSize = Mathf.RoundToInt(Mathf.Sqrt(puzzle.Length));
            isPlayingRevealAnim = false;
            loadedProgress = progress;
            this.puzzle = new int[puzzleSize][];
            for (int i = 0; i < puzzleSize; ++i)
            {
                this.puzzle[i] = new int[puzzleSize];
                for (int j = 0; j < puzzleSize; ++j)
                {
                    char c = progress[i * puzzleSize + j];
                    if (c == CHAR_EMPTY)
                    {
                        this.puzzle[i][j] = VALUE_EMPTY;
                    }
                    else if (c == CHAR_ZERO || c == CHAR_ZERO_LOCK)
                    {
                        this.puzzle[i][j] = VALUE_ZERO;
                    }
                    else if (c == CHAR_ONE || c == CHAR_ONE_LOCK)
                    {
                        this.puzzle[i][j] = VALUE_ONE;
                    }
                }
            }

            this.solution = solution;
            StartCheckInitBoard();
            onPuzzleInitialized(this.puzzle);
        }

        private Coroutine checkInitBoardCR;
        public void StartCheckInitBoard()
        {
            if (checkInitBoardCR != null)
                StopCoroutine(checkInitBoardCR);
            checkInitBoardCR = StartCoroutine(CR_StartCheckInitBoard());
        }

        public void CompleteCheckInitBoard()
        {
            if (checkInitBoardCR != null)
                StopCoroutine(checkInitBoardCR);
        }

        IEnumerator CR_StartCheckInitBoard()
        {
            yield return new WaitForSeconds(1);
            isOccuredBoardError = true;
            SupportService.CustomLogger.GetLogger().LogError("Unexpected error when initialize puzzle");
            if (UIReferences.Instance != null)
            {
                UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.UNEXPECTED_ERROR, I2.Loc.ScriptLocalization.UNEXPECTED_ERROR_MSG, I2.Loc.ScriptLocalization.OK.ToUpper(), I2.Loc.ScriptLocalization.NO.ToUpper(),
                    delegate
                    {
                        GameManager.Instance.PrepareGame();
                        isOccuredBoardError = false;
                    }, null, null, false);
            }
        }

        protected virtual void OnVisualBoardInitialized()
        {
            for (int i = 0; i < puzzle.Length; ++i)
            {
                for (int j = 0; j < puzzle[0].Length; ++j)
                {
                    char c = loadedProgress[i * puzzleSize + j];
                    if (c == CHAR_ONE_LOCK || c == CHAR_ZERO_LOCK)
                    {
                        Index2D index = new Index2D(i, j);
                        AddImmutableIndex(index);
                    }
                }
            }
        }

        protected virtual void OnPuzzleShown()
        {
            if (validateCoroutine != null)
                StopCoroutine(validateCoroutine);
            validateCoroutine = StartCoroutine(CrValidateDelay());
        }

        protected virtual void OnPuzzleHidden()
        {
            StopAllCoroutines();
        }

        public bool IsValidIndex(int row, int column)
        {
            return IsValidIndex(new Index2D(row, column));
        }

        public bool IsValidIndex(Index2D i)
        {
            if (puzzle == null)
                return false;
            if (i.row < 0 || i.row >= puzzle.Length)
                return false;
            if (i.column < 0 || i.column >= puzzle[i.row].Length)
                return false;
            return true;
        }

        public bool IsImmutableIndex(int row, int column)
        {
            return IsImmutableIndex(new Index2D(row, column));
        }

        public bool IsImmutableIndex(Index2D i)
        {
            return immutableIndex != null && immutableIndex.Contains(i);
        }

        public void AddImmutableIndex(Index2D i)
        {
            if (!immutableIndex.Contains(i))
            {
                immutableIndex.Add(i);
                onImmutableIndexAdded(i);
            }
        }

        public virtual void SetNextValue(Index2D i)
        {
            int value = puzzle[i.row][i.column] == VALUE_ONE ? VALUE_EMPTY : puzzle[i.row][i.column] + 1;
            SetValue(i, value);
        }

        public virtual void SetValue(Index2D i, int value, bool validate = true)
        {
            puzzle[i.row][i.column] = value;
            onCellValueSet(i, value);
            CountRow(i.row);
            CountColumn(i.column);
            if (validate)
            {
                if (validateCoroutine != null)
                    StopCoroutine(validateCoroutine);
                validateCoroutine = StartCoroutine(CrValidateDelay());
            }
        }

        protected virtual void Reveal(Index2D i)
        {
            int value = int.Parse(solution[i.row * puzzleSize + i.column].ToString());
            SetValue(i, value);
            AddImmutableIndex(i);
            onCellRevealed(i);
        }

        public virtual void RevealRandom()
        {
            List<Index2D> mutableCell = new List<Index2D>();
            for (int i = 0; i < puzzle.Length; ++i)
            {
                for (int j = 0; j < puzzle[0].Length; ++j)
                {
                    Index2D index = new Index2D(i, j);
                    if (!IsImmutableIndex(index))
                        mutableCell.Add(index);
                }
            }

            if (mutableCell.Count > 0)
            {
                List<Index2D> emptyMutableCell = mutableCell.FindAll((index) => { return puzzle[index.row][index.column] == VALUE_EMPTY; });
                Index2D i;
                if (emptyMutableCell.Count > 0)
                {
                    i = emptyMutableCell[UnityEngine.Random.Range(0, emptyMutableCell.Count - 1)];
                }
                else
                {
                    i = mutableCell[UnityEngine.Random.Range(0, mutableCell.Count - 1)];
                }
                onCellAboutToReveal(i);
                AddImmutableIndex(i);
                StartCoroutine(CrRevealAnim(i));
            }
            else
                onCellRevealed(new Index2D(-1, -1)); //dirty fix if there is no cell to reveal (to reset powerup type)
        }

        public Index2D GetRandom(List<Index2D> excluded)
        {
            List<Index2D> mutableCell = new List<Index2D>();
            for (int i = 0; i < puzzle.Length; ++i)
            {
                for (int j = 0; j < puzzle[0].Length; ++j)
                {
                    Index2D index = new Index2D(i, j);
                    if (!IsImmutableIndex(index))
                        mutableCell.Add(index);
                }
            }
            if (mutableCell.Count > 0)
            {
                List<Index2D> emptyMutableCell = mutableCell.FindAll((index) => { return excluded.Contains(index) == false; });
                if (emptyMutableCell.Count > 0)
                {
                    return emptyMutableCell[UnityEngine.Random.Range(0, emptyMutableCell.Count - 1)];
                }
                else
                {
                    //No more cells to reveal
                    return new Index2D(-1, -1);
                }
            }
            else
            {
                return new Index2D(-1, -1);
            }
        }

        protected IEnumerator CrRevealAnim(Index2D i2d)
        {
            isPlayingRevealAnim = true;
            for (int i = 0; i < revealAnimCycle; ++i)
            {
                SetValue(i2d, VALUE_ZERO, false);
                isPlayingRevealAnim = true;
                yield return new WaitForSeconds(0.2f);
                SetValue(i2d, VALUE_ONE, false);
                isPlayingRevealAnim = true;
                yield return new WaitForSeconds(0.2f);
            }
            isPlayingRevealAnim = false;
            Reveal(i2d);
        }

        public bool IsPuzzleSolved()
        {
            string puzzleStr = Helper.PuzzleIntGridToString(puzzle);
            return puzzleStr.Equals(solution);
        }

        protected virtual IEnumerator CrValidateDelay()
        {
            yield return null;
            yield return new WaitForSeconds(validateDelay);
            ValidatePuzzle();
        }

        public bool[] lastTimeErrorsState = new bool[3] { false, false, false };
        public bool[] currentErrorsState = new bool[3] { false, false, false };

        public void ValidatePuzzle()
        {
            try
            {
                lastTimeErrorsState = new bool[3] { false, false, false };
                for (int i = 0; i < 3; i++)
                    lastTimeErrorsState[i] = currentErrorsState[i] == true;

                currentErrorsState = new bool[3] { false, false, false };

                HashSet<Index2D> errorCells = new HashSet<Index2D>();
                ValidateTripletRule(errorCells);
                ValidateEqualtyRule(errorCells);
                ValidateUniqueRule(errorCells);
                onPuzzleValidated(errorCells);

                if (errorCells.Count > 0)
                {
                    onSolvingError();
                }
                if (IsPuzzleSolved() && !isSolvedEventLock)
                {
                    isSolvedEventLock = true;
                    onPuzzleSolved();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Reported in LogicalBoard.ValidatePuzzle(): " + e.ToString());
            }
        }

        protected virtual void ValidateTripletRule(ICollection<Index2D> container)
        {
            bool tripletDetected = false;
            violateRule1 = false;
            for (int i = 0; i < puzzleSize; ++i)
            {
                for (int j = 0; j < puzzleSize; ++j)
                {
                    //horizontal triplet
                    tripletDetected =
                        IsValidIndex(i, j - 1) && puzzle[i][j - 1] == puzzle[i][j] &&
                        IsValidIndex(i, j + 1) && puzzle[i][j] == puzzle[i][j + 1] &&
                        puzzle[i][j] != VALUE_EMPTY;
                    if (tripletDetected)
                    {
                        container.Add(new Index2D(i, j - 1));
                        container.Add(new Index2D(i, j));
                        container.Add(new Index2D(i, j + 1));
                        currentErrorsState[0] = true;
                    }
                    if (tripletDetected)
                        violateRule1 = true;

                    //vertical triplet
                    tripletDetected =
                        IsValidIndex(i - 1, j) && puzzle[i - 1][j] == puzzle[i][j] &&
                        IsValidIndex(i + 1, j) && puzzle[i][j] == puzzle[i + 1][j] &&
                        puzzle[i][j] != VALUE_EMPTY;
                    if (tripletDetected)
                        violateRule1 = true;
                    if (tripletDetected)
                    {
                        container.Add(new Index2D(i - 1, j));
                        container.Add(new Index2D(i, j));
                        container.Add(new Index2D(i + 1, j));
                        currentErrorsState[0] = true;
                    }
                }
            }
        }

        protected virtual void ValidateEqualtyRule(ICollection<Index2D> container)
        {
            violateRule2 = false;
            for (int i = 0; i < puzzleSize; ++i)
            {
                ValidateEqualtyRuleForRow(container, i);
                ValidateEqualtyRuleForColumn(container, i);
            }
        }

        protected virtual void ValidateEqualtyRuleForRow(ICollection<Index2D> container, int rowIndex)
        {
            int errorValue = VALUE_EMPTY;
            int[] row = Helper.GetRow<int>(puzzle, rowIndex);
            if (Validator.CountOccurrences<int>(row, VALUE_ZERO) > puzzleSize * 0.5f)
            {
                errorValue = VALUE_ZERO;
            }
            else if (Validator.CountOccurrences<int>(row, VALUE_ONE) > puzzleSize * 0.5f)
            {
                errorValue = VALUE_ONE;
            }
            if (errorValue != VALUE_EMPTY)
            {
                violateRule2 = true;
                for (int j = 0; j < puzzleSize; ++j)
                {
                    if (puzzle[rowIndex][j] == errorValue)
                    {
                        container.Add(new Index2D(rowIndex, j));
                        currentErrorsState[1] = true;
                    }
                }
            }
        }

        protected virtual void ValidateEqualtyRuleForColumn(ICollection<Index2D> container, int columnIndex)
        {
            int errorValue = VALUE_EMPTY;
            int[] column = Helper.GetColumn<int>(puzzle, columnIndex);
            if (Validator.CountOccurrences<int>(column, VALUE_ZERO) > puzzleSize * 0.5f)
            {
                errorValue = VALUE_ZERO;
            }
            else if (Validator.CountOccurrences<int>(column, VALUE_ONE) > puzzleSize * 0.5f)
            {
                errorValue = VALUE_ONE;
            }
            if (errorValue != VALUE_EMPTY)
            {
                violateRule2 = true;
                for (int j = 0; j < puzzleSize; ++j)
                {
                    if (puzzle[j][columnIndex] == errorValue)
                    {
                        container.Add(new Index2D(j, columnIndex));
                        currentErrorsState[1] = true;
                    }
                }
            }
        }

        protected virtual void ValidateUniqueRule(ICollection<Index2D> container)
        {
            violateRule3 = false;
            ValidateUniqueRuleForRow(container);
            ValidateUniqueRuleForColumn(container);
        }

        protected virtual void ValidateUniqueRuleForRow(ICollection<Index2D> container)
        {
            for (int i = 0; i < puzzleSize - 1; ++i)
            {
                for (int j = i + 1; j < puzzleSize; ++j)
                {
                    int[] firstRow = Helper.GetRow(puzzle, i);
                    int[] secondRow = Helper.GetRow(puzzle, j);
                    if (Helper.ArraySequenceEquals(firstRow, secondRow, VALUE_EMPTY))
                    {
                        violateRule3 = true;
                        for (int k = 0; k < puzzleSize; ++k)
                        {
                            container.Add(new Index2D(i, k));
                            container.Add(new Index2D(j, k));
                            currentErrorsState[2] = true;
                        }
                    }
                }
            }
        }

        protected virtual void ValidateUniqueRuleForColumn(ICollection<Index2D> container)
        {
            for (int i = 0; i < puzzleSize - 1; ++i)
            {
                for (int j = i + 1; j < puzzleSize; ++j)
                {
                    int[] firstColumn = Helper.GetColumn(puzzle, i);
                    int[] secondColumn = Helper.GetColumn(puzzle, j);
                    if (Helper.ArraySequenceEquals(firstColumn, secondColumn, VALUE_EMPTY))
                    {
                        violateRule3 = true;
                        for (int k = 0; k < puzzleSize; ++k)
                        {
                            container.Add(new Index2D(k, i));
                            container.Add(new Index2D(k, j));
                            currentErrorsState[2] = true;
                        }
                    }
                }
            }
        }

        protected virtual void OnVisualBoardHeaderInitialized()
        {
            for (int i = 0; i < puzzleSize; ++i)
            {
                CountRow(i);
                CountColumn(i);
            }
        }

        protected virtual void CountRow(int i)
        {
            int[] row = Helper.GetRow(puzzle, i);
            int zeroCount = Validator.CountOccurrences(row, VALUE_ZERO);
            int oneCount = Validator.CountOccurrences(row, VALUE_ONE);
            onRowCounted(i, new Vector2(zeroCount, oneCount));
        }

        protected virtual void CountColumn(int i)
        {
            int[] column = Helper.GetColumn(puzzle, i);
            int zeroCount = Validator.CountOccurrences(column, VALUE_ZERO);
            int oneCount = Validator.CountOccurrences(column, VALUE_ONE);
            onColumnCounted(i, new Vector2(zeroCount, oneCount));
        }

        public void ResetBoard()
        {
            for (int i = 0; i < puzzleSize; ++i)
            {
                for (int j = 0; j < puzzleSize; ++j)
                {
                    if (!IsImmutableIndex(i, j))
                        SetValue(new Index2D(i, j), VALUE_EMPTY);
                }
            }
            onPuzzleReseted();
        }

        public void ReportProgress()
        {
            if (isOccuredBoardError == true)
                return;

            System.Text.StringBuilder progress = new System.Text.StringBuilder();

            for (int i = 0; i < puzzle.Length; ++i)
            {
                for (int j = 0; j < puzzle[i].Length; ++j)
                {
                    char c = puzzle[i][j] == -1 ? CHAR_EMPTY : puzzle[i][j] == 1 ? CHAR_ONE : CHAR_ZERO;
                    if (IsImmutableIndex(i, j))
                    {
                        if (c == CHAR_ONE)
                            c = CHAR_ONE_LOCK;
                        else if (c == CHAR_ZERO)
                            c = CHAR_ZERO_LOCK;
                    }
                    progress.Append(c);
                }
            }

            onProgressReported(progress.ToString());
        }

        public int GetValue(Index2D i)
        {
            if (!IsValidIndex(i))
                return VALUE_EMPTY;
            return puzzle[i.row][i.column];
        }

        public virtual void Undo()
        {
            if (!CanUndo())
            {
                onNoUndoAvailable();
                return;
            }

            Index2D peek = undoStack.Peek();
            undoStack.RemovePeekAll(peek);
            SetValue(peek, VALUE_EMPTY);
            onCellUndone(peek);
        }

        public bool CanUndo()
        {
            if (undoStack == null)
            {
                return false;
            }
            if (undoStack.Count == 0)
            {
                return false;
            }
            Index2D peek = undoStack.Peek();
            if (GetValue(peek).Equals(VALUE_EMPTY))
            {
                undoStack.RemovePeekAll(peek);
                return CanUndo();
            }

            if (IsImmutableIndex(peek))
            {
                undoStack.RemovePeekAll(peek);
                return CanUndo();
            }

            return true;
        }
        public string GetProgressImmediately()
        {
            System.Text.StringBuilder progress = new System.Text.StringBuilder();
            if (puzzle == null)
                return "";
            for (int i = 0; i < puzzle.Length; ++i)
            {
                for (int j = 0; j < puzzle[i].Length; ++j)
                {
                    char c = puzzle[i][j] == -1 ? CHAR_EMPTY : puzzle[i][j] == 1 ? CHAR_ONE : CHAR_ZERO;
                    if (IsImmutableIndex(i, j))
                    {
                        if (c == CHAR_ONE)
                            c = CHAR_ONE;
                        else if (c == CHAR_ZERO)
                            c = CHAR_ZERO;
                    }
                    progress.Append(c);
                }
            }
            return progress.ToString();
        }

        public virtual void SetValueNoInteract(Index2D i, int value)
        {
            puzzle[i.row][i.column] = value;
            onCellValueSet(i, value);
            CountRow(i.row);
            CountColumn(i.column);

            if (validateCoroutine != null)
                StopCoroutine(validateCoroutine);
            validateCoroutine = StartCoroutine(CrValidateDelay());
        }

        public void InitPuzzle(string puzzle, string solution, bool renderTexture = false)
        {
            renderTexturePurpose = renderTexture;
            undoStack = new Stack<Index2D>();
            immutableIndex = new HashSet<Index2D>();
            puzzleSize = Mathf.RoundToInt(Mathf.Sqrt(puzzle.Length));

            this.puzzle = new int[puzzleSize][];
            for (int i = 0; i < puzzleSize; ++i)
            {
                this.puzzle[i] = new int[puzzleSize];
                for (int j = 0; j < puzzleSize; ++j)
                {
                    string p = puzzle[i * puzzleSize + j].ToString();
                    if (p == Puzzle.DOT)
                    {
                        this.puzzle[i][j] = -1;
                    }
                    else
                    {
                        this.puzzle[i][j] = int.Parse(p);
                    }
                }
            }

            this.solution = solution;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < puzzle.Length; ++i)
            {
                char c = puzzle[i];
                sb.Append(
                    c == CHAR_ZERO ? CHAR_ZERO_LOCK :
                    c == CHAR_ONE ? CHAR_ONE_LOCK :
                    CHAR_EMPTY);
            }
            loadedProgress = sb.ToString();
            onPuzzleInitialized(this.puzzle);
        }
 

        public virtual float GetAccurateCompletionPercentage()
        {
            if (puzzle == null || solution == null || puzzle.Length == 0 || solution.Length != puzzleSize * puzzleSize)
                return 0f;

            int totalMutableCells = 0;
            int correctlyFilledCells = 0;

            for (int row = 0; row < puzzleSize; row++)
            {
                for (int col = 0; col < puzzleSize; col++)
                {
                    Index2D index = new Index2D(row, col);

                    if (IsImmutableIndex(index))
                        continue;

                    totalMutableCells++;

                    int currentValue = puzzle[row][col];
                    int correctValue = int.Parse(solution[row * puzzleSize + col].ToString());

                    if (currentValue != VALUE_EMPTY && currentValue == correctValue)
                    {
                        correctlyFilledCells++;
                    }
                }
            }

            if (totalMutableCells == 0)
                return 100f;

            return (float)correctlyFilledCells / totalMutableCells * 100f;
        }
    }
}