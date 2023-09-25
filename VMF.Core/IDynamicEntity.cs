using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SC = System.Collections;

namespace VMF.Core
{
    public class EntityFieldData
    {
        public string Name { get; set; }
        public Type ValueType { get; set; }
        public object Value { get; set; }
        public FieldAccess Access { get; set; }

        public Func<SC.IEnumerable> Options { get; set; }

        //ui template for the field,,null for default
        public string FieldUIType { get; set; }
        //data source for autocompletee/search fields
        public string DataSource { get; set; }
        /// <summary>
        /// field label, optional
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// list of field dependencies (used only for autocomplete so far)
        /// </summary>
        public IEnumerable<string> DependsOn { get; set; }
        /// <summary>
        /// field validation info, can be null
        /// </summary>
        public string ValidationProblem { get; set; }

        /// <summary>
        /// field size, for example string max length
        /// </summary>
        public int? Size { get; set; }
        /// <summary>
        /// property/field on the entity type, or null if the field is dynamic
        /// </summary>
        public MemberInfo Property { get; set; }
        //min value for numeric fields
        public decimal? MinVal { get; set; }
        /// <summary>
        /// max value for numeric fields
        /// </summary>
        public decimal? MaxVal { get; set; }

        public object Owner { get; set; }

        public Type DeclaringType { get; set; }
    }
    public interface IDynamicEntity
    {
        /// <summary>
        /// assign value from json (for handling)
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        object DynSetFieldFromJson(string fieldName, JToken value);
        /// <summary>
        /// return list of all expando field names
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> DynGetFieldNames();
        /// <summary>
        /// return null if field is not expando
        /// If you return any data from this function then 
        /// this information will be used, even if there are
        /// actual properties on the entity. So ExpandoGetFieldData overrides
        /// the static properties.
        /// But when setting the property value, first we look at the properties
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultDescriptor">information about field already found, or null if there is such field (then only expando)</param>
        /// <returns></returns>
        EntityFieldData DynGetFieldData(string name, EntityFieldData defaultDescriptor);
        object DynGetValue(string name);
        void DynSetValue(string name, object value);
        /// <summary>
        /// list of all action names provided as an expando
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> DynGetActions();
        /// <summary>
        /// check if action is enabled. If this is not an expando action this function should return null.
        /// Can also be used for disabling actions that aren't expando (then return true or false)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool? DynIsActionEnabled(string name);
        /// <summary>
        /// return expando action instance
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ObjectActionBase DynGetAction(string name);
    }
}
