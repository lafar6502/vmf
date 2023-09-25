using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;

namespace VMF.Services.DataBinding
{
    public class EntityFieldReflection
    {

        

        

        public EntityFieldData GetFieldStatic(Type objectType, string bindingPath)
        {
            return WalkTheStaticPath(objectType, bindingPath);
        }

        /// <summary>
        /// get field value and accompanying information
        /// </summary>
        /// <param name="root"></param>
        /// <param name="bindingPath"></param>
        /// <returns></returns>
        public EntityFieldData GetField(object root, string bindingPath)
        {
            return WalkThePath(root, bindingPath);
        }

        public object GetFieldValue(object root, string bindingPath)
        {
            var fd = GetField(root, bindingPath);
            return fd == null ? null : fd.Value;
        }

        //private static Dictionary<Type, Dictionary<string, MemberInfo>> _reflectionCache = new Dictionary<Type, Dictionary<string, MemberInfo>>();
        private static ConcurrentDictionary<string, MemberInfo> _reflCache = new ConcurrentDictionary<string, MemberInfo>();
        private static MemberInfo _notFound = typeof(EntityFieldReflection);
        private static PropertyInfo GetTypeProperty(Type t, string propName)
        {
            var key = t.FullName + ":P:" + propName;
            var mi = _reflCache.GetOrAdd(key, k =>
            {
                var p = t.GetProperty(propName);
                return p ?? _notFound;
            });
            if (mi == _notFound) return null;
            return mi as PropertyInfo;
        }

        private static FieldInfo GetTypeField(Type t, string propName)
        {
            var key = t.FullName + ":F:" + propName;
            var mi = _reflCache.GetOrAdd(key, k =>
            {
                var p = t.GetField(propName);
                return p ?? _notFound;
            });
            if (mi == _notFound) return null;
            return mi as FieldInfo;
        }

        private EntityFieldData WalkThePath(object root, string bindingPath)
        {
            var idx = bindingPath.IndexOf('.');
            var cp = idx > 0 ? bindingPath.Substring(0, idx) : bindingPath;
            var remaining = idx > 0 ? bindingPath.Substring(idx + 1) : "";
            var tp = root.GetType();
            var final = string.IsNullOrEmpty(remaining);


            var ret = new EntityFieldData
            {
                Owner = root,
                Access = FieldAccess.ReadOnly,
                DeclaringType = tp,
                Name = cp
            };

            var pi = GetTypeProperty(tp, cp);
            if (pi != null)
            {
                ret.Property = pi;
                ret.Value = pi.GetValue(root);
                ret.ValueType = pi.PropertyType;
            }
            else //pi == null
            {
                var fi = GetTypeField(tp, cp);
                if (fi != null)
                {
                    ret.Property = fi;
                    ret.ValueType = fi.FieldType;
                    ret.Value = fi.GetValue(root);
                }
            }


            var exp = root as IDynamicEntity;

            //get expando either when final or when we dont have a member to check
            var haveExpandos = false;
            if (exp != null && (final || ret.Property == null))
            {
                var efd = exp.DynGetFieldData(cp, ret.Property == null ? null : ret);
                if (efd != null && !object.ReferenceEquals(ret, efd))
                {
                    haveExpandos = true;
                    ret.Access = efd.Access;
                    ret.DataSource = efd.DataSource;
                    ret.DependsOn = efd.DependsOn;
                    ret.FieldUIType = efd.FieldUIType;
                    ret.Label = efd.Label;
                    ret.Options = efd.Options;
                    ret.ValidationProblem = efd.ValidationProblem;
                    ret.Value = efd.Value;
                    ret.ValueType = efd.ValueType;
                }
            }

            if (ret.Property == null && !haveExpandos) return null;

            if (!final)
            {
                FaFieldDescriptor rest = null;
                if (ret.Value == null) //we found null, now do the static walk
                {
                    rest = WalkTheStaticPath(ret.ValueType, remaining);
                }
                else
                {
                    rest = WalkThePath(ret.Value, remaining);
                }
                return rest;
            }

            if (root is SoodaObject)
            {
                var so = (SoodaObject)root;
                var fi = so.GetClassInfo().FindFieldByName(cp);
                if (fi != null)
                {
                    if (fi.Size > 0) ret.Size = fi.Size;
                }
            }
            if (ret.ActualMember != null)
            {
                if (ret.Access == FieldAccess.Undefined)
                {
                    var accp = GetTypeProperty(tp, "FieldAccess_" + ret.ActualMember.Name);
                    if (accp != null && accp.PropertyType == typeof(FieldAccess))
                    {
                        ret.Access = (FieldAccess)accp.GetValue(root);
                    }
                }

                if (ret.Options == null)
                {
                    var optsp = GetTypeProperty(tp, ret.ActualMember.Name + "Options");
                    if (optsp != null)
                    {
                        ret.Options = () => optsp.GetValue(root) as SC.IEnumerable;
                    }
                }

                if (ret.Validation == null)
                {
                    var valp = GetTypeProperty(tp, "Validation_" + ret.ActualMember.Name);
                    if (valp != null)
                    {
                        if (valp.PropertyType == typeof(FieldValidation))
                        {
                            ret.Validation = valp.GetValue(root) as FieldValidation;
                        }
                        else if (valp.PropertyType == typeof(string))
                        {
                            var str = valp.GetValue(root) as string;
                            ret.Validation = new FieldValidation
                            {
                                IsError = !string.IsNullOrEmpty(str),
                                Message = str
                            };
                            if (ret.Validation.IsError && str.StartsWith("WARN:"))
                            {
                                ret.Validation.IsError = false;
                                ret.Validation.Message = str.Substring(5);
                            }
                        }
                    }
                }
            }

            return ret;
        }

