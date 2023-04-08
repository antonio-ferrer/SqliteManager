using CommonAccessDataObjectHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace SqliteManager
{
    public class DbManager
    {
        private readonly string connectionString;
        public DbManager(string dbPath)
        {
            connectionString = $"Data Source={dbPath};Version=3;";
            CreateDatabaseIfNotExists(dbPath);
        }

        private void CreateDatabaseIfNotExists(string dbPath)
        {
            if (!System.IO.File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }
        }

        public void ExecuteCommand(StringSqlCommand stringSqlCommand)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(stringSqlCommand, connection);
                if (stringSqlCommand.HasParameter)
                {
                    foreach (var p in stringSqlCommand)
                    {
                        command.Parameters.AddWithValue(p.Name, p.Value);
                    }
                }
                command.ExecuteNonQuery();
            }
        }

        public DataTable GetData(StringSqlCommand stringSqlCommand)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                return connection.GetDataTable(stringSqlCommand);
            }
        }

        public IEnumerable<T> GetEntity<T>(StringSqlCommand stringSqlCommand, Func<IDataRecord, T> parse)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                return connection.GetElements<T>(stringSqlCommand, parse);
            }
        }
    }
}
