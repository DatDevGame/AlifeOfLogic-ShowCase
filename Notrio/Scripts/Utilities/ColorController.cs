using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorController : MonoBehaviour {

    public System.Action<ColorController, Color> onColorChanged = delegate { };

    [SerializeField]
    private Color _color;
#pragma warning disable IDE1006 // Naming Styles
    public Color color //dont rename, to use with reflection (DayNightAdapter)
#pragma warning restore IDE1006 // Naming Styles
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            onColorChanged(this, _color);
        }
    }

    private void OnValidate()
    {
        color = _color;   
    }
}
