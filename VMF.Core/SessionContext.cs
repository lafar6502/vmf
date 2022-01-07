using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Data;

namespace VMF.Core
{
    public enum SCDataScope
    {
        Request = 0,
        Transaction = 2,
        Session = 4,
        User = 6,
        Workstation = 8
    }

    public class SessionContext
    {
        [ThreadStatic]
        private static SessionContext _sc = null;
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static SessionContext Current 
        {
            get { return _sc; }
            set { _sc = value; }
        }

        public SessionContext()
        {
            CurrentTransactionMode = Core.TransactionMode.Discard;
            Language = AppGlobal.Config.Get("DefaultLanguage", "en");
            User = AppUser.Current;
            if (User != null)
            {
                if (!string.IsNullOrEmpty(User.DefaultLanguage)) Language = User.DefaultLanguage;
            }
        }

        public AppUser User { get; set; }
        public string Language { get; set; }
        public IVMFTransaction Transaction { get; set; }
        public TransactionMode CurrentTransactionMode { get; set; }
        public string RequestId { get; set; }
        /// <summary>
        /// access token you got from the link
        /// to be used only with anonymous access
        /// </summary>
        public string AccessToken
        {
            get { return GetData(SCDataScope.Session, "_accessToken", (string)null); }
            set { SetData(SCDataScope.Session, "_accessToken", value); }
        }

        /// <summary>
        /// client connection info (for anonymous...)
        /// </summary>
        public string ClientInfo { get; set; }
        /// <summary>
        /// client machine name
        /// To uniquely identify a workstation
        /// WARN: this is not the IP of the client, we maintain workstation id 
        /// via cookies (because the IP is changed by firewalls)
        /// </summary>
        public string WorkstationId { get; set; }

        private IDictionary<string, object> _data;
        
        public IDictionary<string, object> SessionScopeData { get; set; }


        public void SetData(SCDataScope scope, string key, object value)
        {
            if (scope == SCDataScope.Request)
            {
                if (_data == null) _data = new Dictionary<string, object>();
                _data[key] = value;
            }
            else if (scope == SCDataScope.Session)
            {
                if (SessionScopeData == null) SessionScopeData = new Dictionary<string, object>();
                if (value != null)
                    SessionScopeData[key] = value;
                else
                    SessionScopeData.Remove(key);
            }
            else if (scope == SCDataScope.User)
            {
                //var upm = AppGlobal.ResolveService<IUserPreferencesManager>();
                //upm.Set(key, value);
            }
            else if (scope == SCDataScope.Workstation)
            {
                //var sd = AppGlobal.ResolveService<IWorkstationDataStore>();
                //if (string.IsNullOrEmpty(this.WorkstationId)) throw new Exception("ClientMachine info empty");
                //sd.Set(this.WorkstationId, key, value);
            }
            else if (scope == SCDataScope.Transaction)
            {
                if (Transaction == null) throw new Exception("No transaction!");
                Transaction.SetData(key, value.ToString());
            }
            else throw new Exception();
            
        }

        public void SetData(string key, object value)
        {
            SetData(SCDataScope.Request, key, value);
        }

        public T GetData<T>(string name, T defVal)
        {
            return GetData(SCDataScope.User, name, defVal);
        }

        public T GetData<T>(SCDataScope scope, string name, T defVal)
        {
            if (_data != null && _data.ContainsKey(name) && _data[name] != null) return (T)_data[name];
            if (scope >= SCDataScope.Transaction)
            {
                if (Transaction != null && Transaction.HasData(name))
                {
                    return Transaction.GetData<T>(name, defVal);
                }
            }
            if (scope >= SCDataScope.Session)
            {
                if (SessionScopeData != null && SessionScopeData.ContainsKey(name) && SessionScopeData[name] != null) return (T)SessionScopeData[name];
            }
            T workstationVal = default(T);
            if (scope == SCDataScope.Workstation && !string.IsNullOrEmpty(this.WorkstationId))
            {
                //var sd = AppGlobal.ResolveService<IWorkstationDataStore>();
                //workstationVal = sd.Get(this.WorkstationId, name, defVal);
                //if (!workstationVal.Equals(defVal)) return workstationVal;
            }
            if (scope >= SCDataScope.User)
            {
                if (this.User != null)
                {
                    //var upm = AppGlobal.ResolveService<IUserPreferencesManager>();
                    //return upm.Get<T>(name, defVal);
                }
                
            }
            return defVal;
        }

        public T GetSetData<T>(string name, Func<T> defVal)
        {
            object v;
            if (_data == null) _data = new Dictionary<string, object>();
            if (_data.TryGetValue(name, out v))
            {
                return (T)v;
            }
            else
            {
                v = defVal();
                _data[name] = v;
                return (T)v;
            }
        }

        /*

        public PrinterInfo WebPrinter
        {
            get
            {

                var pn = GetData(SCDataScope.Workstation, "WebPrinterName", "");
                if (string.IsNullOrEmpty(pn)) pn = GetData(SCDataScope.Session, "WebPrinterName", "");
                IPrintManager pm = AppGlobal.ResolveService<IPrintManager>();

                if (!string.IsNullOrEmpty(pn))
                {
                    var pr = pm.GetActivePrinter(pn);
                    return pr;
                }
                if (string.IsNullOrEmpty(pn) && !string.IsNullOrEmpty(this.WorkstationId))
                {
                    var ap = pm.ActivePrinters;
                    var p2 = ap.FirstOrDefault(x => x.Active && x.MachineName == this.WorkstationId);
                    if (p2 != null) return p2;
                    p2 = ap.FirstOrDefault(x => x.Active && x.MachineName.StartsWith(this.WorkstationId + "/"));
                    if (p2 != null) return p2;
                }
                return null;
            }
            set
            {
                WebPrinterName = value == null ? null : value.MachineName;
            }
        }*/

        /// <summary>
        ///  web printer name that has been configured (but without checking if its active or not)
        /// </summary>
        public string WebPrinterName
        {
            get
            {
                var pn = GetData(SCDataScope.Workstation, "WebPrinterName", "");
                if (string.IsNullOrEmpty(pn)) pn = GetData(SCDataScope.Session, "WebPrinterName", "");
                return pn;
            }
            set
            {
                SetData(SCDataScope.Session, "WebPrinterName", value);
            }
        }

        /// <summary>
        /// database connection managed externally for facile transaction
        /// and message bus. Connection lifetime managed independently - we dont touch it.
        /// </summary>
        public IDbConnection RequestDbConnection
        {
            get { return GetData("_fa_dbcn", (IDbConnection)null); }
            set { SetData("_fa_dbcn", value); }
        }

        
    }
}
