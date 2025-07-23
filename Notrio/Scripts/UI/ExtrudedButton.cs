using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

namespace Takuzu
{
    public class ExtrudedButton : Button
    {
        public float extrusion = 4;
        public RectTransform content;
        public Vector2 contentUnpressPosition;
        private Vector2 contentPressedPosition;
        public Text label;
        public Color labelEnableColor = Color.white;
        public Color labelDisableColor = Color.grey;
        public bool overrideMaterialOnInteractableFalse;
        public Material imgOverrideMaterial;
        public Material textOverrideMaterial;
        public Image[] imgToOverride;
        public Text[] textToOverride;

#if UNITY_EDITOR
        protected override void Reset()
        {
            transition = Transition.SpriteSwap;
            string pressedSpritePath = "Assets/Notrio/Sprites/Other/button-pressed.psd";
            string normalSpritePath = "Assets/Notrio/Sprites/Other/button-normal.psd";
            SpriteState tmp = spriteState;
            tmp.pressedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(pressedSpritePath);
            spriteState = tmp;

            Image img = GetComponent<Image>();
            if (img != null)
            {
                targetGraphic = img;
                img.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(normalSpritePath);
            }
            extrusion = 4;
            content = transform.GetChild(0) as RectTransform;
            if (content != null)
            {
                contentUnpressPosition = content.anchoredPosition;
            }
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            if (content != null)
            {
                contentPressedPosition = content.anchoredPosition + Vector2.down * extrusion;
            }
        }

        private void Update()
        {
            if (content != null)
            {
                content.anchoredPosition = IsPressed() && interactable ? contentPressedPosition : contentUnpressPosition;
            }

            if (label != null)
            {
                label.color = interactable ? labelEnableColor : labelDisableColor;
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            imgToOverride = imgToOverride.Where(i => i != null).ToArray();
            textToOverride = textToOverride.Where(i => i != null).ToArray();
            if (!overrideMaterialOnInteractableFalse)
                return;
            if (state == SelectionState.Disabled)
            {
                for (int i = 0; i < imgToOverride.Length; ++i)
                {
                    imgToOverride[i].material = imgOverrideMaterial;
                }
                for (int i = 0; i < textToOverride.Length; ++i)
                {
                    textToOverride[i].material = textOverrideMaterial;
                }
            }
            else
            {
                for (int i = 0; i < imgToOverride.Length; ++i)
                {
                    imgToOverride[i].material = null;
                }
                for (int i=0;i<textToOverride.Length;++i)
                {
                    textToOverride[i].material = null;
                }
            }
        }
    }
}