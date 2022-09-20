using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
namespace VMF.Core
{
    public static class JsonNetExtensions
    {
        public static void AssignTo(this JObject me, object target)
        {
            foreach (var p in me)
            {
                var pi = target.GetType().GetProperty(p.Key);
                if (pi != null) pi.SetValue(target, p.Value.ToObject(pi.PropertyType), null);
            }
        }

        public static void AssignTo(this JObject me, object target, JsonSerializer ser)
        {
            foreach (var p in me)
            {
                var pi = target.GetType().GetProperty(p.Key);
                if (pi != null) pi.SetValue(target, p.Value.ToObject(pi.PropertyType, ser), null);
            }
        }


        public static Type ClrDataType(this JTokenType t)
        {
            switch(t)
            {
                case JTokenType.String:
                    return typeof(string);
                case JTokenType.Boolean:
                    return typeof(bool);
                case JTokenType.Bytes:
                    return typeof(byte[]);
                case JTokenType.Date:
                    return typeof(DateTime);
                case JTokenType.Float:
                    return typeof(Double);
                case JTokenType.Integer:
                    return typeof(int);
                case JTokenType.Guid:
                    return typeof(Guid);
                case JTokenType.Array:
                    return typeof(object[]);
                case JTokenType.Object:
                    return typeof(JObject);
                case JTokenType.TimeSpan:
                    return typeof(TimeSpan);
                case JTokenType.Null:
                case JTokenType.Undefined:
                default:
                    return typeof(object);
            }
        }

        public static object ClrValue(this JToken t)
        {
            if (t == null) return null;
            switch (t.Type)
            {
                case JTokenType.String:
                    return t.Value<string>();
                case JTokenType.Boolean:
                    return t.Value<bool>();
                case JTokenType.Bytes:
                    return t.Value<byte[]>();
                case JTokenType.Date:
                    return t.Value<DateTime>();
                case JTokenType.Float:
                    return t.Value<Double>();
                case JTokenType.Integer:
                    return t.Value<int>();
                case JTokenType.Guid:
                    return t.Value<Guid>();
                case JTokenType.Array:
                    return ((JArray)t).Select(x => ClrValue(x)).ToArray();
                case JTokenType.Object:
                    return (JObject)t;
                case JTokenType.TimeSpan:
                    return t.Value<TimeSpan>();
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;
                default:
                    return t;
            }
        }
    }
}
