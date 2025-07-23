using System;
using System.Collections;
using System.Collections.Generic;
using Takuzu;
using Takuzu.Generator;
using UnityEngine;

public class MultiplayerSession : MonoBehaviour
{

    internal static MultiplayerSession CreateSession(Transform container, string localId, List<string> playerIds, Action<int, int> randomOffset, Action puzzleLoaded, Action finished)
    {
        GameObject go = new GameObject();
        go.name = "Session";
        go.transform.SetParent(container);
        MultiplayerSession session = go.AddComponent<MultiplayerSession>();
        foreach (var playerId in playerIds)
        {
            session.PlayerSessionDatas.Add(playerId, new PlayerSessionData());
        }
        session.RandomOffsetSet = randomOffset;
        session.puzzleLoaded = puzzleLoaded;
        session.finished = finished;
        session.localId = localId;
        return session;
    }
    public static MultiplayerSession Instance;
    public static void RevealNextPlayer()
    {
        if (Instance == null)
            return;

        Instance.InsatnceRevealNextPlayer();
    }

    private class PlayerSessionData
    {
        public int randomPuzzleOffset = 0;
        public bool puzzleLoaded = false;
        public Dictionary<Index2D, int> moves = new Dictionary<Index2D, int>();
        public bool finished = false;
        public bool solved = false;
        public double solvedTime = -1;
        public int playerSkinIndex = 0;
    }
    public static Action<bool> SessionFinished = delegate { };
    public static Action SessionStarted = delegate { };
    public static bool playerWin = false;
    public static bool sessionFinished = false;
    private Dictionary<string, PlayerSessionData> PlayerSessionDatas = new Dictionary<string, PlayerSessionData>();
    private Action<int, int> RandomOffsetSet;
    private Action puzzleLoaded;
    private Action finished;
    private string localId;
    private string puzzleId;
    private Puzzle multiplayerPuzzle;
    private int countMultiplayerCellNeedFill;
    public bool gameStarted = false;
    private int currentPlayerIndex = 0;
    private bool isCurrentPlayerViewProgress = true;
    public bool IsCurrentPlayerViewProgress
    {
        get
        {
            return isCurrentPlayerViewProgress;

        }
        private set
        {
            isCurrentPlayerViewProgress = value;
        }
    }

