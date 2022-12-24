using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    public interface ISelectView
    {
        IEnumerable<string> GetViewNames();
    }
}
