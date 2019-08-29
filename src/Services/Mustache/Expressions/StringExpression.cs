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

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.Utilities;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Steeltoe.Initializr.Services.Mustache
{
    public class StringExpression : IExpression
    {
        private readonly CalculatedParam _param;
        private readonly EvaluationExpression _evaluationExpression;
        private readonly ILogger _logger;

        public StringExpression(ILogger logger, CalculatedParam param, MustacheConfigSchema config)
        {
            _param = param;
            _logger = logger;
            _evaluationExpression = BuildEvaluationExpression(config);
        }

        public EvaluationExpression BuildEvaluationExpression(MustacheConfigSchema config)
        {
            var options = ScriptOptions.Default.AddReferences(Assembly.GetExecutingAssembly());
            using (Timing.Over(_logger, "Build StringExpression"))
            {
                return CSharpScript.EvaluateAsync<EvaluationExpression>(_param.Expression, options).Result;
            }
        }

        public async Task<string> EvaluateExpressionAsync(Dictionary<string, string> dataView)
        {
            using (Timing.Over(_logger, "Eval StringExpression"))
            {
                return await Task.Run(() => _evaluationExpression(dataView));
            }
        }
    }
}
