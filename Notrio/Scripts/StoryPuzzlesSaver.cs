using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;
using Takuzu.Generator;
using System;

public class StoryPuzzlesSaver : MonoBehaviour {
    public static StoryPuzzlesSaver Instance;
    [HideInInspector]
    public string maxNodeKey = "MAX_PROGRESS_NODE";
    [HideInInspector]
    public string maxProgressInNodePrefix = "MAX_PROGRESS_NODE_NO_";
    public static Action<int, int> maxNodeChanged = delegate { };
    public static Action puzzleIndexChanged = delegate { };
    public static Action StoryModeCompleted = delegate { };
    public static int maxNodeCount = 19;

    public int MaxNode { set {
            value = Mathf.Max(value, MaxNode);
            if(value!= MaxNode){
                maxNodeChanged(value, MaxNode);
                PlayerDb.SetInt(maxNodeKey, value);
            }
        } get {
            return PlayerDb.GetInt(maxNodeKey, -1);
        }
    }

    public bool StoryModeIsCompleted
    {
        get
        {
            return (GetMaxProgressInNode(maxNodeCount) / ProgressRequiredToFinishNode(maxNodeCount) >= 1);
        }
    }


    public int currentMileStone { get {
            int preAge = MaxNode >= 0 ? PuzzleManager.Instance.ageList[MaxNode] : 0;
            int realAge = preAge;
            return realAge;
        } }

    public static int currentNodeIndex { get {
            if (Instance == null || PuzzleManager.Instance == null)
                return -2;
            int nodeIndex = PuzzleManager.currentIsChallenge ? Instance.MaxNode : GetIndexNode(PuzzleManager.currentLevel, PuzzleManager.currentSize);
            int preAge = nodeIndex > 0 ? PuzzleManager.Instance.ageList[nodeIndex - 1] : 0;
            int realAge = preAge;
            return realAge;
        } }

    public enum SolvableStatus
    {
        Solved,
        Current,
        Default,
        UnAssign,
        MaxNode
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
        if(MaxNode >= 16)
        {
            OndemandResourceLoader.LoadAssetsBundle("video",-1);
        }
    }

    public int GetCurrentLevel()
    {
        if (PuzzleManager.currentIsChallenge)
            return -2;
        bool currentIsAvailable = PuzzleManager.currentLevel != Level.UnGraded && PuzzleManager.currentSize != Size.Unknown;
        int nodeIndex = !(PuzzleManager.currentIsChallenge == false && currentIsAvailable) ? Instance.MaxNode : GetIndexNode(PuzzleManager.currentLevel, PuzzleManager.currentSize);
        int remainIndexNode = nodeIndex;
        int level = 0;
        for (int i = 4; i <= nodeIndex; i += 4)
        {
            remainIndexNode -= 4;
            level = level + ProgressRequiredToFinishNode(i - 1) * 4;
        }
        int currentProgress = GetMaxProgressInNode(nodeIndex);
        int requiredProgress = ProgressRequiredToFinishNode(nodeIndex);
        if (nodeIndex <= MaxNode)
        {
            level = level + 1 + (remainIndexNode * requiredProgress) + GetCurrentPuzzleIndesOffset(nodeIndex);
        }
        else
            level = level + 1 + (remainIndexNode * requiredProgress) + (currentProgress < requiredProgress ? currentProgress : (requiredProgress - 1));
            
        return level + 1;
    }

    public int GetCurrentPuzzleIndesOffset(int nodeIndex)
    {
        int requiredProgress = ProgressRequiredToFinishNode(nodeIndex);
        int puzzleIndesOffset = GetBoardIndex(GetFinishedIndexSaveKey(GetPuzzlePackFromNodeIndex(nodeIndex).name, GetSizeFromNodeIndex(nodeIndex)));
        puzzleIndesOffset = (puzzleIndesOffset >= requiredProgress - 1) ? requiredProgress - 1 : puzzleIndesOffset;
        return puzzleIndesOffset;
    }

