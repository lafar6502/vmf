using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using VMF.Core.Util;
using VMF.Core;

namespace VMF.UI.App_Start
{
    public class ContainerConfig
    {
        public static void Configure(IWindsorContainer wc)
        {
            var cp = new JsonConfigProvider();
            VMF.Core.VMFGlobal.Config = cp;
            VMFGlobal.Container = wc;
        }
    }
}
