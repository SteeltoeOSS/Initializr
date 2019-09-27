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

using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.Services.Mustache;
using Steeltoe.InitializrTests;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.Initializr.Tests
{
    public class MustacheDataViewTests : XunitLoggingBase
    {
        private readonly ILogger<MustacheDataViewTests> _logger;

        public MustacheDataViewTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            _logger = loggerFactory.CreateLogger<MustacheDataViewTests>();
        }

        [Fact]
        public async Task TestBoolExpressions()
        {
            var config = new MustacheConfigSchema()
            {
                Params = new List<Param>
                {
                    new Param { Name = "A", DefaultValue = "true" },
                    new Param { Name = "B", DefaultValue = "false" },
                    new Param { Name = "C", DefaultValue = "false" },
                },
            };

            var dv = new Dictionary<string, string>
            {
                { "A", "true" },
                { "B", "false" },
                { "C", "false" },
            };

            var calcParam = new CalculatedParam
            {
                Name = "testExp",
                Expression = "A || B && !C ",
                ExpressionType = ExpressionTypeEnum.Bool,
            };

            var expression = new BooleanExpression(_logger, calcParam, config);
            var result = await expression.EvaluateExpressionAsync(dv);
            Assert.Equal("True", result);
        }

        [Fact]
        public async Task TestBoolExpressionsActuators()
        {
            var config = new MustacheConfigSchema()
            {
                Params = new List<Param>
                {
                    new Param { Name = "MySql", DefaultValue = "true" },
                    new Param { Name = "MySqlEFCore", DefaultValue = "false" },
                    new Param { Name = "C", DefaultValue = "false" },
                },
            };
            var dv = new Dictionary<string, string>
            {
                { "MySql", "true" },
                { "MySqlEFCore", "false" },
            };
            var calcParam = new CalculatedParam
            {
                Name = "MySqlOrMySqlEFCore",
                Expression = "MySql || MySqlEFCore",
                ExpressionType = ExpressionTypeEnum.Bool,
            };

            var expression = new BooleanExpression(_logger, calcParam, config);
            var result = await expression.EvaluateExpressionAsync(dv);
            Assert.Equal(true.ToString(), result);
        }

        [Fact]
        public async Task TestStringExpression()
        {
            var config = new MustacheConfigSchema()
            {
                Params = new List<Param>
                {
                    new Param { Name = "MySql", DefaultValue = "true" },
                    new Param { Name = "MySqlEFCore", DefaultValue = "false" },
                    new Param { Name = "C", DefaultValue = "false" },
                },
            };
            var dv = new Dictionary<string, string>
            {
                { "TargetFrameworkVersion", "netcoreapp2.2" },
            };
            var calcParam = new CalculatedParam
            {
                Name = "AspNetCoreVersion",
                Expression = "dataView => dataView[\"TargetFrameworkVersion\"]==\"netcoreapp2.2\"? \"2.2.0\": null",
                ExpressionType = ExpressionTypeEnum.Bool,
            };

            var expression = new StringExpression(_logger, calcParam, config);
            var result = await expression.EvaluateExpressionAsync(dv);
            Assert.Equal("2.2.0", result);
        }

        [Fact]
        public async Task Test_MorethanOneExpression_true()
        {
            var config = new MustacheConfigSchema();
            var dv = new Dictionary<string, string>
            {
                { "IsMoreThanOne", "false" },
                { "ConfigServer", "true" },
                { "SQLServer", "true" },
                { "Redis", "false" },
            };
            var calcParam = new CalculatedParam
            {
                Name = "IsMoreThanOne",
                Expression = "ConfigServer,SQLServer,Redis",
                ExpressionType = ExpressionTypeEnum.MoreThanOne,
            };

            var expression = new MoreThanOneExpression(_logger, calcParam, config);
            var result = await expression.EvaluateExpressionAsync(dv);
            Assert.Equal("True", result);
        }

        [Fact]
        public async Task Test_MorethanOneExpression_false()
        {
            var config = new MustacheConfigSchema();
            var dv = new Dictionary<string, string>
            {
                { "IsMoreThanOne", "false" },
                { "ConfigServer", "false" },
                { "SQLServer", "true" },
                { "Redis", "false" },
            };
            var calcParam = new CalculatedParam
            {
                Name = "IsMoreThanOne",
                Expression = "ConfigServer,SQLServer,Redis",
                ExpressionType = ExpressionTypeEnum.MoreThanOne,
            };

            var expression = new MoreThanOneExpression(_logger, calcParam, config);
            var result = await expression.EvaluateExpressionAsync(dv);
            Assert.Equal("False", result);
        }

        [Fact]
        public async Task TestCaseExpression()
        {
            var config = new MustacheConfigSchema()
            {
                Params = new List<Param>
                {
                    new Param { Name = "MySql", DefaultValue = "true" },
                    new Param { Name = "MySqlEFCore", DefaultValue = "false" },
                    new Param { Name = "C", DefaultValue = "false" },
                },
            };
            var dv = new Dictionary<string, string>
            {
                { "TargetFrameworkVersion", "netcoreapp2.2" },
            };
            var calcParam = new CalculatedParam
            {
                Name = "AspNetCoreVersion",
                Expression = "TargetFrameworkVersion,netcoreapp2.2=2.2.0,netcoreapp2.1=2.1.1,default=False",
                ExpressionType = ExpressionTypeEnum.Case,
            };

            var expression = new CaseExpression(_logger, calcParam, config);
            var result = await expression.EvaluateExpressionAsync(dv);
            Assert.Equal("2.2.0", result);
        }

        [Fact]
        public async Task TestAnyExpression()
        {
            var config = new MustacheConfigSchema()
            {
                Params = new List<Param>
                {
                    new Param { Name = "MySql", DefaultValue = "true" },
                    new Param { Name = "MySqlEFCore", DefaultValue = "false" },
                    new Param { Name = "C", DefaultValue = "false" },
                },
            };
            var dv = new Dictionary<string, string>
            {
                { "MySql", "true" },
            };
            var calcParam = new CalculatedParam
            {
                Name = "AnyEFCore",
                Expression = "MySql,Postgres,Redis,MongoDB,OAuthConnector",
                ExpressionType = ExpressionTypeEnum.Any,
            };

            var expression = new AnyExpression(_logger, calcParam, config);
            var result = await expression.EvaluateExpressionAsync(dv);
            Assert.Equal("True", result);
        }
    }
}