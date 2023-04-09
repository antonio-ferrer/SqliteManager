using CommonAccessDataObjectHelper;
using FileHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqliteManager
{
    public class DbManager : ICommonDbManager, IDisposable
    {
        private string connectionString;
        private string filePath;
        private bool disposedValue;
       

        public DbManager()
        {
        }

        public DbManager(string dbPath)
        {
            CreateDatabaseIfNotExists(dbPath);
        }

        public ICommonDbManager Initialize(params KeyValuePair<string, string>[] config)
        {
            Regex rxFilePath = new Regex(@"path", RegexOptions.IgnoreCase);
            var filePath = config.FirstOrDefault(c => rxFilePath.IsMatch(c.Key)).Value;
            if (!string.IsNullOrEmpty(filePath))
                CreateDatabaseIfNotExists(filePath);
            else
                throw new ArgumentException("the only valid parameter for this operation is \"path\"", nameof(config));
            return this;
        }

        public void CreateDatabaseIfNotExists(string dbPath)
        {
            filePath = dbPath;
            connectionString = $"Data Source={dbPath};Version=3;";
            if (!System.IO.File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }
        }

        public void ExecuteCommand(StringSqlCommand stringSqlCommand)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new NotSupportedException("connection string is not initialized! use CreateDatabaseIfNotExists to define it!");
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
            if (string.IsNullOrEmpty(connectionString))
                throw new NotSupportedException("connection string is not initialized! use CreateDatabaseIfNotExists to define it!");
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                return connection.GetDataTable(stringSqlCommand);
            }
        }

        public IEnumerable<T> GetEntity<T>(StringSqlCommand stringSqlCommand, Func<IDataRecord, T> parse)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new NotSupportedException("connection string is not initialized! use CreateDatabaseIfNotExists to define it!");
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                return connection.GetElements<T>(stringSqlCommand, parse);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SQLiteConnection.ClearAllPools();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.Collect();
            FileOperations.WaitForReleaseFile(filePath, TimeSpan.FromSeconds(30));
        }
    }
}