        private FaFieldDescriptor WalkTheStaticPath(Type rootType, string bindingPath)
        {
            var idx = bindingPath.IndexOf('.');
            var cp = idx > 0 ? bindingPath.Substring(0, idx) : bindingPath;
            var remaining = idx > 0 ? bindingPath.Substring(idx + 1) : "";
            var final = string.IsNullOrEmpty(remaining);

            var ret = new FaFieldDescriptor
            {
                Owner = null,
                Access = FieldAccess.Undefined,
                DeclaringType = rootType,
                Name = cp,
                ActualMember = null
            };

            var pi = GetTypeProperty(rootType, cp);
            if (pi != null)
            {
                ret.ActualMember = pi;
                ret.Value = pi.GetGetMethod().IsStatic ? pi.GetValue(null) : null;
                ret.ValueType = pi.PropertyType;
            }
            else //pi == null
            {
                var fi = GetTypeField(rootType, cp);
                if (fi != null)
                {
                    ret.ActualMember = fi;
                    ret.ValueType = fi.FieldType;
                    ret.Value = fi.IsStatic ? fi.GetValue(null) : null;
                }
            }
            if (ret.ActualMember == null)
            {
                return null;
            }

            if (!final)
            {
                if (ret.Value != null)
                    return WalkThePath(ret.Value, remaining);
                else
                    return WalkTheStaticPath(ret.ValueType, remaining);
            }
            else //discover remaining info via reflection
            {
                var accp = GetTypeProperty(rootType, "FieldAccess_" + ret.ActualMember.Name);
                if (accp != null && accp.PropertyType == typeof(FieldAccess) && accp.GetGetMethod().IsStatic)
                {
                    ret.Access = (FieldAccess)accp.GetValue(null);
                }
                var optsp = GetTypeProperty(rootType, ret.ActualMember.Name + "Options");
                if (optsp != null && optsp.GetGetMethod().IsStatic)
                {
                    ret.Options = () => optsp.GetValue(null) as SC.IEnumerable;
                }
                var valp = GetTypeProperty(rootType, "Validation_" + ret.ActualMember.Name);
                if (valp != null && valp.GetGetMethod().IsStatic)
                {
                    if (valp.PropertyType == typeof(FieldValidation))
                    {
                        ret.Validation = valp.GetValue(null) as FieldValidation;
                    }
                    else if (valp.PropertyType == typeof(string))
                    {
                        var str = valp.GetValue(null) as string;
                        ret.Validation = new FieldValidation
                        {
                            IsError = !string.IsNullOrEmpty(str),
                            Message = str
                        };
                    }
                }
                return ret;
            }
        }
    }
}