    private void InsatnceRevealNextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % PlayerSessionDatas.Count;
        Debug.Log("Reveal next player" + currentPlayerIndex);
        if (!VisualBoard.Instance.IsInit())
            return;
        UpdateCurrentViewBoard(currentPlayerIndex);
    }
    private int lastUpdateIndex = -1;
    private void UpdateCurrentViewBoard(int index)
    {
        PlayerSessionData current = GetPlayerDataFromIndex(PlayerSessionDatas, index);
        if (current == null)
            return;

        if (current == PlayerSessionDatas[localId])
        {
            if (lastUpdateIndex != index)
            {
                VisualBoard.Instance.ClearInActiveCellsImmediately();
                VisualBoard.Instance.ShowAllSetCells();
                InputHandler.Instance.hideCursorForOpponentView = false;
                IsCurrentPlayerViewProgress = true;
                LogicalBoard.Instance.EnableInput();
                Powerup.Instance.EnablepowerUp();
                SkinManager.ResetSkinToCurrentActive();
            }
        }
        else
        {
            InputHandler.Instance.hideCursorForOpponentView = true;
            IsCurrentPlayerViewProgress = false;
            if (lastUpdateIndex != index)
            {
                VisualBoard.Instance.HideAllSetCells();
                LogicalBoard.Instance.DisableInput();
                Powerup.Instance.DisablePowerUp();
            }
            foreach (var k in current.moves.Keys)
            {
                if (current.moves[k] == 0 || current.moves[k] == 1)
                    VisualBoard.Instance.SetInActive(k);
                else
                {
                    VisualBoard.Instance.SetActive(k);
                }
            }
            SkinManager.SetSkinTemporary(current.playerSkinIndex);
        }
        lastUpdateIndex = index;
    }

    private void RevealLocalPlayerBoard()
    {
        int index = GetIndexFromPlayerId(PlayerSessionDatas, localId);
        if (index == -1)
            return;
        PlayerSessionData current = GetPlayerDataFromIndex(PlayerSessionDatas, index);

        VisualBoard.Instance.ClearInActiveCellsImmediately();
        VisualBoard.Instance.ShowAllSetCells();
        InputHandler.Instance.hideCursorForOpponentView = false;
        IsCurrentPlayerViewProgress = true;
        LogicalBoard.Instance.EnableInput();
        Powerup.Instance.EnablepowerUp();
        SkinManager.ResetSkinToCurrentActive();

        lastUpdateIndex = index;
    }

    private int GetIndexFromPlayerId(Dictionary<string, PlayerSessionData> dict, string id)
    {
        int i = 0;
        foreach (var item in dict.Keys)
        {
            if (item == id)
            {
                return i;
            }
            i++;
        }
        return -1;
    }

    private PlayerSessionData GetPlayerDataFromIndex(Dictionary<string, PlayerSessionData> dict, int index)
    {
        int i = 0;
        foreach (var value in dict)
        {
            if (i == index)
            {
                return value.Value;
            }
            i++;
        }
        return null;
    }

    private int GetIndexFromPlayerId(string localId)
    {
        int i = 0;
        foreach (var player in PlayerSessionDatas)
        {
            if (player.Key == localId)
            {
                return i;
            }
            i++;
        }
        return 0;
    }

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LogicalBoard.onPuzzleSolved += OnPuzzleSolved;

        playerWin = false;
        sessionFinished = false;
        isCurrentPlayerViewProgress = true;
        currentPlayerIndex = GetIndexFromPlayerId(localId);
        StartSession();
        SessionStarted();
    }

    void OnDestroy()
    {
        LogicalBoard.onPuzzleSolved -= OnPuzzleSolved;

        StopWaitPlayerRespondFinished();
        InputHandler.Instance.hideCursorForOpponentView = false;
        StopSession();
        ResolveLocalResult();
    }

    void OnPuzzleSolved()
    {
        StartWaitPlayerRespondFinished();
    }

    private Coroutine sessionCR;
    public void StartSession()
    {
        if (sessionCR != null)
            StopCoroutine(sessionCR);
        sessionCR = StartCoroutine(SessionCR());
    }

    public void StopSession()
    {
        if (sessionCR != null)
            StopCoroutine(sessionCR);
    }

    private IEnumerator SessionCR()
    {
        //Lock Player intereaction here
        yield return new WaitForSeconds(1);
        RandomOffsetSet(UnityEngine.Random.Range(0, 200), SkinManager.Instance.currentActivatedSkinIndex);
        yield return new WaitUntil(() => WaitForAllPlayerOffset());
        LoadPuzzle();
        yield return new WaitUntil(() => WaitForAllPlayerLoadPuzzle());
        gameStarted = true;
        StartGame();
        yield return null;
        RevealLocalPlayerBoard();
        //Unlock player intereaction here
        yield return new WaitUntil(() => WaitUntilAllPlayerFinished());
        if (LogicalBoard.Instance.IsPuzzleSolved())
            VisualBoard.Instance.HidePuzzleDelay(VisualBoard.Instance.puzzleSolvedHideDelay + 2.5f);
        Destroy(gameObject);
    }

    private Coroutine waitRespondCR;
    public void StartWaitPlayerRespondFinished()
    {
        if (waitRespondCR != null)
            StopCoroutine(waitRespondCR);
        waitRespondCR = StartCoroutine(CR_WaitPlayerRespondFinished());
    }

    public void StopWaitPlayerRespondFinished()
    {
        if (waitRespondCR != null)
            StopCoroutine(waitRespondCR);
    }

    IEnumerator CR_WaitPlayerRespondFinished()
    {
        float t = 0;
        while (!WaitUntilAllPlayerFinished() && t < 5.1f)
        {
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        if (LogicalBoard.Instance.IsPuzzleSolved())
            VisualBoard.Instance.HidePuzzleDelay(VisualBoard.Instance.puzzleSolvedHideDelay + 2.5f);
        Destroy(gameObject);
    }

    private void GetMultiplayerPuzzle(int offset, Action<string> callback)
    {
        int size = 12;
        int level = 3;
        List<int> validSizes = new List<int>() { 6, 8, 10, 12 };

        if (MultiplayerRoom.Instance != null)
        {
            //level =  offset % ((int)MultiplayerRoom.Instance.currentPuzzleLevel) + 1;
            //size = validSizes[offset %(validSizes.IndexOf(MultiplayerRoom.Instance.currentPuzzleSize) + 1)];
            level = MultiplayerRoom.Instance.currentPuzzleLevel;
            size = MultiplayerRoom.Instance.currentPuzzleSize;
        }

        CloudServiceManager.Instance.GetMultiPlayerPuzzle(level, size, offset, (puzzleGSData) =>
         {
             if (puzzleGSData == null)
             {
                 callback(null);
                 return;
             }
             Debug.Log(puzzleGSData.JSON);
             string creationTime = DateTime.UtcNow.ToString("yyyy:MM:dd.HH:mm:ss.fff");
             string p = puzzleGSData.GetString("puzzle");
             string s = puzzleGSData.GetString("solution");
             Size sz = (Size)puzzleGSData.GetInt("size");
             Level lv = (Level)puzzleGSData.GetInt("level");
             string id = string.Format("{0}{1}-{2}-{3}",
             "MULTI",
             creationTime,
             sz.ToString(),
             lv.ToString());

             Puzzle puzzle = new Puzzle(sz, lv, p, s, -1, -1, -1, -1);

             multiplayerPuzzle = puzzle;
             string puzzleStr = multiplayerPuzzle.puzzle;
             puzzleStr = puzzleStr.Replace("1", "");
             puzzleStr = puzzleStr.Replace("0", "");
             countMultiplayerCellNeedFill = puzzleStr.Length;

             PuzzleManager.Instance.multiPlayerPuzzles.Add(puzzle);
             PuzzleManager.Instance.multiPlayerPuzzleIds.Add(id);
             callback(id);
         });
    }

    private bool WaitUntilAllPlayerFinished()
    {
        foreach (var sessionData in PlayerSessionDatas)
        {
            if (sessionData.Key != localId && sessionData.Value.finished == false)
                return false;
        }
        Debug.Log(PlayerSessionDatas.Count);
        return true;
    }

    private void StartGame()
    {
        GameManager.Instance.PlayAPuzzle(puzzleId, false);
    }

    private bool WaitForAllPlayerLoadPuzzle()
    {
        foreach (var sessionData in PlayerSessionDatas)
        {
            if (sessionData.Value.puzzleLoaded == false)
                return false;
        }
        return true;
    }

    private void LoadPuzzle()
    {
        int offset = 0;
        foreach (var data in PlayerSessionDatas)
        {
            offset += data.Value.randomPuzzleOffset;
        }
        Debug.Log("Try to load target puzzle");
        GetMultiplayerPuzzle(offset, id =>
        {
            if (id == null)
            {
                Debug.Log("Can not get target puzzle");
                MultiplayerManager.Instance.LeaveRoom();
                GameManager.Instance.PrepareGame();
                UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), I2.Loc.ScriptLocalization.MULTIPLAYER_COULD_NOT_LOAD_REQUIRED_PUZZLE,
                I2.Loc.ScriptLocalization.OK.ToUpper(), "", () =>
                {
                    UIReferences.Instance.matchingPanelController.Hide();
                    MultiplayerManager.Instance.LeaveRoom();
                    MultiplayerManager.Instance.matchGroupBtn.SetActive(true);
                    MultiplayerManager.Instance.findingGroupBtn.SetActive(false);
                    if (GameManager.Instance.GameState == GameState.Prepare)
                        MultiplayerManager.Instance.controller.ShowIfNot();
                });
                return;
            }
            puzzleId = id;
            puzzleLoaded();
        });
    }

    private bool WaitForAllPlayerOffset()
    {
        foreach (var sessionData in PlayerSessionDatas)
        {
            if (sessionData.Value.randomPuzzleOffset == 0)
                return false;
        }
        return true;
    }

    private void ResolveLocalResult()
    {
        if (gameStarted == false)
            return;
        if (PlayerSessionDatas.Count == 0)
        {
            //* Resolve when all players disconnected */
            //* Treat as we disconnect and resolve as LOSE */
            PlayerInfoManager.Instance.UpdateWinLoseNumber(false);
            MultiplayerManager.Instance.SetLocalWinLose(false);
            Debug.Log("Resolve win lose: Both disconnect LOSE");
            playerWin = false;
            sessionFinished = true;
            if (LogicalBoard.Instance.IsPuzzleSolved() == false)
            {
                LogicalBoard.onPuzzleSolved();
                VisualBoard.Instance.HidePuzzleDelay(VisualBoard.Instance.puzzleSolvedHideDelay + 2.5f);
            }
            SessionFinished(false);
            return;
        }
        if (PlayerSessionDatas.Count == 1)
        {
            //* Resolve when other player disconnect */
            PlayerInfoManager.Instance.UpdateWinLoseNumber(true);
            MultiplayerManager.Instance.SetLocalWinLose(true);
            Debug.Log("Resolve win lose: Opponent disconnect WIN");
            playerWin = true;
            sessionFinished = true;
            if (LogicalBoard.Instance.IsPuzzleSolved() == false)
            {
                LogicalBoard.onPuzzleSolved();
                VisualBoard.Instance.HidePuzzleDelay(VisualBoard.Instance.puzzleSolvedHideDelay + 2.5f);
            }
            SessionFinished(true);
            return;
        }

        //* Resolve case when both players are still in room */
        //* If Both are finished check for their finished time */
        //* Else who hasn't finished yet lose */
        if (PlayerSessionDatas[localId].finished == false)
        {
            Debug.Log("Resolve win lose: local hasn't finished yet LOSE");
            CoinManager.Instance.RemoveCoins(MultiplayerRoom.Instance.currentBetCoin);
            PlayerInfoManager.Instance.UpdateWinLoseNumber(false);
            MultiplayerManager.Instance.SetLocalWinLose(false);
            return;
        }
        if (PlayerSessionDatas[localId].finished == true)
        {
            if (PlayerSessionDatas[localId].solved == false)
            {
                PlayerInfoManager.Instance.UpdateWinLoseNumber(false);
                MultiplayerManager.Instance.SetLocalWinLose(false);
                Debug.Log("Resolve win lose: local has finished but the puzzle hasn't been solved yet LOSE");
                playerWin = false;
                sessionFinished = true;
                if (LogicalBoard.Instance.IsPuzzleSolved() == false)
                {
                    LogicalBoard.onPuzzleSolved();
                    VisualBoard.Instance.HidePuzzleDelay(VisualBoard.Instance.puzzleSolvedHideDelay + 2.5f);
                }
                SessionFinished(false);
                return;
            }
            if (PlayerSessionDatas[localId].solved == true)
            {
                foreach (var data in PlayerSessionDatas)
                {
                    if (data.Key != localId && data.Value.solved == true && data.Value.solvedTime <= PlayerSessionDatas[localId].solvedTime)
                    {
                        PlayerInfoManager.Instance.UpdateWinLoseNumber(false);
                        MultiplayerManager.Instance.SetLocalWinLose(false);
                        Debug.Log("Resolve win lose: local has finished and solved the puzzle but the time stamp is later than the opponent LOSE");
                        playerWin = false;
                        sessionFinished = true;
                        if (LogicalBoard.Instance.IsPuzzleSolved() == false)
                        {
                            LogicalBoard.onPuzzleSolved();
                            VisualBoard.Instance.HidePuzzleDelay(VisualBoard.Instance.puzzleSolvedHideDelay + 2.5f);
                        }
                        SessionFinished(false);
                        return;
                    }
                }
                PlayerInfoManager.Instance.UpdateWinLoseNumber(true);
                MultiplayerManager.Instance.SetLocalWinLose(true);
                Debug.Log("Resolve win lose: local has finished and solved the puzzle earlier than the opponent WIN");
                playerWin = true;
                sessionFinished = true;
                SessionFinished(true);
                return;
            }
        }
    }

    internal void SetRandomOffset(string senderId, short offset, int skinIndex)
    {
        PlayerSessionDatas[senderId].randomPuzzleOffset = offset;
        PlayerSessionDatas[senderId].playerSkinIndex = skinIndex;
    }

    internal void SetPuzzleLoaded(string senderId)
    {
        PlayerSessionDatas[senderId].puzzleLoaded = true;
    }

    internal void SetCellsValue(string senderId, Index2D changedIndex, short value, int skinIndex)
    {
        PlayerSessionDatas[senderId].playerSkinIndex = skinIndex;
        if (PlayerSessionDatas[senderId].moves.ContainsKey(changedIndex))
        {
            if (value != -1)
                PlayerSessionDatas[senderId].moves[changedIndex] = value;
            else
                PlayerSessionDatas[senderId].moves.Remove(changedIndex);
        }

        if (!PlayerSessionDatas[senderId].moves.ContainsKey(changedIndex) && value != -1)
        {
            PlayerSessionDatas[senderId].moves.Add(changedIndex, value);
        }

        if (!VisualBoard.Instance.IsInit())
            return;
        UpdateCurrentViewBoard(currentPlayerIndex);

        if (UIReferences.Instance.headerMultiplayeInfo != null)
        {
            PlayerSessionData sessionData;
            sessionData = PlayerSessionDatas[senderId];
            int numberMove = sessionData.moves.Count;
            if (senderId == localId)
            {
                float progress = (float)numberMove / countMultiplayerCellNeedFill;
                UIReferences.Instance.headerMultiplayeInfo.SetCurrentPlayerProgress(progress);
            }
            else
            {
                float progress = (float)numberMove / countMultiplayerCellNeedFill;
                UIReferences.Instance.headerMultiplayeInfo.SetOpponentProgress(progress);
            }
        }
    }

    internal void SetPuzzleSolved(string senderId, double timeStamp)
    {
        PlayerSessionDatas[senderId].solved = true;
        PlayerSessionDatas[senderId].solvedTime = timeStamp;
        PlayerSessionDatas[senderId].finished = true;
        if (PlayerSessionDatas[localId].finished == false)
        {
            finished();
        }
    }

    internal void SetPuzzleFinished(string senderId)
    {
        PlayerSessionDatas[senderId].finished = true;
    }

    internal void RemovePlayers(string[] participantIds)
    {
        foreach (var id in participantIds)
        {
            if (PlayerSessionDatas.ContainsKey(id))
            {
                PlayerSessionDatas.Remove(id);
            }
        }
    }
}
