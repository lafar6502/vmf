using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VMF.Core.Util;
using System.IO;
using VMF.Core;
using VMF.Services.Transactions;
using Castle.Windsor;
using NLog;
using VMF.BusinessObjects;

namespace VMFTest
{
    [TestClass]
    public class MessageBusTests
    {
        [TestInitialize]
        public void Init()
        {
            NLog.Config.SimpleConfigurator.ConfigureForConsoleLogging(LogLevel.Debug);
            TestContainerConfig.SetupContainer();
        }

    }
}
