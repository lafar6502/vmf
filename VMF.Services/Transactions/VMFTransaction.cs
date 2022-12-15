using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;
using Sooda;
using System.Data;
using NLog;
using System.Data.Common;
using System.Transactions;
using NGinnBPM.MessageBus;
using Newtonsoft.Json;

namespace VMF.Services.Transactions
{
    public class VMFTransaction : IVMFTransaction
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public VMFTransaction(IDbConnection cn)
        {
            if (SoodaTransaction.HasActiveTransaction)
            {
                log.Error("There is active transaction");
                throw new Exception();
            }

            _st = new SoodaTransaction();
            if (cn != null)
            {
                var ds = _st.OpenDataSource("default", cn);
            }
            else
            {
                var ds = _st.OpenDataSource("default");
            }
            
        }
        private SoodaTransaction _st;

        private IDbConnection _defaultCn = null;
        public string Tid { get; set; }

        public bool HasModifications
        {
            get 
            { 
                return _st.HasUncommitedChanges; 
            }
        }

        /// <summary>
        /// current transactions database connection
        /// </summary>
        public IDbConnection DefaultConnection
        {
            get
            {
                if (_defaultCn == null)
                {
                    var ds = _st.OpenDataSource("default");
                    _defaultCn = ds.Connection;
                }
                return _defaultCn;
            }
        }

        public void Commit()
        {
            _st.Commit();
        }

        internal class VMFTranState
        {
            public string SoodaState { get; set; }
            public string MBState { get; set; }
            public Dictionary<string, object> Data { get; set; }
        }
        public void DeserializeState(string state)
        {
            var ss = JsonConvert.DeserializeObject<VMFTranState>(state);
            if (!string.IsNullOrEmpty(ss.MBState)) MessageBusContext.SetCurrentTransactionState(ss.MBState);
            _data = ss.Data ?? new Dictionary<string, object>();
            _st.Deserialize(ss.SoodaState);
        }

        public void Dispose()
        {
            if (_st != null)
            {
                _st.Dispose();
                _st = null;
            }
        }

        private Dictionary<string, object> _data = new Dictionary<string, object>();
        public T GetData<T>(string key, T defaultValue)
        {
            return _data.GetValueOrDefault(key, defaultValue);
        }

        public bool HasData(string key)
        {
            return _data.ContainsKey(key);
        }

        public void ReEnlist()
        {
            ReEnlistSooda();
        }

        private void ReEnlistSooda()
        {
            if (Transaction.Current == null) return;
            foreach (var ds in _st.Schema.DataSources)
            {
                var dss = _st.OpenDataSource(ds);
                if (!dss.IsOpen) continue;
                var cn = dss.Connection as DbConnection;
                if (cn == null || cn.State == ConnectionState.Closed || cn.State == ConnectionState.Broken) continue;
                //if (Debug) log.Info("Re-enlisting connection '{0}'", ds.Name);
                cn.EnlistTransaction(Transaction.Current);
            }

        }

        public string SerializeState()
        {
            var ss = new VMFTranState
            {
                SoodaState = _st.Serialize(),
                Data = _data,
                MBState = MessageBusContext.GetSerializedCurrentTransactionState()
            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(ss);
        }

        public void SetData(string key, object value)
        {
            _data[key] = value;
        }
    }
}
