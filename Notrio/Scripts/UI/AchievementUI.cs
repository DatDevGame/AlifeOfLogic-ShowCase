using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu.Achievements
{
    public class AchievementUI : MonoBehaviour
    {
		[HideInInspector]
        public OverlayUIController overlayUIController;
		[HideInInspector]
        public AchievementPanel panel;

        Queue<AchievementInfo> achievements;
		private void Awake() {
			if(UIReferences.Instance!=null){
				UpdateReferences();
			}
			UIReferences.UiReferencesUpdated += UpdateReferences;
		}

		private void UpdateReferences()
		{
			overlayUIController = UIReferences.Instance.overlayUIController;
			panel = UIReferences.Instance.overlayAchievementPanel;
		}

		private void Start()
        {
            AchievementManager.onNewAchievementUnlocked += OnNewAchievementUnlocked;
            OverlayPanel.onPanelStateChanged += OnPanelStateChanged;
        }

        private void OnDestroy()
        {
            AchievementManager.onNewAchievementUnlocked -= OnNewAchievementUnlocked;
            OverlayPanel.onPanelStateChanged -= OnPanelStateChanged;
			UIReferences.UiReferencesUpdated -= UpdateReferences;
        }

        private void OnNewAchievementUnlocked(AchievementInfo a)
        {
            if (achievements == null)
                achievements = new Queue<AchievementInfo>();

            achievements.Enqueue(a);
            WillShowPanel();
        }

        private void OnPanelStateChanged(OverlayPanel p, bool isShow)
        {
            if (p is AchievementPanel && !isShow)
            {
                WillShowPanel();
            }
        }

        private void WillShowPanel()
        {
            CoroutineHelper.Instance.PostponeActionUntil(
               () =>
               {
                   if (achievements.Count > 0 && !panel.IsShowing)
                   {
                       AchievementInfo a = achievements.Dequeue();
                       print("dequeue");
                       panel.SetAchievementInfo(a);
                       panel.Show();
                   }
               },

               () =>
               {
                   return overlayUIController.ShowingPanelCount == 0;
               });
        }
    }
}