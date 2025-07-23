using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class CharacterFacialAnim : MonoBehaviour
    {
        [System.Serializable]
        public struct Element
        {
            public string name;
            public RectTransform rt;
            public float multiplier;
        }

        public bool dontLookYet;
        public Vector2 canvasSize = new Vector2(640, 1136);
        public Vector2 headAnchor = new Vector2(0.5f, 1);
        public float speed;
        public float resetLookDelay;
        public List<Element> elements;

        private Vector2 headCanvasPos;
        private Vector2 lookDir;
        private Coroutine resetLookDirCoroutine;

        public const int EARS = 0;
        public const int FACE = 1;
        public const int HAIR = 2;
        public const int GLASSES_NOSE_MOUTH = 3;
        public const int EYES = 4;

        private void Awake()
        {
            headCanvasPos = new Vector2(canvasSize.x * headAnchor.x, canvasSize.y * headAnchor.y);
        }

        private void Update()
        {
            if (Input.GetMouseButton(0) && !dontLookYet)
            {
                Vector2 viewportPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                viewportPoint.x = Mathf.Clamp01(viewportPoint.x);
                viewportPoint.y = Mathf.Clamp01(viewportPoint.y);
                Vector2 mouseCanvasPos = new Vector2(viewportPoint.x * canvasSize.x, viewportPoint.y * canvasSize.y);
                lookDir = (mouseCanvasPos - headCanvasPos);
                if (resetLookDirCoroutine != null)
                {
                    StopCoroutine(resetLookDirCoroutine);
                    resetLookDirCoroutine = null;
                }
            }
            else
            {
                if (resetLookDirCoroutine == null)
                    resetLookDirCoroutine = StartCoroutine(CrResetLookDir());
            }

            LookAtDirection(lookDir);
        }

        private void LookAtDirection(Vector2 dir)
        {
            for (int i = 0; i < elements.Count; ++i)
            {
                elements[i].rt.anchoredPosition = Vector3.Lerp(elements[i].rt.anchoredPosition, dir * elements[i].multiplier, speed * Time.deltaTime);
            }
        }

        private IEnumerator CrResetLookDir()
        {
            yield return new WaitForSeconds(resetLookDelay);
            lookDir = Vector2.zero;
            resetLookDirCoroutine = null;
        }

        public void SetElementGraphic(int index, Sprite s)
        {
            elements[index].rt.GetComponent<Image>().sprite = s;
        }
    }
}