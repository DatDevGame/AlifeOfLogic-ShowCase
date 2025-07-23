using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu.Generator {
    [System.Serializable]
    public struct PuzzleSimplified
    {
        public string puzzle;
        public string solution;

        public PuzzleSimplified(string p, string s)
        {
            puzzle = p;
            solution = s;
        }

        public static explicit operator PuzzleSimplified(Puzzle p)
        {
            return new PuzzleSimplified(p.puzzle, p.solution);
        }
    }
}