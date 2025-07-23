using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class OverlayUIController : MonoBehaviour
    {
        public static List<OverlayUIController> overlayUIControllerIntances = new List<OverlayUIController>();
        public bool pauseOnLooseFocusInEditor;
        public Canvas canvas;
        public UiGroupController darkenImage;
        public Image darkenImgComponent;
        public Texture2D darkenImgTexture;
        public GameObject[] panels;

        private List<OverlayPanel> overlayPanels;
        private List<RectTransform> rt;
        private Dictionary<Vector4, Sprite> darkenImgVersions;

        public int ShowingPanelCount
        {
            get
            {
                int count = overlayPanels.FindAll((p) => { return p.IsShowing; }).Count;
                return count;
            }
        }

        private void Awake()
        {
            overlayUIControllerIntances.Add(this);
            rt = new List<RectTransform>();
            overlayPanels = new List<OverlayPanel>();
            for (int i = 0; i < panels.Length; ++i)
            {
                OverlayPanel p = panels[i].GetComponentInChildren<OverlayPanel>(true);
                if (p != null)
                {
                    overlayPanels.Add(p);
                    rt.Add(panels[i].transform as RectTransform);
                }
            }

            darkenImgVersions = new Dictionary<Vector4, Sprite>();
            for (int i = 0; i < overlayPanels.Count; ++i)
            {
                if (!darkenImgVersions.ContainsKey(overlayPanels[i].recommendDarkenImageBorder))
                {
                    Sprite s = Sprite.Create(
                        darkenImgTexture,
                        new Rect(0, 0, darkenImgTexture.width, darkenImgTexture.height),
                        new Vector2(0.5f, 0.5f),
                        100,
                        0,
                        SpriteMeshType.FullRect,
                        overlayPanels[i].recommendDarkenImageBorder);
                    darkenImgVersions[overlayPanels[i].recommendDarkenImageBorder] = s;
                }
            }

            OverlayPanel.onPanelStateChanged += OnOverlayPanelChanged;
            GameManager.GameStateChanged += OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.GameOver) //changing render mode to record GIF
            {
                CloseAll();
                //canvas.renderMode = RenderMode.ScreenSpaceCamera;
                //canvas.worldCamera = Camera.main;
            }
            else
            {
                //canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }

        private void Start()
        {
            darkenImage.Hide();
        }

        private void OnDestroy()
        {
            OverlayPanel.onPanelStateChanged -= OnOverlayPanelChanged;
            GameManager.GameStateChanged -= OnGameStateChanged;
            overlayUIControllerIntances.Remove(this);
        }

        private void OnOverlayPanelChanged(OverlayPanel p, bool isShowing)
        {
            //the footer buttons of Level Selection panel and Leaderboard panel only work in overlay mode
            if ((p is LevelSelectorPanelController || p is LeaderboardController) &&
                isShowing)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }
            /*
            if (isShowing)
            {
                int i = overlayPanels.IndexOf(p);
                int panelIndex = overlayPanels[i].transform.GetSiblingIndex();
                int darkenImgIndex = panelIndex - (panelIndex == transform.childCount - 1 ? 1 : 0);
                //rt[i].BringToFront();
                if (!overlayPanels[i].hasSelfDarkenImage)
                {
                    darkenImage.transform.SetSiblingIndex(darkenImgIndex);
                    //darkenImage.gameObject.SetActive(true);
                    darkenImgComponent.sprite = darkenImgVersions[overlayPanels[i].recommendDarkenImageBorder];
                    if (darkenImage.isShowing == false)
                        darkenImage.Show();
                }
                else
                {
                    //darkenImage.transform.SendToBack();
                    //darkenImage.gameObject.SetActive(false);
                    if (darkenImage.isShowing == true)
                    {
                        darkenImage.Hide();
                        CoroutineHelper.Instance.DoActionDelay(() =>
                        {
                            darkenImage.transform.SendToBack();
                        }, darkenImage.maxDuration);
                    }
                }

                return;
            }
            else if(ShowingPanelCount > 0)
            {

            }
            */
            int maxSiblingIndex = -1;
            int maxPanelIndex = -1;
            for (int i = overlayPanels.Count - 1; i >= 0; --i)
            {
                if (overlayPanels[i].IsShowing)
                {
                    int panelIndex = overlayPanels[i].transform.GetSiblingIndex();
                    if (panelIndex > maxSiblingIndex)
                    {
                        maxSiblingIndex = panelIndex;
                        maxPanelIndex = i;
                    }
                }
            }
            if (maxSiblingIndex >= 0)
            {
                int darkenImgIndex = maxSiblingIndex - (maxSiblingIndex == transform.childCount - 1 ? 1 : 0);
                //rt[i].BringToFront();
                if (!overlayPanels[maxPanelIndex].hasSelfDarkenImage)
                {
                    darkenImage.transform.SetSiblingIndex(darkenImgIndex);
                    //darkenImage.gameObject.SetActive(true);
                    darkenImgComponent.sprite = darkenImgVersions[overlayPanels[maxPanelIndex].recommendDarkenImageBorder];
                    if (darkenImage.isShowing == false)
                        darkenImage.Show();
                }
                else
                {
                    //darkenImage.transform.SendToBack();
                    //darkenImage.gameObject.SetActive(false);
                    if (darkenImage.isShowing == true)
                    {
                        darkenImage.Hide();
                        CoroutineHelper.Instance.DoActionDelay(() =>
                        {
                            darkenImage.transform.SendToBack();
                        }, darkenImage.MaxDuration);
                    }
                }
                return;
            }

            darkenImage.transform.SendToBack();
            //darkenImage.gameObject.SetActive(false);
            if (darkenImage.isShowing == true)
                darkenImage.Hide();
        }

        public void ShowOnly(System.Type panelType)
        {
            for (int i = 0; i < overlayPanels.Count; ++i)
            {
                if (overlayPanels[i].GetType().Equals(panelType))
                {
                    if (!overlayPanels[i].IsShowing)
                        overlayPanels[i].Show();
                }
                else
                {
                    if (overlayPanels[i].IsShowing)
                        overlayPanels[i].Hide();
                }
            }
        }

        public void CloseAll()
        {
            for (int i = 0; i < overlayPanels.Count; ++i)
            {
                if (overlayPanels[i].IsShowing)
                    overlayPanels[i].Hide();
            }
        }
    }
}