using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameSparks;
using GameSparks.Core;
using GameSparks.Api;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using Takuzu.Generator;
using System.Data;
using Mono.Data.Sqlite;
using System;
using Newtonsoft.Json;
using System.IO;

namespace Takuzu
{
    public class DailyPuzzleUploader : EditorWindow
    {
        private string databasePath;
        private GameSparksUnity gamespark;
        private string eventKey = "UPLOAD_JSON_PUZZLE";
        private string eventAttributeCollectionName = "COLLECTION_NAME";
        private string eventAttributePuzzles = "PUZZLES";
        private int total;
        private string collectionName = "multiplayer_puzzle_library";
        [MenuItem("Tools/Daily puzzle uploader")]
        public static void ShowWindow()
        {
            DailyPuzzleUploader window = GetWindow<DailyPuzzleUploader>();
            window.Show();
        }

        private void OnEnable()
        {
            gamespark = FindObjectOfType<GameSparksUnity>();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel("Database path:               " + databasePath);
            if (GUILayout.Button("Browse...", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                BrowseDatabase();
            }
            EditorGUILayout.EndHorizontal();
            gamespark = EditorGUILayout.ObjectField("GameSpark", gamespark, typeof(GameSparksUnity), false) as GameSparksUnity;
            collectionName = EditorGUILayout.TextField("Collection Name: ", collectionName);
            bool canUpload =
                !string.IsNullOrEmpty(databasePath) &&
                gamespark != null &&
                gamespark.settings != null;
            if (canUpload)
            {
                GUI.enabled = !uploading;
                if (GUILayout.Button("Upload"))
                {
                    CreatePuzzlePacks();
                    total = puzzlesJsonStrings.Count;
                    UploadMultiplePackes(5);
                }
                GUI.enabled = true;
            }
            else
            {
                string msg =
                    "Make sure: \n" +
                    "\t- Database is selected.\n" +
                    "\t- GameSparkUnity component is assigned.\n" +
                    "\t- GameSparkSettings slot of the component is assigned.\n";
                EditorGUILayout.HelpBox(msg, MessageType.Info);
            }
        }
        private int uploadCount = 0;
        private bool uploading = false;
        private void UploadMultiplePackes(int packcount){
            int sentPackCount = Math.Min(packcount, puzzlesJsonStrings.Count);
            if(sentPackCount == 0){
                uploading = false;
            }else{
                uploading = true;
            }
            for (int i = 0; i < sentPackCount; i++)
            {
                UploadPuzzlePack(puzzlesJsonStrings[0], ()=>{
                    uploadCount --;
                    if(uploadCount == 0){
                        UploadMultiplePackes(packcount);
                    }
                });
                puzzlesJsonStrings.RemoveAt(0);
                uploadCount++;
            }
        }
        private void UploadPuzzlePack(string json, Action callback)
        {
            string sentJson = json;
            new LogEventRequest()
                .SetEventKey(eventKey)
                .SetEventAttribute(eventAttributeCollectionName, collectionName)
                .SetEventAttribute(eventAttributePuzzles, sentJson)
                .Send(response =>{
                    if (response.HasErrors)
                    {
                        Debug.LogWarning(response.Errors.JSON);
                        UploadPuzzlePack(sentJson, callback);
                    }else{
                        total--;
                        Debug.Log("Uploaded 1 pack "+ total +"left");
                        callback();
                    }
                });
        }

        public class PuzzleJSONObj{
            public string puzzle = "";
            public string solution = "";
            public int size = 6;
            public int level = 1;
            public int exclude = 0;
        }
        List<string> puzzlesJsonStrings = new List<string>();
        private void CreatePuzzlePacks()
        {
            List<Puzzle> puzzle = new List<Puzzle>();
            GetAllPuzzle(databasePath, puzzle);
            List<PuzzleJSONObj> _data = new List<PuzzleJSONObj>();
            int created = 0;
            while(created < puzzle.Count){
                int lastCreatedCount = created;
                int upperLimit = Math.Min(lastCreatedCount + 500, puzzle.Count);
                for (int i = lastCreatedCount; i < upperLimit; i++)
                {
                    _data.Add(new PuzzleJSONObj(){
                        puzzle = puzzle[i].puzzle,
                        solution = puzzle[i].solution,
                        size = (int) puzzle[i].size,
                        level = (int) puzzle[i].level
                    });
                    created++;
                }
                Debug.Log("json puzzle created with "+ _data.Count + "puzzles");
                string json = JsonConvert.SerializeObject(_data.ToArray());
                puzzlesJsonStrings.Add(json);
                _data.Clear();
            }
        }

        private void BrowseDatabase()
        {
            string selectedFile = EditorUtility.OpenFilePanelWithFilters("Select database file...", Application.dataPath, new string[2] { "Database (.db)", "db" });
            if (!string.IsNullOrEmpty(selectedFile))
                databasePath = selectedFile;
        }

        public void GetAllPuzzle(string db, ICollection<Puzzle> puzzleContainer)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            string commandText = string.Empty;

            try
            {
                commandText = string.Format("SELECT PUZZLE, SOLUTION, SIZE, LEVEL FROM {0}", Data.puzzleTableName);
                connection = Data.ConnectToDatabase(db);
                command = Data.CreateCommand(connection, commandText);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    try
                    {
                        string puzzle = reader.GetString(0);
                        string solution = reader.GetString(1);
                        Size size = (Size)reader.GetInt32(2);
                        Level level = (Level)reader.GetInt32(3);

                        Puzzle p = new Puzzle(size, level, puzzle, solution, -1, -1, -1, -1);
                        puzzleContainer.Add(p);
                    }
                    catch (System.InvalidCastException)
                    {
                        Data.Flush(connection, command, reader);
                        return;
                    }
                }

            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }

            Data.Flush(connection, command, reader);
        }
    }
}