using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameSparks.Core;
using Pinwheel;

namespace Takuzu
{
    public class LeaderboardController : OverlayPanel
    {
        public LeaderboardBuilder builder;
        public OverlayGroupController controller;
        public string lbType;
        public int entryCountPerLoad;
        public int loadMoreOffset;
        public Button individualButton;
        public Button friendButton;
        public Button countryButton;
        public Image individualBg;
        public Image individualIcon;
        public Image friendBg;
        public Image friendIcon;
        public Image countryBg;
        public Image countryIcon;
        public Button loginButton;
        public Button expButton;
        public Button dailyButton;
        public Button weeklyButton;
        public Button closeButton;
        public ListView listView;
        public AnimController loadingAnim;
        public RectTransform circle;
        public float circleSpeed;
        public Vector2[] circlePositions;
        public SwipeHandler swipeHandler;

        public static float refreshThresholdMinutes = 1;

        [Space]
        public Color buttonBgHighlightColor;
        public Color buttonIconHighlightColor;
        [Space]
        public Color buttonBgUnHighlightColor;
        public Color buttonIconUnHighlightColor;

        private int lastToIndex;
        private int typeIndex;
        [HideInInspector]
        public float deactivateTime;
        [HideInInspector]
        public OverlayPanel callingSource;

        public const int EXP_LB_INDEX = 0;
        public const int DAILY_LB_INDEX = 2;
        public const int WEEKLY_LB_INDEX = 1;

        public override void Show()
        {
            if (!TryLoadDefaultLb())
            {
                TryReloadLb();
            }
            controller.ShowIfNot();
            IsShowing = true;
            transform.BringToFront();
            onPanelStateChanged(this, true);
        }

        public void Show(int lbIndex)
        {
            SelectLb(lbIndex);
            Show();
        }

        public override void Hide()
        {
            controller.HideIfNot();
            IsShowing = false;
            onPanelStateChanged(this, false);
            deactivateTime = Time.time;
            if (callingSource != null)
                callingSource.Show();
        }

        private void OnEnable()
        {
            swipeHandler.onSwipe += OnFooterSwiped;
            listView.onReachLastElement += OnReachLastElement;
            CloudServiceManager.onLoginGameSpark += OnLoginGameSpark;
        }

