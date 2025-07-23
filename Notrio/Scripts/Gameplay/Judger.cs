using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Takuzu.Generator;

namespace Takuzu
{
    public class Judger : MonoBehaviour
    {
        [Serializable]
        public class JudgingResult
        {
            public int flagCount;
            public int revealCount;
            public int resetCount;
            public int errorCount;
            public int undoCount;
            public int solvingTime;
            public bool noPowerup;
            public bool noError;
            public int exp;
            public int coin;
        }

        public static Action onPreJudging = delegate { };
        public static Judger Instance { get; private set; }
        public static Action<int> onExpGained = delegate { };
        public static Action<int> onCoinGained = delegate { };
        public static Action onFinishPuzzle = delegate { };
        public static Action<JudgingResult> onJudgingCompleted = delegate { };
        public Timer timer;
        public List<ExpJudgingProfile> expJudgingProfile;
        public List<CoinJudgingProfile> coinJudgingProfile;
        public const float scoreToCoinFactor = 1;
        public int flagCount;
        public int revealCount;
        public int resetCount;
        public int errorCount;
        public int undoCount;
        private Action saveInfoAction;
        private Action resetInfoAction;
        private Action judgeAction;

        public const string JUDGING_INFO_PREFIX = "JUDGING-";
        public const string SOLVING_TIME_PREFIX = "SOLVING-TIME-";
        public const string FLAG_TOTAL_KEY = "FLAG_TOTAL";
        public const string REVEAL_TOTAL_KEY = "REVEAL_TOTAL";
        public const string UNDO_TOTAL_KEY = "UNDO_TOTAL";
        public const string RESET_TOTAL_KEY = "RESET_TOTAL";
        public const string ERROR_TOTAL_KEY = "ERROR_TOTAL";

        private void OnEnable()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
            LogicalBoard.onPuzzleSolved += OnPuzzleSolved;
            LogicalBoard.onCellRevealed += OnCellRevealed;
            LogicalBoard.onSolvingError += OnSolvingError;
            LogicalBoard.onPuzzleReseted += OnPuzzleReseted;
            LogicalBoard.onCellUndone += OnCellUndone;
            VisualBoard.onCellFlagged += OnCellFlagged;
            PuzzleManager.onPuzzleSelected += OnPuzzleSelected;
            ProgressSavingScheduler.Tick += OnSchedulerTick;
            //Add on multiplayer resolve match result
            MultiplayerSession.SessionFinished += OnMultiplayerSesionFinished;
        }

