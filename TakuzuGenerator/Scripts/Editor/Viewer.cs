using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;

namespace Takuzu.Generator
{
    public static class Viewer
    {
        public static void GetAllPuzzle(string db, ICollection<int> idContainer, ICollection<Puzzle> puzzleContainer, Size size, Level[] difficulty, string pack, bool sortByLevel)
        {
            if (difficulty.Length == 0)
                return;

            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            string commandText = string.Empty;

            try
            {
                commandText = "SELECT * FROM PUZZLE WHERE ";
                commandText += string.Format("SIZE = '{0}'", (int)size);

                string levelSet = "";
                for (int i = 0; i < difficulty.Length; ++i)
                {
                    levelSet += ((int)difficulty[i]).ToString();
                    if (i < difficulty.Length - 1)
                    {
                        levelSet += ", ";
                    }
                }
                levelSet = string.Format("({0})", levelSet);
                commandText += string.Format(" AND LEVEL IN {0}", levelSet);

                if (!pack.Equals(""))
                {
                    commandText += string.Format(" AND PACK = '{0}'", pack);
                }

                if (sortByLevel)
                {
                    commandText += " ORDER BY LEVEL";
                }

                connection = Data.ConnectToDatabase(db);
                command = Data.CreateCommand(connection, commandText);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    try
                    {
                        int id = reader.GetInt32(0);
                        idContainer.Add(id);
                        string puzzle = reader.GetString(2);
                        string solution = reader.GetString(3);
                        Level level = (Level)reader.GetInt32(12);
                        Puzzle p = new Puzzle(size, level, puzzle, solution, -1, -1, -1, -1);
                        puzzleContainer.Add(p);
                    }
                    catch (InvalidCastException)
                    {
                        continue;
                    }
                }
            }
            catch
            {

            }

            Data.Flush(connection, command, reader);
        }

        public static void DeletePuzzle(string db, int id)
        {
            Data.DeletePuzzleById(db, id);
        }
    }
}