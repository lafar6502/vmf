using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core.Configurator
{
    /// <summary>
    /// 
    /// </summary>
    public class ParamInfo
    {
        public string ParamGroup { get; set; }
        public string Name { get; set; }
        public Type ParamType { get; set; }
        public string I18NId { get; set; }
        public FieldAccess Access { get; set; }
        /// <summary>
        /// input param, or out/calculated param
        /// </summary>
        public bool IsInput { get; set; }
        public string Label { get; set; }
        public object Value { get; set; }
        public string ValueLabel { get; set; }

        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; }

        public string[] DependsOn { get; set; }

        /// <summary>
        /// selection options
        /// </summary>
        public IEnumerable<object> Options { get; set; }

        public string DataSource { get; set; }
        public string FieldTemplate { get; set; }
        /// <summary>
        /// additional config options
        /// </summary>
        public Dictionary<string, object> UIOptions { get; set; }

        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        /// <summary>
        /// if any other fields depend on this one (helper)
        /// </summary>
        public bool HasDependants { get; set; }


    }
}
