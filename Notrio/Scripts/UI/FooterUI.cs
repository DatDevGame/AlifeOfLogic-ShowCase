using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyMobile;
using UnityEngine.SceneManagement;
using System;

namespace Takuzu
{
    public class FooterUI : MonoBehaviour
    {
        public Button settingButton;
		[HideInInspector]
        public SettingPanel settingMenu;
        public Button leaderboardButton;
        public LeaderboardController leaderboard;
        public UiGroupController controller;
        public float showDelay;
		private void Awake() {
			if(UIReferences.Instance!=null){
				UpdateReferences();
			}
			UIReferences.UiReferencesUpdated += UpdateReferences;
		}

		private void UpdateReferences()
		{
			settingMenu = UIReferences.Instance.overlaySettingPanel;
		}

		private void OnDestroy()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
			UIReferences.UiReferencesUpdated -= UpdateReferences;
        }

        private void Start()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
            settingButton.onClick.AddListener(delegate
            {
                settingMenu.Show();
            });

            leaderboardButton.onClick.AddListener(delegate
            {
                leaderboard.Show();
                leaderboard.callingSource = null;
            });

            controller.ShowIfNot();
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing)
            {
                controller.HideIfNot();
            }
            else if (newState == GameState.Prepare)
            {
                CoroutineHelper.Instance.DoActionDelay(
                    () =>
                    {
                        if (GameManager.Instance.GameState == GameState.Prepare)
                            controller.ShowIfNot();
                    },
                    showDelay);
            }
        }
    }
}