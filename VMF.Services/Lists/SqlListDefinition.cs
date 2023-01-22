using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Services.Lists
{
    public class SqlListDefinition
    {
        public string Id { get; set; }
        public class Column
        {
            public string Name { get; set; }
            public string Expr { get; set; }
            public bool Sortable { get; set; }

            public string DataType { get; set; }

            public string Permissions { get; set; }

        }

        public class FilterParam
        {
            public string Name { get; set; }
            public string Expr { get; set; }
            public bool Required { get; set; }
            public object DefaultValue { get; set; }

            public Dictionary<string, string> FilterMap { get; set; }
        }

        public Column[] Columns { get; set; }

        /// <summary>
        /// query array to make it easier to put lines in json
        /// </summary>
        public string[] Query { get; set; }

        public FilterParam[] SearchFilters { get; set; }

        public class SecFilter
        {
            public string Permission { get; set; }
            public string Query { get; set; }
        }

        public SecFilter[] SecurityFilters { get; set; }

    }
}
