using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.Utilities;

namespace Steeltoe.Initializr.Services.Mustache
{
    public class AnyExpression : IExpression
    {
        private readonly CalculatedParam _param;
        private readonly EvaluationExpression _evaluationExpression;
        private readonly ILogger _logger;

        public AnyExpression(ILogger logger, CalculatedParam param, MustacheConfigSchema schema)
        {
            _param = param;
            _logger = logger;
            _evaluationExpression = BuildEvaluationExpression(schema);
        }

        public EvaluationExpression BuildEvaluationExpression(MustacheConfigSchema schema)
        {
            //TODO: Add validation
            using (Timing.Over(_logger, "Build Any Expression"))
            {
                var keys = _param.Expression.Split(',');
                return dataView => dataView.Any(kvp =>
                                        keys.Contains(kvp.Key)
                                        && bool.TryParse(kvp.Value.ToString(), out var boolValue)
                                        && boolValue).ToString();
            }
        }

        public async Task<string> EvaluateExpressionAsync(Dictionary<string, string> dataView)
        {
            using (Timing.Over(_logger, "Build Any Expression"))
            {
                return await Task.Run(() => _evaluationExpression(dataView));
            }
        }
    }
}
