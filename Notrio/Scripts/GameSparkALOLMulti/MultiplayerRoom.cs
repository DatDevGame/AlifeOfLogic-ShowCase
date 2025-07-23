using System;
using System.Collections;
using System.Collections.Generic;
using EasyMobile;
using Takuzu;
using UnityEngine;

public class MultiplayerRoom : MonoBehaviour
{

    public static event System.Action LoadedMultiplayerPuzzle = delegate { };
    public static event System.Action<MultiplayerInfo, int, int, int> LoadedMatchInfo = delegate { };
    public static event System.Action OpponentReady = delegate { };

    internal class RoomOption
    {
        public Action<byte[]> onLocalPlayerSendMessage { set; get; }
        public bool botMode { get; set; }
        public bool defaultReady { get; set; }
        public Transform container { get; set; }
        public List<string> playerIds { get; set; }
        public string localPlayerId { get; set; }
    }

    internal static MultiplayerRoom CreateNewRoom(RoomOption roomOption)
    {
        GameObject go = CreateThisGameObjectInstance(roomOption.container);
        go.name = "room";
        MultiplayerRoom room = go.GetComponent<MultiplayerRoom>();
        room.roomOption = roomOption;
        return room;
    }

    private static GameObject CreateThisGameObjectInstance(Transform container)
    {
        GameObject go = new GameObject();
        go.transform.SetParent(container);
        go.AddComponent<MultiplayerRoom>();
        return go;
    }

    private class RoomPlayerData
    {
        public bool playerReady { set; get; }
        public string playerName = "";
        public string avatarUrl = "";
        public int playerNode = -1;
        public int loseNumber = -1;
        public int winNumber = -1;
        public int randomSeed = 0;
    }

    public const string BOT_DEFAULT_ID = "IMBOT";
    public const string PLAYER_DEFAULT_ID = "IMPLAYER";
    public Action aPlayerSolvedPuzzle = delegate { };
    private RoomOption roomOption;
    public bool botMode;
    private string localId;
    public MultiplayerDataHelper dataHelper;
    private MultiplayerLocalPlayer localPlayer;
    private MultiplayerBotPlayer botPlayer;
    private Dictionary<string, RoomPlayerData> roomPlayerDatas = new Dictionary<string, RoomPlayerData>();
    private List<string> playerIds = new List<string>();
    private MultiplayerSession session;
    private Action<byte[]> onLocalPlayerSendMessage;

    public int currentPuzzleSize { get; private set; }
    public int currentPuzzleLevel { get; private set; }
    public int currentBetCoin { get; private set; }

    private int randomPuzzleSize;
    private int randomePuzzleLevel;
    private bool isOpponnentReady;
    private bool isInitialize;
    public static MultiplayerRoom Instance;

    void Awake()
    {
        Instance = this;
        MatchingPanelController.AcceptMatchingEvent += OnAcceptMatchingEvent;
        MultiplayerSession.SessionFinished += OnSessionFinished;
    }

    private void OnDestroy()
    {
        MatchingPanelController.AcceptMatchingEvent -= OnAcceptMatchingEvent;
        MultiplayerSession.SessionFinished -= OnSessionFinished;
    }

    void OnSessionFinished(bool isWin)
    {
        foreach (var key in roomPlayerDatas.Keys)
        {
            Debug.Log(key);
        }
        if (botMode)
        {
            if (isWin)
                roomPlayerDatas[BOT_DEFAULT_ID].loseNumber++;
            else
                roomPlayerDatas[BOT_DEFAULT_ID].winNumber++;
        }
    }

    private void OnAcceptMatchingEvent()
    {
        PlayerReady();
    }

