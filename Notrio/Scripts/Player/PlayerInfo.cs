using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    [System.Serializable]
    public struct PlayerInfo : System.IComparable<PlayerInfo>
    {
        public int level;
        public int exp;

        public float NormalizedExp
        {
            get
            {
                return exp * 1.0f / ExpProfile.active.exp[level];
            }
        }

        public PlayerInfo(int level, int exp)
        {
            this.level = level;
            this.exp = exp;
        }

        public int CompareTo(PlayerInfo i)
        {
            return ExpProfile.active.ToTotalExp(this).CompareTo(ExpProfile.active.ToTotalExp(i));
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }

        public PlayerInfo AddExp(int exp)
        {
            PlayerInfo i = this;
            int expSum = ExpProfile.active.ToTotalExp(i) + exp;
            return ExpProfile.active.FromTotalExp(expSum);
        }
    }
}