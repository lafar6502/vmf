using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;

namespace VMF.Core
{
    public class AppGlobal
    {
        public static string AppProfile { get; set; }
        public static IConfiguration Config { get; set; }
        public static IWindsorContainer Container { get; set; }


        public static T ResolveService<T>()
        {
            return Container.Resolve<T>();
        }
        //lang for the ui/html
        public static string UILang
        {
            get { return Config.Get("DefaultUILang", "en"); }
        }
    }
}