    public int GetRandomSizeByLevel(int level)
    {
        switch (level)
        {
            case 1:
                return 12;
            case 2:
                return 12;
            case 3:
                return 10;
            case 4:
                return 8;
            case 5:
                return 6;
            default:
                return 8;
        }
    }
    private void Start()
    {
        Debug.Log("Creating new Room");
        dataHelper = new MultiplayerDataHelper();
        botMode = roomOption.botMode;
        int botWinNumber = UnityEngine.Random.Range(Mathf.Max(0, PlayerInfoManager.Instance.winNumber - 20), PlayerInfoManager.Instance.winNumber + 20);
        int botloseNumber = Mathf.Max(0, botWinNumber + UnityEngine.Random.Range(-botWinNumber, botWinNumber));

        int maxLevel = Mathf.Clamp((int)((StoryPuzzlesSaver.Instance.MaxNode + 1) / 4) + 1, 1, 5);
        randomePuzzleLevel = UnityEngine.Random.Range(1, maxLevel + 1);
        randomPuzzleSize = GetRandomSizeByLevel(randomePuzzleLevel);

        if (roomOption.botMode == true)
        {
            botPlayer = MultiplayerBotPlayer.CreatePlayer(BOT_DEFAULT_ID, transform, OnPlayerPuzzleValueSet, OnPlayerPuzzleSolved, this);
            localPlayer = MultiplayerLocalPlayer.CreatePlayer(PLAYER_DEFAULT_ID, transform, OnPlayerPuzzleValueSet, OnPlayerPuzzleSolved);
            int botMaxNode = UnityEngine.Random.Range(0, 18);
            int botRandomLevel = UnityEngine.Random.Range(1, Mathf.Clamp((int)((botMaxNode + 1) / 4) + 1, 1, 5) + 1);
            int botRandomSize = GetRandomSizeByLevel(botRandomLevel);
            roomPlayerDatas.Add(BOT_DEFAULT_ID, new RoomPlayerData()
            {
                playerReady = true,
                playerName = LeaderBoardScreenUI.playerDefaultName,
                playerNode = botMaxNode,
                avatarUrl = "",
                loseNumber = botloseNumber,
                winNumber = botWinNumber,
                randomSeed = UnityEngine.Random.Range(0, 100)
            });
            playerIds.Add(BOT_DEFAULT_ID);
            roomPlayerDatas.Add(PLAYER_DEFAULT_ID, new RoomPlayerData()
            {
                playerReady = roomOption.defaultReady
            });
            playerIds.Add(PLAYER_DEFAULT_ID);
            localId = PLAYER_DEFAULT_ID;
        }

        if (roomOption.botMode == false)
        {
            localPlayer = MultiplayerLocalPlayer.CreatePlayer(roomOption.localPlayerId, transform, OnPlayerPuzzleValueSet, OnPlayerPuzzleSolved);
            foreach (var playerId in roomOption.playerIds)
            {
                roomPlayerDatas.Add(playerId, new RoomPlayerData()
                {
                    playerReady = roomOption.defaultReady
                });
            }
            localId = roomOption.localPlayerId;
            onLocalPlayerSendMessage = roomOption.onLocalPlayerSendMessage;
            playerIds = roomOption.playerIds;
        }
        StartRoomLifeCycle();
        isInitialize = true;
    }

    private void StartRoomLifeCycle()
    {
        StartCoroutine(RoomLifeCycleCR(false));
    }

    IEnumerator Delay(Func<bool> condition, Action callback)
    {
        yield return new WaitUntil(condition);
        callback();
    }

