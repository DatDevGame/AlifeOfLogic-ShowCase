using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu.Generator;

[CreateAssetMenu(fileName = "New Exp judging profile", menuName = "App specific/Exp Judging Profile", order = 0)]
public class ExpJudgingProfile : ScriptableObject
{
    public Size size;
    public Level level;
    public int minExp;
    public AnimationCurve baseExpByTime;

    [Space]
    [Range(0, 100)]
    public float flagSubtractPercent;
    [Range(0, 100)]
    public float revealSubtractPercent;
    [Range(0, 100)]
    public float resetSubtractPercent;
    [Range(0, 100)]
    public float errorSubtractPercent;
    [Range(0, 100)]
    public float undoSubtractPercent;

    [Space]
    [Range(0, 100)]
    public float noPowerupBonusPercent;
    [Range(0, 100)]
    public float noErrorBonusPercent;
}
