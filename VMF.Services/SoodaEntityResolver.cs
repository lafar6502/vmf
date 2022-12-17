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
            var st = SoodaTransaction.ActiveTransaction;
            var sf = st.GetFactory(entity.Entity);
            if (sf == null) throw new Exception("Not found:" + entity.Entity);
            var ft = sf.GetPrimaryKeyFieldHandler().GetFieldType();
            var flds = sf.GetClassInfo().GetPrimaryKeyFields();
            if (flds.Length != 1) throw new Exception("Keys..");
            var kv = Convert.ChangeType(entity.Id, flds[0].Type);
            var v = sf.GetRef(st, kv);
            return v;
        }

        public IEnumerable<object> GetMany(IEnumerable<EntityRef> refs)
        {
            var claszz = refs.GroupBy(x => x.Entity);
            foreach(var g in claszz)
            {
                
            }
            foreach(var e in refs)
            {
                yield return Get(e);
            }
        }

        public string GetObjectLabel(object obj)
        {
            var so = obj as SoodaObject;
            return so.GetLabel(false);
        }

        public EntityRef GetObjectRef(object obj)
        {
            if (obj == null) return null;
            var so = obj as SoodaObject;
            if (so == null) throw new Exception();
            var kv = so.GetPrimaryKeyValue();
            return new EntityRef(obj.GetType().Name, kv.ToString());
        }

        public bool KnowsEntityType(Type t)
        {
            if (!typeof(SoodaObject).IsAssignableFrom(t)) return false;
            return true;
        }

        public Type KnowsEntityType(string entName)
        {
            var tf = SoodaTransaction.ActiveTransaction.GetFactory(entName);
            if (tf == null) return null;
            return tf.TheType;
        }
    }
}