    private List<int> defaultAvailableLevels = new List<int>() { 1, 2, 3, 4, 5 };
    private List<int> GetAvailableLevels(int maxLevelAllowed)
    {
        List<int> allAvailableLevels = new List<int>();
        if (CloudServiceManager.Instance != null && CloudServiceManager.Instance.appConfig != null)
        {
            allAvailableLevels = CloudServiceManager.Instance.appConfig.GetIntList("MultiplayerAvailableLevels");
        }
        if (allAvailableLevels == null || allAvailableLevels.Count == 0)
        {
            allAvailableLevels = defaultAvailableLevels;
        }
        List<int> result = new List<int>();
        foreach (var level in allAvailableLevels)
        {
            if (level <= maxLevelAllowed)
            {
                result.Add(level);
            }
        }
        return result;
    }
    private List<int> GetAllAvailableLevels()
    {
        List<int> allAvailableLevels = new List<int>();
        if (CloudServiceManager.Instance != null && CloudServiceManager.Instance.appConfig != null)
        {
            allAvailableLevels = CloudServiceManager.Instance.appConfig.GetIntList("MultiplayerAvailableLevels");
        }
        if (allAvailableLevels == null || allAvailableLevels.Count == 0)
        {
            allAvailableLevels = defaultAvailableLevels;
        }
        return allAvailableLevels;
    }
    private Dictionary<int, List<int>> defaultAvailableSizes = new Dictionary<int, List<int>>(){
        {1, new List<int>(){12}},
        {2, new List<int>(){12}},
        {3, new List<int>(){10}},
        {4, new List<int>(){8}},
        {5, new List<int>(){6}},
    };
    private List<int> GetAvailableSizeForLevel(int level)
    {
        List<int> result = new List<int>();
        if (CloudServiceManager.Instance != null && CloudServiceManager.Instance.appConfig != null)
        {
            result = CloudServiceManager.Instance.appConfig.GetIntList(string.Format("MultiplayerAvailableSizeForLevel{0}", level));
        }
        if (result == null || result.Count == 0)
        {
            result = defaultAvailableSizes[level];
        }
        return result;
    }

    private Dictionary<string, int> defaultBetCoinsForLevelSize = new Dictionary<string, int>()
    {   {"BetCoinsAmountForLevel1Size6",12},
        {"BetCoinsAmountForLevel1Size8",12},
        {"BetCoinsAmountForLevel1Size10",12},
        {"BetCoinsAmountForLevel1Size12",12},
        {"BetCoinsAmountForLevel2Size6",12},
        {"BetCoinsAmountForLevel2Size8",12},
        {"BetCoinsAmountForLevel2Size10",12},
        {"BetCoinsAmountForLevel2Size12",12},
        {"BetCoinsAmountForLevel3Size6",12},
        {"BetCoinsAmountForLevel3Size8",12},
        {"BetCoinsAmountForLevel3Size10",12},
        {"BetCoinsAmountForLevel3Size12",12},
        {"BetCoinsAmountForLevel4Size6",12},
        {"BetCoinsAmountForLevel4Size8",12},
        {"BetCoinsAmountForLevel4Size10",12},
        {"BetCoinsAmountForLevel4Size12",12},
        {"BetCoinsAmountForLevel5Size6",12},
        {"BetCoinsAmountForLevel5Size8",12},
        {"BetCoinsAmountForLevel5Size10",12},
        {"BetCoinsAmountForLevel5Size12",12},};

    private int GetBetCoinsFromLevelAndSize(int level, int size)
    {
        string getBetCoinsAmountKey = string.Format("BetCoinsAmountForLevel{0}Size{1}", level, size);
        if (CloudServiceManager.Instance != null & CloudServiceManager.Instance.appConfig != null)
        {
            return CloudServiceManager.Instance.appConfig.GetInt(getBetCoinsAmountKey) ?? defaultBetCoinsForLevelSize[getBetCoinsAmountKey];
        }
        return defaultBetCoinsForLevelSize[getBetCoinsAmountKey];

    }

