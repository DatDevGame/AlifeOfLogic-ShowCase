using LionStudios.Suite.Analytics;
using System;
using System.Collections.Generic;
using Takuzu;
using Takuzu.Generator;
using UnityEngine;
using static StoryPuzzlesSaver;

public enum TutorialTypeAnalytic
{
    Start,
    Completed
}

public class AlolAnalytics : MonoBehaviour
{
    private static DateTime m_DateTimeStart;
    private static int m_CountRevealPerLevel = 0;
    private static int m_CountUndoPerLevel = 0;

    public static void IncreaseReveal(int amount = 1) => m_CountRevealPerLevel += amount;
    public static void IncreaseUndo(int amount = 1) => m_CountUndoPerLevel += amount;
    private static int GetBoosterUsed() => m_CountRevealPerLevel + m_CountUndoPerLevel;
    public static void MissionStarted(string puzzleID)
    {
        try
        {
            m_CountRevealPerLevel = 0;
            m_CountUndoPerLevel = 0;

            Puzzle puzzle = PuzzleManager.Instance.GetPuzzleById(puzzleID);
            int nodeIndex = GetIndexNode(puzzle.level, puzzle.size);
            SolvableStatus solvableStatus = Instance.ValidateLevel(nodeIndex);
            if (solvableStatus == SolvableStatus.Current)
            {
                string currentMileStone = String.Format("{0}.{1}", nodeIndex + 1,
                    Instance.GetMaxProgressInNode(nodeIndex) < Instance.ProgressRequiredToFinishNode(nodeIndex)
                    ? Instance.GetMaxProgressInNode(nodeIndex) + 1 : Instance.ProgressRequiredToFinishNode(nodeIndex));

                int currentLevel = Instance.GetCurrentLevel();
                bool isTutorial = false;
                string missionType = "main";
                string missionName = $"{missionType}_{currentLevel}";
                string missionID = $"{currentLevel}";
                int missionAttempt = GetMissionAttempt(currentLevel);

                //AdditionalData
                Dictionary<string, object> additionalData = new Dictionary<string, object>();
                Dictionary<string, int> gameplayData = new Dictionary<string, int>
                    {
                        { "target_score", 1}
                    };
                Dictionary<string, int> economyData = new Dictionary<string, int>
                    {
                        { "coin_balance", CoinManager.Instance.Coins}
                    };
                additionalData.Add("gameplay_data", gameplayData);
                additionalData.Add("economy_data", economyData);
                m_DateTimeStart = DateTime.Now;
                LionAnalytics.MissionStarted(isTutorial, missionType, missionName, missionID, missionAttempt, additionalData);
            }

            int GetMissionAttempt(int currentLevel)
            {
                int missionAttempt = PlayerPrefs.GetInt($"missionAttempt_{nodeIndex}-{currentLevel}", 1);
                PlayerPrefs.SetInt($"missionAttempt_{nodeIndex}-{currentLevel}", missionAttempt + 1);
                return missionAttempt;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    public static void MissionAbandoned(string puzzleID)
    {
        try
        {
            Puzzle puzzle = PuzzleManager.Instance.GetPuzzleById(puzzleID);
            int nodeIndex = GetIndexNode(puzzle.level, puzzle.size);
            int score = (int)LogicalBoard.Instance.GetAccurateCompletionPercentage();
            int currentLevel = Instance.GetCurrentLevel();

            bool isTutorial = false;
            string missionType = $"main";
            string missionName = $"{missionType}_{currentLevel}";
            string missionID = $"{currentLevel}";

            SolvableStatus solvableStatus = Instance.ValidateLevel(nodeIndex);
            if (solvableStatus == SolvableStatus.Current)
            {
                int missionAttempt = GetMissionAttempt(currentLevel);
                TimeSpan timeSpan = DateTime.Now - m_DateTimeStart;
                int totalSecondPlayed = (int)timeSpan.TotalSeconds + PlayerPrefs.GetInt($"Time_in_level-Abandoned-{nodeIndex}-{currentLevel}", 0);
                PlayerPrefs.SetInt($"Time_in_level-Abandoned-{nodeIndex}-{currentLevel}", totalSecondPlayed);
                //AdditionalData
                Dictionary<string, object> additionalData = new Dictionary<string, object>();

                // gameplay_data
                var gameplayData = new Dictionary<string, object>
                    {
                        { "time_in_level", totalSecondPlayed},
                        { "target_score", 1},
                        { "reached_score", score}
                    };

                // economy_data
                var economyData = new Dictionary<string, object>
                    {
                        { "coin_balance", CoinManager.Instance.Coins},
                        { "coin_spent", 0}
                    };

                // boosters_data
                var boostersData = new Dictionary<string, object>
                    {
                        { "boosters_used", GetBoosterUsed()}
                    };

                // monetization_data
                var monetizationData = new Dictionary<string, object>
                    {
                        { "rv_watched", 0}
                    };
                // Add all to the main dictionary
                additionalData.Add("gameplay_data", gameplayData);
                additionalData.Add("economy_data", economyData);
                additionalData.Add("boosters_data", boostersData);
                additionalData.Add("monetization_data", monetizationData);

                LionAnalytics.MissionAbandoned(isTutorial, missionType, missionName, missionID, score, missionAttempt, additionalData);
            }

            int GetMissionAttempt(int currentLevel)
            {
                int missionAttempt = PlayerPrefs.GetInt($"missionAttempt_{nodeIndex}-{currentLevel}", 1);
                return missionAttempt - 1;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

    }
    public static void MissionCompleted(string puzzleID, int timeSecondCompleted)
    {
        try
        {
            Puzzle puzzle = PuzzleManager.Instance.GetPuzzleById(puzzleID);
            int nodeIndex = GetIndexNode(puzzle.level, puzzle.size);
            int score = (int)LogicalBoard.Instance.GetAccurateCompletionPercentage();
            int currentLevel = Instance.GetCurrentLevel();

            bool isTutorial = false;
            string missionType = $"main";
            string missionName = $"{missionType}_{currentLevel}";
            string missionID = $"{currentLevel}";

            SolvableStatus solvableStatus = Instance.ValidateLevel(nodeIndex);
            if (solvableStatus == SolvableStatus.Current)
            {
                int missionAttempt = GetMissionAttempt(currentLevel);

                //AdditionalData
                Dictionary<string, object> additionalData = new Dictionary<string, object>();

                // gameplay_data
                var gameplayData = new Dictionary<string, object>
                    {
                        { "time_in_level", timeSecondCompleted},
                        { "target_score", 1},
                        { "reached_score", score}
                    };

                // economy_data
                var economyData = new Dictionary<string, object>
                    {
                        { "coin_balance", CoinManager.Instance.Coins},
                        { "coin_spent", 0}
                    };

                // boosters_data
                var boostersData = new Dictionary<string, object>
                    {
                        { "boosters_used", GetBoosterUsed()}
                    };

                // monetization_data
                var monetizationData = new Dictionary<string, object>
                    {
                        { "rv_watched", 0}
                    };
                // Add all to the main dictionary
                additionalData.Add("gameplay_data", gameplayData);
                additionalData.Add("economy_data", economyData);
                additionalData.Add("boosters_data", boostersData);
                additionalData.Add("monetization_data", monetizationData);

                LionAnalytics.MissionCompleted(isTutorial, missionType, missionName, missionID, score, missionAttempt, additionalData);
            }

            int GetMissionAttempt(int currentLevel)
            {
                int missionAttempt = PlayerPrefs.GetInt($"missionAttempt_{nodeIndex}-{currentLevel}", 1);
                return missionAttempt - 1;
            }

        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public static void StartTutorialMain()
    {
        PlayerPrefs.SetInt(GetKeyMissionAttemptFTUE(), GetMissionAttemptFTUE() + 1);

        string missionType = "main";
        string missionName = $"{missionType}_1";
        string missionID = $"{1}";
        int missionAttempt = GetMissionAttemptFTUE();
        LionAnalytics.MissionStarted(true, missionType, missionName, missionID, missionAttempt, null);
        Debug.Log($"Key Pro 1 - {missionAttempt}");
    }
    public static void CompletedTutorialMain()
    {
        string callOneTimeKey = "CompletedTutorialMain!!@@@###";
        if (PlayerPrefs.HasKey(callOneTimeKey))
            return;
        PlayerPrefs.SetInt(callOneTimeKey, 1);

        string missionType = "main";
        string missionName = $"{missionType}_1";
        string missionID = $"{1}";
        int missionAttempt = GetMissionAttemptFTUE();
        LionAnalytics.MissionCompleted(true, missionType, missionName, missionID, missionAttempt, null);

        Debug.Log($"Key Pro 2 - {missionAttempt}");
    }
    private static string GetKeyMissionAttemptFTUE() => "TutorialMain!((@876!";
    private static int GetMissionAttemptFTUE() => PlayerPrefs.GetInt(GetKeyMissionAttemptFTUE(), 0);

    public static void Tutorial(int id)
    {
        //bool isTutorial = true;
        //string missionType = "main";
        //string missionName = $"{missionType}_{1}";
        //string missionID = $"{1}";
        //int missionAttempt = 1;
        //string stepName = "tutorial";
        //Dictionary<string, object> additionalData = new Dictionary<string, object>
        //{
        //    { "step_count", id}
        //};

        //string keyTutorial = $"{id}_{missionType}-{missionName}-{missionID}";
        //bool isCalled = PlayerPrefs.HasKey(keyTutorial);
        //if (isCalled)
        //    return;

        //PlayerPrefs.SetInt(keyTutorial, 1);
        //LionAnalytics.MissionStep(isTutorial, missionType, missionName, missionID, 0, missionAttempt, additionalData, stepName: stepName);
    }
    public static void EconomyEvent(string puzzleID, int amount)
    {
        Puzzle puzzle = PuzzleManager.Instance.GetPuzzleById(puzzleID);
        int nodeIndex = GetIndexNode(puzzle.level, puzzle.size);
        int score = (int)LogicalBoard.Instance.GetAccurateCompletionPercentage();
        int currentLevel = Instance.GetCurrentLevel();

        string missionType = $"main";
        string missionName = $"{missionType}_{currentLevel}";
        string missionID = $"{currentLevel}";
        int missionAttempt = GetMissionAttempt(currentLevel);
        Dictionary<string, object> additionalData = new()
        {
            { "mission_data", new Dictionary<string, object>
            {
                { "mission_type", $"{missionType}" },
                { "mission_name", $"{missionType}_{missionID}"},
                { "mission_id", $"{missionID}"},
                { "mission_attempt", $"{missionAttempt}"}
            }}
        };

        //Product product = new Product();
        //product.AddItem("coin", "soft", amount);
        //Transaction transaction = new Transaction("coin", "soft", null, product, location, location);
        //LionAnalytics.EconomyEvent(transaction, transaction.productID, transaction.transactionID, location, additionalData);

        int GetMissionAttempt(int currentLevel)
        {
            int missionAttempt = PlayerPrefs.GetInt($"missionAttempt_{nodeIndex}-{currentLevel}", 1);
            return missionAttempt - 1;
        }
    }
    public static void PowerUpUsed(string powerName, int amount, int coinSpend)
    {
        string puzzleID = PuzzleManager.currentPuzzleId;
        Puzzle puzzle = PuzzleManager.Instance.GetPuzzleById(puzzleID);
        int nodeIndex = GetIndexNode(puzzle.level, puzzle.size);
        int currentLevel = Instance.GetCurrentLevel();
        string missionType = $"main";
        string missionID = $"{currentLevel}";
        int missionAttempt = GetMissionAttempt(currentLevel);

        Dictionary<string, object> additionalData = new Dictionary<string, object>
        {
            { "amount_used", amount },
            { "coin_used", coinSpend }
        };

        LionAnalytics.PowerUpUsed(missionID, missionType, missionAttempt, powerName, additionalData);

        int GetMissionAttempt(int currentLevel)
        {
            int missionAttempt = PlayerPrefs.GetInt($"missionAttempt_{nodeIndex}-{currentLevel}", 1);
            return missionAttempt - 1;
        }
    }
}
