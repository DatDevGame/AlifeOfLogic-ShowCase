using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Takuzu.Generator;
using System.Threading;
using System.IO;
using System.Diagnostics;

public class TakuzuMakerEditor : EditorWindow
{

    public const int MAKER_TAB = 0;
    public const int GENERATING_TAB = 1;
    public const int GRADER_TAB = 3;

    private int selectedTab;
    private int gridSize;
    private string[] gridSizeEnumText;
    private int[] gridSizeEnumValue;
    private string[] difficultyEnumText;
    private int levelOfDifficulty;
    private int numberOfPuzzle;
    private int puzzlePerGrid;  // may not be met if grid is discarded by "maxAllowedFailuresPerGrid"
    private bool excludePrevious;
    private int defaultAttemptsForFormingGrid = 1;
    private int defaultAttemptsForGeneratingPuzzle = 1;
    private int defaultMaxCompensateTrialsForGeneratingPuzzle = 50;
    private int defaultMaxAllowedFailuresPerGrid = 5;
    private int maxAttemptsForFormingGrid = 1;
    private int maxTrialsForFormingGrid = 200;
    private int maxAttemptsForGeneratingPuzzle = 1;
    private int maxTrialsForGeneratingPuzzle = 200;
    private int maxCompensateTrialsForGeneratingPuzzle;
    private int maxAllowedFailuresPerGrid;   // stop generating puzzle from a grid after this number of failures
    private int givenCellCount;
    private float givenMin;
    private float givenMax;
    private Dictionary<int, int> maxTrialsForGeneratingPuzzlePrefs;
    private Dictionary<int, int> maxTrialsForFormingGridPrefs;
    private Dictionary<int, Vector2> givenRangePrefs;
    private Dictionary<Level, Dictionary<int, Vector2>> recommendedGivenRanges; // for each level and size
    private GenerationInfo generationInfo;
    private bool saveInstantly;
    private bool useRecommendParams;
    private List<string> logs;
    private bool showLog;
    private bool showAnalyzeLog;
    private Vector2 logScrollPos;
    private GUIStyle logStyle;
    private int maxLoggingLine;
    private Stopwatch timer;
    private Thread generateThread;
    private string databasePath;
    private List<Puzzle> generatedPuzzles;
    private Vector2 graderScrollPos;
    private List<bool> graderToggleLevel1;
    private List<bool> graderToggleLevel2;
    private List<string> graderKey;
    private GradingProfile lastGradingProfile;

    private string sizeKey = "MAKER_SIZE";
    private string givenKey = "MAKER_GIVEN";
    private string levelKey = "MAKER_LEVEL";
    private string quantityKey = "MAKER_QUANTITY";
    private string excludeKey = "MAKER_EXCLUDE";
    private string databaseKey = "MAKER_DATABASE";
    private string useRecommendParamKey = "MAKER_RECOMMEND_PARAM";
    private string gradingProfilePathKey = "MAKER_GRADING_PROFILE";

    [MenuItem("Tools/Takuzu Maker", priority = 0)]
    public static void ShowWindow()
    {
        TakuzuMakerEditor takuzuMaker = GetWindow<TakuzuMakerEditor>("Takuzu Maker");
        takuzuMaker.Show();
    }

    private void OnLog(string log)
    {
        logs.Add(log);
        if (logs.Count > maxLoggingLine)
            logs.RemoveAt(0);
    }

