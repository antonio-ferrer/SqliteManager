using CommonAccessDataObjectHelper;
using LocalCryptoDb;
using SqliteManager;
using System.Data;

namespace Test.LocalCryptoDb
{
    public class LocalCryptoDbTestClass
    {

        private const string DbDirectories = @"C:\sqliteDbs";
        private string? tempDbPath;

        [Fact]
        public void CreateEncryptDb()
        {
            bool result = TryCreateEncryptDb(out LocalCryptoDbManager? db);
            db?.Dispose();
            Assert.True(result);
        }

        [Fact]
        public void InsertCandidates()
        {
            bool create = TryCreateEncryptDb(out LocalCryptoDbManager? db);
            Assert.True(create);
            bool result = TryInsertCandidateTable(db);
            db?.Dispose();
            Assert.True(result);
        }

        [Fact]
        public void GetCandidates()
        {
            bool create = TryCreateEncryptDb(out LocalCryptoDbManager? db);
            Assert.True(create);
            bool result = TryGetCandidateTable(db, out DataTable? table);
            db?.Dispose();
            Assert.True(result);
        }

        private bool TryCreateEncryptDb(out LocalCryptoDbManager? db)
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

        private bool TryInsertCandidateTable(LocalCryptoDbManager? db)
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

        private bool TryGetCandidateTable(LocalCryptoDbManager? db, out DataTable? table)
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

        private string GetEncryptedPath() => $"{Path.Combine(DbDirectories, "encryptTest.db")}";

        private string GetWorkPath()
        {
            if (!Directory.Exists(DbDirectories))
                throw new NotSupportedException($@"Create the directory: {DbDirectories}");
            if (tempDbPath == null)
                tempDbPath = Path.GetRandomFileName();
            return Path.Combine(DbDirectories, tempDbPath);
        }
    }
}