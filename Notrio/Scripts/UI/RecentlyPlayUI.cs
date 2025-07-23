using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class RecentlyPlayUI : MonoBehaviour
    {
        public Button recentButton;
        public CanvasGroup recentButtonGroup;

        private void Start()
        {
            recentButton.onClick.AddListener(delegate
            {
                PuzzleManager.currentIsRecent = true;
                GameManager.Instance.PlayAPuzzle(PuzzleManager.RecentlyPlayId);
            });
        }

        public bool HasRecentPuzzle()
        {
            return !string.IsNullOrEmpty(PuzzleManager.RecentlyPlayId);
        }

        private void Update()
        {
            recentButtonGroup.interactable = HasRecentPuzzle();
        }
    }
}