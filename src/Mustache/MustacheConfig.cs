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

namespace Steeltoe.Initializr.Services
{
    public class MustacheConfig
    {
        public List<Dependency> Dependencies { get; set; }

        public Version SteeltoeVersion { get; set; }

        public Version TargetFrameworkVersion { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class Version
#pragma warning restore SA1402 // File may only contain a single type
    {
        public string DefaultValue { get; set; }

        public string Description { get; set; }

        public List<ChoiceValue> Choices { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class ChoiceValue
#pragma warning restore SA1402 // File may only contain a single type
    {
        public string Choice { get; set; }

        public string Description { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class Dependency
#pragma warning restore SA1402 // File may only contain a single type
    {
        public string Name { get; set; }

        public string DefaultValue { get; set; }

        public string Description { get; set; }

        public string FriendlyName { get; set; }
    }
}