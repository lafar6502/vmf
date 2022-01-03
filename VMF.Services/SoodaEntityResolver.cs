using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;
using Sooda;

namespace VMF.Services
{
    public class SoodaEntityResolver : IEntityResolver
    {
        public object Get(string reference)
        {
            var er = EntityRef.Parse(reference);
            return Get(er);
        }

        public object Get(EntityRef entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetMany(IEnumerable<EntityRef> refs)
        {
            var claszz = refs.GroupBy(x => x.Entity);
            foreach(var g in claszz)
            {
                
            }
            throw new NotImplementedException();
        }

        public string GetObjectLabel(object obj)
        {
            throw new NotImplementedException();
        }

        public EntityRef GetObjectRef(object obj)
        {
            throw new NotImplementedException();
        }

        public bool KnowsEntityType(Type t)
        {
            throw new NotImplementedException();
        }

        public Type KnowsEntityType(string entName)
        {
            throw new NotImplementedException();
        }
    }
}
