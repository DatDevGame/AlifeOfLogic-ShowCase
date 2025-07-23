using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public static class EditorUtils
{
    public static T FindAssetOfType<T>(string path = "Assets", string filter = "") where T : UnityEngine.Object
    {
        var assets = FindAssetsOfType<T>(path, filter);
        if (assets.Count <= 0)
        {
            Debug.Log($"<color=yellow>WARNING!!!:</color> Asset with type {typeof(T)} is not founded");
            return null;
        }
        return assets[0];
    }

    public static UnityEngine.Object FindAssetOfType(Type type, string path = "Assets", string filter = "")
    {
        var assets = FindAssetsOfType(type, path, filter);
        if (assets.Count <= 0)
        {
            Debug.Log($"<color=yellow>WARNING!!!:</color> Asset with type {type} is not founded");
            return null;
        }
        return assets[0];
    }

    public static List<T> FindAssetsOfType<T>(string path = "Assets", string filter = "") where T : UnityEngine.Object
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name} {filter}", new[] { path });
        List<T> result = new List<T>();
        foreach (var t in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(t);
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                result.Add(asset);
            }
        }
        return result;
    }

    public static List<UnityEngine.Object> FindAssetsOfType(Type type, string path = "Assets", string filter = "")
    {
        var guids = AssetDatabase.FindAssets($"t:{type.Name} {filter}", new[] { path });
        List<UnityEngine.Object> result = new List<UnityEngine.Object>();
        foreach (var t in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(t);
            var asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
            if (asset != null)
            {
                result.Add(asset);
            }
        }
        return result;
    }

    public static string FindScriptByName(string name)
    {
        var files = Directory.GetFiles(Application.dataPath, $"{name}.cs", SearchOption.AllDirectories);
        if (files == null || files.Length <= 0)
        {
            Debug.Log($"<color=yellow>WARNING!!!:</color> Script {name} is not founded");
            return null;
        }
        return files[0];
    }

    public static string ToRelativePath(this string absolutePath)
    {
        return absolutePath.Substring(absolutePath.IndexOf("Assets"));
    }

    public static void GenerateEnum(string filePathAndName, List<string> enumList, string summary, string enumName)
    {
        // Create a new enum file.
        if (File.Exists(filePathAndName))
        {
            File.Delete(filePathAndName);
        }
        using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
        {
            // Write the enum declaration.
            streamWriter.WriteLine($"/// <summary>\n/// {summary}\n/// </summary>");
            streamWriter.WriteLine($"public enum {enumName}");
            streamWriter.WriteLine("{");

            // Write the enum values.
            for (int i = 0; i < enumList.Count; i++)
            {
                streamWriter.WriteLine("    " + enumList[i] + ",");
            }

            // Close the enum declaration.
            streamWriter.Write("}");
        }

        // Refresh the Assets folder.
        AssetDatabase.Refresh();
    }

    public static string GetCurrentActiveAssetPath()
    {
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (folderPath.Contains("."))
            folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
        return folderPath;
    }
}
#endif