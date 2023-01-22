using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    public static class MiscExtensions
    {
        public static string LimitLength(this string s, int maxLen)
        {
            if (s == null || s.Length <= maxLen) return s;
            return s.Substring(0, maxLen);
        }


        //util function for batching iteration
        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(
                  this IEnumerable<TSource> source, int size)
        {
            TSource[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new TSource[size];

                bucket[count++] = item;
                if (count != size)
                    continue;

                yield return bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null && count > 0)
                yield return bucket.Take(count);
        }

        public static IEnumerable<IEnumerable<TSource>> EqualBatches<TSource>(
            this IEnumerable<TSource> source, int maxBatchSize, int maxBatches, bool roundRobin
            )
        {
            var c0 = source.ToList();
            var nb = maxBatches;
            if (nb > c0.Count) nb = c0.Count;

            var nb2 = (int)Math.Ceiling((double)c0.Count / (double)maxBatchSize);

            var rez = new List<TSource>[nb];
            for (var i = 0; i < rez.Length; i++)
            {
                rez[i] = new List<TSource>();
            }

            for (var i = 0; i < c0.Count; i++)
            {
                var idx = nb * i / c0.Count;
                if (roundRobin)
                {
                    idx = i % nb;
                }
                rez[idx].Add(c0[i]);
            }
            return rez;
        }

        public static string FirstLine(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            var newLinePos = str.IndexOf(Environment.NewLine, StringComparison.CurrentCulture);
            return newLinePos > 0 ? str.Substring(0, newLinePos) : str;
        }
        /// <summary>
        /// number of lines in the string
        /// empty str - 0 lines
        /// last empty line doesn't count.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int LineCount(this string str)
        {
            var sep = "\n";
            int cnt = 0;
            int idx = 0;
            while (idx < str.Length)
            {
                idx = str.IndexOf(sep, idx);
                cnt++;
                if (idx < 0) break;
                idx += sep.Length;
            }
            return cnt;
        }

        public static IEnumerable<Type> WithBaseTypes(this Type t)
        {
            return WithBaseTypes(t, null);
        }

        public static IEnumerable<Type> WithBaseTypes(this Type t, Type untilType)
        {
            do
            {
                yield return t;
                t = t.BaseType;
            }
            while (t != null && t.BaseType != null && t != typeof(object) && t != untilType);
        }

        /// <summary>
        /// check if object represents a number
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNumeric(object obj)
        {
            return (obj == null) ? false : IsNumeric(obj.GetType());
        }

        /// <summary>
        /// nullable type - nullable<sth>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsNullable(this Type t)
        {
            if (!t.IsValueType) return true; //refereences are nullable 
            return Activator.CreateInstance(t) == null; //i dont know if its ever possible
        }
        /// <summary>
        /// check if type represents a number
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNumeric(Type type)
        {
            if (type == null)
                return false;

            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }
            return false;
        }

        public static bool IsSimpleType(Type t)
        {
            return t.IsPrimitive || t == typeof(decimal) || t == typeof(String) || t == typeof(DateTime);
        }


        public static string GetTypeName(Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.String:
                    return "string";
                case TypeCode.Int32:
                    return "int";
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Double:
                    return "double";
                case TypeCode.Decimal:
                    return "decimal";
                case TypeCode.Int64:
                    return "long";
                case TypeCode.DateTime:
                    return "datetime";
            }
            return t.FullName;
        }

        private static Dictionary<string, Type> _simpleTypez = new Dictionary<string, Type>() {
            {"string", typeof(string) },
            {"int", typeof(int) },
            {"integer", typeof(int) },
            {"bool", typeof(bool) },
            {"double", typeof(double) },
            {"decimal", typeof(decimal) },
            {"long", typeof(long) },
            {"datetime", typeof(DateTime) }
        };

        private static Dictionary<string, Type> _foundTypes = new Dictionary<string, Type>();
        public static Type GetTypeByName(string s)
        {
            Type tp;
            if (_simpleTypez.TryGetValue(s.ToLower(), out tp)) return tp;
            var t0 = Type.GetType(s);
            if (t0 != null) return t0;
            lock (_foundTypes)
            {
                if (_foundTypes.ContainsKey(s))
                {
                    return _foundTypes[s];
                }
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic))
            {
                var n = asm.GetName();
                if (n != null && !string.IsNullOrEmpty(n.Name) && s.StartsWith(n.Name + "."))
                {
                    t0 = Type.GetType(s + ", " + n.Name);
                    break;
                }
            }
            lock (_foundTypes)
            {
                _foundTypes[s] = t0;
            }
            return t0;
        }

        public static string Replace(this string s, char[] replaceChars, char replaceWith)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                sb.Append(replaceChars.Contains(c) ? replaceWith : c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// sql day number (1:sunday... 7:saturday)
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public static int GetSqlWeekday(DayOfWeek w)
        {
            return ((int)w) + 1;
        }

        /// <summary>
        /// calculate .net day of week from sql weekday number
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static DayOfWeek GetOfWeekFromSql(int n)
        {
            return (DayOfWeek)(n - 1);
        }
        public static int GetWeekOfYear(this DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            //DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            //if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            //{
            //    time = time.AddDays(3);
            //}

            // Return the week of our adjusted day
            var wn = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
            System.Diagnostics.Debug.Assert(wn >= 1 && wn <= 53); //53 is returned only if the week number should be 1 (2014.12.31 for example)

            return wn;
        }

        public static DateTime SetDayOfWeek(this DateTime dt, DayOfWeek dw)
        {
            var d0 = dt.DayOfWeek;
            return dt.AddDays(dw - d0);
        }

        /// <summary>
        /// calculate age in full years
        /// </summary>
        /// <param name="birthday"></param>
        /// <param name="today"></param>
        /// <returns></returns>
        public static int AgeInYears(DateTime birthday, DateTime today)
        {
            return ((today.Year - birthday.Year) * 372 + (today.Month - birthday.Month) * 31 + (today.Day - birthday.Day)) / 372;
        }





        public static string Coalesce(params string[] values)
        {
            return values.FirstOrDefault(x => !string.IsNullOrEmpty(x));
        }

        /// <summary>
        /// check if decimal is an integer
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool IsIntegerNumber(this decimal d)
        {
            return d % 1.0m == 0m;
        }




        public static IEnumerable<Type> MeAndParentTypes(this Type t)
        {
            while (t != null && t != typeof(object))
            {
                yield return t;
                t = t.BaseType;
            }
        }

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

        public static T GetValueOrDefault<T>(this IDictionary<string, object> d, string key, T defVal = default(T))
        {
            try
            {
                object v;
                if (!d.TryGetValue(key, out v)) return defVal;
                return ConvertTo<T>(v, defVal);
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
                return ConvertTo<T>(v, defVal);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting " + key + ": " + ex.Message);
            }

        }

        /// <summary>
        /// update records in this collection by retrieving data from c2 and executing an update on records matched (by key)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <param name="c2"></param>
        /// <param name="key"></param>
        /// <param name="update"></param>
        public static void MergeData<T>(this IEnumerable<T> c, IEnumerable<T> c2, Func<T, object> key, Action<T, T> update)
        {
            var d2 = c2.ToDictionary(key);
            T tz;
            foreach (var x in c)
            {
                var k = key(x);
                update(x, d2.TryGetValue(k, out tz) ? tz : default(T));
            }
        }

        /// <summary>
        /// if type is a collection (array or IEnumerable<of T>) return the type of collection element.
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
