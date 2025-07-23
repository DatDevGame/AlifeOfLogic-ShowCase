using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Takuzu.Generator
{
    [CustomEditor(typeof(CryptoKey))]
    public class CryptoKeyEditor : Editor
    {
        CryptoKey instance;
        int min;
        int max;

        private void OnEnable()
        {
            instance = (CryptoKey)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("Pre encrypt", EditorStyles.boldLabel);
            min = EditorGUILayout.IntField("Min",min);
            max = EditorGUILayout.IntField("Max", max);
            if (GUILayout.Button("Pre encrypt"))
            {
                instance.BakeValue(min, max);
                EditorUtility.SetDirty(instance);
            }
        }
    }
}