    private IEnumerator RoomLifeCycleCR(bool isRematch)
    {
        foreach (var key in roomPlayerDatas.Keys)
        {
            Debug.Log(key);
        }
        yield return new WaitForSeconds(0.5f);
        if (!isRematch)
            SendPlayerInfomation();
        yield return new WaitUntil(() => WaitAllPlayerInfomation());

        RoomPlayerData opponentRoomData;
        MultiplayerInfo opponentPlayerInfo;
        foreach (var playerId in playerIds)
        {
            if (!playerId.Equals(localId))
            {
                opponentRoomData = roomPlayerDatas[playerId];
                opponentPlayerInfo = new MultiplayerInfo()
                {
                    playerName = opponentRoomData.playerName,
                    playerNode = opponentRoomData.playerNode,
                    avatarUrl = opponentRoomData.avatarUrl,
                    winNumber = opponentRoomData.winNumber,
                    loseNumber = opponentRoomData.loseNumber
                };
                //int totalLevelRandomSeed = opponentRoomData.randomSeed + roomPlayerDatas[localId].randomSeed;
                //int totalSizeRandomSeed = opponentRoomData.randomSeed * roomPlayerDatas[localId].randomSeed;
                //Debug.Log("MULTIPLAYER_TEST current random = " + roomPlayerDatas[localId].randomSeed);
                //Debug.Log("MULTIPLAYER_TEST Total random: " + totalLevelRandomSeed + " " + totalSizeRandomSeed);
                //Debug.Log("MULTIPLAYER_TEST Opponent MaxNode and Level " + opponentPlayerInfo.playerNode + " " + (int) StoryPuzzlesSaver.GetDifficultLevelFromIndex(opponentPlayerInfo.playerNode));
                //Debug.Log("MULTIPLAYER_TEST Player MaxNode and Level " + roomPlayerDatas[localId].playerNode + " " + (int) StoryPuzzlesSaver.GetDifficultLevelFromIndex(roomPlayerDatas[localId].playerNode));

                // int maxAllowedLevel = Math.Min((int)StoryPuzzlesSaver.GetDifficultLevelFromIndex(opponentPlayerInfo.playerNode), (int)StoryPuzzlesSaver.GetDifficultLevelFromIndex(roomPlayerDatas[localId].playerNode));
                // List<int> allowedLevels = GetAvailableLevels(maxAllowedLevel);
                // currentPuzzleLevel = allowedLevels[totalLevelRandomSeed % allowedLevels.Count];
                // //Debug.Log("MULTIPLAYER_TEST Selected Level: " + currentPuzzleLevel);
                // List<int> allowedSizes = GetAvailableSizeForLevel(currentPuzzleLevel);
                // currentPuzzleSize = allowedSizes[totalSizeRandomSeed % allowedSizes.Count];
                // //Debug.Log("MULTIPLAYER_TEST Selected Size: " + currentPuzzleSize);
                // int minNode = Math.Min(opponentPlayerInfo.playerNode, roomPlayerDatas[localId].playerNode);
                // //Get random values
                // int multiplyRandomSeed = opponentRoomData.randomSeed * roomPlayerDatas[localId].randomSeed;
                // int addRandomSeed = opponentRoomData.randomSeed + roomPlayerDatas[localId].randomSeed;
                // //Get Max allowed level and size
                // int maxAllowedLevel = Math.Min((int)StoryPuzzlesSaver.GetDifficultLevelFromIndex(opponentPlayerInfo.playerNode), 
                //                                 (int)StoryPuzzlesSaver.GetDifficultLevelFromIndex(roomPlayerDatas[localId].playerNode));
                // int maxAllowedSize = (int)StoryPuzzlesSaver.GetSizeFromNodeIndex(minNode);
                // //Choose level
                // List<int> allowedLevels = GetAvailableLevels(maxAllowedLevel);
                // currentPuzzleLevel = allowedLevels[multiplyRandomSeed % allowedLevels.Count];
                // //Choose size
                // //If selected level is lower than min level of two participants get full available sizes
                // List<int> allowedSizes = GetAvailableSizeForLevel(currentPuzzleLevel);
                // if(currentPuzzleLevel == maxAllowedLevel)
                //     allowedSizes.RemoveAll(size => size > maxAllowedSize);
                // currentPuzzleSize = allowedSizes[addRandomSeed % allowedSizes.Count];


                int minNode = Math.Min(opponentPlayerInfo.playerNode, roomPlayerDatas[localId].playerNode);
                //Get random values
                int totalRandom = opponentRoomData.randomSeed + roomPlayerDatas[localId].randomSeed;
                //Get Allowed Index
                List<int> allowedIndexes = GetAllowToSelectNodeIndexes(minNode);
                int selectedNode = allowedIndexes[totalRandom % allowedIndexes.Count];
                //GEt Current Level and Size
                currentPuzzleLevel = (int) StoryPuzzlesSaver.GetDifficultLevelFromIndex(selectedNode - 1);
                currentPuzzleSize = (int) StoryPuzzlesSaver.GetSizeFromNodeIndex(selectedNode);

                currentBetCoin = GetBetCoinsFromLevelAndSize(currentPuzzleLevel, currentPuzzleSize);
                LoadedMatchInfo(opponentPlayerInfo, currentPuzzleLevel, currentPuzzleSize, currentBetCoin);

                break;
            }
        }

        yield return new WaitUntil(() => IsAllPlayersReady());
        if (roomPlayerDatas.Count == 1)
        {
            //Debug.Log("MULTIPLAYER_TEST Not enough player can not start the game");
            DestroyImmediate(gameObject);
            yield break;
        }
        ResetRandomStates();
        ResetReadyStates();

        isOpponnentReady = false;
        session = MultiplayerSession.CreateSession(transform, localId, playerIds, OnRandomPuzzleOffset, OnPuzzledLoaded, OnMatchFinished);
        yield return new WaitUntil(() => session == null);
        SendPlayerInfomation();
        if (roomOption.botMode)
        {
            roomPlayerDatas[BOT_DEFAULT_ID].randomSeed = UnityEngine.Random.Range(0, 100);
        }
        yield return new WaitUntil(() => WaitAllPlayerInfomation());
        StartCoroutine(RoomLifeCycleCR(true));
    }

