using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core.Configurator
{
    public class ProductConfigInfo
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }

        public List<ParamInfo> Fields { get; set; }

        /// <summary>
        /// validation warnings
        /// </summary>
        public List<string> Warnings { get; set; }
        /// <summary>
        /// validation errors
        /// </summary>
        public List<string> Errors { get; set; }
        /// <summary>
        /// defined config sections (additional calculations)
        /// </summary>
        public List<string> ConfigSections { get; set; }
    }
}
