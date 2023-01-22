using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    public class ListQuery
    {
        public string ListId { get; set; }
        public string[] SelectColumns { get; set; }
        public int Start { get; set; }
        public int Limit { get; set; }
        /// <summary>
        /// return total count
        /// </summary>
        public bool WithCount { get; set; }
        public string OrderBy { get; set; }
        public bool OrderDesc { get; set; }
        public class Filter
        {
            public string Name { get; set; }
            public string Op { get; set; }
            public JToken Args { get; set; }
        }

        public Filter[] Filters { get; set; }
    }

    public class ListQueryResults
    {
        public ListQuery Query { get; set; }
        public string[] Columns { get; set; }
        public int? TotalCount { get; set; }

        public List<Dictionary<string, object>> Results { get; set; }
        public bool HasMore { get; set; }
    }
    public interface IListDataProvider : IListProvider
    {
        ListQueryResults Query(ListQuery q);
        int GetRowCount(ListQuery q);
    }
}
