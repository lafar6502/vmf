using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Web.Mvc;
using VMF.Core.Util;

namespace VMF.UI.Lib.Mvc
{
    public class JsonNetResult : ActionResult
    {
        private object ret;
        private Func<object> callback;
        public JsonSerializerSettings SerializationSettings { get; set; }
        public bool WrapException = true;

        public JsonNetResult(object obj)
        {
            ret = obj;
        }

        public JsonNetResult(Func<object> cb)
        {
            callback = cb;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/json";
            var obj = callback != null ? callback() : ret;
            var ss = SerializationSettings ?? JsonSerialization.Default;
            string json = null;
            if (obj is Exception && WrapException)
            {
                var ex = obj as Exception;
                context.HttpContext.Response.StatusCode = 500;
                json = JsonConvert.SerializeObject(new
                {
                    Success = false,
                    Message = ex.Message,
                    Stack = ex.ToString()
                });
            }
            else
            {
                json = JsonConvert.SerializeObject(obj, ss);
            }

            context.HttpContext.Response.Write(json);
        }
    }
}
