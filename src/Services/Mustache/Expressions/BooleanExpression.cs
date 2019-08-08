using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.Utilities;

namespace Steeltoe.Initializr.Services.Mustache
{
    /// <summary>
    /// Build a boolean Expression from string notation
    /// for example A || B && C
    /// where A, B, C are parameters provided earlier in the context;
    /// </summary>
    public class BooleanExpression : IExpression
    {
        private readonly CalculatedParam _param;
        private readonly EvaluationExpression _evaluationExpression;
        private readonly ILogger _logger;

        public BooleanExpression(ILogger logger, CalculatedParam param, MustacheConfigSchema config)
        {
            _param = param;
            _logger = logger;
            _evaluationExpression = BuildEvaluationExpression(config);
        }

        public EvaluationExpression BuildEvaluationExpression(MustacheConfigSchema config)
        {
            var lambdaString = GetLambdaString(config);
            var options = ScriptOptions.Default.AddReferences(Assembly.GetExecutingAssembly());
            using (Timing.Over(_logger, "Build bool expression"))
            {
                return CSharpScript
                    .EvaluateAsync<EvaluationExpression>(lambdaString, options).Result;
            }
        }

        public async Task<string> EvaluateExpressionAsync(Dictionary<string, string> dataView)
        {
            using (Timing.Over(_logger, "Eval bool expression"))
            {
                return await Task.Run(() => _evaluationExpression(dataView));
            }
        }

        private string GetLambdaString(MustacheConfigSchema config)
        {
                var lambda = string.Empty;

                var expressionString = _param.Expression;
                var tree = SyntaxFactory.ParseExpression(expressionString);

                foreach (var node in tree.DescendantTokens())
                {
                    var key = node.ToString();
                    if (config.Params.Any(p => p.Name == key))
                    {
                        lambda += $" (bool.TryParse(dataView[\"{key}\"], out bool bool{key}Value) && bool{key}Value)";
                    }
                    else
                    {
                        lambda += key;
                    }
                }

                lambda = "dataView => { bool result = " + lambda + "; return result.ToString(); }";
                _logger.LogDebug("Created boolean lambda : " + lambda);
                return lambda;
        }
    }
}
