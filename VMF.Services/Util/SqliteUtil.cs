using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Dapper;

namespace VMF.Services.Util
{
    internal class SqliteUtil
    {

        private static Logger log = LogManager.GetCurrentClassLogger();
        public static SQLiteConnection OpenDb(string fileName, string ddl, bool readOnly = false)
        {
            var connstr1 = string.Format("Data Source={0};Version=3;Enlist=N;", fileName);
            var connstr2 = string.Format("Data Source={0};Version=3;Enlist=N;PRAGMA journal_mode=WAL;{1}", fileName, readOnly ? "Read Only=True;" : "");

            
            var hasFile = File.Exists(fileName);
            if (!hasFile)
            {
                try
                {
                    log.Info("Initializing database  {0}", connstr1);
                    using (var c2 = new SQLiteConnection(connstr1))
                    {
                        c2.Execute(ddl);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error initializing new database {0}: {1}", fileName, ex.ToString());
                    File.Delete(fileName);
                    throw;
                }
            }
            

            var cn = new SQLiteConnection(connstr2);
            cn.Open();
            return cn;
        }
    }
}
