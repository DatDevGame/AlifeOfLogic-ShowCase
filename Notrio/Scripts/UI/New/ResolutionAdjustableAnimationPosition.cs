using System;
using System.Collections.Generic;
using Pinwheel;
using UnityEngine;

namespace Takuzu
{
    /// <summary>
    /// This component changes the PositionAnimation in order to fit 
    /// with weird screen resolution (IphoneX, Note 8++...)
    /// </summary>
    public class ResolutionAdjustableAnimationPosition : ResolutionAdjustableComponent<PositionAnimation, float>
    {
        protected override void AdjustComponent(PositionAnimation targetComponent, float adjustValue)
        {
            /// Add value to each Keyframe in the PositionAnimation component.
            foreach (CurveTuple curveTuple in targetComponent.curves)
            {
                for (int i = 0; i < curveTuple.y.keys.Length; i++)
                {
                    // The Keyframe's value can't be changed,
                    // so we have to remove the old keyframe and add the new one with new value.
                    Keyframe newKeyframe = new Keyframe(curveTuple.y.keys[i].time, curveTuple.y.keys[i].value + adjustValue);
                    curveTuple.y.RemoveKey(i);
                    curveTuple.y.AddKey(newKeyframe);
                }
            }
        }
    }
}
