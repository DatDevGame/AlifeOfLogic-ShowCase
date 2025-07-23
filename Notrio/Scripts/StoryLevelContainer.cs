using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu.Generator;
using Takuzu;
using System;

public class StoryLevelContainer : MonoBehaviour {
    public string UIGuideSaveKey;
    public string UIGuideFirstPuzzleSaveKey;
    public GameObject puzzleLevelTemplate;
	public SnappingScroller scroller;
    public List<Image> characterImgs;
	[Serializable]
	public class levelSizes {
		public Level level;
		public Text packName;
		public List<Image> accentImageList;
        public List<Text> accentTextList;
		[HideInInspector]
		public List<Size> currentLevelSizes;
		[HideInInspector]
		public PuzzlePack pack;
		[HideInInspector]
		public List<LevelSelector> selectorPool;
	}

	public List<levelSizes> packLevelSizes;
	public List<Transform> storyModeContainer;
    public Color currentAccentColor;
    public static StoryLevelContainer instance;
    private Dictionary<int, StoryPuzzlesSaver.SolvableStatus> levelSelectorProgress = new Dictionary<int, StoryPuzzlesSaver.SolvableStatus>();
    private int containerAge = -1;
    private float colorBias = 1.075f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            DestroyImmediate(instance.gameObject);
            instance = this;
        }

        UpdateStoryList();
        UpdateScroller();
        StoryPuzzlesSaver.maxNodeChanged += OnMaxNodeChanged;
        StoryPuzzlesSaver.puzzleIndexChanged += OnPuzzleIndexChanged;
        CloudServiceManager.onPlayerDbSyncEnd += OnPlayerDBSynced;
		scroller.onScrollingViewPositionChanged += OnScrollViewPositionChanged;
        PlayerDb.Resetted += OnPlayerDBResetted;
        GameManager.GameStateChanged += OnGameStateChanged;
    }

    private void OnScrollViewPositionChanged(float percent)
	{
		UpdateAccentColor(percent);
	}

    void Update()
    {
        if(scroller.GetCurrentPosition() != currentColorPercent)
            UpdateAccentColor(scroller.GetCurrentPosition());
    }
    private float currentColorPercent = 0;
	private void UpdateAccentColor(float percent)
	{
        if(currentColorPercent == percent)
            return;
        currentColorPercent = percent;
		float lerpColorPosition = percent*4;
		int currentPack = Mathf.Clamp((int) lerpColorPosition,0, packLevelSizes.Count - 1);
		int nextPack = Mathf.Clamp(currentPack + 1, 0, packLevelSizes.Count - 1);
		float accentLerpFactor = lerpColorPosition % 1;
        List<Color> accentColors = PuzzleManager.Instance.accentColors;
		Color currentColor = Color.Lerp(accentColors[Mathf.Clamp((int) packLevelSizes[currentPack].level - 1,0,accentColors.Count - 1)], accentColors[Mathf.Clamp((int) packLevelSizes[nextPack].level - 1, 0, accentColors.Count - 1)], accentLerpFactor);
        Color solvedColor = currentColor;
        solvedColor.r = solvedColor.r * colorBias;
        solvedColor.g = solvedColor.g * colorBias;
        solvedColor.b = solvedColor.b * colorBias;

        foreach (var image in packLevelSizes[currentPack].accentImageList)
		{
			image.color = currentColor;
		} 
		foreach (var image in packLevelSizes[nextPack].accentImageList)
		{
			image.color = currentColor;
		}
        foreach (var text in packLevelSizes[currentPack].accentTextList)
		{
			text.color = currentColor;
		} 
		foreach (var text in packLevelSizes[nextPack].accentTextList)
		{
			text.color = currentColor;
		}
        currentAccentColor = currentColor;
    }

	private void OnMaxNodeChanged(int newValue, int oldValue)
	{
        UpdateScroller();
	}

	private void UpdateScroller()
	{
		CoroutineHelper.Instance.PostponeActionUntil(()=>{
			if(scroller!=null){
				scroller.SnapIndex = packLevelSizes.IndexOf(packLevelSizes.Find(level =>  level.level == StoryPuzzlesSaver.Instance.GetMaxDifficultLevel()));
				scroller.Snap();
			}
		},()=>StoryPuzzlesSaver.Instance!=null);
	}

	private void UpdateStoryList()
    {
        StartCoroutine(CR_WaitForPuzzleManager());
    }

    private void OnDestroy()
    {
		StoryPuzzlesSaver.maxNodeChanged -= OnMaxNodeChanged;
        StoryPuzzlesSaver.puzzleIndexChanged -= OnPuzzleIndexChanged;
        scroller.onScrollingViewPositionChanged -= OnScrollViewPositionChanged;
        CloudServiceManager.onPlayerDbSyncEnd -= OnPlayerDBSynced;
        PlayerDb.Resetted -= OnPlayerDBResetted;
        GameManager.GameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState arg1, GameState arg2)
    {
        if (arg1.Equals(GameState.Prepare))
        {
            if (UIGuide.instance && packLevelSizes[0].selectorPool.Count > 0 && !UIGuide.instance.IsGuildeShown(UIGuideFirstPuzzleSaveKey))
            {
                CoroutineHelper.Instance.PostponeActionUntil(() =>
                {
                    if (packLevelSizes[0].selectorPool[0].gameObject)
                    {
                        scroller.SnapIndex = 0;
                        scroller.Snap();
                        ExtrudedButton selectorBtn = packLevelSizes[0].selectorPool[0].GetComponent<ExtrudedButton>();
                        List<Image> maskedObject = selectorBtn.content.GetComponentsInChildren<Image>(true).ToList();
                        maskedObject.Add(characterImgs[scroller.SnapIndex]);
                        maskedObject.Add(selectorBtn.content.GetComponent<Image>());
                        maskedObject.Add(selectorBtn.targetGraphic.GetComponent<Image>());
                        UIGuide.UIGuideInformation uIGuideInformation = new UIGuide.UIGuideInformation(UIGuideFirstPuzzleSaveKey, maskedObject, characterImgs[scroller.SnapIndex].gameObject, packLevelSizes[0].selectorPool[0].gameObject, GameState.Prepare);
                        uIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUIDE_FIRST_PUZZLE;
                        uIGuideInformation.clickableButton = packLevelSizes[0].selectorPool[0].button;
                        Vector3[] worldConners = new Vector3[4];
                        characterImgs[scroller.SnapIndex].rectTransform.GetWorldCorners(worldConners);
                        uIGuideInformation.bubleTextWidth = 345;
                        uIGuideInformation.transformOffset = new Vector3(0, (worldConners[1].y - worldConners[0].y) * 0.8f, 0);

                        UIGuide.instance.HighLightThis(uIGuideInformation);
                    }
                }, () => GameManager.Instance.GameState.Equals(GameState.Prepare));
            }
            if (StoryPuzzlesSaver.Instance.MaxNode >= 0 && UIGuide.instance && packLevelSizes[0].selectorPool.Count > 0 && !UIGuide.instance.IsGuildeShown(UIGuideSaveKey))
            {
                CoroutineHelper.Instance.PostponeActionUntil(() =>
                {
                    CoroutineHelper.Instance.DoActionDelay(() =>
                    {
                        if (packLevelSizes[0].selectorPool[0].gameObject)
                        {
                            scroller.SnapIndex = 0;
                            scroller.Snap();
                            ExtrudedButton selectorBtn = packLevelSizes[0].selectorPool[0].GetComponent<ExtrudedButton>();
                            List<Image> maskedObject = selectorBtn.content.GetComponentsInChildren<Image>(true).ToList();
                            maskedObject.Add(characterImgs[scroller.SnapIndex]);
                            maskedObject.Add(selectorBtn.content.GetComponent<Image>());
                            maskedObject.Add(selectorBtn.targetGraphic.GetComponent<Image>());
                            UIGuide.UIGuideInformation uIGuideInformation = new UIGuide.UIGuideInformation(UIGuideSaveKey, maskedObject, characterImgs[scroller.SnapIndex].gameObject, packLevelSizes[0].selectorPool[0].gameObject, GameState.Prepare);
                            uIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUIDE_COMPLETE_MILESTONE;
                            Vector3[] worldConners = new Vector3[4];
                            characterImgs[scroller.SnapIndex].rectTransform.GetWorldCorners(worldConners);
                            uIGuideInformation.bubleTextWidth = 440;
                            uIGuideInformation.transformOffset = new Vector3(0, (worldConners[1].y - worldConners[0].y) * 0.8f, 0);

                            UIGuide.instance.HighLightThis(uIGuideInformation);
                        }
                    }, 0);
                }, () => GameManager.Instance.GameState.Equals(GameState.Prepare));
            }
        }
    }

    private void OnPlayerDBResetted()
    {
        levelSelectorProgress.Clear();
        UpdateStoryList();
    }

    private void OnPlayerDBSynced()
    {
        int nodeIndexB = StoryPuzzlesSaver.Instance.MaxNode;
        int preAgeB = nodeIndexB > 0 ? PuzzleManager.Instance.ageList[nodeIndexB - 1] : 0;
        int realAgeB = preAgeB + StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndexB);
        if (realAgeB != containerAge)
        {
            levelSelectorProgress.Clear();
            UpdateStoryList();
        }
    }

    private void OnPuzzleIndexChanged()
    {
        UpdateStoryList();
    }

    private IEnumerator CR_WaitForPuzzleManager()
    {
        yield return new WaitUntil(() => { return PuzzleManager.Instance!= null && StoryPuzzlesSaver.Instance!= null; });
        yield return new WaitForEndOfFrame();
		foreach (var levelSizes in packLevelSizes)
		{
			levelSizes.currentLevelSizes = PuzzleManager.Instance.packSizesList.Find(pack => pack.packLevel == levelSizes.level).sizes;
			levelSizes.pack = PuzzleManager.Instance.packs.Find(pack => pack.difficulties.Contains(levelSizes.level));
            levelSizes.packName.text = Takuzu.Utilities.GetLocalizePackNameByLevel(levelSizes.level);
		}
        yield return null;
        Init();
    }

    private void Init()
    {
        bool showFireWork = false;
        for (int i = 0; i < storyModeContainer.Count; i++)
		{
			List<Transform> childs = storyModeContainer[i].GetAllChildren();
            List<GameObject> reused = new List<GameObject>();
            foreach (var child in childs)
			{
                reused.Add(child.gameObject);
			}
			packLevelSizes[i].selectorPool.Clear();
			int offset = 0;
			foreach (var size in packLevelSizes[i].currentLevelSizes)
			{

                GameObject puzzleLevelSelector = null;
                if(reused.Count > 0)
                {
                    puzzleLevelSelector = reused[0];
                    if(puzzleLevelSelector.activeSelf == false)
                        puzzleLevelSelector.SetActive(true);
                    reused.Remove(puzzleLevelSelector);
                }

                if(puzzleLevelSelector == null)
                    puzzleLevelSelector = Instantiate(puzzleLevelTemplate, storyModeContainer[i]);

				LevelSelector levelSelector = puzzleLevelSelector.GetComponent<LevelSelector>();

                //* Setup connection line between level
                levelSelector.leftConnection.SetActive(true);
                levelSelector.rightConnection.SetActive(true);
                if (packLevelSizes[i].level == Level.Easy && size == Size.Six)
                    levelSelector.leftConnection.SetActive(false);
                if (packLevelSizes[i].level == Level.Insane && size == Size.Twelve)
                    levelSelector.rightConnection.SetActive(false);
                //**************************************

                Color solvedColor = PuzzleManager.Instance.accentColors[i];
                solvedColor.r = solvedColor.r * colorBias;
                solvedColor.g = solvedColor.g * colorBias;
                solvedColor.b = solvedColor.b * colorBias;
                levelSelector.SolvedColor = solvedColor;

                packLevelSizes[i].selectorPool.Add(levelSelector);
                int puzzleIndesOffset = StoryPuzzlesSaver.Instance.GetBoardIndex(StoryPuzzlesSaver.Instance.GetFinishedIndexSaveKey(packLevelSizes[i].pack.name, size));
                string puzzleId = PuzzleManager.Instance.GetPuzzleId(packLevelSizes[i].pack, size, puzzleIndesOffset);
				//levelSelector.SetLabel(string.Format("{0}",PuzzleManager.Instance.ageList[StoryPuzzlesSaver.GetIndexNode(packLevelSizes[i].level, size)]));
                int nodeIndex = StoryPuzzlesSaver.GetIndexNode(packLevelSizes[i].level, size);
                float progress = (float) StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex) / (float) StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex);
                levelSelector.SetNodeIndex(nodeIndex);
                levelSelector.SetProgress(progress ,StoryPuzzlesSaver.Instance.currentMileStone.ToString(), false, StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
                levelSelector.SetPuzzle(puzzleId);

                StoryPuzzlesSaver.SolvableStatus solvable = StoryPuzzlesSaver.Instance.ValidateLevel(packLevelSizes[i].level, size);
                StoryPuzzlesSaver.SolvableStatus lastProgress;
                levelSelectorProgress.TryGetValue(nodeIndex, out lastProgress);
                if (!levelSelectorProgress.ContainsKey(nodeIndex) ||( levelSelectorProgress.ContainsKey(nodeIndex) && solvable == lastProgress))
                {
                    levelSelector.initAnimation = false;
                }
                else
                {
                    levelSelector.initAnimation = true;
                    showFireWork = true;
                }
                if (levelSelectorProgress.ContainsKey(nodeIndex))
                {
                    levelSelectorProgress[nodeIndex] = solvable;
                }
                else
                {
                    levelSelectorProgress.Add(nodeIndex, solvable);
                }
                levelSelector.onClickOnUnPlayableLevel += OnClickonUnplayableLevel;
				offset++;
			}

            foreach (var item in reused)
            {
                item.SetActive(false);
            }
		}
        int nodeIndexB = StoryPuzzlesSaver.Instance.MaxNode;
        int preAgeB = nodeIndexB > 0 ? PuzzleManager.Instance.ageList[nodeIndexB - 1] : 0;
        int realAgeB = preAgeB + StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndexB);
        containerAge = realAgeB;
        if (showFireWork)
        {
            UIReferences.Instance.mainCameraController.PlayLeavesParticle(2);
        }
        if (GameManager.Instance.GameState == GameState.Prepare && !UIGuide.instance.IsGuildeShown(UIGuideFirstPuzzleSaveKey))
        {
            CoroutineHelper.Instance.PostponeActionUntil(() =>
            {
                if (packLevelSizes[0].selectorPool[0].gameObject)
                {
                    scroller.SnapIndex = 0;
                    scroller.Snap();
                    ExtrudedButton selectorBtn = packLevelSizes[0].selectorPool[0].GetComponent<ExtrudedButton>();
                    List<Image> maskedObject = selectorBtn.content.GetComponentsInChildren<Image>(true).ToList();
                    maskedObject.Add(characterImgs[scroller.SnapIndex]);
                    maskedObject.Add(selectorBtn.content.GetComponent<Image>());
                    maskedObject.Add(selectorBtn.targetGraphic.GetComponent<Image>());
                    UIGuide.UIGuideInformation uIGuideInformation = new UIGuide.UIGuideInformation(UIGuideFirstPuzzleSaveKey, maskedObject, characterImgs[scroller.SnapIndex].gameObject, packLevelSizes[0].selectorPool[0].gameObject, GameState.Prepare);
                    uIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUIDE_FIRST_PUZZLE;
                    uIGuideInformation.clickableButton = packLevelSizes[0].selectorPool[0].button;

                    Vector3[] worldConners = new Vector3[4];
                    characterImgs[scroller.SnapIndex].rectTransform.GetWorldCorners(worldConners);
                    uIGuideInformation.bubleTextWidth = 345;
                    uIGuideInformation.transformOffset = new Vector3(0, (worldConners[1].y - worldConners[0].y)* 0.8f, 0);

                    UIGuide.instance.HighLightThis(uIGuideInformation);
                    HasShownFirstInstruction = true;
                }
            }, () => GameManager.Instance.GameState.Equals(GameState.Prepare));
        }
    }

    public bool HasShownFirstInstruction = false;

    private void OnClickonUnplayableLevel(LevelSelector levelSelector)
    {
        InGameNotificationPopup.Instance.confirmationDialog.Show(I2.Loc.ScriptLocalization.ATTENTION, I2.Loc.ScriptLocalization.MILE_STONE_REQUIRED, I2.Loc.ScriptLocalization.OK, "",()=>{
			//Debug.Log("Ok Clicked");
		});
    }
}
