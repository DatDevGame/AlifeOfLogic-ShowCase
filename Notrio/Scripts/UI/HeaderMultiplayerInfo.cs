using System.Collections;
using Takuzu;
using UnityEngine;
using UnityEngine.UI;

public class HeaderMultiplayerInfo : MonoBehaviour
{

    [Header("References Object")]
    public RawImage playerAvatarRawImg;
    public RawImage opponentAvatarRawImg;
    public RectTransform playerAvatarContainer;
    public RectTransform opponentAvatarContainer;
    public Image playerProgressBar;
    public Image opponentProgressBar;
    public Text playerNameTxt;
    public Text opponentNameTxt;
    public Text playerLevelTxt;
    public Text opponentLevelTxt;
    public Button changeProgressBtn;
    public Texture defaultAvatar;

    [Header("Config")]
    public Vector2 defaultSize;
    public Vector2 highlightSize;
    public Color defaultColor;
    public Color highlightColor;

    [HideInInspector]
    public bool isHighlightPlayer;
    private Color originalPlayerColor;
    private Color originalOpponentColor;
    private Coroutine changeCoroutine;
    private Coroutine flickerCoroutine;

    private void Start()
    {
        originalPlayerColor = playerProgressBar.color;
        originalOpponentColor = opponentProgressBar.color;
        changeProgressBtn.onClick.AddListener(() =>
        {
            if (!LogicalBoard.Instance.isPlayingRevealAnim)
            {
                ChangeShowProgress();
                MultiplayerSession.RevealNextPlayer();
            }
        });

        MultiplayerRoom.LoadedMultiplayerPuzzle += ResetHeaderInfo;
    }

    private void OnDestroy()
    {
        MultiplayerRoom.LoadedMultiplayerPuzzle -= ResetHeaderInfo;
        StopFlickerAni();
    }

    public void ResetHeaderInfo()
    {
        isHighlightPlayer = true;
        playerAvatarContainer.sizeDelta = highlightSize;
        opponentAvatarContainer.sizeDelta = defaultSize;
        playerProgressBar.color = originalPlayerColor;
        opponentProgressBar.color = originalOpponentColor;
        playerProgressBar.transform.localScale = new Vector3(0, 1, 1);
        opponentProgressBar.transform.localScale = new Vector3(0, 1, 1);
        changeProgressBtn.interactable = true;
        playerProgressBar.color = originalPlayerColor;
        opponentProgressBar.color = new Color(originalOpponentColor.r, originalOpponentColor.g, originalOpponentColor.b, 0.5f);
        SetPlayerInfo();
        SetOpponentInfo();
    }

