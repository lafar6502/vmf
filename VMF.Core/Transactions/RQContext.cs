using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace VMF.Core.Transactions
{
    public class RQContext
    {
        public int Id { get; set; }
        public IVMFTransaction VMFTransaction { get; set; }

        public static RQContext Current
        {
            get
            {
                return (RQContext) CallContext.LogicalGetData("_vmfRequestContext");
            }
            set
            {
                CallContext.LogicalSetData("_vmfRequestContext", value);
            }
        }

        public void Dispose()
        {
            var t0 = VMFTransaction;
            VMFTransaction = null;
            if (t0 != null) t0.Dispose();
        }
    }
}
