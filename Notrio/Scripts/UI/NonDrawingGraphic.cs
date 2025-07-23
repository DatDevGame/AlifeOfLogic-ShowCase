using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Essentially, an UI element can block graphic raycast if it has a Graphic (Image, RawImage, Text, etc.) component attached.
/// But in some case, we just don't want it to be drawn onto the screen to reduce overdraw (even if you set it alpha to 0, it still be drawn)
/// To do that, attach this component to a UI element and check on raycastTarget, don't bother on the Color and Material properties, they do nothing.
/// From the original post: https://answers.unity3d.com/questions/1091618/ui-panel-without-image-component-as-raycast-target.html
/// </summary>
public class NonDrawingGraphic : Graphic
{
    public override void SetMaterialDirty() { return; }
    public override void SetVerticesDirty() { return; }

    /// Probably not necessary since the chain of calls `Rebuild()`->`UpdateGeometry()`->`DoMeshGeneration()`->`OnPopulateMesh()` won't happen; so here really just as a fail-safe.
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        return;
    }
}