    public void SetPlayerInfo()
    {
        //playerNameTxt.text = MultiplayerManager.Instance.playerMultiplayerInfo.playerName;
        int maxNode = MultiplayerManager.Instance.playerMultiplayerInfo.playerNode;
        int maxDiff = 0;
        maxDiff = Mathf.Max(0, maxNode);
        string levelName = Takuzu.Utilities.GetLocalizePackNameByLevel(StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff));
        playerLevelTxt.text = levelName.Substring(0, 1) + levelName.Substring(1, levelName.Length - 1).ToLower();
        //playerLevelTxt.color = PuzzleManager.Instance.accentColors[Mathf.Clamp((int)StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff) - 1, 0, PuzzleManager.Instance.accentColors.Count - 1)];
        if (MultiplayerManager.Instance.avatarRawImg.texture.Equals("default-avatar"))
        {
            CloudServiceManager.Instance.RequestAvatarForPlayer(CloudServiceManager.playerId, (response) =>
            {
                if (response.HasErrors)
                    return;
                string url = response.ScriptData.GetString("FbAvatarUrl");
                MultiplayerManager.Instance.playerMultiplayerInfo.avatarUrl = url;
                if (string.IsNullOrEmpty(url))
                    return;
                CloudServiceManager.Instance.DownloadMultiplayerAvatar(url, playerAvatarRawImg);
            });
        }
        else
        {
            playerAvatarRawImg.texture = MultiplayerManager.Instance.avatarRawImg.texture;
        }
    }

    public void SetOpponentInfo()
    {
        opponentNameTxt.text = MultiplayerManager.Instance.opponentMultiplayerInfo.playerName;
        int maxNode = MultiplayerManager.Instance.opponentMultiplayerInfo.playerNode;
        int maxDiff = 0;
        maxDiff = Mathf.Max(0, maxNode);
        string levelName = Takuzu.Utilities.GetLocalizePackNameByLevel(StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff));
        opponentLevelTxt.text = levelName.Substring(0, 1) + levelName.Substring(1, levelName.Length - 1).ToLower();
        opponentAvatarRawImg.texture = defaultAvatar;
        if (UIReferences.Instance.matchingPanelController.opponentAvatar.texture.name.Equals("default-avatar"))
        {
            string avatarUrl = MultiplayerManager.Instance.opponentMultiplayerInfo.avatarUrl;
            if (string.IsNullOrEmpty(avatarUrl))
                return;
            CloudServiceManager.Instance.DownloadMultiplayerAvatar(avatarUrl, opponentAvatarRawImg);
        }
        else
            opponentAvatarRawImg.texture = UIReferences.Instance.matchingPanelController.opponentAvatar.texture;
    }

    public void SetCurrentPlayerProgress(float progress)
    {
        playerProgressBar.transform.localScale = new Vector3(progress, 1, 1);
    }

    public void SetOpponentProgress(float progress)
    {
        opponentProgressBar.transform.localScale = new Vector3(progress, 1, 1);
    }

    public void ChangeShowProgress()
    {
        isHighlightPlayer = !isHighlightPlayer;
        if (changeCoroutine != null)
            StopCoroutine(changeCoroutine);
        changeCoroutine = StartCoroutine(CR_ChangeShowProgress());
    }

    IEnumerator CR_ChangeShowProgress()
    {
        changeProgressBtn.interactable = false;
        Vector2 playerStartSize = playerAvatarContainer.sizeDelta;
        Vector2 opponentStartSize = opponentAvatarContainer.sizeDelta;
        float value = 0;
        float speed = 1 / 0.3f;
        StartElasticityAni(isHighlightPlayer);
        while (value < 1)
        {
            value += Time.deltaTime * speed;
            if (isHighlightPlayer)
            {
                //playerAvatarContainer.sizeDelta = Vector2.Lerp(playerStartSize, highlightSize, value);
                opponentAvatarContainer.sizeDelta = Vector2.Lerp(opponentStartSize, defaultSize, value);
                playerProgressBar.color = Color.Lerp(new Color(originalPlayerColor.r, originalPlayerColor.g, originalPlayerColor.b, 0.5f), originalPlayerColor, value);
                opponentProgressBar.color = Color.Lerp(originalOpponentColor, new Color(originalOpponentColor.r, originalOpponentColor.g, originalOpponentColor.b, 0.5f), value);
            }
            else
            {
                playerAvatarContainer.sizeDelta = Vector2.Lerp(playerStartSize, defaultSize, value);
                //opponentAvatarContainer.sizeDelta = Vector2.Lerp(opponentStartSize, highlightSize, value);
                playerProgressBar.color = Color.Lerp(originalPlayerColor, new Color(originalPlayerColor.r, originalPlayerColor.g, originalPlayerColor.b, 0.5f), value);
                opponentProgressBar.color = Color.Lerp(new Color(originalOpponentColor.r, originalOpponentColor.g, originalOpponentColor.b, 0.5f), originalOpponentColor, value);
            }
            yield return null;
        }
    }

    public void StartElasticityAni(bool isPlayerProgress)
    {
        if (flickerCoroutine != null)
            StopCoroutine(flickerCoroutine);
        flickerCoroutine = StartCoroutine(CR_FlickerAvatar(isPlayerProgress));
    }

    public void StopFlickerAni()
    {
        if (flickerCoroutine != null)
            StopCoroutine(flickerCoroutine);
    }

    IEnumerator CR_FlickerAvatar(bool isPlayerProgress)
    {
        float value = 0;
        float speed = 1 / 0.15f;
        int numberFlicker = 5;
        bool isSwitched = true;
        while (numberFlicker > 0)
        {
            while (value < 1)
            {
                value += Time.deltaTime * speed;
                if (isHighlightPlayer)
                {
                    if (isSwitched)
                        playerAvatarContainer.sizeDelta = Vector2.Lerp(defaultSize, highlightSize, value);
                    else
                        playerAvatarContainer.sizeDelta = Vector2.Lerp(highlightSize, defaultSize, value);
                }
                else
                {
                    if (isSwitched)
                        opponentAvatarContainer.sizeDelta = Vector2.Lerp(defaultSize, highlightSize, value);
                    else
                        opponentAvatarContainer.sizeDelta = Vector2.Lerp(highlightSize, defaultSize, value);
                }
                yield return null;
            }
            value = 0;
            isSwitched = !isSwitched;
            numberFlicker--;
        }
        changeProgressBtn.interactable = true;
    }
}
