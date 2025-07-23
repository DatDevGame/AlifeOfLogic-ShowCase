/// <summary>
/// This class is responsible for generating puzzles that match the input requirements.
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Takuzu.Generator
{
    public struct GenerationInfo
    {
        public bool shouldStop;
        public bool isStopped;
        public int generatedPuzzles;
        public int totalPuzzles;

        public GenerationInfo(int totalPuzzles)
        {
            this.totalPuzzles = totalPuzzles;
            shouldStop = false;
            isStopped = true;
            generatedPuzzles = 0;
        }

    }

    public static class Generator
    {
        public static bool AnalyzeMode = false;

        public static Action<string> onFailed = delegate { };
        public static Action<Puzzle> onNewPuzzleCreated = delegate { };

        public static Stopwatch stopwatch;
        public static int minGivensToSolveWithoutSearch = -1;
        public static int minGivensToHaveUniqueSolution = -1;

        public static long totalDigHolesTrials = 0;
        public static long failedDigHolesTrials = 0;
        public static long totalCompensateTrials = 0;
        public static long failedCompensateTrials = 0;

        /// <summary>
        /// Creates the puzzles by removing cells from the input grid ("holes digging")
        /// to form valid puzzles with the desired number of givens and have a unique solution which is the input grid.
        /// </summary>
        /// <returns>list of generated puzzles.</returns>
        /// <param name="grid">the input grid to dig holes from.</param>
        /// <param name="numOfGivens">number of givens of the generated puzzle.</param>
        /// <param name="level">level the required level, passing Level.UnGraded means ignored (all level accepted).</param>
        /// <param name="numOfPuzzles">number of distinct puzzles to create, 
        ///                             -1 means all possible puzzles can be generated with the input conditions.
        ///                             0 is illegal.
        ///                             Note that if we pass -1 for numOfPuzzles, we also need to pass -1 for maxTrialsPerAttempt,
        ///                             and a value different from -1 for maxAttempts to enable exhausted,
        ///                             otherwise the process will run forever without exit point.</param>
        /// <param name="excludedList">any puzzle that replicates a puzzle included in this list will be discarded.</param>
        /// <param name="maxAttempts">maximum number of dig holes sessions to perform, the process terminates if this limit is reached,
        ///                         -1 means no limit. 0 is illegal.
        ///                         Use this parameter with care since it may cause the process to loop forever
        ///                         (especially if maxTrialsPerAttempt is too small to find a valid puzzle,
        ///                         or in some cases it's actually impossible to find the required puzzle).</param>
        /// <param name="maxTrialsPerAttempt">stop a dig holes session if reaches this number of trials, -1 means no limit.
        ///                                     This limit is used to "free" the session from sinking into a "hard" route where it takes too long to find the valid puzzle.</param>
        /// <param name="maxCompensateTrialsPerDigHoleSession">stop a compensate session if reaches this number of trials, -1 means no limit. 
        /// This limit is used to "free" the session from sinking into a "hard" route where it takes too long to find the valid puzzle.
        /// Be cautious when using -1 as it may take forever with large grids (e.g. >=10).</param>
        /// <param name="silent">whether or not display debug text to console.</param>
        /// <param name="info">the generation info.</param>
        public static List<Puzzle> GeneratePuzzles(string[][] grid, int numOfGivens, Level[] level, int numOfPuzzles, ICollection<string> excludedList, int maxAttempts, long maxTrialsPerAttempt, long maxCompensateTrialsPerDigHoleSession, bool silent, ref GenerationInfo info)
        {
            // Argument checking...
            if ((numOfPuzzles <= -1) && (maxAttempts <= -1) && (maxTrialsPerAttempt >= 0))
            {
                // This combination of arguments will cause the program to loop forever.
                throw new ArgumentException("The current set of arguments (numOfPuzzles<=-1 && maxAttempts<=-1 && maxTrialsPerAttempt>=0) will cause infinite loop.");
            }
            else if ((maxAttempts == 0) || (numOfPuzzles == 0))
            {
                throw new ArgumentException();
            }

            // Use a stopwatch to measure executing time.
            stopwatch = new Stopwatch();
            stopwatch.Start();

            // Reset stats.
            minGivensToSolveWithoutSearch = -1;
            minGivensToHaveUniqueSolution = -1;

            // The list to hold generated puzzles.
            List<Puzzle> puzzles = new List<Puzzle>();

            // Make a deep copy to use throughout the process so that the original grid won't be changed.
            string[][] puzzle = Helper.DeepCopyJaggedArray(grid);

            // Deduce the required puzzle size
            int size = grid.Length;

            // Calculate the number of holes to dig from the grid.
            int numOfHoles = size * size - numOfGivens;

            // Dig holes to form valid puzzles and repeat until one of the following happens:
            //  - The desired number of puzzles has been created.
            //  - The maximum number of attempts allowed was met.
            //  - The process has run to exhausted (tried all possibilites) without successful.
            //  Exhausted means it's impossible to generate a valid puzzle with the required number of givens from the input grid.
            //  Currently, we're not sure if this case applies for other grids, or maybe some
            //  of them may be possible to generate the desired puzzle from.
            int attempts = 0;   // number of attempt to generate puzzles, each dig holes session counts as an attempt.
            bool success = true;
            bool reachedMaxAttempts = false;
            bool createdEnoughPuzzles = false;

            if (AnalyzeMode)
                Helper.LogFormat("Generating {0}x{0} puzzles with {1} givens...", size, numOfGivens);

            do
            {
                if (AnalyzeMode)
                    Helper.LogFormat("Attempt #{0} of {1}; Puzzles created: {2}\r", attempts + 1, maxAttempts, puzzles.Count);
                // Reset trials counts for the new session.
                totalDigHolesTrials = 0;
                failedDigHolesTrials = 0;

                // Start the new dig holes session and then determine stopping conditions.
                success = success && DigHoles(puzzle, grid, numOfGivens, numOfHoles, level, puzzles, maxTrialsPerAttempt, maxCompensateTrialsPerDigHoleSession, excludedList, ref info);
                reachedMaxAttempts = ++attempts == maxAttempts;
                createdEnoughPuzzles = puzzles.Count == numOfPuzzles;

            } while (success && !reachedMaxAttempts && !createdEnoughPuzzles && !info.shouldStop);
            stopwatch.Stop();

            if (!silent && AnalyzeMode)
            {
                Helper.LogFormat("===========================================================");
                Helper.LogFormat("Finished creating puzzles. Statistics:");
                string levels = "";
                foreach (Level l in level)
                {
                    levels += l.ToString() + " ";
                }
                Helper.LogFormat("Puzzles required:\t\t{0} (size {1}x{1}, {2} givens, level {3})", numOfPuzzles, size, numOfGivens, levels);
                Helper.LogFormat("Puzzles created:\t\t{0}", puzzles.Count);
                Helper.LogFormat("Max attempts allowed:\t\t{0}", maxAttempts);
                Helper.LogFormat("Attempts:\t\t\t{0}", attempts);
                Helper.LogFormat("Max trials per attempt:\t\t{0}", maxTrialsPerAttempt);
                Helper.LogFormat("Last totalDigHolesTrials:\t{0}", totalDigHolesTrials);
                Helper.LogFormat("Last failedDigHolesTrials:\t{0} ({1})", failedDigHolesTrials, (float)failedDigHolesTrials / totalDigHolesTrials);
                Helper.LogFormat("Last totalCompensateTrials:\t{0}", totalCompensateTrials);
                Helper.LogFormat("Last failedCompensateTrials:\t{0} ({1})", failedCompensateTrials, (float)failedCompensateTrials / totalCompensateTrials);
                Helper.LogFormat("Exhausted:\t\t\t{0}", (!success).ToString());
                Helper.LogFormat("minGivensToHaveUniqueSolution:\t{0}", minGivensToHaveUniqueSolution);
                Helper.LogFormat("minGivensToSolveWithoutSearch:\t{0}", minGivensToSolveWithoutSearch);
                Helper.LogFormat("Time:\t\t\t\t{0}", Helper.FormatTime(stopwatch.Elapsed));
                Helper.LogFormat("===========================================================");
            }

            return puzzles;
        }

        /// <summary>
        /// Digs holes from the input grid to form valid puzzles with desired numOfHoles,
        /// and have one unique solution, which is the input grid itself.
        /// </summary>
        /// <returns>true if the process has finished, false otherwise.</returns>
        /// <param name="puzzle">the puzzle being generated by digging holes.</param>
        /// <param name="solution">the solution of the puzzle.</param>
        /// <param name="numOfGivens">the required number of givens of the puzzle.</param>
        /// <param name="numOfHoles">the number of holes to dig.</param>
        /// <param name="level">the required puzzle level.</param>
        /// <param name="puzzles">the list to add the generated puzzle to.</param>
        /// <param name="maxTrialsPerAttempt">maximum number of trials allowed, the process is terminated if this number is reached.</param>
        /// <param name="maxCompensateTrials">stop a compensate session if reaches this number of trials, -1 means no limit. 
        /// This limit is used to "free" the session from sinking into a "hard" route where it takes too long to find the valid puzzle.
        /// Be cautious when using -1 as it may take forever with large grids (e.g. >=10).</param>
        /// <param name="excludedList">the list contains puzzles to be excluded.</param>
        /// <param name="info">the generation info.</param>
        private static bool DigHoles(string[][] puzzle, string[][] solution, int numOfGivens, int numOfHoles, Level[] level, List<Puzzle> puzzles, long maxTrialsPerAttempt, long maxCompensateTrials, ICollection<string> excludedList, ref GenerationInfo info)
        {
            if (info.shouldStop)
                return true;

            bool finish = false;

            if (AnalyzeMode)
                Helper.LogFormat("<color=green>Digging holes. numOfHoles: " + numOfHoles + "</color>");

            if (numOfHoles == 0)
            {
                if (AnalyzeMode)
                {
                    Helper.LogFormat("<color=green>Puzzle before eliminating redundancies:</color>");
                    Helper.PrintGrid(puzzle);
                }
                // Eliminate redundancies if any.
                int eliminateNum = EliminateRedundantGivens(puzzle);
                if (AnalyzeMode)
                    Helper.LogFormat("<color=green>eliminateNum: " + eliminateNum + "</color>");

                // Compensate eliminated cells and validate final puzzle. The process
                // finishes if compensate is successful.
                // Since we're starting a new compesate session, we need to reset the counters.
                totalCompensateTrials = 0;
                failedCompensateTrials = 0;
                bool success = Compensate(puzzle, solution, numOfGivens, eliminateNum, level, puzzles, maxCompensateTrials, excludedList, ref info);

                if (success)
                {
                    finish = true;
                }
            }
            else
            {
                // Try removing a random cell from the puzzle grid and see if it still
                // satisfies the condition of having only one unique solution.
                int size = puzzle.Length;
                int[] randomPositions = Helper.GenerateShuffledList(size * size);    // list of cell positions in a random order.

                for (int i = 0; i < randomPositions.Length; i++)
                {
                    if (AnalyzeMode)
                    {
                        Helper.LogFormat("totalDigHolesTrials {0}, maxTrialsPerAttempt {1}", totalDigHolesTrials, maxTrialsPerAttempt);
                    }

                    // Stop if maximum allowed number of trials for this session has been reached.
                    if (totalDigHolesTrials == maxTrialsPerAttempt)
                    {
                        finish = true;
                        break;
                    }

                    // Otherwise continue digging.  
                    int position = randomPositions[i];
                    int row = position / size;
                    int col = position % size;

                    // Remove cell value if it was not removed before.
                    if (!puzzle[row][col].Equals(Puzzle.DOT))
                    {
                        string[][] puzzleCopy = Helper.DeepCopyJaggedArray(puzzle);  // create a deep copy of the puzzle and work on it.
                        puzzleCopy[row][col] = Puzzle.DOT;  // remove the value from the cell by setting it to a dot.

                        if (AnalyzeMode)
                            Helper.LogFormat("<color=green>Dig hole at row {0}, col {1}.</color>", row, col);

                        totalDigHolesTrials++;

                        //Display progress.
                        if (maxTrialsPerAttempt <= 0)
                        {
                            if (AnalyzeMode)
                                Helper.LogFormat((totalDigHolesTrials % 2 == 0) ? "<color=green>.  \r</color>" : "<color=green>.. \r</color>");   // spaces added to clear garbage from previous print command.
                        }
                        else
                        {
                            if (AnalyzeMode)
                                Helper.LogFormat("<color=green>Current attempt progress: {0}  \r</color>", (float)totalDigHolesTrials / maxTrialsPerAttempt);
                        }

                        // Now check if the puzzle is still uniquely solved.
                        Solver.SolvingStatistics stats;
                        bool valid = Solver.HasUniqueSolution(puzzleCopy, out stats);

                        if (valid)
                        {
                            // Count the number of remained givens in the current puzzle for analysis.
                            // This is only applicable when running in exhausted mode.
                            if (AnalyzeMode)
                            {
                                int remainedGivens = Validator.CountKnownCells(puzzleCopy, Puzzle.VALUE_ZERO, Puzzle.VALUE_ONE);

                                if ((remainedGivens < minGivensToHaveUniqueSolution) || (minGivensToHaveUniqueSolution < 0))
                                {
                                    minGivensToHaveUniqueSolution = remainedGivens;
                                }

                                if (stats.numOfSolvedCellsAfterParsing == (size * size))
                                {
                                    if ((remainedGivens < minGivensToSolveWithoutSearch) || (minGivensToSolveWithoutSearch < 0))
                                    {
                                        minGivensToSolveWithoutSearch = remainedGivens;
                                    }
                                }
                            }

                            // Puzzle has one unique solution and is valid. Continue removing.
                            finish = DigHoles(puzzleCopy, solution, numOfGivens, numOfHoles - 1, level, puzzles, maxTrialsPerAttempt, maxCompensateTrials, excludedList, ref info);
                            if (finish)
                                break;
                        }
                        else
                        {
                            if (AnalyzeMode)
                            {
                                Helper.LogFormat("<b><color=green>Current puzzle is invalid. Retrying...</color></b>");
                                //Helper.PrintGrid(puzzleCopy);
                            }
                            failedDigHolesTrials++;
                            onFailed("unique solution");
                        }
                    }
                }
            }

            return finish;
        }

        /// <summary>
        /// Checks for redundant given cells in the input puzzle grid. Redundant given cells are those whose
        /// values can be easily deduced from the game rules. Effectively, this methods detects groups of three consecutive cells
        /// where all values are given. If found, remove value from a random cell among the three.
        /// </summary>
        /// <returns>number of redundant cells eliminated.</returns>
        /// <param name="puzzle">the puzzle grid to eliminate redundancy.</param>
        private static int EliminateRedundantGivens(string[][] puzzle)
        {
            int eliminateNum = 0;   // number of eliminated redundant cells.
            int size = puzzle.Length;

            // Iterate through all rows and columns and examine all triplets in each of them.
            // We'll eliminate the redundant cell which is the cell whose value can be logically
            // deduced from the values of the two remained cells. For example "1" in "010", "0" in "110", etc.
            for (int i = 0; i < size; i++)
            {
                // Check triplets in row i.
                for (int c = 0; c <= size - 3; c++)
                {
                    string first = puzzle[i][c];
                    string middle = puzzle[i][c + 1];
                    string last = puzzle[i][c + 2];

                    // Redundant if all three values were given. Remove the one that has different value than the other.
                    if ((!first.Equals(Puzzle.DOT)) && (!middle.Equals(Puzzle.DOT)) && (!last.Equals(Puzzle.DOT)))
                    {
                        int row = -1;
                        int col = -1;
                        bool equal1 = first.Equals(middle);
                        bool equal2 = middle.Equals(last);

                        if (!equal1 && equal2)
                        {
                            // the different cell is the first one,
                            row = i;
                            col = c;
                        }
                        else if (!equal1 && !equal2)
                        {
                            // the middle one,
                            row = i;
                            col = c + 1;
                        }
                        else if (equal1 && !equal2)
                        {
                            // or the last one.
                            row = i;
                            col = c + 2;
                        }

                        // Eliminate the different cell which is the redundant one.
                        puzzle[row][col] = Puzzle.DOT;
                        eliminateNum++;

                        if (AnalyzeMode)
                        {
                            Helper.LogFormat("Eliminate redundant cell at row {0}, col {1}", row, col);
                        }
                    }
                }

                // Check triplets in column i.
                for (int r = 0; r <= size - 3; r++)
                {
                    string first = puzzle[r][i];
                    string middle = puzzle[r + 1][i];
                    string last = puzzle[r + 2][i];

                    // Repeat the above process for column.
                    if ((!first.Equals(Puzzle.DOT)) && (!middle.Equals(Puzzle.DOT)) && (!last.Equals(Puzzle.DOT)))
                    {
                        int row = -1, col = -1;
                        bool equal1 = first.Equals(middle);
                        bool equal2 = middle.Equals(last);

                        if (!equal1 && equal2)
                        {
                            row = r;
                            col = i;
                        }    // the different cell is the first one, or...
                        else if (!equal1 && !equal2)
                        {
                            row = r + 1;
                            col = i;
                        }  // the middle one, or...
                        else if (equal1 && !equal2)
                        {
                            row = r + 2;
                            col = i;
                        }   // the last one.

                        puzzle[row][col] = Puzzle.DOT;

                        eliminateNum++;

                        if (AnalyzeMode)
                        {
                            Helper.LogFormat("Eliminate redundant cell at row {0}, col {1}", row, col);
                        }
                    }
                }
            }

            return eliminateNum;
        }

        /// <summary>
        /// Adds the required number of values to random cells in the puzzle with such that they
        /// won't create redundancy. The values set to those cells are the values
        /// of corresponding cells in the solution grid.
        /// In the end, the final puzzle will be validated for unique solution requirement,
        /// if valid, adds it to the puzzle list and terminates the process, provided that
        /// the puzzle doesn't repeat another puzzle formerly added to the list.
        /// </summary>
        /// <returns>true if a valid puzzle is formed and added to puzzle list, false if all trials failed.</returns>
        /// <param name="puzzle">input puzzle grid to add cell values.</param>
        /// <param name="solution">the solution grid where values are taken to fill into the puzzle grid.</param>
        /// <param name="numOfGivens">number of givens of the puzzle.</param>
        /// <param name="compensateNum">number of cells to add values to.</param>
        /// <param name="level">level the required level, passing Level.UnGraded means ignored (all level accepted).</param>
        /// <param name="puzzles">list contains generated puzzles.</param>
        /// <param name="maxCompensateTrials">stop a compensate session if reaches this number of trials, -1 means no limit. 
        /// This limit is used to "free" the session from sinking into a "hard" route where it takes too long to find the valid puzzle.
        /// Be cautious when using -1 as it may take forever with large grids (e.g. >=10).</param>
        /// <param name="excludedList">list contains puzzles to not replicate.</param>
        /// <param name="info">the generation info.</param>
        private static bool Compensate(string[][] puzzle, string[][] solution, int numOfGivens, int compensateNum, Level[] level, List<Puzzle> puzzles, long maxCompensateTrials, ICollection<string> excludedList, ref GenerationInfo info)
        {
            if (info.shouldStop)
            {
                return true;
            }

            bool finish = false;

            if (compensateNum == 0)
            {
                if (totalCompensateTrials == maxCompensateTrials)
                {
                    return true;
                }

                totalCompensateTrials++;

                // We'll see if it is valid: has one unique solution and desired level
                Solver.SolvingStatistics stats;
                bool valid = IsGoodPuzzle(puzzle, level, out stats);

                if (valid)
                {
                    // Puzzle has one unique solution and desired level and therefore is valid.
                    // But we still need to check if it repeats another one formerly added to the puzzle list.
                    // And we also need to make sure that it is not in the exclude list either!
                    bool repeat = false;
                    string puzzleStr = Helper.PuzzleStringGridToString(puzzle);     // convert puzzle grid to string for comparison purpose.    

                    repeat = excludedList.Contains(puzzleStr);

                    if (repeat)
                    {
                        onFailed("replicate");
                        return finish;
                    }
                    else
                    {
                        foreach (string ep in excludedList)
                        {
                            repeat = puzzleStr.Equals(ep);
                            if (repeat)
                            {
                                onFailed("exclude");
                                return finish;
                            }
                        }
                    }

                    // Create a new Puzzle instance and add it to the list if not repeated.
                    if (!repeat)
                    {
                        int size = puzzle.Length;
                        string solutionStr = Helper.PuzzleStringGridToString(solution);
                        int parseNum = stats.numOfSolvedCellsAfterParsing;
                        int lsdNum = stats.numOfSolvedCellsAfterLsd;
                        int alsdNum = stats.numOfSolvedCellsAfterAlsd;
                        Level l = LevelDef.GradePuzzle((Size)size, stats).level;
                        Puzzle newPuzzle = new Puzzle((Size)size, l, puzzleStr, solutionStr, numOfGivens, parseNum, lsdNum, alsdNum);
                        puzzles.Add(newPuzzle);

                        info.generatedPuzzles += 1;
                        Helper.LogSuccessFormat("New puzzle created: level {0}, size {1}x{1}, {2} givens ", l, size, numOfGivens);
                        onNewPuzzleCreated(newPuzzle);

                        finish = true;  // job is done!
                    }
                    else
                    {
                        if (AnalyzeMode)
                        {
                            Helper.LogFormat("<b><color=magneta>*** Discarded repeated puzzle. </color></b>***");
                        }
                    }
                }

                if (!finish)
                {
                    if (AnalyzeMode) Helper.LogFormat("Compensating completed. Puzzle is invalid. Process repeating... totalCompensateTrials {0}, maxCompensateTrials {1}", totalCompensateTrials, maxCompensateTrials);
                    failedCompensateTrials++;
                }
            }
            else
            {
                // More compensating to do.
                int size = puzzle.Length;
                int[] randomPositions = Helper.GenerateShuffledList(size * size);    // list of cell positions in a random order.
                for (int i = 0; i < randomPositions.Length; i++)
                {
                    int position = randomPositions[i];
                    int row = position / size;
                    int col = position % size;

                    string cell = puzzle[row][col];
                    bool unoccupied = (cell.Equals(Puzzle.DOT));

                    if (unoccupied)
                    {
                        // Before filling a value to this cell, we need to make sure
                        // that we won't create a new redundancy, so we need to check
                        // all triplets involving this cell and make sure that no triplet
                        // has 2 occupied cells (that will create redundancy when third cell is filled).
                        List<string[]> triplets = Helper.GetTripletsValue(puzzle, row, col);

                        bool willBeRedundant = false;
                        foreach (string[] triplet in triplets)
                        {
                            string first = triplet[0];
                            string middle = triplet[1];
                            string last = triplet[2];

                            int occupiedCells = 0;

                            if (!first.Equals(Puzzle.DOT))
                                occupiedCells++;

                            if (!middle.Equals(Puzzle.DOT))
                                occupiedCells++;

                            if (!last.Equals(Puzzle.DOT))
                                occupiedCells++;

                            if (occupiedCells == 2)
                            {
                                willBeRedundant = true;
                                break;
                            }
                        }

                        // If redundancy won't be created, fill the cell with the corresponding value of the cell
                        // at the same position in the solution grid and continue with the next compensate position.
                        if (!willBeRedundant)
                        {
                            // Create a copy grid of the puzzle grid and work on the copy rather the original grid.
                            string[][] puzzleCopy = Helper.DeepCopyJaggedArray(puzzle);
                            puzzleCopy[row][col] = solution[row][col];

                            // Continue compensating next position.
                            finish = Compensate(puzzleCopy, solution, numOfGivens, compensateNum - 1, level, puzzles, maxCompensateTrials, excludedList, ref info);
                            if (finish)
                                break;
                        }
                    }
                }
            }

            return finish;
        }

        /// <summary>
        /// Checks if the puzzle has unique solution and desired level of difficulty.
        /// </summary>
        /// <returns>True if satisfies all conditions, False otherwise.</returns>
        /// <param name="puzzle">The puzzle to examine.</param>
        /// <param name="level">The required level, passing Level.Ungraded means ignored (all level accepted).</param>
        /// <param name="stats">Solving statistics.</param>
        private static bool IsGoodPuzzle(string[][] puzzle, Level[] level, out Solver.SolvingStatistics stats)
        {
            bool isGood = true;

            // Init solving stats with default values.
            stats = default(Solver.SolvingStatistics);

            // First check if the puzzle has unique solution. Note that hasUniqueSolution()
            // will solve the puzzle and store its solving statistic in relevant variables
            // that we get below.
            isGood = isGood && Solver.HasUniqueSolution(puzzle, out stats);
            if (!isGood)
                Generator.onFailed("unique solution");
            // Check the difficulty level requirement if any.
            if (!Helper.ArrayContains<Level>(level, Level.UnGraded))
            {
                // Check if the puzzle has the desired difficulty level.
                int size = puzzle.Length;
                Size enumSize = (Size)size;
                Level l = LevelDef.GradePuzzle(enumSize, stats).level;

                // Puzzle is valid if it has unique solution and desired level of difficulty.
                isGood = isGood && Helper.ArrayContains<Level>(level, l);
                if (!isGood)
                    Generator.onFailed("level");
            }

            return isGood;
        }
    }
}

