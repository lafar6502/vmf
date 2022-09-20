using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace VMF.Core
{
    public interface IVMFTransactionFactory
    {
        IVMFTransaction CreateTransaction(IDbConnection cn = null);
    }
}
