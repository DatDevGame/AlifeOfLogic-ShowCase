/// <summary>
/// This class contains methods to fill values into an empty grid until to form a valid grid.
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Takuzu.Generator
{
    public static class Filler
    {
        public static Action<string> onFailed = delegate { };
        public static bool analyzeMode = false;

        public static long totalTrials = 0;
        public static long failedTrials = 0;

        // used to inform how much of the grid was constructed.
        public static int currentFinishedRow = -1;
        private static HashSet<int> selectedIndex;

        /// <summary>
        /// Generates numOfGrids valid grids of size x size that satisfies 2 game rules.
        /// This method stops when one of the following happens: 
        ///      - The desired number of grids has been created.
        ///      - The maximum number of attempts was met.
        ///      - formGrid() has run to exhausted (tried all possibilities).
        /// </summary>
        /// <returns>The list of generated grids serialized as strings.</returns>
        /// <param name="size">Size of grids to be generated.</param>
        /// <param name="value1">Value1.</param>
        /// <param name="value2">Value2.</param>
        /// <param name="numOfGrids">Number of grids to generate, -1 means all possible grids, 0 is illegal.</param>
        /// <param name="maxAttempts">Maximum number of attempts allowed, -1 means no limit, 0 is illegal.</param>
        /// <param name="maxTrialsPerAttempt">Maximum number of trials allowed in one form grid session, -1 means no limit.</param>
        /// <param name="excludeList">Excluded list.</param>
        public static List<string> FormGrids(int size, int value1, int value2, int numOfGrids, int maxAttempts, long maxTrialsPerAttempt, IEnumerable<string> excludeList, ref GenerationInfo info)
        {
            // Arguments checking.
            if ((maxAttempts == 0) || (numOfGrids == 0))
            {
                throw new ArgumentException();
            }

            // Use a stopwatch to measure executing time.
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int[] lineIndices = new int[size];   // array contains index numbers in a line: 1,2,3,4,etc.
            for (int i = 0; i < lineIndices.Length; i++)
            {
                lineIndices[i] = i;
            }

            // First find list of valid lines, valid lines are arrays containing occurrences of
            // value1, value2 so that the 2 rules of the game are satisfied.
            List<int[]> validLines = FormValidLines(lineIndices, value1, value2);

            // Create a list to hold generated grids.
            List<string> listOfGrids = new List<string>();

            // Create a placeholding grid used for construction and validation.
            int[][] grid = new int[size][];
            for (int i = 0; i < size; i++)
            {
                grid[i] = new int[size];
            }

            // Form grids by adding rows one-by-one. This process will be repeated until one of the following happens:
            //  - The desired number of grids has been created.
            //  - The maximum number of attempts was met.
            //  - formGrid() has run to exhausted (tried all possibilities).
            int attempts = 0;
            bool exhausted = false;
            bool reachedMaxAttempts = false;
            bool createdEnoughGrids = false;

            do
            {
                //if (analyzeMode)
                //    Helper.LogFormat("Current attempt: {0:D}; Grids created: {1:D}", attempts + 1, listOfGrids.Count);
                // Reset stats.
                totalTrials = 0;
                failedTrials = 0;
                currentFinishedRow = -1;
                int[] shuffleIndices = Helper.GenerateShuffledList(validLines.Count);
                //string s = "";
                //for (int i=0;i<shuffleIndices.Length;++i)
                //{
                //    s += shuffleIndices[i] + " ";
                //}
                //UnityEngine.Debug.Log("<b><color=red>" + s + "</color></b>");
                //debugIndex = new List<int>();
                selectedIndex = new HashSet<int>();
                // Try forming a new grid and determine terminating conditions.
                exhausted = exhausted || !FormOneGrid(validLines, shuffleIndices, 0, size, value1, value2, 0, grid, listOfGrids, maxTrialsPerAttempt, excludeList, ref info);
                reachedMaxAttempts = (++attempts == maxAttempts);
                createdEnoughGrids = listOfGrids.Count == numOfGrids;

            } while (!exhausted && !reachedMaxAttempts && !createdEnoughGrids && !info.shouldStop);

            // Stop the stopwatch and get executing time.
            stopwatch.Stop();
            if (analyzeMode)
            {
                Helper.LogFormat("===========================================================");
                Helper.LogFormat("Finished generating valid grids. Results:");
                Helper.LogFormat("Grids required:\t\t{0} (size {1}x{1})", numOfGrids, size);
                Helper.LogFormat("Grids constructed:\t{0} ({1} requirement)", listOfGrids.Count, (float)listOfGrids.Count / numOfGrids);
                Helper.LogFormat("Max attempts allowed:\t{0}", maxAttempts);
                Helper.LogFormat("Attempts:\t\t{0}", attempts);
                Helper.LogFormat("Max trials per attempt:\t{0}", maxTrialsPerAttempt);
                Helper.LogFormat("Last totalTrials:\t{0}", totalTrials);
                Helper.LogFormat("Last failedTrials:\t{0} ({1})", failedTrials, (float)failedTrials / totalTrials);
                Helper.LogFormat("Time:\t\t\t{0}", Helper.FormatTime(stopwatch.Elapsed));
                Helper.LogFormat("===========================================================");
            }
            return listOfGrids;
        }
        /// <summary>
        /// Form a valid grid by adding rows taken from the list of validLines one-by-one to an empty grid.
        /// Validating is performed while adding rows to make sure that the formed grid satisfies all game rules.
        /// Note that this method calls itself recursively.
        /// </summary>
        /// <returns><c>true</c>, if the process has finished, <c>false</c> otherwise.</returns>
        /// <param name="validLines">List of valid lines.</param>
        /// <param name="size">Size.</param>
        /// <param name="value1">Value1.</param>
        /// <param name="value2">Value2.</param>
        /// <param name="startRow">Start row.</param>
        /// <param name="grid">Grid.</param>
        /// <param name="listOfGrids">List of grids.</param>
        /// <param name="maxTrialsPerAttempt">Max trials per attempt.</param>
        /// <param name="excludedList">Excluded list.</param>
        /// 

        public static bool FormOneGrid(List<int[]> validLines, int[] shuffleIndices, int currentShuffleIndex, int size, int value1, int value2, int startRow, int[][] grid, List<string> listOfGrids, long maxTrialsPerAttempt, IEnumerable<string> excludedList, ref GenerationInfo info)
        {
            if (info.shouldStop)
                return false;

            bool finish = false;
            if (startRow == size)
            {
                bool failed = false;
                if (!failed)
                {
                    failed = !Validator.UniqueRowCheck(grid);
                    if (failed)
                        UnityEngine.Debug.LogWarning("OUCH");
                }

                if (!failed)
                {
                    failed = !Validator.UniqueColumnCheck<int>(grid);
                    if (failed)
                        onFailed("unique");
                }
                // Grid was successfully constructed!
                // But we still need to check if the current grid replicates another one added to the list previously.
                // And we also need to make sure that it is not in the excluded list either!
                if (!failed)
                {
                    string gridStr = Helper.PuzzleIntGridToString(grid);    // convert the int grid to a string for comparison.

                    foreach (string g in listOfGrids)
                    {
                        failed = gridStr.Equals(g);
                        if (failed)
                        {
                            onFailed("replicate");
                            break;  // replicate a former grid
                        }
                    }

                    if (!failed)
                    {
                        foreach (string eg in excludedList)
                        {
                            failed = gridStr.Equals(eg);
                            if (failed)
                            {
                                onFailed("exclude");
                                break;  // is in excluded list
                            }
                        }
                    }


                    if (!failed)
                    {
                        Helper.LogSuccessFormat("New grid created!");
                        listOfGrids.Add(gridStr);
                        finish = true;  // Job done!
                                        //if (analyzeMode)
                                        //    Helper.LogFormat("Generated grid number: {0:D}", listOfGrids.Count);

                    }
                }

                //UnityEngine.Debug.Log("----------------------------");
            }
            else
            {
                // Add new row the grid.
                // First create a shuffle list of validLines indices.


                for (int i = currentShuffleIndex; i < shuffleIndices.Length; ++i)
                {
                    // Stop if maximum allowed number of trials has been reached.
                    if (totalTrials == maxTrialsPerAttempt)
                    {
                        finish = true;
                        break;
                    }

                    // Otherwise continue working.
                    int index = shuffleIndices[i];

                    if (selectedIndex.Contains(index))
                        continue;

                    //debugIndex.Add(index);
                    //string s = "";
                    //for (int k = 0; k < debugIndex.Count; ++k)
                    //{
                    //    s += debugIndex[k] + " ";
                    //}

                    //UnityEngine.Debug.Log("<color=blue>" + s + "</color>");
                    // Assign the current startRow with a random line from validLines list.
                    // Note that we don't need to create a copy of grid to work on since we
                    // only check for the part contains row [0] to row [startRow] so those rows
                    // from [startRow+1] onward (added in previous steps) don't matter.
                    //int[] nextLineToAppend = validLines[index];
                    //bool isAppended = false;
                    //for (int j = 0; j < startRow; ++j)
                    //{
                    //    if (Helper.ArraySequenceEquals<int>(grid[j], nextLineToAppend))
                    //    {
                    //        isAppended = true;
                    //        break;
                    //    }
                    //}
                    //if (!isAppended)
                    //{
                    grid[startRow] = validLines[index];
                    //}
                    // Count the number of totalTrials for analysis purpose.
                    ++totalTrials;

                    // Display progress.
                    //if (maxTrialsPerAttempt <= 0)
                    //{
                    //    if (analyzeMode)
                    //        Helper.LogFormat((totalTrials % 2 == 0) ? ".   \r" : "..  \r"); // spaces to clear garbage from previous print command.
                    //}
                    //else
                    //{
                    //    if (analyzeMode)
                    //        Helper.LogFormat("Current attempt progress: {0:P}  \r", (float)totalTrials / maxTrialsPerAttempt);
                    //}

                    // Check if the so-far grid statisfies 2 game rules.
                    // Only need to check columns since rows already valid.
                    bool satisfied = true;

                    if (startRow >= 2)
                    {
                        for (int col = 0; col < size; ++col)
                        {
                            // Optimization stategy: perform early error-detecting by breaking 
                            // validating process into small steps. If any step fails, we'll end 
                            // the process immediately with a fail test result without spending more
                            // time testing the current row.

                            // 1st step: detect 3-in-a-row triplet. Triplet consists of the last three cells
                            // of the column that contains the cell belonging to the newly added row.
                            int first = grid[startRow - 2][col];
                            int middle = grid[startRow - 1][col];
                            int last = grid[startRow][col];

                            if ((first == middle) && (middle == last))
                            {
                                satisfied = false; // 3-in-a-row detected.
                                onFailed("triple");
                                break;
                            }

                            // 2nd step: check if any value exceeds its maximum allowed occurrences (size/2).
                            // Effectively that is checking if num1 or num2 > size/2.
                            // This will also make sure that num1=num2=size/2 when the grid is fully filled.
                            int zeroCount = 0, oneCount = 0;
                            for (int r = 0; r <= startRow; ++r)
                            {
                                int value = grid[r][col];
                                if (value == 0)
                                    zeroCount += 1;
                                else if (value == 1)
                                    oneCount += 1;
                            }

                            if (zeroCount > size * 0.5f || oneCount > size * 0.5f)
                            {
                                onFailed("equal");
                                satisfied = false;
                                break;
                            }
                        }
                    }

                    // If all columns satisfy the 2 rules, continue adding the next row.
                    // It not, repeat for loop with the next line from validLines.
                    if (satisfied)
                    {
                        //if (analyzeMode)
                        //{
                        //    if (startRow > currentFinishedRow)
                        //    {
                        //        currentFinishedRow = startRow;
                        //        Helper.LogFormat("Finished row number " + currentFinishedRow);
                        //    }
                        //    else
                        //    {
                        //        Helper.LogFormat("Replaced row number " + startRow);
                        //    }
                        //}

                        selectedIndex.Add(index);
                        finish = FormOneGrid(validLines, shuffleIndices, currentShuffleIndex + 1, size, value1, value2, startRow + 1, grid, listOfGrids, maxTrialsPerAttempt, excludedList, ref info);
                        if (finish)
                            break;    // stop the process when finished.
                    }
                    else
                    {
                        ++failedTrials;    // count the number of failed trials for analysis.
                        //debugIndex.RemoveAt(debugIndex.Count - 1);
                    }
                }
            }

            return finish;
        }

        /// <summary>
        /// Constructs valid lines that have equal number of value1 and value2,
        /// and have no 3-in-a-row triplet.
        /// </summary>
        /// <returns>The constructed valid lines.</returns>
        /// <param name="lineIndices">Line indices.</param>
        /// <param name="value1">Value1.</param>
        /// <param name="value2">Value2.</param>
        public static List<int[]> FormValidLines(int[] lineIndices, int value1, int value2)
        {
            int size = lineIndices.Length;
            int halfSize = (int)(size / 2);

            // Create a list to store all the lines with equal number of value1 and value2.
            List<int[]> equalizedLines = new List<int[]>();

            // First find all possible combinations of halfSize indices where we can fill
            // with one value (either value1 or value2), that means we fill half the line
            // with one value and only need to fill the other half with the other value 
            // and we can form a full line with equal number of value1 and value2.
            List<int[]> combinations = Helper.GetCombinations(lineIndices, halfSize);

            // Now construct lines with equal number of value1 and value2.
            foreach (int[] indices in combinations)
            {
                int[] newLine = new int[size];

                for (int i = 0; i < newLine.Length; i++)
                {
                    // Fill all the line positions with index presented in the indices combination
                    // with values1, and other indices with value2.
                    if (Helper.ArrayContains(indices, i))
                    {
                        newLine[i] = value1;
                    }
                    else
                    {
                        newLine[i] = value2;
                    }
                }
                equalizedLines.Add(newLine);
            }

            // Finally check each line for 3-in-a-row rule (since equality rule already satisfied),
            // then form a list of all satisfied lines.
            List<int[]> validLines = new List<int[]>(); // list contains valid lines (pass the test)

            foreach (int[] line in equalizedLines)
            {
                bool satisfied = Validator.TripletRuleCheck(line, value1, value2);

                if (satisfied)
                {
                    validLines.Add(line);
                }
            }

            // Statistics.
            int validLinesNum = validLines.Count;
            int linePossibilities = (int)(Math.Pow(2, size));
            double oneLineValidPercent = (double)validLinesNum / linePossibilities;
            double allLinesValidPercent = Math.Pow(oneLineValidPercent, size);
            double numOfPuzzleWithValidRows = Math.Pow(validLinesNum, size);
            double numOfValidPuzzle = (numOfPuzzleWithValidRows * allLinesValidPercent);

            if (analyzeMode)
            {
                Helper.LogFormat("===========================================================");
                Helper.LogFormat("Filler statistics:");
                Helper.LogFormat("Line size:\t\t\t{0}", size);
                Helper.LogFormat("Possible lines:\t\t\t{0}", linePossibilities);
                Helper.LogFormat("Equalized lines:\t\t{0}", equalizedLines.Count);
                Helper.LogFormat("Valid lines:\t\t\t{0}", validLinesNum);
                Helper.LogFormat("oneLineValidPercent:\t\t{0:}", oneLineValidPercent);
                Helper.LogFormat("allLinesValidPercent:\t\t{0}", allLinesValidPercent);
                Helper.LogFormat("numOfPuzzleWithValidRows:\t{0}", numOfPuzzleWithValidRows);
                Helper.LogFormat("numOfValidPuzzle:\t\t{0}", numOfValidPuzzle);
                Helper.LogFormat("===========================================================");
            }

            return validLines;
        }
    }
}

