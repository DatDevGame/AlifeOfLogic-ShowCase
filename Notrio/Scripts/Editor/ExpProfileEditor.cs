using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Takuzu
{
    [CustomEditor(typeof(ExpProfile))]
    public class ExpProfileEditor : Editor
    {
        ExpProfile instance;
        private void OnEnable()
        {
            instance = (ExpProfile)target;
        }

        public override void OnInspectorGUI()
        {
            if (instance.exp == null)
            {
                instance.exp = new List<int>();
                instance.rank = new List<string>();
                instance.icon = new List<Sprite>();
                instance.accentColor = new List<Color>();
                CreateNewLevel();
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Width(75));
            EditorGUILayout.LabelField("EXP", EditorStyles.boldLabel, GUILayout.Width(30), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("RANK", EditorStyles.boldLabel, GUILayout.Width(30), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("ICON", EditorStyles.boldLabel, GUILayout.Width(30), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("ACCENT", EditorStyles.boldLabel, GUILayout.Width(30), GUILayout.ExpandWidth(true));
            EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();
            for (int i = 1; i < instance.LevelCount; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Level " + i, EditorStyles.boldLabel, GUILayout.Width(75));
                instance.exp[i] = EditorGUILayout.IntField(instance.exp[i]);
                instance.rank[i] = EditorGUILayout.TextField(instance.rank[i]);
                instance.icon[i] = EditorGUILayout.ObjectField(instance.icon[i], typeof(Sprite), false) as Sprite;
                instance.accentColor[i] = EditorGUILayout.ColorField(instance.accentColor[i]);
                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(EditorGUIUtility.singleLineHeight)))
                {
                    RemoveLevel(i);
                }
                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(EditorGUIUtility.singleLineHeight)))
            {
                CreateNewLevel();
            }
            EditorGUILayout.EndHorizontal();

            EditorUtility.SetDirty(instance);
        }

        private void CreateNewLevel()
        {
            instance.exp.Add(0);
            instance.rank.Add("");
            instance.icon.Add(null);
            instance.accentColor.Add(Color.white);
        }

        private void RemoveLevel(int i)
        {
            instance.exp.RemoveAt(i);
            instance.rank.RemoveAt(i);
            instance.icon.RemoveAt(i);
            instance.accentColor.RemoveAt(i);
        }
    }
}