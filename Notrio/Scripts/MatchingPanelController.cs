using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;
using UnityEngine.UI;
using Takuzu.Generator;
using System;

public class MatchingPanelController : OverlayPanel
{
    public static event System.Action ShowMatchingPanelEvent = delegate { };
    public static event System.Action AcceptMatchingEvent = delegate { };
    public static event System.Action DeclineMatchingEvent = delegate { };

    public OverlayGroupController controller;
    public Text title;
    public ClockController clockController;
    public Image bgImg;
    public Button acceptBtn;
    public Button declineBtn;
    public Button waitingBtn;
    public Text difficultTxt;
    public Text sizeTxt;
    public Text betCoinTxt;
    public RawImage playerAvatar;
    public RawImage opponentAvatar;
    public Text playerName;
    public Text opponentName;
    public Text playerLevel;
    public Text opponentLevel;
    public Text playerWinNumber;
    public Text opponentWinNumber;
    public Text playerLoseNumber;
    public Text opponentLoseNumber;
    public Texture defaultAvatar;
    private Coroutine showRematchCR;
    private Coroutine timeOutCR;
    private int timeWaitingAccept = 90;
    private long lastCountDownTick;

    private void Start()
    {
        MultiplayerRoom.LoadedMatchInfo += OnLoadedMatchInfo;
        GameManager.GameStateChanged += OnGameStateChanged;
        MultiplayerManager.OpponentDisconnected += OnOpponentDisconnected;
        MultiplayerRoom.OpponentReady += OnOpponentReady;
        acceptBtn.onClick.AddListener(() =>
        {
            if (MultiplayerRoom.Instance == null)
            {
                Hide();
                UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), I2.Loc.ScriptLocalization.MULTIPLAYER_OPPONENT_DECLINE,
                    I2.Loc.ScriptLocalization.OK.ToUpper(), "", () =>
                    {
                        UIReferences.Instance.overlayWinMenu.OnDeclineMatchingEvent();
                    });

                //* Originally this handle the case after finished puzzle game over panel is showing.
                //* But in some rare case multiplayer room get destroyed before the game started 
                //* => click accept button will prompt the other player has disconnected and return to an empty scene */

                if (GameManager.Instance.GameState == GameState.Prepare)
                {
                    //* if this is prepare state and other player has declined the match => return to main */
                    MultiplayerManager.Instance.matchGroupBtn.gameObject.SetActive(true);
                    MultiplayerManager.Instance.findingGroupBtn.gameObject.SetActive(false);
                    MultiplayerManager.Instance.controller.ShowIfNot();
                }
                return;
            }

            if (CoinManager.Instance.Coins < 0)
            {
                Hide();
                UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), String.Format(I2.Loc.ScriptLocalization.OWE_COIN,
                    Mathf.Abs(CoinManager.Instance.Coins), Mathf.Abs(CoinManager.Instance.Coins) > 1 ? "s" : ""), I2.Loc.ScriptLocalization.OK.ToUpper(), "", () =>
                    {
                        DeclineMatchingEvent();
                        UIReferences.Instance.overlayWinMenu.SetPlayerDisconnected();
                    });
                return;
            }

            if (EnergyManager.Instance.CurrentEnergy < EnergyManager.Instance.MultiplayerEnergyCost)
            {
                Hide();
                UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION.ToUpper(), string.Format(I2.Loc.ScriptLocalization.NOT_ENOUGH_ENERGY, EnergyManager.Instance.MultiplayerEnergyCost),
                I2.Loc.ScriptLocalization.OK.ToUpper(), "", () =>
                {
                    DeclineMatchingEvent();
                    UIReferences.Instance.overlayWinMenu.SetPlayerDisconnected();
                });
                return;
            }

            acceptBtn.gameObject.SetActive(false);
            waitingBtn.gameObject.SetActive(true);
            if (GameManager.Instance.GameState != GameState.GameOver)
                MultiplayerManager.Instance.ResetLocalWin();
            AcceptMatchingEvent();
        });

        declineBtn.onClick.AddListener(() =>
        {
            DeclineHandle();
        });
    }

    public void DeclineHandle()
    {
        Hide();
        UIReferences.Instance.overlayWinMenu.SetPlayerDisconnected();
        DeclineMatchingEvent();
    }

    private void OnDestroy()
    {
        MultiplayerRoom.LoadedMatchInfo -= OnLoadedMatchInfo;
        GameManager.GameStateChanged -= OnGameStateChanged;
        MultiplayerManager.OpponentDisconnected -= OnOpponentDisconnected;
        MultiplayerRoom.OpponentReady -= OnOpponentReady;
        StopAllCoroutines();
    }

    void OnOpponentReady()
    {
        if (showRematchCR != null)
        {
            StopCoroutine(showRematchCR);
        }
        showRematchCR = StartCoroutine(CR_ShowRematchingPopup());
        // if (acceptBtn.gameObject.activeSelf == false)
        //     StopCountTimeOut();
    }

    void OnOpponentDisconnected()
    {
        if (showRematchCR != null)
        {
            StopCoroutine(showRematchCR);
        }
        StopCountTimeOut();
    }

    void OnGameStateChanged(GameState newState, GameState oldState)
    {
        StopCountTimeOut();
        Hide();
    }

    void OnLoadedMatchInfo(MultiplayerInfo multiplayerInfo, int puzLevel, int puzSize, int bCoin)
    {
        SetPuzzlInfo((Level)puzLevel, (Size)puzSize, bCoin);
        SetOpponentInfo(multiplayerInfo);
    }

    public void SetPuzzlInfo(Level puzzleLevel, Size puzzleSize, int betcoin)
    {
        string diff = Utilities.GetDifficultyDisplayName(puzzleLevel);
        difficultTxt.text = diff.Substring(0, 1).ToUpper() + diff.Substring(1, diff.Length - 1).ToLower();
        betCoinTxt.text = betcoin.ToString();
        sizeTxt.text = string.Format("{0}x{1}", (int)puzzleSize, (int)puzzleSize);
        if (UIReferences.Instance.ingameBGAdapter != null)
        {
            int bgIndex = (int)puzzleLevel < 5 ? (int)puzzleLevel : 0;
            if (PersonalizeManager.NightModeEnable)
                bgImg.sprite = UIReferences.Instance.ingameBGAdapter.ingameBgs[bgIndex].nightSprite;
            else
                bgImg.sprite = UIReferences.Instance.ingameBGAdapter.ingameBgs[bgIndex].daySprite;
        }
    }

    public void SetPlayerInfo(MultiplayerInfo multiplayerInfo)
    {
        //playerName.text = multiplayerInfo.playerName;
        playerWinNumber.text = multiplayerInfo.winNumber.ToString();
        playerLoseNumber.text = multiplayerInfo.loseNumber.ToString();
        string maxNode = multiplayerInfo.playerNode.ToString();
        int maxDiff = -1;
        if (!string.IsNullOrEmpty(maxNode))
        {
            maxNode = maxNode[0].ToString().ToUpper() + (maxNode.Length > 1 ? maxNode.Substring(1) : string.Empty);
            int.TryParse(maxNode, out maxDiff);
        }
        string levelName = Takuzu.Utilities.GetLocalizePackNameByLevel(StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff));
        playerLevel.text = levelName.Substring(0, 1) + levelName.Substring(1, levelName.Length - 1).ToLower();
        playerLevel.color = PuzzleManager.Instance.accentColors[Mathf.Clamp((int)StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff) - 1, 0, PuzzleManager.Instance.accentColors.Count - 1)];
        if (MultiplayerManager.Instance.avatarRawImg.texture.name.Equals("default-avatar"))
        {
            CloudServiceManager.Instance.RequestAvatarForPlayer(CloudServiceManager.playerId, (response) =>
            {
                if (response == null)
                    return;
                if (response.HasErrors)
                    return;
                string url = response.ScriptData.GetString("FbAvatarUrl");
                multiplayerInfo.avatarUrl = url;
                if (string.IsNullOrEmpty(url))
                    return;
                CloudServiceManager.Instance.DownloadMultiplayerAvatar(url, playerAvatar);
            });
        }
        else
        {
            playerAvatar.texture = MultiplayerManager.Instance.avatarRawImg.texture;
        }
    }

    public void SetOpponentInfo(MultiplayerInfo multiplayerInfo)
    {
        opponentName.text = multiplayerInfo.playerName;
        string maxNode = multiplayerInfo.playerNode.ToString();
        int maxDiff = -1;
        if (!string.IsNullOrEmpty(maxNode))
        {
            maxNode = maxNode[0].ToString().ToUpper() + (maxNode.Length > 1 ? maxNode.Substring(1) : string.Empty);
            int.TryParse(maxNode, out maxDiff);
        }
        string levelName = Takuzu.Utilities.GetLocalizePackNameByLevel(StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff));
        opponentLevel.text = levelName.Substring(0, 1) + levelName.Substring(1, levelName.Length - 1).ToLower();
        opponentLevel.color = PuzzleManager.Instance.accentColors[Mathf.Clamp((int)StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff) - 1, 0, PuzzleManager.Instance.accentColors.Count - 1)];
        opponentWinNumber.text = multiplayerInfo.winNumber.ToString();
        opponentLoseNumber.text = multiplayerInfo.loseNumber.ToString();

        string avatarUrl = multiplayerInfo.avatarUrl;
        if (string.IsNullOrEmpty(avatarUrl))
            return;
        CloudServiceManager.Instance.DownloadMultiplayerAvatar(avatarUrl, opponentAvatar);
    }

    public void SetTitle(string title)
    {
        this.title.text = title;
    }

    IEnumerator CR_ShowRematchingPopup()
    {
        yield return new WaitUntil(() => !UIReferences.Instance.overlayTaskPanel.IsShowing);
        yield return new WaitForSeconds(0.5f);
        if (!GameManager.Instance.GameState.Equals(GameState.Prepare) && !IsShowing)
        {
            title.text = I2.Loc.ScriptLocalization.REMATCH.ToUpper();
            Show();
            if (GameManager.Instance.GameState.Equals(GameState.GameOver))
                UIReferences.Instance.overlayWinMenu.Hide();
        }
    }

    public override void Show()
    {
        if (!IsShowing)
        {
            StartCountTimeOut();
            ShowMatchingPanelEvent();
            IsShowing = true;
            controller.ShowIfNot();
            transform.BringToFront();
            onPanelStateChanged(this, true);
            acceptBtn.gameObject.SetActive(true);
            declineBtn.gameObject.SetActive(true);
            waitingBtn.gameObject.SetActive(false);
            opponentAvatar.texture = defaultAvatar;
            SetPlayerInfo(MultiplayerManager.Instance.playerMultiplayerInfo);
            SetOpponentInfo(MultiplayerManager.Instance.opponentMultiplayerInfo);
        }
    }

    public override void Hide()
    {
        if (IsShowing)
        {
            IsShowing = false;
            controller.HideIfNot();
            onPanelStateChanged(this, false);
            StopCountTimeOut();
            if (GameManager.Instance.GameState.Equals(GameState.GameOver))
                UIReferences.Instance.overlayWinMenu.Show();
        }
    }

    public void StartCountTimeOut()
    {
        if (timeOutCR != null)
            StopCoroutine(timeOutCR);
        timeOutCR = StartCoroutine(CR_CountTimeOut());
    }

    public void StopCountTimeOut()
    {
        Debug.Log("Stop count down timer");
        if (timeOutCR != null)
            StopCoroutine(timeOutCR);
    }

    IEnumerator CR_CountTimeOut()
    {
        yield return new WaitForSeconds(0.1f);
        lastCountDownTick = DateTime.UtcNow.Ticks;
        TimeSpan timeSpan;
        timeSpan = new TimeSpan(0, 0, timeWaitingAccept);
        float timeRemain;
        clockController.UpdateTime(timeSpan);
        timeRemain = timeWaitingAccept - ((DateTime.UtcNow.Ticks - lastCountDownTick) / TimeSpan.TicksPerSecond);
        while (timeRemain > 0)
        {
            if (MultiplayerSession.Instance != null)
                yield break;
            yield return new WaitForSeconds(1);
            timeRemain = Mathf.Max(0, timeWaitingAccept - ((DateTime.UtcNow.Ticks - lastCountDownTick) / TimeSpan.TicksPerSecond));
            timeSpan = new TimeSpan(0, 0, (int)timeRemain);
            clockController.UpdateTime(timeSpan);
        }
        Hide();
        DeclineMatchingEvent();
    }
}
