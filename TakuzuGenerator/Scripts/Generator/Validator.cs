/// <summary>
/// Puzzle validator class provides methods to evaluate the validity of a puzzle.
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;

namespace Takuzu.Generator
{
    public static class Validator
    {

        /// <summary>
        /// Checks if a grid (2d jagged array) satisfies two game rules: no 3-in-a-row and equal numbers of two values.
        /// </summary>
        /// <returns><c>true</c>, if satisfied, <c>false</c> otherwise.</returns>
        /// <param name="grid">Grid.</param>
        /// <param name="value1">Value1.</param>
        /// <param name="value2">Value2.</param>
        public static bool ValidateGrid<T>(T[][] grid, T value1, T value2)
        {
            int size = grid.Length;

            // Iterate through all cells on the diagonal of the grid, get all the 
            // rows and columns that cross at each cell and validate them.
            for (int i = 0; i < size; i++)
            {
                // Check row i.
                T[] row = Helper.GetRow(grid, i);
                bool rowCheck = ValidateLine(row, value1, value2);

                if (!rowCheck)
                {
                    return false;
                }

                // Check column i.
                T[] col = Helper.GetColumn(grid, i);
                bool colCheck = ValidateLine(col, value1, value2);

                if (!colCheck)
                {
                    return false;
                }
            }
            if (!UniqueRowCheck<T>(grid))
                return false;
            if (!UniqueColumnCheck<T>(grid))
                return false;
            // If we reach here everything was fine.
            return true;
        }

        public static bool UniqueRowCheck<T>(T[][] inputArray)
        {
            int length = inputArray.Length;
            for (int i = 0; i < length - 1; ++i)
            {
                for (int j = i + 1; j < length; ++j)
                {
                    T[] arr1 = Helper.GetRow<T>(inputArray, i);
                    T[] arr2 = Helper.GetRow<T>(inputArray, j);
                    if (Helper.ArraySequenceEquals<T>(arr1, arr2))
                        return false;
                }
            }
            return true;
        }

        public static bool UniqueColumnCheck<T>(T[][] inputArray)
        {
            int length = inputArray.Length;
            for (int i = 0; i < length - 1; ++i)
            {
                for (int j = i + 1; j < length; ++j)
                {
                    T[] arr1 = Helper.GetColumn<T>(inputArray, i);
                    T[] arr2 = Helper.GetColumn<T>(inputArray, j);
                    if (Helper.ArraySequenceEquals<T>(arr1, arr2))
                        return false;
                }
            }
            return true;
        }

