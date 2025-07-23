using System;
using System.Collections;
using System.Collections.Generic;
using EasyMobile;
using GameSparks.Core;
using Takuzu;
using UnityEngine;
using UnityEngine.UI;

public struct MultiplayerInfo
{
    public string playerName;
    public int playerNode;
    public int winNumber;
    public int loseNumber;
    public string avatarUrl;
}

public class MultiplayerManager : MonoBehaviour, IRealTimeMultiplayerListener
{
    public static System.Action OpponentDisconnected = delegate { };

    public Button findMatchBtn;
    public Button quickMatchBtn;
    public Button cancelBtn;
    public Text playerNameTxt;
    public Text playerLevelTxt;
    public RawImage avatarRawImg;
    public Text winNumberTxt;
    public Text loseNumberTxt;
    public Text energyCostTxt;
    public GameObject matchGroupBtn;
    public GameObject findingGroupBtn;

    public MultiplayerRoom room { get; private set; }
    public UiGroupController controller;

    public static MultiplayerManager Instance;
    public static Action<bool> EnterMultiplayerMatch = delegate { };
    public static Action<bool> MultiplayerBotWin = delegate { };
    public MultiplayerInfo playerMultiplayerInfo;
    public MultiplayerInfo opponentMultiplayerInfo;
    public int LocalUserWin { get; private set; }
    public int LocalOpponentWin { get; private set; }
    public bool isLoadedAvatar { get; private set; }
    private Coroutine waitingOpponentInfoCR;
    private float timeOut
    {
        get
        {
            return PlayerPrefs.GetFloat("timeOutWaitingOpponent", defaultTimeOut);
        }
        set
        {
            PlayerPrefs.SetFloat("timeOutWaitingOpponent", value);
        }
    }
    private float defaultTimeOut = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(this);
            return;
        }
        isLoadedAvatar = false;
        AssignUIButton();
        LoadMultiplayerUserData();
        if (EnergyManager.Instance != null)
            energyCostTxt.text = EnergyManager.Instance.MultiplayerEnergyCost.ToString();
        CloudServiceManager.onPlayerDbSyncEnd += OnPlayerDbSyncEnd;
        GameManager.GameStateChanged += OnGameStateChanged;
        MatchingPanelController.DeclineMatchingEvent += OnDeclineMatchingEvent;
        MultiplayerRoom.LoadedMatchInfo += OnLoadedMatchInfo;
        MultiplayerSession.SessionFinished += OnSessionFinished;
        MultiplayerSession.SessionStarted += OnSessionStarted;
        CloudServiceManager.onConfigLoaded += OnConfigLoaded;
    }

    private void Start()
    {
        if (UIReferences.Instance != null)
        {
            UIReferences.Instance.tournamentSideUI.homeBtn.onClick.RemoveAllListeners();
            UIReferences.Instance.tournamentSideUI.homeBtn.onClick.AddListener(delegate
            {
                StopAllCoroutines();
                CancelMatchMaking();
                if (IsRoomConnected())
                    LeaveRoom();
                SceneLoadingManager.Instance.LoadMainScene();
            });
        }
    }

    private void OnDestroy()
    {
        GameManager.GameStateChanged -= OnGameStateChanged;
        MultiplayerRoom.LoadedMatchInfo -= OnLoadedMatchInfo;
        MatchingPanelController.DeclineMatchingEvent -= OnDeclineMatchingEvent;
        CloudServiceManager.onPlayerDbSyncEnd -= OnPlayerDbSyncEnd;
        MultiplayerSession.SessionFinished -= OnSessionFinished;
        MultiplayerSession.SessionStarted -= OnSessionStarted;
        CloudServiceManager.onConfigLoaded -= OnConfigLoaded;
    }

    void OnConfigLoaded(GSData config)
    {
        float? timeOutConfig = config.GetInt("timeOutWaitingOpponent");
        if (timeOutConfig.HasValue)
            timeOut = timeOutConfig.Value;
    }

    private void OnSessionStarted()
    {
        try
        {
            if (this == null)
                return;

            if (room == null)
                return;

            if (room.botMode)
            {
                EnterMultiplayerMatch(true);
            }
            else
            {
                List<Participant> participants = GameServices.RealTime.GetConnectedParticipants();
                List<string> playerIds = new List<string>();
                foreach (var participant in participants)
                {
                    playerIds.Add(participant.ParticipantId);
                }
                if (playerIds.IndexOf(GameServices.RealTime.GetSelf().ParticipantId) == 0)
                {
                    EnterMultiplayerMatch(false);
                }
            }
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning("OnSessionStarted NullReferenceException " + e);
        }
    }

    void OnSessionFinished(bool playerWin)
    {
        if (this == null)
            return;

        if (room == null)
            return;

        UpdateWinLosePlayerData(playerWin);
        if (room.botMode)
        {
            MultiplayerBotWin(!playerWin);
        }
    }

    void OnLoadedMatchInfo(MultiplayerInfo multiplayerInfo, int puzLevel, int puzSize, int bCoin)
    {
        opponentMultiplayerInfo = multiplayerInfo;
    }

    void OnDeclineMatchingEvent()
    {
        LeaveRoom();
        matchGroupBtn.gameObject.SetActive(true);
        findingGroupBtn.gameObject.SetActive(false);
        StopWaitingOpponentInfo();
        if (GameManager.Instance.GameState.Equals(GameState.Prepare))
            controller.ShowIfNot();
    }

    void OnPlayerDbSyncEnd()
    {
        LoadMultiplayerUserData();
    }

    void LoadMultiplayerUserData()
    {
        SetDefaultPlayerInfoData(playerMultiplayerInfo);
        if (CloudServiceManager.Instance != null && StoryPuzzlesSaver.Instance != null)
        {
            playerMultiplayerInfo.playerName = (String.IsNullOrEmpty(CloudServiceManager.playerName) ? LeaderBoardScreenUI.playerDefaultName : CloudServiceManager.playerName);
            playerMultiplayerInfo.winNumber = PlayerInfoManager.Instance.winNumber;
            playerMultiplayerInfo.loseNumber = PlayerInfoManager.Instance.loseNumber;
            playerMultiplayerInfo.playerNode = StoryPuzzlesSaver.Instance.MaxNode;

            winNumberTxt.text = playerMultiplayerInfo.winNumber.ToString();
            loseNumberTxt.text = playerMultiplayerInfo.loseNumber.ToString();
            string maxNode = playerMultiplayerInfo.playerNode.ToString();
            int maxDiff = -1;
            if (!string.IsNullOrEmpty(maxNode))
            {
                maxNode = maxNode[0].ToString().ToUpper() + (maxNode.Length > 1 ? maxNode.Substring(1) : string.Empty);
                int.TryParse(maxNode, out maxDiff);
            }
            string levelName = Takuzu.Utilities.GetLocalizePackNameByLevel(StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff));
            playerLevelTxt.text = levelName.Substring(0, 1) + levelName.Substring(1, levelName.Length - 1).ToLower();
            playerLevelTxt.color = PuzzleManager.Instance.accentColors[Mathf.Clamp((int)StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff) - 1, 0, PuzzleManager.Instance.accentColors.Count - 1)];

            CloudServiceManager.Instance.RequestAvatarForPlayer(CloudServiceManager.playerId, (response) =>
            {
                if (response == null)
                    return;
                if (response.HasErrors)
                    return;
                string url = response.ScriptData.GetString("FbAvatarUrl");
                playerMultiplayerInfo.avatarUrl = url;
                if (string.IsNullOrEmpty(url))
                    return;
                CloudServiceManager.Instance.DownloadMultiplayerAvatar(url, avatarRawImg);
            });
        }
    }

    private void SetDefaultPlayerInfoData(MultiplayerInfo playerMultiplayerInfo)
    {
        playerMultiplayerInfo.playerName = "";
        playerMultiplayerInfo.avatarUrl = "";
        playerMultiplayerInfo.playerNode = 1;
        playerMultiplayerInfo.winNumber = 0;
        playerMultiplayerInfo.loseNumber = 0;
    }

    public void UpdateWinLosePlayerData(bool isWin)
    {
        playerMultiplayerInfo.winNumber = PlayerInfoManager.Instance.winNumber;
        playerMultiplayerInfo.loseNumber = PlayerInfoManager.Instance.loseNumber;
    }

    private void OnGameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing)
        {
            UIReferences.Instance.overlayWinMenu.ResetDisconnectedState();
            controller.HideIfNot();
        }

        if (newState == GameState.Prepare)
        {
            opponentMultiplayerInfo = new MultiplayerInfo();
            winNumberTxt.text = PlayerInfoManager.Instance.winNumber.ToString();
            loseNumberTxt.text = PlayerInfoManager.Instance.loseNumber.ToString();
            playerMultiplayerInfo.winNumber = PlayerInfoManager.Instance.winNumber;
            playerMultiplayerInfo.loseNumber = PlayerInfoManager.Instance.loseNumber;
            matchGroupBtn.SetActive(true);
            findingGroupBtn.SetActive(false);
            controller.ShowIfNot();
            LeaveRoom();
        }

    }

    private void ShowAdBeforeMatching(Action callback)
    {
#if UNITY_IOS
        callback();
        return;
#endif
        ShowInterstitialAdWithCallback beforeMatchingAd = new ShowInterstitialAdWithCallback();
        beforeMatchingAd.ShowInterstitialAd(callback);
    }

    private class ShowInterstitialAdWithCallback
    {
        private Action callback;
        public void ShowInterstitialAd(Action callback)
        {
            this.callback = callback;
            if(AdDisplayer.IsAllowToShowAd() == false)
            {
                TryCallback();
                return;
            }
            //Check if ad is removed then callback immediately
            if (Advertising.IsAdRemoved())
            {
                TryCallback();
                return;
            }

            //Check if ad is not ready then callback immediately
            if (Advertising.IsInterstitialAdReady() == false)
            {
                TryCallback();
                return;
            }

            Advertising.InterstitialAdCompleted += OnInterstitalAdCompleted;
            Advertising.ShowInterstitialAd();
        }
        private void TryCallback()
        {
            if (callback != null)
            {
                try
                {
                    callback();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
        }
        private void OnInterstitalAdCompleted(InterstitialAdNetwork network, AdPlacement placement)
        {
            Advertising.InterstitialAdCompleted -= OnInterstitalAdCompleted;
            TryCallback();
        }
    }

    private void AssignUIButton()
    {
        findMatchBtn.onClick.AddListener(() =>
        {
            ShowAdBeforeMatching(() =>
            {
                if (CoinManager.Instance.Coins < 0)
                {
                    UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), String.Format(I2.Loc.ScriptLocalization.OWE_COIN,
                        Mathf.Abs(CoinManager.Instance.Coins)), I2.Loc.ScriptLocalization.Get_Coin.ToUpper(), "", () =>
                        {
                            UIReferences.Instance.overlayCoinShopUI.Show();
                        });
                    return;
                }

                if (EnergyManager.Instance.CurrentEnergy < EnergyManager.Instance.MultiplayerEnergyCost)
                {
                    UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), string.Format(I2.Loc.ScriptLocalization.NOT_ENOUGH_ENERGY, EnergyManager.Instance.MultiplayerEnergyCost),
                    I2.Loc.ScriptLocalization.Get_Energy.ToUpper(), "", () =>
                    {
                        UIReferences.Instance.overlayEnergyExchangePanel.Show();
                    });
                    return;
                }

                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), I2.Loc.ScriptLocalization.NO_INTERNET_CONNECTION,
                    I2.Loc.ScriptLocalization.OK.ToUpper(), "", () =>
                    {
                    });
                    return;
                }

                if (IsRoomConnected())
                    LeaveRoom();
                opponentMultiplayerInfo = new MultiplayerInfo();
                StopWaitingOpponentInfo();

