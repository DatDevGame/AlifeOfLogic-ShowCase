using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu.Generator;

namespace Takuzu
{
    [CreateAssetMenu(fileName = "New puzzle pack", menuName = "App specific/Puzzle Pack", order = 0)]
    public class PuzzlePack : ScriptableObject
    {
        [SerializeField]
        private string dbPath;
        public string DbPath
        {
            get
            {
#if UNITY_EDITOR
                return dbPath;
#elif UNITY_ANDROID || UNITY_IOS
                return System.IO.Path.GetFileName(dbPath);
#endif
            }
            set
            {
                //Debug.Log("Set puzzle pack " + name + " dbPath: " + value);
                //Debug.Log(StackTraceUtility.ExtractStackTrace());
                dbPath = value;
            }
        }
        public string packName;
        public int puzzleCount;
        public int puzzleCountOfSize6;
        public int puzzleCountOfSize8;
        public int puzzleCountOfSize10;
        public int puzzleCountOfSize12;
        public string description;
        public List<Level> difficulties;
        public int price;

        public int GetPuzzlePreCountNumber(Size s)
        {
            if (s == Size.Six)
                return puzzleCountOfSize6;
            else if (s == Size.Eight)
                return puzzleCountOfSize8;
            else if (s == Size.Ten)
                return puzzleCountOfSize10;
            else if (s == Size.Twelve)
                return puzzleCountOfSize12;
            else
                return 0;
        }

        public Size GetNextPuzzleSizeInPack(Size s)
        {
            if (s == Size.Six && puzzleCountOfSize8 > 0)
                return Size.Eight;
            else if (s == Size.Eight && puzzleCountOfSize10 > 0)
                return Size.Ten;
            else if (s == Size.Ten && puzzleCountOfSize12 > 0)
                return Size.Twelve;
            else
                return Size.Unknown;
        }


    }
}