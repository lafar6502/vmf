using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    public interface IVMFTransFactory
    {
        IVMFTransaction CreateTransaction();
        IVMFTransaction CreateTransaction(IDbConnection externalConnection);
    }
}
