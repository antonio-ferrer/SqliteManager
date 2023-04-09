using CommonAccessDataObjectHelper;
using LocalCryptoDb;
using SqliteManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalCryptDbConsoleApp
{
    public class LocalCryptoDbTestClass
    {

        private string tempDbPath;
        private const string directory = @"C:\sqliteDbs";

        public void CreateEncryptDb()
        {
            bool result = TryCreateEncryptDb(out LocalCryptoDbManager db);
            db.Dispose();
        }

        public void InsertCandidates()
        {
            bool create = TryCreateEncryptDb(out LocalCryptoDbManager db);
            bool result = TryInsertCandidateTable(db);
            db.Dispose();
        }

        public void GetCandidates()
        {
            bool create = TryCreateEncryptDb(out LocalCryptoDbManager db);
            bool result = TryGetCandidateTable(db, out DataTable table);
            db.Dispose();
        }

        private bool TryCreateEncryptDb(out LocalCryptoDbManager db)
        {
            try
            {
                db = new LocalCryptoDbManager(
                    () => new EncryptionManager.AesEncryptor().SetSecret("test1", "test2"),
                    () => new DbManager()
                );

                db.Initialize(GetEncryptedPath(), GetWorkPath());
                db.ExecuteCommand("CREATE TABLE IF NOT EXISTS Panpacapam (loveId INTEGER PRIMARY KEY, candidate TEXT NOT NULL)");

                return true;
            }
            catch
            {
                db = null;
                return false;
            }
        }

        private bool TryInsertCandidateTable(LocalCryptoDbManager db)
        {
            if (db == null) return false;
            try
            {
                StringSqlCommand insert = new StringSqlCommand(@"INSERT INTO Panpacapam (loveId, candidate) VALUES (@id, @name) ON CONFLICT DO NOTHING;");
                var parameters = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(1, "Serena Willians"),
                    new KeyValuePair<int, string>(2, "JLo"),
                    new KeyValuePair<int, string>(3, "Beyoncé")
                };
                foreach (var p in parameters)
                {
                    insert["@id"] = p.Key;
                    insert["@name"] = p.Value;
                    db.ExecuteCommand(insert);
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        private bool TryGetCandidateTable(LocalCryptoDbManager db, out DataTable table)
        {
            table = null;
            if (db == null)
                return false;
            try
            {
                table = db.GetData("SELECT loveId, candidate FROM Panpacapam");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetEncryptedPath() => $"{Path.Combine(directory, "encryptTest.db")}";

        private string GetWorkPath()
        {
            if (!Directory.Exists(directory))
                throw new NotSupportedException($@"Create the directory: {directory}");
            if (tempDbPath == null)
                tempDbPath = "workingDb.db";//Path.GetRandomFileName();
            return Path.Combine(directory, tempDbPath);
        }
    }
}
