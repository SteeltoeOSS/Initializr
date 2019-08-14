using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Steeltoe.Initializr.Services.Mustache
{
    public class MustacheTemplateSettings
    {
        public MustacheConfigSchema Schema { get;  }

        public IEnumerable<SourceFile> SourceSets { get; }

        public IDictionary<string, IExpression> EvaluationExpressions { get; }

        private readonly ILogger _logger;

        public MustacheTemplateSettings(ILogger logger, string path)
        {
            _logger = logger;
            Schema = ReadSchema(path);
            SourceSets = GetSourceSets(path);
            EvaluationExpressions = GetEvaluationExpressions(Schema);
        }

        private MustacheConfigSchema ReadSchema(string templatePath)
        {
            var json = File.ReadAllText(Path.Combine(templatePath, "mustache.json"));
            var returnValue = JsonConvert.DeserializeObject<MustacheConfigSchema>(json);
            if (returnValue == null)
            {
                throw new InvalidDataException($"could not find config at {templatePath}");
            }

            return returnValue;
        }

        private IEnumerable<SourceFile> GetSourceSets(string path)
        {
            var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).ToList();
            var returnValue = new List<SourceFile>();

            foreach (var file in files)
            {
                returnValue.Add(new SourceFile {
                    Name = file.Replace(Path.GetFullPath(path), string.Empty)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    FullPath = file,
                    Text = File.ReadAllText(file),
                });
            }

            return returnValue;
        }

        private IDictionary<string, IExpression> GetEvaluationExpressions(MustacheConfigSchema schema)
        {
            var evalExpressions = new Dictionary<string, IExpression>();
            foreach (var calculatedParam in schema.CalculatedParams)
            {
                IExpression expression = null;
                switch (calculatedParam.ExpressionType)
                {
                    case ExpressionTypeEnum.Any:
                        expression = new AnyExpression(_logger, calculatedParam, schema);
                        break;
                    case ExpressionTypeEnum.Bool:
                        expression = new BooleanExpression(_logger, calculatedParam, schema);
                        break;
                    case ExpressionTypeEnum.Case:
                        expression = new CaseExpression(_logger, calculatedParam, schema);
                        break;
                    case ExpressionTypeEnum.String:
                        expression = new CaseExpression(_logger, calculatedParam, schema);
                        break;
                }

                if (expression != null)
                {
                    evalExpressions.Add(calculatedParam.Name, expression);
                }
            }

            return evalExpressions;
        }
    }
}