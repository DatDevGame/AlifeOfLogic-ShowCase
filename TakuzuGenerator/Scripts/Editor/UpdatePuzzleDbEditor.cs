using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Data;

namespace Takuzu.Generator
{
    public class UpdatePuzzleDbEditor : EditorWindow
    {

        [MenuItem("Tools/Takuzu Update", priority = 0)]
        public static void ShowWindow()
        {
            UpdatePuzzleDbEditor takuzuUpdate = GetWindow<UpdatePuzzleDbEditor>("Takuzu Update");
        }

        private string oldDataBasePath = "";
        private string newDataBasePath = "";
        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Old database path:               " + oldDataBasePath);
            if (GUILayout.Button("Browse Old DB", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                EditorCommon.BrowseDatabase(ref oldDataBasePath);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New database path:               " + newDataBasePath);
            if (GUILayout.Button("Browse New DB", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                EditorCommon.BrowseDatabase(ref newDataBasePath);
            }
            EditorGUILayout.EndHorizontal();
            EditorCommon.DrawSeparator();
            if (GUILayout.Button("Update"))
            {
                StartUpdate();
            }
        }

        private void StartUpdate()
        {
            if (oldDataBasePath == "" || newDataBasePath == "")
                return;

            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;

            string commandText = string.Empty;
            List<Puzzle> newPuzzles = new List<Puzzle>();
            List<Size> sizes = new List<Size>();
            List<Level> levels = new List<Level>();

            try
            {
                commandText = string.Format(
                    "SELECT PUZZLE, SOLUTION, SIZE, LEVEL, GIVENNUMBER FROM {0}",
                    Data.puzzleTableName);

                connection = Data.ConnectToDatabase(newDataBasePath);
                command = Data.CreateCommand(connection, commandText);
                reader = command.ExecuteReader();
                Puzzle p = null;
                while (reader.Read())
                {
                    try
                    {
                        string puzzleStr = reader.GetString(0);
                        string solutionStr = reader.GetString(1);
                        int size = reader.GetInt32(2);
                        int level = reader.GetInt32(3);
                        int gn = reader.GetInt32(4);
                        
                        p = new Puzzle((Size)size, (Level)level, puzzleStr, solutionStr, gn, -1, -1, -1);
                        newPuzzles.Add(p);
                        Debug.Log("Add new Puzzle");
                        if (!sizes.Contains(p.size))
                        {
                            sizes.Add(p.size);
                        }
                        if (!levels.Contains(p.level))
                        {
                            levels.Add(p.level);
                        }
                    }
                    catch
                    {
                        p = null;
                    }
                }

            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.ToString());
                Data.Flush(connection, command, reader);
                return;
            }
            Data.Flush(connection, command, reader);


            Debug.Log("New puzzle count: " + newPuzzles.Count);
            IDbConnection connection2 = null;
            IDbCommand command2 = null;

            string commandText2 = string.Empty;
            commandText2 = "";
            int affectedRow = 0;

            try
            {
                foreach (var level in levels)
                {
                    foreach (var size in sizes)
                    {
                        List<Puzzle> updateList = newPuzzles.FindAll(p => (p.size == size && p.level == level));
                        updateList.Sort(FillRateCompare);

                        string valueText = "";
                        for (int offset = 0; offset < updateList.Count; offset++)
                        {
                            valueText = string.Format(
                                "UPDATE PUZZLE SET PUZZLE = '{0}', SOLUTION = '{1}' , GIVENNUMBER = '{2}' WHERE ID IN(SELECT ID FROM {3} WHERE SIZE = {4} AND LEVEL = {5} LIMIT 1 OFFSET {6}); ",
                                updateList[offset].puzzle, updateList[offset].solution, updateList[offset].givenNum, Data.puzzleTableName, (int)size,(int) level, offset);
                            commandText2 += valueText;
                        }
                    }
                }
                Debug.Log(commandText2);

                connection2 = Data.ConnectToDatabase(oldDataBasePath);
                command2 = Data.CreateCommand(connection2, commandText2);
                affectedRow = command2.ExecuteNonQuery();
                Debug.Log(string.Format("Update {0} puzzles to database {1}.", affectedRow, oldDataBasePath));
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            Data.Flush(connection2, command2);
        }

        private static int FillRateCompare(Puzzle p1, Puzzle p2)
        {
            if (p1.puzzle.Split('.').Length < p2.puzzle.Split('.').Length)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }
}