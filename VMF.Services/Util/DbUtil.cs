using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;

namespace VMF.Services.Util
{
    public class DbUtil
    {
        public static IDbConnection CreateDefaultConnection()
        {
            var dbfact = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var cs = VMFGlobal.Config.Get("default.connectionString", "");
            var cn = dbfact.CreateConnection();
            cn.ConnectionString = cs;
            cn.Open();
            return cn;
        }
    }
}
