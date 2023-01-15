using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;

namespace VMF.UI.Lib
{
    public class WidgetContext
    {
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public FieldAccess Access { get; set; }
        public string I18Id { get; set; }
        public string Label { get; set; }
        public string[] DependsOn { get; set; }
        /// <summary>
        /// field rendering template
        /// </summary>
        public string Template { get; set; }

        public string DataSource { get; set; }

        /// <summary>
        /// field options. Usually list of string values or Id/Label pairs (IdLabel)
        /// </summary>
        public IEnumerable<object> Options { get; set; }
        /// <summary>
        /// client-side UI options (for custom options)
        /// </summary>
        public IDictionary<string, object> UIOptions { get; set; }
    }
}
