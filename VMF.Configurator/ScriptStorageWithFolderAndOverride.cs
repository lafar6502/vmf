using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boo.Lang.Compiler;
using Facile.Core;

namespace Unitech.CutLists.V2
{
    public class ScriptStorageWithFolderAndOverride : ScriptStorageWithFolder
    {
        protected string _overrideDir;

        public ScriptStorageWithFolderAndOverride(string baseDir, string overrideDir) : base(baseDir)
        {
            _overrideDir = overrideDir;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileurl"></param>
        /// <returns></returns>
        public override string MapUrlToFilePath(string fileurl, string extension = null)
        {
            var ext = extension ?? FileExtension;
            foreach(var bd in new string[] { _overrideDir, _bdir })
            {
                var di = new DirectoryInfo(bd);
                var zdx = fileurl.IndexOf(Path.DirectorySeparatorChar);
                var furl = fileurl;
                if (zdx >= 0)
                {
                    if (!string.Equals(di.Name, fileurl.Substring(0, zdx), StringComparison.InvariantCultureIgnoreCase))
                    {
                        //huh? wrong dir?
                        continue;
                    }
                    furl = fileurl.Substring(zdx + 1);
                }
                
                var fp = Path.Combine(bd, furl);
                if (!File.Exists(fp) && !fp.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)) fp += ext;
                if (File.Exists(fp)) return fp;
            }
            return null;
        }

        private string GetFileUrlFromPath(string pth, string dir)
        {
            var rp = GetRelativePath(pth, dir);
            if (Path.HasExtension(rp))
            {
                var ex = Path.GetExtension(rp);
                rp = rp.Remove(rp.Length - ex.Length);
            }
            return rp;
        }

        /// <summary>
        /// finds all script urls mapped to actual file location...
        /// </summary>
        /// <returns></returns>
        protected IDictionary<string, string> GetScriptUrlsDictionary()
        {
            var dic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            var f0 = Directory.GetFiles(_bdir, "*" + FileExtension, SearchOption.AllDirectories); //.Select(x => GetRelativePath(x, _bdir)).Select(x => x.Remove(x.Length - 4)).ToList();
            foreach(var fp in f0)
            {
                var based = Path.GetDirectoryName(_bdir);
                dic.Add(GetFileUrlFromPath(fp, based), fp);
            }
            if (!Directory.Exists(_overrideDir)) return dic;

            var f1 = Directory.GetFiles(_overrideDir, "*" + FileExtension, SearchOption.AllDirectories);
            foreach(var fp in f1)
            {
                var based = Path.GetDirectoryName(_overrideDir);
                var rp = GetFileUrlFromPath(fp, based);
                dic.Remove(rp);
                dic.Add(rp, fp);
            }
            return dic;
        }

        public override IEnumerable<string> GetScriptUrls()
        {
            var dic = GetScriptUrlsDictionary();
            return dic.Keys.OrderBy(x => x);
        }

        



    }
}
