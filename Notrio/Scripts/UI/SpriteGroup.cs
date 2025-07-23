using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    public class SpriteGroup : MonoBehaviour
    {
        [SerializeField]
        private Color color;

        [SerializeField]
        private bool isEnabled;

        private SpriteRenderer[] renderers;

        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
#if UNITY_EDITOR
                GetRenderers();
#endif
                if (renderers != null)
                {
                    for (int i = 0; i < renderers.Length; ++i)
                    {
                        MaterialPropertyBlock p = new MaterialPropertyBlock();
                        renderers[i].GetPropertyBlock(p);
                        p.SetColor("_Color", color);
                        renderers[i].SetPropertyBlock(p);
                    }
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
#if UNITY_EDITOR
                GetRenderers();
#endif
                if (renderers != null)
                {
                    for (int i = 0; i < renderers.Length; ++i)
                    {
                        renderers[i].enabled = isEnabled;
                    }
                }
            }

        }

        private void Reset()
        {
            Color = Color.white;
            IsEnabled = true;
        }

        private void OnValidate()
        {
            Color = color;
            IsEnabled = isEnabled;
        }

        public void Awake()
        {
            GetRenderers();
        }

        private void GetRenderers()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
    }
}