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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Steeltoe.Initializr.TemplateEngine.Services;

namespace Steeltoe.Initializr.TemplateEngine.Models
{
    public class GeneratorModel
    {
        private string _projectName;

        [ProjectNameValidation]
        public string ProjectName
        {
            get => _projectName ?? "SteeltoeProject";
            set => _projectName = value;
        }

        public string Description { get; set; }

        public string ArchiveName => ProjectName + ".zip";

        public string SteeltoeVersion { get; set; } = Constants.Steeltoe24;

        public string TargetFramework { get; set; } = Constants.NetCoreApp31;

        public string Template { get; set; } = Constants.WebApi;

        public string Dependencies { get; set; }

        public string[] GetDependencies()
        {
            return string.IsNullOrEmpty(Dependencies) ? null : Dependencies.ToLower().Split(',');
        }

        public IEnumerable<string> GetTemplateParameters()
        {
            var templateParameters = GetDependencies()?.Where(d => d != null).ToList();

            if (!string.IsNullOrEmpty(SteeltoeVersion) && SteeltoeVersion != "3.0")
            {
                templateParameters?.Add($"SteeltoeVersion={SteeltoeVersion}");
            }

            return templateParameters ?? new List<string>();
        }
    }
}
