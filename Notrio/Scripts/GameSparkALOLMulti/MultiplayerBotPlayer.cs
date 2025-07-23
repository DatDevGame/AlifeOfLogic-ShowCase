using System;
using System.Collections;
using System.Collections.Generic;
using Takuzu;
using Takuzu.Generator;
using UnityEngine;

public class MultiplayerBotPlayer : MonoBehaviour
{
    internal static MultiplayerBotPlayer CreatePlayer(string id, Transform container, Action<string, Index2D, int, int> onPlayerPuzzleValueSet, Action<string> onPlayerPuzzleSolved, MultiplayerRoom room)
    {
        GameObject go = new GameObject();
        go.transform.SetParent(container);
        go.name = "bot";
        MultiplayerBotPlayer bot = go.AddComponent<MultiplayerBotPlayer>();
        bot.id = id;
        bot.onValueSet = onPlayerPuzzleValueSet;
        bot.onPuzzleSolved = onPlayerPuzzleSolved;
        bot.room = room;
        return bot;
    }
    public class SolvingTimeRandomRange
    {
        public float minimumUndoStepThinkingTime = 1;
        public float maximumUndoStepThinkingTime = 3;
        public float minimumParseStepThinkingTime = 1;
        public float maximumParseStepThinkingTime = 3;
        public float minimumLSDStepThinkingime = 1;
        public float maximumLSDStepThinkingTime = 15;
        public float minimumRamdomStepThinkingTime = 1;
        public float maximumRandomStepThinkingTime = 30;
        public float undoRate = 1;
    }
    private const string BOT_ACCEPT_REMATCH_MIN_TIME_KEY = "BotAcceptRematchMinTime";
    private float BotAcceptRematchMinTime
    {
        get { return (appConfigIsReady) ? CloudServiceManager.Instance.appConfig.GetFloat(BOT_ACCEPT_REMATCH_MIN_TIME_KEY) ?? 10 : 10; }
    }
    private const string BOT_ACCEPT_REMATCH_MAX_TIME_KEY = "BotAcceptRematchMaxTime";
    private float BotAcceptRematchMaxTime
    {
        get { return (appConfigIsReady) ? CloudServiceManager.Instance.appConfig.GetFloat(BOT_ACCEPT_REMATCH_MAX_TIME_KEY) ?? 60 : 60; }
    }
    private const string BOT_ACCEPT_RATE_CLOUD_KEY = "BotAcceptRate";
    private float BotAcceptRate
    {
        get { return (appConfigIsReady) ? CloudServiceManager.Instance.appConfig.GetFloat(BOT_ACCEPT_RATE_CLOUD_KEY) ?? 100 : 100; }
    }
    private bool appConfigIsReady
    {
        get { return CloudServiceManager.Instance != null && CloudServiceManager.Instance.appConfig != null; }
    }
    private Dictionary<int, SolvingTimeRandomRange> solvingTimeRandomRankDictionary = new Dictionary<int, SolvingTimeRandomRange>(){
        {1, new SolvingTimeRandomRange()},
        {2, new SolvingTimeRandomRange()},
        {3, new SolvingTimeRandomRange()},
        {4, new SolvingTimeRandomRange()},
        {5, new SolvingTimeRandomRange()},
    };
    private int skinIndex = 0;
    private string id;
    private Action<string, Index2D, int, int> onValueSet;
    private Action<string, Index2D> onValueUnset = null;
    private Action<string> onPuzzleSolved;
    private MultiplayerRoom room;
    private int currentSkillLevel = 5;
    private void Start()
    {
        MultiplayerSession.SessionStarted += OnSessionStarted;
        MultiplayerSession.SessionFinished += OnSessionFinished;
        room.aPlayerSolvedPuzzle += LocalPlayerSolved;
        skinIndex = GetRandomSkinIndex();
    }

    private const string multiplayerSkinRandomChancesCloudKey = "multiplayerSkinIndexRandomChances";


    private List<float> defaultMultiplayerskinRandomChances = null;

    private List<float> multiplayerSkinIndexRandomChances
    {
        get
        {
            List<float> defaultValue = new List<float>();

            if (CloudServiceManager.Instance != null && CloudServiceManager.Instance.appConfig != null && CloudServiceManager.Instance.appConfig.ContainsKey(multiplayerSkinRandomChancesCloudKey))
            {
                defaultValue = CloudServiceManager.Instance.appConfig.GetFloatList(multiplayerSkinRandomChancesCloudKey);
            }
            else
            {
                if (defaultMultiplayerskinRandomChances == null)
                {
                    defaultMultiplayerskinRandomChances = new List<float>();
                    for (int i = 0; i < SkinManager.Instance.availableSkin.Count; i++)
                    {
                        defaultMultiplayerskinRandomChances.Add(1);
                    }
                }
                defaultValue = defaultMultiplayerskinRandomChances;
            }

            return defaultValue;
        }
    }

    private int GetRandomSkinIndex()
    {
        float totalChances = 0;
        foreach (var item in multiplayerSkinIndexRandomChances)
        {
            totalChances += item;
        }
        float randomValue = UnityEngine.Random.Range(0, totalChances);
        int randomIndex = 0;

        float randomValueCount = 0;
        foreach (var item in multiplayerSkinIndexRandomChances)
        {
            randomValueCount += item;
            if (randomValue < randomValueCount)
            {
                break;
            }
            randomIndex++;
        }
        randomIndex = Mathf.Clamp(randomIndex, 0, SkinManager.Instance.availableSkin.Count - 1);
        return randomIndex;
    }

