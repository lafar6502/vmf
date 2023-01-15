using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VMF.Core;
using SC = System.Collections;

namespace VMF.Services.Entities
{
    public class EntityUpdateHandler
    {
        public IEntityResolver EntityResolver { get; set; }
        public object Update(string entityRef, JObject data)
        {
            var ent = EntityResolver.Get(entityRef);
            Update(ent, data);
            return ent;
        }

        public void Update(object entity, JObject updateData)
        {
            var tp = entity.GetType();
            var exp = entity as IDynamicEntity;
            foreach(var kv in updateData)
            {
                var pi = tp.GetProperty(kv.Key);
                if (pi != null)
                {
                    SetValueFromJson(entity, pi, kv.Value);
                }
                else if (exp != null)
                {
                    exp.DynSetFieldFromJson(kv.Key, kv.Value);
                }
                else throw new Exception("Dont know how to set: " + kv.Key);
            }
        }

        protected object SetValueFromJson(object obj, PropertyInfo pi, JToken val)
        {
            object pval = null;
            if (val != null)
            {
                pval = ConvertValue(val, pi.PropertyType, this.EntityResolver);
                if (pi.SetMethod == null)
                { //no setter - maybe a collection then??
                    var enu = pi.PropertyType.GetInterfaces().Where(x => x.IsGenericType)
                            .FirstOrDefault(x => x.GetGenericTypeDefinition() == typeof(ICollection<>));
                    if (enu != null)
                    {
                        var curVal = pi.GetValue(obj);
                        if (EntityResolver.KnowsEntityType(enu.GetGenericArguments().FirstOrDefault()))
                        {
                            UpdateCollection(curVal, pval);
                        }
                        else
                        {
                            UpdateCollection(curVal, pval);
                        }
                        return pval;
                    }
                    else
                    {
                        //log.Warn("Property setter not found {0}.{1}", pi.DeclaringType.Name, pi.Name);
                        //no setter - so we dont set.
                        return null;
                    }
                }

            }
            pi.SetValue(obj, pval);
            return pval;
        }

        private void UpdateCollection(dynamic destination, dynamic source)
        {
            SC.IEnumerable enu = destination as SC.IEnumerable;
            List<object> l = new List<object>(enu.Cast<object>());
            foreach (var obj in source)
            {
                if (l.Contains(obj))
                {
                    l.Remove(obj);
                }
                else
                {
                    destination.Add(obj);
                }
            }
            var mi = ((object)destination).GetType().GetMethod("Remove");
            foreach (var obj in l)
            {
                //destination.Remove(obj);
                mi.Invoke((object)destination, new object[] { obj });
            }
        }



        public static object ConvertValue(JToken value, Type destType, IEntityResolver entityResolver)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                return null;
            }
            if (destType.IsValueType || destType == typeof(string))
            {
                return value.ToObject(destType);
            }

            if (entityResolver.KnowsEntityType(destType))
            {
                EntityRef er = null;
                //value is a reference? means string -> entity ref
                if (value is JObject)
                {
                    er = value.ToObject<EntityRef>();
                    if (string.IsNullOrEmpty(er.Entity))
                    {//special treatment, sometimes sb puts everything into the Id (IdLabel with entityref)
                        EntityRef er2;
                        if (!string.IsNullOrEmpty(er.Id) && EntityRef.TryParse(er.Id, out er2) && !string.IsNullOrEmpty(er2.Id))
                        {
                            er = er2;
                        }
                        else
                        {
                            er.Entity = destType.Name;
                        }
                    }
                    return entityResolver.Get(er);
                }
                else if (value is JValue)
                {
                    JValue jv = (JValue)value;
                    var jvs = jv.Value.ToString();
                    if (string.IsNullOrEmpty(jvs)) return null;
                    if (jv.Value.GetType().IsValueType)
                    {
                        return entityResolver.Get(new EntityRef(destType.Name, jvs));
                    }
                    else if (jv.Value is string)
                    {
                        var eref = EntityRef.Parse(jvs);
                        int k;
                        if (string.IsNullOrEmpty(eref.Id) && Int32.TryParse(eref.Entity, out k))
                        {
                            eref = new EntityRef(destType.Name, k.ToString());
                        }
                        return entityResolver.Get(eref);
                    }
                    else throw new Exception();
                }
                else throw new Exception();
            }

            if (value is JObject && destType.IsGenericType && destType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                return value.ToObject(destType);
            }
            if (destType.IsArray)
            {
                var jarr = value as JArray;
                if (jarr == null) throw new Exception("Expected array  ");
                var at = destType.GetElementType();
                SC.ArrayList al = new SC.ArrayList();
                foreach (JToken jt2 in jarr)
                {
                    al.Add(ConvertValue(jt2, at, entityResolver));
                }
                var val = al.ToArray(at);
                return val;
            }

            if (value is JObject && destType == typeof(Dictionary<string, object>))
            {
                return value.ToObject(destType);
            }
            else
            {
                var itfs = destType.GetInterfaces().Union(new Type[] { destType });
                var enu = itfs.Where(x => x.IsGenericType)
                            .FirstOrDefault(x => x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (enu != null)
                {
                    var elemType = enu.GetGenericArguments().FirstOrDefault();
                    Type listType = typeof(List<>).MakeGenericType(new[] { elemType });
                    SC.IList list = (SC.IList)Activator.CreateInstance(listType);
                    var jarr = value as JArray;
                    if (jarr == null) throw new Exception("Expected array");
                    foreach (var x in jarr)
                    {
                        list.Add(ConvertValue(x, elemType, entityResolver));
                    }
                    return list;
                }
                else
                {
                    return value.ToObject(destType);
                }
            }
        }
    }
}
