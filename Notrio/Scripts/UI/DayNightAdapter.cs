using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace Takuzu
{
    public class DayNightAdapter : MonoBehaviour
    {
        public enum DayNightAdapterAction
        {
            ChangeColor,
            ChangeSprite,
            ChangeColorAndSprite,
            BlendSprite,
            BlendColor,
            None
        }

        private static Material blendSpriteMaterial;

        public static Material BlendSpriteMaterial
        {
            get
            {
                if (blendSpriteMaterial == null)
                {
                    blendSpriteMaterial = new Material(Shader.Find("SgLib/BlendedSprite"))
                    {
                        enableInstancing = true
                    };
                }
                return blendSpriteMaterial;
            }
        }

        public Component target;
        public DayNightAdapterAction action;

        [Space]
        public Color dayColor;
        public Color nightColor;

        [Space]
        public Sprite daySprite;
        public Sprite nightSprite;

        [Space]
        public float blendSpeed;

        private void Reset()
        {
            target = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            PersonalizeManager.onNightModeChanged += OnNightModeChanged;

            if (action == DayNightAdapterAction.BlendSprite)
            {
                Renderer r = target as Renderer;
                r.sharedMaterial = BlendSpriteMaterial;
                MaterialPropertyBlock p = new MaterialPropertyBlock();
                r.GetPropertyBlock(p);
                p.SetTexture("_MainTex", daySprite.texture);
                p.SetTexture("_SecondaryTex", nightSprite.texture);
                r.SetPropertyBlock(p);
            }
        }

        private void OnDestroy()
        {
            PersonalizeManager.onNightModeChanged -= OnNightModeChanged;
        }

        private void OnEnable()
        {
            Adapt();
        }

        private void OnNightModeChanged(bool enable)
        {
            Adapt();
        }

        public void Adapt()
        {
            StopAllCoroutines();
            if (action == DayNightAdapterAction.ChangeColor)
            {
                ChangeColor();
            }
            else if (action == DayNightAdapterAction.ChangeSprite)
            {
                ChangedSprite();
            }
            else if (action == DayNightAdapterAction.ChangeColorAndSprite)
            {
                ChangeColor();
                ChangedSprite();
            }
            else if (action == DayNightAdapterAction.BlendSprite)
            {
                BlendSprite();
            }
            else if (action == DayNightAdapterAction.BlendColor)
            {
                BlendColor();
            }
        }

        private void ChangedSprite()
        {
            PropertyInfo targetSprite = target.GetType().GetProperty("sprite");
            if (targetSprite != null && targetSprite.CanWrite)
            {
                Sprite s = PersonalizeManager.NightModeEnable ? nightSprite : daySprite;
                targetSprite.SetValue(target, s, null);
            }
        }

        private void ChangeColor()
        {
            PropertyInfo targetColor = target.GetType().GetProperty("color");
            if (targetColor != null && targetColor.CanWrite)
            {
                Color c = PersonalizeManager.NightModeEnable ? nightColor : dayColor;
                targetColor.SetValue(target, c, null);
            }
        }

        private void BlendSprite()
        {
            if (!gameObject.activeInHierarchy)
            {
                ChangedSprite();
                return;
            }

            StartCoroutine(CrBlendSprite());
        }

        private IEnumerator CrBlendSprite()
        {
            Renderer r = target as Renderer;
            //float time = 0;
            MaterialPropertyBlock p = new MaterialPropertyBlock();
            r.GetPropertyBlock(p);
            float f = p.GetFloat("_BlendFraction");
            float d = PersonalizeManager.NightModeEnable ? 1 : 0;
            if(blendSpeed <= 0)
            {
                f = d;
                p.SetFloat("_BlendFraction", f);
                r.SetPropertyBlock(p);
            }
            while (f != d)
            {
                f = Mathf.MoveTowards(f, d, blendSpeed * Time.smoothDeltaTime);
                p.SetFloat("_BlendFraction", f);
                r.SetPropertyBlock(p);
                yield return null;
            }
        }

        private void BlendColor()
        {
            StartCoroutine(CrBlendColor());
        }

        private IEnumerator CrBlendColor()
        {
            PropertyInfo colorProps = target.GetType().GetProperty("color");
            if (colorProps != null && colorProps.CanWrite)
            {
                Vector4 d = PersonalizeManager.NightModeEnable ? nightColor : dayColor;
                Vector4 f = (Color)colorProps.GetValue(target, null);
                if (blendSpeed <= 0)
                {
                    f = d;
                    colorProps.SetValue(target, (Color)f, null);
                }
                while (f != d)
                {
                    f = Vector4.MoveTowards(f, d, blendSpeed * Time.deltaTime);
                    colorProps.SetValue(target, (Color)f, null);
                    yield return null;
                }
            }
            yield return null;
        }
    }
}
