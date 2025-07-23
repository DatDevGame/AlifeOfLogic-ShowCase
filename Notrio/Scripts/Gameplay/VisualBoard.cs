using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Takuzu
{
    public class VisualBoard : MonoBehaviour
    {
        [System.Serializable]
        public struct BoardConfigBySize
        {
            public int size;
            public int headerSize;
            public float offset;
        }
        public static VisualBoard Instance { get; private set; }
        public static Action onHeaderInitialized = delegate { };
        public static Action onInitialized = delegate { };
        public static Action onPuzzleShown = delegate { };
        public static Action onPuzzleHidden = delegate { };
        public static Action<Index2D> onCellFlagged = delegate { };

        public GameObject handTemplate;

        [SerializeField]
        private HandController handController;

        public Cell cellTemplate;
        public GameObject cellHeader;
        public GameObject boardBgTemplate;
        public List<BoardConfigBySize> boardConfig;
        public float highlightErrorDelay;
        public bool dontHideOnPuzzleSolved;
        public float puzzleSolvedHideDelay;
        public float[] totalTimeFlipList = new float[] { 2.2f, 2.5f, 2.8f, 3.1f };
        public float currentTotalTimeFlip;
        public ColorController cellBackgroundColorController;
        public ColorController boardBackgroundColorController;
        public ColorController cellHeaderColorController;
        public float borderThickness;
        public float shadowDistance;
        public Color shadowColor;
        [HideInInspector]
        public ICollection<Index2D> currentErrorCells;
        [Header("On puzzle solved")]
        public float shinyDelay;
        public int shinyLoop;
        public float shinyDurationPerLoop;

        private Cell[][] cells;
        private bool flagMode;

        private HashSet<Index2D> highlightedCells;
        private HashSet<Index2D> flaggedCells;
        private List<TextMesh> rowCountingHeader;
        private List<TextMesh> columnCountingHeader;
        private GameObject cellRoot;
        private GameObject headerRoot;
        [HideInInspector]
        public SpriteRenderer background;
        private SpriteRenderer shadow;
        private Coroutine highlightErrorCoroutine;
        public List<Sprite> flipSprites = new List<Sprite>();
        public string locationFlipSprite = "bgflip/";
        private string currentFlipSpriteName;
        public string CurrentFlipSpriteName
        {
            get
            {
                return currentFlipSpriteName;
            }
            private set
            {
                currentFlipSpriteName = value;
            }
        }

        public float flipCellDelay = 0.07f;
        public float timeFlip = 0.1f;
        public float timeHoldFlip = 0.25f;
        public Vector2Int MinMaxFlipNumber = new Vector2Int(3, 7);

        public int FlagCount
        {
            get
            {
                return flaggedCells != null ? flaggedCells.Count : 0;
            }
        }

        private Coroutine hideBoardCR;
        public void GetFlipSprites()
        {
            if (!SceneManager.GetActiveScene().name.Equals("Tutorial"))
                currentFlipSpriteName = "bg-flip-" + StoryPuzzlesSaver.Instance.GetCurrentLevel();
            else
                currentFlipSpriteName = "bg-flip-tutorial";
            if (!FlipBackGround.currentFlipBgName.Equals(currentFlipSpriteName))
            {
                FlipBackGround.UnloadSubSprites();
                FlipBackGround.UnloadMainSprite();
                flipSprites.Clear();
            }
            StartCoroutine(DelayOneFrame(() =>
            {
                FlipBackGround.GetMainSprite(currentFlipSpriteName);
                flipSprites = FlipBackGround.GetSubSprites(currentFlipSpriteName);
            }));
        }

        private IEnumerator DelayOneFrame(Action callback)
        {
            yield return null;
            callback();
        }

        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this;
        }

        private void OnEnable()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
            LogicalBoard.onPuzzleInitialized += OnPuzzleInitialized;
            LogicalBoard.onCellValueSet += OnCellValueSet;
            LogicalBoard.onPuzzleValidated += OnPuzzleValidated;
            LogicalBoard.onPuzzleSolved += OnPuzzleSolved;
            LogicalBoard.onRowCounted += OnRowCounted;
            LogicalBoard.onColumnCounted += OnColumnCounted;
            LogicalBoard.onPuzzleReseted += OnPuzzleReseted;
            LogicalBoard.onImmutableIndexAdded += OnImmutableIndexAdded;
            Powerup.onPowerupChanged += OnPowerupChanged;

        }

        private void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
            LogicalBoard.onPuzzleInitialized -= OnPuzzleInitialized;
            LogicalBoard.onCellValueSet -= OnCellValueSet;
            LogicalBoard.onPuzzleValidated -= OnPuzzleValidated;
            LogicalBoard.onPuzzleSolved -= OnPuzzleSolved;
            LogicalBoard.onRowCounted -= OnRowCounted;
            LogicalBoard.onColumnCounted -= OnColumnCounted;
            LogicalBoard.onPuzzleReseted -= OnPuzzleReseted;
            LogicalBoard.onImmutableIndexAdded -= OnImmutableIndexAdded;
            Powerup.onPowerupChanged -= OnPowerupChanged;
        }

        public bool IsInit()
        {
            return cells != null && cells.Length > 0;
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing && oldState == GameState.Paused)
            {
                //ShowPuzzle();
            }
            if (newState == GameState.Paused)
            {
                //HidePuzzle();
            }
            if (newState == GameState.Prepare && (oldState == GameState.Paused || oldState == GameState.GameOver))
            {
                StopAllCoroutines();
                Destroy(cellRoot);
                Destroy(headerRoot);
                cells = null;
            }
        }

        private void OnPuzzleInitialized(int[][] puzzle)
        {
            if (hideBoardCR != null)
                StopCoroutine(hideBoardCR);

            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                InputHandler.Instance.ResetAssistCursorPosition();
                InputHandler.Instance.EnableCursor();
            }, 1);

            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                highlightedCells = new HashSet<Index2D>();
                flaggedCells = new HashSet<Index2D>();
                setValues = new Dictionary<Index2D, int>();
                CreateGrid(puzzle);
                CreateHeader(puzzle);
                ShowPuzzle();
                onInitialized();
                if (!PuzzleManager.currentIsChallenge && !PuzzleManager.currentIsMultiMode)
                    GetFlipSprites();

                if (cellRoot != null && headerRoot != null && cellRoot.activeInHierarchy == true && headerRoot.activeInHierarchy == true)
                {
                    if (LogicalBoard.Instance != null)
                        LogicalBoard.Instance.CompleteCheckInitBoard();
                }

            }, 0);
        }

        private void OnImmutableIndexAdded(Index2D i)
        {
            Cell c = cells[i.row][i.column];
            c.Solve();
        }

        private void CreateGrid(int[][] puzzle)
        {
            if (cellRoot != null)
                Destroy(cellRoot);

            bool colorBlindMode = PersonalizeManager.ColorBlindFriendlyModeEnable;
            GameObject root = new GameObject("Cells");
            root.transform.position = Vector3.zero;
            cells = new Cell[puzzle.Length][];
            if (!SceneManager.GetActiveScene().name.Equals("Tutorial"))
                currentTotalTimeFlip = totalTimeFlipList[(cells.Length - 6) / 2];
            else
                currentTotalTimeFlip = 3f;
            for (int i = 0; i < puzzle.Length; ++i)
            {
                cells[i] = new Cell[puzzle[i].Length];
                for (int j = 0; j < puzzle[i].Length; ++j)
                {
                    Cell c = CreateCell(i, j, puzzle[i][j]);
                    c.SetColorBlindMode(colorBlindMode);
                    c.SetBackgroundColorController(cellBackgroundColorController);
                    c.transform.parent = root.transform;
                }
            }

            GameObject bg = Instantiate(boardBgTemplate);
            bg.transform.parent = root.transform;
            bg.transform.position = Vector2.one * (cells.Length / 2 - 0.5f);
            bg.transform.localScale = Vector3.one;
            background = bg.GetComponent<SpriteRenderer>();
            background.color = boardBackgroundColorController.color;
            background.size = Vector2.one * (cells.Length + borderThickness);
            InputHandler.Instance.SetCursorBound(bg.transform.position, background.size);

            GameObject sd = Instantiate(boardBgTemplate);
            sd.name = "BoardShadow";
            sd.transform.parent = root.transform;
            sd.transform.position = new Vector3(bg.transform.position.x, -shadowDistance, 0);
            sd.transform.localScale = Vector3.one;
            shadow = sd.GetComponent<SpriteRenderer>();
            shadow.color = shadowColor;
            shadow.size = new Vector2(background.size.x, 1);
            shadow.sortingOrder = background.sortingOrder - 1;

            cellRoot = root;
        }

        private void CreateHeader(int[][] puzzle)
        {
            if (headerRoot != null)
                Destroy(headerRoot);
            BoardConfigBySize config = boardConfig.Find((b) => { return b.size == puzzle.Length; });
            int headerSize = config.headerSize;

            GameObject root = new GameObject("Grid header");
            root.transform.position = Vector3.zero;

            //letter header
            Vector3 origin = new Vector3(0, puzzle.Length, 0);
            for (int i = 1; i <= puzzle.Length; ++i)
            {
                GameObject g = Instantiate(cellHeader, origin + Vector3.down * config.offset, Quaternion.identity);
                g.transform.parent = root.transform;
                TextMesh tm = g.GetComponent<TextMesh>();
                tm.text = char.ConvertFromUtf32(i + 64);
                tm.anchor = TextAnchor.LowerCenter;
                tm.fontSize = headerSize;
                g.GetComponent<CellHeader>().SetColorController(cellHeaderColorController);
                origin += Vector3.right;
            }

            //letter header
            origin = new Vector3(-1, puzzle.Length - 1, 0);
            for (int i = 1; i <= puzzle.Length; ++i)
            {
                GameObject g = Instantiate(cellHeader, origin + Vector3.right * config.offset, Quaternion.identity);
                g.transform.parent = root.transform;
                TextMesh tm = g.GetComponent<TextMesh>();
                tm.text = i.ToString();
                tm.anchor = TextAnchor.MiddleRight;
                tm.fontSize = headerSize;
                g.GetComponent<CellHeader>().SetColorController(cellHeaderColorController);
                origin += Vector3.down;
            }

            //row counting header
            rowCountingHeader = new List<TextMesh>();
            origin = new Vector3(puzzle.Length, 0, 0);
            for (int i = 1; i <= puzzle.Length; ++i)
            {
                GameObject g = Instantiate(cellHeader, origin + Vector3.left * config.offset, Quaternion.identity);
                g.transform.parent = root.transform;
                TextMesh tm = g.GetComponent<TextMesh>();
                tm.text = GetHeaderString(0, 0);
                tm.anchor = TextAnchor.MiddleLeft;
                tm.fontSize = headerSize;
                rowCountingHeader.Add(tm);
                g.GetComponent<CellHeader>().SetColorController(cellHeaderColorController);
                origin += Vector3.up;
            }

            //column counting header
            columnCountingHeader = new List<TextMesh>();
            origin = new Vector3(0, -1, 0);
            for (int i = 1; i <= puzzle.Length; ++i)
            {
                GameObject g = Instantiate(cellHeader, origin + Vector3.up * config.offset, Quaternion.identity);
                g.transform.parent = root.transform;
                TextMesh tm = g.GetComponent<TextMesh>();
                tm.text = GetHeaderString(0, 0);
                tm.anchor = TextAnchor.UpperCenter;
                tm.fontSize = headerSize;
                columnCountingHeader.Add(tm);
                g.GetComponent<CellHeader>().SetColorController(cellHeaderColorController);
                origin += Vector3.right;
            }

            headerRoot = root;
            headerRoot.SetActive(false);

            onHeaderInitialized();
        }
        public bool applyCurrentSkin = true;
        private Cell CreateCell(int i, int j, int value)
        {
            GameObject g = Instantiate(cellTemplate.gameObject, new Vector3(j, i, 0), Quaternion.identity);
            g.name = string.Format("Cell ({0}, {1})", i, j);
            //g.transform.localScale = GameManager.Instance ? GameManager.Instance.cellScale * Vector3.one : Vector3.one;
            Cell c = g.GetComponent<Cell>();
            c.SetValue(value);
            if (applyCurrentSkin)
            {
                c.SetSkin(SkinManager.GetActivatedSkin());
                c.listenToSkinChangedEvent = true;
            }
            else
            {
                c.SetSkin(SkinManager.Instance.availableSkin[0]);
            }
            cells[i][j] = c;
            return c;
        }

        public void ShowPuzzle(float delay = 0)
        {
            //StartCoroutine(CrShowPuzzle(delay));
            headerRoot.SetActive(true);
            Index2D[] indices = Utilities.GetShuffleIndicesArray2D(cells.Length, cells.Length);
            for (int i = 0; i < indices.Length; ++i)
            {
                cells[indices[i].row][indices[i].column].Show();
                //yield return null;
            }
            background.gameObject.SetActive(true);
            shadow.gameObject.SetActive(true);
            onPuzzleShown();
        }

        //private IEnumerator CrShowPuzzle(float delay)
        //{
        //    yield return new WaitForSeconds(delay);
        //    headerRoot.SetActive(true);
        //    Index2D[] indices = Utilities.GetShuffleIndicesArray2D(cells.Length, cells.Length);
        //    for (int i = 0; i < indices.Length; ++i)
        //    {
        //        cells[indices[i].row][indices[i].column].Show();
        //        //yield return null;
        //    }
        //    onPuzzleShown();
        //}

        public void HidePuzzle()
        {
            //StartCoroutine(CrHidePuzzle(delay));
            headerRoot.SetActive(false);
            Index2D[] indices = Utilities.GetShuffleIndicesArray2D(cells.Length, cells.Length);
            for (int i = 0; i < indices.Length; ++i)
            {
                cells[indices[i].row][indices[i].column].Hide();
                //yield return null;
            }
            background.gameObject.SetActive(false);
            shadow.gameObject.SetActive(false);
            onPuzzleHidden();
        }

        public void HidePuzzleDelay(float delay)
        {
            if (hideBoardCR != null)
                StopCoroutine(hideBoardCR);
            hideBoardCR = StartCoroutine(CrHidePuzzleDelay(delay));
        }

        public IEnumerator CrHidePuzzleDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HidePuzzle();
        }

        private IEnumerator HidePuzzleAfterTakeScreenshot()
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            HidePuzzle();
        }

        //private IEnumerator CrHidePuzzle(float delay)
        //{
        //    yield return new WaitForSeconds(delay);
        //    headerRoot.SetActive(false);
        //    Index2D[] indices = Utilities.GetShuffleIndicesArray2D(cells.Length, cells.Length);
        //    for (int i = 0; i < indices.Length; ++i)
        //    {
        //        cells[indices[i].row][indices[i].column].Hide();
        //        //yield return null;
        //    }
        //    onPuzzleHidden();
        //}

        private Dictionary<Index2D, int> setValues = new Dictionary<Index2D, int>();
        private void OnCellValueSet(Index2D i, int value)
        {
            if (setValues.ContainsKey(i))
            {
                setValues[i] = value;
            }
            else
            {
                setValues.Add(i, value);
            }
            Cell c = cells[i.row][i.column];
            c.SetValue(value);
            if (value == LogicalBoard.VALUE_EMPTY && c.isFlag)
            {
                c.HideFlag();
                flaggedCells.Remove(i);
            }
            if (value != LogicalBoard.VALUE_EMPTY && flagMode && !flaggedCells.Contains(i))
            {
                c.ShowFlag();
                flaggedCells.Add(i);
                onCellFlagged(i);
            }
        }

        public void HideAllSetCells()
        {
            if (cells == null)
                return;

            foreach (var k in setValues.Keys)
            {
                if (cells[k.row] != null)
                {
                    cells[k.row][k.column].SetValue(-1);
                }
            }
        }

        public void ShowAllSetCells()
        {
            if (cells == null)
                return;

            foreach (var k in setValues.Keys)
            {
                if (cells[k.row] != null)
                {
                    cells[k.row][k.column].SetValue(setValues[k]);
                }
            }
        }

        private void OnPuzzleValidated(ICollection<Index2D> errorCells)
        {
            if (highlightErrorCoroutine != null)
                StopCoroutine(highlightErrorCoroutine);
            currentErrorCells = errorCells;
            highlightErrorCoroutine = StartCoroutine(CrHighLightErrorDelay(errorCells));
        }

        private IEnumerator CrHighLightErrorDelay(ICollection<Index2D> errorCells)
        {
            yield return new WaitForSeconds(highlightErrorDelay);
            highlightedCells.ExceptWith(errorCells);
            foreach (Index2D i in highlightedCells)
            {
                cells[i.row][i.column].Unhighlight();
            }
            highlightedCells.Clear();
            foreach (Index2D i in errorCells)
            {
                cells[i.row][i.column].Highlight();
                highlightedCells.Add(i);
            }

#if UNITY_ANDROID || UNITY_IOS
            if (SystemInfo.supportsVibration && PersonalizeManager.VibrateEnable && errorCells.Count > 0)
            {
                Handheld.Vibrate();
            }
#endif
        }

        private void OnPuzzleSolved()
        {
            for (int i = 0; i < cells.Length; ++i)
            {
                for (int j = 0; j < cells[i].Length; ++j)
                {
                    //cells[i][j].Solve();
                    cells[i][j].isSolved = true;
                }
            }
            PlayShinyAnim();
            headerRoot.SetActive(false);
            if (!dontHideOnPuzzleSolved && !PuzzleManager.currentIsMultiMode)
                HidePuzzleDelay(puzzleSolvedHideDelay + ((!PuzzleManager.currentIsMultiMode && !PuzzleManager.currentIsChallenge) ? VisualBoard.Instance.currentTotalTimeFlip : 2.5f));
            if (!PuzzleManager.currentIsChallenge && !PuzzleManager.currentIsMultiMode)
                StartCoroutine(CR_SetAnimationForFlipImage());
        }

        IEnumerator CR_SetAnimationForFlipImage()
        {
            if (!FlipBackGround.currentFlipBgName.Equals(currentFlipSpriteName))
            {
                FlipBackGround.UnloadSubSprites();
                FlipBackGround.UnloadMainSprite();
                flipSprites.Clear();
            }
            FlipBackGround.GetMainSprite(currentFlipSpriteName);
            flipSprites = FlipBackGround.GetSubSprites(currentFlipSpriteName);
            if (!SceneManager.GetActiveScene().name.Equals("Tutorial"))
                yield return new WaitForSeconds(1);
            else
                yield return new WaitForSeconds(1);

            if (flipSprites == null)
                yield break;

            if (flipSprites.Count <= 0)
                yield break;

            float timeDelay = flipCellDelay;
            int countSprite = flipSprites.Count - 1;
            for (int i = 0; i < cells.Length; i++)
            {
                for (int j = cells[i].Length - 1; j >= 0; j--)
                {
                    cells[i][j].Solve();
                    cells[i][j].isSolved = true;
                    cells[i][j].SetFlipSprite(flipSprites[countSprite]);
                    if (countSprite > 0)
                        countSprite--;
                    cells[i][j].FlipOver(timeDelay * (i + j), timeDelay * (cells.Length + cells[i].Length - i - j - 2));
                    //cells[i][j].FlipOver(UnityEngine.Random.Range(0.1f, 0.1f), UnityEngine.Random.Range(0.1f, 0.5f));
                }
            }
        }

        private void PlayShinyAnim()
        {
            StartCoroutine(CrPlayShinyAnim());
        }

        private IEnumerator CrPlayShinyAnim()
        {
            yield return new WaitForSeconds(shinyDelay);
            List<Index2D> startCell = new List<Index2D>();
            for (int i = cells.Length - 1; i >= 0; --i)
            {
                startCell.Add(new Index2D(i, 0));
            }

            for (int i = 1; i < cells.Length; ++i)
            {
                startCell.Add(new Index2D(0, i));
            }

            for (int loop = 0; loop < shinyLoop; ++loop)
            {
                for (int i = 0; i < startCell.Count; ++i)
                {
                    int r = startCell[i].row;
                    int c = startCell[i].column;
                    while (r < cells.Length && c < cells.Length)
                    {
                        cells[r][c].PlayShinyAnim();
                        r += 1;
                        c += 1;
                    }

                    yield return new WaitForSeconds(shinyDurationPerLoop / (cells.Length * 2));
                }
                yield return new WaitForSeconds(shinyDurationPerLoop / (cells.Length * 2));
            }

            //Index2D[] index = Utilities.GetShuffleIndicesArray2D(cells.Length, cells.Length);
            //for (int i=0;i<index.Length;++i)
            //{
            //    cells[index[i].row][index[i].column].PlayShinyAnim();
            //    yield return new WaitForSeconds(0.05f);

            //}
        }

        private void OnPowerupChanged(PowerupType newType, PowerupType oldType)
        {
            flagMode = newType == PowerupType.Flag;
        }

        private void OnRowCounted(int i, Vector2 count)
        {
            rowCountingHeader[i].text = GetHeaderString((int) count.x, (int) count.y);
        }

        private void OnColumnCounted(int i, Vector2 count)
        {
            columnCountingHeader[i].text = GetHeaderString((int) count.x, (int) count.y);
        }

        public string GetHeaderString(int zeroCount, int oneCount)
        {
            return string.Format("{0}|{1}", zeroCount, oneCount);
        }

        public void ResetFlag()
        {
            foreach (Index2D i in flaggedCells)
            {
                Cell c = cells[i.row][i.column];
                c.HideFlag();
            }
            flaggedCells.Clear();
        }

        public void OnPuzzleReseted()
        {
            foreach (Index2D i in highlightedCells)
            {
                cells[i.row][i.column].UnhighlightImmedately();
            }
            highlightedCells.Clear();
            flaggedCells.Clear();
        }

        private void Update()
        {
            if (boardBackgroundColorController != null && background != null)
            {
                background.color = boardBackgroundColorController.color;
            }
            HighLightCursorPosition();
        }

        private void HighLightCursorPosition()
        {
            if (cells == null || cells.Length == 0)
                return;
            for (int i = 0; i < cells.Length; ++i)
            {
                for (int j = 0; j < cells[i].Length; ++j)
                {
                    cells[i][j].StopHighLight();
                }
            }
            if (InputHandler.Instance.cursor != null && InputHandler.Instance.cursor.gameObject.activeSelf && InputHandler.Instance.hideCursorForScreenShot == false)
            {
                Index2D index = ((Vector2) InputHandler.Instance.CursorPosition).ToIndex2D();
                if (!LogicalBoard.Instance.IsValidIndex(index))
                    return;
                cells[index.row][index.column].HighLightCell();
            }
        }

        public void ShowSymbols()
        {
            for (int i = 0; i < cells.Length; ++i)
            {
                for (int j = 0; j < cells[i].Length; ++j)
                {
                    cells[i][j].SetColorBlindMode(true);
                }
            }
        }
        public void ShowHandUI(List<Index2D> index)
        {
            if (cells == null && handController == null)
                return;
            List<Vector3> listPos = new List<Vector3>();
            for (int i = 0; i < index.Count; i++)
                listPos.Add(cells[index[i].row][index[i].column].transform.position);
            handController.ShowHand(listPos);
        }

        public bool GetHandState()
        {
            return handController.IsShowing;
        }

        public void HideHandUI()
        {
            if (handController == null)
                return;
            handController.HideHand();
        }

        public void SetInActiveRows(int[] rows)
        {
            StartCoroutine(CR_SetInActiveRows(rows));
        }

        public void SetActiveRows(int[] rows)
        {
            StartCoroutine(CR_SetActiveRows(rows));
        }

        public void SetInActive(Index2D id)
        {
            try
            {
                cells[id.row][id.column].SetInActiveColor();
            }
            catch (System.IndexOutOfRangeException e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        public void SetActive(Index2D id)
        {
            try
            {
                cells[id.row][id.column].SetActiveColor();
            }
            catch (System.IndexOutOfRangeException e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        public void ClearInActiveCellsImmediately()
        {
            if (cells == null)
                return;

            for (int i = 0; i < cells.Length; ++i)
            {
                if (cells[i] != null)
                {
                    for (int j = 0; j < cells[i].Length; ++j)
                    {
                        cells[i][j].SetActiveColor();
                    }
                }
            }
        }

        IEnumerator CR_SetInActiveRows(int[] rows)
        {
            foreach (var row in rows)
            {
                foreach (var cell in cells[row])
                {
                    cell.SetInActiveColor();
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        IEnumerator CR_SetActiveRows(int[] rows)
        {
            foreach (var row in rows)
            {
                foreach (var cell in cells[row])
                {
                    cell.SetActiveColor();
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        public void SetInActiveColumns(int[] columns)
        {
            StartCoroutine(CR_SetInActiveColumns(columns));
        }

        public void SetActiveColumns(int[] columns)
        {
            StartCoroutine(CR_SetActiveColumns(columns));
        }

        IEnumerator CR_SetActiveColumns(int[] columns)
        {
            for (int i = 0; i < cells.Length; ++i)
            {
                for (int j = 0; j < cells[i].Length; ++j)
                {
                    foreach (var column in columns)
                    {
                        if (j == column)
                        {
                            cells[i][j].SetActiveColor();
                            yield return new WaitForEndOfFrame();
                        }
                    }
                }
            }
        }

        IEnumerator CR_SetInActiveColumns(int[] columns)
        {
            for (int i = 0; i < cells.Length; ++i)
            {
                for (int j = 0; j < cells[i].Length; ++j)
                {
                    foreach (var column in columns)
                    {
                        if (j == column)
                        {
                            cells[i][j].SetInActiveColor();
                            yield return new WaitForEndOfFrame();
                        }
                    }
                }
            }
        }

        public void ClearInActiveCells()
        {
            StartCoroutine(CR_ClearInActiveCells());
        }

        IEnumerator CR_ClearInActiveCells()
        {
            for (int i = 0; i < cells.Length; ++i)
            {
                for (int j = 0; j < cells[i].Length; ++j)
                {
                    cells[i][j].SetActiveColor();
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        public void HighLightCurrentErrorCells()
        {
            if (highlightErrorCoroutine != null)
                StopCoroutine(highlightErrorCoroutine);
            highlightErrorCoroutine = StartCoroutine(CrHighLightErrorDelay(currentErrorCells));
        }

        public void StopHighlightErrorCells()
        {
            if (highlightErrorCoroutine != null)
                StopCoroutine(highlightErrorCoroutine);

            foreach (Index2D i in highlightedCells)
            {
                cells[i.row][i.column].Unhighlight();
            }
            highlightedCells.Clear();

        }
    }
}