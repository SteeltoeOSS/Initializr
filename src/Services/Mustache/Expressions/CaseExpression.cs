using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.Utilities;

namespace Steeltoe.Initializr.Services.Mustache
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