    private List<int> GetAllowToSelectNodeIndexes(int maxNode)
    {
        List<int> allowed = new List<int>();
        List<int> allowedLevel = GetAllAvailableLevels();
        foreach (var level in allowedLevel)
        {
            List<int> allowedSize = GetAvailableSizeForLevel(level);
            foreach (var size in allowedSize)
                allowed.Add(StoryPuzzlesSaver.GetIndexNode((Takuzu.Generator.Level)level, (Takuzu.Generator.Size)size));
        }
        if(allowed.Count == 0)
            allowed.Add(0);
        int minAllowedNode = allowed[0];
        foreach (var index in allowed)
        {
            if(index < minAllowedNode)
                minAllowedNode = index;
        }
        allowed.RemoveAll(node => node > maxNode);
        if(allowed.Count == 0)
            allowed.Add(minAllowedNode);
        return allowed;
    }

    private void ResetRandomStates()
    {
        //Debug.Log("MULTIPLAYER_TEST ResetRandomStates");
        foreach (var key in roomPlayerDatas.Keys)
        {
            roomPlayerDatas[key].randomSeed = -1;
        }
    }

    private void ResetReadyStates()
    {
        //Debug.Log("MULTIPLAYER_TEST ResetRandomStates");
        foreach (var key in roomPlayerDatas.Keys)
        {
            roomPlayerDatas[key].playerReady = false;
        }
    }

    private bool IsAllPlayersReady()
    {
        bool isAllReady = true;
        foreach (var playerId in playerIds)
        {
            if (roomPlayerDatas[playerId].playerReady == false)
            {
                isAllReady = false;
            }
            else
            {
                if (!isOpponnentReady && playerId != localId)
                {
                    isOpponnentReady = true;
                    OpponentReady();
                }
            }
        }
        return isAllReady;
    }

    public bool WaitAllPlayerInfomation()
    {
        foreach (var playerRoomData in roomPlayerDatas)
        {
            if (string.IsNullOrEmpty(playerRoomData.Value.playerName) || playerRoomData.Value.randomSeed == -1)
            {
                return false;
            }
        }
        return true;
    }

