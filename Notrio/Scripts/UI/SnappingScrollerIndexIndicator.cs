using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class SnappingScrollerIndexIndicator : MonoBehaviour
    {
        public enum HighLightMode
        {
            Current, FromBeginning
        }

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
                highlightColor = _color;
                unHighlightColor = new Color(_color.r, _color.g, _color.b, 0.5f * _color.a);
            }
        }

        private void OnValidate()
        {
            color = _color;
        }

        public SnappingScroller scroller;
        public GameObject indicatorTemplate;
        public Color highlightColor;
        public Vector3 highlightScale;
        public Color unHighlightColor;
        public Vector3 unhighlightScale;
        public Sprite highlightSprite;
        public Sprite unHighlightSprite;
        public HighLightMode mode;
        public List<Image> indicator;

        public float colorBlendSpeed = 1;
        public float scalingSpeed = 1;


        private void Awake()
        {
            indicator = new List<Image>();
        }

        private void Update()
        {
            indicator.RemoveAll((g) => { return g == null; });
            if (indicator.Count < scroller.ElementCount)
            {
                CreateIndicator(scroller.ElementCount - indicator.Count);
            }
            else if (indicator.Count > scroller.ElementCount)
            {
                RemoveIndicator(indicator.Count - scroller.ElementCount);
            }

            if (scroller.SnapIndex >= 0 && scroller.SnapIndex < indicator.Count)
            {
                for (int i = 0; i < indicator.Count; ++i)
                {
                    if (mode == HighLightMode.Current)
                    {
                        Color targetColor = i == scroller.SnapIndex ? highlightColor : unHighlightColor;
                        indicator[i].color = Vector4.MoveTowards(indicator[i].color, targetColor, colorBlendSpeed * Time.smoothDeltaTime);

                        Vector3 targetScale = i == scroller.SnapIndex ? highlightScale : unhighlightScale;
                        indicator[i].rectTransform.localScale = Vector3.MoveTowards(indicator[i].rectTransform.localScale, targetScale, scalingSpeed * Time.smoothDeltaTime);

                        indicator[i].sprite = i == scroller.SnapIndex ? highlightSprite : unHighlightSprite;
                    }
                    else
                    {
                        Color targetColor = i <= scroller.SnapIndex ? highlightColor : unHighlightColor;
                        indicator[i].color = Vector4.MoveTowards(indicator[i].color, targetColor, colorBlendSpeed * Time.smoothDeltaTime);

                        Vector3 targetScale = i <= scroller.SnapIndex ? highlightScale : unhighlightScale;
                        indicator[i].rectTransform.localScale = Vector3.MoveTowards(indicator[i].rectTransform.localScale, targetScale, scalingSpeed * Time.smoothDeltaTime);

                        indicator[i].sprite = i <= scroller.SnapIndex ? highlightSprite : unHighlightSprite;
                    }
                }
            }
        }

        private void CreateIndicator(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                GameObject g = Instantiate(indicatorTemplate);
                g.transform.SetParent(transform, false);
                Image graphic = g.GetComponent<Image>();
                indicator.Add(graphic);
            }
        }

        private void RemoveIndicator(int count)
        {
            int c = transform.childCount;
            for (int i = 0; i < Mathf.Min(c, count); ++i)
            {
                Destroy(transform.GetChild(c - 1 - i).gameObject);
                indicator.RemoveAt(c - 1 - i);
            }

        }
    }
}