using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;


namespace Takuzu.Generator
{
    public static class Maker
    {
        public static void SavePuzzle(string db, Puzzle p)
        {
            SavePuzzles(db, new Puzzle[1] { p });
        }

        public static void SavePuzzles(string db, ICollection<Puzzle> container)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            string commandText = string.Empty;
            string valueText = string.Empty;
            int affectedRow = 0;
            try
            {
                commandText = "INSERT INTO PUZZLE VALUES ";

                int i = 0;
                foreach (Puzzle p in container)
                {
                    valueText = string.Format(
                        "({0},{1},'{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}')",
                        "NULL", "NULL", p.puzzle, p.solution, (int)p.size, p.givenNum, p.parseNum, p.parsePercent, p.lsdNum, p.lsdPercent, p.alsdNum, p.alsdPercent, (int)p.level);
                    commandText += valueText;
                    if (i < container.Count - 1)
                    {
                        commandText += ",";
                        i += 1;
                    }
                    else
                    {
                        commandText += ";";
                    }
                }

                connection = Data.ConnectToDatabase(db);
                command = Data.CreateCommand(connection, commandText);
                affectedRow = command.ExecuteNonQuery();
                Helper.LogSuccessFormat("Adding {0} puzzles to database {1}.", affectedRow, db);

            }
            catch (System.Exception e)
            {
                Helper.LogErrorFormat(e.ToString());
            }
            Data.Flush(connection, command);

        }
    }
}