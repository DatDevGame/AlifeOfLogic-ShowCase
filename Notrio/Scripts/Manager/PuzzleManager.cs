using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu.Generator;
using System.Linq;
using System.IO;
using System.Data;
using Mono.Data.Sqlite;
using GameSparks.Api.Responses;
using GameSparks.Core;
using System;
using GameSparks.Api.Messages;
using UnityEngine.SceneManagement;
namespace Takuzu
{
    public class PuzzleManager : MonoBehaviour
    {
        public static PuzzleManager Instance { get; private set; }
        public static System.Action<List<Puzzle>> onChallengeListChanged = delegate { };
        //event fired after selecting a puzzle
        //params: puzzleId, puzzleString, solutionString, progressString
        public static System.Action<string, string, string, string> onPuzzleSelected = delegate { };
        public static System.Action<PuzzlePack> onPackSelected = delegate { };
        public static System.Action<PuzzlePack> onPackUnlocked = delegate { };
        public static System.Action onUnlockTournamentChapter = delegate { };

        public CryptoKey cryptoKey;
        public List<PuzzlePack> packs;
        public List<int> ageList;
        public List<Color> accentColors;
        public List<PackSizes> packSizesList;
        [Serializable]
        public struct LevelInfor
        {
            public Level level;
            public string levelName;
        }
        public List<LevelInfor> levelInfors;
        [Serializable]
        public struct PackSizes
        {
            public Level packLevel;
            public List<Size> sizes;
        }

        public static bool databaseReady;
        [HideInInspector]
        public List<Puzzle> challengePuzzles;
        [HideInInspector]
        public DateTime serverNow = DateTime.UtcNow;
        [HideInInspector]
        public List<string> challengeIds;
        [HideInInspector]
        public static PuzzlePack currentPack;

        public static string currentPuzzleId;
        public static bool currentIsChallenge;
        public static bool? currentIsRecent;
        public static string currentPuzzleStr;
        public static string currentSolutionStr;
        public static string currentProgressStr;
        public static Size currentSize;
        public static Level currentLevel;
        public static bool isRequestingChallenge;
        public static string nextPuzzleId;

        private static string recentlyPlayId;
        public static string RecentlyPlayId
        {
            set
            {
                recentlyPlayId = value;
                PlayerDb.SetString(RECENT_KEY, recentlyPlayId);
            }
            get
            {
                if (string.IsNullOrEmpty(recentlyPlayId))
                {
                    recentlyPlayId = PlayerDb.GetString(RECENT_KEY, "");
                }
                return recentlyPlayId;
            }
        }

        internal string GetAge(int packIndex, int difficultLength, int offset)
        {
            return ageList[packIndex * difficultLength + offset].ToString();
        }

        public const string PROGRESS_PREFIX = "PROGRESS-";
        public const string SOLVED_PREFIX = "SOLVED-";
        public const string DAILY_PUZZLE_PREFIX = "DAILY-";
        public const string WEEKLY_PUZZLE_PREFIX = "WEEKLY-";
        public const string SUBMITTED_PREFIX = "SUBMITTED-";
        public const string UNLOCK_PREFIX = "UNLOCK-";
        public const string RECENT_KEY = "RECENT";
        public const string PLAYED_PREFIX = "PLAYED-";
        public const string NEWEST_PREFIX = "NEWEST-";
        public const string RECEIVED_TOTAL_PREFIX = "RECEIVED-";
        public const string DAILY_CHAPTER_PREFIX = "DAILY-CHAPTER-";

        private void OnEnable()
        {
            CloudServiceManager.onGamesparkAuthenticated += OnGamesparkAuthenticated;
            CloudServiceManager.onLoginGameSpark += OnLoginGamespark;
            LogicalBoard.onProgressReported += OnProgressReported;
            Judger.onJudgingCompleted += OnJudgingCompleted;
            PlayerDb.Resetted += OnPlayerDbReset;
            PlayerDb.RequestDecrypt += OnPlayerDbRequestDecrypt;
            PlayerDb.RequestEncrypt += OnPlayerDbRequestEncrypt;
            GameManager.GameStateChanged += OnGameStateChanged;
            NotificationManager.onNotificationReceived += OnNotificationReceived;
        }

        private void OnDisable()
        {
            CloudServiceManager.onGamesparkAuthenticated -= OnGamesparkAuthenticated;
            CloudServiceManager.onLoginGameSpark -= OnLoginGamespark;
            LogicalBoard.onProgressReported -= OnProgressReported;
            Judger.onJudgingCompleted -= OnJudgingCompleted;
            PlayerDb.Resetted -= OnPlayerDbReset;
            PlayerDb.RequestDecrypt -= OnPlayerDbRequestDecrypt;
            PlayerDb.RequestEncrypt -= OnPlayerDbRequestEncrypt;
            GameManager.GameStateChanged -= OnGameStateChanged;
            NotificationManager.onNotificationReceived -= OnNotificationReceived;
        }

