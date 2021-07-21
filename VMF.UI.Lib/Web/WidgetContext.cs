using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.UI.Lib.Web
{
    public class WidgetContext
    {
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string Access { get; set; }
        public string I18Id { get; set; }
        public string Label { get; set; }
        public string[] DependsOn { get; set; }

        public string Template { get; set; }


    }
}
