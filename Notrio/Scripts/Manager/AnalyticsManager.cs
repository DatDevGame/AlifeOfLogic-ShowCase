using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace Takuzu
{
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }

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

        private void OnEnable()
        {
            /*
            PuzzleManager.onPackUnlocked += OnPackUnlocked;
            PuzzleManager.onPuzzleSelected += OnPuzzleSelected;
            Judger.onJudgingCompleted += OnJudgingCompleted;
            CloudServiceManager.onLoginGameSpark += OnLoginGameSpark;
            GameManager.GameStateChanged += OnGameStateChanged;
            CloudServiceManager.onSubmitSolvingTimeCancelled += OnSubmitSolvingTimeCancelled;
            */
        }

        private void OnDisable()
        {
            /*
            PuzzleManager.onPackUnlocked -= OnPackUnlocked;
            PuzzleManager.onPuzzleSelected -= OnPuzzleSelected;
            Judger.onJudgingCompleted -= OnJudgingCompleted;
            CloudServiceManager.onLoginGameSpark -= OnLoginGameSpark;
            GameManager.GameStateChanged -= OnGameStateChanged;
            CloudServiceManager.onSubmitSolvingTimeCancelled -= OnSubmitSolvingTimeCancelled;
            */
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Prepare && (oldState == GameState.Paused || oldState == GameState.GameOver))
            {
                int flag = Judger.Instance.flagCount;
                int reveal = Judger.Instance.revealCount;
                int reset = Judger.Instance.resetCount;
                int undo = Judger.Instance.undoCount;
                bool hasUsedPowerup = flag > 0 || reveal > 0 || reset > 0 || undo > 0;
                if (hasUsedPowerup)
                {
                    SendCustomEvent("powerup-usage", new Dictionary<string, object>()
                    {
                        { "puzzle-id", PuzzleManager.currentPuzzleId },
                        { "flag", flag },
                        { "reveal", reveal },
                        { "reset", reset },
                        { "undo", undo }
                    });
                }
            }
        }

        public AnalyticsResult SendCustomEvent(string eventName)
        {
            print(string.Format("Analytic: send custom event '{0}'", eventName));
            return Analytics.CustomEvent(eventName);
        }

        public AnalyticsResult SendCustomEvent(string eventName, IDictionary<string, object> eventData)
        {
            print(string.Format("Analytic: send custom event '{0}'", eventName));
            return Analytics.CustomEvent(eventName, eventData);
        }

        private void OnPackUnlocked(PuzzlePack pack)
        {
            SendCustomEvent("unlock-pack", new Dictionary<string, object>()
            {
                { "pack", pack.packName }
            });
        }

        private void OnJudgingCompleted(Judger.JudgingResult result)
        {
            SendCustomEvent("solve", new Dictionary<string, object>()
            {
                { "puzzle-id", PuzzleManager.currentPuzzleId },
                { "solving-time", result.solvingTime }
            });
        }

        private void OnLoginGameSpark(GameSparks.Api.Responses.AuthenticationResponse response)
        {
            if (!response.HasErrors && response.NewPlayer == true)
            {
                SendCustomEvent("user-connect-with-facebook");
            }
        }

        private void OnPuzzleSelected(string id, string puzzleStr, string solutionStr, string progressStr)
        {
            string key = PuzzleManager.PROGRESS_PREFIX + id;
            if (!PlayerDb.HasKey(key))
            {
                SendCustomEvent("play-puzzle", new Dictionary<string, object>()
                {
                    { "puzzle-id", id }
                });
            }
        }

        private void OnSubmitSolvingTimeCancelled(float remainingMinutes)
        {
            SendCustomEvent("submit-solving-time-cancelled", new Dictionary<string, object>()
            {
                { "remaining-minutes", remainingMinutes }
            });
        }
    }
}