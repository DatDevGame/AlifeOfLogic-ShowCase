using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    [CreateAssetMenu(fileName = "RewardCoinProfile", menuName = "App specific/Reward Coin Profile", order = 0)]
    public class RewardCoinProfile : ScriptableObject
    {
        public static RewardCoinProfile active;

        public int rewardOnFbLogin;
        public int rewardOnWatchingAd;
        public int rewardOnFinishTutorialFirstTime;
        public int rewardOnShareCode;
        public int rewardOnEnterCode;
    }
}