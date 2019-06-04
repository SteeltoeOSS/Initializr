using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InitializrApi.Models
{
    public class GeneratorModel
    {
        private string[] _dependencies;

        public string[] dependencies
        {
            get => _dependencies;
            set => _dependencies = (value == null || value.Length == 0 || value[0] == null) ? value : value[0].ToLower().Split(',');
        }
        public string projectName { get; set; }
        public string templateShortName { get; set; }
        
        public string description { get; set; }
        public string steeltoeVersion { get; set; }
    }
}
