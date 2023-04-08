using CommonAccessDataObjectHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteManager;
using System;
using System.CodeDom;

namespace Test.SqliteManager
{
    [TestClass]
    public class StringSqlCommandTest
    {
        [TestMethod]
        public void DefaultCreation()
        {
            StringSqlCommand cmd = new StringSqlCommand("Select 1");
            Assert.IsNotNull(cmd);
        }

        [TestMethod]
        public void CreationWithParameters()
        {
            try
            {
                StringSqlCommand cmd = new StringSqlCommand("Select @p1, @p2, @p3", 1, 2, 3);
                int p1 = (int)cmd["@p1"];
                int p2 = (int)cmd["@p2"];
                int p3 = (int)cmd["@p3"];
                Assert.AreEqual(1+2+3, p1+p2+p3);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void CheckEnumeration()
        {
            try
            {
                StringSqlCommand cmd = new StringSqlCommand("Select @p1, @p2, @p3", 1, 2, 3);
                int val = 0;
                foreach(var i in cmd)
                {
                    val += (int)i.Value;
                }
                Assert.AreEqual(1+2+3, val);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void CheckParsing()
        {
            try
            {
                StringSqlCommand cmd = new StringSqlCommand("Select @p1, @p2, @p3", 1, 2, 3);
                var arrayValues = cmd.ConvertParameters((p) => (int)p.Value);
                var arrayNames = cmd.ConvertParameters((p) => p.Name);
                if(arrayValues.GetType().FullName != typeof(int[]).FullName)
                {
                    Assert.Fail();
                }
                if (arrayNames.GetType().FullName != typeof(string[]).FullName)
                {
                    Assert.Fail();
                }
                Assert.AreEqual(6, arrayNames.Length + arrayValues.Length);
            }
            catch
            {
                Assert.Fail();
            }
        }


    }
}
