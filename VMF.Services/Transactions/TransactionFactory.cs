using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;

namespace VMF.Services.Transactions
{
    public class TransactionFactory : IVMFTransactionFactory
    {
        public IVMFTransaction CreateTransaction(IDbConnection cn = null)
        {
            return new VMFTransaction(cn);
        }
    }
}