    public int GetMaxLevel()
    {
        int nodeIndex = Mathf.Min(maxNodeCount, Instance.MaxNode + 1);
        int remainIndexNode = nodeIndex;
        int level = 0;
        for (int i = 4; i <= nodeIndex; i += 4)
        {
            remainIndexNode -= 4;
            level = level + ProgressRequiredToFinishNode(i - 1) * 4;
        }
        int currentProgress = GetMaxProgressInNode(nodeIndex);
        int requiredProgress = ProgressRequiredToFinishNode(nodeIndex);
        if (nodeIndex <= MaxNode)
            level = level + 1 + (remainIndexNode * requiredProgress) + (requiredProgress - 1);
        else
            level = level + 1 + (remainIndexNode * requiredProgress) + (currentProgress < requiredProgress ? currentProgress : (requiredProgress - 1));
        return level;
    }

    public string GetMaxProgressInNodeKey(int node)
    {
        return maxProgressInNodePrefix + node;
    }

    public int GetMaxProgressInNode(int node)
    {
        string key = GetMaxProgressInNodeKey(node);
        if (node<MaxNode)
        {
            int progressRequired = ProgressRequiredToFinishNode(node);
            PlayerDb.SetInt(key, progressRequired);
            return progressRequired;
        }
        return PlayerDb.GetInt(key, 0);
    }

    public void SaveMaxProgressInNode(int node, int progress)
    {
        string key = GetMaxProgressInNodeKey(node);
        if (progress >= GetMaxProgressInNode(node))
        {
            PlayerDb.SetInt(key, progress);
            if (StoryModeIsCompleted)
            {
                Debug.Log("StoryModeCompleted");
                StoryModeCompleted();
            }
        }
    }

    public void IncreaseMaxProgressInNode(Level currentLevel, Size currentSize)
    {
        if (IsCompleteCurrentNode(currentLevel, currentSize))
            return;
        int nodeIndex = GetIndexNode(currentLevel, currentSize);
        int currentProgress = GetMaxProgressInNode(nodeIndex);
        currentProgress++;
        SaveMaxProgressInNode(nodeIndex, currentProgress);
    }