#if UNITY_ANDROID
                UIReferences.Instance.tournamentSideUI.StartDisableHomeBtn();
#endif
                FindMatch();
            });
        });

        quickMatchBtn.onClick.AddListener(() =>
        {
            ShowAdBeforeMatching(() =>
            {
                if (CoinManager.Instance.Coins < 0)
                {
                    UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), String.Format(I2.Loc.ScriptLocalization.OWE_COIN,
                        Mathf.Abs(CoinManager.Instance.Coins)), I2.Loc.ScriptLocalization.Get_Coin.ToUpper(), "", () =>
                        {
                            UIReferences.Instance.overlayCoinShopUI.Show();
                        });
                    return;
                }

                if (EnergyManager.Instance.CurrentEnergy < EnergyManager.Instance.MultiplayerEnergyCost)
                {
                    UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), string.Format(I2.Loc.ScriptLocalization.NOT_ENOUGH_ENERGY, EnergyManager.Instance.MultiplayerEnergyCost),
                    I2.Loc.ScriptLocalization.Get_Energy.ToUpper(), "", () =>
                        {
                            UIReferences.Instance.overlayEnergyExchangePanel.Show();
                        });
                    return;
                }

                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), I2.Loc.ScriptLocalization.NO_INTERNET_CONNECTION,
                    I2.Loc.ScriptLocalization.OK.ToUpper(), "", () =>
                    {
                    });
                    return;
                }

                if (IsRoomConnected())
                    LeaveRoom();
                matchGroupBtn.SetActive(false);
                findingGroupBtn.SetActive(true);
                opponentMultiplayerInfo = new MultiplayerInfo();
                StopWaitingOpponentInfo();
                CreateQuickMatch();
            });
        });

        cancelBtn.onClick.AddListener(() =>
        {
            matchGroupBtn.SetActive(true);
            findingGroupBtn.SetActive(false);
            CancelMatchMaking();
            if (IsRoomConnected())
                LeaveRoom();
            opponentMultiplayerInfo = new MultiplayerInfo();
            StopWaitingOpponentInfo();
        });
    }

    public MatchRequest MatchRequest
    {
        get
        {
            var matchRequest = new MatchRequest()
            {
                ExclusiveBitmask = 0,
                MinPlayers = 2,
                MaxPlayers = 2
            };

            return matchRequest;
        }
    }
    private void FindMatch()
    {
        if (room != null)
            DestroyImmediate(room);
#if UNITY_EDITOR
        EnterBotMode();
#else
			GameServices.RealTime.CreateWithMatchmakerUI(MatchRequest, this);
#endif
    }

    private void CreateQuickMatch()
    {
        if (room != null)
            DestroyImmediate(room);
#if UNITY_EDITOR
        //Doing Nothing
#else
            Debug.Log("QUICK MATCH");
            GameServices.RealTime.CreateQuickMatch(MatchRequest, this);
#endif
        if (timeoutCR != null)
            StopCoroutine(timeoutCR);
        timeoutCR = StartCoroutine(MatchMakingTimeoutCR());
    }

    private Coroutine timeoutCR;

    private IEnumerator MatchMakingTimeoutCR()
    {
#if UNITY_EDITOR
        yield return new WaitForSeconds(1);
#else
        yield return new WaitForSeconds(30);
#endif
        CancelMatchMaking(false);
        yield return new WaitForSeconds(2);
        if (room == null)
            EnterBotMode();
    }

    private void EnterBotMode()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return;

        room = MultiplayerRoom.CreateNewRoom(new MultiplayerRoom.RoomOption()
        {
            botMode = true,
            defaultReady = false,
            container = transform
        });

        StartWaitingOpponentInfo(timeOut);

        WaitUntilInternetDisconnect(() =>
        {
            if (this == null)
                return;
            //* Bot fake losing internet connection
            //* Call onPeerDisconnected
            HandleFakePeerDisconnectedForBotMode(new string[] { MultiplayerRoom.BOT_DEFAULT_ID });
        });
    }

    private void WaitUntilInternetDisconnect(Action onInternetDisconnect)
    {
        StartCoroutine(WaitUntilInternetDisconnectCR(onInternetDisconnect));
    }

    private IEnumerator WaitUntilInternetDisconnectCR(Action onInternetDisconnect)
    {
        yield return new WaitForSeconds(1);
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            onInternetDisconnect();
            yield break;
        }
        StartCoroutine(WaitUntilInternetDisconnectCR(onInternetDisconnect));
    }

    private void EnterMultiplayerMode()
    {
        List<Participant> participants = GameServices.RealTime.GetConnectedParticipants();
        List<string> playerIds = new List<string>();
        foreach (var participant in participants)
        {
            playerIds.Add(participant.ParticipantId);
        }
        room = MultiplayerRoom.CreateNewRoom(new MultiplayerRoom.RoomOption()
        {
            botMode = false,
            defaultReady = false,
            container = transform,
            playerIds = playerIds,
            localPlayerId = GameServices.RealTime.GetSelf().ParticipantId,
            onLocalPlayerSendMessage = (data) => SendMessageToAll(true, data)
        });
        StartWaitingOpponentInfo(timeOut);
    }

    private void StartWaitingOpponentInfo(float timeOut)
    {
        if (waitingOpponentInfoCR != null)
            StopCoroutine(waitingOpponentInfoCR);
        waitingOpponentInfoCR = StartCoroutine(CR_WaitingOpponentInfo(timeOut));
    }
    private void StopWaitingOpponentInfo()
    {
        if (waitingOpponentInfoCR != null)
            StopCoroutine(waitingOpponentInfoCR);
    }

    IEnumerator CR_WaitingOpponentInfo(float timeOut)
    {
        float t = 0;
        while (string.IsNullOrEmpty(opponentMultiplayerInfo.playerName))
        {
            t += Time.deltaTime;
            if (t >= timeOut)
            {
                CancelMatchMaking();
                if (IsRoomConnected())
                    LeaveRoom();
                opponentMultiplayerInfo = new MultiplayerInfo();
                CreateQuickMatch();
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();
        controller.HideIfNot();
        UIReferences.Instance.matchingPanelController.SetTitle(I2.Loc.ScriptLocalization.MATCHED.ToUpper());
        UIReferences.Instance.matchingPanelController.Show();
    }

    public void SendMessageToAll(bool sendReliable, byte[] data)
    {
#if UNITY_EDITOR
        OnRealTimeMessageReceived("EDITOR_TEST", data);
#else
            GameServices.RealTime.SendMessageToAll(sendReliable, data);
#endif
    }

    public void SendMessage(bool sendReliable, byte[] data, Participant targetParticipant)
    {
        if (targetParticipant == null)
        {
            return;
        }
        GameServices.RealTime.SendMessage(sendReliable, targetParticipant.ParticipantId, data);
    }

    public void CancelMatchMaking(bool stopTimoutCR = true)
    {
        Debug.Log("cancel match making");
#if UNITY_EDITOR

#else
                //GameServices.RealTime.CancelMatchmaking();
#endif
        if (stopTimoutCR == true && timeoutCR != null)
            StopCoroutine(timeoutCR);
    }
    public void LeaveRoom()
    {
        //* manually clean up multiplayer object before actually call leave room */
        //* so that the onLeftRoom callback from EM will not clean multiplayer objects again */
        OnLeftRoomCleanUp(true);
#if UNITY_EDITOR

#else
        Debug.Log("Leave ROOM");
        if(IsRoomConnected()){
            GameServices.RealTime.LeaveRoom();
        }
#endif
    }

    public bool IsRoomConnected()
    {
        return GameServices.RealTime.IsRoomConnected();
    }

    public Participant GetParticipant(string participantId)
    {
        return GameServices.RealTime.GetParticipant(participantId);
    }

    private void OnLeftRoomCleanUp(bool manualCall = false)
    {
        //* Player has left the room clean up multiplayer objects and resolve result */
        //* If this is call from from gameServices (manualCall = false) => local player left room remove all player in the room so that on resolve result in session can resolve correctly*/
        if (room == null)
            return;
        if (manualCall == false)
            room.RemovePlayers(room.GetAllPlayers());
        DestroyImmediate(room.gameObject);
    }

    public void OnLeftRoom()
    {
        if (this == null)
            return;

        if (room != null && room.botMode)
            return;

        Debug.Log("I disconnected");
        OnLeftRoomCleanUp();
    }

    public void OnParticipantLeft(Participant participant)
    {
        if (this == null)
            return;
        Debug.Log("P left");
    }

    public void OnPeersConnected(string[] participantIds)
    {
        if (this == null)
            return;

        Debug.Log("Peer connected");
    }

    public void HandleFakePeerDisconnectedForBotMode(string[] participantIds)
    {
        if ((participantIds.ToList().Contains(MultiplayerRoom.PLAYER_DEFAULT_ID) ||
        Application.internetReachability == NetworkReachability.NotReachable) && GameManager.Instance.GameState == GameState.Playing)
        {
            Debug.Log("Multiplayer BOT On Peer Disconnected - Self Disconnect");
            LeaveRoom();
            GameManager.Instance.PrepareGame();
        }
        else
        {
            Debug.Log("Multiplayer BOT On Peer Disconnected - Other Disconnect");
            StartCoroutine(DelayHandleOnPeersDisConnectedEvent(participantIds));
        }
    }

    public void OnPeersDisconnected(string[] participantIds)
    {
        if (this == null)
            return;

        if (room != null && room.botMode)
            return;

        if ((participantIds.ToList().Contains(GameServices.RealTime.GetSelf().ParticipantId) ||
        Application.internetReachability == NetworkReachability.NotReachable) && GameManager.Instance.GameState == GameState.Playing)
        {
            Debug.Log("Multiplayer On Peer Disconnected - Self Disconnect");
            LeaveRoom();
            GameManager.Instance.PrepareGame();
        }
        else
        {
            Debug.Log("Multiplayer On Peer Disconnected - Other Disconnect");
            StartCoroutine(DelayHandleOnPeersDisConnectedEvent(participantIds));
        }
    }

    private IEnumerator DelayHandleOnPeersDisConnectedEvent(string[] participantIds)
    {
        yield return null;
        yield return null;
        UIReferences.Instance.overlayWinMenu.SetOpponentDisconnected();
        if ((GameManager.Instance.GameState.Equals(GameState.Prepare) || GameManager.Instance.GameState.Equals(GameState.GameOver)) && UIReferences.Instance.matchingPanelController.IsShowing)
        {
            if (UIReferences.Instance.matchingPanelController.IsShowing)
            {
                UIReferences.Instance.matchingPanelController.Hide();

                UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), I2.Loc.ScriptLocalization.MULTIPLAYER_OPPONENT_DECLINE,
                    I2.Loc.ScriptLocalization.OK.ToUpper(), "", () =>
                    {
                        UIReferences.Instance.overlayWinMenu.OnDeclineMatchingEvent();
                    });
                matchGroupBtn.gameObject.SetActive(true);
                findingGroupBtn.gameObject.SetActive(false);
            }
        }

        if (room != null)
        {
            room.RemovePlayers(participantIds);
            LeaveRoom();
        }

        if (GameManager.Instance.GameState.Equals(GameState.Prepare))
            controller.ShowIfNot();
        OpponentDisconnected();
    }

    public void OnRealTimeMessageReceived(string senderId, byte[] data)
    {
        if (this == null)
            return;

        if (room == null)
            return;

        if (room.botMode)
            return;

        if (room != null)
            room.ReceiveMessage(senderId, data);
    }

    public void OnRoomConnected(bool success)
    {
        if (this == null)
            return;

        if (room != null)
        {
            GameServices.RealTime.LeaveRoom();
            return;
        }

        // cancel matching if in bot mode
        if (room != null && room.botMode)
            return;

        Debug.Log("FOUND ROOM");
        if (success == false)
            return;
        if (timeoutCR != null)
            StopCoroutine(timeoutCR);
        EnterMultiplayerMode();
    }

    public void OnRoomSetupProgress(float percent)
    {
        if (this == null)
            return;
        Debug.Log("Progress: " + percent);
    }

    public bool ShouldReinviteDisconnectedPlayer(Participant participant)
    {
        return false;
    }

    private void OnApplicationQuit()
    {
        LeaveRoom();
    }
    private double unfocusMilis = 0;
    private double maxUnfocusTime
    {
        get
        {
#if UNITY_ANDROID
            return 0;
#else
            return 5000;
#endif
        }
    }
    void OnApplicationFocus(bool focusStatus)
    {
        if (focusStatus)
        {
            if (room != null && MultiplayerSession.Instance != null)
            {
                if ((GameServices.RealTime.GetConnectedParticipants() != null && GameServices.RealTime.GetConnectedParticipants().Count < 2) || GetCurrentMilis() - unfocusMilis > maxUnfocusTime)
                {
                    LeaveRoom();
                    if (GameManager.Instance.GameState.Equals(GameState.Playing) || GameManager.Instance.GameState.Equals(GameState.Paused))
                    {
                        StartCoroutine(CR_ShowSuspendPopup());
                    }
                    else
                    {
                        if (GameManager.Instance.GameState.Equals(GameState.Prepare))
                        {
                            UIReferences.Instance.matchingPanelController.Hide();
                            matchGroupBtn.SetActive(true);
                            findingGroupBtn.SetActive(false);
                            controller.ShowIfNot();
                            UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), I2.Loc.ScriptLocalization.MULTIPLAYER_OPPONENT_DECLINE,
                           I2.Loc.ScriptLocalization.OK.ToUpper(), "", () =>
                           {

                           });
                        }
                    }
                    GameManager.Instance.PrepareGame();
                }
            }
        }
        else
        {
            unfocusMilis = GetCurrentMilis();
        }
    }

    IEnumerator CR_ShowSuspendPopup()
    {
        yield return new WaitForSeconds(0.1f);
        UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), I2.Loc.ScriptLocalization.MULTIPLAYER_SUSPEND_MATCH,
            I2.Loc.ScriptLocalization.OK.ToUpper(), "", () =>
            {

            });
    }

    private double GetCurrentMilis()
    {
        return DateTime.UtcNow.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                ).TotalMilliseconds;
    }

    public void ResetLocalWin()
    {
        LocalUserWin = LocalOpponentWin = 0;
    }

    public void SetLocalWinLose(bool isUserWin)
    {
        if (isUserWin)
            LocalUserWin++;
        else
            LocalOpponentWin++;
    }
}
