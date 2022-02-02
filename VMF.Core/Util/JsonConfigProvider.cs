using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace VMF.Core.Util
{
    public class JsonConfigProvider : IConfigProvider
    {
        private JObject _data;
        private static Logger log = LogManager.GetCurrentClassLogger();

        public JsonConfigProvider() : this(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory))
        {

        }

        public JsonConfigProvider(string profile) : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config"), Environment.MachineName, profile)
        {

        }
        

        public JsonConfigProvider(string directory, string machineName, string profile)
        {
            //config.json
            //config.machineName.json
            var filz = new string[]
            {
                "config.json",
                String.Format("{0}.json", machineName),
                String.Format("{0}.{1}.json", profile, machineName)
            };
            LoadData(directory, filz);
        }

        private void LoadData(string baseDir, IEnumerable<string> files)
        {
            _data = new JObject();
            _data["basedir"] = baseDir;
            foreach(var f in files)
            {
                var f2 = Path.Combine(baseDir, f);
                if (File.Exists(f2))
                {
                    log.Info("Loading config file {0}", f2);
                    var j = LoadFile(f2);
                    _data.Merge(j, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Replace
                    });
                }
            }

        }

        private JObject LoadFile(string file)
        {
            var tx = File.ReadAllText(file, Encoding.UTF8);
            return JObject.Parse(tx);
        }
        public T Get<T>(string name, T defaultValue)
        {
            return GetInternal<T>(name, defaultValue);
        }

        public T Get<T>(string name)
        {
            return GetInternal<T>(name, default(T));
        }

        public void SetProperties(string configValue, object target)
        {
            throw new NotImplementedException();
        }

        protected T GetInternal<T>(string name, T defaultValue)
        {
            //if (_lastCheck.AddSeconds(30) < DateTime.Now)
            //{
            //    ReloadIfNecessary();
            //}
            var v = _data.GetValue(name);
            if (v == null)
            {
                v = GetJsonValue(_data, name);
            }
            if (defaultValue != null && v != null)
            {
                return (T)v.ToObject(defaultValue.GetType());
            }
            return v == null ? defaultValue : v.ToObject<T>();

        }

        private static JToken GetJsonValue(JToken root, string path)
        {
            if (root.Type != JTokenType.Object && root.Type != JTokenType.Array)
            {
                return null;
            }
            int idx = path.IndexOf('.');
            string segment = idx > 0 ? path.Substring(0, idx) : path;
            var t = root[segment];
            if (t == null) return null;


            if (idx > 0)
            {
                return GetJsonValue(t, path.Substring(idx + 1));
            }
            else
            {
                return t;
            }
        }
    }
}
