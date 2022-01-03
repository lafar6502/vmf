using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    public interface IConfigProvider
    {
        T Get<T>(string name, T defaultValue);
        T Get<T>(string name);
        void SetProperties(string configValue, object target);
    }

    

}
