using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

[assembly: PreApplicationStartMethod(typeof(VMF.UI.App_Start.ApplicationPreInit), "Start")]

namespace VMF.UI.App_Start
{
    public class  ApplicationPreInit
    {

        public static void Start()
        {
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(VMF.UI.Lib.Mvc.TransactionManagerModule));
        }
    }
}
