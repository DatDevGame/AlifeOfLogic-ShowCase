using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Takuzu.Achievements
{
    [CustomEditor(typeof(AchievementInfo))]
    public class AchievementInfoEditor : Editor
    {
        AchievementInfo instance;
        List<string> checkerClassFullName;

        string[] classDisplayOption;
        int[] classSelectOption;
        int selectedClass;

        private void OnEnable()
        {
            instance = (AchievementInfo)target;
            checkerClassFullName = new List<string>();

            Type checkerBaseType = typeof(Takuzu.Achievements.AchievementChecker);
            List<Type> checkerType = new List<Type>(checkerBaseType.Assembly.GetTypes());
            for (int i = 0; i < checkerType.Count; ++i)
            {
                if (checkerType[i].IsSubclassOf(checkerBaseType))
                {
                    checkerClassFullName.Add(checkerType[i].FullName);
                }
            }
            checkerClassFullName.Insert(0, "None");
            selectedClass = checkerClassFullName.FindIndex((c) => { return c.Equals(instance.progressCheckerClass); });
            selectedClass = Mathf.Max(0, selectedClass);
            classDisplayOption = checkerClassFullName.ToArray();
            classSelectOption = Utilities.GetIndicesArray(classDisplayOption.Length);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            selectedClass = EditorGUILayout.IntPopup("Checker class", selectedClass, classDisplayOption, classSelectOption);
            instance.progressCheckerClass = classDisplayOption[selectedClass];
            EditorUtility.SetDirty(instance);

            if (GUILayout.Button("Progress"))
            {
                Debug.Log(instance.Progress);
            }
            if (GUILayout.Button("Is Completed"))
            {
                Debug.Log(instance.IsCompleted);
            }
        }
    }
}