    public void SendPlayerInfomation()
    {
        string id = localId;
        List<byte> data = new List<byte>();
        data.Add((byte)MultiplayerDataHelper.MessageType.SendPlayerInformation);

        int size = randomPuzzleSize;
        int level = randomePuzzleLevel;

        byte[] pNameCharArr = new byte[MultiplayerDataHelper.ByteSizeConst];
        byte[] originalPName = System.Text.Encoding.UTF8.GetBytes(string.IsNullOrEmpty(MultiplayerManager.Instance.playerMultiplayerInfo.playerName) ? "" : MultiplayerManager.Instance.playerMultiplayerInfo.playerName);
        for (int i = 0; i < Math.Min(MultiplayerDataHelper.ByteSizeConst, originalPName.Length); i++)
        {
            pNameCharArr[i] = originalPName[i];
        }

        byte[] avatarUrlCharArr = new byte[MultiplayerDataHelper.ByteSizeConst];
        byte[] originalAvatarUrl = System.Text.Encoding.UTF8.GetBytes(string.IsNullOrEmpty(MultiplayerManager.Instance.playerMultiplayerInfo.avatarUrl) ? "" : MultiplayerManager.Instance.playerMultiplayerInfo.avatarUrl);
        for (int i = 0; i < Math.Min(MultiplayerDataHelper.ByteSizeConst, originalAvatarUrl.Length); i++)
        {
            avatarUrlCharArr[i] = originalAvatarUrl[i];
        }
        data.AddRange(dataHelper.getBytes(new MultiplayerDataHelper.PlayerInformationStruct()
        {
            playerName = pNameCharArr,
            avatarUrl = avatarUrlCharArr,
            playerNode = (short)MultiplayerManager.Instance.playerMultiplayerInfo.playerNode,
            winNumber = (short)MultiplayerManager.Instance.playerMultiplayerInfo.winNumber,
            loseNumber = (short)MultiplayerManager.Instance.playerMultiplayerInfo.loseNumber,
            randomSeed = (short)UnityEngine.Random.Range(0, 100),
        }));
        ReceiveMessage(id, data.ToArray());
        if (botMode == false)
        {
            onLocalPlayerSendMessage(data.ToArray());
        }
    }

    public int CountRoomPlayerData()
    {
        return roomPlayerDatas.Count;
    }

    public void PlayerReady(string id = "")
    {
#if UNITY_IOS
        if (AdDisplayer.IsAllowToShowAd() && Advertising.IsInterstitialAdReady() && AdsFrequencyManager.Instance.IsAppropriateFrequencyForInterstitial() && !Advertising.IsAdRemoved()
            && InAppPurchaser.Instance != null && !InAppPurchaser.Instance.IsSubscibed() && Application.internetReachability != NetworkReachability.NotReachable)
        {
#if UNITY_IOS
            Time.timeScale = 0;
            AudioListener.pause = true;
#endif
            Advertising.InterstitialAdCompleted += (adNetwork, adPlacement) =>
            {
                if (this == null)
                {
                    Debug.Log("This is null");
                    return;
                }

                Debug.Log("SendReadyMessage");
                SendReadyMessage(id);
            };
            Advertising.ShowInterstitialAd();
            return;
        }
#endif
        //Debug.Log(" MULTIPLAYER_TESTSendReadyMessage");
        SendReadyMessage(id);
    }

    IEnumerator CR_CallBack(Action callback)
    {
        yield return new WaitForSeconds(15);
        callback();
    }

    private void SendReadyMessage(string id)
    {
        if (id == "")
            id = localId;
        List<byte> data = new List<byte>();
        data.Add((byte)MultiplayerDataHelper.MessageType.Ready);
        ReceiveMessage(id, data.ToArray());
        if (botMode == false)
        {
            onLocalPlayerSendMessage(data.ToArray());
        }
    }

