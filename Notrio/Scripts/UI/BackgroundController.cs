using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    public class BackgroundController : MonoBehaviour
    {
        public SnappingScroller scroller;
        [Range(0f, 0.5f)]
        public float fadeThreshold;
        public float minZoom;
        public float maxZoom;
        public SpriteRenderer playingBgRenderer;
        public SpriteRenderer menuBgRenderer;

        private List<Sprite> menuBgs;

        public Sprite tournamentSprite;
        public List<string> menuBgNames;
        public List<string> ingameBgNames;

        private int index1, index2;
        private float lerpIndex;

        private void Awake()
        {
            OverlayPanel.onPanelStateChanged += OnPanelStateChanged;
            OndemandResourceLoader.LoadAssetsBundle("textures", -1);
        }

        private void OnDestroy()
        {
            OverlayPanel.onPanelStateChanged -= OnPanelStateChanged;

            UnloadMenuBackgrounds();
        }

        private void OnPanelStateChanged(OverlayPanel p, bool isShow)
        {
            if (typeof(LevelUpPanel) == p.GetType())
            {
                if (isShow)
                {
                    CoroutineHelper.Instance.DoActionDelay(() =>
                    {
                        playingBgRenderer.enabled = false;
                    }, 0.25f);
                }
                else
                {
                    playingBgRenderer.enabled = true;
                }
            }
        }

        private void Start()
        {
            index1 = -1;
            index2 = -1;
        }

        private void LoadMenuBackgrounds()
        {
            menuBgs = new List<Sprite>();
            for (int i = 0; i < menuBgNames.Count; ++i)
            {
                menuBgs.Add(Background.Get(menuBgNames[i]));
            }
        }

        private void UnloadMenuBackgrounds()
        {
            for (int i = 0; i < menuBgNames.Count; ++i)
            {
                Background.Unload(menuBgNames[i]);
            }
            menuBgs = null;
        }

        private void Update()
        {
            if (GameManager.Instance.GameState == GameState.Prepare)
            {
                menuBgRenderer.gameObject.SetActive(true);
                menuBgRenderer.color = Vector4.MoveTowards(menuBgRenderer.color, Color.white, Time.deltaTime * 2);

                if (playingBgRenderer.color != Color.clear)
                {
                    playingBgRenderer.color = Vector4.MoveTowards(playingBgRenderer.color, Color.clear, Time.deltaTime * 2);
                }
                else if (playingBgRenderer.gameObject.activeInHierarchy)
                {
                    playingBgRenderer.gameObject.SetActive(false);
                }

                if (menuBgs == null || menuBgs.Count == 0)
                {
                    LoadMenuBackgrounds();
                }

                if (scroller != null)
                {
                    lerpIndex = scroller.RelativeNormalizedScrollPos * scroller.ElementCount;
                    lerpIndex = Mathf.Clamp(lerpIndex, 0, menuBgNames.Count - 1);
                    index1 = Mathf.FloorToInt(lerpIndex);
                    index2 = Mathf.CeilToInt(lerpIndex);
                    index1 = Mathf.Clamp(index1, 0, menuBgNames.Count - 1);
                    index2 = Mathf.Clamp(index2, 0, menuBgNames.Count - 1);
                    menuBgRenderer.sprite = menuBgs[index1];
                    MaterialPropertyBlock p = new MaterialPropertyBlock();
                    menuBgRenderer.GetPropertyBlock(p);
                    p.SetTexture("_MainTex", menuBgs[index1].texture);
                    p.SetTexture("_SecondaryTex", menuBgs[index2].texture);
                    p.SetFloat("_BlendFraction", MapAlpha(menuBgNames[index1] != menuBgNames[index2] ? 1 - (index2 - lerpIndex) : 1));
                    p.SetFloat("_MainScale", MapZoom(menuBgNames[index1] != menuBgNames[index2] ? 1 - (lerpIndex - index1) : 1));
                    p.SetFloat("_SecondaryScale", MapZoom(menuBgNames[index1] != menuBgNames[index2] ? 1 - (index2 - lerpIndex) : 1));
                    menuBgRenderer.SetPropertyBlock(p);
                }
                else
                {
                    menuBgRenderer.sprite = menuBgs[0];
                    MaterialPropertyBlock p = new MaterialPropertyBlock();
                    menuBgRenderer.GetPropertyBlock(p);
                    p.SetTexture("_MainTex", menuBgs[0].texture);
                    menuBgRenderer.SetPropertyBlock(p);
                }
            }
            else
            {
                playingBgRenderer.gameObject.SetActive(true);
                Color c = playingBgRenderer.color;
                playingBgRenderer.color = Vector4.MoveTowards(playingBgRenderer.color, Color.white, Time.deltaTime * 2);

                if (menuBgRenderer.color != Color.clear)
                {
                    menuBgRenderer.color = Vector4.MoveTowards(menuBgRenderer.color, Color.clear, Time.deltaTime * 2);
                }
                else if (menuBgRenderer.gameObject.activeInHierarchy)
                {
                    menuBgRenderer.gameObject.SetActive(false);
                    UnloadMenuBackgrounds();
                }
            }
        }

        private float MapAlpha(float f)
        {
            float min = 0.5f - fadeThreshold;
            float max = 0.5f + fadeThreshold;

            return Mathf.Clamp01((f - min) / (max - min));
        }

        private float MapZoom(float f)
        {
            return Mathf.Lerp(minZoom, maxZoom, f);
        }

#if UNITY_EDITOR
        GUIStyle style;
        private void OnDrawGizmos()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                return;
            if (Camera.current != null && Vector3.Distance(Camera.current.transform.position, menuBgRenderer.transform.position) < 100)
            {
                if (style == null)
                {
                    style = new GUIStyle(UnityEditor.EditorStyles.miniLabel);
                }

                style.normal.textColor = Color.cyan;
                style.alignment = TextAnchor.MiddleCenter;
                UnityEditor.Handles.Label(menuBgRenderer.transform.position, "<Background image\nis assigned\nat runtime>", style);
            }
        }
#endif
    }
}