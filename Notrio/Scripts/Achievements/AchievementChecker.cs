using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu.Achievements
{
    public abstract class AchievementChecker
    {
        public const string GET_PROGRESS_METHOD_NAME = "GetProgress";
        public const string IS_COMPLETED_METHOD_NAME = "IsCompleted";

        public abstract int GetProgress();
        public abstract bool IsCompleted(int requirement);
    }
}