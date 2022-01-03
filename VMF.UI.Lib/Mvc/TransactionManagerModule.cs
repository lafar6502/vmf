using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NLog;

namespace VMF.UI.Lib.Mvc
{
    public class TransactionManagerModule : IHttpModule
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public void Dispose()
        {
            
        }

        public void Init(HttpApplication context)
        {
            
            log.Info("A");
        }
    }
}
