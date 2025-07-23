using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu.Generator;

[CreateAssetMenu(fileName = "New Coin judging profile", menuName = "App specific/Coin Judging Profile", order = 0)]
public class CoinJudgingProfile : ScriptableObject
{
    public Size size;
    public Level level;
    public int minBaseCoin;
    public int maxBaseCoin;

    [Range(0, 100)]
    public float noPowerupBonusPercent;
    [Range(0, 100)]
    public float noErrorBonusPercent;

    [Space]
    public int dailyChallengeFixedReward;
    public int weeklyChallengeFixedReward;

    [Space]
    public bool noJudgingOnChallenge;
}
