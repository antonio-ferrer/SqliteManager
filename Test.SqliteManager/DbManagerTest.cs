using CommonAccessDataObjectHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteManager;
using System;
using System.Linq;

namespace Test.SqliteManager
{
    [TestClass]
    public class DbManagerTest
    {
        [TestMethod]
        public void CreateDatabase()
        {
            Assert.IsTrue(TryCreateDatase());
        }

        private bool TryCreateDatase(string path=null)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = System.IO.Path.GetTempFileName();
                    System.IO.File.Delete(path);
                    path += ".db";
                }
                DbManager db = new DbManager(path);
                db.ExecuteCommand("CREATE TABLE IF NOT EXISTS T1 (ID INT)");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetCurrentDbPath()
        {
            return Environment.CurrentDirectory + "\\test.db";
        }

        private bool TryInsertSomeValues()
        {
            try
            {
                string path = GetCurrentDbPath();
                if (TryCreateDatase(path))
                {
                    DbManager db = new DbManager(path);
                    db.ExecuteCommand(new StringSqlCommand("INSERT INTO T1 (ID) VALUES (@p1), (@p2), (@p3)", 1, 2, 3));
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        [TestMethod]
        public void InsertSomeValues()
        {
           Assert.IsTrue(TryInsertSomeValues());
        }

        [TestMethod]
        public void GetSomeValuesFromDataTable()
        {
            if (!TryInsertSomeValues())
                throw new Exception("fail on get some values");
            string path = GetCurrentDbPath();
            DbManager db = new DbManager(path);
            var tb = db.GetData("SELECT * FROM T1");
            Assert.IsTrue(tb.Rows.Count >= 3);
        }

        [TestMethod]
        public void GetSomeValues()
        {
            if (!TryInsertSomeValues())
                throw new Exception("fail on get some values");
            string path = GetCurrentDbPath();
            DbManager db = new DbManager(path);
            var values = db.GetEntity<int>("SELECT * FROM T1", (dr)=>dr.GetInt32(0));
            Assert.IsTrue(values.Count() >= 3);
        }
    }
}
