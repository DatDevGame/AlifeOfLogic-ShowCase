using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScalerHelper : MonoBehaviour
    {
        public const float tallLayoutThreshold = 0.525f;
        public static readonly Vector2 tallLayoutReferfenceResolution = new Vector2(700, 1440);
        public static readonly Vector2 normalLayoutReferenceResolution = new Vector2(640, 1136);
        public static Vector2 currentReferenceResolution = new Vector2(640, 1136);

        public CanvasScaler scaler;

        private void Update()
        {
            if (ScreenManager.Instance == null)
                return;
            Vector2 res = ScreenManager.Instance.Resolution;
            float a = res.x / res.y;
            if (a <= tallLayoutThreshold)
            {
                currentReferenceResolution = tallLayoutReferfenceResolution;
            }
            else
            {
                currentReferenceResolution = normalLayoutReferenceResolution;
            }
            scaler.referenceResolution = currentReferenceResolution;
        }
    }
}