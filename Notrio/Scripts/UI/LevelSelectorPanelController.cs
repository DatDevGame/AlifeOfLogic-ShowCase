using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu.Generator;
using Pinwheel;

namespace Takuzu
{
    public class LevelSelectorPanelController : OverlayPanel
    {
        public UiGroupController controller;
        public int maxLevelCount = 50;
        public SnappingScroller scroller;
        public GameObject levelSelectorTemplate;
        public GameObject levelSelectorRootTemplate;
        public Text title;
        public Image titleBackground;
        public Button size6x6Button;
        public Button size8x8Button;
        public Button size10x10Button;
        public Button size12x12Button;
        public Button closeButton;

        public Color highlightColor;
        public Color unHighlightColor;
        public Image size6x6ButtonBackground;
        public Image size8x8ButtonBackground;
        public Image size10x10ButtonBackground;
        public Image size12x12ButtonBackground;
        public RectTransform circle;
        public float circleSpeed;
        public Vector2[] circlePositions;
        public AnimController loadingBar;
        public GameObject scrollerContent;
        public SwipeHandler swipeHandler;

        public int maxSelectorPerRoot;
        public int elementCountPerDisplayLoop;
        public float displayLoopInterval;
        public List<LevelSelector> selectorPool;

        private PuzzlePack pack;
        private PuzzlePack lastPack;
        private Size size;
        private int sizeIndex;


        public override void Show()
        {
            controller.ShowIfNot();
            IsShowing = true;
            transform.BringToFront();
            HighlightSelectorStatusIfNeeded();
            onPanelStateChanged(this, true);
        }

        public override void Hide()
        {
            StopAllCoroutines();
            controller.HideIfNot();
            IsShowing = false;
            onPanelStateChanged(this, false);
        }

        private void HighlightSelectorStatusIfNeeded()
        {
            for (int i = 0; i < selectorPool.Count; ++i)
            {
                selectorPool[i].HighlightStatusIfNeeded();
            }
        }

        private void OnDestroy()
        {
            PuzzleManager.onPuzzleSelected -= OnPuzzleSelected;
            GameManager.GameStateChanged -= OnGameStateChanged;
            swipeHandler.onSwipe -= OnFooterSwiped;
        }

        private void Awake()
        {
            PuzzleManager.onPuzzleSelected += OnPuzzleSelected;
            GameManager.GameStateChanged += OnGameStateChanged;

            swipeHandler.onSwipe += OnFooterSwiped;

            closeButton.onClick.AddListener(delegate
            {
                Hide();
            });

            size6x6Button.onClick.AddListener(delegate
            {
                size = Size.Six;
                DisplayPack();
                sizeIndex = 0;
            });

            size8x8Button.onClick.AddListener(delegate
            {
                size = Size.Eight;
                DisplayPack();
                sizeIndex = 1;
            });

            size10x10Button.onClick.AddListener(delegate
            {
                size = Size.Ten;
                DisplayPack();
                sizeIndex = 2;
            });

            size12x12Button.onClick.AddListener(delegate
            {
                size = Size.Twelve;
                DisplayPack();
                sizeIndex = 3;
            });
        }

        /// <summary>
        /// Default size is 6x6
        /// </summary>
        public void DisplayPackWithDefaultPuzzleSizeIfCurrentPackChanged()
        {
            if (pack == lastPack)
            {
                DisplayPack();
            }
            else
            {
                size = Size.Six;
                DisplayPack();
                sizeIndex = 0;
            }
        }

        private void Update()
        {
            if (circle.anchoredPosition != circlePositions[sizeIndex])
            {
                circle.anchoredPosition = Vector2.Lerp(circle.anchoredPosition, circlePositions[sizeIndex], circleSpeed * Time.deltaTime);
                if (Mathf.Abs(circle.anchoredPosition.x - circlePositions[sizeIndex].x) <= 1f)
                {
                    circle.anchoredPosition = circlePositions[sizeIndex];
                }
            }
        }

        private void OnFooterSwiped(Vector2 delta)
        {
            int des = delta.x > 0 ? 3 : delta.x < 0 ? 0 : sizeIndex;
            if (sizeIndex != des)
            {
                sizeIndex = (int)Mathf.MoveTowards(sizeIndex, des, 1);
                if (sizeIndex == 0)
                    size6x6Button.onClick.Invoke();
                else if (sizeIndex == 1)
                    size8x8Button.onClick.Invoke();
                else if (sizeIndex == 2)
                    size10x10Button.onClick.Invoke();
                else if (sizeIndex == 3)
                    size12x12Button.onClick.Invoke();
            }
        }

