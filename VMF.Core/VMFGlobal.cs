using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;

namespace VMF.Core
{
    public class VMFGlobal
    {
        public static string AppProfile { get; set; }
        public static IConfigProvider Config { get; set; }
        public static IWindsorContainer Container { get; set; }


        public static T ResolveService<T>()
        {
            return Container.Resolve<T>();
        }

    }
}
