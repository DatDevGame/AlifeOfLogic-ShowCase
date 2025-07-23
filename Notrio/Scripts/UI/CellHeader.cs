using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellHeader : MonoBehaviour {

    public TextMesh text;
    private ColorController colorController;

    public void SetColorController(ColorController controller)
    {
        colorController = controller;
    }

    private void Update()
    {
        if (colorController != null)
            text.color = colorController.color;
    }
}
