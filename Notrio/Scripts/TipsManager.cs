using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;
using Takuzu.Generator;

public class TipsManager : MonoBehaviour {
    [SerializeField]
    private List<TipInformationScriptableObject> tipInformationScriptableObjects;
    public static TipsManager Instance;
    public Action UpdateTipsList = delegate { };
    public TipsPanel tipPanel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            DestroyImmediate(gameObject);
        PuzzleManager.onPuzzleSelected += OnPuzzleSelected;
        PlayerDb.Resetted += OnPlayerDBSyncEnd;
        CloudServiceManager.onPlayerDbSyncEnd += OnPlayerDBSyncEnd;
    }

    private void OnDestroy()
    {
        PuzzleManager.onPuzzleSelected -= OnPuzzleSelected;
        PlayerDb.Resetted -= OnPlayerDBSyncEnd;
        CloudServiceManager.onPlayerDbSyncEnd -= OnPlayerDBSyncEnd;
    }

    private void OnPlayerDBSyncEnd()
    {
        UpdateTipsList();
    }

    private void OnPuzzleSelected(string currentPuzzleId,string currentPuzzleStr,string currentSolutionStr,string currentProgressStr)
    {
        if (!tipPanel || PuzzleManager.Instance.IsChallenge(currentPuzzleId) || PuzzleManager.Instance.IsMultiMode(currentPuzzleId))
            return;
        Puzzle p = PuzzleManager.Instance.GetPuzzleById(currentPuzzleId);
        int nodeIndex = StoryPuzzlesSaver.GetIndexNode(p.level, p.size);
        int puzzleInNode = StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex);
        //TipInformationScriptableObject tip = tipInformationScriptableObjects.Find(item => (item.node == nodeIndex && item.puzzleInNode == puzzleInNode));

        for(int i = 0; i < tipInformationScriptableObjects.Count;i++)
        {
            if(tipInformationScriptableObjects[i].node == nodeIndex && tipInformationScriptableObjects[i].puzzleInNode == puzzleInNode)
            {
                TipInformationScriptableObject tip = tipInformationScriptableObjects[i];
                CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    if (!tip || PlayerDb.GetBool(tip.saveKey, false) || CheckOldTips(tip))
                        return;
                    if (tip.autoShow)
                        tipPanel.ShowThisTip(tipInformationScriptableObjects[i], i, currentPuzzleId, null);
                    else
                        MarkAsShownTip(tip);
                }, 0.7f);
                break;
            }
        }
    }

    public List<TipInformationScriptableObject> availabaleTips { get {
            List <TipInformationScriptableObject> r = tipInformationScriptableObjects.FindAll(item => (PlayerDb.GetBool(item.saveKey, false) || CheckOldTips(item)));
            // you want 100 to prioritize node value
            r.Sort((x, y) => (x.node * 100 + x.puzzleInNode).CompareTo(y.node * 100 + y.puzzleInNode));
            return r;} }

    private bool CheckOldTips(TipInformationScriptableObject item)
    {
        if (item.node < StoryPuzzlesSaver.Instance.MaxNode)
            return true;
        if (item.node == StoryPuzzlesSaver.Instance.MaxNode && item.node <= StoryPuzzlesSaver.Instance.GetMaxProgressInNode(StoryPuzzlesSaver.Instance.MaxNode))
            return true;
        return false;
;    }

    internal void RequestShowTip(string id, Action<string> callback)
    {
        //Show and wait to close
        if(!tipPanel || PuzzleManager.Instance.IsChallenge(id))
        {
            callback(id);
            return;
        }
        else
        {
            Puzzle p = PuzzleManager.Instance.GetPuzzleById(id);
            int nodeIndex = StoryPuzzlesSaver.GetIndexNode(p.level, p.size);
            int preAge = nodeIndex > 0 ? PuzzleManager.Instance.ageList[nodeIndex - 1] : 0;
            int realAge = preAge + StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex);
            //TipInformationScriptableObject tip = tipInformationScriptableObjects.Find(item => (item.node == realAge + 1));
            for (int index = 0; index < tipInformationScriptableObjects.Count; index++)
            {
                if (tipInformationScriptableObjects[index].node == (realAge + 1))
                {
                    TipInformationScriptableObject tip = tipInformationScriptableObjects[index];
                    Debug.Log("Show Tip" + (realAge + 1) + tip);
                    if (!tip || PlayerDb.GetBool(tip.saveKey, false))
                    {
                        callback(id);
                        return;
                    }
                    tipPanel.ShowThisTip(tip, index, id, callback);
                }
                break;
            }
        }
    }

    public void MarkAsShownTip(TipInformationScriptableObject tipInformationScriptableObject)
    {
        PlayerDb.SetBool(tipInformationScriptableObject.saveKey, true);
        PlayerDb.Save();
        UpdateTipsList();
    }
}
