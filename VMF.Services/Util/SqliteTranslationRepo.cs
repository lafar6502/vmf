using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using Dapper;
using VMF.Core;
using System.IO;
using VMF.Core.Util;

namespace VMF.Services.Util
{

    /// <summary>
    /// how does it work
    /// we have main repo and customer/profile specific repo overrides
    /// what about user-editable data? 
    /// should we also have a repo for that? or is it customer/profile specific?
    /// and how do we handle patching/versioning?
    /// 
    /// take text file from git, load into the database
    /// update database in runtime (overrides..)
    /// dump new text file with changes
    /// </summary>
    public class SqliteTranslationRepo : ITextTranslation
    {
        public string DbFile { get; set; }
        private Dictionary<string, string> _cache = null;
        private DateTime _lastRead = DateTime.MinValue;
        /// <summary>
        /// </summary>
        const string dbinit = @"create table Translation(Id nvarchar(200), Lang nchar(2), Level int not null, Txt TEXT, PRIMARY KEY(Id, Lang, Level));";

        public IEnumerable<string> Languages => throw new NotImplementedException();

        public event Action<ITextTranslation> TranslationsChanged;

        private static string Key(string id, string lang)
        {
            return id + ":" + lang;
        }

        protected class Entry
        {
            public string Id { get; set; }
            public string Lang { get; set; }
            public int Level { get; set; }
            public string Txt { get; set; }
        }

        protected Dictionary<string, string> InitialRead()
        {
            var dic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            using (var cn = OpenDb(true))
            {
                var qry = "select Id, Lang, Level, Txt from Translation order by Id, Lang, Level";
                foreach(var ent in cn.Query<Entry>(qry))
                {
                    var ky = Key(ent.Id, ent.Lang);
                    dic[ky] = ent.Txt;
                }
            }
            return dic;
        }

        protected Dictionary<string, string> EnsureCacheLoaded()
        {
            lock(this)
            {
                if (_cache != null) return _cache;
                var dic = InitialRead();
                _lastRead = DateTime.Now;
                _cache = dic;
                return dic;
            }
        }

        public string Get(string id, string language)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("id");
            if (string.IsNullOrEmpty(language)) throw new ArgumentException("language");
            var ky = Key(id, language);
            var d = _cache;
            if (d == null)
            {
                d = EnsureCacheLoaded();
            }
            return d.GetValueOrDefault(ky, null);
        }

        public string GetFirst(IEnumerable<string> ids, string language)
        {
            if (string.IsNullOrEmpty(language)) throw new ArgumentException("language");
            var d = _cache;
            if (d == null)
            {
                d = EnsureCacheLoaded();
            }
            foreach(var id in ids)
            {
                var ky = Key(id, language);
                var v = d.GetValueOrDefault(ky, null);
                if (v != null) return v;
            }
            return null;
        }

        public void Set(string id, string text, string language)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("id");
            if (string.IsNullOrEmpty(language)) throw new ArgumentException("language");
            using (var cn = OpenDb(false))
            {
                var p = new { text = text, id = id, lang = language, level = 3 };
                int n = cn.Execute("update Translation set Txt=@text where Id=@id and Lang=@lang and Level=@level", p);
                if (n == 0) cn.Execute("insert into Translation(Id, Lang, Level, Txt) values(@id, @lang, @level, @text)", p);
                lock (this)
                {
                    var k = Key(id, language);
                    var d = EnsureCacheLoaded();
                    d[k] = text;
                }
            }
                
        }

        

        protected SQLiteConnection OpenDb(bool readOnly = false)
        {
            
            return SqliteUtil.OpenDb(this.DbFile, dbinit, readOnly);
            
        }

        /// <summary>
        /// levels: 1 - main file, 2 - profile, 3 - user overrides
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="level"></param>
        public void LoadTextFile(string filePath, int level)
        {

        }

        public void SaveTextFile(string filePath, int level)
        {

        }

        public void SaveTo(TextWriter output, int level)
        {
            using (var cn = OpenDb(true))
            {
                cn.Query<Entry>("select Id, Lang, Txt, Level from Translation where Level=@level order by Id, Lang", new { level = level });
            }
        }

        protected void Save(IEnumerable<Entry> ents, TextWriter output)
        {
            foreach(var e in ents)
            {
                output.Write(e.Id);
                
            }
        }

    }
}
