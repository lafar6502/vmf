using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace VMF.Core.Util
{
    public class JsonSerialization
    {

        public static JsonSerializerSettings Default { get; set; } = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            TypeNameHandling = TypeNameHandling.Auto,
            DefaultValueHandling = DefaultValueHandling.Include,
            Formatting = Formatting.Indented
        };
    }
}