	private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Judger.onJudgingCompleted += OnJudgingCompleted;
    }
    private void OnDestroy()
    {
        Judger.onJudgingCompleted -= OnJudgingCompleted;
    }

    private void OnJudgingCompleted(Judger.JudgingResult result)
    {
        if (!PuzzleManager.currentIsChallenge && !PuzzleManager.currentIsMultiMode)
        {
            IncreaseMaxProgressInNode(PuzzleManager.currentLevel, PuzzleManager.currentSize);
            if (IsCompleteCurrentNode(PuzzleManager.currentLevel, PuzzleManager.currentSize))
                SetMaxNode(PuzzleManager.currentLevel, PuzzleManager.currentSize);

            SaveMaxDifficultyAchieved(GetMaxDifficultLevel());

            UpdateStoryModePuzzleIndex(PuzzleManager.currentPack.name, PuzzleManager.currentSize);

            PlayerDb.SetString("AGE", ((MaxNode >= 0 ? PuzzleManager.Instance.ageList[MaxNode] : 0) + GetMaxProgressInNode(MaxNode + 1)).ToString());
            PlayerDb.SetInt("MAX_PROGRESS_NODE", MaxNode);
            PlayerDb.Save();
            CloudServiceManager.Instance.SubmitAge(PuzzleManager.Instance.ageList[MaxNode < 0 ? 0 : MaxNode].ToString());

            if (!CloudServiceManager.isGuest)
                CloudServiceManager.Instance.SyncPlayerDb();
            if((int) PuzzleManager.currentLevel == 5){
                OndemandResourceLoader.LoadAssetsBundle("video", -1);
            }
        }
    }

    public int ProgressRequiredToFinishNode(int nodeIndex)
    {
        switch(nodeIndex)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                return 3;
            case 4:
            case 5:
            case 6:
            case 7:
                return 4;
            case 8:
            case 9:
            case 10:
            case 11:
                return 5;
            case 12:
            case 13:
            case 14:
            case 15:
                return 6;
            case 16:
            case 17:
            case 18:
            case 19:
                return 7;
            default:
                return 5;
        }
    }

    private bool IsCompleteCurrentNode(Level currentLevel, Size currentSize)
    {
        int nodeIndex = GetIndexNode(currentLevel, currentSize);
        int currentProgress = GetMaxProgressInNode(nodeIndex);
        return currentProgress >= ProgressRequiredToFinishNode(nodeIndex);
    }

    private void UpdateStoryModePuzzleIndex(string name, Size currentSize)
    {
        string saveKey = GetFinishedIndexSaveKey(name, currentSize);
        int index = GetBoardIndex(saveKey);
        PuzzlePack puzzlePack = PuzzleManager.Instance.packs.Find(pack => pack.name == name);
        int count = PuzzleManager.CountPuzzleOfPack(puzzlePack, currentSize);
        index = (index + 1) % count;
        if(puzzlePack != null && puzzlePack.difficulties.Count > 0)
        {
            int nodeIndex = GetIndexNode(puzzlePack.difficulties[0], currentSize);
            int progressRequireOfNode = ProgressRequiredToFinishNode(nodeIndex);
            if (index >= progressRequireOfNode)
                index = 0;
        }
        SetBoardIndex(saveKey, index);
        puzzleIndexChanged();
    }

    private void SetMaxNode(Level currentLevel, Size currentSize)
    {
        int nodeIndex = GetIndexNode(currentLevel, currentSize);
        MaxNode = nodeIndex;
    }

    public static int GetIndexNode(Level level, Size size)
    {
        int nodeIndex = 0;
        foreach (var item in PuzzleManager.Instance.packSizesList)
        {
            if (item.packLevel < level)
            {
                nodeIndex += item.sizes.Count;
            }
        }
        foreach (var item in PuzzleManager.Instance.packSizesList.Find(item => item.packLevel == level).sizes)
        {
            if(item < size)
            {
                nodeIndex++;
            }
        }
        return nodeIndex;
    }


    private int GetDifficultLevelIndex(string packName)
    {
        int count = PuzzleManager.Instance.packs.Count;
        for (int i = 0; i < count; i++)
        {
            if (PuzzleManager.Instance.packs[i].name.Equals(packName))
                return i;
        }
        return -1;
    }

    public static Size GetSizeFromNodeIndex(int NodeIndexRef)
    {
        int nodeIndex = 0;
        NodeIndexRef = Mathf.Max(0, NodeIndexRef);
        Level maxLevel = Level.Easy;
        foreach (var item in PuzzleManager.Instance.packSizesList)
        {
            nodeIndex += item.sizes.Count;
            maxLevel = item.packLevel;
            if (nodeIndex > NodeIndexRef)
            {
                nodeIndex -= item.sizes.Count;
                break;
            }
        }
        Size s = PuzzleManager.Instance.packSizesList.Find(item => item.packLevel == maxLevel).sizes[NodeIndexRef - nodeIndex];
        return s;
    }

    public static PuzzlePack GetPuzzlePackFromNodeIndex(int nodeIndexRef)
    {
        int nodeIndex = 0;
        Level maxLevel = Level.Easy;
        foreach (var item in PuzzleManager.Instance.packSizesList)
        {
            nodeIndex += item.sizes.Count;
            maxLevel = item.packLevel;
            if (nodeIndex > nodeIndexRef)
            {
                nodeIndex -= item.sizes.Count;
                break;
            }
        }
        return PuzzleManager.Instance.packs.Find(pack => pack.difficulties.Contains(maxLevel));
    }

    public static Level GetDifficultLevelFromIndex(int nodeIndexRef)
    {
        int nodeIndex = 0;
        Level maxLevel = Level.Easy;
        foreach (var item in PuzzleManager.Instance.packSizesList)
        {
            nodeIndex += item.sizes.Count;
            maxLevel = item.packLevel;

            if (nodeIndex > nodeIndexRef + 1)
                break;

        }
        return maxLevel;
    }

    public static Level GetDifficultLevelFromYears(int years)
    {
        int index = -1;
        foreach (var age in PuzzleManager.Instance.ageList)
        {
            if (age <= years)
            {
                if (index == -1)
                    index = 0;
                index++;
            }
            else
            {
                break;
            }
        }
        return GetDifficultLevelFromIndex(index);
    }

    private int ConvertSizeToNumber(Size size)
    {
        switch (size)
        {
            case Size.Unknown:
                return -1;
            case Size.Six:
                return 0;
            case Size.Eight:
                return 1;
            case Size.Ten:
                return 2;
            case Size.Twelve:
                return 3;
            case Size.Fourteen:
                return 4;
            case Size.Sixteen:
                return 5;
            case Size.Eighteen:
                return 6;
            default:
                return -1;
        }
    }

    internal string GetNextPuzzle()
    {
        //bool finishCurrentNode = IsCompleteCurrentNode(PuzzleManager.currentLevel, PuzzleManager.currentSize);
        int nodeIndex = GetIndexNode(PuzzleManager.currentLevel, PuzzleManager.currentSize);
        //if (finishCurrentNode)
        //{
        //    nodeIndex++;
        //}
        int puzzleIndesOffset = GetBoardIndex(GetFinishedIndexSaveKey(GetPuzzlePackFromNodeIndex(nodeIndex).name, GetSizeFromNodeIndex(nodeIndex)));
        string puzzleId = PuzzleManager.Instance.GetPuzzleId(GetPuzzlePackFromNodeIndex(nodeIndex), GetSizeFromNodeIndex(nodeIndex), puzzleIndesOffset);
        return puzzleId;
    }

    private static string MAX_LEVEL_SAVE_KEY = "MAX_LEVEL_SAVE_KEY";
    public static Action<Level> NewMaxDifficultAchieved = delegate { };

    private void SaveMaxDifficultyAchieved(Level level)
    {
        if ((int)level > PlayerPrefs.GetInt(MAX_LEVEL_SAVE_KEY, 0))
        {
            NewMaxDifficultAchieved(level);
            PlayerPrefs.SetInt(MAX_LEVEL_SAVE_KEY, (int)level);
        }
    }

    public Level GetMaxDifficultLevel()
    {
        int nodeIndex = 0;
        Level maxLevel = Level.Easy;
        foreach (var item in PuzzleManager.Instance.packSizesList)
        {
            nodeIndex += item.sizes.Count;
            maxLevel = item.packLevel;

            if (nodeIndex > MaxNode + 1)
                break;
        }

        return maxLevel;
    }

    internal string GetFinishedIndexSaveKey(string packName, Size puzzleSize)
    {
        return string.Format("FINISHED_STORY_PUZZLES_SAVER_INDEX_KEY_{0}_{1}", packName, puzzleSize);
    }
 
    public string GetBoardState(string saveKey)
    {
        return PlayerPrefs.GetString(saveKey);
    }
    public void SetBoardIndex(string saveKey, int index)
    {
        PlayerDb.SetInt(saveKey, index);
        PlayerDb.Save();
    }
    public int GetBoardIndex(string saveKey)
    {
        return PlayerDb.GetInt(saveKey, 0);
    }

    internal SolvableStatus ValidateLevel(string puzzleId)
    {
        Puzzle p;
        if (PuzzleManager.Instance.IsChallenge(puzzleId))
        {
            p = PuzzleManager.Instance.GetChallengeById(puzzleId);
        }
        else
        {
            p = PuzzleManager.Instance.GetPuzzleById(puzzleId);
        }
        return ValidateLevel(p.level, p.size);
    }

    public SolvableStatus ValidateLevel(Level level,Size size)
    {
        int testNodeIndex = GetIndexNode(level, size);
        return ValidateLevel(testNodeIndex);
    }

    public SolvableStatus ValidateLevel(int nodeIndex)
    {
        if (nodeIndex < MaxNode)
        {
            return SolvableStatus.Solved;
        }
        else if (nodeIndex == MaxNode)
        {
            return SolvableStatus.MaxNode;
        }
        else if (nodeIndex == MaxNode + 1)
        {
            return SolvableStatus.Current;
        }
        else
        {
            return SolvableStatus.Default;
        }
    }
}
