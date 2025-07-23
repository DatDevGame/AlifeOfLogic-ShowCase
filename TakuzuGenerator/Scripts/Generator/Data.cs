using System;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Takuzu.Generator
{
    public static class Data
    {
        public static Action<string> onDatabaseReady = delegate { };

        public static readonly string puzzleTableName = "PUZZLE";
        public static readonly string infoTableName = "INFO";

        public static string persistentDataPath;
        public static string streamingAssetsPath;

        public static void PrepareDatabase(string dbName)
        {
#if UNITY_EDITOR
            if (File.Exists(dbName))
            {
                onDatabaseReady(dbName);
            }
            else
            {
                throw new ArgumentException("No database found");
            }
#elif UNITY_ANDROID || UNITY_IOS
            //if (!ExistsDatabase(dbName))
            //{
                CopyDatabaseFromStreamingAsset(dbName);
            //}
            onDatabaseReady(dbName);

#endif
        }

        public static void CreateDatabase(string dbPath)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            try
            {
                connection = ConnectToDatabase(dbPath);
                command = CreateCommand(connection, string.Format("CREATE TABLE {0}(NAME TEXT, VALUE TEXT)", infoTableName));
                command.ExecuteNonQuery();
                Flush(null, command, null);

                string commandText;
                commandText = string.Format(
                    "CREATE TABLE {0} " +
                    "(" +
                    "   ID INTEGER PRIMARY KEY," +
                    "   PACK TEXT," +
                    "   PUZZLE TEXT NOT NULL," +
                    "   SOLUTION TEXT NOT NULL," +
                    "   SIZE INTEGER NOT NULL," +
                    "   GIVENNUMBER INTEGER," +
                    "   PARSENUMBER INTEGER," +
                    "   PARSEPERCENT FLOAT," +
                    "   LSDNUMBER INTEGER," +
                    "   LSDPERCENT FLOAT," +
                    "   ALSDNUMBER INTEGER," +
                    "   ALSDPERCENT FLOAT," +
                    "   LEVEL INTEGER NOT NULL" +
                    ")",
                    puzzleTableName);
                command = CreateCommand(connection, commandText);
                command.ExecuteNonQuery();

                Flush(connection, command, null);
            }
            catch (System.Exception e)
            {
                Helper.LogErrorFormat(e.Message);
            }
            finally
            {
                Flush(connection, command);
            }
        }

        public static IDbConnection ConnectToDatabase(string dbName)
        {
            IDbConnection connection = new SqliteConnection(GetConnectionString(dbName));
            connection.Open();
            return connection;
        }

        public static IDbCommand CreateCommand(IDbConnection connection, string commandText)
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            return command;
        }

        /// <summary>
        /// Get connection string for a database.
        /// </summary>
        /// <param name="dbName">Name of the database. In editor, you have to pass in the full path to the database.</param>
        /// <returns></returns>
        public static string GetConnectionString(string dbName)
        {
#if UNITY_EDITOR
            return "URI=file:" + dbName;
#else
            return "URI=file:" + GetDataPath(dbName);
#endif

        }

        public static void EnableWAL(string dbName)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            string commandText = string.Empty;
            try
            {
                commandText = string.Format(
                    "PRAGMA journal_mode = \"WAL\";");
                connection = ConnectToDatabase(dbName);
                command = CreateCommand(connection, commandText);
                Debug.Log("Set journal mode: " + command.ExecuteScalar().ToString());
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            Flush(connection, command);
            Debug.Log("CURRENT: " + GetJournalMode(dbName));
        }

        public static string GetJournalMode(string dbName)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            string commandText = string.Empty;
            string jounalMode = string.Empty;
            try
            {
                commandText = string.Format("PRAGMA journal_mode");
                connection = ConnectToDatabase(dbName);
                command = CreateCommand(connection, commandText);
                jounalMode = command.ExecuteScalar().ToString();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            Flush(connection, command);

            return jounalMode;
        }

        public static string GetStreamingAssetsPath(string dbName)
        {
            if (string.IsNullOrEmpty(streamingAssetsPath))
            {
                streamingAssetsPath = Application.streamingAssetsPath;
            }
            return Path.Combine(streamingAssetsPath, dbName);
        }

        public static string GetDataPath(string dbName)
        {
            if (string.IsNullOrEmpty(persistentDataPath))
            {
                persistentDataPath = Application.persistentDataPath;
            }
            return Path.Combine(persistentDataPath, dbName);
        }

        public static void CopyDatabaseFromStreamingAsset(string dbName)
        {
#if UNITY_ANDROID
            WWW db = new WWW(GetStreamingAssetsPath(dbName));
            while (!db.isDone) { };
            if (!string.IsNullOrEmpty(db.error))
            {
                Debug.Log(db.error);
            }
            else
            {
                try
                {
                    File.WriteAllBytes(GetDataPath(dbName), db.bytes);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }
            }

#elif UNITY_IOS
            try
            {
                File.Copy(GetStreamingAssetsPath(dbName), GetDataPath(dbName), true);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
            }
#endif
        }

        public static bool ExistsDatabase(string dbName)
        {
            return dbName != null && File.Exists(GetDataPath(dbName));
        }

        //check if size of the file in streaming assets and local storage is equal, to prevent error when player close the app while database is copying 
        public static bool IsFileSizeEqual(string dbName)
        {
            int streamingAssetBytesLength = File.ReadAllBytes(GetStreamingAssetsPath(dbName)).Length;
            int localStorageBytesLength = File.ReadAllBytes(GetDataPath(dbName)).Length;
            return streamingAssetBytesLength == localStorageBytesLength;
        }

        public static void GetAllPack(string dbName, ICollection<string> container)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            string commandText = string.Empty;
            try
            {
                commandText = string.Format("SELECT DISTINCT PACK FROM {0}", puzzleTableName);
                connection = ConnectToDatabase(dbName);
                command = CreateCommand(connection, commandText);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string packName = reader.GetString(0);
                    container.Add(packName);
                }
            }
            catch (InvalidCastException)
            {
                Flush(connection, command, reader);
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            Flush(connection, command, reader);
        }

        public static void Flush(IDbConnection connection = null, IDbCommand command = null, IDataReader reader = null)
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
            if (command != null)
            {
                command.Dispose();
            }
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }

        }

        public static void GetAllPuzzleString(string db, ICollection<string> container)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            string commandText = string.Empty;
            try
            {
                commandText = "SELECT PUZZLE FROM PUZZLE";

                connection = ConnectToDatabase(db);
                command = CreateCommand(connection, commandText);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string p = reader.GetString(0);
                    container.Add(p);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }

            Flush(connection, command, reader);
        }

        public static void GetAllSolutionString(string db, ICollection<string> container)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            string commandText = string.Empty;
            try
            {
                commandText = "SELECT SOLUTION FROM PUZZLE";

                connection = ConnectToDatabase(db);
                command = CreateCommand(connection, commandText);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string p = reader.GetString(0);
                    container.Add(p);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }

            Flush(connection, command, reader);
        }

        public static void DeletePuzzleById(string db, int id)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            string commandText = string.Empty;

            try
            {
                commandText = string.Format("DELETE FROM {0} WHERE ID = '{1}'", puzzleTableName, id);
                connection = ConnectToDatabase(db);
                command = CreateCommand(connection, commandText);
                command.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            Flush(connection, command);
        }

        public static void UpdateInfoTable(string db)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            string commandText = string.Empty;

            try
            {
                connection = ConnectToDatabase(db);

                //clear old data
                commandText = string.Format("DELETE FROM {0}", infoTableName);
                command = CreateCommand(connection, commandText);
                command.ExecuteNonQuery();
                Flush(null, command);

                //get all size from the pack
                commandText = string.Format("SELECT DISTINCT SIZE FROM {0}", puzzleTableName);
                command = CreateCommand(connection, commandText);
                reader = command.ExecuteReader();
                List<int> sizes = new List<int>();
                while (reader.Read())
                {
                    try
                    {
                        sizes.Add(reader.GetInt32(0));
                    }
                    catch (InvalidCastException)
                    {
                        break;
                    }
                }
                Flush(null, command);

                commandText = string.Format("SELECT DISTINCT SIZE, LEVEL FROM {0}", puzzleTableName);
                command = CreateCommand(connection, commandText);
                reader = command.ExecuteReader();
                List<string> level = new List<string>();
                while (reader.Read())
                {
                    try
                    {
                        string code = reader.GetInt32(0) + "_" + reader.GetInt32(1);
                        level.Add(code);
                    }
                    catch (InvalidCastException)
                    {
                        break;
                    }
                }
                Flush(null, command, reader);

                commandText = string.Format("SELECT count(ID) FROM {0}", puzzleTableName);
                command = CreateCommand(connection, commandText);
                reader = command.ExecuteReader();
                int count = 0;
                if (reader.Read())
                {
                    count = reader.GetInt32(0);
                }
                Flush(null, command);

                commandText = string.Format(
                    "INSERT INTO {0} VALUES " +
                    "('SIZE', '{1}')," +
                    "('LEVEL', '{2}')," +
                    "('COUNT', '{3}')",
                    infoTableName,
                    sizes.ListElementToString(),
                    level.ListElementToString(),
                    count);
                command = CreateCommand(connection, commandText);
                command.ExecuteNonQuery();
                Flush(null, command, reader);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            Flush(connection, command, reader);
        }

        public static void GetDbInfo(string db, IDictionary<string, string> infoContainer)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            string commandText = string.Empty;

            try
            {
                commandText = string.Format("SELECT * FROM {0}", infoTableName);
                connection = ConnectToDatabase(db);
                command = CreateCommand(connection, commandText);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        string key = reader.GetString(0);
                        string value = reader.GetString(1);
                        infoContainer.Add(key, value);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            Flush(connection, command, reader);
        }
    }
}

