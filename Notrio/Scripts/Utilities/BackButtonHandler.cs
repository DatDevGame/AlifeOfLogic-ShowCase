using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Takuzu
{
    public class BackButtonHandler : MonoBehaviour
    {
        bool canSkipTutorial = false;
        void Awake()
        {
            canSkipTutorial = PlayerPrefs.HasKey(PlayerDb.FINISH_TUTORIAL_KEY);
        }

#if UNITY_ANDROID && EASY_MOBILE
        void Update()
        {
            // Exit on Android Back button
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (LoadingScreen.Instance != null && LoadingScreen.Instance.IsShowing)
                    return;

                if (UIGuide.instance != null && (UIGuide.instance.isShownGuide || UIGuide.instance.isWaitingGuide))
                    return;

                if (!TryHidePanel())
                {
                    if (GameManager.Instance.GameState == GameState.Playing)
                    {
                        HandlePlayingState();
                        return;
                    }

                    if (GameManager.Instance.GameState == GameState.Prepare)
                    {
                        HandlePrepareState();
                        return;
                    }

                    if (GameManager.Instance.GameState == GameState.GameOver)
                    {
                        HandleGameOverState();
                        return;
                    }
                }
            }
        }

        private bool TryHidePanel()
        {
            Transform lastChild = UIReferences.Instance.overlayCanvas.transform.GetChild(UIReferences.Instance.overlayCanvas.transform.childCount - 1);
            OverlayPanel panel = lastChild.GetComponent<OverlayPanel>();

            if (UIReferences.Instance.subscriptionDetailPanel != null && UIReferences.Instance.subscriptionDetailPanel.IsShowing)
            {
                UIReferences.Instance.subscriptionDetailPanel.Hide();
                return true;
            }

            if (panel == null || !panel.IsShowing)
            {
                return false;
            }
            else
            {
                if (panel is ConfirmPolicyPanelController)
                    return true;

                if (panel is MatchingPanelController)
                {
                    panel.GetComponent<MatchingPanelController>().DeclineHandle();
                    return true;
                }

                if (panel is WinMenu)
                    return false;

                if (!(panel is TaskPanel) && !(panel is RewardDetailPanel) && !(panel is TutorialCompletePanel))
                {
                    panel.Hide();
                }

                if (panel is PauseMenu)
                {
                    GameManager.Instance.StartGame();
                }

                return true;
            }
        }

        private void HandlePlayingState()
        {
            if (UIReferences.Instance == null)
                return;
            if (UIReferences.Instance.overlayPauseMenu == null)
                return;

            GameManager.Instance.PauseGame();
            UIReferences.Instance.overlayPauseMenu.Show();
        }

        private void HandleGameOverState()
        {
            if (UIReferences.Instance == null)
                return;

            if (UIReferences.Instance.overlayWinMenu == null)
                return;

            if (UIReferences.Instance.overlayWinMenu.IsShowing)
                UIReferences.Instance.overlayWinMenu.GoBackHandle();
        }

        private void HandlePrepareState()
        {
            if (UIReferences.Instance == null)
                return;

            if (SceneManager.GetActiveScene().name.Equals(SceneLoadingManager.Instance.mainSceneName))
            {
                if (SceneManager.GetActiveScene().name.Equals(SceneLoadingManager.Instance.mainSceneName))
                {
                    if (UIReferences.Instance.overlayConfirmDialog == null)
                        return;

                    UIReferences.Instance.overlayConfirmDialog.Show(
                        I2.Loc.ScriptLocalization.EXIT_GAME_TITLE,
                        I2.Loc.ScriptLocalization.EXIT_GAME_DESCRIPTION,
                        () =>
                        {
                            Quit();
                        });
                    return;
                }
            }

            if (SceneManager.GetActiveScene().name.Equals(SceneLoadingManager.Instance.tournamentSceneName))
            {
                if (SceneLoadingManager.Instance == null)
                    return;

                SceneLoadingManager.Instance.LoadMainScene();
                return;
            }

            if (SceneManager.GetActiveScene().name.Equals(SceneLoadingManager.Instance.multiplayerScene))
            {
                if (SceneLoadingManager.Instance == null)
                    return;

                SceneLoadingManager.Instance.LoadMainScene();
                return;
            }

            if (SceneManager.GetActiveScene().name.Equals(SceneLoadingManager.Instance.tutorialSceneName))
            {
                if (TutorialManager4.Instance == null || !canSkipTutorial || TutorialManager4.Instance.tutorialComplatePanel.IsShowing)
                    return;
                TutorialManager4.Instance.Skip();
                return;
            }
        }

        public void Quit()
        {
#if !UNITY_EDITOR
            Application.Quit();
#else
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
#endif
    }
}