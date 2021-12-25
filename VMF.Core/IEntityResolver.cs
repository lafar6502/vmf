using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    public class EntityRef
    {
        public EntityRef()
        {
        }

        public EntityRef(string entity, string id)
        {
            Entity = entity;
            Id = id;
        }

        public EntityRef(string refString)
        {
            string entity, id;
            if (!DoParse(refString, out entity, out id)) throw new Exception("Invalid entity ref: " + refString);
            Entity = entity;
            Id = id;
        }

        public string Entity { get; set; }
        public string Id { get; set; }

        public override string ToString()
        {
            return string.Format("{0}~{1}", Entity, Id);
        }

        private static bool DoParse(string s, out string entity, out string id)
        {
            int idx = s.LastIndexOf('~');
            if (idx < 0)
            {
                entity = s;
                id = null;
            }
            else
            {
                entity = s.Substring(0, idx);
                id = s.Substring(idx + 1);
            }
            return true;
        }

        public static EntityRef Parse(string s)
        {
            
            return new EntityRef(s);
        }

        public static bool TryParse(string s, out EntityRef er)
        {
            string entity, id;
            if (DoParse(s, out entity, out id))
            {
                er = new EntityRef(entity, id);
                return true;
            }
            else
            {
                er = null;
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Entity.GetHashCode() + Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            EntityRef er2 = obj as EntityRef;
            if (er2 == null) return false;
            return er2.Entity == this.Entity && er2.Id == this.Id;
        }

        public static bool operator==(EntityRef er1, EntityRef er2)
        {
            if (ReferenceEquals(er1, null)) return ReferenceEquals(er2, null);
            return  er1.Equals(er2);
        }

        public static bool operator !=(EntityRef er1, EntityRef er2)
        {
            if (ReferenceEquals(er1,  null)) return !ReferenceEquals(er2, null);
            return !er1.Equals(er2);
        }

    }

    public interface IEntityResolver
    {
        object Get(string reference);
        object Get(EntityRef entity);
        EntityRef GetObjectRef(object obj);
        bool KnowsEntityType(Type t);
        /// <summary>
        /// will return entity type for specified entity name,
        /// provided that it 'knows' that entity
        /// </summary>
        /// <param name="entName"></param>
        /// <returns></returns>
        Type KnowsEntityType(string entName);
        /// <summary>
        /// return dipslay label for an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string GetObjectLabel(object obj);

        IEnumerable<object> GetMany(IEnumerable<EntityRef> refs);
    }
}
