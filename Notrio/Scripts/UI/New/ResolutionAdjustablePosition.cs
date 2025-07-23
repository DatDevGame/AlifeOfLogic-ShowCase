using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Takuzu
{
    /// <summary>
    /// This component changes the GameObject original position in order to fit 
    /// with weird screen resolution (IphoneX, Note 8++...)
    /// </summary>
    public class ResolutionAdjustablePosition : ResolutionAdjustableComponent<Transform, float>
    {
        protected override void AdjustComponent(Transform targetComponent, float adjustValue)
        {
            /// Add value to the target's position.
            StartCoroutine(CR_Adjust(targetComponent, adjustValue));
        }

        IEnumerator CR_Adjust(Transform targetComponent, float adjustValue)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            targetComponent.localPosition = new Vector3(targetComponent.localPosition.x, targetComponent.localPosition.y + adjustValue, targetComponent.localPosition.z);
        }
    }
}
