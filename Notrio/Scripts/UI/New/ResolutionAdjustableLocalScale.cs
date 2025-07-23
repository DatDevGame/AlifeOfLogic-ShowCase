using System;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    /// <summary>
    /// This component changes the header animation position in order to fit 
    /// with weird screen resolution (IphoneX, Note 8++...)
    /// </summary>
    public class ResolutionAdjustableLocalScale : ResolutionAdjustableComponent<Transform, Vector3>
    {
        protected override void AdjustComponent(Transform targetComponent, Vector3 adjustValue)
        {
            targetComponent.localScale = new Vector3(targetComponent.localScale.x + adjustValue.x,
                                                     targetComponent.localScale.y + adjustValue.y,
                                                     targetComponent.localScale.z + adjustValue.z);
        }
    }
}
