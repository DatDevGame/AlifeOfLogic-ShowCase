using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Takuzu.Generator
{
    public class TakuzuViewerEditor : EditorWindow
    {
        private string db;
        private string[][] puzzleGrid;
        private string[][] solutionGrid;
        private int selectedPuzzleIndex;
        private int gridSize;
        private string[] gridSizeEnumText;
        private int[] gridSizeEnumValue;
        private bool showSolution;
        private bool sortByLevel;
        private int levelOfDifficulty;
        private string[] difficultyEnumText;
        private List<Puzzle> loadedPuzzles;
        private List<int> loadedPuzzlesId;
        private Vector2 scrollPos;
        private string lastDb;
        private int lastGridSize;
        private int lastDifficulty;
        private string lastPack;
        private bool lastSortByLevel;
        private GUIStyle scrollViewStyle;
        private GUIStyle oddCellStyle;
        private GUIStyle evenCellStyle;
        private GUIStyle removeButtonStyle;
        private string packName;
        private CryptoKey key;
        private bool useKeyForDecryption;

        private const string DB_KEY = "VIEWER_DB";
        private const string SIZE_KEY = "VIEWER_SIZE";
        private const string SHOW_SOLUTION_KEY = "VIEWER_SHOW_SOLUTION";
        private const string SORT_KEY = "VIEWER_SORT";
        private const string CRYPTO_KEY_KEY = "VIEWER_CRYPTO_KEY";
        private const string USE_CRYPTO_KEY = "VIEWER_USE_CRYPTO";
        private const string PACK_KEY = "VIEWER_PACK";
        private const string DIFFICULTY_KEY = "VIEWER_DIFFICULTY";

        [MenuItem("Tools/Takuzu Viewer", priority = 1)]
        public static void ShowWindow()
        {
            TakuzuViewerEditor window = GetWindow<TakuzuViewerEditor>("Takuzu viewer");
            window.minSize = new Vector2(830, 600);
            window.Show();
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            PackSelector.OnPackSelected += OnPackSelected;

            gridSizeEnumText = new string[7] { "6x6", "8x8", "10x10", "12x12", "14x14", "16x16", "18x18" };
            gridSizeEnumValue = new int[7] { 6, 8, 10, 12, 14, 16, 18 };
            gridSize = gridSizeEnumValue[0];

            System.Array enumValue = System.Enum.GetValues(typeof(Level));
            difficultyEnumText = new string[enumValue.Length];
            for (int i = 0; i < enumValue.Length; ++i)
            {
                difficultyEnumText[i] = enumValue.GetValue(i).ToString();
            }

            scrollViewStyle = new GUIStyle();
            scrollViewStyle.padding = new RectOffset(10, 23, 0, 0);
            packName = "";

            oddCellStyle = new GUIStyle(EditorCommon.OddItemStyle);
            evenCellStyle = new GUIStyle(EditorCommon.EvenItemStyle);

            Color removeButtonColor = new Color32(234, 0, 0, 255);
            removeButtonStyle = new GUIStyle();
            Texture2D removeButtonBackground = new Texture2D(1, 1);
            removeButtonBackground.SetPixels32(new Color32[1] { removeButtonColor });
            removeButtonBackground.Apply();
            removeButtonStyle.normal.background = removeButtonBackground;
            removeButtonStyle.alignment = TextAnchor.MiddleCenter;
            removeButtonStyle.fontStyle = FontStyle.Bold;
            removeButtonStyle.normal.textColor = Color.white;
            Texture2D removeButtonHoverBackground = new Texture2D(1, 1);
            removeButtonHoverBackground.SetPixels32(new Color32[1] { removeButtonColor * 1.2f });
            removeButtonHoverBackground.Apply();
            removeButtonStyle.hover.background = removeButtonHoverBackground;
            removeButtonStyle.hover.textColor = Color.white;

            LoadParams();
        }

        private void OnDestroy()
        {
            PackSelector.OnPackSelected -= OnPackSelected;
            SaveParams();
        }

        private void LoadParams()
        {
            db = EditorPrefs.GetString(DB_KEY);
            gridSize = EditorPrefs.GetInt(SIZE_KEY);
            showSolution = EditorPrefs.GetBool(SHOW_SOLUTION_KEY);
            sortByLevel = EditorPrefs.GetBool(SORT_KEY);
            key = AssetDatabase.LoadAssetAtPath<CryptoKey>(EditorPrefs.GetString(CRYPTO_KEY_KEY));
            useKeyForDecryption = EditorPrefs.GetBool(USE_CRYPTO_KEY);
            packName = EditorPrefs.GetString(PACK_KEY);
            levelOfDifficulty = EditorPrefs.GetInt(DIFFICULTY_KEY);
        }

        private void SaveParams()
        {
            EditorPrefs.SetString(DB_KEY, db);
            EditorPrefs.SetInt(SIZE_KEY, gridSize);
            EditorPrefs.SetBool(SHOW_SOLUTION_KEY, showSolution);
            EditorPrefs.SetBool(SORT_KEY, sortByLevel);
            EditorPrefs.SetString(CRYPTO_KEY_KEY, AssetDatabase.GetAssetPath(key));
            EditorPrefs.SetBool(USE_CRYPTO_KEY, useKeyForDecryption);
            EditorPrefs.SetString(PACK_KEY, packName);
            EditorPrefs.SetInt(DIFFICULTY_KEY, levelOfDifficulty);
        }

        private void OnPackSelected(string pack)
        {
            packName = pack;
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawViewer();
            HandleEvent();
        }

        private void HandleEvent()
        {
            if (Event.current != null && Event.current.type == EventType.MouseMove)
            {
                Repaint();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Database path:             " + db);
            if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                EditorCommon.BrowseDatabase(ref db);
            }
            EditorGUILayout.EndHorizontal();

            key = (CryptoKey)EditorGUILayout.ObjectField("Crypto key", key, typeof(CryptoKey), false);
            useKeyForDecryption = EditorGUILayout.Toggle("Use key for decryption", useKeyForDecryption);

            EditorCommon.DrawSeparator();
        }

        private void DrawViewer()
        {
            float spacing = 10;
            EditorGUILayout.BeginHorizontal();
            Rect gridRect = new Rect(spacing, 80, 500, 500);
            DrawGrid(gridRect);
            Rect browserRect = new Rect(gridRect.max.x + spacing, gridRect.min.y, 300, 500);
            DrawBrowser(browserRect);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGrid(Rect r)
        {
            GUILayout.BeginArea(r);
            if (puzzleGrid != null && solutionGrid != null)
            {
                int size = gridSize;
                int cellSize = (int)(r.size.x / size);
                for (int i = size - 1; i >= 0; --i)
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.Height(cellSize));
                    for (int j = 0; j < size; ++j)
                    {
                        GUIStyle style = (i + j) % 2 == 0 ? evenCellStyle : oddCellStyle;
                        style.fontSize = (int)(cellSize / 2);
                        string cellText;
                        if (selectedPuzzleIndex != -1)
                        {
                            style.normal.textColor = puzzleGrid[i][j] == Puzzle.DOT ? new Color(0.4f, 0.4f, 0.4f, 1) : Color.black;
                            if (puzzleGrid[i][j] != Puzzle.DOT)
                            {
                                cellText = puzzleGrid[i][j];
                            }
                            else
                            {
                                cellText = showSolution ? solutionGrid[i][j] : "";
                            }
                        }
                        else
                        {
                            cellText = "";
                        }
                        GUILayout.Label(cellText, style, GUILayout.Height(cellSize), GUILayout.Width(cellSize));
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            GUILayout.EndArea();
        }

        private void DrawBrowser(Rect r)
        {
            GUI.enabled = Data.ExistsDatabase(db);
            GUILayout.BeginArea(r);
            gridSize = EditorGUILayout.IntPopup("Grid size", gridSize, gridSizeEnumText, gridSizeEnumValue);
            levelOfDifficulty = EditorGUILayout.MaskField("Difficulty", levelOfDifficulty, difficultyEnumText);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Pack", GUILayout.Width(146));
            Rect btnRect = EditorGUILayout.GetControlRect(false);
            if (GUI.Button(btnRect, packName.Equals(string.Empty) ? "All pack" : packName, EditorStyles.miniButton))
            {
                PackSelector selector = new PackSelector(true);
                selector.databasePath = db;
                PopupWindow.Show(btnRect, selector);
            }
            EditorGUILayout.EndHorizontal();
            sortByLevel = EditorGUILayout.Toggle("Sort by Level", sortByLevel);
            showSolution = EditorGUILayout.Toggle("Show solution", showSolution);

            EditorGUILayout.Space();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, scrollViewStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (db!=lastDb|| gridSize != lastGridSize || levelOfDifficulty != lastDifficulty || lastPack != packName || sortByLevel != lastSortByLevel)
            {
                Refresh();
            }
            if (loadedPuzzles == null || loadedPuzzles.Count == 0)
            {
                EditorGUILayout.HelpBox("No puzzles found.", MessageType.Info);
                if (GUILayout.Button("Refresh", EditorStyles.miniButton))
                {
                    Refresh();
                }
                selectedPuzzleIndex = -1;
            }
            else
            {
                for (int i = 0; i < loadedPuzzles.Count; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    string itemLabel = loadedPuzzlesId[i].ToString() + " - " + loadedPuzzles[i].level;
                    GUIStyle style = i == selectedPuzzleIndex ? EditorCommon.SelectedItemStyle : i % 2 == 0 ? EditorCommon.EvenItemStyle : EditorCommon.OddItemStyle;
                    if (GUILayout.Button(itemLabel, style, GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                        try
                        {
                            selectedPuzzleIndex = i;
                            string puzzleStr =
                                key != null && useKeyForDecryption ?
                                Crypto.Decrypt(loadedPuzzles[i].puzzle) :
                                loadedPuzzles[i].puzzle;
                            string solutionStr =
                                key != null && useKeyForDecryption ?
                                Crypto.Decrypt(loadedPuzzles[i].solution) :
                                loadedPuzzles[i].solution;
                            if (!string.IsNullOrEmpty(puzzleStr) &&
                                !string.IsNullOrEmpty(solutionStr) &&
                                puzzleStr.IsPuzzleStringOfSize((Size)gridSize) &&
                                solutionStr.IsSolutionStringOfSize((Size)gridSize))
                            {
                                puzzleGrid = Helper.PuzzleStringToStringGrid(puzzleStr);
                                solutionGrid = Helper.PuzzleStringToStringGrid(solutionStr);
                            }
                            else
                            {
                                puzzleGrid = null;
                                solutionGrid = null;
                                EditorUtility.DisplayDialog("Error", "Cannot read puzzle, please check the Crypto key.", "OK");
                            }
                        }
                        catch
                        {
                            puzzleGrid = null;
                            solutionGrid = null;
                            EditorUtility.DisplayDialog("Error", "Cannot read puzzle, please check the Crypto key.", "OK");
                        }
                    }
                    if (selectedPuzzleIndex == i)
                    {
                        if (GUILayout.Button("X", removeButtonStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(EditorGUIUtility.singleLineHeight)))
                        {
                            if (EditorUtility.DisplayDialog("Remove puzzle", "Remove this puzzle from database?", "Yes", "No"))
                            {
                                Viewer.DeletePuzzle(db, loadedPuzzlesId[selectedPuzzleIndex]);
                                loadedPuzzlesId.RemoveAt(selectedPuzzleIndex);
                                loadedPuzzles.RemoveAt(selectedPuzzleIndex);
                                selectedPuzzleIndex = -1;
                                Data.UpdateInfoTable(db);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.enabled = true;
        }

        private Level[] CreateAcceptedLevel()
        {
            List<Level> acceptedLevel = new List<Level>();
            for (int i = 0; i < difficultyEnumText.Length; ++i)
            {
                if (((levelOfDifficulty >> i) & 1) == 1)
                    acceptedLevel.Add((Level)i);
            }

            return acceptedLevel.ToArray();
        }

        private void Refresh()
        {
            loadedPuzzles = new List<Puzzle>();
            loadedPuzzlesId = new List<int>();
            Viewer.GetAllPuzzle(db, loadedPuzzlesId, loadedPuzzles, (Size)gridSize, CreateAcceptedLevel(), packName, sortByLevel);
            lastDb = db;
            lastGridSize = gridSize;
            lastDifficulty = levelOfDifficulty;
            lastPack = packName;
            lastSortByLevel = sortByLevel;
            selectedPuzzleIndex = -1;
        }
    }
}