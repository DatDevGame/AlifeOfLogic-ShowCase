/// <summary>
/// This class represents a puzzle.
/// </summary>
using System;

namespace Takuzu.Generator
{
    public class Puzzle
    {
        // List of available permutations.
        // 0 0 0
        // 0 0 1
        // 0 1 0
        // 1 0 0
        // 0 1 1
        // 1 0 1
        // 1 1 0
        // 1 1 1

        public static readonly string[] FAILED_PATTERNS = { "000", "111" };
        // IMPORTANT!!! The order of the strings in the following two arrays are vital
        // for the program to function properly. They are NOT to be changed.
        public static readonly string[] CONFIRMED_PATTERNS_1 = { ".11", "1.1", "11." };
        public static readonly string[] CONFIRMED_PATTERNS_2 = { ".00", "0.0", "00." };

        public const string VALUE_ZERO = "0";
        public const string VALUE_ONE = "1";
        public const string DEFAULT_VALUE = "01";
        public const string DOT = ".";
        public const string X = "x";

        public readonly Size size;
        public readonly Level level;
        public readonly string puzzle;
        public readonly string solution;
        public readonly int givenNum;
        public readonly int parseNum;
        public readonly float parsePercent;
        public readonly int lsdNum;
        public readonly float lsdPercent;
        public readonly int alsdNum;
        public readonly float alsdPercent;

        // Constructor
        public Puzzle(Size sz, Level l, string p, string s, int gn, int pn, int lsdn, int alsdn)
        {
            size = sz;
            level = l;
            puzzle = p;
            solution = s;
            givenNum = gn;
            parseNum = pn;
            lsdNum = lsdn;
            alsdNum = alsdn;

            // Calculate percentages
            int sizeVal = (int)size;
            parsePercent = (float)parseNum * 100 / (sizeVal * sizeVal);
            lsdPercent = (float)lsdNum * 100 / (sizeVal * sizeVal);
            alsdPercent = (float)alsdNum * 100 / (sizeVal * sizeVal);
        }
    }
}