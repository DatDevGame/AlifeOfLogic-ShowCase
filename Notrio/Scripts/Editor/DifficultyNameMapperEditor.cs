using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Takuzu.Generator;

namespace Takuzu
{
    [CustomEditor(typeof(DifficultyNameMapper))]
    public class DifficultyNameMapperEditor : Editor
    {
        DifficultyNameMapper instance;
        int toRemoveIndex;

        private void OnEnable()
        {
            instance = (DifficultyNameMapper)target;
            toRemoveIndex = -1;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("DIFFICULTY", EditorStyles.boldLabel, GUILayout.Width(70), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("DISPLAY NAME", EditorStyles.boldLabel, GUILayout.Width(70), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(" ", GUILayout.Width(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < instance.Map.Count; ++i)
            {
                DifficultyNameMapperKeyValuePair p = instance.Map[i];
                EditorGUILayout.BeginHorizontal();
                p.level = (Level)EditorGUILayout.EnumPopup(p.level);
                p.displayName = EditorGUILayout.TextField(p.displayName);
                if (GUILayout.Button("-", GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    toRemoveIndex = i;
                }
                EditorGUILayout.EndHorizontal();
                instance.Map[i] = p;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add", GUILayout.Width(100), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                instance.Map.Add(new DifficultyNameMapperKeyValuePair());
            }
            EditorGUILayout.EndHorizontal();

            if (toRemoveIndex>0 && toRemoveIndex<instance.Map.Count)
            {
                instance.Map.RemoveAt(toRemoveIndex);
                toRemoveIndex = -1;
            }

            EditorUtility.SetDirty(instance);
        }
    }
}