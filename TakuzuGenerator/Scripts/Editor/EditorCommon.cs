using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Takuzu.Generator
{
    public static class EditorCommon
    {
        private static Color32 oddItemColor;
        public static Color32 OddItemColor
        {
            get
            {
                return new Color32(180, 180, 180, 255);
            }
        }

        private static Color32 evenItemColor;
        public static Color32 EvenItemColor
        {
            get
            {
                return new Color32(170, 170, 170, 255);
            }
        }

        private static Color32 selectedItemColor;
        public static Color32 SelectedItemColor
        {
            get
            {
                return new Color32(0, 180, 255, 255);
            }
        }

        private static float hoverItemColorMultiplier;
        private static float HoverItemColorMultiplier
        {
            get
            {
                return 1.2f;
            }
        }

        private static GUIStyle centeredBoldLabel;
        public static GUIStyle CenteredBoldLabel
        {
            get
            {
                if (centeredBoldLabel == null)
                {
                    centeredBoldLabel = new GUIStyle();
                    centeredBoldLabel.alignment = TextAnchor.MiddleCenter;
                    centeredBoldLabel.fontStyle = FontStyle.Bold;
                    centeredBoldLabel.margin = new RectOffset(5, 5, 2, 2);
                    centeredBoldLabel.stretchWidth = true;
                }
                return centeredBoldLabel;
            }
        }

        private static GUIStyle boldLabel;
        public static GUIStyle BoldLabel
        {
            get
            {
                if (boldLabel == null)
                {
                    boldLabel = new GUIStyle();
                    boldLabel.alignment = TextAnchor.MiddleLeft;
                    boldLabel.fontStyle = FontStyle.Bold;
                    boldLabel.margin = new RectOffset(5, 5, 2, 2);
                    boldLabel.stretchWidth = true;
                }
                return boldLabel;
            }
        }

        private static GUIStyle italicLabel;
        public static GUIStyle ItalicLabel
        {
            get
            {
                if (italicLabel == null)
                {
                    italicLabel = new GUIStyle();
                    italicLabel.fontStyle = FontStyle.Italic;
                    italicLabel.stretchWidth = true;
                }
                return italicLabel;
            }
        }

        private static GUIStyle oddItemStyle;
        public static GUIStyle OddItemStyle
        {
            get
            {
                if (oddItemStyle == null)
                {
                    oddItemStyle = new GUIStyle();
                    oddItemStyle.alignment = TextAnchor.MiddleCenter;

                    Texture2D background = new Texture2D(1, 1);
                    background.SetPixels(new Color[1] { (Color)OddItemColor });
                    background.Apply();

                    Texture2D hoverBackground = new Texture2D(1, 1);
                    hoverBackground.SetPixels(new Color[1] { (Color)OddItemColor * HoverItemColorMultiplier });
                    hoverBackground.Apply();

                    oddItemStyle.normal.background = background;
                    oddItemStyle.hover.background = hoverBackground;
                }
                return oddItemStyle;
            }
        }

        private static GUIStyle evenItemStyle;
        public static GUIStyle EvenItemStyle
        {
            get
            {
                if (evenItemStyle == null)
                {
                    evenItemStyle = new GUIStyle();
                    evenItemStyle.alignment = TextAnchor.MiddleCenter;

                    Texture2D background = new Texture2D(1, 1);
                    background.SetPixels(new Color[1] { (Color)EvenItemColor });
                    background.Apply();

                    Texture2D hoverBackground = new Texture2D(1, 1);
                    hoverBackground.SetPixels(new Color[1] { (Color)EvenItemColor * HoverItemColorMultiplier });
                    hoverBackground.Apply();

                    evenItemStyle.normal.background = background;
                    evenItemStyle.hover.background = hoverBackground;
                }
                return evenItemStyle;
            }
        }

        private static GUIStyle selectedItemStyle;
        public static GUIStyle SelectedItemStyle
        {
            get
            {
                if (selectedItemStyle == null)
                {
                    selectedItemStyle = new GUIStyle();
                    selectedItemStyle.alignment = TextAnchor.MiddleCenter;

                    Texture2D background = new Texture2D(1, 1);
                    background.SetPixels(new Color[1] { (Color)SelectedItemColor });
                    background.Apply();

                    Texture2D hoverBackground = new Texture2D(1, 1);
                    hoverBackground.SetPixels(new Color[1] { (Color)SelectedItemColor * HoverItemColorMultiplier });
                    hoverBackground.Apply();

                    selectedItemStyle.normal.background = background;
                    selectedItemStyle.hover.background = hoverBackground;
                }
                return selectedItemStyle;
            }
        }

        public static void DrawSeparator()
        {
            Rect separatorRect = EditorGUILayout.GetControlRect();
            Rect r = new Rect();
            r.position = new Vector2(separatorRect.position.x, separatorRect.position.y + separatorRect.size.y / 2);
            r.size = new Vector2(separatorRect.size.x, 1);
            EditorGUI.DrawRect(r, Color.gray);
        }

        public static void BrowseDatabase(ref string result)
        {
            string selectedFile = EditorUtility.OpenFilePanelWithFilters("Select database file...", Application.dataPath, new string[2] { "Database (.db)", "db" });
            if (!string.IsNullOrEmpty(selectedFile))
                result = selectedFile;
        }

        public static void CreateNewDatabase(ref string result)
        {
            string selectedFile = EditorUtility.SaveFilePanelInProject("New database", "New database", "db", "");
            if (!string.IsNullOrEmpty(selectedFile))
            {
                Data.CreateDatabase(selectedFile);
                result = selectedFile;
                AssetDatabase.Refresh();
            }
        }

        public static void BrowseFolder(ref string result)
        {
            string selectedFolder = EditorUtility.OpenFolderPanel("Select folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedFolder))
                result = selectedFolder;
        }
    }
}