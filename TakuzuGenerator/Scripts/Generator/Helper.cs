/// <summary>
/// Helper class.
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Takuzu.Generator
{
    public static class Helper
    {
        /// <summary>
        /// Returns the cell indices of all the triplets containing the target cell specified by row and col.
        /// </summary>
        /// <returns>The triplets of cell.</returns>
        /// <param name="gridSize">Grid size.</param>
        /// <param name="cellRow">Cell row.</param>
        /// <param name="cellCol">Cell col.</param>
        public static List<int[][]> GetTripletsIndices(int gridSize, int row, int col)
        {
            List<int[][]> triplets = new List<int[][]>();

            // Simple way to find cell's triplets is to iterate through all the triplets
            // within the cell's row and column and detect those that contains the target cell
            // (by checking the indices of the cells).
            for (int i = 0; i <= gridSize - 3; i++)
            {
                // Get the indices of 3 cells in each triplet.
                int first = i;
                int middle = i + 1;
                int last = i + 2;

                if ((col == first) || (col == middle) || (col == last))
                {
                    int[][] rowTriplet = new int[][]
                    {
                        new int[] { row, first },
                        new int[] { row, middle },
                        new int[] { row, last }
                    };

                    triplets.Add(rowTriplet);
                }

                if ((row == first) || (row == middle) || (row == last))
                {
                    int[][] colTriplet = new int[][]
                    {
                        new int[] { first, col },
                        new int[] { middle, col },
                        new int[] { last, col }
                    };

                    triplets.Add(colTriplet);
                }
            }

            return triplets;
        }

        /// <summary>
        /// Returns the cell value (as opposed to indices) of all the triplets containing the target cell specified by row and col.
        /// </summary>
        /// <returns>The triplets value.</returns>
        /// <param name="grid">Grid.</param>
        /// <param name="row">Row.</param>
        /// <param name="col">Col.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static List<T[]> GetTripletsValue<T>(T[][] grid, int row, int col)
        {
            List<T[]> triplets = new List<T[]>();

            // Get the row and column crossing at the target cell.
            T[] rowArray = GetRow(grid, row);
            T[] colArray = GetColumn(grid, col);

            // Simple way to find cell's triplets is to iterate through all the triplets
            // within the cell's row and column and detect those that contains the target cell
            // (by checking the indices of the cells).
            for (int c = 0; c <= rowArray.Length - 3; c++)
            {
                // Get the indices of 3 cells in the triplet.
                int first = c;
                int middle = c + 1;
                int last = c + 2;

                if ((col == first) || (col == middle) || (col == last))
                {
                    T[] triplet = { rowArray[first], rowArray[middle], rowArray[last] };
                    triplets.Add(triplet);
                }
            }

            for (int r = 0; r <= colArray.Length - 3; r++)
            {
                int first = r;
                int middle = r + 1;
                int last = r + 2;

                if ((row == first) || (row == middle) || (row == last))
                {
                    T[] triplet = { colArray[first], colArray[middle], colArray[last] };
                    triplets.Add(triplet);
                }
            }
            return triplets;
        }

        /// <summary>
        /// Returns the value array of the specified row of the given grid.
        /// </summary>
        /// <returns>The row.</returns>
        /// <param name="grid">Grid.</param>
        /// <param name="row">Row.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T[] GetRow<T>(T[][] grid, int row)
        {
            int size = grid.Length;
            T[] array = new T[size];

            for (int col = 0; col < size; col++)
            {
                array[col] = grid[row][col];
            }

            return array;
        }

        /// <summary>
        /// Returns the value array of the specified column of the given grid.
        /// </summary>
        /// <returns>The column.</returns>
        /// <param name="grid">Grid.</param>
        /// <param name="col">Col.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T[] GetColumn<T>(T[][] grid, int col)
        {
            int size = grid.Length;
            T[] array = new T[size];

            for (int row = 0; row < size; row++)
            {
                array[row] = grid[row][col];
            }

            return array;
        }

        /// <summary>
        /// Parses string content and returns a new grid (2D array) ot int values, each
        /// grid element is the numeric value of corresponding character of the input string.
        /// </summary>
        /// <returns>The string to int grid.</returns>
        /// <param name="str">String.</param>
        public static int[][] PuzzleStringToIntGrid(string str)
        {
            double pSize = Math.Sqrt(str.Length);
            bool valid = IsValidPuzzleSize(pSize);

            if (!valid)
            {
                throw new ArgumentException();
            }

            int size = (int)pSize;
            int[][] array = new int[size][];

            for (int i = 0; i < size; i++)
            {
                array[i] = new int[size];
                for (int j = 0; j < size; j++)
                {
                    array[i][j] = (int)Char.GetNumericValue(str[i * size + j]);
                }
            }

            return array;
        }

        /// <summary>
        /// Parses string content and returns a new grid (2D array) of strings, each
        /// grid element is a string of corresponding character of the input string.
        /// </summary>
        /// <returns>The string to string grid.</returns>
        /// <param name="str">String.</param>
        public static string[][] PuzzleStringToStringGrid(string str)
        {
            double pSize = Math.Sqrt(str.Length);
            bool valid = IsValidPuzzleSize(pSize);

            if (!valid)
            {
                throw new ArgumentException();
            }

            int size = (int)pSize;
            string[][] array = new string[size][];

            for (int i = 0; i < size; i++)
            {
                array[i] = new string[size];
                for (int j = 0; j < size; j++)
                {
                    array[i][j] = str[i * size + j].ToString();
                }
            }

            return array;
        }

        /// <summary>
        /// Converts a sring grid (2D array of strings) to a string.
        /// </summary>
        /// <returns>The string grid to string.</returns>
        /// <param name="grid">Grid.</param>
        public static string PuzzleStringGridToString(string[][] grid)
        {
            int len = grid.Length * grid.Length;
            StringBuilder sb = new StringBuilder(len);

            for (int row = 0; row < grid.Length; row++)
            {
                for (int col = 0; col < grid[row].Length; col++)
                {
                    sb.Append(grid[row][col]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a int grid (2D array of ints)to a String.
        /// </summary>
        /// <returns>The int grid to string.</returns>
        /// <param name="grid">Grid.</param>
        public static string PuzzleIntGridToString(int[][] grid)
        {
            int len = grid.Length * grid.Length;
            StringBuilder sb = new StringBuilder(len);

            for (int row = 0; row < grid.Length; row++)
            {
                for (int col = 0; col < grid[row].Length; col++)
                {
                    sb.Append(grid[row][col].ToString());
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts int[][] grid to string[][] grid. 
        /// </summary>
        /// <returns>The grid to string grid.</returns>
        /// <param name="grid">Grid.</param>
        public static string[][] IntGridToStringGrid(int[][] grid)
        {
            string[][] result = new string[grid.Length][];

            for (int row = 0; row < grid.Length; row++)
            {
                result[row] = new string[grid[row].Length];
                for (int col = 0; col < grid[row].Length; col++)
                {
                    result[row][col] = grid[row][col].ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if the given size satisfies all size requirements of this game.
        /// Returns true if satisfies, false otherwise.
        /// </summary>
        /// <returns>The valid puzzle size.</returns>
        /// <param name="size">Size.</param>
        public static bool IsValidPuzzleSize(double size)
        {
            bool valid = true;

            // For this kind of puzzle, puzzle size must be a mathematical integer and even number.
            // Moreover, puzzle size must be at least 4 (4x4 grid) for the puzzle to be meaningful.
            if (size != Math.Ceiling(size))
            {
                Helper.LogFormat("Invalid puzzle string: non-integer puzzle size calculated.");
                valid = false;
            }
            else if ((size % 2) != 0)
            {
                Helper.LogFormat("Invalid puzzle string: puzzle size is odd.");
                valid = false;
            }
            else if (size < 4)
            {
                Helper.LogFormat("Invalid puzzle string: puzzle size smaller than 4x4.");
                valid = false;
            }

            return valid;
        }

        /// <summary>
        /// Prints the grid content to the console.
        /// </summary>
        /// <param name="grid">Grid.</param>
        public static void PrintGrid(string[][] grid)
        {
            for (int row = 0; row < grid.Length; row++)
            {
                string rowStr = "|";
                for (int col = 0; col < grid[row].Length; col++)
                {
                    string val = grid[row][col];
                    val = (val.Equals(Puzzle.DOT)) ? Puzzle.X : val;    // Replace dot with x for better visibility.
                    val = (val.Length > 1) ? val : (val + "");
                    rowStr += " " + val + " |";
                }
                Helper.LogFormat(rowStr);

                string line = "";
                for (int i = 0; i < rowStr.Length; i++)
                {
                    line += "-";
                }
                Helper.LogFormat(line);
            }
        }

        /// <summary>
        /// Prints the grid content to the console.
        /// </summary>
        /// <param name="grid">Grid.</param>
        public static void PrintGrid(int[][] grid)
        {
            for (int row = 0; row < grid.Length; row++)
            {
                string rowStr = "|";
                for (int col = 0; col < grid[row].Length; col++)
                {
                    rowStr += "  " + grid[row][col] + "  |";
                }
                Helper.LogFormat(rowStr);

                string line = "";
                for (int i = 0; i < rowStr.Length; i++)
                {
                    line += "_";
                }
                Helper.LogFormat(line);
            }
        }

        /// <summary>
        /// Creates a shuffled list containing values from 0 to size-1.
        /// </summary>
        /// <returns>The shuffle list.</returns>
        /// <param name="size">Size.</param>
        public static int[] GenerateShuffledList(int size)
        {
            Random rand = new Random();
            int[] shuffle = new int[size];

            // Initialize the list.
            for (int i = 0; i < shuffle.Length; ++i)
            {
                shuffle[i] = i;
            }

            // Now shuffle list.
            for (int j = shuffle.Length - 1; j > 0; --j)
            {
                // Get a random index before j
                int k = rand.Next(j);

                // Swap the values
                int temp = shuffle[j];
                shuffle[j] = shuffle[k];
                shuffle[k] = temp;
            }

            return shuffle;
        }

        /// <summary>
        /// Creates a deep copy of a 2D jagged array.
        /// </summary>
        /// <returns>The copy jagged array.</returns>
        /// <param name="jaggedArray">Jagged array.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T[][] DeepCopyJaggedArray<T>(T[][] jaggedArray)
        {
            int row = jaggedArray.Length;
            T[][] copy = new T[row][];

            for (int i = 0; i < row; ++i)
            {
                int col = jaggedArray[i].Length;
                copy[i] = new T[col];

                for (int j = 0; j < col; ++j)
                {
                    copy[i][j] = jaggedArray[i][j];
                }
            }

            return copy;
        }

        /// <summary>
        /// Checks if the given array contains an int value.
        /// Returns true if it contains, false otherwise.
        /// </summary>
        /// <returns><c>true</c>, if contains was arrayed, <c>false</c> otherwise.</returns>
        /// <param name="array">Array.</param>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool ArrayContains<T>(T[] array, T element)
        {
            return Array.IndexOf(array, element) >= 0;
        }

        public static bool ArraySequenceEquals<T>(T[] lhs, T[] rhs)
        {
            if (lhs.Length != rhs.Length)
                return false;

            int length = lhs.Length;
            for (int i = 0; i < length; ++i)
            {
                if (!lhs[i].Equals(rhs[i]))
                    return false;
            }
            return true;
        }

        public static bool ArraySequenceEquals<T>(T[] lhs, T[] rhs, T exceptionElement)
        {
            if (lhs.Length != rhs.Length)
                return false;
            if (ArrayContains<T>(lhs, exceptionElement) || ArrayContains<T>(rhs, exceptionElement))
                return false;
            int length = lhs.Length;
            for (int i = 0; i < length; ++i)
            {
                if (!lhs[i].Equals(rhs[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Converts milliseconds to hour, min, sec format.
        /// Returns a string of the formatted time.
        /// </summary>
        /// <returns>The time.</returns>
        /// <param name="millis">Millis.</param>
        public static string FormatTime(TimeSpan timeSpan)
        {
            string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                timeSpan.Hours,
                                timeSpan.Minutes,
                                timeSpan.Seconds,
                                timeSpan.Milliseconds);

            return answer;
        }

        /// <summary>
        /// Returns all combinations (sub-arrays) with the specified length from the parent array.
        /// </summary>
        /// <returns>The combinations.</returns>
        /// <param name="inputArray">Input array.</param>
        /// <param name="len">Length.</param>
        public static List<T[]> GetCombinations<T>(T[] parentArray, int length)
        {
            List<T[]> combinationList = new List<T[]>();
            T[] combination = new T[length];
            DoGetCombinations(parentArray, length, 0, combination, combinationList);

            return combinationList;
        }

        /// <summary>
        /// Does the actual combinations searching job.
        /// </summary>
        /// <param name="inputArray">Input array.</param>
        /// <param name="length">Length.</param>
        /// <param name="startPosition">Start position.</param>
        /// <param name="subset">Subset.</param>
        /// <param name="list">List.</param>
        private static void DoGetCombinations<T>(T[] parentArray, int length, int startPosition, T[] subset, List<T[]> list)
        {
            if (length == 0)
            {
                // Subset is completed!
                T[] copy = new T[subset.Length];
                Array.Copy(subset, copy, subset.Length);
                list.Add(copy); // note that we need to create a copy since subset is to change in the next iteration.
                return;
            }

            for (int i = startPosition; i <= parentArray.Length - length; i++)
            {
                subset[subset.Length - length] = parentArray[i];
                DoGetCombinations(parentArray, length - 1, i + 1, subset, list);
            }
        }

        public static Action<string> onLog = delegate { };
        public static void LogFormat(string log, params object[] arg)
        {
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor ||
                UnityEngine.Application.platform == UnityEngine.RuntimePlatform.OSXEditor)
            {
                for (int i = 0; i < arg.Length; ++i)
                {
                    log = log.Replace("{" + i + "}", arg[i].ToString());
                }
                onLog(log);
            }
            else
            {
                Console.WriteLine(log, arg);
            }
        }

        public static void LogErrorFormat(string error, params object[] arg)
        {
            string log;
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor ||
                UnityEngine.Application.platform == UnityEngine.RuntimePlatform.OSXEditor)
            {
                log = "<b><color=red>" + error + "</color></b>";
            }
            else
            {
                log = error;
            }
            LogFormat(log, arg);
        }

        public static void LogSuccessFormat(string msg, params object[] arg)
        {
            string log;
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor ||
                UnityEngine.Application.platform == UnityEngine.RuntimePlatform.OSXEditor)
            {
                log = "<b><color=green>" + msg + "</color></b>";
            }
            else
            {
                log = msg;
            }
            LogFormat(log, arg);
        }

        public static void LogInfoFormat(string msg, params object[] arg)
        {
            string log;
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor ||
                UnityEngine.Application.platform == UnityEngine.RuntimePlatform.OSXEditor)
            {
                log = "<b><color=blue>" + msg + "</color></b>";
            }
            else
            {
                log = msg;
            }
            LogFormat(log, arg);
        }

        public static bool IsPuzzleStringOfSize(this string str, Size s)
        {
            int sizeInt = (int)s;
            bool validStringLength = str.Length / sizeInt == sizeInt && str.Length % sizeInt == 0;
            if (!validStringLength)
                return false;
            for (int i = 0; i < str.Length; ++i)
            {
                bool validValue = str[i].ToString().Equals(Puzzle.VALUE_ONE) || str[i].ToString().Equals(Puzzle.VALUE_ZERO) || str[i].ToString().Equals(Puzzle.DOT);
                if (!validValue)
                    return false;
            }
            return true;
        }

        public static bool IsSolutionStringOfSize(this string str, Size s)
        {
            int sizeInt = (int)s;
            bool validStringLength = str.Length / sizeInt == sizeInt && str.Length % sizeInt == 0;
            if (!validStringLength)
                return false;
            for (int i = 0; i < str.Length; ++i)
            {
                bool validValue = str[i].ToString().Equals(Puzzle.VALUE_ONE) || str[i].ToString().Equals(Puzzle.VALUE_ZERO);
                if (!validValue)
                    return false;
            }
            return true;
        }

        public static string ListElementToString<T>(this IEnumerable<T> list, string separator = ",")
        {
            string s = "";
            IEnumerator<T> i = list.GetEnumerator();

            if (i.MoveNext())
            {
                s += i.Current.ToString();
            }
            while (i.MoveNext())
            {
                s += separator;
                s += i.Current.ToString();
            }
            return s;
        }
    }
}

