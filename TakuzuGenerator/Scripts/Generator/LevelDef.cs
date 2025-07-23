/// <summary>
/// This class represents the definition of a level of a particular puzzle size with all the properties defining difficulty.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Takuzu.Generator
{
    [System.Serializable]
    public sealed class LevelDef
    {
        private static Dictionary<string, LevelDef> map = new Dictionary<string, LevelDef>();

        public readonly Size size;
        public readonly Level level;
        public float minParsePercent;
        public float maxParsePercent;
        public float minLsdPercent;
        public float maxLsdPercent;
        public float minAlsdPercent;
        public float maxAlsdPercent;
        public readonly string code;

        private LevelDef(Size sz, Level l, float minPP, float maxPP, float minLSDP, float maxLSDP, float minALSDP, float maxALSDP, bool addToSelfDict = true)
        {
            size = sz;
            level = l;
            minParsePercent = minPP;
            maxParsePercent = maxPP;
            minLsdPercent = minLSDP;
            maxLsdPercent = maxLSDP;
            minAlsdPercent = minALSDP;
            maxAlsdPercent = maxALSDP;

            if (addToSelfDict)
            {
                code = GetLevelCode(size, level);
                map.Add(code, this);
            }
        }

        /// <summary>
        /// Forms the LevelDef code.
        /// </summary>
        /// <returns>The level code.</returns>
        /// <param name="sz">Size.</param>
        /// <param name="l">L.</param>
        public static string GetLevelCode(Size sz, Level l)
        {
            return ((int)sz).ToString() + "_" + ((int)l).ToString();
        }

        public static string GetLevelName(Size sz, Level l)
        {
            return sz.ToString() + " - " + l.ToString();
        }

        public LevelDef Clone()
        {
            return new LevelDef(this.size, this.level, this.minParsePercent, this.maxParsePercent, this.minLsdPercent, this.maxLsdPercent, this.minAlsdPercent, this.maxAlsdPercent, false);
        }

        ///// <summary>
        ///// Gets the LevelDef from size and abstract level.
        ///// </summary>
        ///// <returns>The level def.</returns>
        ///// <param name="sz">Size.</param>
        ///// <param name="l">L.</param>
        //public static LevelDef GetLevelDef(Size sz, Level l)
        //{
        //    string levelCode = GetLevelCode(sz, l);

        //    if (map[levelCode] != null)
        //        return map[levelCode] as LevelDef;
        //    else
        //        return LevelDef.UnGraded;
        //}

        /// <summary>
        /// Grade the difficulty of the given puzzle according to its
        /// solving statistics and the grading criteria defined in each LevelDef.
        /// </summary>
        /// <returns>Resulted level.</returns>
        /// <param name="size">Size.</param>
        /// <param name="parsePercent">Parse percent.</param>
        /// <param name="lsdPercent">Lsd percent.</param>
        /// <param name="alsdPercent">Alsd percent.</param>
        public static LevelDef GradePuzzle(Size eSize, Solver.SolvingStatistics stats)
        {
            LevelDef result;
            Dictionary<string, LevelDef> gradingMap;
            if (GradingProfile.active != null)
            {
                result = GradingProfile.active.UnGraded;
                gradingMap = GradingProfile.active.map;
            }
            else
            {
                result = LevelDef.UnGraded;
                gradingMap = LevelDef.map;
            }

            // Calculate necessary statistics for grading.
            int size = (int)eSize;
            float parsePercent = (float)stats.numOfSolvedCellsAfterParsing * 100 / (size * size);
            float lsdPercent = (float)stats.numOfSolvedCellsAfterLsd * 100 / (size * size);
            float alsdPercent = (float)stats.numOfSolvedCellsAfterAlsd * 100 / (size * size);

            // Loop thru all the defined levels and check those corresponding to the puzzle size.
            foreach (var entry in gradingMap)
            {
                LevelDef ld = (LevelDef)entry.Value;

                if (eSize == ld.size)
                {
                    bool ppCond = parsePercent >= ld.minParsePercent && parsePercent <= ld.maxParsePercent;
                    bool lsdpCond = lsdPercent >= ld.minLsdPercent && lsdPercent <= ld.maxLsdPercent;
                    bool alsdCond = alsdPercent >= ld.minAlsdPercent && alsdPercent <= ld.maxAlsdPercent;

                    if (ppCond && lsdpCond && alsdCond)
                    {
                        result = ld;
                        break;
                    }
                }
            }

            return result;
        }

        // PuzzleLevel difficulty definition on a per size basis, including a common ungraded level.
        public static readonly LevelDef UnGraded = new LevelDef(Size.Unknown, Level.UnGraded, -1, -1, -1, -1, -1, -1);

        public static readonly LevelDef SixEasy = new LevelDef(Size.Six, Level.Easy, 100, 100, 100, 100, 100, 100);
        public static readonly LevelDef SixMedium = new LevelDef(Size.Six, Level.Medium, 55, 85, 100, 100, 100, 100);
        public static readonly LevelDef SixHard = new LevelDef(Size.Six, Level.Hard, 0, 85, 0, 85, 65, 85);
        public static readonly LevelDef SixEvil = new LevelDef(Size.Six, Level.Evil, 0, 60, 0, 60, 45, 60);
        public static readonly LevelDef SixInsane = new LevelDef(Size.Six, Level.Insane, 0, 40, 0, 40, 0, 40);

        public static readonly LevelDef EightEasy = new LevelDef(Size.Eight, Level.Easy, 100, 100, 100, 100, 100, 100);
        public static readonly LevelDef EightMedium = new LevelDef(Size.Eight, Level.Medium, 70, 86, 100, 100, 100, 100);
        public static readonly LevelDef EightHard = new LevelDef(Size.Eight, Level.Hard, 0, 88, 0, 88, 76, 88);
        public static readonly LevelDef EightEvil = new LevelDef(Size.Eight, Level.Evil, 0, 65, 0, 65, 45, 65);
        public static readonly LevelDef EightInsane = new LevelDef(Size.Eight, Level.Insane, 0, 40, 0, 40, 0, 40);

        public static readonly LevelDef TenEasy = new LevelDef(Size.Ten, Level.Easy, 100, 100, 100, 100, 100, 100);
        public static readonly LevelDef TenMedium = new LevelDef(Size.Ten, Level.Medium, 65, 87, 100, 100, 100, 100);
        public static readonly LevelDef TenHard = new LevelDef(Size.Ten, Level.Hard, 0, 92, 0, 92, 81, 92);
        public static readonly LevelDef TenEvil = new LevelDef(Size.Ten, Level.Evil, 0, 75, 0, 75, 55, 75);
        public static readonly LevelDef TenInsane = new LevelDef(Size.Ten, Level.Insane, 0, 50, 0, 50, 0, 50);

        public static readonly LevelDef TwelveEasy = new LevelDef(Size.Twelve, Level.Easy, 100, 100, 100, 100, 100, 100);
        public static readonly LevelDef TwelveMedium = new LevelDef(Size.Twelve, Level.Medium, 70, 85, 100, 100, 100, 100);
        public static readonly LevelDef TwelveHard = new LevelDef(Size.Twelve, Level.Hard, 0, 95, 0, 95, 86, 95);
        public static readonly LevelDef TwelveEvil = new LevelDef(Size.Twelve, Level.Evil, 0, 80, 0, 80, 60, 80);
        public static readonly LevelDef TwelveInsane = new LevelDef(Size.Twelve, Level.Insane, 0, 50, 0, 50, 0, 50);

        public static readonly LevelDef FourteenEasy = new LevelDef(Size.Fourteen, Level.Easy, 100, 100, 100, 100, 100, 100);
        public static readonly LevelDef FourteenMedium = new LevelDef(Size.Fourteen, Level.Medium, 70, 85, 100, 100, 100, 100);
        public static readonly LevelDef FourteenHard = new LevelDef(Size.Fourteen, Level.Hard, 0, 95, 0, 95, 86, 95);
        public static readonly LevelDef FourteenEvil = new LevelDef(Size.Fourteen, Level.Evil, 0, 80, 0, 80, 60, 80);
        public static readonly LevelDef FourteenInsane = new LevelDef(Size.Fourteen, Level.Insane, 0, 50, 0, 50, 0, 50);
    }
}

