using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pinwheel
{
    [AddComponentMenu("Pinwheel/Animate/Color Animation")]
    public class ColorAnimation : ProceduralAnimation
    {
        public ColorAnimationType type;
        public Gradient[] gradients;
        public bool multiply;
        private Color baseColor;

        private SpriteRenderer spriteRenderer;
        private MeshRenderer meshRenderer;
        private Graphic graphic;
        private CanvasGroup group;
        private System.Action<Color> blendAction;

        public void Reset()
        {
            multiply = false;
            duration = 0.3f;
            gradients = new Gradient[1] { new Gradient() };

            if (GetComponent<SpriteRenderer>() != null)
                type = ColorAnimationType.Sprite;
            else if (GetComponent<MeshRenderer>() != null)
                type = ColorAnimationType.Mesh;
            else if (GetComponent<Graphic>() != null)
                type = ColorAnimationType.UI;
        }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (type == ColorAnimationType.Sprite)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                baseColor = spriteRenderer.color;
                blendAction += BlendSpriteRenderer;
            }
            else if (type == ColorAnimationType.Mesh)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                baseColor = meshRenderer.sharedMaterial.color;
                blendAction += BlendMeshRenderer;
            }
            else if (type == ColorAnimationType.UI)
            {
                graphic = GetComponent<Graphic>();
                baseColor = graphic.color;
                blendAction += BlendGraphicColor;
            }
            else if (type == ColorAnimationType.CanvasGroup)
            {
                group = GetComponent<CanvasGroup>();
                baseColor = new Color(1, 1, 1, group.alpha);
                blendAction += BlendCanvasGroupAlpha;
            }
        }

        public override void Play(int gradientIndex)
        {
            StopAllCoroutines();
            Gradient g = gradients[gradientIndex];
            StartCoroutine(CrPlay(g, duration, multiply));
        }

        public void Play(int gradientIndex, float duration)
        {
            StopAllCoroutines();
            Gradient g = gradients[gradientIndex];
            StartCoroutine(CrPlay(g, duration, multiply));
        }

        public void Play(int gradientIndex, float duration, bool multiply)
        {
            StopAllCoroutines();
            Gradient g = gradients[gradientIndex];
            StartCoroutine(CrPlay(g, duration, multiply));
        }

        public void Play(Gradient g)
        {
            StopAllCoroutines();
            StartCoroutine(CrPlay(g, duration, multiply));
        }

        public void Play(Gradient g, float duration)
        {
            StopAllCoroutines();
            StartCoroutine(CrPlay(g, duration, multiply));
        }

        public void Play(Gradient g, float duration, bool multiply)
        {
            StopAllCoroutines();
            StartCoroutine(CrPlay(g, duration, multiply));
        }

        private IEnumerator CrPlay(Gradient g, float duration, bool multiply)
        {
            Color c;
            Color multiplier = multiply ? baseColor : Color.white;
            float time = 0;
            while (time <= duration)
            {
                c = g.Evaluate(time / duration);
                blendAction(c * multiplier);
                if (time == duration)
                    break;
                time = Mathf.MoveTowards(time, duration, Time.smoothDeltaTime);
                yield return null;
            }
        }

        private void BlendSpriteRenderer(Color c)
        {
            spriteRenderer.color = c;
        }

        private void BlendMeshRenderer(Color c)
        {
            meshRenderer.sharedMaterial.color = c;
        }

        private void BlendGraphicColor(Color c)
        {
            graphic.color = c;
        }

        private void BlendCanvasGroupAlpha(Color c)
        {
            group.alpha = c.a;
        }
    }

    public enum ColorAnimationType
    {
        Sprite, Mesh, UI, CanvasGroup
    }
}