using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CountryCodeMapper))]
public class CountryCodeMapperEditor : Editor
{
    CountryCodeMapper instance;

    public void OnEnable()
    {
        instance = (CountryCodeMapper)target;
    }

    private string newCountryCode = "";
    private string newCountryName = "";

    public override void OnInspectorGUI()
    {
        this.newCountryCode = EditorGUILayout.TextField("Country Code", this.newCountryCode);
        this.newCountryName = EditorGUILayout.TextField("Country Name", this.newCountryName);

        if (GUILayout.Button("Add"))
        {
            if (string.IsNullOrEmpty(this.newCountryCode) || string.IsNullOrEmpty(this.newCountryName))
                return;

            if (instance.map.FindIndex(item => item.code == this.newCountryCode) == -1)
            {
                instance.map.Add(new CountryCodeMapperEntry()
                {
                    code = this.newCountryCode,
                    englishName = this.newCountryName
                });
            }

            instance.map.Sort(delegate (CountryCodeMapperEntry x, CountryCodeMapperEntry y)
            {
                return string.Compare(x.englishName, y.englishName, System.StringComparison.Ordinal);
            });

            this.newCountryCode = "";
            this.newCountryName = "";
        }

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("#", EditorStyles.boldLabel, GUILayout.Width(50), GUILayout.ExpandWidth(false));
        EditorGUILayout.LabelField("CODE", EditorStyles.boldLabel, GUILayout.Width(30), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("ENGLISH NAME", EditorStyles.boldLabel, GUILayout.Width(30), GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < instance.map.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(i.ToString(), EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.TextField(instance.map[i].code);
            EditorGUILayout.TextField(instance.map[i].englishName);
            EditorGUILayout.EndHorizontal();
        }

        EditorUtility.SetDirty(instance);
    }
}
