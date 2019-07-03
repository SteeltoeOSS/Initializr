// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Steeltoe.Initializr.Models
{
    public class GeneratorModel
    {
        private string[] _dependencies;
        private string _templateShortName;
        private string _projectName;

        public string[] Dependencies
        {
            get => _dependencies;
            set => _dependencies = (value == null || value.Length == 0 || value[0] == null) ? null : value[0].ToLower().Split(',');
        }

        public string ProjectName { get => _projectName ?? "steeltoeProject" ; set => _projectName = value; }

        public string TemplateShortName
        {
            get
            {
               // _templateShortName = _templateShortName ?? DEFAULT_TEMPLATE;
                //if (SteeltoeVersion == "3.0")
                //{
                //    _templateShortName = "steeltoe";
                //}

                return _templateShortName;
            }
            set => _templateShortName = value;
        }

        public string Description { get; set; }

        public string SteeltoeVersion { get; set; }

        public string ArchiveName
        {
            get => ProjectName + ".zip";
        }

        public string TargetFrameworkVersion { get; set; }

        public IEnumerable<string> GetTemplateParameters()
        {
            var templateParameters = Dependencies?.Where(d => d != null).ToList();

            if (!string.IsNullOrEmpty(SteeltoeVersion) && SteeltoeVersion != "3.0")
            {
                templateParameters.Add($"SteeltoeVersion={SteeltoeVersion}");
            }

            return templateParameters ?? new List<string>();
        }
    }
}
