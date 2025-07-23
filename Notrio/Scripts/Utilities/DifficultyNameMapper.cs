using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu.Generator;

namespace Takuzu
{
    [CreateAssetMenu(fileName = "DifficultyNameMapper", menuName = "App specific/Difficulty Name Mapper")]
    public class DifficultyNameMapper : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private List<DifficultyNameMapperKeyValuePair> map;
        public List<DifficultyNameMapperKeyValuePair> Map
        {
            get
            {
                if (map == null)
                    map = new List<DifficultyNameMapperKeyValuePair>();
                return map;
            }
            set
            {
                map = value;
            }
        }

        public string ToDisplayName(Level l)
        {
            DifficultyNameMapperKeyValuePair pair = Map.Find(p => p.level.Equals(l));
            return I2.Loc.LocalizationManager.GetTranslation(pair.displayName ?? l.ToString());
        }

        public Level ToEnumValue(string displayName)
        {
            DifficultyNameMapperKeyValuePair pair = Map.Find(p => p.displayName.Equals(displayName));
            return pair.level;
        }
    }

    [System.Serializable]
    public struct DifficultyNameMapperKeyValuePair
    {
        public Level level;
        public string displayName;
    }
}