        private void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
            LogicalBoard.onPuzzleSolved -= OnPuzzleSolved;
            LogicalBoard.onCellRevealed -= OnCellRevealed;
            LogicalBoard.onSolvingError -= OnSolvingError;
            LogicalBoard.onPuzzleReseted -= OnPuzzleReseted;
            LogicalBoard.onCellUndone -= OnCellUndone;
            VisualBoard.onCellFlagged -= OnCellFlagged;
            PuzzleManager.onPuzzleSelected -= OnPuzzleSelected;
            ProgressSavingScheduler.Tick -= OnSchedulerTick;
            MultiplayerSession.SessionFinished -= OnMultiplayerSesionFinished;
        }

        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (GameManager.Instance.GameState == GameState.Playing || GameManager.Instance.GameState == GameState.Paused)
            {
                if (focus)
                {
                    LoadJudgingInfo(PuzzleManager.currentPuzzleId);
                }
                else
                {
                    SaveJudgingInfo(PuzzleManager.currentPuzzleId);
                }
            }
        }

        private void OnSchedulerTick()
        {
            SaveJudgingInfo(PuzzleManager.currentPuzzleId);
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Prepare)
            {
                if (oldState == GameState.Paused)
                {
                    saveInfoAction();
                }
                if (oldState == GameState.GameOver)
                {
                    CoroutineHelper.Instance.DoActionDelay(
                        () =>
                        {
                            resetInfoAction();
                        }, 0);
                }
            }
        }

        private void OnPuzzleSolved()
        {
            //Dont do judgeAction too soon if this is multiplayer mode
            if(PuzzleManager.Instance.IsMultiMode(PuzzleManager.currentPuzzleId) == false)
                judgeAction();
        }
        private void OnMultiplayerSesionFinished(bool playerWin){
            if(PuzzleManager.Instance.IsMultiMode(PuzzleManager.currentPuzzleId) == true)
                judgeAction();
        }

        private void OnPuzzleSelected(string id, string puzzle, string solution, string progress)
        {
            LoadJudgingInfo(id);
            saveInfoAction = delegate { SaveJudgingInfo(id); };
            resetInfoAction = delegate { ResetJudgingInfo(id); };

            Size size = PuzzleManager.currentSize;
            Level level = PuzzleManager.currentLevel;
            ExpJudgingProfile expProfile = expJudgingProfile.Find((e) => { return e.size == size && e.level == level; });
            CoinJudgingProfile coinProfile = coinJudgingProfile.Find((c) => { return c.size == size && c.level == level; });
            bool isDaily = id.Contains(PuzzleManager.DAILY_PUZZLE_PREFIX);
            bool isWeekly = id.Contains(PuzzleManager.WEEKLY_PUZZLE_PREFIX);
            bool isMultiplayer = PuzzleManager.Instance.IsMultiMode(id);
            judgeAction = delegate { Judge(expProfile, coinProfile, isDaily, isWeekly, isMultiplayer); };
        }

        private void Judge(ExpJudgingProfile expProfile, CoinJudgingProfile coinProfile, bool isDaily, bool isWeekly, bool isMultiplayer)
        {
            onPreJudging();
            JudgingResult result = new JudgingResult
            {
                flagCount = flagCount,
                revealCount = revealCount,
                errorCount = errorCount,
                resetCount = resetCount,
                undoCount = undoCount
            };

            bool noPowerup = revealCount == 0 && resetCount == 0 && flagCount == 0;
            bool noError = errorCount == 0;
            result.noPowerup = noPowerup;
            result.noError = noError;
            result.solvingTime = (int)timer.Elapsed.TotalSeconds;
            int gainedExp = 0;
            if (expProfile != null)
            {
                float score = 100;
                score -= revealCount * expProfile.revealSubtractPercent;
                score -= errorCount * expProfile.errorSubtractPercent;
                score -= resetCount * expProfile.resetSubtractPercent;
                score -= flagCount * expProfile.flagSubtractPercent;
                score -= undoCount * expProfile.undoSubtractPercent;
                score += noPowerup ? expProfile.noPowerupBonusPercent : 0;
                score += noError ? expProfile.noErrorBonusPercent : 0;

                float baseExp = expProfile.baseExpByTime.Evaluate((float)timer.Elapsed.TotalMinutes);
                gainedExp = (int)Mathf.Max(expProfile.minExp, baseExp * score / 100);
            }
            gainedExp = ((isDaily || isWeekly) && PuzzleManager.Instance.IsPuzzleSolved(PuzzleManager.currentPuzzleId)) ? 0 : gainedExp;
            result.exp = gainedExp;
            int gainedCoin = 0;
            if (coinProfile != null)
            {
                bool needJudging =
                    !(isDaily || isWeekly || isMultiplayer) ||
                    ((isDaily || isWeekly) && !coinProfile.noJudgingOnChallenge);
                if (needJudging)
                {
                    gainedCoin = UnityEngine.Random.Range(coinProfile.minBaseCoin, coinProfile.maxBaseCoin + 1);
                    float score = 100;
                    score += noPowerup ? coinProfile.noPowerupBonusPercent : 0;
                    score += noError ? coinProfile.noErrorBonusPercent : 0;
                    gainedCoin = (int) (gainedCoin * score * scoreToCoinFactor / 100 );
                    if (isDaily && PuzzleManager.Instance.IsPuzzleSolved(PuzzleManager.currentPuzzleId))
                    {
                        score = 0;
                        gainedCoin = 0;
                    }
                }

                //gainedCoin += isDaily && !PuzzleManager.Instance.IsPuzzleSolved(PuzzleManager.currentPuzzleId) ? GetTournamentBaseReward((int)PuzzleManager.currentLevel - 1) : 0;
                //gainedCoin += isWeekly && !PuzzleManager.Instance.IsPuzzleSolved(PuzzleManager.currentPuzzleId) ? GetTournamentBaseReward((int)PuzzleManager.currentLevel - 1) : 0;

                if(isMultiplayer){
                    gainedCoin = 0;
                    bool losingBet = false;
                    if(MultiplayerSession.sessionFinished == false)
                        losingBet = !LogicalBoard.Instance.IsPuzzleSolved();
                    else
                        losingBet = !MultiplayerSession.playerWin;

                    if(losingBet)
                        gainedCoin -= MultiplayerRoom.Instance.currentBetCoin;
                    else
                        gainedCoin += MultiplayerRoom.Instance.currentBetCoin;

                    if (losingBet)
                    {
                        gainedExp = 0;
                        result.exp = 0;
                    }
                }
                result.coin = gainedCoin;
            }

            //string key = SOLVING_TIME_PREFIX + PuzzleManager.currentPuzzleId;
            //PlayerDb.SetInt(key, result.solvingTime);
            onFinishPuzzle();
            onExpGained(gainedExp);
            onCoinGained(gainedCoin);
            onJudgingCompleted(result);
        }

        public static int GetTournamentBaseReward(int i)
        {
            if (CloudServiceManager.Instance.appConfig.GetIntList("TournamentBaseRewards").Count == 0)
                return 0;
            else
                return CloudServiceManager.Instance.appConfig.GetIntList("TournamentBaseRewards").ToArray()[i]; 
        }

        private void OnCellRevealed(Index2D i)
        {
            revealCount += 1;
            int revealTotal = PlayerDb.GetInt(REVEAL_TOTAL_KEY, 0) + 1;
            PlayerDb.SetInt(REVEAL_TOTAL_KEY, revealTotal);
        }

        private void OnSolvingError()
        {
            errorCount += 1;
            int errorTotal = PlayerDb.GetInt(ERROR_TOTAL_KEY, 0) + 1;
            PlayerDb.SetInt(ERROR_TOTAL_KEY, errorTotal);

        }

        private void OnPuzzleReseted()
        {
            resetCount += 1;
            int resetTotal = PlayerDb.GetInt(RESET_TOTAL_KEY, 0) + 1;
            PlayerDb.SetInt(RESET_TOTAL_KEY, resetTotal);
        }

        private void OnCellFlagged(Index2D i)
        {
            flagCount += 1;
            int flagTotal = PlayerDb.GetInt(FLAG_TOTAL_KEY, 0) + 1;
            PlayerDb.SetInt(FLAG_TOTAL_KEY, flagTotal);
        }

        private void OnCellUndone(Index2D i)
        {
            undoCount += 1;
            int undoTotal = PlayerDb.GetInt(UNDO_TOTAL_KEY, 0) + 1;
            PlayerDb.SetInt(UNDO_TOTAL_KEY, undoTotal);
        }

        public void SaveJudgingInfo(string id)
        {
            if (flagCount == 0 && revealCount == 0 && resetCount == 0 && errorCount == 0 && undoCount == 0)
                return;
            string key = JUDGING_INFO_PREFIX + id;
            string info = string.Format("{0}-{1}-{2}-{3}-{4}", flagCount, revealCount, resetCount, errorCount, undoCount);
            PlayerDb.SetString(key, info);
        }

        public void LoadJudgingInfo(string id)
        {
            string key = JUDGING_INFO_PREFIX + id;
            string info = PlayerDb.GetString(key, "");
            if (!string.IsNullOrEmpty(info))
            {
                string[] s = info.Split('-');

                try { flagCount = int.Parse(s[0]); } catch { flagCount = 0; }
                try { revealCount = int.Parse(s[1]); } catch { revealCount = 0; }
                try { resetCount = int.Parse(s[2]); } catch { resetCount = 0; }
                try { errorCount = int.Parse(s[3]); } catch { errorCount = 0; }
                try { undoCount = int.Parse(s[4]); } catch { undoCount = 0; }
            }
            else
            {
                flagCount = 0;
                revealCount = 0;
                resetCount = 0;
                errorCount = 0;
                undoCount = 0;
            }

        }

        public void ResetJudgingInfo(string id)
        {
            string key = JUDGING_INFO_PREFIX + id;
            PlayerDb.DeleteKey(key);
            flagCount = 0;
            revealCount = 0;
            resetCount = 0;
            errorCount = 0;
            undoCount = 0;
        }

        public int GetRewardForDailyChallenge(Size s)
        {
            CoinJudgingProfile c = coinJudgingProfile.Find((profile) =>
            {
                return profile.size == s;
            });
            if (c != null)
            {
                return c.dailyChallengeFixedReward;
            }
            else
            {
                return 0;
            }
        }

        public int GetRewardForWeeklyChallenge(Size s)
        {
            CoinJudgingProfile c = coinJudgingProfile.Find((profile) =>
            {
                return profile.size == s;
            });
            if (c != null)
            {
                return c.weeklyChallengeFixedReward;
            }
            else
            {
                return 0;
            }
        }
    }
}