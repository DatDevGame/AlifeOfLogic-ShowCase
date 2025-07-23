using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Takuzu.Generator;

namespace Takuzu
{
    public class ExpJudgingProfileImporter : EditorWindow
    {
        private string csvPath;
        Dictionary<int, Size> sizeDict;
        Dictionary<int, Level> levelDict;

        [MenuItem("Tools/Exp Judging Profile Importer")]
        public static void ShowWindow()
        {
            ExpJudgingProfileImporter window = EditorWindow.GetWindow<ExpJudgingProfileImporter>();
            window.Show();
        }

        private void OnEnable()
        {

        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Csv file path", csvPath);
            if (GUILayout.Button("Browse", GUILayout.Width(100), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                csvPath = EditorUtility.OpenFilePanel("Select .csv file", "", "csv");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Import", GUILayout.Width(200), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                Import();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void Import()
        {
            sizeDict = new Dictionary<int, Size>();
            sizeDict.Add(6, Size.Six);
            sizeDict.Add(8, Size.Eight);
            sizeDict.Add(10, Size.Ten);
            sizeDict.Add(12, Size.Twelve);

            levelDict = new Dictionary<int, Level>();
            levelDict.Add(1, Level.Easy);
            levelDict.Add(2, Level.Medium);
            levelDict.Add(3, Level.Hard);
            levelDict.Add(4, Level.Evil);
            levelDict.Add(5, Level.Insane);

            string[] guid = AssetDatabase.FindAssets("t:ExpJudgingProfile");
            List<ExpJudgingProfile> profile = new List<ExpJudgingProfile>();
            for (int i = 0; i < guid.Length; ++i)
            {
                profile.Add(AssetDatabase.LoadAssetAtPath<ExpJudgingProfile>(AssetDatabase.GUIDToAssetPath(guid[i])));
            }

            List<string> csvString = new List<string>();
            csvString.AddRange(System.IO.File.ReadAllLines(csvPath));
            csvString.RemoveAt(0);
            for (int i = 0; i < csvString.Count; ++i)
            {
                string[] s = csvString[i].Replace(',','.').Split(';');
                Size size = sizeDict[(int)float.Parse(s[0])];
                Level level = levelDict[(int)float.Parse(s[1])];
                int expMin = (int)float.Parse(s[2]);
                int baseMin = (int)float.Parse(s[3]);
                int baseMax = (int)float.Parse(s[4]);
                int timeMin = (int)float.Parse(s[5]);
                int timeMax = (int)float.Parse(s[6]);

                Keyframe k1 = new Keyframe(0, baseMax);
                Keyframe k2 = new Keyframe(timeMin, baseMax);
                Keyframe k3 = new Keyframe(timeMax, baseMin);
                AnimationCurve curve = new AnimationCurve(k1, k2, k3);

                List<ExpJudgingProfile> p = profile.FindAll((pi) => { return pi.size == size && pi.level == level; });
                for (int j=0;j<p.Count;++j)
                {
                    p[j].minExp = expMin;
                    p[j].baseExpByTime = curve;
                    EditorUtility.SetDirty(p[j]);
                }
            }
        }
    }
}