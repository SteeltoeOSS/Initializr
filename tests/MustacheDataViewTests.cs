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

using Steeltoe.Initializr.Services.Mustache;
using System.Collections.Generic;
using Xunit;

namespace Steeltoe.Initializr.Tests
{
    public class MustacheDataViewTests
    {
        [Fact]
        public void TestExpressions()
        {
            var cdv = new MustacheConfig();
            var dv = new Dictionary<string, object>
            {
                { "A", true },
                { "B", false },
                { "C", false },
            };
            var calcExp = new CalculatedParam
            {
                Name = "testExp",
                Expression = "A || B && !C ",
                ExpressionType = ExpressionTypeEnum.Lookup,
            };

            var lambda = cdv.BuildLookupLambda(calcExp, dv);
            cdv.EvaluateLambdaExpression<bool>(calcExp.Name, lambda, dv);
            Assert.Contains(dv, (item) => item.Key == calcExp.Name && item.Value is bool itemValue && itemValue);
        }

        [Fact]
        public void TestExpressionsActuators()
        {
            var cdv = new MustacheConfig();
            var dv = new Dictionary<string, object>
            {
                { "MySql", true },
                { "MySqlEFCore", false },
            };
            var calcExp = new CalculatedParam
            {
                Name = "MySqlOrMySqlEFCore",
                Expression = "MySql || MySqlEFCore",
                ExpressionType = ExpressionTypeEnum.Lookup,
            };

            var lambda = cdv.BuildLookupLambda(calcExp, dv);
            cdv.EvaluateLambdaExpression<bool>(calcExp.Name, lambda, dv);
            Assert.Contains(dv, (item) => item.Key == calcExp.Name && item.Value is bool itemValue && itemValue);
        }

        [Fact]
        public void TestLambdaExpression()
        {
            var cdv = new MustacheConfig();
            var dv = new Dictionary<string, object>
            {
                { "TargetFrameworkVersion", "netcoreapp2.2" },
            };
            var calcExp = new CalculatedParam
            {
                Name = "AspNetCoreVersion",
                Expression = "dataView => dataView[\"TargetFrameworkVersion\"]==\"netcoreapp2.2\"? \"2.2.0\": null",
                ExpressionType = ExpressionTypeEnum.Lambda,
            };

            cdv.EvaluateLambdaExpression<string>(calcExp.Name, calcExp.Expression, dv);
            Assert.Contains(dv, (item) => item.Key == calcExp.Name && (string)item.Value == "2.2.0");
        }
    }
}