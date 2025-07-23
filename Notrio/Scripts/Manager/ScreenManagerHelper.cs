using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    public class ScreenManagerHelper : MonoBehaviour
    {
        GUIStyle style;
        Texture2D black;

        private void Start()
        {
            style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.wordWrap = true;
            style.fontSize = Mathf.RoundToInt(Screen.height * 0.15f);
            style.alignment = TextAnchor.MiddleCenter;
            black = new Texture2D(1, 1);
            black.SetPixels(new Color[1] { Color.black });
            black.Apply();
            
        }

        private void OnGUI()
        {
            style.fontSize = Mathf.RoundToInt(Screen.height * 0.15f);
            Rect r = new Rect(0, 0, Screen.width, Screen.height);
            GUIContent content = new GUIContent("Screen aspect ratio not supported!", black);
            GUI.DrawTexture(r, black);
            GUI.Label(r, content, style);
        }

        private void OnDestroy()
        {
            if (black != null)
                Destroy(black);
        }
    }
}