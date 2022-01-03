using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sooda;

namespace VMF.BusinessObjects.Common
{
    public class BaseObject : SoodaObject
    {
        public BaseObject(SoodaConstructor c) : base(c) { }

        public BaseObject(SoodaTransaction t) : base(t) { }

        
    }
}
