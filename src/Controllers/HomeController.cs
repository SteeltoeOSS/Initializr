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

using Microsoft.AspNetCore.Mvc;
using Steeltoe.Initializr.Models;
using Steeltoe.Initializr.Services;
using Steeltoe.Initializr.Services.Mustache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Steeltoe.Initializr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private const string LOGO = @"          (                                                
          )\ )    )           (     )                      
         (()/( ( /(   (    (  )\ ( /(        (             
          /(_)))\()) ))\  ))\((_))\()) (    ))\            
  ____   (_)) (_))/ /((_)/((_)_ (_))/  )\  /((_)__ __ __   
 / /\ \  / __|| |_ (_)) (_)) | || |_  ((_)(_))  \ \\ \\ \  
< <  > > \__ \|  _|/ -_)/ -_)| ||  _|/ _ \/ -_)  > >> >> > 
 \_\/_/  |___/ \__|\___|\___||_| \__|\___/\___| /_//_//_/  
                                                           ";

        private readonly MustacheTemplateService _templateService;

        public HomeController(IEnumerable<ITemplateService> services)
        {
            _templateService = services.OfType<MustacheTemplateService>().FirstOrDefault();
        }

        [Route("/")]
        [HttpGet]
        [IsCurlRequest]
        public ActionResult<string> CurlHelp()
        {
            var result = new StringBuilder();
            result.Append(LOGO);
            result.Append(GetDependencies());
            result.Append(GetExamples());

            return Content(result.ToString());
        }

        private string GetExamples()
        {
            return @"
Get Dependencies: 
    curl https://start.steeltoe.io/api/templates/dependencies | jq .

Get Versions:
    curl https://start.steeeltoe.io/api/templates/templates | jq .

Get project: 
    curl https://start.steeltoe.io/starter.zip -d dependencies=actuators,cloudfoundry -o myProject.zip
 
    curl https://start.steeltoe.io/starter.zip -d dependencies=actuators,cloudfoundry -d templateShortName=Steeltoe-React -d targetFrameworkVersion=netcoreapp3.1 -d projectName=MyCompany.MySample -o myProject.zip
";
        }

        private string GetDependencies()
        {
            var result = new StringBuilder();
            var dependencies = _templateService.GetDependencies(string.Empty, TemplateVersion.V3);
            var fieldWidths = new int[] { 40, 100 };

            result.Append("\nDependencies: \n");
            result.Append(GetHorizontalBorder(fieldWidths));
            result.Append(GetRow("Title", "Description", fieldWidths));
            result.Append(GetHorizontalBorder(fieldWidths));
            foreach (var dep in dependencies)
            {
                result.Append(GetRow(dep.Name, dep.Description.Replace("Steeltoe: ", ""), fieldWidths));
            }

            result.Append(GetHorizontalBorder(fieldWidths));
            result.Append("\n");
            return result.ToString();
        }

        private string GetRow(string title, string description, int[] fieldWidths)
        {
           return $"| {title.PadRight(fieldWidths[0] - 1)}| {description.PadRight(fieldWidths[1] - 1)}|\n";
        }

        private string GetHorizontalBorder(int[] widths)
        {
            return "+" + new string('-', widths[0]) + "+" + new string('-', widths[1]) + "+\n";
        }
    }
}