    void OnDestroy()
    {
        MultiplayerSession.SessionStarted -= OnSessionStarted;
        MultiplayerSession.SessionFinished -= OnSessionFinished;
        room.aPlayerSolvedPuzzle -= LocalPlayerSolved;
    }

    public void SetBotSkillLevel(int level)
    {
        currentSkillLevel = level;
    }

    private void LocalPlayerSolved()
    {
        List<byte> data = new List<byte>();
        data.Add((byte)MultiplayerDataHelper.MessageType.SessionFinished);
        room.ReceiveMessage(id, data.ToArray());
    }

    private void OnSessionFinished(bool obj)
    {
        StartCoroutine(DelayReadyCR());
    }

    private IEnumerator DelayReadyCR()
    {
        yield return new WaitForSeconds(UIReferences.Instance.overlayWinMenu.showDelayOnPuzzleSolved);
        yield return new WaitForSeconds(UnityEngine.Random.Range(BotAcceptRematchMinTime, BotAcceptRematchMaxTime));
        if (UnityEngine.Random.value * 100 < BotAcceptRate)
        {
            room.PlayerReady(id);
            Debug.Log("BOT Ready to play again");
        }
        else
        {
            MultiplayerManager.Instance.HandleFakePeerDisconnectedForBotMode(new string[] { id });
            Debug.Log("BOT have left room");
        }
    }

    private void OnSessionStarted()
    {
        StartCoroutine(BOTCR());
    }