    private void OnEnable()
    {
        Generator.onNewPuzzleCreated += OnNewPuzzleCreated;
        wantsMouseMove = true;
        logs = new List<string>();
        maxLoggingLine = 150;
        showLog = true;
        logStyle = new GUIStyle();
        logStyle.richText = true;
        logStyle.padding = new RectOffset(15, 15, 15, 15);
        logScrollPos = Vector2.zero;
        Helper.onLog += OnLog;

        timer = new Stopwatch();
        generationInfo = new GenerationInfo(numberOfPuzzle);

        selectedTab = MAKER_TAB;
        gridSizeEnumText = new string[7] { "6x6", "8x8", "10x10", "12x12", "14x14", "16x16", "18x18" };
        gridSizeEnumValue = new int[7] { 6, 8, 10, 12, 14, 16, 18 };
        gridSize = gridSizeEnumValue[0];
        givenCellCount = 1;
        numberOfPuzzle = 1;

        System.Array enumValue = System.Enum.GetValues(typeof(Level));
        difficultyEnumText = new string[enumValue.Length];
        for (int i = 0; i < enumValue.Length; ++i)
        {
            difficultyEnumText[i] = enumValue.GetValue(i).ToString();
        }

        // Initialize params.
        maxAttemptsForFormingGrid = defaultAttemptsForFormingGrid;
        maxAttemptsForGeneratingPuzzle = defaultAttemptsForGeneratingPuzzle;
        maxCompensateTrialsForGeneratingPuzzle = defaultMaxCompensateTrialsForGeneratingPuzzle;
        maxAllowedFailuresPerGrid = defaultMaxAllowedFailuresPerGrid;

        maxTrialsForGeneratingPuzzlePrefs = new Dictionary<int, int>();
        maxTrialsForGeneratingPuzzlePrefs.Add(6, 150);
        maxTrialsForGeneratingPuzzlePrefs.Add(8, 200);
        maxTrialsForGeneratingPuzzlePrefs.Add(10, 220);
        maxTrialsForGeneratingPuzzlePrefs.Add(12, 250);
        maxTrialsForGeneratingPuzzlePrefs.Add(14, 300);
        maxTrialsForGeneratingPuzzlePrefs.Add(16, 350);
        maxTrialsForGeneratingPuzzlePrefs.Add(18, 400);

        maxTrialsForFormingGridPrefs = new Dictionary<int, int>();
        maxTrialsForFormingGridPrefs.Add(6, 300);
        maxTrialsForFormingGridPrefs.Add(8, 500);
        maxTrialsForFormingGridPrefs.Add(10, 800);
        maxTrialsForFormingGridPrefs.Add(12, 1000);
        maxTrialsForFormingGridPrefs.Add(14, 1200);
        maxTrialsForFormingGridPrefs.Add(16, 1500);
        maxTrialsForFormingGridPrefs.Add(18, 1700);

        givenRangePrefs = new Dictionary<int, Vector2>();
        givenRangePrefs.Add(6, new Vector2(6, 24));
        givenRangePrefs.Add(8, new Vector2(14, 39));
        givenRangePrefs.Add(10, new Vector2(22, 58));
        givenRangePrefs.Add(12, new Vector2(33, 81));
        givenRangePrefs.Add(14, new Vector2(50, 116));
        givenRangePrefs.Add(16, new Vector2(70, 153));
        givenRangePrefs.Add(18, new Vector2(88, 194));

        recommendedGivenRanges = new Dictionary<Level, Dictionary<int, Vector2>>();

        // Easy level.
        recommendedGivenRanges.Add(Level.Easy, new Dictionary<int, Vector2>()
        {
            {6, new Vector2(10,20)},    // widest possible: 10-23   - it can take very long to generate at max given (i.e. 23 in this case) so we normally narrow the range down
            {8, new Vector2(19,35)},    // widest possible: 19-39
            {10, new Vector2(30,50)},   // widest possible: 30-56
            {12, new Vector2(43,70)},   // widest possible: 43-79
            {14, new Vector2(50,116)},   // widest possible: unknown yet
            {16, new Vector2(70,153)},   // widest possible: unknown yet
            {18, new Vector2(88,194)},   // widest possible: unknown yet
        });

        // Medium level.
        recommendedGivenRanges.Add(Level.Medium, new Dictionary<int, Vector2>()
        {
            {6, new Vector2(9,18)},    // widest possible: 9-20
            {8, new Vector2(27,25)},    // widest possible: 16-29
            {10, new Vector2(25,45)},   // widest possible: 25-49
            {12, new Vector2(36,65)},   // widest possible: 36-67
            {14, new Vector2(50,116)},   // widest possible: unknown yet
            {16, new Vector2(70,153)},   // widest possible: unknown yet
            {18, new Vector2(88,194)},   // widest possible: unknown yet
        });

        // Hard level.
        recommendedGivenRanges.Add(Level.Hard, new Dictionary<int, Vector2>()
        {
            {6, new Vector2(9,16)},    // widest possible: 9-18
            {8, new Vector2(15,28)},    // widest possible: 15-30
            {10, new Vector2(23,40)},   // widest possible: 23-45
            {12, new Vector2(35,60)},   // widest possible: 35-65
            {14, new Vector2(50,116)},   // widest possible: unknown yet
            {16, new Vector2(70,153)},   // widest possible: unknown yet
            {18, new Vector2(88,194)},   // widest possible: unknown yet
        });

        // Evil level.
        recommendedGivenRanges.Add(Level.Evil, new Dictionary<int, Vector2>()
        {
            {6, new Vector2(8,13)},    // widest possible: 8-14
            {8, new Vector2(14,22)},    // widest possible: 14-24
            {10, new Vector2(22,34)},   // widest possible: 22-38
            {12, new Vector2(34,50)},   // widest possible: 34-54
            {14, new Vector2(50,116)},   // widest possible: unknown yet
            {16, new Vector2(70,153)},   // widest possible: unknown yet
            {18, new Vector2(88,194)},   // widest possible: unknown yet
        });

        // Insane level.
        recommendedGivenRanges.Add(Level.Insane, new Dictionary<int, Vector2>()
        {
            {6, new Vector2(8,11)},    // widest possible: 8-12
            {8, new Vector2(14,17)},    // widest possible: 14-18
            {10, new Vector2(22,32)},   // widest possible: 22-34
            {12, new Vector2(33,42)},   // widest possible: 33-45
            {14, new Vector2(50,116)},   // widest possible: unknown yet
            {16, new Vector2(70,153)},   // widest possible: unknown yet
            {18, new Vector2(88,194)},   // widest possible: unknown yet
        });

        gridSize = EditorPrefs.GetInt(sizeKey, gridSizeEnumValue[0]);
        levelOfDifficulty = EditorPrefs.GetInt(levelKey, 0);
        givenCellCount = EditorPrefs.GetInt(givenKey, 0);
        numberOfPuzzle = EditorPrefs.GetInt(quantityKey, 1);
        excludePrevious = EditorPrefs.GetBool(excludeKey, true);
        databasePath = EditorPrefs.GetString(databaseKey, "");
        useRecommendParams = EditorPrefs.GetBool(useRecommendParamKey, true);
        GradingProfile.active = AssetDatabase.LoadAssetAtPath<GradingProfile>(EditorPrefs.GetString(gradingProfilePathKey, ""));
        if (!File.Exists(databasePath))
            databasePath = "";
        givenMin = givenRangePrefs[gridSize].x;
        givenMax = givenRangePrefs[gridSize].y;

        graderToggleLevel1 = new List<bool>();
        graderToggleLevel2 = new List<bool>();
    }

