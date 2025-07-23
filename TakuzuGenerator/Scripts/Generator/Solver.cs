/// <summary>
/// Takuzu solver.
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Takuzu.Generator
{
    public static class Solver
    {
        public struct SolvingStatistics
        {
            public int numOfSolvedCellsAfterParsing;
            public int numOfSolvedCellsByLsd;
            public int numOfSolvedCellsAfterLsd;
            public int numOfSolvedCellsByAlsd;
            public int numOfSolvedCellsAfterAlsd;
            public long totalSearchTrials;
            public long failedSearchTrials;
        }

        /// <summary>
        /// Checks if the input puzzle has only one unique solution.
        /// </summary>
        /// <returns>true if has one unique solution, false otherwise.</returns>
        /// <param name="puzzle">the puzzle to examine.</param>
        /// <param name="stats">Solving statistics.</param>
        public static bool HasUniqueSolution(string[][] puzzle, out SolvingStatistics stats)
        {
            List<string[][]> solutions = SolvePuzzle(puzzle, 2, true, true, out stats, true); // look for maximum 2 solutions, LSD on, silent set to true.
            return (solutions.Count == 1);
        }

        /// <param name="silent">If set to <c>true</c> silent.</param>
        /// <summary>
        /// Solves the input puzzle with the option of using LSD techniques.
        /// </summary>
        /// <returns>list of all solutions found.</returns>
        /// <param name="puzzle">input puzzle string.</param>
        /// <param name="numOfSolutions">number of solutions to find, -1 means find all possible solutions.</param>
        /// <param name="useLSD">If set to <c>true</c> use LSD technique when solving.</param>
        /// <param name="useALSD">If set to <c>true</c> use ALSD technique when solving.</param>
        /// <param name="stats">Solving statistics.</param>
        /// <param name="silent">If set to <c>true</c> silent.</param>
        public static List<string[][]> SolvePuzzle(string[][] puzzle, int numOfSolutions, bool useLSD, bool useALSD, out SolvingStatistics stats, bool silent)
        {
            // Use a stopwatch to measure executing time.
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Init solving stats with default values.
            stats = default(SolvingStatistics);

            // Prepare list of returned solutions
            List<string[][]> solutions = new List<string[][]>();

            // Prepare a new grid with the same sizes as the input puzzle to form on solutions on
            int size = puzzle.Length;
            string[][] result = new string[size][];
            for (int i = 0; i < size; i++)
            {
                result[i] = new string[size];
            }

            if (!silent)
                Helper.LogFormat("Solving puzzle...");

            bool success = ParsePuzzle(puzzle, result, ref stats.numOfSolvedCellsAfterParsing, silent);

            if (!success)
            {
                if (!silent)
                    Helper.LogFormat("Puzzle solving failed: could not parse puzzle.");
                return solutions;
            }

            if (useLSD)
            {
                bool lsdSuccess = ApplyLsd(result, false, ref stats.numOfSolvedCellsByLsd, silent);

                if (!lsdSuccess)
                {
                    if (!silent)
                        Helper.LogFormat("Puzzle solving failed: error when applying LSD.");
                    return solutions;   // puzzle is not solvable
                }
            }

            if (useALSD)
            {
                bool alsdSuccess = ApplyLsd(result, true, ref stats.numOfSolvedCellsByAlsd, silent);
                if (!alsdSuccess)
                {
                    if (!silent)
                        Helper.LogFormat("Puzzle solving failed: error when applying ALSD.");
                    return solutions;   // puzzle is not solvable
                }
            }

            //Calculate stats
            stats.numOfSolvedCellsAfterLsd = stats.numOfSolvedCellsAfterParsing + stats.numOfSolvedCellsByLsd;
            stats.numOfSolvedCellsAfterAlsd = stats.numOfSolvedCellsAfterLsd + stats.numOfSolvedCellsByAlsd;

            // Finally apply search algorithm.
            Search(result, solutions, numOfSolutions, ref stats.totalSearchTrials, ref stats.failedSearchTrials, silent);

            // Stop the stopwatch and get executing time.
            stopwatch.Stop();

            if (!silent)
            {
                Helper.LogFormat("===========================================================");
                Helper.LogFormat("Finished solving puzzle. Results:");
                Helper.LogFormat("Puzzle size:\t\t\t{0:D} x {0:D}", size);
                Helper.LogFormat("Solutions required:\t\t{0:D}", numOfSolutions);
                Helper.LogFormat("Solutions found:\t\t{0:D}", solutions.Count);
                Helper.LogFormat("numOfSolvedCellsAfterParsing:\t\t{0:D} ({1:P})", stats.numOfSolvedCellsAfterParsing, (float)stats.numOfSolvedCellsAfterParsing / (size * size));
                Helper.LogFormat("Used LSD:\t\t\t{0}", useLSD ? "Yes" : "No");
                Helper.LogFormat("numOfSolvedCellsAfterLsd:\t{0:D} ({1:P})", stats.numOfSolvedCellsAfterLsd, (float)stats.numOfSolvedCellsAfterLsd / (size * size));
                Helper.LogFormat("Used ALSD:\t\t\t{0}", useALSD ? "Yes" : "No");
                Helper.LogFormat("numOfSolvedCellsAfterAlsd:\t{0:D} ({1:P})", stats.numOfSolvedCellsAfterAlsd, (float)stats.numOfSolvedCellsAfterAlsd / (size * size));
                Helper.LogFormat("Total search trials:\t\t\t{0:D}", stats.totalSearchTrials);
                Helper.LogFormat("Failed search trials:\t\t\t{0:D} ({1:P})", stats.failedSearchTrials, (float)stats.failedSearchTrials / stats.totalSearchTrials);
                Helper.LogFormat("Time:\t\t\t\t{0}", Helper.FormatTime(stopwatch.Elapsed));
                Helper.LogFormat("===========================================================");
            }

            return solutions;
        }

        /// <summary>
        /// Parse the input puzzle and construct a corresponding 2D grid, each element
        /// in this grid is a string of default values for that element. Then applies the given 
        /// values onto the grid while performing constraint propagation.
        /// Result is a grid of parsed puzzle where each element is a string
        /// represents all possible values remained for that particular element.
        /// </summary>
        /// <returns>true if no contradiction found, false otherwise.</returns>
        /// <param name="puzzle">Puzzle.</param>
        /// <param name="result">Result.</param>
        /// <param name="numOfParsedCells">Number of cells solved after parsing.</param>
        /// <param name="silent">Silent.</param>
        private static bool ParsePuzzle(string[][] puzzle, string[][] result, ref int numOfSolvedCellsAfterParsing, bool silent)
        {
            bool success = true;

            // Assign each element of the result array with a string
            // representing all possible values for that element.
            int size = puzzle.Length;

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    result[row][col] = Puzzle.DEFAULT_VALUE;
                }
            }

            // Now apply all the given values and perform constraint propagation
            // one the newly created array.
            int numOfGivens = 0;

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    string val = puzzle[row][col];
                    if ((val.Equals(Puzzle.VALUE_ZERO)) || (val.Equals(Puzzle.VALUE_ONE)))
                    {
                        numOfGivens++;
                        success = AssignCellValue(result, row, col, val);
                        if (!success)
                        {
                            // Parsing failed.
                            if (!silent)
                            {
                                Helper.LogFormat("Invalid puzzle: failed when applying givens...");
                            }
                            return success;
                        }
                    }
                }
            }

            // If we reach here, parsing has succeeded.
            numOfSolvedCellsAfterParsing = Validator.CountKnownCells(result, Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE);

            // If we reach here, the puzzle was parsed succesfully.
            // Print out information for debug purpose.
            if (!silent)
            {
                Helper.LogFormat("===========================================================");
                Helper.LogFormat("Input puzzle size: ({0:D} x {0:D})", size);
                Helper.LogFormat("Number of givens: {0:D} ({1:P})", numOfGivens, (float)numOfGivens / (size * size));
                Helper.PrintGrid(puzzle);
                Helper.LogFormat("===========================================================");
                Helper.LogFormat("numOfParsedCells: {0:D} ({1:P})", numOfSolvedCellsAfterParsing, (float)numOfSolvedCellsAfterParsing / (size * size));
                Helper.LogFormat("Parsed puzzle:");
                Helper.PrintGrid(result);
                Helper.LogFormat("===========================================================");
            }

            return success;
        }

        /// <summary>
        /// Perform LSD or ALSD technique on the given grid. More info about this technique see the summary of DoApplyLsdToGrid method.
        /// </summary>
        /// <returns><c>true</c>, if if no error occurred, regardless there's any cell solved by the applied technique,
        ///         <c>false</c> if there's error while apply the technique, normally this means the given puzzle has problem and is not solvable.</returns>
        /// <param name="values">the puzzle grid to apply this technique.</param>
        /// <param name="useAlsd">If set to <c>true</c> use ALSD technique.</param>
        /// <param name="numOfSolvedCells">Number of cells solved by applying this technique.</param>
        /// <param name="silent">If set to <c>true</c> silent.</param>
        private static bool ApplyLsd(string[][] values, bool useAdvancedTechnique, ref int numOfSolvedCells, bool silent)
        {
            if (!silent)
                Helper.LogFormat("Now applying {0} technique...", useAdvancedTechnique ? "ALSD" : "LSD");

            int numOfSolvedCellsBefore = Validator.CountKnownCells(values, Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE);
            int size = values.Length;
            int result = DoApplyLsdToGrid(values, useAdvancedTechnique, silent); // DoApplyLsdToGrid will call itself recursively
            bool success = (result < 0) ? false : true;

            if (!success)
            {
                if (!silent)
                    Helper.LogFormat("Invalid puzzle: failed applying {0} technique.", useAdvancedTechnique ? "ALSD" : "LSD");
            }
            else
            {
                // Count the number of cells solved by this technique.
                int numOfSolvedCellsAfter = Validator.CountKnownCells(values, Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE);
                numOfSolvedCells = numOfSolvedCellsAfter - numOfSolvedCellsBefore;

                if (!silent)
                {
                    Helper.LogFormat("===========================================================");
                    Helper.LogFormat("Finished applying {0} technique on puzzle. Result: {1}", useAdvancedTechnique ? "ALSD" : "LSD", success ? "Success" : "Fail");
                    Helper.LogFormat("Grid after applying this technique:");
                    Helper.PrintGrid(values);
                    Helper.LogFormat("===========================================================");
                    Helper.LogFormat("Number of cells solved by this technique:\t{0:D}", numOfSolvedCells);
                    Helper.LogFormat("Total cells solved after applying this technique:\t{0:D} ({1:P})", numOfSolvedCellsAfter, (float)numOfSolvedCells / (size * size));
                    Helper.LogFormat("===========================================================");
                }
            }

            return success;
        }

        /// <summary>
        /// Applies a solving technique called "LSD" (Last Square Deduction): if a line
        /// has only one available slot for one value (X), and more than 2 slots for the
        /// other value (otherwise the technique won't apply), and if applying the value X
        /// for a certain square among the remaining empty ones leads to a contradiction
        /// WITHIN that line, we can be sure that this particular square must have the
        /// opposite value.
        /// The fact that we only check for contradiction within the line makes sure that
        /// this technique can be easily applied by human players (rather than machines),
        /// in fact this is a common technique to solve MEDIUM puzzles. Therefore, this is
        /// a useful measure to grade the difficulty level of the puzzle being solved.
        /// ALSD (Advanced Last Square Deduction): a LSD technique that preceeded by a
        /// check using a technique call "Different Two".
        /// "Different Two": consider a group of four cells: [1 2 3 4], if 1 & 4 has
        /// different colors, then 2 & 3 must have different color. In other words,
        /// we can be sure that 1 cell of VALUE_ZERO and 1 cell of VALUE_ONE are surely
        /// assigned to cells 2 & 3, regardless of their actual order.
        /// The combination with "Different Two" helps ALSD to apply LSD on more subtle
        /// line configuration, hence a more advanced technique.
        /// </summary>
        /// <returns> 0: the process terminated without error, regardless there's any cell solved by LSD
        ///          -1: the process terminated with error while assigning an LSD-dictated value to a certain cell, normally this means the given puzzle has problem and is not solvable
        ///           1: the process is repeating itself </returns>
        /// <param name="values">the puzzle grid to solve.</param>
        /// <param name="useAlsd">If set to <c>true</c> use Advanced LSD (ALSD) technique.</param>
        /// <param name="silent">If set to <c>true</c> silent.</param>
        private static int DoApplyLsdToGrid(string[][] values, bool useAdvancedTechnique, bool silent)
        {
            int resultCode = 0;

            // Loop thru all cells on the diagonal of the grid, examine the row and column
            // crossing at each cell to find the line has only one slot left for either value
            // and perform the LSD technique on that line.
            // Get row and column arrays of the cell.
            int size = values.Length;
            string[] testValues = { Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE };

            for (int i = 0; i < size; i++)
            {
                // Get the row i and column i and count the occurences of
                // each value within that line.
                // First examine the row number i.
                string[] rowArray = Helper.GetRow(values, i);
                int targetC = -1;
                string failedRowValue = null;

                for (int j = 0; j < testValues.Length; j++)
                {
                    string testValue = testValues[j];

                    if (!useAdvancedTechnique)
                    {
                        targetC = ApplyLsdToOneLine(rowArray, testValue);
                    }
                    else
                    {
                        targetC = ApplyAlsdToOneLine(rowArray, testValue);
                    }

                    if (targetC >= 0)
                    {
                        failedRowValue = testValue;
                        break;
                    }
                }

                if (targetC >= 0)
                {
                    string targetRowValue = (failedRowValue.Equals(Puzzle.VALUE_ZERO)) ? Puzzle.VALUE_ONE : Puzzle.VALUE_ZERO;
                    if (!silent)
                    {
                        Helper.LogFormat("Checking row " + (i + 1));
                        Helper.LogFormat("Assigning cell {0:D},{1:D} with value {2}...", i + 1, targetC + 1, targetRowValue);
                    }
                    bool success = AssignCellValue(values, i, targetC, targetRowValue);

                    if (success)
                    {
                        resultCode = 1;
                    }
                    else
                    {
                        resultCode = -1;
                        break;
                    }
                }

                // Now examine the column number i.
                // Note that the values grid may be updated now due to the row examination above.
                string[] colArray = Helper.GetColumn(values, i);
                int targetR = -1;
                string failedColValue = null;

                for (int k = 0; k < testValues.Length; k++)
                {
                    string testValue = testValues[k];

                    if (!useAdvancedTechnique)
                    {
                        targetR = ApplyLsdToOneLine(colArray, testValue);
                    }
                    else
                    {
                        targetR = ApplyAlsdToOneLine(colArray, testValue);
                    }

                    if (targetR >= 0)
                    {
                        failedColValue = testValue;
                        break;
                    }
                }

                if (targetR >= 0)
                {
                    string targetColValue = (failedColValue.Equals(Puzzle.VALUE_ZERO)) ? Puzzle.VALUE_ONE : Puzzle.VALUE_ZERO;
                    if (!silent)
                    {
                        Helper.LogFormat("Checking column " + (i + 1));
                        Helper.LogFormat("Assigning cell {0:D},{1:D} with value {2}...", targetR + 1, i + 1, targetColValue);
                    }
                    bool success = AssignCellValue(values, targetR, i, targetColValue);

                    if (success)
                    {
                        resultCode = 1;
                    }
                    else
                    {
                        resultCode = -1;
                        break;
                    }
                }
            }

            // Repeat the process if a there's at least one cell has been assigned with a new value.
            if (resultCode > 0)
            {
                resultCode = DoApplyLsdToGrid(values, useAdvancedTechnique, silent);
            }

            return resultCode;
        }

        /// <summary>
        /// Apply LSD technique on the given row or column (in form of an array of values).
        /// </summary>
        /// <returns>the index of the cell within the line that causes contradiction when assigned the test value, -1 if the technique doesn't apply.</returns>
        /// <param name="line">the array of row/column values.</param>
        /// <param name="testValue">the value to be tested.</param>
        private static int ApplyLsdToOneLine(string[] line, string testValue)
        {
            if (!(testValue.Equals(Puzzle.VALUE_ZERO) || testValue.Equals(Puzzle.VALUE_ONE)))
            {
                throw new ArgumentException("Invalid testValue.");
            }

            int result = -1;
            string otherValue = (testValue.Equals(Puzzle.VALUE_ZERO)) ? Puzzle.VALUE_ONE : Puzzle.VALUE_ZERO;
            int occurencesOfTestValue = Validator.CountOccurrences(line, testValue);
            int occurencesOfOtherValue = Validator.CountOccurrences(line, otherValue);
            int size = line.Length;

            if (((size / 2 - occurencesOfTestValue) == 1) && ((size / 2 - occurencesOfOtherValue) >= 2))
            {
                // If the line satisfies the requirements of the technique, keep going: try assign one of
                // the empty cell with the testValue, and all remaining cells with the otherValue to
                // see if contradiction occurs in the line.
                // If yes, assign that cell with the otherValue.
                // If no, repeat the assigning trial with the next cell.                
                for (int c = 0; c < line.Length; c++)
                {
                    if (line[c].Equals(Puzzle.DEFAULT_VALUE))
                    {
                        // Create a copy of the being examined line and try assigning values on it.
                        // Note that a shallow copy is enough, as we'll only alter the elements of this
                        // copy with new strings and test it, there's no point creating new string objects 
                        string[] lineCopy = (string[])line.Clone();

                        lineCopy[c] = testValue;
                        for (int c2 = 0; c2 < lineCopy.Length; c2++)
                        {
                            if (lineCopy[c2].Equals(Puzzle.DEFAULT_VALUE) && (c2 != c))
                            {
                                lineCopy[c2] = otherValue;
                            }
                        }

                        // Now examine the filled line to see if contradiction occurred.
                        // If there's contradiction, LSD has succeeded and the value of the
                        // cell at position c is determined to be otherValue.
                        bool valid = Validator.ValidateLine(lineCopy, Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE);   // Actually just using Validator.TripletRuleCheck will suffice.
                        if (!valid)
                        {
                            result = c;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Applies ALSD (Advanced Last Square Deduction) technique on the given row or column.
        /// </summary>
        /// <returns>the index of the cell within the line that causes contradiction when assigned the test value, -1 if the technique doesn't apply.</returns>
        /// <param name="line">the array of row/column values.</param>
        /// <param name="testValue">the value to be tested.</param>
        private static int ApplyAlsdToOneLine(string[] line, string testValue)
        {
            if (!(testValue.Equals(Puzzle.VALUE_ZERO) || testValue.Equals(Puzzle.VALUE_ONE)))
            {
                throw new ArgumentException("Invalid testValue.");
            }

            int result = -1;
            string otherValue = (testValue.Equals(Puzzle.VALUE_ZERO)) ? Puzzle.VALUE_ONE : Puzzle.VALUE_ZERO;
            int occurencesOfTestValue = Validator.CountOccurrences(line, testValue);
            int occurencesOfOtherValue = Validator.CountOccurrences(line, otherValue);
            int size = line.Length;

            // Search for pairs of cells that contain two different values according to "Different Two" technique.
            // We wil considered these pairs as "filled" with two different values, regardless of the actual order of their cells.
            // We will ignore these cells when filling and validating the line.
            List<int> excludedCells = new List<int>();

            for (int i = 0; i <= line.Length - 4; i++)
            {
                int one, two, three, four;
                string cellOne, cellTwo, cellThree, cellFour;

                one = i;
                two = i + 1;
                three = i + 2;
                four = i + 3;
                cellOne = line[one];
                cellTwo = line[two];
                cellThree = line[three];
                cellFour = line[four];

                if (!cellOne.Equals(Puzzle.DEFAULT_VALUE) && !cellFour.Equals(Puzzle.DEFAULT_VALUE) && !cellOne.Equals(cellFour))
                {
                    if (cellTwo.Equals(cellThree) && cellTwo.Equals(Puzzle.DEFAULT_VALUE))
                    {
                        excludedCells.Add(two);
                        excludedCells.Add(three);
                    }
                }
            }

            // Now proceed with the technique.
            // First adjust the number of occurences to reflect the pairs with determined values and were excluded.
            occurencesOfTestValue += excludedCells.Count / 2;
            occurencesOfOtherValue += excludedCells.Count / 2;

            // Now proceed as the normal LSD technique with the note that those cells in the excludedCells won't be filled and validated.
            // Also there's a an addition case when there's only one empty cell left in the line after excluding.
            if (((size / 2 - occurencesOfTestValue) == 1) && ((size / 2 - occurencesOfOtherValue) >= 2))
            {
                for (int c = 0; c < line.Length; c++)
                {
                    if (line[c].Equals(Puzzle.DEFAULT_VALUE) && !excludedCells.Contains(c))
                    {
                        // Create a copy of the being examined line and try assigning values on it.
                        // Note that a shallow copy is enough, as we'll only alter the elements of this
                        // copy with new string references and test it, there's no point creating new string objects
                        string[] lineCopy = (string[])line.Clone();

                        lineCopy[c] = testValue;
                        for (int c2 = 0; c2 < lineCopy.Length; c2++)
                        {
                            if (lineCopy[c2].Equals(Puzzle.DEFAULT_VALUE) && !excludedCells.Contains(c2) && (c2 != c))
                            {
                                lineCopy[c2] = otherValue;
                            }
                        }

                        // Since the line actually has some holes now, validateLine() will certainly failed,
                        // but for the purpose of this test, triplet rule check is the more suitable and perfectly
                        // sufficient one to use.
                        bool valid = Validator.TripletRuleCheck(lineCopy, Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE);
                        if (!valid)
                        {
                            result = c;
                            break;
                        }
                    }
                }
            }
            else if (((size / 2 - occurencesOfTestValue) == 0) && ((size / 2 - occurencesOfOtherValue) >= 1))
            {
                // This is a special case when after excluding the line only has one slot free.
                // And if we apply the targetValue to this empty cell contradiction will occur.
                for (int p = 0; p < line.Length; p++)
                {
                    if (line[p].Equals(Puzzle.DEFAULT_VALUE) && !excludedCells.Contains(p))
                    {
                        result = p;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Performs search algorithm with back-tracking on the parsed puzzle to find the final solution.
        /// Returns true if no contradiction found, otherwise returns false.
        /// </summary>
        /// <param name="values">Values.</param>
        /// <param name="solutions">Solutions.</param>
        /// <param name="numOfSolutions">Number of solutions.</param>
        /// <param name="totalTrials">Total number of search trials.</param>
        /// <param name="failedTrials">Total number of failed search trials.</param>
        /// <param name="silent">If set to <c>true</c> silent.</param>
        private static bool Search(string[][] values, List<string[][]> solutions, int numOfSolutions, ref long totalTrials, ref long failedTrials, bool silent)
        {
            bool finish = false;
            int size = values.Length;

            // Check if the puzzle is solved.
            bool solved = true;

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (values[row][col].Length != 1)   // or use values[row][col].Equals(Puzzle.DEFAULT_VALUE) which is probably slower.
                    {
                        solved = false;
                        break;
                    }
                }
            }



            if (solved)
            {
                solved = Validator.UniqueRowCheck<string>(values) && Validator.UniqueColumnCheck<string>(values);

                if (solved)
                {
                    // Add solution to the list, no need to create a copy of values 
                    // since itself is a copy created in the previous step.
                    solutions.Add(values);

                    // Done if we've found the required number of solutions.
                    int num = solutions.Count;

                    if (!silent)
                        Helper.LogFormat("Num of solutions found: " + num);

                    if (num == numOfSolutions)
                        finish = true;
                }
            }
            else
            {
                // If not solved, do searching.
                // Search strategy: (note that a good strategy is one that can help detect contradiction as fast as possible)
                // First determine the row/column with fewest available places for one
                // particular value (in other words, find the row/column that has the most cells assigned with
                // that value) then start the search by assigning that value (we call targetValue) 
                // to a random unsolved cell in that row/column. Backtracking is performed if contradiction found.

                // Iterate through all cells on the diagonal of the values grid, examine the 
                // row and column crossing at each cell.
                int minAvailablePlaces = -1;
                int targetRow = -1, targetCol = -1;
                string targetValue = string.Empty;

                // Loop through the grid diagonal.
                for (int i = 0; i < size; i++)
                {
                    // Check row i.
                    int rowPlacesForZero = size / 2;
                    int rowPlacesForOne = size / 2;
                    int rowUnsolvedCellIndex = -1;

                    for (int c = 0; c < size; c++)
                    {
                        if (values[i][c].Equals(Puzzle.VALUE_ZERO))
                            rowPlacesForZero--;  // count remained places for each value in row i.
                        else if (values[i][c].Equals(Puzzle.VALUE_ONE))
                            rowPlacesForOne--;
                        else
                            rowUnsolvedCellIndex = c;   // index of the last unsolved cell in row i.
                    }

                    // Check column i.
                    int colPlacesForZero = size / 2;
                    int colPlacesForOne = size / 2;
                    int colUnsolvedCellIndex = -1;

                    for (int r = 0; r < size; r++)
                    {
                        if (values[r][i].Equals(Puzzle.VALUE_ZERO))
                            colPlacesForZero--;  // count remained places for each value in column i.
                        else if (values[r][i].Equals(Puzzle.VALUE_ONE))
                            colPlacesForOne--;
                        else
                            colUnsolvedCellIndex = r;   // index of the last unsolved cell in column i.
                    }

                    // Ugly code below.
                    if ((rowPlacesForZero > 0) && ((rowPlacesForZero < minAvailablePlaces) || (minAvailablePlaces < 0)))
                    {
                        minAvailablePlaces = rowPlacesForZero;
                        targetRow = i;
                        targetCol = rowUnsolvedCellIndex;
                        targetValue = Puzzle.VALUE_ZERO;
                    }

                    if ((rowPlacesForOne > 0) && ((rowPlacesForOne < minAvailablePlaces) || (minAvailablePlaces < 0)))
                    {
                        minAvailablePlaces = rowPlacesForOne;
                        targetRow = i;
                        targetCol = rowUnsolvedCellIndex;
                        targetValue = Puzzle.VALUE_ONE;
                    }

                    if ((colPlacesForZero > 0) && ((colPlacesForZero < minAvailablePlaces) || (minAvailablePlaces < 0)))
                    {
                        minAvailablePlaces = colPlacesForZero;
                        targetRow = colUnsolvedCellIndex;
                        targetCol = i;
                        targetValue = Puzzle.VALUE_ZERO;
                    }

                    if ((colPlacesForOne > 0) && ((colPlacesForOne < minAvailablePlaces) || (minAvailablePlaces < 0)))
                    {
                        minAvailablePlaces = colPlacesForOne;
                        targetRow = colUnsolvedCellIndex;
                        targetCol = i;
                        targetValue = Puzzle.VALUE_ONE;
                    }
                }

                // Now try assigning targetValue to the cell at targetRow, targetCol.
                // If contradiction found, try again with the other value (backtracking).
                string otherValue = string.Equals(targetValue, Puzzle.VALUE_ZERO) ? Puzzle.VALUE_ONE : Puzzle.VALUE_ZERO;
                string[] trialValues = { targetValue, otherValue };

                for (int j = 0; j < trialValues.Length; j++)
                {
                    totalTrials++;     // count number of trials for analysis.

                    // Create a copy of values for trials. Note that we need to use Helper.deepCopyOfGrid() to
                    // actually create a new 2D array "instance". Arrays.copyOf() won't work here since
                    // values is a 2D array, and Arrays.copyOf() will only copy the reference of each "row" of 
                    // values to valuesCopy, resulting in values and valuesCopy share same memory location for
                    // their "rows", in other words, they point to same "row" arrays). Consequently,
                    // alter valuesCopy will also affect values. Helper.deepCopyOfGrid() can solve this issue.
                    string[][] valuesCopy = Helper.DeepCopyJaggedArray(values);
                    bool success = AssignCellValue(valuesCopy, targetRow, targetCol, trialValues[j], true);
                    if (success)
                    {
                        finish = Search(valuesCopy, solutions, numOfSolutions, ref totalTrials, ref failedTrials, silent);     // continue searching if no contradiction found.
                        if (finish)
                            break;
                    }
                    else
                    {
                        failedTrials++;    // count number of failed trials for analysis.
                    }
                }
            }

            return finish;
        }

        /// <summary>
        /// Checks for contradiction in the given triplet (3-in-a-row cells), as well as
        /// assigns unfilled cell if a confirmed value for that cell is already deduced.
        /// </summary>
        /// <returns>true if no contradiction occurs, false otherwise.</returns>
        /// <param name="values">Values.</param>
        /// <param name="tripletIndices">Triplet indices.</param>
        private static bool FillTriplet(string[][] values, int[][] tripletIndices, bool silent = true)
        {
            int firstRow = tripletIndices[0][0];
            int firstCol = tripletIndices[0][1];
            int middleRow = tripletIndices[1][0];
            int middleCol = tripletIndices[1][1];
            int lastRow = tripletIndices[2][0];
            int lastCol = tripletIndices[2][1];

            string first = values[firstRow][firstCol];
            string middle = values[middleRow][middleCol];
            string last = values[lastRow][lastCol];

            string[] triplet = { first, middle, last };
            bool valid = Validator.TripletRuleCheck(triplet, Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE);

            if (!valid)
            {
                if (!silent)
                    Helper.LogFormat("Contradiction: 3-in-a-row detected at row {0:D}, column {1:D}.", middleRow, middleCol);
                return false;
            }

            // Construct a combined string of three triplet cells' values for ease of pattern detection.
            // First replace any default values ("01") with dot "." to match with the patterns style.
            if (first.Equals(Puzzle.DEFAULT_VALUE))
                first = Puzzle.DOT;

            if (middle.Equals(Puzzle.DEFAULT_VALUE))
                middle = Puzzle.DOT;

            if (last.Equals(Puzzle.DEFAULT_VALUE))
                last = Puzzle.DOT;

            StringBuilder sb = new StringBuilder();
            sb.Append(first).Append(middle).Append(last);

            string combined = sb.ToString();

            foreach (string p1 in Puzzle.CONFIRMED_PATTERNS_1)
            {
                if (combined.Equals(p1))
                {
                    int row, col;
                    if (first.Equals(Puzzle.DOT))
                    {
                        row = firstRow;
                        col = firstCol;
                    }
                    else if (middle.Equals(Puzzle.DOT))
                    {
                        row = middleRow;
                        col = middleCol;
                    }
                    else
                    {
                        row = lastRow;
                        col = lastCol;
                    }

                    bool success = AssignCellValue(values, row, col, Puzzle.VALUE_ZERO);

                    if (!success)
                        return false;
                }
            }

            foreach (string p2 in Puzzle.CONFIRMED_PATTERNS_2)
            {
                if (combined.Equals(p2))
                {
                    int row, col;
                    if (first.Equals(Puzzle.DOT))
                    {
                        row = firstRow;
                        col = firstCol;
                    }
                    else if (middle.Equals(Puzzle.DOT))
                    {
                        row = middleRow;
                        col = middleCol;
                    }
                    else
                    {
                        row = lastRow;
                        col = lastCol;
                    }

                    bool success = AssignCellValue(values, row, col, Puzzle.VALUE_ONE);

                    if (!success)
                        return false;
                }
            }

            // If we reach here no contradiction found.
            return true;
        }

        /// <summary>
        /// Assigns the given value to the cell specified by row and column in the given grid,
        /// then performs recursive constraint propagation if the previous assigning is successful.
        /// </summary>
        /// <returns><c>true</c>, if no contradiction found, <c>false</c> otherwise.</returns>
        /// <param name="values">Values.</param>
        /// <param name="row">Row.</param>
        /// <param name="col">Col.</param>
        /// <param name="val">Value.</param>
        private static bool AssignCellValue(string[][] values, int row, int col, string val, bool validateColumnUnique = false)
        {
            if (!(val.Equals(Puzzle.VALUE_ZERO) || val.Equals(Puzzle.VALUE_ONE)))
            {
                throw new ArgumentException("Invalid targetValue.");
            }

            string otherVal = (val.Equals(Puzzle.VALUE_ZERO)) ? Puzzle.VALUE_ONE : Puzzle.VALUE_ZERO;

            // Eliminate otherVal from cell's string.
            int index = values[row][col].IndexOf(otherVal);
            StringBuilder sb = new StringBuilder(values[row][col]);

            if (index > -1)
            {
                values[row][col] = sb.Remove(index, 1).ToString();
            }

            if (values[row][col].Length == 0)
            {
                return false;   // contradiction: removed last value.
            }

            // Get row and column arrays of the cell.
            string[] rowArray = Helper.GetRow(values, row);
            string[] colArray = Helper.GetColumn(values, col);

            int rowOccurrences = Validator.CountOccurrences(rowArray, val);
            int colOccurrences = Validator.CountOccurrences(colArray, val);

            if ((rowOccurrences > rowArray.Length / 2) || (colOccurrences > colArray.Length / 2))
            {
                return false;   // contradiction: exceed allowed number of occurrences of a value.
            }

            // Now fill other cells by constraint propagation.
            // 1st RULE: if the number of occurrences of one value in a row/column reaches puzzleSize/2,
            // fill all unfilled cells in that row/column with the other value.
            if (rowOccurrences == rowArray.Length / 2)
            {
                for (int c = 0; c < rowArray.Length; c++)
                {
                    if ((c != col) && (rowArray[c].Length > 1))
                    {
                        bool success = AssignCellValue(values, row, c, otherVal);     // assign other unfilled cells in the row with otherVal.
                        if (!success)
                            return false;
                    }
                }
            }

            if (colOccurrences == colArray.Length / 2)
            {
                for (int r = 0; r < colArray.Length; r++)
                {
                    if ((r != row) && (colArray[r].Length > 1))
                    {
                        bool success = AssignCellValue(values, r, col, otherVal);     // assign other unfilled cells in the column with otherVal.
                        if (!success)
                            return false;
                    }
                }
            }

            // 2nd RULE: if any group of 3 consecutive cells (triplet) has 2 cells of same value, 
            // then the other cell must have different value.
            List<int[][]> triplets = Helper.GetTripletsIndices(values.Length, row, col);
            foreach (int[][] triplet in triplets)
            {
                bool success = FillTriplet(values, triplet);
                if (!success)
                    return false;     // contradiction: 3-in-a-row detected.
            }

            if (validateColumnUnique && !Validator.UniqueColumnCheck<string>(values, Puzzle.DEFAULT_VALUE))
            {
                //UnityEngine.Debug.LogError("Failed here boy");
                ////UnityEngine.Debug.Log(Helper.PuzzleStringGridToString(values));
                //Helper.PrintGrid(values);
                //Helper.LogFormat("====================");
                return false;
            }

            // If we reach here no contradiction found.
            return true;
        }
    }
}
