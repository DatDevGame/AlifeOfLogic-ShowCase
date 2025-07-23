using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class CoinFlyingEffect : MonoBehaviour
    {
        public Vector2 startPoint;
        public Vector2 endPoint;
        public float movementSpeed;
        public float maxAmplitude;
        public float sampleStep;
        public AnimationCurve atten;
        public bool destroyOnFinish;
        public float scaleUpMultiplier = 2f;
        public float scaleSpeed = 3;
        public float fadeSpeed = 3;

        private Vector2 normal;
        private float seed;
        private RectTransform rt;
        private Graphic graphic;
        private float sampleValue;
        private float sampleX;
        Vector2 linearPos;
        float f;
        float offset;
        

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            graphic = GetComponent<Graphic>();
            seed = Random.Range(-1000.0f, 1000.0f);
            UpdateNormal();
            sampleX = seed;
            f = 0;
        }

        private void Update()
        {
            linearPos = Vector2.Lerp(startPoint, endPoint, f);
            sampleValue = Mathf.PerlinNoise(sampleX, 0.0123f) * 2 - 1;
            offset = atten.Evaluate(f) * sampleValue * maxAmplitude;
            rt.anchoredPosition = linearPos + normal * offset;

            f = Mathf.MoveTowards(f, 1, movementSpeed * Time.smoothDeltaTime);
            sampleX += sampleStep;

            if (f == 1)
            {
                transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * scaleUpMultiplier, scaleSpeed * Time.smoothDeltaTime);
                Color c = graphic.color;
                c.a = 0;
                graphic.color = Vector4.MoveTowards(graphic.color, c, fadeSpeed * Time.smoothDeltaTime);

                if (transform.localScale == Vector3.one * scaleUpMultiplier && graphic.color.a == 0)
                    Destroy(gameObject);
            }
        }

        public void UpdateNormal()
        {
            Vector2 dir = (endPoint - startPoint).normalized;
            normal = Vector3.Cross(dir, Vector3.forward);
        }
    }
}