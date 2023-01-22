using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core.Lists
{
    public interface IQueryParamProvider
    {
        object GetQueryParam(string key);
    }
}
