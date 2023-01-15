using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    public class ListInfo
    {
        public string ListId { get; set; }

        public class Column
        {
            public string Name { get; set; }
            public string Label { get; set; }
            public string DataType { get; set; }
            public bool Sortable { get; set; }
            public bool Visible { get; set; }
        }

        public List<Column> Columns { get; set; } = new List<Column>();

        public string KeyField { get; set; }
        public bool CountSupported { get; set; }

        
    }
}
