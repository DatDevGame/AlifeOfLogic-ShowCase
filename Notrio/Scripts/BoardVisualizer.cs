using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Takuzu
{
    public class BoardVisualizer : MonoBehaviour
    {

        [System.Serializable]
        public struct BoardConfigBySize
        {
            public int size;
            public int headerSize;
            public float offset;
        }

        public Action onHeaderInitialized = delegate { };
        public Action onInitialized = delegate { };
        public Action onPuzzleShown = delegate { };
        public Action onPuzzleHidden = delegate { };
        public Action<Index2D> onCellFlagged = delegate { };

        [SerializeField]
        private HandController handController;

        public bool allowVibration = false;
        public BoardLogical boardLogical;
        public Transform container;
        public Cell cellTemplate;
        public GameObject cellHeader;
        public GameObject boardBgTemplate;
        public List<BoardConfigBySize> boardConfig;
        public float highlightErrorDelay;
        public bool dontHideOnPuzzleSolved;
        public float puzzleSolvedHideDelay;
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
        private SpriteRenderer background;
        private SpriteRenderer shadow;
        private Coroutine highlightErrorCoroutine;


        public int FlagCount
        {
            get
            {
                return flaggedCells != null ? flaggedCells.Count : 0;
            }
        }

        private void OnEnable()
        {
            boardLogical.onPuzzleInitialized += OnPuzzleInitialized;
            boardLogical.onCellValueSet += OnCellValueSet;
            boardLogical.onPuzzleValidated += OnPuzzleValidated;
            boardLogical.onPuzzleSolved += OnPuzzleSolved;
            boardLogical.onRowCounted += OnRowCounted;
            boardLogical.onColumnCounted += OnColumnCounted;
            boardLogical.onPuzzleReseted += OnPuzzleReseted;
            boardLogical.onImmutableIndexAdded += OnImmutableIndexAdded;
            //Powerup.onPowerupChanged += OnPowerupChanged;

        }

        private void OnDisable()
        {
            boardLogical.onPuzzleInitialized -= OnPuzzleInitialized;
            boardLogical.onCellValueSet -= OnCellValueSet;
            boardLogical.onPuzzleValidated -= OnPuzzleValidated;
            boardLogical.onPuzzleSolved -= OnPuzzleSolved;
            boardLogical.onRowCounted -= OnRowCounted;
            boardLogical.onColumnCounted -= OnColumnCounted;
            boardLogical.onPuzzleReseted -= OnPuzzleReseted;
            boardLogical.onImmutableIndexAdded -= OnImmutableIndexAdded;
            //Powerup.onPowerupChanged -= OnPowerupChanged;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void OnPuzzleInitialized(int[][] puzzle)
        {
            highlightedCells = new HashSet<Index2D>();
            flaggedCells = new HashSet<Index2D>();
            CreateGrid(puzzle);
            CreateHeader(puzzle);
            ShowPuzzle();
            onInitialized();
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
            root.transform.SetParent(container);
            root.transform.position = Vector3.zero;

            cells = new Cell[puzzle.Length][];
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
            boardBackgroundColorController.onColorChanged += (controller , color) =>
            {
                if (background != null)
                {
                    background.color = color;
                }
            };
            background.size = Vector2.one * (cells.Length + borderThickness);

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
            cellRoot.transform.localPosition = Vector3.zero;
        }

        private void CreateHeader(int[][] puzzle)
        {
            if (headerRoot != null)
                Destroy(headerRoot);
            BoardConfigBySize config = boardConfig.Find((b) => { return b.size == puzzle.Length; });
            int headerSize = config.headerSize;

            GameObject root = new GameObject("Grid header");
            root.transform.SetParent(container);
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
            headerRoot.transform.localPosition = Vector3.zero;
            headerRoot.SetActive(false);

            onHeaderInitialized();
        }
        public SkinScriptableObject skinSO;
        private Cell CreateCell(int i, int j, int value)
        {
            GameObject g = Instantiate(cellTemplate.gameObject, new Vector3(j, i, 0), Quaternion.identity, container);
            g.name = string.Format("Cell ({0}, {1})", i, j);
            //g.transform.localScale = GameManager.Instance ? GameManager.Instance.cellScale * Vector3.one : Vector3.one;
            Cell c = g.GetComponent<Cell>();
            c.SetValue(value);
            if (skinSO != null)
            {
                c.SetSkin(skinSO);
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

        private void OnCellValueSet(Index2D i, int value)
        {
            Cell c = cells[i.row][i.column];
            c.SetValue(value);
            if (value == BoardLogical.VALUE_EMPTY && c.isFlag)
            {
                c.HideFlag();
                flaggedCells.Remove(i);
            }
            if (value != BoardLogical.VALUE_EMPTY && flagMode && !flaggedCells.Contains(i))
            {
                c.ShowFlag();
                flaggedCells.Add(i);
                onCellFlagged(i);
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
            if (SystemInfo.supportsVibration && PersonalizeManager.VibrateEnable && errorCells.Count > 0 && allowVibration)
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
            if (!dontHideOnPuzzleSolved)
                StartCoroutine(CrHidePuzzleDelay(puzzleSolvedHideDelay));
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
            rowCountingHeader[i].text = GetHeaderString((int)count.x, (int)count.y);
        }

        private void OnColumnCounted(int i, Vector2 count)
        {
            columnCountingHeader[i].text = GetHeaderString((int)count.x, (int)count.y);
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

        public void SetInActiveColumns(int[] columns)
        {
            StartCoroutine(CR_SetInActiveColumns(columns));
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

        public void HighlightCells(Index2D[] index2D, float duration)
        {
            StartCoroutine(CrPlayHighlightCells(index2D, duration));
        }

        public void SetActiveCells(Index2D[] index2Ds, float duration)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                for (int j = 0; j < cells[i].Length; j++)
                {
                    bool containCurrentValue = false;
                    for (int k = 0; k < index2Ds.Length; k++)
                    {
                        if (i == index2Ds[k].row && j == index2Ds[k].column)
                        {
                            containCurrentValue = true;
                            break;
                        }
                    }
                    if (!containCurrentValue)
                    {
                        cells[i][j].SetInActiveColor(duration);
                    }
                }
            }
        }

        public void UnSetActiveCells(float duration)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                for (int j = 0; j < cells[i].Length; j++)
                {
                    cells[i][j].SetActiveColor(duration);
                }
            }
        }

        private IEnumerator CrPlayHighlightCells(Index2D[] index2D, float duration)
        {
            float t = 0;
            float maxAnimationTime = 0;
            for (int i = 0; i < index2D.Length; i++)
            {
                Cell highlightedCell = cells[index2D[i].row][index2D[i].column];
                if (highlightedCell.shinyAnim.duration > maxAnimationTime)
                    maxAnimationTime = highlightedCell.shinyAnim.duration;
            }
            while (t< duration)
            {
                for (int i = 0; i < index2D.Length; i++)
                {
                    Cell highlightedCell = cells[index2D[i].row][index2D[i].column];
                    highlightedCell.PlayShinyAnim();
                }
                yield return new WaitForSeconds(maxAnimationTime);
                t += maxAnimationTime;
            }
        }
    }
}