using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Data;
using Mono.Data.Sqlite;

namespace Takuzu.Generator
{
    public static class Encryptor
    {
        public static void CloneDatabase(string srcDatabase, string desDatabase)
        {
            if (!File.Exists(srcDatabase))
            {
                throw new System.ArgumentException("Source database not exists");
            }
            byte[] data = File.ReadAllBytes(srcDatabase);
            File.WriteAllBytes(desDatabase, data);
        }

        public static void GetOriginPuzzle(string srcDatabase, ICollection<int> idContainer, ICollection<string> puzzleContainer, ICollection<string> solutionContainer)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            string commandText = string.Empty;
            try
            {
                commandText = string.Format("SELECT ID, PUZZLE, SOLUTION FROM {0}", Data.puzzleTableName);
                connection = Data.ConnectToDatabase(srcDatabase);
                command = Data.CreateCommand(connection, commandText);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    try
                    {
                        idContainer.Add(reader.GetInt32(0));
                        puzzleContainer.Add(reader.GetString(1));
                        solutionContainer.Add(reader.GetString(2));
                    }
                    catch (System.InvalidCastException)
                    {
                        Data.Flush(connection, command, reader);
                        return;
                    }
                }

            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            Data.Flush(connection, command, reader);
        }

        public static void SaveEncryptedPuzzle(string desDatabase, ICollection<int> idContainer, ICollection<string> puzzleContainer, ICollection<string> solutionContainer)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            string commandText = string.Empty;
            try
            {
                connection = Data.ConnectToDatabase(desDatabase);

                IEnumerator<int> ii = idContainer.GetEnumerator();
                IEnumerator<string> pi = puzzleContainer.GetEnumerator();
                IEnumerator<string> si = solutionContainer.GetEnumerator();

                while (ii.MoveNext() && pi.MoveNext() && si.MoveNext())
                {
                    commandText = string.Format("UPDATE {0} SET PUZZLE = '{1}', SOLUTION = '{2}' WHERE ID = '{3}'",
                        Data.puzzleTableName, pi.Current, si.Current, ii.Current);
                    command = Data.CreateCommand(connection, commandText);
                    command.ExecuteNonQuery();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            Data.Flush(connection, command);
        }
    }
}