using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace Takuzu.Achievements
{
    /// <summary>
    /// Store information of an achievement.
    /// </summary>
    [CreateAssetMenu(fileName = "New Achievement Info", menuName = "App specific/Achievement Info")]
    [Serializable]
    public class AchievementInfo : ScriptableObject
    {
        [SerializeField, Tooltip("Use this achievement in the game?")]
        private bool isUse;

        [SerializeField, Tooltip("Badge's display sprite.")]
        private Sprite badge;

        [SerializeField, Tooltip("Achievement's save id.")]
        private string id;

        [SerializeField]
        private string achievementName;

        [SerializeField]
        private string summary;

        [SerializeField]
        private int requirement;

        [HideInInspector]
        public string progressCheckerClass;

        public string ID { get { return id; } }
        public bool IsUse { get { return isUse; } }
        public Sprite Badge { get { return badge; } }

        public AchievementInfo()
        {
            isUse = true;
        }

        public int Progress
        {
            get
            {
                int p = 0;
                Type checkerType = Type.GetType(progressCheckerClass);
                if (checkerType != null)
                {
                    MethodInfo m = checkerType.GetMethod(AchievementChecker.GET_PROGRESS_METHOD_NAME);
                    if (m != null)
                    {
                        var checker = Activator.CreateInstance(checkerType);
                        p = (int)m.Invoke(checker, null);
                    }
                }

                return p;
            }
        }

        public bool IsCompleted
        {
            get
            {
                bool completed = false;
                Type checkerType = Type.GetType(progressCheckerClass);
                
                if (checkerType != null)
                {
                    MethodInfo m = checkerType.GetMethod(AchievementChecker.IS_COMPLETED_METHOD_NAME);
                    if (m != null)
                    {
                        var checker = Activator.CreateInstance(checkerType);
                        completed = (bool)m.Invoke(checker, new object[] { requirement });
                    }
                }

                return completed;
            }
        }
    }
}