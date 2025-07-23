using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinwheel;

namespace Takuzu
{
    public struct RawData
    {
        public const string DEFAULT_JSON = "{\"keys\":[], \"values\":[]}";
        public static RawData DefaultData
        {
            get
            {
                return new RawData
                {
                    keys = new string[0],
                    values = new string[0]
                };
            }
        }

        public string[] keys;
        public string[] values;

        public static Dictionary<string, string> ToDictionary(RawData raw)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            for (int i = 0; i < Mathf.Min(raw.keys.Length, raw.values.Length); ++i)
            {
                d.Add(
                    raw.keys[i] ?? string.Empty,
                    raw.values[i]
                    );
            }
            return d;
        }

        public static RawData FromDictionary(Dictionary<string, string> d)
        {
            string[] keyCollection = new string[d.Count];
            string[] valueCollection = new string[d.Count];
            d.Keys.CopyTo(keyCollection, 0);
            d.Values.CopyTo(valueCollection, 0);
            return new RawData()
            {
                keys = keyCollection,
                values = valueCollection
            };
        }

        public static RawData FromString(string rawString)
        {
            return JsonUtility.FromJson<RawData>(rawString);
        }

        public static string ToString(RawData raw)
        {
            return JsonUtility.ToJson(raw);
        }

        public override string ToString()
        {
            return ToString(this);
        }
    }
}