        public bool TryLoadDefaultLb()
        {
            if (string.IsNullOrEmpty(builder.lbShortCode))
            {
                builder.ClearEntries();
                builder.SetupParameters(lbType, LeaderboardBuilder.GROUP_BY_INDIVIDUAL);
                builder.RequestLeaderboardData(entryCountPerLoad);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryReloadLb()
        {
            if (listView.DataCount == 0)
            {
                Reload();
                return true;
            }

            if ((Time.time - deactivateTime) / 60 > refreshThresholdMinutes)
            {
                Reload();
                return true;
            }
            else
            {
                if (SocialManager.Instance.IsLoggedInFb && builder.currentPlayerEntryRoot.transform.childCount == 0)
                {
                    builder.RequestCurrentPlayerRank();
                    return true;
                }
            }
            return false;
        }

        public void Reload()
        {
            builder.ClearEntries();
            builder.SetupParameters(builder.TYPE_CURRENT, builder.GROUP_CURRENT);
            builder.RequestLeaderboardData(entryCountPerLoad);
        }

        private void OnDisable()
        {
            swipeHandler.onSwipe -= OnFooterSwiped;
            listView.onReachLastElement -= OnReachLastElement;
            CloudServiceManager.onLoginGameSpark -= OnLoginGameSpark;
        }

        private void Start()
        {
            UnHighlightButton();
            individualBg.color = buttonBgHighlightColor;
            individualIcon.color = buttonIconHighlightColor;

            loginButton.onClick.AddListener(delegate
            {
            });

            individualButton.onClick.AddListener(delegate
            {
                UnHighlightButton();
                individualBg.color = buttonBgHighlightColor;
                individualIcon.color = buttonIconHighlightColor;
                builder.ClearEntries();
                builder.SetupParameters(lbType, LeaderboardBuilder.GROUP_BY_INDIVIDUAL);
                builder.RequestLeaderboardData(entryCountPerLoad);
                DeactivateButtonSeconds();
            });

            friendButton.onClick.AddListener(delegate
            {
                UnHighlightButton();
                friendBg.color = buttonBgHighlightColor;
                friendIcon.color = buttonIconHighlightColor;
                builder.ClearEntries();
                builder.SetupParameters(lbType, LeaderboardBuilder.GROUP_BY_FRIENDS);
                builder.RequestLeaderboardData(entryCountPerLoad);
                DeactivateButtonSeconds();
            });

            countryButton.onClick.AddListener(delegate
            {
                UnHighlightButton();
                countryBg.color = buttonBgHighlightColor;
                countryIcon.color = buttonIconHighlightColor;
                builder.ClearEntries();
                builder.SetupParameters(lbType, LeaderboardBuilder.GROUP_BY_COUNTRY);
                builder.RequestLeaderboardData(entryCountPerLoad);
                DeactivateButtonSeconds();
            });

            expButton.onClick.AddListener(delegate
            {
                SelectExpLb();
                DeactivateButtonSeconds();
            });

            dailyButton.onClick.AddListener(delegate
            {
                SelectDailyLb();
                DeactivateButtonSeconds();
            });

            weeklyButton.onClick.AddListener(delegate
            {
                SelectWeeklyLb();
                DeactivateButtonSeconds();
            });

            closeButton.onClick.AddListener(delegate
            {
                Hide();
            });

        }

        private void DeactivateButtonSeconds(float s = 1)
        {
            individualButton.interactable = false;
            friendButton.interactable = false;
            countryButton.interactable = false;
            expButton.interactable = false;
            dailyButton.interactable = false;
            weeklyButton.interactable = false;

            CoroutineHelper.Instance.DoActionDelay(
                () =>
                {
                    individualButton.interactable = true;
                    friendButton.interactable = true;
                    countryButton.interactable = true;
                    expButton.interactable = true;
                    dailyButton.interactable = true;
                    weeklyButton.interactable = true;
                }, s);
        }

        public void SelectLb(int lbIndex)
        {
            if (lbIndex == EXP_LB_INDEX)
                SelectExpLb();
            else if (lbIndex == DAILY_LB_INDEX)
                SelectDailyLb();
            else if (lbIndex == WEEKLY_LB_INDEX)
                SelectWeeklyLb();
        }

        private void SelectExpLb()
        {
            typeIndex = EXP_LB_INDEX;
            lbType = LeaderboardBuilder.TYPE_EXP;
            builder.ClearEntries();
            builder.SetupParameters(lbType, builder.GROUP_CURRENT);
            builder.RequestLeaderboardData(entryCountPerLoad);
        }

        private void SelectDailyLb()
        {
            typeIndex = DAILY_LB_INDEX;
            lbType = LeaderboardBuilder.TYPE_DAILY;
            builder.ClearEntries();
            builder.SetupParameters(lbType, builder.GROUP_CURRENT);
            builder.RequestLeaderboardData(entryCountPerLoad);
        }

        private void SelectWeeklyLb()
        {
            typeIndex = WEEKLY_LB_INDEX;
            lbType = LeaderboardBuilder.TYPE_WEEKLY;
            builder.ClearEntries();
            builder.SetupParameters(lbType, builder.GROUP_CURRENT);
            builder.RequestLeaderboardData(entryCountPerLoad);
        }

        private void OnFooterSwiped(Vector2 delta)
        {
            int des = delta.x > 0 ? 2 : delta.x < 0 ? 0 : typeIndex;
            if (typeIndex != des)
            {
                typeIndex = (int)Mathf.MoveTowards(typeIndex, des, 1);
                if (typeIndex == EXP_LB_INDEX)
                    expButton.onClick.Invoke();
                else if (typeIndex == DAILY_LB_INDEX)
                    dailyButton.onClick.Invoke();
                else if (typeIndex == WEEKLY_LB_INDEX)
                    weeklyButton.onClick.Invoke();
            }
        }

        private void OnLoginGameSpark(GameSparks.Api.Responses.AuthenticationResponse response)
        {
            if (response.HasErrors)
            {
                builder.CurrentPlayerEntryMessage = LeaderboardBuilder.NO_CONNECTION_MSG;
            }
            else
            {
                StartCoroutine(CrRefreshLeaderboardAfterLogin());
            }
        }

        private void OnReachLastElement()
        {
            //builder.RequestMoreEntry(entryCountPerLoad);
        }

        private void Update()
        {
            if (SocialManager.Instance.IsLoggedInFb)
            {
                loginButton.gameObject.SetActive(false);
            }
            else
            {
                loginButton.gameObject.SetActive(true);
                builder.ClearCurrentPlayerEntry();
            }

            if (listView.DataCount - lastToIndex <= loadMoreOffset &&
                lastToIndex < listView.toIndex &&
                !builder.isLoadingMore)
            {
                builder.RequestMoreEntry(entryCountPerLoad);
            }
            lastToIndex = listView.toIndex;

            if (builder.isLoadingCurrentPlayerEntry ||
                builder.isLoadingListviewFirstEntry ||
                builder.isLoadingMore ||
                CloudServiceManager.isLoggingIn)
            {
                if (!loadingAnim.isPlaying && loadingAnim.gameObject.activeInHierarchy)
                    loadingAnim.Play();
            }
            else
            {
                if (loadingAnim.isPlaying)
                    loadingAnim.Stop();
            }

            if (circle.anchoredPosition != circlePositions[typeIndex])
            {
                circle.anchoredPosition = Vector2.Lerp(circle.anchoredPosition, circlePositions[typeIndex], circleSpeed * Time.deltaTime);
                if (Mathf.Abs(circle.anchoredPosition.x - circlePositions[typeIndex].x) <= 1f)
                {
                    circle.anchoredPosition = circlePositions[typeIndex];
                }
            }

        }

        private void UnHighlightButton()
        {
            individualBg.color = buttonBgUnHighlightColor;
            friendBg.color = buttonBgUnHighlightColor;
            countryBg.color = buttonBgUnHighlightColor;

            individualIcon.color = buttonIconUnHighlightColor;
            friendIcon.color = buttonIconUnHighlightColor;
            countryIcon.color = buttonIconUnHighlightColor;
        }

        private IEnumerator CrRefreshLeaderboardAfterLogin()
        {
            yield return new WaitUntil(() =>
            {
                return GS.Authenticated &&
                !CloudServiceManager.isGuest &&
                CloudServiceManager.countryId != null;
            });
            if (listView.DataCount != 0)
            {
                builder.RequestCurrentPlayerRank();
            }
            else
            {
                builder.RequestLeaderboardData();
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