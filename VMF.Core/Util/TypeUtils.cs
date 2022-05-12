using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core.Util
{
    public static class TypeUtils
    {
        public static object ConvertTo(object v, Type destType)
        {
            if (v == null) return null;
            if (v == DBNull.Value) return null;
            if (destType.IsAssignableFrom(v.GetType())) return v;

            if (destType.IsArray && v.GetType().IsArray)
            {
                Array arr = (Array)v;
                var arr2 = Array.CreateInstance(destType.GetElementType(), arr.Length);

                if (destType.GetElementType().IsAssignableFrom(v.GetType().GetElementType()))
                {
                    Array.Copy(arr, arr2, arr.Length);
                }
                else
                {
                    for (var i = 0; i < arr.Length; i++)
                    {
                        var v2 = ConvertTo(arr.GetValue(i), destType.GetElementType());
                        arr2.SetValue(v2, i);
                    }
                }
                return arr2;
            }
            var t2 = Nullable.GetUnderlyingType(destType);
            if (t2 != null) return Convert.ChangeType(v, t2);
            if (v != null && v is JToken)
            {
                return ((JToken)v).ToObject(destType);
            }
            return Convert.ChangeType(v, destType);
        }

        public static T ConvertTo<T>(object v, T defVal)
        {
            if (v == null) return defVal;
            if (v == DBNull.Value) return defVal;
            return (T)ConvertTo(v, typeof(T));
        }

        /// <summary>
        /// type and all its base types, except of typeof(object)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<Type> WithBaseTypes(this Type t)
        {
            Type t0 = t;
            while(t0 != null && t0 != typeof(object))
            {
                yield return t0;
                t0 = t0.BaseType;
            }
        }

        /// <summary>
        /// type of element in a generic collection or array
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetCollectionElementType(Type type)
        {
            // Type is Array
            // short-circuit if you expect lots of arrays 
            if (type.IsArray)
                return type.GetElementType();

            // type is IEnumerable<T>;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            // type implements/extends IEnumerable<T>;
            var enumType = type.GetInterfaces()
                                    .Where(t => t.IsGenericType &&
                                           t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                    .Select(t => t.GetGenericArguments()[0]).FirstOrDefault();
            if (enumType != null) return enumType;
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type)) return typeof(object);
            return null;
        }
    }
}