    private void OnRandomPuzzleOffset(int offset, int skinIndex)
    {
        //Debug.Log("MULTIPLAYER_TEST LOCAL RAND:: " + offset);
        List<byte> data = new List<byte>();
        data.Add((byte)MultiplayerDataHelper.MessageType.PuzzleSelect);
        data.AddRange(dataHelper.getBytes(new MultiplayerDataHelper.PuzzleSelectStruct()
        {
            offset = (short)offset,
            skinIndex = (short)skinIndex
        }));
        ReceiveMessage(localId, data.ToArray());
        if (botMode == false)
        {
            onLocalPlayerSendMessage(data.ToArray());
        }
    }
    private void OnPuzzledLoaded()
    {
        List<byte> data = new List<byte>();
        data.Add((byte)MultiplayerDataHelper.MessageType.PuzzleLoaded);
        ReceiveMessage(localId, data.ToArray());
        if (botMode == false)
        {
            onLocalPlayerSendMessage(data.ToArray());
        }
    }
    private void OnMatchFinished()
    {
        List<byte> data = new List<byte>();
        data.Add((byte)MultiplayerDataHelper.MessageType.SessionFinished);
        ReceiveMessage(localId, data.ToArray());
        if (botMode == false)
        {
            onLocalPlayerSendMessage(data.ToArray());
        }
    }
    private void OnMessageFromMultiplayerServices(string id, byte[] data)
    {
        //Received message from multiplayer services
        ReceiveMessage(id, data);
    }
    private void OnPlayerPuzzleValueSet(string id, Index2D index, int value, int skinIndex)
    {
        //Send Message to all
        List<byte> data = new List<byte>();
        data.Add((byte)MultiplayerDataHelper.MessageType.CellValueSet);
        data.AddRange(dataHelper.getBytes(new MultiplayerDataHelper.OnCellValueSetStruct()
        {
            col = (short)index.column,
            row = (short)index.row,
            value = (short)value,
            skinIndex = (short)skinIndex
        }));
        ReceiveMessage(id, data.ToArray());
        if (botMode == false)
        {
            onLocalPlayerSendMessage(data.ToArray());
        }
    }
    private void OnPlayerPuzzleSolved(string id)
    {
        //send message to all
        CloudServiceManager.Instance.GetCurrentServerTime(time =>
        {
            List<byte> data = new List<byte>();
            data.Add((byte)MultiplayerDataHelper.MessageType.PuzzleSolved);
            data.AddRange(dataHelper.getBytes(new MultiplayerDataHelper.OnPuzzleSolvedStruct()
            {
                timeStamp = time
            }));
            ReceiveMessage(id, data.ToArray());
            if (botMode == false)
            {
                onLocalPlayerSendMessage(data.ToArray());
            }
        });
    }

    internal void RemovePlayers(string[] participantIds)
    {
        foreach (var id in participantIds)
        {
            if (roomPlayerDatas.ContainsKey(id))
            {
                roomPlayerDatas.Remove(id);
            }
        }
        if (session != null)
        {
            session.RemovePlayers(participantIds);
        }
        if (roomPlayerDatas.Count == 1)
        {
            DestroyImmediate(gameObject);
        }
    }

    internal string[] GetAllPlayers()
    {
        List<string> ids = new List<string>();
        foreach (var keyValuePair in roomPlayerDatas)
        {
            ids.Add(keyValuePair.Key);
        }
        return ids.ToArray();
    }

