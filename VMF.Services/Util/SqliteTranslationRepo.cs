using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using Dapper;
using VMF.Core;

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

        const string dbinit = @"create table Translation(Id nvarchar(200), lang nchar(2), Txt TEXT, PRIMARY KEY(Id, Lang));
create table TranslationP(Id nvarchar(200), lang nchar(2), Txt TEXT, PRIMARY KEY(Id, Lang));
create table TranslationU(Id nvarchar(200), lang nchar(2), Txt TEXT, PRIMARY KEY(Id, Lang));";

        public IEnumerable<string> Languages => throw new NotImplementedException();

        public event Action<ITextTranslation> TranslationsChanged;

        public string Get(string id, string language)
        {
            throw new NotImplementedException();
        }

        public string GetFirst(IEnumerable<string> ids, string language)
        {
            throw new NotImplementedException();
        }

        public void Set(string id, string text, string language)
        {
            throw new NotImplementedException();
        }

        protected SQLiteConnection OpenDb(bool readOnly = false)
        {
            lock(this)
            {
                return SqliteUtil.OpenDb(this.DbFile, dbinit, readOnly);
            }
        }
    }
}
