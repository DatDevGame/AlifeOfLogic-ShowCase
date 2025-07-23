using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    public abstract class OverlayPanel: MonoBehaviour
    {
        public static System.Action<OverlayPanel, bool> onPanelStateChanged = delegate { };
        public bool hasSelfDarkenImage;
        public Vector4 recommendDarkenImageBorder = new Vector4(300, 100, 300, 100);
        public bool IsShowing { get; set; }
        public abstract void Show();
        public abstract void Hide();
    }
}