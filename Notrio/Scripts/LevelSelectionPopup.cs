using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;
using UnityEngine.UI;
using Takuzu.Generator;
using System;
using LionStudios.Suite.Analytics;
using LionStudios.Suite.Ads;
using static StoryPuzzlesSaver;

public class LevelSelectionPopup : OverlayPanel
{
    public Button closeButton;
    public Button playButton;
    public OverlayGroupController controller;
    public Text panelTitle;
    public Text panelEnengyCost;
    public Text packName;
    public Text sizeText;
    public Text playBtnText;
    public Text yearText;
    public GameObject currentIcon;
    public GameObject solvedIcon;
    public GameObject stateContainer;
    public Color currentColor;
    public Color solvedColor;
    public CustomProgressBar progressBar;
    public List<Image> characterImages;
    public Image background;
    private string currentBg = "";
    private void Awake()
    {
        LevelSelector.onClickOnPlayablePuzzle += OnClickOnPlayablePuzzle;
    }
    private void Start()
    {
        closeButton.onClick.AddListener(delegate
        {
            Hide();
        });
    }

    private void OnDestroy()
    {
        LevelSelector.onClickOnPlayablePuzzle -= OnClickOnPlayablePuzzle;
    }

    private void OnClickOnPlayablePuzzle(string puzzleId)
    {
        if ((int)PuzzleManager.Instance.GetPuzzleById(puzzleId).level >= 3)
        {
            if (OndemandResourceLoader.IsBundleLoaded("textures") == false)
            {
                OndemandResourceLoader.Request rq = OndemandResourceLoader.LoadAssetsBundle("textures");
                DisplayAssetDownloadingProgress(rq);
                return;
            }
        }
        if ((int)PuzzleManager.Instance.GetPuzzleById(puzzleId).level == 5 && PuzzleManager.Instance.GetPuzzleById(puzzleId).size == Size.Twelve)
        {
            if (OndemandResourceLoader.IsBundleLoaded("video") == false)
            {
                OndemandResourceLoader.Request rq = OndemandResourceLoader.LoadAssetsBundle("video");
                DisplayAssetDownloadingProgress(rq);
                return;
            }
        }
        UpdateUIElements(puzzleId);
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(delegate
        {
            GameManager.Instance.PlayAPuzzle(puzzleId);
            Hide();

            #region Lion Event
            //TODO: LionAnalytics.MissionStarted
            AlolAnalytics.MissionStarted(puzzleId);
            #endregion
        });
        Show();
    }

    private void DisplayAssetDownloadingProgress(OndemandResourceLoader.Request request)
    {
        StartCoroutine(DisplayAssetDownloadingProgressCR(request));
    }

    private IEnumerator DisplayAssetDownloadingProgressCR(OndemandResourceLoader.Request request)
    {
        LoadingScreen.Instance.ActivateLoadingGraphic();
        LoadingScreen.Instance.loadingAnim.Play(0);
        LoadingScreen.Instance.EnableDescriptionText();
        while (request.status.finished == false)
        {
            LoadingScreen.Instance.SetProgressDisplay(I2.Loc.ScriptLocalization.DOWNLOAD_RESOURCE, request.status.progress);
            yield return null;
        }
        LoadingScreen.Instance.DisableDiscriptionText();
        LoadingScreen.Instance.loadingAnim.Play(1);
        LoadingScreen.Instance.DeactivateLoadingGraphic(3);
    }

    private void UpdateUIElements(string puzzleId)
    {
        Debug.Log(puzzleId);
        Puzzle puzzle = PuzzleManager.Instance.GetPuzzleById(puzzleId);
        sizeText.text = String.Format("{0}x{0}", (int)puzzle.size);
        //string levelString = PuzzleManager.Instance.levelInfors.Find(item => (item.level == puzzle.level)).levelName;
        string levelString = Utilities.GetDifficultyDisplayName(puzzle.level);
        string head = levelString.Substring(0, 1).ToUpper();
        string tail = levelString.Substring(1, levelString.Length - 1).ToLower();
        packName.text = head + tail;
        panelEnengyCost.text = EnergyManager.Instance.GetCostByLevel(puzzle, EnergyManager.Instance.StoryModeEnergyCost).ToString();
        if (PuzzleManager.Instance.IsPuzzleInProgress(puzzleId))
            playBtnText.text = I2.Loc.ScriptLocalization.RESUME.ToUpper();
        else
            playBtnText.text = I2.Loc.ScriptLocalization.START.ToUpper();
        int nodeIndex = StoryPuzzlesSaver.GetIndexNode(puzzle.level, puzzle.size);
        int preAge = nodeIndex > 0 ? PuzzleManager.Instance.ageList[nodeIndex - 1] : 0;

        currentBg = String.Format("age-{0}", nodeIndex);
        background.sprite = Background.Get(currentBg);
        int realAge = PuzzleManager.Instance.ageList[nodeIndex];
        panelTitle.text = string.Format(I2.Loc.ScriptLocalization.LEVEL_SELECTION_POPUP_MILE_STONE.ToUpper(), (nodeIndex + 1).ToString());
        string currentMileStone = String.Format("{0}.{1}", nodeIndex + 1,
            StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex) < StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex)
            ? StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex) + 1 : StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
        float progress = ((float)StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex)) / ((float)StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
        yearText.text = currentMileStone;
        yearText.enabled = progress < 1;
        currentIcon.SetActive(nodeIndex == StoryPuzzlesSaver.Instance.MaxNode + 1);
        solvedIcon.SetActive(nodeIndex < StoryPuzzlesSaver.Instance.MaxNode + 1);
        progressBar.SetProgress(progress, progress != 1, StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
        //progressBar.SetDisplayText(currentMileStone);
        progressBar.transform.parent.gameObject.SetActive(currentIcon.activeSelf);
        foreach (var item in characterImages)
        {
            item.gameObject.SetActive(false);
        }
        characterImages[(int)puzzle.level - 1].gameObject.SetActive(true);
    }

    public override void Show()
    {
        IsShowing = true;
        controller.ShowIfNot();
        transform.BringToFront();
        onPanelStateChanged(this, true);
    }

    public override void Hide()
    {
        Background.Unload(currentBg);
        background.sprite = null;
        IsShowing = false;
        controller.HideIfNot();
        onPanelStateChanged(this, false);
    }
}