    internal void ReceiveMessage(string senderId, byte[] data)
    {
        if (isInitialize == false)
        {
            //* Delay messages that require roomPlayerDatas to be initialized*/
            StartCoroutine(Delay(() => { return isInitialize; }, () =>
            {
                ReceiveMessage(senderId, data);
            }));
            return;
        }

        Debug.Log(BitConverter.ToString(data));

        //Listen for other random offset
        if ((MultiplayerDataHelper.MessageType)data[0] == MultiplayerDataHelper.MessageType.PuzzleSelect)
        {
            MultiplayerDataHelper.PuzzleSelectStruct dataStruct = dataHelper.fromBytes(dataHelper.SubArray<byte>(data, 1, data.Length - 1), new MultiplayerDataHelper.PuzzleSelectStruct());
            if (session != null)
            {
                session.SetRandomOffset(senderId, dataStruct.offset, dataStruct.skinIndex);
            }
        }
        if ((MultiplayerDataHelper.MessageType)data[0] == MultiplayerDataHelper.MessageType.PuzzleLoaded)
        {
            if (session != null)
            {
                session.SetPuzzleLoaded(senderId);
                LoadedMultiplayerPuzzle();
            }
        }
        if ((MultiplayerDataHelper.MessageType)data[0] == MultiplayerDataHelper.MessageType.CellValueSet)
        {
            MultiplayerDataHelper.OnCellValueSetStruct dataStruct = dataHelper.fromBytes(dataHelper.SubArray<byte>(data, 1, data.Length - 1), new MultiplayerDataHelper.OnCellValueSetStruct());
            Debug.Log(dataStruct.col + " " + dataStruct.row + " " + dataStruct.value + " " + dataStruct.skinIndex);
            Index2D changedIndex = new Index2D(dataStruct.row, dataStruct.col);

            if (session != null)
            {
                session.SetCellsValue(senderId, changedIndex, dataStruct.value, dataStruct.skinIndex);
            }
        }
        if ((MultiplayerDataHelper.MessageType)data[0] == MultiplayerDataHelper.MessageType.PuzzleSolved)
        {
            MultiplayerDataHelper.OnPuzzleSolvedStruct dataStruct = dataHelper.fromBytes(dataHelper.SubArray<byte>(data, 1, data.Length - 1), new MultiplayerDataHelper.OnPuzzleSolvedStruct());

            if (session != null)
            {
                session.SetPuzzleSolved(senderId, dataStruct.timeStamp);
            }
            if (botMode)
                aPlayerSolvedPuzzle();
        }
        if ((MultiplayerDataHelper.MessageType)data[0] == MultiplayerDataHelper.MessageType.SessionFinished)
        {
            if (session != null)
            {
                session.SetPuzzleFinished(senderId);
            }
        }
        if ((MultiplayerDataHelper.MessageType)data[0] == MultiplayerDataHelper.MessageType.Ready)
        {
            if (roomPlayerDatas.ContainsKey(senderId))
            {
                roomPlayerDatas[senderId].playerReady = true;
            }
        }
        if ((MultiplayerDataHelper.MessageType)data[0] == MultiplayerDataHelper.MessageType.SendPlayerInformation)
        {
            MultiplayerDataHelper.PlayerInformationStruct dataStruct = dataHelper.fromBytes(dataHelper.SubArray<byte>(data, 1, data.Length - 1), new MultiplayerDataHelper.PlayerInformationStruct()
            {
                playerName = new byte[MultiplayerDataHelper.ByteSizeConst],
                avatarUrl = new byte[MultiplayerDataHelper.ByteSizeConst]
            });
            if (roomPlayerDatas.ContainsKey(senderId))
            {
                //Debug.Log("MULTIPLAYER_TEST After-Sender ID = " + senderId);
                roomPlayerDatas[senderId].playerName = System.Text.Encoding.UTF8.GetString(dataStruct.playerName).Replace("\0", string.Empty);
                roomPlayerDatas[senderId].avatarUrl = System.Text.Encoding.UTF8.GetString(dataStruct.avatarUrl).Replace("\0", string.Empty);
                roomPlayerDatas[senderId].playerNode = dataStruct.playerNode;
                roomPlayerDatas[senderId].winNumber = dataStruct.winNumber;
                roomPlayerDatas[senderId].loseNumber = dataStruct.loseNumber;
                roomPlayerDatas[senderId].randomSeed = dataStruct.randomSeed;
            }
        }
    }
}
