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
    }
}
