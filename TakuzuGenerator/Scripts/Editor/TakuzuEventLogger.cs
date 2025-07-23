using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Takuzu.Generator;

public class TakuzuEventLogger : EditorWindow
{
    [MenuItem("Tools/Event logger", priority = 4)]
    public static void ShowWindow()
    {
        TakuzuEventLogger window = GetWindow<TakuzuEventLogger>();
        window.Show();
    }

    private void OnEnable()
    {
        Filler.onFailed += OnFillerFailed;
        Generator.onFailed += OnGeneratorFailed;

        fillerReplicate = fillerExclude = fillerUnique = fillerTriple = fillerEqual = 0;
        generatorReplicate = generatorExclude = generatorUniqueSolution = generatorLevel = 0;
    }

    int fillerReplicate, fillerExclude, fillerUnique, fillerTriple, fillerEqual;
    private void OnFillerFailed(string s)
    {
        if (s.Equals("replicate"))
        {
            fillerReplicate += 1;
        }
        else if (s.Equals("exclude"))
        {
            fillerExclude += 1;
        }
        else if (s.Equals("unique"))
        {
            fillerUnique += 1;
        }
        else if (s.Equals("triple"))
        {
            fillerTriple += 1;
        }
        else if (s.Equals("equal"))
        {
            fillerEqual += 1;
        }
    }

    private int generatorReplicate, generatorExclude, generatorUniqueSolution, generatorLevel;
    private void OnGeneratorFailed(string s)
    {
        if (s.Equals("replicate"))
        {
            generatorReplicate += 1;
        }
        else if (s.Equals("exclude"))
        {
            generatorExclude += 1;
        }
        else if (s.Equals("unique solution"))
        {
            generatorUniqueSolution += 1;
        }
        else if (s.Equals("level"))
        {
            generatorLevel += 1;
        }
    }

    public void Update()
    {
        Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Filler:", EditorStyles.boldLabel);
        EditorGUI.indentLevel += 1;
        EditorGUILayout.LabelField("Replicate: " + fillerReplicate);
        EditorGUILayout.LabelField("Exlude: " + fillerExclude);
        EditorGUILayout.LabelField("Unique: " + fillerUnique);
        EditorGUILayout.LabelField("Triple: " + fillerTriple);
        EditorGUILayout.LabelField("Equal: " + fillerEqual);
        EditorGUI.indentLevel -= 1;

        EditorGUILayout.LabelField("Generator:", EditorStyles.boldLabel);
        EditorGUI.indentLevel += 1;
        EditorGUILayout.LabelField("Replicate: " + generatorReplicate);
        EditorGUILayout.LabelField("Exlude: " + generatorExclude);
        EditorGUILayout.LabelField("Unique solution: " + generatorUniqueSolution);
        EditorGUILayout.LabelField("Level: " + generatorLevel);
        EditorGUI.indentLevel -= 1;

        EditorGUILayout.Space();
        if (GUILayout.Button("Reset"))
        {
            fillerReplicate = fillerExclude = fillerUnique = fillerTriple = fillerEqual = 0;
            generatorReplicate = generatorExclude = generatorUniqueSolution = generatorLevel = 0;
        }

    }
}
