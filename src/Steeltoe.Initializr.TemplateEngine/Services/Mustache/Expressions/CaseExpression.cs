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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.TemplateEngine.Utilities;

namespace Steeltoe.Initializr.TemplateEngine.Services.Mustache.Expressions
{
    public class CaseExpression : IExpression
    {
        private readonly CalculatedParam _param;
        private readonly EvaluationExpression _evaluationExpression;
        private readonly ILogger _logger;

        public CaseExpression(ILogger logger, CalculatedParam param, MustacheConfigSchema schema)
        {
            _logger = logger;
            _param = param;
            _evaluationExpression = BuildEvaluationExpression(schema);
        }

        public EvaluationExpression BuildEvaluationExpression(MustacheConfigSchema schema)
        {
            using (Timing.Over(_logger, "Build case Expression"))
            {
                // TODO: Add validation
                string EvaluationExpression(Dictionary<string, string> dataView)
                {
                    var terms = _param.Expression.Split(',');
                    var lookupKey = terms[0]; // First term is the lookup Case(term0) { ...
                    var dataViewValue = dataView.ContainsKey(lookupKey) ? dataView[lookupKey] : string.Empty;
                    var caseResult = string.Empty;
                    foreach (var term in terms.Skip(1))
                    {
                        var expression = term.Split('=');
                        if ((dataViewValue == null || !expression[0].Equals(dataViewValue)) &&
                            expression[0] != "default")
                        {
                            continue;
                        }

                        caseResult = expression[1];
                        break;
                    }

                    return caseResult;
                }

                return EvaluationExpression;
            }
        }

        public async Task<string> EvaluateExpressionAsync(Dictionary<string, string> dataView)
        {
            using (Timing.Over(_logger, "Build case Expression"))
            {
                return await Task.Run(() => _evaluationExpression(dataView));
            }
        }
    }
}