    private void OnDestroy()
    {
        Generator.onNewPuzzleCreated -= OnNewPuzzleCreated;
        SaveEditorParam();
    }

    private void SaveEditorParam()
    {
        EditorPrefs.SetInt(sizeKey, gridSize);
        EditorPrefs.SetInt(levelKey, (int)levelOfDifficulty);
        EditorPrefs.SetInt(givenKey, givenCellCount);
        EditorPrefs.SetInt(quantityKey, numberOfPuzzle);
        EditorPrefs.SetBool(excludeKey, excludePrevious);
        EditorPrefs.SetString(databaseKey, databasePath);
        EditorPrefs.SetBool(useRecommendParamKey, useRecommendParams);
        if (GradingProfile.active != null)
            EditorPrefs.SetString(gradingProfilePathKey, AssetDatabase.GetAssetPath(GradingProfile.active));
    }

    private void OnGUI()
    {
        if (selectedTab != GENERATING_TAB)
        {
            DrawHeader();
            DrawSelectedTab();
        }
        else
        {
            DrawGeneratingGUI();
        }
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Database path:               " + databasePath);
        if (GUILayout.Button("New", EditorStyles.miniButton, GUILayout.Width(100)))
        {
            EditorCommon.CreateNewDatabase(ref databasePath);
        }
        if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(100)))
        {
            EditorCommon.BrowseDatabase(ref databasePath);
        }
        EditorGUILayout.EndHorizontal();

        GradingProfile.active = EditorGUILayout.ObjectField("Grading profile", GradingProfile.active, typeof(GradingProfile), false) as GradingProfile;

        EditorCommon.DrawSeparator();

        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = selectedTab == MAKER_TAB ? Color.gray : Color.white;
        if (GUILayout.Button("Maker", EditorStyles.miniButtonLeft))
        {
            selectedTab = MAKER_TAB;
        }
        GUI.backgroundColor = selectedTab == GRADER_TAB ? Color.gray : Color.white;
        if (GUILayout.Button("Grader", EditorStyles.miniButtonRight))
        {
            selectedTab = GRADER_TAB;
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    private void DrawSelectedTab()
    {
        if (selectedTab == MAKER_TAB)
        {
            DrawMakerTab();
        }
        else if (selectedTab == GRADER_TAB)
        {
            DrawGraderTab();
        }
    }

    private void DrawMakerTab()
    {
        gridSize = EditorGUILayout.IntPopup("Grid size", gridSize, gridSizeEnumText, gridSizeEnumValue);
        levelOfDifficulty = EditorGUILayout.MaskField("Difficulty", levelOfDifficulty, difficultyEnumText);
        numberOfPuzzle = Mathf.Max(1, EditorGUILayout.IntField("Number of puzzles", numberOfPuzzle));
        puzzlePerGrid = Mathf.Clamp(EditorGUILayout.IntField("Puzzle per grid", puzzlePerGrid), 1, numberOfPuzzle);
        EditorGUILayout.HelpBox("Puzzles per grid may not be met if grid is discarded by \"maxAllowedFailuresPerGrid\"", MessageType.Info);

        excludePrevious = EditorGUILayout.Toggle("Exclude from previous", excludePrevious);
        saveInstantly = EditorGUILayout.Toggle("Save instantly", saveInstantly);
        useRecommendParams = EditorGUILayout.Toggle("Use recommend params", useRecommendParams);

        EditorGUI.BeginDisabledGroup(useRecommendParams);

        EditorGUI.indentLevel += 1;
        EditorGUILayout.LabelField("Form grids:");
        EditorGUI.indentLevel += 1;
        maxAttemptsForFormingGrid = EditorGUILayout.IntField("Max attempts", maxAttemptsForFormingGrid);
        maxTrialsForFormingGrid = EditorGUILayout.IntField("Max trials", maxTrialsForFormingGrid);
        EditorGUI.indentLevel -= 1;

        EditorGUILayout.LabelField("Generate puzzles:");
        EditorGUI.indentLevel += 1;
        givenMin = Mathf.Clamp(givenMin, givenRangePrefs[gridSize].x, givenRangePrefs[gridSize].y);
        givenMax = Mathf.Clamp(givenMax, givenRangePrefs[gridSize].x, givenRangePrefs[gridSize].y);
        EditorGUILayout.MinMaxSlider("Given range " + "(" + (int)givenMin + " - " + (int)givenMax + ")", ref givenMin, ref givenMax, (int)givenRangePrefs[gridSize].x, (int)givenRangePrefs[gridSize].y);
        givenCellCount = Mathf.Clamp(givenCellCount, (int)givenMin, (int)givenMax);
        maxAttemptsForGeneratingPuzzle = EditorGUILayout.IntField("Max attempts", maxAttemptsForGeneratingPuzzle);
        maxTrialsForGeneratingPuzzle = EditorGUILayout.IntField("Max trials", maxTrialsForGeneratingPuzzle);
        maxCompensateTrialsForGeneratingPuzzle = EditorGUILayout.IntField("Max Compensate Trials", maxCompensateTrialsForGeneratingPuzzle);
        maxAllowedFailuresPerGrid = EditorGUILayout.IntField("Max Allowed Failure Per Grid", maxAllowedFailuresPerGrid);
        EditorGUI.indentLevel -= 1;
        EditorGUI.indentLevel -= 1;

        EditorGUI.EndDisabledGroup();

        if (useRecommendParams)
        {
            maxAttemptsForFormingGrid = defaultAttemptsForFormingGrid;
            maxAttemptsForGeneratingPuzzle = defaultAttemptsForGeneratingPuzzle;
            maxTrialsForFormingGrid = maxTrialsForFormingGridPrefs[gridSize];
            maxTrialsForGeneratingPuzzle = maxTrialsForGeneratingPuzzlePrefs[gridSize];
            maxCompensateTrialsForGeneratingPuzzle = defaultMaxCompensateTrialsForGeneratingPuzzle;
            maxAllowedFailuresPerGrid = defaultMaxAllowedFailuresPerGrid;
        }

        bool canGenerate = levelOfDifficulty != 0 &&
            !string.IsNullOrEmpty(databasePath);
        if (!useRecommendParams)
        {
            if (maxAttemptsForFormingGrid == 0 || maxTrialsForFormingGrid == 0 || maxAttemptsForGeneratingPuzzle == 0 || maxTrialsForGeneratingPuzzle == 0 || maxCompensateTrialsForGeneratingPuzzle == 0 || maxAllowedFailuresPerGrid == 0)
                canGenerate = false;
        }

        GUI.enabled = canGenerate;
        if (GUILayout.Button("Generate"))
        {
            StartGenerate();
        }
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

    private void DrawGraderTab()
    {
        if (GradingProfile.active == null)
        {
            EditorGUILayout.HelpBox("No Grading Profile supplied.", MessageType.Info);
            return;
        }

        Rect r = EditorGUILayout.GetControlRect();
        Rect btnRect = new Rect();
        btnRect.size = new Vector2(100, r.size.y);
        btnRect.position = r.max - btnRect.size;
        if (graderToggleLevel1.Contains(true))
        {
            if (GUI.Button(btnRect, "Collapse all"))
            {
                for (int i = 0; i < graderToggleLevel1.Count; ++i)
                    graderToggleLevel1[i] = false;
                for (int i = 0; i < graderToggleLevel2.Count; ++i)
                    graderToggleLevel2[i] = false;
            }
        }
        else
        {
            if (GUI.Button(btnRect, "Expand all"))
            {
                for (int i = 0; i < graderToggleLevel1.Count; ++i)
                    graderToggleLevel1[i] = true;
                for (int i = 0; i < graderToggleLevel2.Count; ++i)
                    graderToggleLevel2[i] = true;
            }
        }

        graderScrollPos = EditorGUILayout.BeginScrollView(graderScrollPos, EditorStyles.inspectorDefaultMargins);

        if (lastGradingProfile != GradingProfile.active)
        {
            graderKey = new List<string>();
            foreach (var key in GradingProfile.active.map.Keys)
            {
                graderKey.Add((string)key);
            }
            graderKey.Sort((string a, string b) =>
            {
                string[] splitA = a.Split('_');
                string[] splitB = b.Split('_');
                if (int.Parse(splitA[0]) < int.Parse(splitB[0]))
                    return -1;
                else if (int.Parse(splitA[0]) > int.Parse(splitB[0]))
                {
                    return 1;
                }
                else
                {
                    if (int.Parse(splitA[1]) < int.Parse(splitB[1]))
                        return -1;
                    else if (int.Parse(splitA[1]) > int.Parse(splitB[1]))
                    {
                        return 1;
                    }
                }
                return 0;
            });

            string tmp = string.Empty;
            graderToggleLevel1 = new List<bool>();
            graderToggleLevel2 = new List<bool>();
            for (int i = 0; i < graderKey.Count; ++i)
            {
                if (!graderKey[i].Split('_')[0].Equals(tmp))
                    graderToggleLevel1.Add(false);
                graderToggleLevel2.Add(false);
            }

        }
        lastGradingProfile = GradingProfile.active;

        int lastGroup = -1;
        for (int i = 0; i < graderKey.Count; ++i)
        {
            int group = int.Parse(graderKey[i].Split('_')[0]);
            if (!group.Equals(lastGroup))
            {
                EditorCommon.DrawSeparator();
                GUIStyle style = EditorStyles.foldout;
                FontStyle previousStyle = style.fontStyle;
                style.fontStyle = FontStyle.Bold;
                graderToggleLevel1[group] = EditorGUILayout.Foldout(graderToggleLevel1[group], ((Size)group).ToString().ToUpper(), true, style);
                style.fontStyle = previousStyle;
                lastGroup = group;

            }
            if (graderToggleLevel1[group])
            {
                EditorGUI.indentLevel += 1;
                LevelDef ld = (LevelDef)GradingProfile.active.map[graderKey[i]];
                GUIStyle style = EditorStyles.foldout;
                FontStyle previousStyle = style.fontStyle;
                style.fontStyle = FontStyle.Bold;
                graderToggleLevel2[i] = EditorGUILayout.Foldout(graderToggleLevel2[i], LevelDef.GetLevelName(ld.size, ld.level), true, style);
                style.fontStyle = previousStyle;
                if (graderToggleLevel2[i])
                {
                    string k = graderKey[i];
                    string label;
                    EditorGUI.indentLevel += 1;

                    EditorGUILayout.BeginHorizontal();
                    label = string.Format("Parse percent ({0:0.00}-{1:0.00})", GradingProfile.active.map[k].minParsePercent, GradingProfile.active.map[k].maxParsePercent);
                    EditorGUILayout.LabelField(label, GUILayout.Width(220));
                    EditorGUILayout.MinMaxSlider(ref GradingProfile.active.map[k].minParsePercent, ref GradingProfile.active.map[k].maxParsePercent, 0, 100);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    label = string.Format("LSD percent ({0:0.00}-{1:0.00})", GradingProfile.active.map[k].minLsdPercent, GradingProfile.active.map[k].maxLsdPercent);
                    EditorGUILayout.LabelField(label, GUILayout.Width(220));
                    EditorGUILayout.MinMaxSlider(ref GradingProfile.active.map[k].minLsdPercent, ref GradingProfile.active.map[k].maxLsdPercent, 0, 100);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    label = string.Format("ALSD percent ({0:0.00}-{1:0.00})", GradingProfile.active.map[k].minAlsdPercent, GradingProfile.active.map[k].maxAlsdPercent);
                    EditorGUILayout.LabelField(label, GUILayout.Width(220));
                    EditorGUILayout.MinMaxSlider(ref GradingProfile.active.map[k].minAlsdPercent, ref GradingProfile.active.map[k].maxAlsdPercent, 0, 100);
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel -= 1;
                }
                EditorGUI.indentLevel -= 1;
            }
        }
        EditorCommon.DrawSeparator();
        EditorGUILayout.EndScrollView();

        EditorUtility.SetDirty(GradingProfile.active);
    }

    private void DrawGeneratingGUI()
    {
        try
        {
            Rect progressBarRect = EditorGUILayout.GetControlRect();
            EditorGUI.ProgressBar(progressBarRect, (float)generationInfo.generatedPuzzles / generationInfo.totalPuzzles, "Generating... " + generationInfo.generatedPuzzles + "/" + generationInfo.totalPuzzles);
            EditorGUILayout.LabelField("PLEASE CLOSE ALL CONNECTIONS TO THE DATABASE, ESPECIALLY IN THE DATABASE BROWSER SOFTWARES.", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("FAILURE TO DO SO, AN ERROR WILL OCCUR!", EditorStyles.boldLabel);

            if (timer != null)
            {
                EditorGUILayout.LabelField("Estimated time: " + timer.Elapsed.ToString());
            }

            showLog = EditorGUILayout.ToggleLeft("Show output", showLog);
            showAnalyzeLog = EditorGUILayout.ToggleLeft("Show analyze log", showAnalyzeLog);
            if (showLog)
            {
                Generator.AnalyzeMode = showAnalyzeLog;
                Filler.analyzeMode = false;
                string log = string.Empty;

                for (int i = 0; i < logs.Count; ++i)
                {
                    log += logs[i] + "\n";
                }

                logScrollPos = EditorGUILayout.BeginScrollView(logScrollPos, false, true);
                EditorGUILayout.TextArea(log, logStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight * logs.Count));
                EditorGUILayout.EndScrollView();
            }
            else
            {
                Generator.AnalyzeMode = false;
                Filler.analyzeMode = false;
            }

            GUI.enabled = !generationInfo.shouldStop;
            if (timer.IsRunning)
            {
                if (GUILayout.Button("Abort"))
                {
                    AbortGenerate();
                }
            }
            else
            {
                if (GUILayout.Button("Finish"))
                {
                    selectedTab = MAKER_TAB;
                    logs.Clear();
                }
            }
            GUI.enabled = true;
            if (generateThread != null && generateThread.IsAlive)
            {
                EditorGUILayout.LabelField("Thread is alive", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Thread is died", EditorStyles.boldLabel);
            }
            Repaint();
        }
        catch (System.ArgumentException e)
        {
            UnityEngine.Debug.LogError(e.ToString());
        }
    }

    private void StartGenerate()
    {
        selectedTab = GENERATING_TAB;
        generateThread = new Thread(GenerateThread);
        generateThread.Priority = System.Threading.ThreadPriority.Highest;
        generateThread.Start();

    }

    private void AbortGenerate()
    {
        generationInfo.shouldStop = true;
        timer.Stop();
    }

    private void GenerateThread()
    {
        logs = new List<string>();
        generatedPuzzles = new List<Puzzle>();
        try
        {
            timer.Reset();
            timer.Start();
            generationInfo = new GenerationInfo(numberOfPuzzle);
            generationInfo.isStopped = false;
            generationInfo.shouldStop = false;

            List<string> grids;

            HashSet<string> excludedSolutionSet = new HashSet<string>();
            HashSet<string> excludedPuzzleSet = new HashSet<string>();
            if (excludePrevious)
            {
                Data.GetAllPuzzleString(databasePath, excludedPuzzleSet);
                Data.GetAllSolutionString(databasePath, excludedSolutionSet);
                Helper.LogInfoFormat("Excluded puzzle count: " + excludedPuzzleSet.Count);
                Helper.LogInfoFormat("Excluded solution count: " + excludedSolutionSet.Count);
            }

            // All accepted levels.
            var acceptedLevels = CreateAcceptedLevel();

            do
            {
                grids = Filler.FormGrids(gridSize, 0, 1, 1, maxAttemptsForFormingGrid, maxTrialsForFormingGrid, excludedSolutionSet, ref generationInfo);
                if (grids.Count == 0)
                    continue;
                int puzzleForThisGrid = 0;
                int failuresOfThisGrid = 0;
                do
                {
                    System.Random rdm = new System.Random(System.DateTime.Now.Millisecond);

                    // If using recommended params, we'll only accept one specific level at each attempt
                    // (which is randomized from the list of accepted levels),
                    // so we can apply the recommend given range corresponding to the target size & level.
                    if (useRecommendParams)
                    {
                        var levelForNextPuzzle = acceptedLevels[rdm.Next(0, acceptedLevels.Length)];
                        acceptedLevels = new Level[] { levelForNextPuzzle };
                        givenMin = (int)recommendedGivenRanges[levelForNextPuzzle][gridSize].x;
                        givenMax = (int)recommendedGivenRanges[levelForNextPuzzle][gridSize].y;
                    }

                    givenCellCount = rdm.Next((int)givenMin, (int)givenMax + 1);
                    Helper.LogInfoFormat("Attempting to create puzzle with {0} givens (range [{1}-{2}])", givenCellCount, (int)givenMin, (int)givenMax);

                    List<Puzzle> puzzles = Generator.GeneratePuzzles(
                        Helper.PuzzleStringToStringGrid(grids[0]),
                        givenCellCount,
                        acceptedLevels,
                        1,
                        excludedPuzzleSet,
                        maxAttemptsForGeneratingPuzzle,
                        maxTrialsForGeneratingPuzzle,
                        maxCompensateTrialsForGeneratingPuzzle,
                        false,
                        ref generationInfo);
                    failuresOfThisGrid += puzzles.Count > 0 ? 0 : 1;
                    puzzleForThisGrid += puzzles.Count;
                    generatedPuzzles.AddRange(puzzles);
                    generationInfo.generatedPuzzles = generatedPuzzles.Count;
                } while (puzzleForThisGrid < puzzlePerGrid && generationInfo.generatedPuzzles < numberOfPuzzle && failuresOfThisGrid < maxAllowedFailuresPerGrid && !generationInfo.shouldStop);
            } while (generationInfo.generatedPuzzles < numberOfPuzzle && !generationInfo.shouldStop);

            if (!generationInfo.shouldStop)
            {
                if (!saveInstantly)
                {
                    Maker.SavePuzzles(databasePath, generatedPuzzles);
                }
                else
                {
                    string msg = string.Format("{0} puzzle(s) have been saved to database during the process.", generatedPuzzles.Count);
                    Helper.LogSuccessFormat(msg);
                }
            }

            Data.UpdateInfoTable(databasePath);
        }
        catch (System.Exception e)
        {
            Helper.LogErrorFormat(e.ToString());
        }

        Helper.LogInfoFormat(generationInfo.shouldStop ? "Aborted." : "Done!");

        generationInfo.shouldStop = false;
        generationInfo.isStopped = true;

        timer.Stop();

    }

    private void OnNewPuzzleCreated(Puzzle newPuzzle)
    {
        Helper.PrintGrid(Helper.PuzzleStringToStringGrid(newPuzzle.puzzle));
        if (saveInstantly && !generationInfo.isStopped)
        {
            Maker.SavePuzzle(databasePath, newPuzzle);
        }
    }
}
