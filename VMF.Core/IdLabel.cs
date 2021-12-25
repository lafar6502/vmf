using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    public class IdLabel
    {
        public object Id { get; set; }
        public string Label { get; set; }
    }

    public class IdLabelCss : IdLabel
    {
        public string Style { get; set; }
        public string Description { get; set; }
    }

    public class IdLabelQty : IdLabelCss
    {
        public decimal Qty { get; set; }
    }
}
