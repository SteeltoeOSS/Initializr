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
using System.IO;

namespace Steeltoe.Initializr.Services.Mustache
{
    public class InclusionExpression
    {
        private readonly string _expression;
        private readonly bool _matchesView;

        public InclusionExpression(string expression, bool matchesView)
        {
            _expression = expression;
            _matchesView = matchesView;
        }

        public bool IsInclusion
        {
            get { return _matchesView; }
        }

        public bool IsMatch(string fileName)
        {
            if (_expression.EndsWith("**"))
            {
                if (fileName.StartsWith(_expression.Replace("/**", string.Empty)))
                {
                    return true;
                }
            }
            else
            {
                var escapedExpression = _expression.Replace('/', Path.DirectorySeparatorChar);
                var exactMatches = escapedExpression.Split(';');
                foreach (var match in exactMatches)
                {
                    if (string.Equals(fileName, match, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
