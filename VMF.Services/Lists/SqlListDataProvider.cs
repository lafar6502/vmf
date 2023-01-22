using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;
using System.IO;
using Newtonsoft.Json;
using System.Data.Common;
using NLog;
using System.Data;
using Dapper;
using VMF.Core.Util;
using VMF.Core.Lists;

namespace VMF.Services.Lists
{
    public class SqlListDataProvider : IListDataProvider, IListProvider
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public string BaseDir { get; set; }
        public string OverrideDir { get; set; }

        public IQueryParamProvider ParamSupplier { get; set; }

        public ListInfo GetList(string listId)
        {
            var ld = GetListDef(listId);
            var lc = new ListInfo
            {
                ListId = ld.Id,
                CountSupported = true,
                KeyField = "_primaryKey",
                Columns = ld.Columns.Select(x => new ListInfo.Column
                {
                    Name = x.Name,
                    Sortable = x.Sortable,
                    DataType = "string"
                }).ToList()
            };
            return lc;
        }

        public int GetRowCount(ListQuery q)
        {
            throw new NotImplementedException();
        }
        public ListQueryResults Query(ListQuery q)
        {
            var db = SessionContext.Current.RequestDbConnection;
            var ld = GetListDef(q.ListId);
            DynamicParameters dp = new DynamicParameters();
            dp.Add("userId", AppUser.Current == null ? (int?)null : AppUser.Current.Id);
            dp.Add("appProfile", VMFGlobal.AppProfile);
            var qparams = new List<object>();
            var baseSql = BuildQuery(ld, q, qparams);
            for (var i = 0; i < qparams.Count; i++)
            {
                dp.Add("qp" + i, qparams[i]);
            }

            var hs = new HashSet<string>(dp.ParameterNames);
            baseSql = StringUtil.SubstDapperParams(baseSql, pn =>
            {
                if (hs.Contains(pn))
                {
                    return "@" + pn;
                }
                else
                {
                    object pv = null;
                    if (ParamSupplier != null)
                    {
                        pv = ParamSupplier.GetQueryParam(pn);
                    }
                    if (pv == null) throw new Exception("Query param missing:" + pn);
                    dp.Add(pn, pv);
                    hs.Add(pn);
                    return "@" + pn;
                }
            });
            var qr = new ListQueryResults
            {
                Query = q,
                Results = new List<Dictionary<string, object>>()
            };
            if (q.Limit > 0)
            {
                var oc = ld.Columns.First();
                if (q.OrderBy != null) oc = ld.Columns.FirstOrDefault(x => x.Name == q.OrderBy);
                if (oc == null) throw new Exception("Order column invalid");
                var orderBy = (string.IsNullOrEmpty(oc.Expr) ? oc.Name : oc.Expr) + " " + (q.OrderDesc ? "desc" : "asc");

                var sql = baseSql + " ORDER BY " + orderBy;
                sql += " \r\nOFFSET @__offset ROWS FETCH NEXT @__limit ROWS ONLY";
                dp.Add("__offset", q.Start);
                dp.Add("__limit", q.Limit+1);
                try
                {
                    using (var dr = db.ExecuteReader(sql, dp))
                    {
                        while (dr.Read())
                        {
                            var row = new Dictionary<string, object>();
                            if (qr.Columns == null)
                            {
                                qr.Columns = new string[dr.FieldCount];
                                for (int i = 0; i < dr.FieldCount; i++)
                                {
                                    qr.Columns[i] = dr.GetName(i);
                                }
                            }
                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                row[qr.Columns[i]] = dr[i];
                            }
                            if (qr.Results.Count < q.Limit)
                            {
                                qr.Results.Add(row);
                            }
                            else
                            {
                                qr.HasMore = true;
                                break;
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    log.Error("Query error in list {0}, query {1}: {2}", ld.Id, sql, e.ToString());
                    throw;
                }
                
            }
            if (q.WithCount)
            {
                if (q.Limit > 0 && !qr.HasMore)
                {
                    qr.TotalCount = qr.Results.Count;
                }
                else
                {
                    var countQry = "select count(*) from (" + baseSql + ")";
                    qr.TotalCount = db.QuerySingle<int>(countQry, dp);
                }
            }
            return qr;
        }

        protected string BuildQuery(SqlListDefinition ld, ListQuery q, List<object> qparams)
        {
            var qry = string.Join("\r\n", ld.Query);

            var usedParams = new HashSet<string>();
            var remParamsKey = "####--REMPARAMS--####";

            //supply param values and sql fragments

            var processedFilters = new Dictionary<string, string>();

            IEnumerable<SqlListDefinition.Column> selectedCols = q.SelectColumns == null ? ld.Columns : ld.Columns.Where(x => q.SelectColumns.Contains(x.Name));
            

            var sb = new StringBuilder();
            foreach (var c in selectedCols)
            {
                if (sb.Length > 0) sb.Append(", ");
                if (!string.IsNullOrEmpty(c.Expr))
                {
                    sb.Append(c.Expr + " as " + c.Name);
                }
                else
                {
                    sb.Append(c.Name);
                }
            }
            processedFilters["_columns"] = sb.ToString();
            var secFilter = "(1=1)";
            if (ld.SecurityFilters != null)
            {
                secFilter = "(1=0)";
                foreach (var filter in ld.SecurityFilters)
                {
                    if (AppUser.Current.HasPermission(filter.Permission))
                    {
                        secFilter = filter.Query;
                        break;
                    }
                }
            }
            processedFilters["_securityFilters"] = secFilter;
            var hasSecurity = false;
            qry = StringUtil.SubstValues(qry, pn =>
            {
                switch (pn)
                {
                    case "_columns":
                        return processedFilters["_columns"];
                    case "_remainingFilters":
                        return remParamsKey; //replace laterr
                    case "_securityFilters":
                        hasSecurity = true;
                        return processedFilters["_securityFilters"];
                    default:
                        //provide filters
                        var f0 = q.Filters.FirstOrDefault(x => x.Name == pn);
                        return BuildFilterExpr(ld, pn, f0, processedFilters, qparams);
                }
            });

            var sb2 = new StringBuilder();
            if (!hasSecurity) sb2.Append(secFilter);
            var remaining = q.Filters.Where(x => !processedFilters.ContainsKey(x.Name));
            foreach(var f in remaining)
            {
                var lf = ld.SearchFilters.FirstOrDefault(x => x.Name == f.Name);
                if (lf == null) continue; //ignore not declared
                var expr = BuildFilterExpr(ld, lf.Name, f, processedFilters, qparams);
                if (sb2.Length > 0) sb2.Append(" and ");
                sb2.Append("(" + expr + ")");
            }
            foreach(var f2 in ld.SearchFilters.Where(x => x.Required && !processedFilters.ContainsKey(x.Name)))
            {
                var expr = BuildFilterExpr(ld, f2.Name, null, processedFilters, qparams);
                if (sb2.Length > 0) sb2.Append(" and ");
                sb2.Append("(" + expr + ")");
            }

            var remFilter = "";
            if (sb2.Length > 0)
            {
                remFilter = StringUtil.SubstValues(sb2.ToString(), pn =>
                {
                    var f0 = q.Filters.FirstOrDefault(x => x.Name == pn);
                    return BuildFilterExpr(ld, pn, f0, processedFilters, qparams);
                });
            }
            qry = qry.Replace(remParamsKey, remFilter);

            return qry;
        }

        protected string ProcessQueryFragment(string qry, ListQuery q, SqlListDefinition def, List<object> qparams, IDictionary<string, string> processedParams)
        {
            return qry;
        }

        protected string BuildFilterExpr(SqlListDefinition ld, string filterName, ListQuery.Filter val, IDictionary<string, string> processedFilters, List<object> qparams)
        {
            var f = ld.SearchFilters.FirstOrDefault(x => x.Name == filterName);
            if (f == null) throw new Exception("Query filter not found:" + filterName);
            var f0 = val;
            object fval = null;
            if (f0 != null)
            {
                fval = f0.Args.ToObject<object>();
            }
            else
            {
                fval = f.DefaultValue;
            }
            if (fval == null) throw new Exception("Filter param missing: " + filterName);
            var expr = f.Expr;
            if (f.FilterMap != null)
            {
                var e2 = f.FilterMap.GetValueOrDefault(fval.ToString(), (string)null);
                if (e2 != null) expr = e2;
            }
            if (expr == null) throw new Exception("No filter query for " + filterName + ", " + fval);
            //now we need to supply param values ..
            var e3 = processedFilters.GetOrAdd(filterName, pn0 => FixParameterRefs(expr, fval, qparams));
            return e3;
        }

        protected string FixParameterRefs(string filterExpr, object paramVal, List<object> qparams)
        {
            var dic = new Dictionary<string, string>();
            var e2 = StringUtil.SubstValues(filterExpr, ppn =>
            {
                var pv = paramVal;
                switch(ppn)
                {
                    case "0":
                        break;
                    case "0%":
                        var spv = paramVal.ToString();
                        pv = spv.EndsWith("%") ? spv : spv + "%";
                        break;
                    case "%0%":
                        spv = paramVal.ToString();
                        pv =  (spv.StartsWith("%") ? spv : "%" + spv) +  (spv.EndsWith("%") ? "" : "%");
                        break;
                    default:
                        return "${" + ppn + "}";
                }
                var res = dic.GetOrAdd(ppn, _ =>
                {
                    qparams.Add(pv);
                    return "@qp" + (qparams.Count - 1);
                });
                return res;
            });
            return e2;
        }

        protected SqlListDefinition GetListDef(string id)
        {
            foreach (var p in new string[] { OverrideDir, BaseDir })
            {
                if (string.IsNullOrEmpty(p)) continue;
                var s0 = Path.Combine(p, id + ".json");
                if (File.Exists(s0))
                {
                    var lst = JsonConvert.DeserializeObject<SqlListDefinition>(File.ReadAllText(s0));
                    lst.Id = Path.GetFileNameWithoutExtension(s0);
                    return lst;
                }
            }
            return null;
        }

        protected void AccessDb(Action<IDbConnection> act)
        {
            if (SessionContext.Current == null) throw new Exception("Need sesssion context");
            if (SessionContext.Current.RequestDbConnection == null) throw new Exception("Need RequestDbConnection");
            var sc = SessionContext.Current.RequestDbConnection;
            act(sc);
        }


    }
}
