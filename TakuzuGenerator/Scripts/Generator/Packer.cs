using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System;

namespace Takuzu.Generator
{
    public static class Packer
    {
        public static void GetExclusivePuzzle(ICollection<string> exclusiveDb, ICollection<string> container)
        {
            IEnumerator<string> i = exclusiveDb.GetEnumerator();
            while (i.MoveNext())
            {
                Data.GetAllPuzzleString(i.Current, container);
            }
        }

        public static void GetPuzzleByPackContent(string db, ICollection<Puzzle> container, Size size, Level level)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            string commandText = string.Empty;

            try
            {
                commandText = string.Format(
                    "SELECT * FROM PUZZLE WHERE SIZE = '{0}' AND LEVEL = '{1}'",
                    (int)size, (int)level);
                connection = Data.ConnectToDatabase(db);
                command = Data.CreateCommand(connection, commandText);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Size sz = (Size)reader.GetInt32(4);
                    Level l = (Level)reader.GetInt32(12);
                    string p = reader.GetString(2);
                    string s = reader.GetString(3);
                    int gn;
                    int pn;
                    int lsdn;
                    int alsdn;
                    try
                    {
                        gn = reader.GetInt32(5);
                        pn = reader.GetInt32(6);
                        lsdn = reader.GetInt32(8);
                        alsdn = reader.GetInt32(10);
                    }
                    catch
                    {
                        gn = 0;
                        pn = 0;
                        lsdn = 0;
                        alsdn = 0;
                    }
                    Puzzle puzzle = new Puzzle(sz, l, p, s, gn, pn, lsdn, alsdn);
                    container.Add(puzzle);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            Data.Flush(connection, command, reader);

        }

        public static void SavePuzzle(string db, ICollection<Puzzle> container, int count, string pack, bool stripStatistic = false)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            string commandText = string.Empty;
            string valueText = string.Empty;
            int actualCount = Mathf.Min(container.Count, count);
            try
            {
                commandText = "INSERT INTO PUZZLE VALUES ";
                int i = 0;
                foreach (var p in container)
                {
                    string puzzleStr = p.puzzle;
                    string solutionStr = p.solution;
                    if (!stripStatistic)
                    {
                        valueText = string.Format(
                            "({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}')",
                            "NULL", pack, puzzleStr, solutionStr, (int)p.size, p.givenNum, p.parseNum, p.parsePercent, p.lsdNum, p.lsdPercent, p.alsdNum, p.alsdPercent, (int)p.level);
                    }
                    else
                    {
                        valueText = string.Format(
                            "({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}')",
                            "NULL", pack, puzzleStr, solutionStr, (int)p.size, p.givenNum, null, null, null, null, null, null, (int)p.level);
                    }

                    commandText += valueText;
                    if (i < actualCount - 1)
                    {
                        commandText += ",";
                        i += 1;
                    }
                    else
                    {
                        commandText += ";";
                        break;
                    }
                }
                connection = Data.ConnectToDatabase(db);
                command = Data.CreateCommand(connection, commandText);
                command.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            Data.Flush(connection, command);
        }
    }
}
