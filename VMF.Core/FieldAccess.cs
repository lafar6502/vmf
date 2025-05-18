using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    public enum FieldAccess
    {
        Undefined,
        NoAccess,
        ReadOnly,
        ReadWrite,
        Required
    }
}
