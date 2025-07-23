using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;
using Takuzu.Generator;

namespace Takuzu.Achievements
{
    public class SolvePuzzleAchievementChecker : AchievementChecker
    {
        public override int GetProgress()
        {
            return PlayerDb.CountKeyStartWith(string.Format("{0}", PuzzleManager.SOLVED_PREFIX));
        }

        public override bool IsCompleted(int requirement)
        {
            return GetProgress() >= requirement;
        }
    }
}
