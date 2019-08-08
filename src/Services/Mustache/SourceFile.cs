using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Steeltoe.Initializr.Services.Mustache
{
    public class SourceFile
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string Text { get; set; }
    }
}
