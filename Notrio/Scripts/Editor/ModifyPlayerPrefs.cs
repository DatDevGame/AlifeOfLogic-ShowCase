using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class ModifyPlayerPrefs : EditorWindow {
    [MenuItem("Tools/Modify PlayerPrefs")]
    public static void ShowWindow()
    {
        ModifyPlayerPrefs window = GetWindow<ModifyPlayerPrefs>();
        window.Show();
    }
    public enum SAVETYPE
    {
        INT,
        STRING,
        FLOAT
    }

    public SAVETYPE sType;
    public string stringValue;
    public string stringKey;

    private void OnGUI()
    {
        stringKey = EditorGUILayout.TextField("Key: ", stringKey);
        stringValue = EditorGUILayout.TextField("Value: ", stringValue);
        sType = (SAVETYPE)EditorGUILayout.EnumPopup("data type to save: ", sType);
        EditorGUILayout.BeginHorizontal("Button");
        if (GUILayout.Button("Get"))
            GetFromPlayerPrefs();
        if (GUILayout.Button("Save/Update"))
            SaveToPlayerPrefs();
        if (GUILayout.Button("Delete"))
            DeleteKeyInPlayerPrefs();
        EditorGUILayout.EndHorizontal();
    }

    private void GetFromPlayerPrefs()
    {
        if (String.IsNullOrEmpty(stringKey))
        {
            Debug.Log("String key is empty");
            return;
        }
        if (PlayerPrefs.HasKey(stringKey) == false)
        {
            Debug.Log("Key not found");
            return;
        }
        switch (sType)
        {
            case SAVETYPE.INT:
                Debug.LogFormat("{0}\n{1}", stringKey, PlayerPrefs.GetInt(stringKey));
                break;
            case SAVETYPE.FLOAT:
                Debug.LogFormat("{0}\n{1}", stringKey, PlayerPrefs.GetFloat(stringKey));
                break;
            case SAVETYPE.STRING:
                Debug.LogFormat("{0}\n{1}", stringKey, PlayerPrefs.GetString(stringKey));
                break;
        }
    }

    private void SaveToPlayerPrefs()
    {
        if (String.IsNullOrEmpty(stringKey))
        {
            Debug.Log("String key is empty");
            return;
        }

        if(String.IsNullOrEmpty(stringValue))
        {
            Debug.Log("Value can not be empty");
            return;
        }

        int intData = 0;
        float floatData = 0;
        string strData = "";

        switch (sType)
        {
            case SAVETYPE.INT:
                if (int.TryParse(stringValue, out intData) == false)
                    return;
                break;
            case SAVETYPE.FLOAT:
                if (float.TryParse(stringValue, out floatData) == false)
                    return; 
                break;
            case SAVETYPE.STRING:
                strData = stringValue;
                break;
        }

        switch (sType)
        {
            case SAVETYPE.INT:
                PlayerPrefs.SetInt(stringKey, intData);
                break;
            case SAVETYPE.FLOAT:
                PlayerPrefs.SetFloat(stringKey, floatData);
                break;
            case SAVETYPE.STRING:
                PlayerPrefs.SetString(stringKey, strData);
                break;
        }
    }

    private void DeleteKeyInPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(stringKey);
    }
}
