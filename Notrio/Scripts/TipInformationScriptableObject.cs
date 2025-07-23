using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "TipInformationScriptableObject", menuName = "ScriptableObj/TipInformationScriptableObject", order = 1)]
public class TipInformationScriptableObject : ScriptableObject {
    [Serializable]
    public struct AnimationAction
    {
        public float deltaTime;
        public ActionType action;
        public string additionalInfor;
    }
    [Serializable]
    public enum ActionType
    {
        HighLight,
        SetOne,
        SetZero,
        Unset,
        Wait,
        SetAtive,
        ClearInactive
    }
    public string tipTitle = "Tip";
    [TextArea]
    public string tipText;
    public List<string> listCellDisplay;
    public int node;
    public int puzzleInNode;
    public bool autoShow;

    public string InitialPuzzle;
    public string puzzleSolution;
    public List<string> paternList = new List<string>();
    public List<AnimationAction> animationTimeline;
    private string keyPrefix = "UNLOCK_TIP_INFORS_";
    private static string localizationTipsDescriptionKeyPrefix = "TIP_DESCRIPTION_";

    public string saveKey { set { keyPrefix = value; } get { return String.Format(keyPrefix + "{0}-{1}", node, puzzleInNode); } }

    [HideInInspector]
    public string TipText
    {
        get
        {
            string localizationString = I2.Loc.LocalizationManager.GetTranslation(string.Format("{0}{1}", localizationTipsDescriptionKeyPrefix, tipTitle.Replace(' ', '_')));
            if (listCellDisplay.Count > 0)
                localizationString = String.Format(localizationString, listCellDisplay.ToArray());

            if (!String.IsNullOrEmpty(localizationString))
            {
                return localizationString;
            }
            else
            {
                Debug.Log("Translate is null");
                return tipText;
            }
        }
    }
}