        public void SetPack(PuzzlePack pack)
        {
            lastPack = this.pack;
            this.pack = pack;
            if (title != null)
            {
                string titleText = pack != null ? pack.packName.ToUpper() : "";
                title.text = titleText;
            }
        }

        public void DisplayPack()
        {
            StopAllCoroutines();

            if (pack == null)
                return;
            if (size == Size.Unknown)
            {
                size = Size.Six;
                sizeIndex = 0;
            }
            Show();
            StartCoroutine(CrDisplayPack());
        }

        private IEnumerator CrDisplayPack(bool resetScroller = true)
        {
            scrollerContent.SetActive(false);
            loadingBar.gameObject.SetActive(true);
            loadingBar.Play();
            ClearElement();
            yield return null;
            if (resetScroller)
            {
                scroller.ResetScrollPos();
                scroller.SnapIndex = 0;
            }
            int count = PuzzleManager.CountPuzzleOfPack(pack, size);
            yield return null;
            int tmp = 0;
            for (int i = 0; i < count; ++i)
            {
                LevelSelector selector;
                if (i >= selectorPool.Count)
                {
                    //GameObject g = Instantiate(levelSelectorTemplate);
                    //selector = g.GetComponent<LevelSelector>();
                    //selectorPool.Add(selector);
                    continue;
                }
                else
                {
                    selector = selectorPool[i];
                }
                //AddElement(selector.gameObject);
                selectorPool[i].SetPuzzle(PuzzleManager.Instance.GetPuzzleId(pack, size, i));
                selectorPool[i].gameObject.SetActive(true);
                tmp += 1;
                //if (tmp >= elementCountPerDisplayLoop)
                //{
                //    tmp = 0;
                //    yield return new WaitForSeconds(displayLoopInterval);
                //}
                yield return null;
            }

            scrollerContent.SetActive(true);
            loadingBar.Stop();
        }

        public void ClearElement()
        {
            for (int i = 0; i < selectorPool.Count; ++i)
            {
                selectorPool[i].gameObject.SetActive(false);
                selectorPool[i].Reset();
            }
        }

        private void OnPuzzleSelected(string id, string puzzle, string solution, string progress)
        {
            if (IsShowing)
            {
                Hide();
            }
            else
            //if (GameManager.Instance != null && GameManager.Instance.GameState == GameState.GameOver)//tap on Next button of WinMenu
            {
                PuzzlePack pack;
                Size s;
                int offset;
                if (!PuzzleManager.Instance.SplitPuzzleId(id, out pack, out s, out offset))
                    return;
                SetPack(pack);
                size = s;
                if (s == Size.Six)
                {
                    sizeIndex = 0;
                    StartCoroutine(CrDisplayPack(false));
                }
                else if (s == Size.Eight)
                {
                    sizeIndex = 1;
                    StartCoroutine(CrDisplayPack(false));
                }
                else if (s == Size.Ten)
                {
                    sizeIndex = 2;
                    StartCoroutine(CrDisplayPack(false));
                }
                else if (s == Size.Twelve)
                {
                    sizeIndex = 3;
                    StartCoroutine(CrDisplayPack(false));
                }


                scroller.SnapIndex = offset / maxSelectorPerRoot;
            }
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Prepare && (oldState == GameState.Paused || oldState == GameState.GameOver) &&
                !PuzzleManager.currentIsChallenge) //&&
                                                   //PuzzleManager.currentIsRecent == false)
            {
                //CoroutineHelper.Instance.DoActionDelay(Show, 0);
            }
        }

#if UNITY_EDITOR
        GUIStyle style;
        private void OnDrawGizmos()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                return;
            if (Camera.current != null && Vector3.Distance(Camera.current.transform.position, swipeHandler.transform.position) < 100)
            {
                if (style == null)
                {
                    style = new GUIStyle(UnityEditor.EditorStyles.miniLabel);
                }

                style.normal.textColor = Color.magenta;
                style.alignment = TextAnchor.MiddleCenter;
                UnityEditor.Handles.Label(swipeHandler.transform.position, "<Parent canvas should be in Overlay mode\nfor these button to work with clicks>", style);
            }
        }
#endif
    }
}