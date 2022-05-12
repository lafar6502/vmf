using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;
using Sooda;
using System.Data;
using NLog;
namespace VMF.Services.Transactions
{
    public class VMFTransaction : IVMFTransaction
    {

        protected VMFTransaction(IDbConnection cn)
        {
            if (SoodaTransaction.ActiveTransaction != null)
            {

            }

            _st = new SoodaTransaction();
            if (cn != null)
            {
                var ds = _st.OpenDataSource("default");
            }
            else
            {
                 
            }
            
        }
        private SoodaTransaction _st;
        public string Tid { get; set; }

        public bool HasModifications
        {
            get 
            { 
                return _st.HasUncommitedChanges; 
            }
        }

        public void Commit()
        {
            _st.Commit();
        }

        public void DeserializeState(string state)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public T GetData<T>(string key, T defaultValue)
        {
            throw new NotImplementedException();
        }

        public bool HasData(string key)
        {
            throw new NotImplementedException();
        }

        public void ReEnlist()
        {
            throw new NotImplementedException();
        }

        public string SerializeState()
        {
            throw new NotImplementedException();
        }

        public void SetData(string key, object value)
        {
            throw new NotImplementedException();
        }
    }
}