        public static bool UniqueColumnCheck<T>(T[][] inputArray, T exception)
        {
            int length = inputArray.Length;
            for (int i = 0; i < length - 1; ++i)
            {
                for (int j = i + 1; j < length; ++j)
                {
                    T[] arr1 = Helper.GetColumn<T>(inputArray, i);
                    T[] arr2 = Helper.GetColumn<T>(inputArray, j);
                    if (Helper.ArraySequenceEquals<T>(arr1, arr2, exception))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if an array satisfies two conditions: no 3-in-a-row triplet and equal occurrences of two values.
        /// </summary>
        /// <returns><c>true</c>, if satisfied, <c>false</c> otherwise.</returns>
        /// <param name="inputArray">Input array.</param>
        /// <param name="value1">Value1.</param>
        /// <param name="value2">Value2.</param>
        public static bool ValidateLine<T>(T[] inputArray, T value1, T value2)
        {
            bool cond1 = TripletRuleCheck(inputArray, value1, value2);
            bool cond2 = EqualityRuleCheck(inputArray, value1, value2);

            return cond1 && cond2;
        }

        /// <summary>
        /// Checks if the array contains a 3-in-a-row triplet of the same value.
        /// </summary>
        /// <returns><c>true</c>, if contains, <c>false</c> otherwise.</returns>
        /// <param name="inputArray">Input array.</param>
        /// <param name="value1">Value1.</param>
        /// <param name="value2">Value2.</param>
        public static bool TripletRuleCheck<T>(T[] inputArray, T value1, T value2)
        {
            if (inputArray.Length >= 3)
            {
                for (int i = 0; i <= inputArray.Length - 3; i++)
                {
                    T first = inputArray[i];
                    T middle = inputArray[i + 1];
                    T last = inputArray[i + 2];

                    if ((first.Equals(value1) || first.Equals(value2)) && (first.Equals(middle) && first.Equals(last)))
                    {
                        return false; // found 3-in-a-row cells of the same value.
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if a row or column (technically an array) has the same occurrences 
        /// of each value and this number of occurrences equals half the array length.
        /// </summary>
        /// <returns>true if satisfies, false otherwise.</returns>
        /// <param name="inputArray">Input array.</param>
        /// <param name="value1">Value1.</param>
        /// <param name="value2">Value2.</param>
        public static bool EqualityRuleCheck<T>(T[] inputArray, T value1, T value2)
        {
            int num1 = 0;
            int num2 = 0;

            foreach (T value in inputArray)
            {
                if (value.Equals(value1))
                    num1++;
                else if (value.Equals(value2))
                    num2++;
            }

            return (num1 == num2 && num2 > 0 && num2 == (int)(inputArray.Length / 2));
        }

        /// <summary>
        /// Counts the occurrences of the given value in the given array.
        /// </summary>
        /// <returns>The occurrences.</returns>
        /// <param name="inputArray">Input array.</param>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static int CountOccurrences<T>(T[] inputArray, T value)
        {
            int num = 0;

            foreach (T val in inputArray)
            {
                if (val.Equals(value))
                    num++;
            }

            return num;
        }

        /// <summary>
        /// Returns the number of cells in the given grid (jagged array)
        /// that has a value equal to value1 or value2 (which means it's a given cell)
        /// </summary>
        /// <returns>number of given cells.</returns>
        /// <param name="grid">Grid.</param>
        /// <param name="value1">Value1.</param>
        /// <param name="value2">Value2.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static int CountKnownCells<T>(T[][] grid, T value1, T value2)
        {
            int num = 0;

            for (int row = 0; row < grid.Length; row++)
            {
                for (int col = 0; col < grid[row].Length; col++)
                {
                    T val = grid[row][col];
                    if ((val.Equals(value1)) || (val.Equals(value2)))
                        num++;
                }
            }

            return num;
        }

        //        /*
        //     *  Checks if the given array list contains same elements.
        //     *  Used for detecting repeated results in result list.
        //     */
        //        public static bool repeatIntArraysCheck(ArrayList<int[][]> arrayList) {
        //            bool repeat = false;
        //
        //            System.out.println("Checking for repeat elements in array list...");
        //
        //            for (int i=(arrayList.size()-1); i>0; i--) {
        //                for (int j=i-1; j>=0; j--) {
        //                    int[][] array1 = arrayList.get(i);
        //                    int[][] array2 = arrayList.get(j);
        //
        //                    bool same = Arrays.deepEquals(array1, array2);
        //
        //                    if (same) {
        //                        repeat = true;
        //                        System.out.printf("Repeat detected between element %d and element %d.\n", i, j);
        //                        break;
        //                    }
        //                }
        //
        //                if (repeat) break;
        //            }
        //
        //            String result = (repeat)? "Fail" : "Pass";
        //            System.out.println("Repeat elements checking done. Result: " + result);
        //
        //            return repeat;
        //        }

        //        public static bool repeatStringArraysCheck(List<string[][]> list)
        //        {
        //            bool repeat = false;
        //
        //            Helper.LogFormat("Checking for duplicated arrays in list...");
        //
        //            for (int i = list.Count - 1; i > 0; i--)
        //            {
        //                for (int j = i - 1; j >= 0; j--)
        //                {
        //                    string[][] array1 = list[i];
        //                    string[][] array2 = list[j];
        //
        //                    bool same = Arrays.deepEquals(array1, array2);
        //
        //                    if (same)
        //                    {
        //                        repeat = true;
        //                        Helper.LogFormat("Duplicating detected between array {0:D} and array {1:D}.", i, j);
        //                        break;
        //                    }
        //                }
        //
        //                if (repeat)
        //                    break;
        //            }
        //
        //            String result = (repeat) ? "Fail" : "Pass";
        //            Helper.LogFormat("Repeat elements checking done. Result: " + result);
        //
        //            return repeat;
        //        }

        /// <summary>
        /// Check if the given string list contains duplicated strings.
        /// </summary>
        /// <returns><c>true</c>, if string check was repeated, <c>false</c> otherwise.</returns>
        /// <param name="stringList">String list.</param>
        public static bool DuplicatedStringCheck(List<string> stringList)
        {
            Helper.LogFormat("Checking for duplicated strings in stringList...");

            for (int i = stringList.Count - 1; i > 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    string str1 = stringList[i];
                    string str2 = stringList[j];

                    if (String.Equals(str1, str2))
                    {
                        Helper.LogFormat("Duplicating detected between string {0:D} and string {1:D}.", i, j);
                        return true;
                    }
                }
            }

            Helper.LogFormat("Duplicated strings checking done: no duplicate found.");
            return false;
        }

        /// <summary>
        /// Check if the given puzzle list contains duplicated puzzles
        /// </summary>
        /// <returns><c>true</c>if there're duplicated puzzles, <c>false</c> otherwise.</returns>
        /// <param name="puzzleList">Puzzle list.</param>
        public static bool DuplicatedPuzzleCheck(List<Puzzle> puzzleList)
        {
            Helper.LogFormat("Checking for duplicated puzzles in puzzleList...");

            for (int i = puzzleList.Count - 1; i > 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    string str1 = puzzleList[i].puzzle;
                    string str2 = puzzleList[j].puzzle;

                    if (String.Equals(str1, str2))
                    {
                        Helper.LogFormat("Duplicating detected between puzzle {0:D} and puzzle {1:D}.", i, j);
                        return true;
                    }
                }
            }

            Helper.LogFormat("Duplicated puzzles checking done: no duplicate found.");
            return false;
        }
    }
}

