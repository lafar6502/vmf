using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;

namespace VMF.UI.Lib
{
    public class FormViewModel
    {
        /// <summary>
        /// scope id (form id)
        /// </summary>
        public string FormScopeId { get; set; }

        public Dictionary<string, object> Values { get; set; } =  new Dictionary<string, object>();

        public Dictionary<string, WidgetContext> Context { get; set; } = new Dictionary<string, WidgetContext>();

        /// <summary>
        /// mapping: object ref => alias (part of name of the value in Values list)
        /// [alias]$[name], or just name for RootRef
        /// </summary>
        public List<string> Refs { get; set; } = new List<string>();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns>fieldId - name with alias in the form _N$fieldName</returns>
        public string AddEntityFieldValue(EntityRef entity, string name, object value)
        {
            string alias = "";
            if (entity != null)
            {
                var s = entity.ToString();
                if (RootRef == null || s != RootRef)
                {
                    var ix = Refs.IndexOf(s);
                    if (ix < 0)
                    {
                        Refs.Add(s);
                        ix = Refs.Count - 1;
                    }
                    alias = "_" + ix + "$";
                }
            }
            var n2 = alias + name;
            Values.Add(n2, value);
            return n2;
        }

        /// <summary>
        /// add field/widget context
        /// </summary>
        /// <param name="wc"></param>
        /// <param name="id">optional context Id. If null, new id will be created and returned.</param>
        /// <returns></returns>
        public string AddWidgetContext(WidgetContext wc, string id = null)
        {
            var baseId = id ?? "ctrl_";
            int cnt=1;
            if (Context.ContainsKey(baseId))
            {
                while(true)
                {
                    var k = baseId + (cnt++);
                    if (!Context.ContainsKey(k))
                    {
                        id = k;
                        break;
                    }
                }
            }
            Context[id] = wc;
            return id;
        }
    }
}
