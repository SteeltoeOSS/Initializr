using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Steeltoe.Initializr.Services.Mustache
{
    public delegate string EvaluationExpression(Dictionary<string, string> dataView);

    public interface IExpression
    {
        EvaluationExpression BuildEvaluationExpression(MustacheConfigSchema schema);

        Task<string> EvaluateExpressionAsync(Dictionary<string, string> dataView);
    }
}
