using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class FlagEditorParam : ScriptableObject
{
    public List<Texture2D> leftAlign;
    public List<Texture2D> centeredAlign;
    public List<Texture2D> rightAlign;



    public FlagEditorParam()
    {
        leftAlign = new List<Texture2D>();
        centeredAlign = new List<Texture2D>();
        rightAlign = new List<Texture2D>();
    }
}

public class FlagEditor : EditorWindow
{
    FlagEditorParam param;
    SerializedObject so;
    SerializedProperty l;
    SerializedProperty m;
    SerializedProperty r;

    Vector2 scrollPos1, scrollPos2, scrollPos3;

    [MenuItem("Tools/Flag editor")]
    public static void ShowWindow()
    {
        FlagEditor window = GetWindow<FlagEditor>();
        window.Show();
    }

    public void OnEnable()
    {
        param = ScriptableObject.CreateInstance<FlagEditorParam>();
        so = new SerializedObject(param);
        l = so.FindProperty("leftAlign");
        m = so.FindProperty("centeredAlign");
        r = so.FindProperty("rightAlign");
    }

    public void OnGUI()
    {
        EditorGUILayout.PropertyField(l, true);
        DrawTextures(param.leftAlign, ref scrollPos1);
        EditorGUILayout.PropertyField(m, true);
        DrawTextures(param.centeredAlign, ref scrollPos2);
        EditorGUILayout.PropertyField(r, true);
        DrawTextures(param.rightAlign, ref scrollPos3);

        so.ApplyModifiedProperties();

        if (GUILayout.Button("Crop"))
        {
            Crop();
        }
    }

    public void DrawTextures(List<Texture2D> l, ref Vector2 scrollPos)
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < l.Count; ++i)
        {
            if (GUILayout.Button(l[i], GUILayout.Width(160), GUILayout.Height(100)))
            {
                l.RemoveAt(i);
                so.Update();
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    public void Crop()
    {
        so.Update();
        for (int i=0;i<param.leftAlign.Count;++i)
        {
            CropRight(param.leftAlign[i]);
        }

        for (int i = 0; i < param.centeredAlign.Count; ++i)
        {
            CropSide(param.centeredAlign[i]);
        }

        for (int i = 0; i < param.rightAlign.Count; ++i)
        {
            CropLeft(param.rightAlign[i]);
        }
        so.Update();
        AssetDatabase.Refresh();
    }

    public void CropRight(Texture2D t)
    {
        int size = Mathf.Min(t.width, t.height);
        int offset = 0;
        Color[] c = t.GetPixels(offset, 0, size, size);
        Texture2D des = new Texture2D(size, size, TextureFormat.ARGB32, false);
        des.SetPixels(c);
        des.Apply();
        if (!Directory.Exists(Path.Combine(Application.dataPath,"Resources/flags")))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Resources/flags"));
        }
        File.WriteAllBytes(Path.Combine(Application.dataPath, string.Format("Resources/flags/{0}.png", t.name)), des.EncodeToPNG());
    }

    public void CropSide(Texture2D t)
    {
        int size = Mathf.Min(t.width, t.height);
        int offset = (int)((Mathf.Max(t.width, t.height) - size) * 0.5f);
        Color[] c = t.GetPixels(offset, 0, size, size);
        Texture2D des = new Texture2D(size, size, TextureFormat.ARGB32, false);
        des.SetPixels(c);
        des.Apply();
        if (!Directory.Exists(Path.Combine(Application.dataPath, "Resources/flags")))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Resources/flags"));
        }
        File.WriteAllBytes(Path.Combine(Application.dataPath, string.Format("Resources/flags/{0}.png", t.name)), des.EncodeToPNG());
    }

    public void CropLeft(Texture2D t)
    {
        int size = Mathf.Min(t.width, t.height);
        int offset = (int)(Mathf.Max(t.width, t.height) - size);
        Color[] c = t.GetPixels(offset, 0, size, size);
        Texture2D des = new Texture2D(size, size, TextureFormat.ARGB32, false);
        des.SetPixels(c);
        des.Apply();
        if (!Directory.Exists(Path.Combine(Application.dataPath, "Resources/flags")))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Resources/flags"));
        }
        File.WriteAllBytes(Path.Combine(Application.dataPath, string.Format("Resources/flags/{0}.png", t.name)), des.EncodeToPNG());
    }
}
