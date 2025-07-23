using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace Takuzu
{
    public class AndroidBackButtonHandler : MonoBehaviour
    {
        public Transform overlayCanvas;
        public PauseMenu pauseMenu;

        [Header("Exit Confirmation Dialog")]
        public string title = "Exit Game";
        public string message = "Are you sure you want to exit?";
        public string yesButton = "Yes";
        public string noButton = "No";

        private void Awake()
        {
            yesButton = I2.Loc.ScriptLocalization.YES.ToUpper();
            noButton = I2.Loc.ScriptLocalization.NO.ToUpper();
            message = I2.Loc.ScriptLocalization.EXIT_GAME_DESCRIPTION;
            title = I2.Loc.ScriptLocalization.EXIT_GAME_TITLE;

        }

#if UNITY_ANDROID && EASY_MOBILE
        void Update()
        {
            // Exit on Android Back button
            if (Input.GetKeyUp(KeyCode.Escape))
            {   
                if (!TryHidePanel())
                {
                    if (GameManager.Instance.GameState == GameState.Playing)
                    {
                        pauseMenu.Show();
                        GameManager.Instance.PauseGame();
                    }
                    else if (GameManager.Instance.GameState == GameState.Prepare)
                    {
                        ShowDialog();
                    }
                }
            }
        }

        private bool TryHidePanel()
        {
            Transform lastChild = overlayCanvas.GetChild(overlayCanvas.childCount - 1);
            OverlayPanel panel = lastChild.GetComponent<OverlayPanel>();
            if (panel==null || !panel.IsShowing)
            {
                return false;
            }
            else
            {
                panel.Hide();
                if (panel is PauseMenu)
                {
                    GameManager.Instance.StartGame();
                }
                return true;                
            }
        }

        private void ShowDialog()
        {
            NativeUI.AlertPopup alert = NativeUI.ShowTwoButtonAlert(
                                              title,
                                              message,
                                              yesButton,
                                              noButton
                                          );

            if (alert != null)
            {
                alert.OnComplete += (int button) =>
                {
                    switch (button)
                    {
                        case 0: // Yes
                            Application.Quit();
                            break;
                        case 1: // No
                            break;
                    }
                };
            }
        }
#endif
    }
}