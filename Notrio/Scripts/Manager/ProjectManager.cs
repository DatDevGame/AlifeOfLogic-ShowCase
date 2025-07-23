using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor;
#endif

#if UNITY_EDITOR
public class ProjectManager : MonoBehaviour, IPreprocessBuild
#else
public class ProjectManager : MonoBehaviour
#endif
{
#if UNITY_EDITOR
    public static string FilePath
    {
        get
        {
            return "Assets/Resources/build.txt";
        }
    }

    public int callbackOrder
    {
        get
        {
            return 0;
        }
    }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        string buildTime = "Build time: " + DateTime.Now.ToString();
        Debug.Log(buildTime);
        System.IO.File.WriteAllText(FilePath, buildTime);
        AssetDatabase.Refresh();
    }
	/*
    public void OnPreprocessBuild(BuildReport report)
    {
        string buildTime = "Build time: " + DateTime.Now.ToString();
        Debug.Log(buildTime);
        System.IO.File.WriteAllText(FilePath, buildTime);
        AssetDatabase.Refresh();
    }
	*/
#endif

    public Mask mainCanvasMask;
    public Mask overlayMask;

    string buildTime;
    GUIStyle style;

    private void Awake()
    {
        buildTime = Resources.Load<TextAsset>("build").text;
        style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = (int)(Screen.height * 0.0123f);
        style.alignment = TextAnchor.UpperRight;

        DontDestroyOnLoad(gameObject);
    }
    
    private void OnGUI()
    {
        Rect r = new Rect(0, 0, Screen.width, 100);
        GUI.Label(r, buildTime, style);
    }
}
