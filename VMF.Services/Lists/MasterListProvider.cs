using NGinnBPM.MessageBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;

namespace VMF.Services.Lists
{
    public class MasterListProvider : IListDataProvider
    {
        public IServiceResolver Resolver { get; set; }

        private IListDataProvider GetProvider(string name)
        {
            return Resolver.GetInstance<IListDataProvider>(name);
        }

        protected IListDataProvider GetProvider(string listId, out string providerListId)
        {
            var idx = listId.IndexOf('.');
            if (idx <= 0) throw new Exception("ListId invalid:" + listId);
            var pname = listId.Substring(0, idx);
            providerListId = listId.Substring(idx + 1);
            return GetProvider(pname);
        }

        ListInfo IListProvider.GetList(string listId)
        {
            var idx = listId.IndexOf('.');
            if (idx <= 0) throw new Exception("ListId invalid:" + listId);
            var pname = listId.Substring(0, idx);
            var id2 = listId.Substring(idx + 1);
            var prov = GetProvider(pname);
            var l = prov.GetList(id2);
            l.ListId = pname + "." + l.ListId;
            return l;
        }

        int IListDataProvider.GetRowCount(ListQuery q)
        {
            string id2;
            var prov = GetProvider(q.ListId, out id2);
            q.ListId = id2;
            return prov.GetRowCount(q);
        }

        ListQueryResults IListDataProvider.Query(ListQuery q)
        {
            string id2;
            var prov = GetProvider(q.ListId, out id2);
            q.ListId = id2;
            return prov.Query(q);
        }
    }
}