        private static Dictionary<string, IDbConnection> dbConnectionsDictionary = new Dictionary<string, IDbConnection>();

        private void OnApplicationQuit()
        {
            foreach (var item in dbConnectionsDictionary)
                item.Value.Close();
        }

        private static IDbConnection GetDBConnection(string dbName)
        {
            if (dbConnectionsDictionary.ContainsKey(dbName) == false)
                dbConnectionsDictionary.Add(dbName, Data.ConnectToDatabase(dbName));
            IDbConnection conn = dbConnectionsDictionary[dbName];
            if (conn.State == ConnectionState.Closed || conn.State == ConnectionState.Broken)
                conn.Open();
            return conn;
        }

        private void OnLoginGamespark(AuthenticationResponse response)
        {
            if (response.HasErrors)
                return;
            if (response.NewPlayer.Value == false)
            {
                string localPlayerId = PlayerDb.GetString(PlayerDb.PLAYER_ID_KEY, string.Empty);
                if (!response.UserId.Equals(localPlayerId))
                {
                    ResetRecently();
                }
            }
        }

        private void OnNotificationReceived(string msg, Dictionary<string, object> data)
        {
            if (data == null)
            {
                Debug.Log("OnNotificationReceived data = null");
                return;
            }
            if (data.ContainsKey("newChallengeAvailable"))
            {
                RequestChallenge();
            }
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Init();
                InvokeRepeating("CheckChallengeAvailable", 60, 60);
            }

            ScriptMessage_RT_TOURNAMENT_REFRESHED.Listener += onTournamentRefreshed;
        }
        private void OnDestroy()
        {
            ScriptMessage_RT_TOURNAMENT_REFRESHED.Listener -= onTournamentRefreshed;
        }

        private void onTournamentRefreshed(ScriptMessage_RT_TOURNAMENT_REFRESHED obj)
        {
            if (SceneManager.GetActiveScene().name.Equals("Tournament"))
                InGameNotificationPopup.Instance.confirmationDialog.Show(I2.Loc.ScriptLocalization.ATTENTION, I2.Loc.ScriptLocalization.TOURNAMENT_OVER, I2.Loc.ScriptLocalization.OK, "", () => { });
            RequestChallenge();
            TournamentDataRequest.ClearCatchedData();
        }


        public void Init()
        {
            PrepareDb();
        }