    private IEnumerator BOTCR()
    {
        yield return new WaitForSeconds(1);
        //Send Random Offset
        SendRandomOffset();
        yield return new WaitForSeconds(0.5f);
        //Send Puzzle Loaded
        SendPuzzleLoaded();

        yield return new WaitUntil(() => GameManager.Instance.GameState == GameState.Playing);
        yield return new WaitForSeconds(2);

        //Start playing
        List<Index2D> botmoves = new List<Index2D>();

        List<float> minimumRamdomStepThinkingTimeCloud = CloudServiceManager.Instance.appConfig.GetFloatList("minimumRandomStepThinkingTime");
        List<float> minimunUndoStepThinkingTimeCloud = CloudServiceManager.Instance.appConfig.GetFloatList("minimumUndoStepThinkingTime");
        List<float> minimumParseStepThinkingTimeCloud = CloudServiceManager.Instance.appConfig.GetFloatList("minimumParseStepThinkingTime");
        List<float> minimumLSDStepThinkingTimeCloud = CloudServiceManager.Instance.appConfig.GetFloatList("minimumLSDStepThinkingTime");
        List<float> undoRateCloud = CloudServiceManager.Instance.appConfig.GetFloatList("undoRate");
        Debug.Log("minimumRamdomStepThinkingTimeCloud = " + String.Join("",
            minimumRamdomStepThinkingTimeCloud
            .ConvertAll(i => i.ToString())
            .ToArray()));
        List<float> maximumParseStepThinkingTimeCloud = CloudServiceManager.Instance.appConfig.GetFloatList("maximumParseStepThinkingTime");
        List<float> maximumLSDStepThinkingTimeCloud = CloudServiceManager.Instance.appConfig.GetFloatList("maximumLSDStepThinkingTime");
        List<float> maximumRandomStepThinkingTimeCloud = CloudServiceManager.Instance.appConfig.GetFloatList("maximumRandomStepThinkingTime");
        List<float> maximumUndoStepThinkingTimeCloud = CloudServiceManager.Instance.appConfig.GetFloatList("maximumUndoStepThinkingTime");
        Debug.Log("maximumParseStepThinkingTimeCloud = " + String.Join("",
                    maximumParseStepThinkingTimeCloud
                    .ConvertAll(i => i.ToString())
                    .ToArray()));
        while ((GameManager.Instance.GameState == GameState.Playing || GameManager.Instance.GameState == GameState.Paused))
        {
            float undoRate = undoRateCloud != null ? undoRateCloud[currentSkillLevel - 1] : solvingTimeRandomRankDictionary[currentSkillLevel].undoRate;
            bool solvedByParseStep = false;
            bool solvedByLSDStep = false;

            Index2D index = new Index2D(-1, -1);
            int value = -1;
            Puzzle p = PuzzleManager.Instance.GetPuzzleByIdIgnoreType(PuzzleManager.currentPuzzleId);
            List<Index2D> parseSteps = Solver.GetParsePuzzleIndex(GetupdatedPuzzleStr(p, botmoves));
            List<Index2D> lsdSteps = Solver.GetLSDIndexes(GetupdatedPuzzleStr(p, botmoves));
            if (parseSteps.Count != 0)
            {
                int randomIndex = 0;
                index = parseSteps[randomIndex];
                parseSteps.RemoveAt(randomIndex);
                if (!index.Equals(new Index2D(-1, -1)))
                {
                    solvedByParseStep = true;
                }
            }
            bool fakeUndo = UnityEngine.Random.value * 100 < undoRate && botmoves.Count > 0 && index.Equals(new Index2D(-1, -1));
            if (fakeUndo)
            {
                int randomMoveIndex = UnityEngine.Random.Range(0, botmoves.Count);
                index = botmoves[randomMoveIndex];
                botmoves.RemoveAt(randomMoveIndex);
            }
            if (index.Equals(new Index2D(-1, -1)) && lsdSteps.Count != 0)
            {
                int randomIndex = 0;
                index = lsdSteps[randomIndex];
                lsdSteps.RemoveAt(randomIndex);
                if (!index.Equals(new Index2D(-1, -1)))
                {
                    solvedByLSDStep = true;
                }
            }
            if (index.Equals(new Index2D(-1, -1)))
            {
                index = GoWithRandomMove(botmoves);
            }

            if (!index.Equals(new Index2D(-1, -1)) && fakeUndo == false)
            {
                try
                {
                    value = int.Parse(p.solution[index.row * (int)p.size + index.column].ToString());
                }
                catch (System.IndexOutOfRangeException e)
                {
                    //*** try to get solution value but the index is out of range return default value 0
                    Debug.LogWarning("BOT Get Solution out of range " + e.Message + " " + index.column + "," + index.row);
                    value = 0;
                }
            }

            List<byte> data = new List<byte>();
            if (index.Equals(new Index2D(-1, -1)))
            {
                //No more tile to play bot has solved the puzzle
                CloudServiceManager.Instance.GetCurrentServerTime(time =>
                {
                    data.Add((byte)MultiplayerDataHelper.MessageType.PuzzleSolved);
                    data.AddRange(room.dataHelper.getBytes(new MultiplayerDataHelper.OnPuzzleSolvedStruct()
                    {
                        timeStamp = time
                    }));
                    room.ReceiveMessage(id, data.ToArray());
                });
                break;
            }
            float minRand = minimumRamdomStepThinkingTimeCloud != null ? minimumRamdomStepThinkingTimeCloud[currentSkillLevel - 1] : solvingTimeRandomRankDictionary[currentSkillLevel].minimumRamdomStepThinkingTime;
            if (fakeUndo)
            {
                minRand = minimunUndoStepThinkingTimeCloud != null ? minimunUndoStepThinkingTimeCloud[currentSkillLevel - 1] : solvingTimeRandomRankDictionary[currentSkillLevel].maximumUndoStepThinkingTime;
                Debug.Log("Undo");
            }
            else
            {
                if (solvedByParseStep)
                {
                    minRand = minimumParseStepThinkingTimeCloud != null ? minimumParseStepThinkingTimeCloud[currentSkillLevel - 1] : solvingTimeRandomRankDictionary[currentSkillLevel].minimumParseStepThinkingTime;
                    Debug.Log("Solved by parse step");
                }
                if (solvedByLSDStep)
                {
                    minRand = minimumLSDStepThinkingTimeCloud != null ? minimumLSDStepThinkingTimeCloud[currentSkillLevel - 1] : solvingTimeRandomRankDictionary[currentSkillLevel].minimumLSDStepThinkingime;
                    Debug.Log("Solved by LSD step");
                }
            }

            float maxRand = maximumParseStepThinkingTimeCloud != null ? maximumRandomStepThinkingTimeCloud[currentSkillLevel - 1] : solvingTimeRandomRankDictionary[currentSkillLevel].maximumRandomStepThinkingTime;
            if (fakeUndo)
            {
                maxRand = maximumUndoStepThinkingTimeCloud != null ? maximumUndoStepThinkingTimeCloud[currentSkillLevel - 1] : solvingTimeRandomRankDictionary[currentSkillLevel].maximumUndoStepThinkingTime;
                Debug.Log("Undo");
            }
            else
            {
                if (solvedByParseStep)
                {
                    maxRand = maximumParseStepThinkingTimeCloud != null ? maximumParseStepThinkingTimeCloud[currentSkillLevel - 1] : solvingTimeRandomRankDictionary[currentSkillLevel].maximumParseStepThinkingTime;
                    Debug.Log("Solved by parse step");
                }
                if (solvedByLSDStep)
                {
                    maxRand = maximumLSDStepThinkingTimeCloud != null ? maximumLSDStepThinkingTimeCloud[currentSkillLevel - 1] : solvingTimeRandomRankDictionary[currentSkillLevel].maximumLSDStepThinkingTime;
                    Debug.Log("Solved by LSD step");
                }
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(minRand, maxRand));
            data.Add((byte)MultiplayerDataHelper.MessageType.CellValueSet);
            data.AddRange(room.dataHelper.getBytes(new MultiplayerDataHelper.OnCellValueSetStruct()
            {
                col = (short)index.column,
                row = (short)index.row,
                value = (short)value,
                skinIndex = (short)this.skinIndex
            }));
            room.ReceiveMessage(id, data.ToArray());

            if (fakeUndo == false)
                botmoves.Add(index);
        }
    }

    private string GetupdatedPuzzleStr(Puzzle p, List<Index2D> botmoves)
    {
        string puzzleStr = p.puzzle;
        char[] puzzleCharArray = puzzleStr.ToCharArray();
        foreach (var move in botmoves)
        {
            try
            {
                puzzleCharArray[move.row * ((int)p.size) + move.column] = p.solution[move.row * (int)p.size + move.column];
            }
            catch (System.IndexOutOfRangeException e)
            {
                //!! Try to update puzzle progress but something wrong happened and make the index out of range
                Debug.LogWarning(e.Message);
            }
        }
        puzzleStr = new string(puzzleCharArray);
        return puzzleStr;
    }

    private Index2D GoWithRandomMove(List<Index2D> botmoves)
    {
        return LogicalBoard.Instance.GetRandom(botmoves);
    }

    private void SendPuzzleLoaded()
    {
        List<byte> data = new List<byte>();
        data.Add((byte)MultiplayerDataHelper.MessageType.PuzzleLoaded);
        room.ReceiveMessage(id, data.ToArray());
    }

    private void SendRandomOffset()
    {
        short random = (short)UnityEngine.Random.Range(0, 500);
        Debug.Log("BOT RAND:: " + random);
        List<byte> data = new List<byte>();
        data.Add((byte)MultiplayerDataHelper.MessageType.PuzzleSelect);
        data.AddRange(room.dataHelper.getBytes(new MultiplayerDataHelper.PuzzleSelectStruct()
        {
            offset = random,
            skinIndex = (short)this.skinIndex
        }));
        room.ReceiveMessage(id, data.ToArray());
    }

