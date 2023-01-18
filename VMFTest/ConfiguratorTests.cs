using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VMF.Core.Util;
using System.IO;
using VMF.Core;
using VMF.Services.Transactions;
using Castle.Windsor;
using VMF.Configurator;

namespace VMFTest
{
    [TestClass]
    public class ConfiguratorTests
    {
        protected ConfigLogicProvider _logic;
        
        [TestInitialize]
        public void Init()
        {
            var c = new JsonConfigProvider("test");
            VMFGlobal.Config = c;
            VMFGlobal.Container = new Castle.Windsor.WindsorContainer();
            _logic = new ConfigLogicProvider();
            _logic.ScriptFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurators");
            Console.WriteLine("Script folder :{0}", _logic.ScriptFolder);
        }

        public static void SetupContainer()
        {
            IWindsorContainer wc = VMFGlobal.Container;

        }

        [TestMethod]
        public void Test1()
        {
            foreach(var n in _logic.GetAllModelNames())
            {
                Console.WriteLine(n);
                var mi = _logic.GetConfigModelInfo(n);
                Assert.IsNotNull(mi);
                foreach(var p in mi.Fields)
                {
                    Console.WriteLine("- {0}:{1}", p.Name, p.ParamType.Name);
                }
            }
        }
    }
}
