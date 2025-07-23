using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Takuzu
{
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance { get; private set; }
        public static Action<Vector2> onScreenResolutionChanged = delegate { };

        public float minAspect = 0.45f;
        public float maxAspect = 1.0f;

        [SerializeField]
        private Vector2 resolution;
        public Vector2 Resolution
        {
            get
            {
                return resolution;
            }
            private set
            {
                Vector2 oldValue = resolution;
                Vector2 newValue = value;
                resolution = newValue;
                if (oldValue != newValue)
                    onScreenResolutionChanged(newValue);
            }
        }

        public bool IsAspectRatioSupported
        {
            get
            {
                float ratio = Resolution.x / Resolution.y;
                return minAspect < ratio && ratio <= maxAspect;
            }
        }

        public float AspectRatio
        {
            get
            {
                return Resolution.x / Resolution.y;
            }
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private ScreenManagerHelper helper;
        public ScreenManagerHelper Helper
        {
            get
            {
                if (helper == null)
                {
                    helper = gameObject.AddComponent<ScreenManagerHelper>();
                }
                return helper;
            }
        }

        private void Update()
        {
            Resolution = new Vector2(Screen.width, Screen.height);
            Helper.enabled = !IsAspectRatioSupported;
        }
    }
}