using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VMF.Core.Util;
using System.IO;
namespace VMFTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestBase1()
        {
            var dm = AppDomain.CurrentDomain.BaseDirectory;
            VMF.Core.Util.JsonTranslationFile jf = new VMF.Core.Util.JsonTranslationFile(Path.Combine(dm, "lang1.json"));
            var a = jf.Get("Test1.Id1", "pl");
        }

        [TestMethod]
        public void TestConfigLoads()
        {
            var dd = AppDomain.CurrentDomain.BaseDirectory;

            var c = new JsonConfigProvider("test");
            var s0 = c.Get("TestString", "");
            Assert.AreEqual(s0, "2");

        }

        public class TestObj
        {
            public string Name { get; set; }
            public int Num { get; set; }
        }

        [TestMethod]
        public void TestConfig2()
        {
            var dd = AppDomain.CurrentDomain.BaseDirectory;

            var c = new JsonConfigProvider("test");
            var s0 = c.Get("TestString", "");
            Assert.AreEqual(s0, "2");
            var p = new TestObj();
            c.SetProperties("TestObj", p);
            //Assert.AreEqual(p.Num, 2);
            var i1 = c.Get<int>("TestObj.Num", -1);
        }
    }
}
