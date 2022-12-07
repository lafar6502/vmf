using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VMF.Core.Util;
using System.IO;
using VMF.Core;
using VMF.Services.Transactions;
using Castle.Windsor;

namespace VMFTest
{
    [TestClass]
    public class TranTest1
    {
        [TestInitialize]
        public void Init()
        {
            var c = new JsonConfigProvider("test");
            VMFGlobal.Config = c;

            VMFGlobal.Container = new Castle.Windsor.WindsorContainer();
        }

        public static void SetupContainer()
        {
            IWindsorContainer wc = VMFGlobal.Container;

        }

        [TestMethod]
        public void Test1()
        {
            TransUtil.SetUpAmbientTransaction();
            TransUtil.CommitAndReEnlist(null);
            TransUtil.CleanupAmbientTransaction(false);
        }
    }
}
