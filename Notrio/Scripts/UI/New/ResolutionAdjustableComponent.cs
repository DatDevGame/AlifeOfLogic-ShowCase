using System;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    /// <summary>
    /// This component changes a specific component  in order to fit 
    /// with weird screen resolution (IphoneX, Note 8++...)
    /// </summary>
    public abstract class ResolutionAdjustableComponent<T, U> : MonoBehaviour
    {
        [SerializeField, Tooltip("The component you want to change.")]
        private T targetComponent;

        [SerializeField, Range(0.1f, 10f), Tooltip("The component will only be changed \n when the the screen rate (height / width) bigger than this value.")]
        private float limitRate = 2f;

        [SerializeField, Tooltip("This value will be used to adjust the component.")]
        private U adjustValue;

        protected virtual void Awake()
        {
            if (targetComponent == null)
                return;

            /// Note that if you click another view panel in the editor,
            /// Unity will change the screen size to that panel size so it's not the device resolution anymore.
            #if UNITY_EDITOR
            float screenRate = 1f / Camera.main.aspect;
            #else
            float screenRate = (float)Screen.height / Screen.width;
            #endif

            if (screenRate < limitRate)
                return;

            AdjustComponent(targetComponent, adjustValue);
        }

        protected abstract void AdjustComponent(T targetComponent, U adjustValue);

        // sonpt: what's this for?
        //        protected virtual void Update()
        //        {
        //            float screenRate = (float)Screen.height / Screen.width;
        //        }
    }
}
