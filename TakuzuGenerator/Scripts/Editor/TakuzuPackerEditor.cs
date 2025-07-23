using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Takuzu.Generator
{
    public class TakuzuPackerEditor : EditorWindow
    {
        private string srcDbPath;
        private string desDbPath;
        private string packName;
        private List<Size> packContentSize;
        private List<Level> packContentLevel;
        private List<int> packContentCount;
        private List<string> exclusiveDb;
        private bool exclusiveCheck;
        private bool stripStatisticInformation;
        private HashSet<string> excludePuzzle;

        private Vector2 scrollPos;
        private bool canMergePackContent;
        private string status;

        private const string SRC_KEY = "PACKER_SRC_DB";
        private const string DES_KEY = "PACKER_DES_DB";
        private const string PACK_KEY = "PACKER_PACK";
        private const string EXCLUSIVE_KEY = "PACKER_EXCLUSIVE";
        private const string STRIP_KEY = "PACKER_STRIP";

        [MenuItem("Tools/Takuzu Packer", priority = 2)]
        public static void ShowWindow()
        {
            TakuzuPackerEditor window = GetWindow<TakuzuPackerEditor>("Takuzu Packer");
            window.minSize = new Vector2(700, 300);
            window.position = new Rect(Vector2.one * 200, window.minSize);
            window.Show();
        }

        private void OnEnable()
        {
            PackSelector.OnPackSelected += OnPackSelected;

            srcDbPath = string.Empty;
            desDbPath = string.Empty;
            LoadParams();
        }

        private void OnDestroy()
        {
            PackSelector.OnPackSelected -= OnPackSelected;
            SaveParams();
        }


        private void SaveParams()
        {
            EditorPrefs.SetString(SRC_KEY, srcDbPath);
            EditorPrefs.SetString(DES_KEY, desDbPath);
            EditorPrefs.SetString(PACK_KEY, packName);
            EditorPrefs.SetBool(EXCLUSIVE_KEY, exclusiveCheck);
            EditorPrefs.SetBool(STRIP_KEY, stripStatisticInformation);
        }

        private void LoadParams()
        {
            srcDbPath = EditorPrefs.GetString(SRC_KEY);
            desDbPath = EditorPrefs.GetString(DES_KEY);
            packName = EditorPrefs.GetString(PACK_KEY);
            exclusiveCheck = EditorPrefs.GetBool(EXCLUSIVE_KEY);
            stripStatisticInformation = EditorPrefs.GetBool(STRIP_KEY);
        }

        private void OnPackSelected(string pack)
        {
            packName = pack;
            Repaint();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            DrawDbSelector();
            DrawPackSelector();
            DrawPackContentSelector();
            DrawExclusiveDatabaseSelector();
            DrawOtherSettings();
            EditorGUILayout.EndScrollView();
            DrawFooter();
        }

        private void DrawDbSelector()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("DATABASE", EditorCommon.BoldLabel, GUILayout.Width(70));
            EditorCommon.DrawSeparator();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel += 1;
            EditorGUILayout.LabelField("Source database:             " + srcDbPath);
            EditorGUI.indentLevel -= 1;
            if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                EditorCommon.BrowseDatabase(ref srcDbPath);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel += 1;
            EditorGUILayout.LabelField("Destination database:       " + desDbPath);
            EditorGUI.indentLevel -= 1;
            if (GUILayout.Button("New", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                EditorCommon.CreateNewDatabase(ref desDbPath);
            }
            if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                EditorCommon.BrowseDatabase(ref desDbPath);
            }
            if (desDbPath.Equals(srcDbPath))
            {
                desDbPath = string.Empty;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPackSelector()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("PACK", EditorCommon.BoldLabel, GUILayout.Width(40));
            EditorCommon.DrawSeparator();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            EditorGUI.indentLevel += 1;
            packName = EditorGUILayout.TextField("Pack name", packName);
            EditorGUI.indentLevel -= 1;
            Rect btnRect = EditorGUILayout.GetControlRect(GUILayout.Width(100));
            if (GUI.Button(btnRect, "Select", EditorStyles.miniButton))
            {
                PackSelector selector = new PackSelector(false);
                selector.databasePath = desDbPath;
                PopupWindow.Show(btnRect, selector);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPackContentSelector()
        {
            if (packContentSize == null)
            {
                ResetPackContent();
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("PACK CONTENT", EditorCommon.BoldLabel, GUILayout.Width(100));
            EditorCommon.DrawSeparator();
            if (GUILayout.Button("R", EditorStyles.miniButton, GUILayout.Width(30)))
            {
                ResetPackContent();
            }
            EditorGUILayout.EndHorizontal();

            if (packContentSize.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Size", EditorCommon.CenteredBoldLabel);
                EditorGUILayout.LabelField("Level", EditorCommon.CenteredBoldLabel);
                EditorGUILayout.LabelField("Quantity", EditorCommon.CenteredBoldLabel);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel += 1;
            for (int i = 0; i < packContentSize.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                packContentSize[i] = (Size)EditorGUILayout.EnumPopup(packContentSize[i]);
                packContentLevel[i] = (Level)EditorGUILayout.EnumPopup(packContentLevel[i]);
                packContentCount[i] = Mathf.Max(1, EditorGUILayout.IntField(packContentCount[i]));

                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(30)))
                {
                    RemovePackContent(i);
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel -= 1;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DetectMergablePackContent();
            if (canMergePackContent)
            {
                if (GUILayout.Button("Merge", EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    MergePackContent();
                }
            }
            if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                AddPackContent();
            }
            EditorGUILayout.EndHorizontal();

        }

        private void ResetPackContent()
        {
            packContentSize = new List<Size>();
            packContentLevel = new List<Level>();
            packContentCount = new List<int>();
        }

        private void RemovePackContent(int index)
        {
            packContentSize.RemoveAt(index);
            packContentLevel.RemoveAt(index);
            packContentCount.RemoveAt(index);
        }

        private void AddPackContent()
        {
            packContentSize.Add(Size.Unknown);
            packContentLevel.Add(Level.UnGraded);
            packContentCount.Add(1);
        }

        private void DrawExclusiveDatabaseSelector()
        {
            if (exclusiveDb == null)
            {
                ResetExclusiveDb();
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("EXCLUDE FROM DATABASE", EditorCommon.BoldLabel, GUILayout.Width(170));
            exclusiveCheck = EditorGUILayout.Toggle(exclusiveCheck, GUILayout.Width(20));
            EditorCommon.DrawSeparator();
            if (GUILayout.Button("R", EditorStyles.miniButton, GUILayout.Width(30)))
            {
                ResetExclusiveDb();
            }
            EditorGUILayout.EndHorizontal();

            if (exclusiveCheck)
            {
                EditorGUI.indentLevel += 1;
                for (int i = 0; i < exclusiveDb.Count; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(exclusiveDb[i], EditorCommon.ItalicLabel);
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(30)))
                    {
                        RemoveExclusiveDb(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel -= 1;

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    string db = string.Empty;
                    EditorCommon.BrowseDatabase(ref db);
                    if (!db.Equals(srcDbPath))
                    {
                        AddExclusiveDb(db);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void ResetExclusiveDb()
        {
            exclusiveDb = new List<string>();
        }

        private void AddExclusiveDb(string db)
        {
            exclusiveDb.Add(db);
        }

        private void RemoveExclusiveDb(int index)
        {
            exclusiveDb.RemoveAt(index);
        }

        private void DrawOtherSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("OTHER SETTINGS", EditorCommon.BoldLabel, GUILayout.Width(120));
            EditorCommon.DrawSeparator();
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel += 1;
            stripStatisticInformation = EditorGUILayout.Toggle("Strip statistic", stripStatisticInformation);
            EditorGUI.indentLevel -= 1;
        }

        private void DrawFooter()
        {
            bool databaseExist =
                !string.IsNullOrEmpty(srcDbPath) && File.Exists(srcDbPath) &&
                !string.IsNullOrEmpty(desDbPath) && File.Exists(desDbPath);
            bool packNameNotEmpty =
                !string.IsNullOrEmpty(packName);
            bool packContentNotEmpty =
                packContentSize.Count > 0 &&
                packContentLevel.Count > 0 &&
                packContentCount.Count > 0;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(status, EditorStyles.miniLabel);
            GUI.enabled = databaseExist && packNameNotEmpty && packContentNotEmpty;
            if (GUILayout.Button("Pack", GUILayout.Width(200)))
            {
                Pack();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void Pack()
        {
            status = "Packing...";
            Repaint();

            List<string> issue = new List<string>();

            //merge pack content requirement to process easier and faster
            MergePackContent();

            //get all puzzle satisfied the condition expressed in each pack content
            HashSet<Puzzle>[] container = new HashSet<Puzzle>[packContentSize.Count];
            for (int i = 0; i < packContentSize.Count; ++i)
            {
                container[i] = new HashSet<Puzzle>();
                Packer.GetPuzzleByPackContent(srcDbPath, container[i], packContentSize[i], packContentLevel[i]);
            }

            //exclude duplicated puzzle if user wants to
            if (exclusiveCheck)
            {
                //get exclusive puzzle
                excludePuzzle = new HashSet<string>();
                Packer.GetExclusivePuzzle(exclusiveDb, excludePuzzle);

                //exclude
                for (int i = 0; i < container.Length; ++i)
                {
                    container[i].RemoveWhere((Puzzle p) =>
                    {
                        return excludePuzzle.Contains(p.puzzle);
                    });
                }
            }

            //check if the number of available puzzle in source database is enough for the requirement
            for (int i = 0; i < container.Length; ++i)
            {
                if (container[i].Count < packContentCount[i])
                {
                    string s = string.Format(
                        "- Not enough puzzle for pack content {0}: Size {1}, Level {2} ({3}/{4}) \n",
                        i, packContentSize[i], packContentLevel[i], container[i].Count, packContentCount[i]);
                    issue.Add(s);
                }
            }

            //if not enough, prompt to ensure the user wants to continue
            bool continuePacking = true;
            if (issue.Count > 0)
            {
                string message = "The process has some issues as follow: \n";
                for (int i = 0; i < issue.Count; ++i)
                {
                    message += issue[i];
                }
                message += "Do you want to continue?";
                continuePacking = EditorUtility.DisplayDialog("Confirm", message, "Yes", "No");
            }
            if (continuePacking)
            {
                //pour puzzles from source database to destination database
                for (int i = 0; i < container.Length; ++i)
                {
                    Packer.SavePuzzle(desDbPath, container[i], packContentCount[i], packName, stripStatisticInformation);
                }
                Data.UpdateInfoTable(desDbPath);
                EditorUtility.DisplayDialog("Done", "Done.", "OK");
            }
            status = "Done.";
        }

        private void DetectMergablePackContent()
        {
            for (int i = 0; i < packContentSize.Count - 1; ++i)
            {
                for (int j = i + 1; j < packContentSize.Count; ++j)
                {
                    if (packContentSize[i] == packContentSize[j] &&
                        packContentLevel[i] == packContentLevel[j])
                    {
                        canMergePackContent = true;
                        return;
                    }
                }
            }
            canMergePackContent = false;
        }

        private void MergePackContent()
        {
            for (int i = 0; i < packContentSize.Count; ++i)
            {
                for (int j = packContentSize.Count - 1; j > i; --j)
                {
                    if (packContentSize[i] == packContentSize[j] &&
                        packContentLevel[i] == packContentLevel[j])
                    {
                        packContentCount[i] += packContentCount[j];
                        RemovePackContent(j);
                    }
                }
            }
        }

    }
}