        private void CheckChallengeAvailable()
        {
            if (GS.Authenticated)
            {
                if (challengePuzzles == null || challengePuzzles.Count == 0)
                {
                    if (!isRequestingChallenge)
                        RequestChallenge();
                }
            }
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Prepare)
            {
                StartCoroutine(CrResetCurrentPuzzleInfo());
            }
        }

        private IEnumerator CrResetCurrentPuzzleInfo()
        {
            yield return null;
            currentPuzzleId = null;
            currentPuzzleStr = null;
            currentProgressStr = null;
            currentSolutionStr = null;
            currentSize = Size.Unknown;
            currentLevel = Level.UnGraded;
            currentIsChallenge = false;
            currentIsRecent = null;
            nextPuzzleId = null;
        }

        private void OnPlayerDbReset()
        {
            ResetRecently();
        }

        private void OnGamesparkAuthenticated()
        {
            RequestChallenge();
        }

        private void RequestChallenge()
        {
            isRequestingChallenge = true;
            CloudServiceManager.Instance.RequestChallenge(RequestChallengeCallback);
        }

        private void RequestChallengeCallback(LogEventResponse response)
        {
            isRequestingChallenge = false;
            if (response.HasErrors)
                return;

            List<GSData> data = response.ScriptData.GetGSDataList("puzzleArray");
            challengePuzzles = new List<Puzzle>();
            challengeIds = new List<string>();
            for (int i = 0; i < data.Count; ++i)
            {
                string creationTime = data[i].GetString("creationTime");
                string puzzleType = data[i].GetString("type");
                string p = data[i].GetGSData("puzzle").GetString("puzzle");
                string s = data[i].GetGSData("puzzle").GetString("solution");
                Size sz = (Size)data[i].GetGSData("puzzle").GetInt("size");
                Level lv = (Level)data[i].GetGSData("puzzle").GetInt("level");
                string id = string.Format("{0}{1}-{2}-{3}",
                    puzzleType.Equals("daily") ? DAILY_PUZZLE_PREFIX : WEEKLY_PUZZLE_PREFIX,
                    creationTime,
                    sz.ToString(),
                    lv.ToString());
                Puzzle puzzle = new Puzzle(sz, lv, p, s, -1, -1, -1, -1);
                challengePuzzles.Add(puzzle);
                challengeIds.Add(id);

                if (puzzleType.Equals("daily"))
                {
                    AppendReceivedChallenge(DAILY_PUZZLE_PREFIX, id);
                }
                else if (puzzleType.Equals("weekly"))
                {
                    AppendReceivedChallenge(WEEKLY_PUZZLE_PREFIX, id);
                }
            }

            long serverTime = response.ScriptData.GetLong("Time") ?? 0;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            serverNow = epoch.AddMilliseconds(serverTime);
            lastTimeGotTimeFromServer = Time.time;
            onChallengeListChanged(challengePuzzles);
        }

        private float lastTimeGotTimeFromServer = 0;

        public TimeSpan timeToEndOfTheDay
        {
            get
            {
                DateTime nowFromServer = serverNow.AddSeconds(Time.time - lastTimeGotTimeFromServer);
                DateTime endDailyTime = nowFromServer.Date.AddDays(1);
                long timeToEndDaily = (new DateTime(endDailyTime.Year, endDailyTime.Month, endDailyTime.Day) - nowFromServer).Ticks;
                timeToEndDaily = timeToEndDaily >= 0 ? timeToEndDaily : 0;
                TimeSpan timeSpanLeft = new TimeSpan(timeToEndDaily);
                return timeSpanLeft;
            }
        }

        public TimeSpan timeToEndOfTheWeek
        {
            get
            {
                DateTime nowFromServer = PuzzleManager.Instance.serverNow.AddSeconds(Time.time - lastTimeGotTimeFromServer);
                DateTime today = new DateTime(nowFromServer.Year, nowFromServer.Month, nowFromServer.Day);
                DateTime endWeeklyDateTime = nowFromServer.AddDays(((int)DayOfWeek.Monday - (int)today.DayOfWeek) % 7 + 7);
                long timeToEndWeekly = (new DateTime(endWeeklyDateTime.Year, endWeeklyDateTime.Month, endWeeklyDateTime.Day) - nowFromServer).Ticks;
                timeToEndWeekly = timeToEndWeekly >= 0 ? timeToEndWeekly : 0;
                TimeSpan timeSpanLeft = new TimeSpan(timeToEndWeekly);
                return timeSpanLeft;
            }
        }
        public const string tournamentBaseRewardIncrKey = "TournamentBaseRewardIncreasement";

        public static int RewardInc { get { return CloudServiceManager.Instance.appConfig.GetInt(tournamentBaseRewardIncrKey) ?? 10; } }

        /// <summary>
        /// Store all received challenge id in a string
        /// </summary>
        /// <param name="challengeType">use the daily or weekly prefix</param>
        /// <param name="id">New id to append</param>
        private void AppendReceivedChallenge(string challengeType, string id)
        {
            string receivedKey = string.Format("{0}{1}", RECEIVED_TOTAL_PREFIX, challengeType);
            string receivedIds = PlayerDb.GetString(receivedKey, string.Empty);
            if (!receivedIds.Contains(id))
                receivedIds = string.Format("{0}{1}{2}", receivedIds, id, ";");
            PlayerDb.SetString(receivedKey, receivedIds);
        }

        public int GetReceivedChallengeCount(string challengeType)
        {
            string key = string.Format("{0}{1}", RECEIVED_TOTAL_PREFIX, challengeType);
            string receivedIds = PlayerDb.GetString(key, string.Empty);
            return receivedIds.Split(new string[] { ";" }, System.StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public System.DateTime GetChallengeCreationDate(string challengeId)
        {
            System.DateTime date = new System.DateTime();
            if (challengeId.StartsWith(DAILY_PUZZLE_PREFIX) || challengeId.StartsWith(WEEKLY_PUZZLE_PREFIX))
            {
                string[] s = challengeId.Split('-');
                int y = int.Parse(s[1]);
                int m = int.Parse(s[2]);
                int d = int.Parse(s[3]);
                date = new System.DateTime(y, m, d);
            }
            return date;
        }

        private void OnProgressReported(string progress)
        {
            if (!string.IsNullOrEmpty(currentPuzzleId))
                SavePuzzleProgress(currentPuzzleId, progress);
        }

        /// <summary>
        /// Copy all database file from streamming asset to app storage area
        /// </summary>
        private void PrepareDb()
        {
            Data.persistentDataPath = Application.persistentDataPath;
            Data.streamingAssetsPath = Application.streamingAssetsPath;
            for (int i = 0; i < packs.Count; ++i)
            {
                try
                {
                    Data.PrepareDatabase(packs[i].DbPath);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }
            }
            databaseReady = true;
        }

        public string GetPackUnlockKey(PuzzlePack pack)
        {
            string key = UNLOCK_PREFIX + pack.packName;
            return key;
        }

        public bool IsPackUnlocked(PuzzlePack pack)
        {
            if (pack.price == 0)
                return true;
            string key = GetPackUnlockKey(pack);
            string encryptedKey = Crypto.Encrypt(key, cryptoKey);
            return PlayerDb.GetBool(encryptedKey, false);
        }

        public void SetPackUnlocked(PuzzlePack pack)
        {
            string key = GetPackUnlockKey(pack);
            string encryptedKey = Crypto.Encrypt(key, cryptoKey);
            PlayerDb.SetBool(encryptedKey, true);
            PlayerDb.Save();
            onPackUnlocked(pack);
        }

        /// <summary>
        /// Decrypt sensitive info before syncing
        /// </summary>
        /// <param name="d"></param>
        private void OnPlayerDbRequestDecrypt(Dictionary<string, object> d)
        {
            for (int i = 0; i < packs.Count; ++i)
            {
                string key = GetPackUnlockKey(packs[i]);
                string encryptedKey = Crypto.Encrypt(key, cryptoKey);
                if (d.ContainsKey(encryptedKey))
                {
                    d[key] = true;
                    d.Remove(encryptedKey);
                }
            }
        }

        /// <summary>
        /// Encrypt sensitive info after syncing
        /// </summary>
        /// <param name="d"></param>
        private void OnPlayerDbRequestEncrypt(Dictionary<string, object> d)
        {
            for (int i = 0; i < packs.Count; ++i)
            {
                string key = GetPackUnlockKey(packs[i]);
                if (d.ContainsKey(key))
                {
                    string encryptedKey = Crypto.Encrypt(key, cryptoKey);
                    d[encryptedKey] = true;
                    d.Remove(key);
                }
            }
        }

        public bool IsPuzzleSolved(string id)
        {
            string key = SOLVED_PREFIX + id;
            bool solved = PlayerDb.GetInt(key, 0) > 0;
            return solved;
        }

        public bool IsPuzzleInProgress(string id)
        {
            string key = PROGRESS_PREFIX + id;
            return !string.IsNullOrEmpty(PlayerDb.GetString(key, string.Empty));
        }

        /// <summary>
        /// Load a puzzle progress and validate that progress
        /// if validate fail, return the original puzzle
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string LoadPuzzleProgress(string id)
        {
            string key = PROGRESS_PREFIX + id;
            string s = PlayerDb.GetString(key, string.Empty);
            Puzzle p;

            if (IsChallenge(id))
            {
                p = GetChallengeById(id);
            }
            else if (IsMultiMode(id))
            {
                p = GetMultiPlayerPuzzleById(id);
            }
            else
            {
                p = GetPuzzleById(id);
            }
            string puzzleStr = DecryptPuzzle(p.puzzle, p.size);
            string solutionStr = DecryptSolution(p.solution, p.size);
            if (!string.IsNullOrEmpty(s) && s.Length == puzzleStr.Length)
            {
                for (int i = 0; i < puzzleStr.Length; ++i)
                {
                    char c = s[i];
                    char pi = puzzleStr[i];
                    char si = solutionStr[i];

                    bool validToken =
                        c == LogicalBoard.CHAR_EMPTY ||
                        c == LogicalBoard.CHAR_ONE ||
                        c == LogicalBoard.CHAR_ONE_LOCK ||
                        c == LogicalBoard.CHAR_ZERO ||
                        c == LogicalBoard.CHAR_ZERO_LOCK;
                    bool validValue = false;
                    if (c == LogicalBoard.CHAR_EMPTY && pi == LogicalBoard.CHAR_EMPTY)
                        validValue = true;
                    if (c == LogicalBoard.CHAR_ONE && (pi == LogicalBoard.CHAR_EMPTY || pi == LogicalBoard.CHAR_ONE))
                        validValue = true;
                    if (c == LogicalBoard.CHAR_ZERO && (pi == LogicalBoard.CHAR_EMPTY || pi == LogicalBoard.CHAR_ZERO))
                        validValue = true;
                    if (c == LogicalBoard.CHAR_ONE_LOCK &&
                        (pi == LogicalBoard.CHAR_EMPTY || pi == LogicalBoard.CHAR_ONE) &&
                        si == LogicalBoard.CHAR_ONE)
                        validValue = true;
                    if (c == LogicalBoard.CHAR_ZERO_LOCK &&
                        (pi == LogicalBoard.CHAR_EMPTY || pi == LogicalBoard.CHAR_ZERO) &&
                        si == LogicalBoard.CHAR_ZERO)
                        validValue = true;

                    bool valid = validToken && validValue;
                    if (!valid)
                    {
                        s = puzzleStr
                            .Replace(LogicalBoard.CHAR_ONE, LogicalBoard.CHAR_ONE_LOCK)
                            .Replace(LogicalBoard.CHAR_ZERO, LogicalBoard.CHAR_ZERO_LOCK);
                        break;
                    }
                }
            }
            else
            {
                s = puzzleStr
                    .Replace(LogicalBoard.CHAR_ONE, LogicalBoard.CHAR_ONE_LOCK)
                    .Replace(LogicalBoard.CHAR_ZERO, LogicalBoard.CHAR_ZERO_LOCK);
            }

            return s;
        }

        public void SavePuzzleProgress(string id, string progress)
        {
            string key = PROGRESS_PREFIX + id;
            PlayerDb.SetString(key, progress);
        }

        public void ResetPuzzleProgress(string id)
        {
            string key = PROGRESS_PREFIX + id;
            PlayerDb.SetString(key, string.Empty);
        }

        public void SetPuzzleSolved(string id)
        {
            string key = SOLVED_PREFIX + id;
            int solveCount = PlayerDb.GetInt(key, 0);
            solveCount += 1;
            PlayerDb.SetInt(key, solveCount);
            ResetPuzzleProgress(id);
            ResetRecently();
        }

        public void ResetRecently()
        {
            RecentlyPlayId = null;
            PlayerDb.DeleteKey(RECENT_KEY);
        }

        public void SetPuzzlePlayed(string id)
        {
            string key = PLAYED_PREFIX + id;
            int playedCount = PlayerDb.GetInt(key, 0);
            playedCount += 1;
            PlayerDb.SetInt(key, playedCount);
            PlayerDb.Save();
        }

        public bool IsPuzzlePlayed(string id)
        {
            string key = PLAYED_PREFIX + id;
            bool played = PlayerDb.GetInt(key, 0) > 0;
            return played;
        }

        public void SelectPuzzle(string id)
        {
            Puzzle selectedPuzzle = GetPuzzleByIdIgnoreType(id);
            currentPuzzleId = id;

            if (IsChallenge(id))
                currentIsChallenge = true;
            else
                currentIsChallenge = false;

            if (IsMultiMode(id))
                currentIsMultiMode = true;
            else
                currentIsMultiMode = false;

            currentPuzzleStr = DecryptPuzzle(selectedPuzzle.puzzle, selectedPuzzle.size);
            currentSolutionStr = DecryptPuzzle(selectedPuzzle.solution, selectedPuzzle.size);
            currentSize = selectedPuzzle.size;
            currentLevel = selectedPuzzle.level;
            currentProgressStr = LoadPuzzleProgress(id);

            if (!currentIsRecent.HasValue)
                currentIsRecent = false;
            if (!currentIsChallenge)
                RecentlyPlayId = currentPuzzleId;
            else
            {
                ResetRecently();
                currentPack = null;
            }
            SetPuzzlePlayed(id);
            PrepareNextPuzzleId();

            if (currentIsChallenge == false && currentIsMultiMode == false)
            {
                PuzzlePack pack = null;
                Size si;
                int offset;
                if (SplitPuzzleId(id, out pack, out si, out offset))
                {
                    currentPack = pack;
                }
            }
            onPuzzleSelected(currentPuzzleId, currentPuzzleStr, currentSolutionStr, currentProgressStr);
        }

        public bool IsChallenge(string id)
        {
            return challengeIds.Contains(id);
        }

        public bool IsMultiMode(string id)
        {
            return multiPlayerPuzzleIds.Contains(id);
        }
        public Puzzle GetPuzzleByIdIgnoreType(string id)
        {
            Puzzle p;

            if (IsChallenge(id))
            {
                p = GetChallengeById(id);
            }
            else if (IsMultiMode(id))
            {
                p = GetMultiPlayerPuzzleById(id);
            }
            else
            {
                p = GetPuzzleById(id);
            }
            return p;
        }
        /// <summary>
        /// Return a puzzle using its id
        /// </summary>
        /// <param name="id">The puzzle's id</param>
        /// <returns></returns>
        public Puzzle GetPuzzleById(string id)
        {
            string[] idSplited = id.Split('-');
            string db = idSplited[0];
#if UNITY_EDITOR
            PuzzlePack pack = packs.Find((ep) => { return Path.GetFileName(ep.DbPath).Equals(db); });
            db = pack.DbPath;
#endif
            string packName = idSplited[1];
            int size = int.Parse(idSplited[2]);
            int offset = int.Parse(idSplited[3]);
            Puzzle p = null;

            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            string commandText = string.Empty;

            try
            {
                commandText = string.Format(
                    "SELECT PUZZLE, SOLUTION, LEVEL FROM {0} WHERE PACK = '{1}' AND SIZE = '{2}' LIMIT 1 OFFSET {3}",
                    Data.puzzleTableName,
                    packName,
                    size,
                    offset);
                connection = GetDBConnection(db);
                command = Data.CreateCommand(connection, commandText);
                reader = command.ExecuteReader();

                if (reader.Read())
                {
                    try
                    {
                        string puzzleStr = reader.GetString(0);
                        string solutionStr = reader.GetString(1);
                        int level = reader.GetInt32(2);
                        p = new Puzzle((Size)size, (Level)level, puzzleStr, solutionStr, -1, -1, -1, -1);
                    }
                    catch
                    {
                        p = null;
                    }
                }

            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.ToString());
            }
            if (command != null)
                command.Dispose();
            if (reader != null)
                reader.Close();
            return p;
        }

        public Puzzle GetChallengeById(string id)
        {
            int index = challengeIds.FindIndex((s) => { return s.Equals(id); });
            if (index != -1)
            {
                return challengePuzzles[index];
            }
            else
            {
                return null;
            }
        }
        public List<Puzzle> multiPlayerPuzzles = new List<Puzzle>();
        public List<string> multiPlayerPuzzleIds = new List<string>();
        public static bool currentIsMultiMode;

        public Puzzle GetMultiPlayerPuzzleById(string id)
        {
            int index = multiPlayerPuzzleIds.FindIndex((s) => { return s.Equals(id); });
            if (index != -1)
            {
                return multiPlayerPuzzles[index];
            }
            else
            {
                return null;
            }
        }

        public string GetDailyChallengeIdString()
        {
            string id = null;
            id = currentPuzzleId.StartsWith(DAILY_PUZZLE_PREFIX) ? currentPuzzleId : null;
            return id;
        }

        public int GetDailyChallengeIdLevel()
        {
            int level = -1;
            level = currentPuzzleId.StartsWith(DAILY_PUZZLE_PREFIX) ? (int)currentLevel : -1;
            return level;
        }

        public int GetDailyChallengeIdSize()
        {
            int size = 6;
            size = currentPuzzleId.StartsWith(DAILY_PUZZLE_PREFIX) ? (int)currentSize : 6;
            return size;
        }

        public string GetWeeklyChallengeIdString()
        {
            string id = null;
            id = currentPuzzleId.StartsWith(WEEKLY_PUZZLE_PREFIX) ? currentPuzzleId : null;
            return id;
        }

        public int GetWeeklyChallengeIdLevel()
        {
            int level = -1;
            level = currentPuzzleId.StartsWith(WEEKLY_PUZZLE_PREFIX) ? (int)currentLevel : -1;
            return level;
        }

        public string GetChallengeId(string puzzleStr)
        {
            string id = null;
            int index = challengePuzzles.FindIndex((Puzzle p) => { return p.puzzle.Equals(puzzleStr); });
            if (index >= 0)
                id = challengeIds[index];
            return id;
        }

        public string GetPuzzleId(PuzzlePack pack, Size size, int offset)
        {
            string id = string.Format("{0}-{1}-{2}-{3}", Path.GetFileName(pack.DbPath), pack.packName, (int)size, offset);
            return id;
        }

        public bool SplitPuzzleId(string id, out PuzzlePack pack, out Size size, out int offset)
        {
            pack = null;
            size = Size.Unknown;
            offset = -1;

            if (IsChallenge(id))
                return false;
            string[] s = id.Split('-');
            string dbPath = s[0];
            string packName = s[1];
            pack = packs.Find(p => Path.GetFileName(p.DbPath).Equals(dbPath) && p.packName.Equals(packName));

            string sizeStr = s[2];
            int sizeInt = int.Parse(sizeStr);
            size = (Size)sizeInt;

            string offsetStr = s[3];
            offset = int.Parse(offsetStr);

            return true;
        }

        public string DecryptPuzzle(string p, Size sz)
        {
            string result = p;
            if (!p.IsPuzzleStringOfSize(sz))
            {
                result = Crypto.Decrypt(p, cryptoKey);
            }
            if (!result.IsPuzzleStringOfSize(sz))
            {
                throw new System.Exception("Cannot decrypt puzzle " + p);
            }
            return result;
        }

        public string DecryptSolution(string s, Size sz)
        {
            string result = s;
            if (!s.IsSolutionStringOfSize(sz))
            {
                result = Crypto.Decrypt(s, cryptoKey);
            }
            if (!result.IsSolutionStringOfSize(sz))
            {
                throw new System.Exception("Cannot decrypt solution " + s);
            }
            return result;
        }

        private void OnJudgingCompleted(Judger.JudgingResult result)
        {
            long solvingTime = (long)result.solvingTime * 1000;
            if (currentPuzzleId.StartsWith(DAILY_PUZZLE_PREFIX))
            {
                if (!IsPuzzleSolved(currentPuzzleId))
                {
                    CloudServiceManager.Instance.SubmitDailySolvingTime(
                        solvingTime,
                        (LogEventResponse response) =>
                        {
                            if (response.HasErrors)
                            {
                                Debug.LogWarning(response.Errors.JSON);
                            }
                        });
                    CloudServiceManager.Instance.SubmitCountryDailySolvingTime(solvingTime);
                }
                else
                {
                    Debug.Log("Daily challenge had been solved before, cancel submitting");
                }
            }
            if (currentPuzzleId.StartsWith(WEEKLY_PUZZLE_PREFIX))
            {
                if (!IsPuzzleSolved(currentPuzzleId))
                {
                    CloudServiceManager.Instance.SubmitWeeklySolvingTime(
                        solvingTime,
                        (LogEventResponse response) =>
                        {
                            if (response.HasErrors)
                            {
                                Debug.LogWarning(response.Errors.JSON);
                            }
                        });
                    CloudServiceManager.Instance.SubmitCountryWeeklySolvingTime(solvingTime);
                }
                else
                {
                    Debug.Log("Weekly challenge had been solved before, cancel submitting");
                }
            }

            SetPuzzleSolved(currentPuzzleId);
        }

        public static int CountPuzzleOfPack(PuzzlePack pack, Size size)
        {
            string db = pack.DbPath;
            string packName = pack.packName;
            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            string commandText = string.Empty;
            int count = 0;
            try
            {
                commandText =
                    string.Format("SELECT COUNT(SIZE) FROM {0} WHERE PACK='{1}' AND SIZE='{2}'",
                    Data.puzzleTableName,
                    packName,
                    (int)size);
                connection = Data.ConnectToDatabase(db);
                command = Data.CreateCommand(connection, commandText);
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    try
                    {
                        count = reader.GetInt32(0);
                    }
                    catch
                    {
                        count = 0;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.ToString());
            }
            Data.Flush(connection, command, reader);
            return count;
        }

        public static List<Level> GetPackDifficulties(PuzzlePack pack)
        {
            List<Level> result = new List<Level>();
            string db = pack.DbPath;
            string packName = pack.packName;
            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            string commandText = string.Empty;
            try
            {
                commandText =
                    string.Format("SELECT DISTINCT LEVEL FROM {0} WHERE PACK='{1}'",
                    Data.puzzleTableName,
                    packName);
                connection = GetDBConnection(db);
                command = Data.CreateCommand(connection, commandText);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        int levelInt = reader.GetInt32(0);
                        result.Add((Level)levelInt);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.ToString());
            }
            if (command != null)
                command.Dispose();
            if (reader != null)
                reader.Close();
            return result;
        }

        public void SelectPack(PuzzlePack pack)
        {
            currentPack = pack;
            onPackSelected(currentPack);
        }

        /// <summary>
        /// Get the last number present in a puzzle id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetLevelOffset(string id)
        {
            int offset = -1;
            if (!id.StartsWith(DAILY_PUZZLE_PREFIX) && !id.StartsWith(WEEKLY_PUZZLE_PREFIX))
            {
                offset = int.Parse(id.Split('-')[3]) + 1;
            }
            return offset;
        }


        /// <summary>
        /// Get the next available puzzle id when selecting one
        /// </summary>
        private void PrepareNextPuzzleId()
        {
            if (currentIsChallenge || currentIsMultiMode)
            {
                nextPuzzleId = null;
                return;
            }

            PuzzlePack pack;
            Size size;
            int offset;

            bool splitSuccessful = SplitPuzzleId(currentPuzzleId, out pack, out size, out offset);
            if (!splitSuccessful)
            {
                nextPuzzleId = null;
                return;
            }

            if (pack == null)
            {
                nextPuzzleId = null;
                return;
            }

            //Try increasing the offset
            offset += 1;
            if (offset >= pack.GetPuzzlePreCountNumber(size)) //if it greater than the number puzzle of current size in the pack, try examine the next available size
            {
                Size nextSize = pack.GetNextPuzzleSizeInPack(size);
                if (nextSize != Size.Unknown) //if there is a larger size available, reset offset and constructing an id
                {
                    offset = 0;
                    nextPuzzleId = GetPuzzleId(pack, nextSize, offset);
                    return;
                }
                else //reach the end of the pack
                {
                    nextPuzzleId = null;
                    return;
                }
            }
            else //otherwise constructing a new id
            {
                nextPuzzleId = GetPuzzleId(pack, size, offset);
                return;
            }
        }

        // struct of Daily Chapter playerpref = key : "DAILY-CHAPTER-easy" value: "1/1/2019_1"
        public bool IsDailyChapterUnlocked(string id)
        {
            String[] idSplits = id.Split('-');
            if (idSplits.Length <= 0)
            {
                //Debug.Log("IsDailyChapterUnlocked false - can't convert puzzle id");
                return false;
            }

            string key = DAILY_CHAPTER_PREFIX + idSplits[idSplits.Length - 1];
            if (!PlayerPrefs.HasKey(key))
            {
                //Debug.Log("IsDailyChapterUnlocked false - not found key");
                return false;
            }

            string[] valueSplits = PlayerPrefs.GetString(key).Split('_');
            if (valueSplits.Length != 2)
            {
                //Debug.Log("IsDailyChapterUnlocked false - can't convert value");
                return false;
            }

            string createdDate = GetChallengeCreationDate(id).ToShortDateString();
            if (!valueSplits[0].Equals(createdDate))
            {
                //Debug.Log("IsDailyChapterUnlocked false - it's value of previous day");
                return false;
            }

            int numberWatchedRewardAds = 0;
            if (!Int32.TryParse(valueSplits[1], out numberWatchedRewardAds))
            {
                //Debug.Log("IsDailyChapterUnlocked false - can't parse value from string to int");
                return false;
            }

            if (numberWatchedRewardAds <= 0)
            {
                //Debug.Log("IsDailyChapterUnlocked false - value <= 0");
                return false;
            }

            //Debug.Log("IsDailyChapterUnlocked true");
            return true;
        }

        // unlock daily chapter if success return true else return false.
        public bool UnlockDailyChapter(string id)
        {
            String[] idSplits = id.Split('-');
            if (idSplits.Length <= 0)
            {
                Debug.Log("UnlockDailyChapter false - can't convert puzzle id");
                return false;
            }

            string key = DAILY_CHAPTER_PREFIX + idSplits[idSplits.Length - 1];
            string createdDate = GetChallengeCreationDate(id).ToShortDateString();
            if (!PlayerPrefs.HasKey(key))
            {
                Debug.Log("UnlockDailyChapter true - add new key");
                PlayerPrefs.SetString(key, createdDate + "_" + "1");
                onUnlockTournamentChapter();
                return true;
            }

            string[] valueSplits = PlayerPrefs.GetString(key).Split('_');
            if (valueSplits.Length != 2)
            {
                Debug.Log("UnlockDailyChapter true - replace value because can't convert re-value");
                PlayerPrefs.SetString(key, createdDate + "_" + "1");
                onUnlockTournamentChapter();
                return true;
            }

            if (!valueSplits[0].Equals(createdDate))
            {
                Debug.Log("UnlockDailyChapter true - replace value because the re-value is old value");
                PlayerPrefs.SetString(key, createdDate + "_" + "1");
                onUnlockTournamentChapter();
                return true;
            }

            int numberWatchedRewardAds = 0;
            if (!Int32.TryParse(valueSplits[1], out numberWatchedRewardAds))
            {
                Debug.Log("UnlockDailyChapter true - replace value before the re-value is old value");
                PlayerPrefs.SetString(key, createdDate + "_" + "1");
                onUnlockTournamentChapter();
                return true;
            }

            Debug.Log("UnlockDailyChapter true - increase re-value");
            PlayerPrefs.SetString(key, createdDate + "_" + (numberWatchedRewardAds + 1).ToString());
            onUnlockTournamentChapter();
            return true;
        }

        private string FakeDateCreation(string date)
        {
            string[] elements = date.Split('/');
            elements[0] = ((int)date[0] + 1).ToString();
            return string.Format("{0}/{1}/{2}", elements[0], elements[1], elements[2]);
        }
    }
}

