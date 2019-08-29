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

namespace Steeltoe.Initializr.Services.Mustache
{
    public class MustacheConfigSchema
    {
        public List<Param> Params { get; set; }

        public List<CalculatedParam> CalculatedParams { get; set; }

        public List<ConditionalInclusions> ConditionalInclusions { get; set; }

        public List<Version> Versions { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class ConditionalInclusions
    {
        public string Name { get; set; }

        public string InclusionExpression { get; set; }
    }

    public class CalculatedParam
    {
        public string Name { get; set; }

        public string Expression { get; set; }

        public ExpressionTypeEnum ExpressionType { get; set; }
    }

    public enum ExpressionTypeEnum
    {
        Any = 0, // Any of the keys are present in Comma seperated list
        Case, // Given string a:x, b:y, c:z Case when key is a, then x ...
        Bool, // Boolean expression over the keys (converted to lambda)
        String, // String lambda expression over keys
    }

    public class Version
    {
        public string Name { get; set; }

        public string DefaultValue { get; set; }

        public string Description { get; set; }

        public List<ChoiceValue> Choices { get; set; }
    }

    public class ChoiceValue
    {
        public string Choice { get; set; }

        public string Description { get; set; }
    }

    public class Param
#pragma warning restore SA1402 // File may only contain a single type
    {
        public string Name { get; set; }

        public string DefaultValue { get; set; }

        public string Description { get; set; }

        public string FriendlyName { get; set; }

        // TODO : Add param type Bool, String etc
    }
}
