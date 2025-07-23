using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    [CreateAssetMenu(fileName = "Exp profile", menuName = "App specific/Exp profile", order = 0)]
    public class ExpProfile : ScriptableObject
    {
        public static ExpProfile active;

        public int LevelCount
        {
            get
            {
                return exp != null ? exp.Count : 0;
            }
        }
        public List<int> exp;
        public List<string> rank;
        public List<Sprite> icon;
        public List<Color> accentColor;

        public int ToTotalExp(PlayerInfo info)
        {
            int sum = 0;
            for (int i = 0; i < info.level; ++i)
            {
                sum += exp[i];
            }
            sum += info.exp;
            return sum;
        }

        public PlayerInfo FromTotalExp(int totalExp)
        {
            int level = 0;
            int exp = 0;
            for (int i = 0; i < this.exp.Count; ++i)
            {
                if (totalExp < this.exp[i])
                    break;
                else
                {
                    level += 1;
                    totalExp -= this.exp[i];
                }
            }
            exp = totalExp;
            return new PlayerInfo(level, exp);
        }
    }
}