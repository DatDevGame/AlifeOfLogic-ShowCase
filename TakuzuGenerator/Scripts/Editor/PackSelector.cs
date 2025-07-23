using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Takuzu.Generator;

namespace Takuzu.Generator
{
    public class PackSelector : PopupWindowContent
    {
        public static System.Action<string> OnPackSelected = delegate { };
        public string databasePath;
        public static string selectedPackName = string.Empty;

        List<string> packs;

        private GUIStyle itemStyle;
        private GUIStyle selectedStyle;
        private bool showAllPackOption;

        public PackSelector(bool showAllPackOption = false)
        {
            this.showAllPackOption = showAllPackOption;
            itemStyle = new GUIStyle();
            Color hoverColor = new Color32(210, 210, 210, 255);
            Texture2D hoverTex = new Texture2D(1, 1);
            hoverTex.SetPixels32(new Color32[1] { hoverColor });
            hoverTex.Apply();
            itemStyle.hover.background = hoverTex;
            itemStyle.alignment = TextAnchor.MiddleCenter;
            itemStyle.fixedHeight = 20;

            selectedStyle = new GUIStyle();
            Color selectedColor = new Color32(0, 180, 255, 255);
            Texture2D selectedTex = new Texture2D(1, 1);
            selectedTex.SetPixels32(new Color32[1] { selectedColor });
            selectedTex.Apply();
            selectedStyle.hover.background = selectedTex;
            selectedStyle.alignment = TextAnchor.MiddleCenter;
            selectedStyle.fixedHeight = 20;
        }

        public override void OnGUI(Rect r)
        {
            if (packs == null)
                LoadPack();

            if (showAllPackOption)
            {
                GUIStyle style = selectedPackName.CompareTo(string.Empty) == 0 ? selectedStyle : itemStyle;
                if (GUILayout.Button("All packs", style))
                {
                    selectedPackName = string.Empty;
                    OnPackSelected(selectedPackName);
                }
            }
            if (packs.Count == 0 && !showAllPackOption)
            {
                EditorGUILayout.HelpBox("No pack found!", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < packs.Count; ++i)
                {
                    GUIStyle style = selectedPackName.CompareTo(packs[i]) == 0 ? selectedStyle : itemStyle;
                    if (GUILayout.Button(packs[i], style))
                    {
                        selectedPackName = packs[i].ToString();
                        OnPackSelected(packs[i]);
                    }
                }
            }
        }

        public void LoadPack()
        {
            packs = new List<string>();
            Data.GetAllPack(databasePath, packs);
        }
    }
}