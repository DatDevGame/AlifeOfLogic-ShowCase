using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Takuzu.Generator;
using System.IO;

namespace Takuzu
{
    [CustomEditor(typeof(PuzzlePack))]
    public class PuzzlePackEditor : Editor
    {
        PuzzlePack instance;
        Object db;

        public void OnEnable()
        {
            instance = (PuzzlePack)target;
        }

        public override void OnInspectorGUI()
        {
            db = EditorGUILayout.ObjectField("Database file", db, typeof(Object), false);
            if (db != null)
            {
                instance.DbPath = AssetDatabase.GetAssetPath(db);
            }
            else
            {
                if (!string.IsNullOrEmpty(instance.DbPath))
                {
                    db = AssetDatabase.LoadAssetAtPath<Object>(instance.DbPath);
                }
                else
                {
                    instance.DbPath = string.Empty;
                }
            }
            EditorGUILayout.LabelField("Db path ", instance.DbPath, EditorCommon.ItalicLabel);
            instance.packName = EditorGUILayout.TextField("Pack name", instance.packName);
            instance.puzzleCountOfSize6 = PuzzleManager.CountPuzzleOfPack(instance, Size.Six);
            instance.puzzleCountOfSize8 = PuzzleManager.CountPuzzleOfPack(instance, Size.Eight);
            instance.puzzleCountOfSize10 = PuzzleManager.CountPuzzleOfPack(instance, Size.Ten);
            instance.puzzleCountOfSize12 = PuzzleManager.CountPuzzleOfPack(instance, Size.Twelve);
            instance.puzzleCount =
                instance.puzzleCountOfSize6 +
                instance.puzzleCountOfSize8 +
                instance.puzzleCountOfSize10 +
                instance.puzzleCountOfSize12;
            EditorGUILayout.LabelField("Puzzle count ", instance.puzzleCount.ToString(), EditorCommon.ItalicLabel);
            EditorGUI.indentLevel += 1;
            EditorGUILayout.LabelField("Size 6", instance.puzzleCountOfSize6.ToString(), EditorCommon.ItalicLabel);
            EditorGUILayout.LabelField("Size 8", instance.puzzleCountOfSize8.ToString(), EditorCommon.ItalicLabel);
            EditorGUILayout.LabelField("Size 10", instance.puzzleCountOfSize10.ToString(), EditorCommon.ItalicLabel);
            EditorGUILayout.LabelField("Size 12", instance.puzzleCountOfSize12.ToString(), EditorCommon.ItalicLabel);
            EditorGUI.indentLevel -= 1;
            instance.difficulties = PuzzleManager.GetPackDifficulties(instance);
            EditorGUILayout.LabelField("Difficulties", instance.difficulties.ListElementToString(", "));
            instance.description = EditorGUILayout.TextField("Description", instance.description);
            instance.price = EditorGUILayout.IntField("Price", instance.price);
            EditorUtility.SetDirty(instance);
        }
    }
}