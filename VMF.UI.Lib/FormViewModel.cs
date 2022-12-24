using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.UI.Lib
{
    public class FormViewModel
    {
        /// <summary>
        /// scope id (form id)
        /// </summary>
        public string FormScopeId { get; set; }

        public Dictionary<string, object> Values { get; set; }

        public Dictionary<string, WidgetContext> Contexts { get; set; }

        /// <summary>
        /// mapping: object ref => alias (part of name of the value in Values list)
        /// [alias]$[name], or just name for RootRef
        /// </summary>
        public Dictionary<string, string> Aliases { get; set; }
        /// <summary>
        /// view's root object ref
        /// </summary>
        public string RootRef { get; set; }

        /// <summary>
        /// transsaction id when generating the view
        /// </summary>
        public string Tid { get; set; }

        /// <summary>
        /// view contains dirty (unsaved) data
        /// </summary>
        public bool Dirty { get; set; }
    }
}
