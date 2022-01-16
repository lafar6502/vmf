using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core.Util
{
    public static class CollectionExtensions
    {
        public static V GetValueOrDefault<K,V>(this IDictionary<K, V> d, K k, V defVal = default(V))
        {
            V v;
            return d.TryGetValue(k, out v) ? v : defVal;
        }

        

        public static T GetValueOrDefault<T>(this IDictionary<string, object> d, string key, T defVal = default(T))
        {
            try
            {
                object v;
                if (!d.TryGetValue(key, out v)) return defVal;
                return TypeUtils.ConvertTo<T>(v, defVal);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting " + key + ": " + ex.Message);
            }

        }

        public static T GetValueOrDefault<T>(this Dictionary<string, object> d, string key, T defVal = default(T))
        {
            try
            {
                object v;
                if (!d.TryGetValue(key, out v)) return defVal;
                return TypeUtils.ConvertTo<T>(v, defVal);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting " + key + ": " + ex.Message);
            }

        }

    }
}
