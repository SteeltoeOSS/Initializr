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
            set => _dependencies = (value == null || value.Length == 0 || value[0] == null) ? value : value[0].Split(',');
        }
        public string name { get; set; }
        public string type { get; set; }
    }
}
