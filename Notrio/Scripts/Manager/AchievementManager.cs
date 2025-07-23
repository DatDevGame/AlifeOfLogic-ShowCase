using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Takuzu.Achievements
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }
        public static Action<AchievementInfo> onNewAchievementUnlocked = delegate { };

        [SerializeField]
        private List<AchievementInfo> achievements;

        public const string ACHIEVEMENT_UNLOCK_PREFIX = "ACHIEVED";

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Prepare)
            {
                UpdateAchievements();
            }
        }

        public static void UnlockAchievement(AchievementInfo a)
        {
            if (!IsAchievementUnlocked(a))
                onNewAchievementUnlocked(a);

            string key = string.Format("{0}-{1}", ACHIEVEMENT_UNLOCK_PREFIX, a.ID);
            PlayerDb.SetBool(key, true);
        }

        public static bool IsAchievementUnlocked(AchievementInfo a)
        {
            string key = string.Format("{0}-{1}", ACHIEVEMENT_UNLOCK_PREFIX, a.ID);
            return PlayerDb.GetBool(key, false);
        }

        public static void UpdateAchievements()
        {
            Instance.StartCoroutine(Instance.CrUpdateAchievements());
        }

        public IEnumerator CrUpdateAchievements()
        {
            yield return null;

            for (int i = 0; i < achievements.Count; ++i)
            {
                if (!achievements[i].IsUse)
                    continue;

                if (achievements[i].IsCompleted)
                    UnlockAchievement(achievements[i]);

                yield return null;
            }
        }
    }
}
