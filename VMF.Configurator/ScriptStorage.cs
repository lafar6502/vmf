using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NGinnBPM.DSLServices;
using System.IO;
using NLog;
using Boo.Lang.Compiler;

namespace VMF.Configurator
{
    public class ScriptStorage : SimpleFSStorage
    {
        protected string _bdir;
        public ScriptStorage(string baseDir) : base(baseDir, true)
        {
            if (string.IsNullOrEmpty(baseDir)) throw new ArgumentException("base dir is null");
            _bdir = baseDir;
        }

        public override IEnumerable<string> GetScriptUrls()
        {
            var urls = Directory.GetFiles(_bdir, "*" + FileExtension, SearchOption.AllDirectories).Select(x => GetRelativePath(x, _bdir)).Select(x => x.Remove(x.Length - 4)).ToList();

            return urls;
        }


        public static string GetRelativePath(string fullPath, string basePath)
        {
            // Require trailing backslash for path
            if (!basePath.EndsWith("\\"))
                basePath += "\\";

            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            // Uri's use forward slashes so convert back to backward slashes
            return relativeUri.ToString().Replace("/", "\\");

        }

        public override string GetTypeNameFromUrl(string url)
        {
            var tn = base.GetTypeNameFromUrl(url);
            tn = tn.Replace('\\', '_').Replace('.', '_');
            return tn;
        }

        public override ICompilerInput CreateCompilerInput(string url)
        {
            var fn = MapUrlToFilePath(url);
            if (fn == null || !File.Exists(fn)) throw new Exception("File not found: " + fn);
            var s = File.ReadAllText(fn);
            var tn = GetTypeNameFromUrl(url);
            return new Boo.Lang.Compiler.IO.StringInput(tn + FileExtension, s);
        }



    }
}