    public static class Solver
    {
        public static List<Index2D> GetParsePuzzleIndex(string puzzleStr)
        {
            List<Index2D> results = new List<Index2D>();
            int size = (int)Math.Sqrt(puzzleStr.Length);

            string[][] puzzle = new string[size][];
            for (int row = 0; row < size; row++)
            {
                puzzle[row] = new string[size];
                for (int col = 0; col < size; col++)
                {
                    if (puzzleStr[row * size + col] == '.')
                        puzzle[row][col] = Puzzle.DEFAULT_VALUE;
                    if (puzzleStr[row * size + col] == '1')
                        puzzle[row][col] = Puzzle.VALUE_ONE;
                    if (puzzleStr[row * size + col] == '0')
                        puzzle[row][col] = Puzzle.VALUE_ZERO;
                }
            }

            string[][] result = new string[size][];
            for (int row = 0; row < size; row++)
            {
                result[row] = new string[size];
                for (int col = 0; col < size; col++)
                {
                    result[row][col] = Puzzle.DEFAULT_VALUE;
                }
            }
            List<Index2D> givenIndexes = new List<Index2D>();
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    string val = puzzle[row][col];
                    if ((val.Equals(Puzzle.VALUE_ZERO)) || (val.Equals(Puzzle.VALUE_ONE)))
                    {
                        givenIndexes.Add(new Index2D(row, col));
                    }
                }
            }
            while (givenIndexes.Count != 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, givenIndexes.Count);
                Index2D pos = givenIndexes[randomIndex];
                string val = puzzle[pos.row][pos.column];
                if ((val.Equals(Puzzle.VALUE_ZERO)) || (val.Equals(Puzzle.VALUE_ONE)))
                {
                    AssignCellValue(result, pos.row, pos.column, val, false, results);
                }
                givenIndexes.RemoveAt(randomIndex);
            }

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (puzzle[row][col] != Puzzle.DEFAULT_VALUE)
                    {
                        int indexOfRC = results.IndexOf(new Index2D(row, col));
                        if (indexOfRC != -1)
                        {
                            results.RemoveAt(indexOfRC);
                        }
                    }
                }
            }

            return results;
        }

        public static List<Index2D> GetLSDIndexes(string puzzleStr)
        {
            List<Index2D> results = new List<Index2D>();
            int size = (int)Math.Sqrt(puzzleStr.Length);

            string[][] puzzle = new string[size][];
            for (int row = 0; row < size; row++)
            {
                puzzle[row] = new string[size];
                for (int col = 0; col < size; col++)
                {
                    if (puzzleStr[row * size + col] == '.')
                        puzzle[row][col] = Puzzle.DEFAULT_VALUE;
                    if (puzzleStr[row * size + col] == '1')
                        puzzle[row][col] = Puzzle.VALUE_ONE;
                    if (puzzleStr[row * size + col] == '0')
                        puzzle[row][col] = Puzzle.VALUE_ZERO;
                }
            }

            string[][] result = new string[size][];
            for (int row = 0; row < size; row++)
            {
                result[row] = new string[size];
                for (int col = 0; col < size; col++)
                {
                    result[row][col] = Puzzle.DEFAULT_VALUE;
                }
            }
            List<Index2D> givenIndexes = new List<Index2D>();
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    string val = puzzle[row][col];
                    if ((val.Equals(Puzzle.VALUE_ZERO)) || (val.Equals(Puzzle.VALUE_ONE)))
                    {
                        givenIndexes.Add(new Index2D(row, col));
                    }
                }
            }
            while (givenIndexes.Count != 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, givenIndexes.Count);
                Index2D pos = givenIndexes[randomIndex];
                string val = puzzle[pos.row][pos.column];
                if ((val.Equals(Puzzle.VALUE_ZERO)) || (val.Equals(Puzzle.VALUE_ONE)))
                {
                    AssignCellValue(result, pos.row, pos.column, val, false, results);
                }
                givenIndexes.RemoveAt(randomIndex);
            }
            results.Clear();
            int numberOfSolvedCells = 0;
            ApplyLsd(result, false, ref numberOfSolvedCells, true, results);
            ApplyLsd(result, true, ref numberOfSolvedCells, true, results);
            return results;
        }

        private static bool ApplyLsd(string[][] values, bool useAdvancedTechnique, ref int numOfSolvedCells, bool silent, List<Index2D> assignedList = null)
        {
            if (!silent)
                Helper.LogFormat("Now applying {0} technique...", useAdvancedTechnique ? "ALSD" : "LSD");

            int numOfSolvedCellsBefore = Validator.CountKnownCells(values, Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE);
            int size = values.Length;
            int result = DoApplyLsdToGrid(values, useAdvancedTechnique, silent, assignedList); // DoApplyLsdToGrid will call itself recursively
            bool success = (result < 0) ? false : true;

            if (!success)
            {
                if (!silent)
                    Helper.LogFormat("Invalid puzzle: failed applying {0} technique.", useAdvancedTechnique ? "ALSD" : "LSD");
            }
            else
            {
                int numOfSolvedCellsAfter = Validator.CountKnownCells(values, Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE);
                numOfSolvedCells = numOfSolvedCellsAfter - numOfSolvedCellsBefore;

                if (!silent)
                {
                    Helper.LogFormat("===========================================================");
                    Helper.LogFormat("Finished applying {0} technique on puzzle. Result: {1}", useAdvancedTechnique ? "ALSD" : "LSD", success ? "Success" : "Fail");
                    Helper.LogFormat("Grid after applying this technique:");
                    Helper.PrintGrid(values);
                    Helper.LogFormat("===========================================================");
                    Helper.LogFormat("Number of cells solved by this technique:\t{0:D}", numOfSolvedCells);
                    Helper.LogFormat("Total cells solved after applying this technique:\t{0:D} ({1:P})", numOfSolvedCellsAfter, (float)numOfSolvedCells / (size * size));
                    Helper.LogFormat("===========================================================");
                }
            }

            return success;
        }

        private static int DoApplyLsdToGrid(string[][] values, bool useAdvancedTechnique, bool silent, List<Index2D> assignedList = null)
        {
            int resultCode = 0;

            // Loop thru all cells on the diagonal of the grid, examine the row and column
            // crossing at each cell to find the line has only one slot left for either value
            // and perform the LSD technique on that line.
            // Get row and column arrays of the cell.
            int size = values.Length;
            string[] testValues = { Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE };

            for (int i = 0; i < size; i++)
            {
                // Get the row i and column i and count the occurences of
                // each value within that line.
                // First examine the row number i.
                string[] rowArray = Helper.GetRow(values, i);
                int targetC = -1;
                string failedRowValue = null;

                for (int j = 0; j < testValues.Length; j++)
                {
                    string testValue = testValues[j];

                    if (!useAdvancedTechnique)
                    {
                        targetC = ApplyLsdToOneLine(rowArray, testValue);
                    }
                    else
                    {
                        targetC = ApplyAlsdToOneLine(rowArray, testValue);
                    }

                    if (targetC >= 0)
                    {
                        failedRowValue = testValue;
                        break;
                    }
                }

                if (targetC >= 0)
                {
                    string targetRowValue = (failedRowValue.Equals(Puzzle.VALUE_ZERO)) ? Puzzle.VALUE_ONE : Puzzle.VALUE_ZERO;
                    if (!silent)
                    {
                        Helper.LogFormat("Checking row " + (i + 1));
                        Helper.LogFormat("Assigning cell {0:D},{1:D} with value {2}...", i + 1, targetC + 1, targetRowValue);
                    }
                    bool success = AssignCellValue(values, i, targetC, targetRowValue, false, assignedList);

                    if (success)
                    {
                        resultCode = 1;
                    }
                    else
                    {
                        resultCode = -1;
                        break;
                    }
                }

                string[] colArray = Helper.GetColumn(values, i);
                int targetR = -1;
                string failedColValue = null;

                for (int k = 0; k < testValues.Length; k++)
                {
                    string testValue = testValues[k];

                    if (!useAdvancedTechnique)
                    {
                        targetR = ApplyLsdToOneLine(colArray, testValue);
                    }
                    else
                    {
                        targetR = ApplyAlsdToOneLine(colArray, testValue);
                    }

                    if (targetR >= 0)
                    {
                        failedColValue = testValue;
                        break;
                    }
                }

                if (targetR >= 0)
                {
                    string targetColValue = (failedColValue.Equals(Puzzle.VALUE_ZERO)) ? Puzzle.VALUE_ONE : Puzzle.VALUE_ZERO;
                    if (!silent)
                    {
                        Helper.LogFormat("Checking column " + (i + 1));
                        Helper.LogFormat("Assigning cell {0:D},{1:D} with value {2}...", targetR + 1, i + 1, targetColValue);
                    }
                    bool success = AssignCellValue(values, targetR, i, targetColValue, false, assignedList);

                    if (success)
                    {
                        resultCode = 1;
                    }
                    else
                    {
                        resultCode = -1;
                        break;
                    }
                }
            }

            // Repeat the process if a there's at least one cell has been assigned with a new value.
            if (resultCode > 0)
            {
                resultCode = DoApplyLsdToGrid(values, useAdvancedTechnique, silent);
            }

            return resultCode;
        }

        private static int ApplyLsdToOneLine(string[] line, string testValue)
        {
            if (!(testValue.Equals(Puzzle.VALUE_ZERO) || testValue.Equals(Puzzle.VALUE_ONE)))
            {
                throw new ArgumentException("Invalid testValue.");
            }

            int result = -1;
            string otherValue = (testValue.Equals(Puzzle.VALUE_ZERO)) ? Puzzle.VALUE_ONE : Puzzle.VALUE_ZERO;
            int occurencesOfTestValue = Validator.CountOccurrences(line, testValue);
            int occurencesOfOtherValue = Validator.CountOccurrences(line, otherValue);
            int size = line.Length;

            if (((size / 2 - occurencesOfTestValue) == 1) && ((size / 2 - occurencesOfOtherValue) >= 2))
            {
                // If the line satisfies the requirements of the technique, keep going: try assign one of
                // the empty cell with the testValue, and all remaining cells with the otherValue to
                // see if contradiction occurs in the line.
                // If yes, assign that cell with the otherValue.
                // If no, repeat the assigning trial with the next cell.                
                for (int c = 0; c < line.Length; c++)
                {
                    if (line[c].Equals(Puzzle.DEFAULT_VALUE))
                    {
                        // Create a copy of the being examined line and try assigning values on it.
                        // Note that a shallow copy is enough, as we'll only alter the elements of this
                        // copy with new strings and test it, there's no point creating new string objects 
                        string[] lineCopy = (string[])line.Clone();

                        lineCopy[c] = testValue;
                        for (int c2 = 0; c2 < lineCopy.Length; c2++)
                        {
                            if (lineCopy[c2].Equals(Puzzle.DEFAULT_VALUE) && (c2 != c))
                            {
                                lineCopy[c2] = otherValue;
                            }
                        }

                        // Now examine the filled line to see if contradiction occurred.
                        // If there's contradiction, LSD has succeeded and the value of the
                        // cell at position c is determined to be otherValue.
                        bool valid = Validator.ValidateLine(lineCopy, Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE);   // Actually just using Validator.TripletRuleCheck will suffice.
                        if (!valid)
                        {
                            result = c;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Applies ALSD (Advanced Last Square Deduction) technique on the given row or column.
        /// </summary>
        /// <returns>the index of the cell within the line that causes contradiction when assigned the test value, -1 if the technique doesn't apply.</returns>
        /// <param name="line">the array of row/column values.</param>
        /// <param name="testValue">the value to be tested.</param>
        private static int ApplyAlsdToOneLine(string[] line, string testValue)
        {
            if (!(testValue.Equals(Puzzle.VALUE_ZERO) || testValue.Equals(Puzzle.VALUE_ONE)))
            {
                throw new ArgumentException("Invalid testValue.");
            }

            int result = -1;
            string otherValue = (testValue.Equals(Puzzle.VALUE_ZERO)) ? Puzzle.VALUE_ONE : Puzzle.VALUE_ZERO;
            int occurencesOfTestValue = Validator.CountOccurrences(line, testValue);
            int occurencesOfOtherValue = Validator.CountOccurrences(line, otherValue);
            int size = line.Length;

            // Search for pairs of cells that contain two different values according to "Different Two" technique.
            // We wil considered these pairs as "filled" with two different values, regardless of the actual order of their cells.
            // We will ignore these cells when filling and validating the line.
            List<int> excludedCells = new List<int>();

            for (int i = 0; i <= line.Length - 4; i++)
            {
                int one, two, three, four;
                string cellOne, cellTwo, cellThree, cellFour;

                one = i;
                two = i + 1;
                three = i + 2;
                four = i + 3;
                cellOne = line[one];
                cellTwo = line[two];
                cellThree = line[three];
                cellFour = line[four];

                if (!cellOne.Equals(Puzzle.DEFAULT_VALUE) && !cellFour.Equals(Puzzle.DEFAULT_VALUE) && !cellOne.Equals(cellFour))
                {
                    if (cellTwo.Equals(cellThree) && cellTwo.Equals(Puzzle.DEFAULT_VALUE))
                    {
                        excludedCells.Add(two);
                        excludedCells.Add(three);
                    }
                }
            }

            // Now proceed with the technique.
            // First adjust the number of occurences to reflect the pairs with determined values and were excluded.
            occurencesOfTestValue += excludedCells.Count / 2;
            occurencesOfOtherValue += excludedCells.Count / 2;

            // Now proceed as the normal LSD technique with the note that those cells in the excludedCells won't be filled and validated.
            // Also there's a an addition case when there's only one empty cell left in the line after excluding.
            if (((size / 2 - occurencesOfTestValue) == 1) && ((size / 2 - occurencesOfOtherValue) >= 2))
            {
                for (int c = 0; c < line.Length; c++)
                {
                    if (line[c].Equals(Puzzle.DEFAULT_VALUE) && !excludedCells.Contains(c))
                    {
                        // Create a copy of the being examined line and try assigning values on it.
                        // Note that a shallow copy is enough, as we'll only alter the elements of this
                        // copy with new string references and test it, there's no point creating new string objects
                        string[] lineCopy = (string[])line.Clone();

                        lineCopy[c] = testValue;
                        for (int c2 = 0; c2 < lineCopy.Length; c2++)
                        {
                            if (lineCopy[c2].Equals(Puzzle.DEFAULT_VALUE) && !excludedCells.Contains(c2) && (c2 != c))
                            {
                                lineCopy[c2] = otherValue;
                            }
                        }

                        // Since the line actually has some holes now, validateLine() will certainly failed,
                        // but for the purpose of this test, triplet rule check is the more suitable and perfectly
                        // sufficient one to use.
                        bool valid = Validator.TripletRuleCheck(lineCopy, Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE);
                        if (!valid)
                        {
                            result = c;
                            break;
                        }
                    }
                }
            }
            else if (((size / 2 - occurencesOfTestValue) == 0) && ((size / 2 - occurencesOfOtherValue) >= 1))
            {
                // This is a special case when after excluding the line only has one slot free.
                // And if we apply the targetValue to this empty cell contradiction will occur.
                for (int p = 0; p < line.Length; p++)
                {
                    if (line[p].Equals(Puzzle.DEFAULT_VALUE) && !excludedCells.Contains(p))
                    {
                        result = p;
                        break;
                    }
                }
            }

            return result;
        }

        private static bool Search(string[][] values, List<string[][]> solutions, int numOfSolutions, ref long totalTrials, ref long failedTrials, bool silent)
        {
            bool finish = false;
            int size = values.Length;

            // Check if the puzzle is solved.
            bool solved = true;

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (values[row][col].Length != 1)   // or use values[row][col].Equals(Puzzle.DEFAULT_VALUE) which is probably slower.
                    {
                        solved = false;
                        break;
                    }
                }
            }



            if (solved)
            {
                solved = Validator.UniqueRowCheck<string>(values) && Validator.UniqueColumnCheck<string>(values);

                if (solved)
                {
                    // Add solution to the list, no need to create a copy of values 
                    // since itself is a copy created in the previous step.
                    solutions.Add(values);

                    // Done if we've found the required number of solutions.
                    int num = solutions.Count;

                    if (!silent)
                        Helper.LogFormat("Num of solutions found: " + num);

                    if (num == numOfSolutions)
                        finish = true;
                }
            }
            else
            {
                // If not solved, do searching.
                // Search strategy: (note that a good strategy is one that can help detect contradiction as fast as possible)
                // First determine the row/column with fewest available places for one
                // particular value (in other words, find the row/column that has the most cells assigned with
                // that value) then start the search by assigning that value (we call targetValue) 
                // to a random unsolved cell in that row/column. Backtracking is performed if contradiction found.

                // Iterate through all cells on the diagonal of the values grid, examine the 
                // row and column crossing at each cell.
                int minAvailablePlaces = -1;
                int targetRow = -1, targetCol = -1;
                string targetValue = string.Empty;

                // Loop through the grid diagonal.
                for (int i = 0; i < size; i++)
                {
                    // Check row i.
                    int rowPlacesForZero = size / 2;
                    int rowPlacesForOne = size / 2;
                    int rowUnsolvedCellIndex = -1;

                    for (int c = 0; c < size; c++)
                    {
                        if (values[i][c].Equals(Puzzle.VALUE_ZERO))
                            rowPlacesForZero--;  // count remained places for each value in row i.
                        else if (values[i][c].Equals(Puzzle.VALUE_ONE))
                            rowPlacesForOne--;
                        else
                            rowUnsolvedCellIndex = c;   // index of the last unsolved cell in row i.
                    }

                    // Check column i.
                    int colPlacesForZero = size / 2;
                    int colPlacesForOne = size / 2;
                    int colUnsolvedCellIndex = -1;

                    for (int r = 0; r < size; r++)
                    {
                        if (values[r][i].Equals(Puzzle.VALUE_ZERO))
                            colPlacesForZero--;  // count remained places for each value in column i.
                        else if (values[r][i].Equals(Puzzle.VALUE_ONE))
                            colPlacesForOne--;
                        else
                            colUnsolvedCellIndex = r;   // index of the last unsolved cell in column i.
                    }

                    // Ugly code below.
                    if ((rowPlacesForZero > 0) && ((rowPlacesForZero < minAvailablePlaces) || (minAvailablePlaces < 0)))
                    {
                        minAvailablePlaces = rowPlacesForZero;
                        targetRow = i;
                        targetCol = rowUnsolvedCellIndex;
                        targetValue = Puzzle.VALUE_ZERO;
                    }

                    if ((rowPlacesForOne > 0) && ((rowPlacesForOne < minAvailablePlaces) || (minAvailablePlaces < 0)))
                    {
                        minAvailablePlaces = rowPlacesForOne;
                        targetRow = i;
                        targetCol = rowUnsolvedCellIndex;
                        targetValue = Puzzle.VALUE_ONE;
                    }

                    if ((colPlacesForZero > 0) && ((colPlacesForZero < minAvailablePlaces) || (minAvailablePlaces < 0)))
                    {
                        minAvailablePlaces = colPlacesForZero;
                        targetRow = colUnsolvedCellIndex;
                        targetCol = i;
                        targetValue = Puzzle.VALUE_ZERO;
                    }

                    if ((colPlacesForOne > 0) && ((colPlacesForOne < minAvailablePlaces) || (minAvailablePlaces < 0)))
                    {
                        minAvailablePlaces = colPlacesForOne;
                        targetRow = colUnsolvedCellIndex;
                        targetCol = i;
                        targetValue = Puzzle.VALUE_ONE;
                    }
                }

                // Now try assigning targetValue to the cell at targetRow, targetCol.
                // If contradiction found, try again with the other value (backtracking).
                string otherValue = string.Equals(targetValue, Puzzle.VALUE_ZERO) ? Puzzle.VALUE_ONE : Puzzle.VALUE_ZERO;
                string[] trialValues = { targetValue, otherValue };

                for (int j = 0; j < trialValues.Length; j++)
                {
                    totalTrials++;     // count number of trials for analysis.

                    // Create a copy of values for trials. Note that we need to use Helper.deepCopyOfGrid() to
                    // actually create a new 2D array "instance". Arrays.copyOf() won't work here since
                    // values is a 2D array, and Arrays.copyOf() will only copy the reference of each "row" of 
                    // values to valuesCopy, resulting in values and valuesCopy share same memory location for
                    // their "rows", in other words, they point to same "row" arrays). Consequently,
                    // alter valuesCopy will also affect values. Helper.deepCopyOfGrid() can solve this issue.
                    string[][] valuesCopy = Helper.DeepCopyJaggedArray(values);
                    bool success = AssignCellValue(valuesCopy, targetRow, targetCol, trialValues[j], true);
                    if (success)
                    {
                        finish = Search(valuesCopy, solutions, numOfSolutions, ref totalTrials, ref failedTrials, silent);     // continue searching if no contradiction found.
                        if (finish)
                            break;
                    }
                    else
                    {
                        failedTrials++;    // count number of failed trials for analysis.
                    }
                }
            }

            return finish;
        }

        private static bool FillTriplet(string[][] values, int[][] tripletIndices, bool silent = true, List<Index2D> assignedList = null)
        {
            int firstRow = tripletIndices[0][0];
            int firstCol = tripletIndices[0][1];
            int middleRow = tripletIndices[1][0];
            int middleCol = tripletIndices[1][1];
            int lastRow = tripletIndices[2][0];
            int lastCol = tripletIndices[2][1];

            string first = values[firstRow][firstCol];
            string middle = values[middleRow][middleCol];
            string last = values[lastRow][lastCol];

            string[] triplet = { first, middle, last };
            bool valid = Validator.TripletRuleCheck(triplet, Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE);

            if (!valid)
            {
                if (!silent)
                    Helper.LogFormat("Contradiction: 3-in-a-row detected at row {0:D}, column {1:D}.", middleRow, middleCol);
                return false;
            }

            // Construct a combined string of three triplet cells' values for ease of pattern detection.
            // First replace any default values ("01") with dot "." to match with the patterns style.
            if (first.Equals(Puzzle.DEFAULT_VALUE))
                first = Puzzle.DOT;

            if (middle.Equals(Puzzle.DEFAULT_VALUE))
                middle = Puzzle.DOT;

            if (last.Equals(Puzzle.DEFAULT_VALUE))
                last = Puzzle.DOT;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(first).Append(middle).Append(last);

            string combined = sb.ToString();

            foreach (string p1 in Puzzle.CONFIRMED_PATTERNS_1)
            {
                if (combined.Equals(p1))
                {
                    int row, col;
                    if (first.Equals(Puzzle.DOT))
                    {
                        row = firstRow;
                        col = firstCol;
                    }
                    else if (middle.Equals(Puzzle.DOT))
                    {
                        row = middleRow;
                        col = middleCol;
                    }
                    else
                    {
                        row = lastRow;
                        col = lastCol;
                    }

                    bool success = AssignCellValue(values, row, col, Puzzle.VALUE_ZERO, false, assignedList);

                    if (!success)
                        return false;
                }
            }

            foreach (string p2 in Puzzle.CONFIRMED_PATTERNS_2)
            {
                if (combined.Equals(p2))
                {
                    int row, col;
                    if (first.Equals(Puzzle.DOT))
                    {
                        row = firstRow;
                        col = firstCol;
                    }
                    else if (middle.Equals(Puzzle.DOT))
                    {
                        row = middleRow;
                        col = middleCol;
                    }
                    else
                    {
                        row = lastRow;
                        col = lastCol;
                    }

                    bool success = AssignCellValue(values, row, col, Puzzle.VALUE_ONE, false, assignedList);

                    if (!success)
                        return false;
                }
            }

            // If we reach here no contradiction found.
            return true;
        }

        private static bool AssignCellValue(string[][] values, int row, int col, string val, bool validateColumnUnique = false, List<Index2D> assignedList = null)
        {
            if (!(val.Equals(Puzzle.VALUE_ZERO) || val.Equals(Puzzle.VALUE_ONE)))
            {
                throw new ArgumentException("Invalid targetValue.");
            }

            string otherVal = (val.Equals(Puzzle.VALUE_ZERO)) ? Puzzle.VALUE_ONE : Puzzle.VALUE_ZERO;

            // Eliminate otherVal from cell's string.
            int index = values[row][col].IndexOf(otherVal);
            System.Text.StringBuilder sb = new System.Text.StringBuilder(values[row][col]);

            if (index > -1)
            {
                values[row][col] = sb.Remove(index, 1).ToString();
            }

            if (values[row][col].Length == 0)
            {
                return false;   // contradiction: removed last value.
            }

            // Get row and column arrays of the cell.
            string[] rowArray = Helper.GetRow(values, row);
            string[] colArray = Helper.GetColumn(values, col);

            int rowOccurrences = Validator.CountOccurrences(rowArray, val);
            int colOccurrences = Validator.CountOccurrences(colArray, val);

            if ((rowOccurrences > rowArray.Length / 2) || (colOccurrences > colArray.Length / 2))
            {
                return false;   // contradiction: exceed allowed number of occurrences of a value.
            }


            // Now fill other cells by constraint propagation.
            // 1st RULE: if the number of occurrences of one value in a row/column reaches puzzleSize/2,
            // fill all unfilled cells in that row/column with the other value.
            if (rowOccurrences == rowArray.Length / 2)
            {
                for (int c = 0; c < rowArray.Length; c++)
                {
                    if ((c != col) && (rowArray[c].Length > 1))
                    {
                        bool success = AssignCellValue(values, row, c, otherVal, false, assignedList);     // assign other unfilled cells in the row with otherVal.
                        if (!success)
                            return false;
                    }
                }
            }

            if (colOccurrences == colArray.Length / 2)
            {
                for (int r = 0; r < colArray.Length; r++)
                {
                    if ((r != row) && (colArray[r].Length > 1))
                    {
                        bool success = AssignCellValue(values, r, col, otherVal, false, assignedList);     // assign other unfilled cells in the column with otherVal.
                        if (!success)
                            return false;
                    }
                }
            }

            // 2nd RULE: if any group of 3 consecutive cells (triplet) has 2 cells of same value, 
            // then the other cell must have different value.
            List<int[][]> triplets = Helper.GetTripletsIndices(values.Length, row, col);
            foreach (int[][] triplet in triplets)
            {
                bool success = FillTriplet(values, triplet, true, assignedList);
                if (!success)
                    return false;     // contradiction: 3-in-a-row detected.
            }

            if (validateColumnUnique && !Validator.UniqueColumnCheck<string>(values, Puzzle.DEFAULT_VALUE))
            {
                //UnityEngine.Debug.LogError("Failed here boy");
                ////UnityEngine.Debug.Log(Helper.PuzzleStringGridToString(values));
                //Helper.PrintGrid(values);
                //Helper.LogFormat("====================");
                return false;
            }

            // If we reach here no contradiction found.
            if (assignedList != null)
            {
                if (!assignedList.Contains(new Index2D(row, col)))
                {
                    assignedList.Add(new Index2D(row, col));
                }
            }
            return true;
        }
    }
}
