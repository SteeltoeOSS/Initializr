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
