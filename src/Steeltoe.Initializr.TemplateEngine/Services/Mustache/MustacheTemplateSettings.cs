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
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Steeltoe.Initializr.TemplateEngine.Services.Mustache.Expressions;

namespace Steeltoe.Initializr.TemplateEngine.Services.Mustache
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
                returnValue.Add(new SourceFile
                {
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
                    case ExpressionTypeEnum.MoreThanOne:
                        expression = new MoreThanOneExpression(_logger, calculatedParam, schema);
                        break;
                    default:
                        throw new InvalidEnumArgumentException("Calculated Expression", (int)calculatedParam.ExpressionType, typeof(ExpressionTypeEnum));
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
