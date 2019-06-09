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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;

namespace Steeltoe.Initializr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private const string DEFAULT_TEMPLATE = "steeltoe2";
        private ITemplateService _templateService;
        private ISteeltoeTemplateService _sttemplateService;

        public TemplatesController(ITemplateService service, ISteeltoeTemplateService stTemplateService)
        {
            _templateService = service;
            _sttemplateService = stTemplateService;
        }

        [Route("/starter.zip")]
        [HttpPost]
        public ActionResult GenerateProjectPost([FromForm] GeneratorModel model)
        {
             return GenerateProject(model);
        }

        [Route("/createtest")]
        public ActionResult GenerateProjectTest([FromQuery(Name = "templateShortName")] string templateShortName)
        {
            var testModel = new GeneratorModel
            {
                TemplateShortName = templateShortName ?? DEFAULT_TEMPLATE,
                ProjectName = "mytest",
                Dependencies = new[] { "actuators,mysql" },
            };
            return GenerateProject(testModel);
        }

        [Route("/dependencies")]
        public ActionResult GetDependencies()
        {
            return Ok(_templateService.GetDependencies("steeltoe"));
        }

        [Route("all")]
        public ActionResult<IEnumerable<TemplateViewModel>> GetTemplates()
        {
            return _templateService.GetAvailableTemplates();
        }

        [Route("stall")]
        public ActionResult<IEnumerable<string>> GetSteeltoeTemplates()
        {
            return _sttemplateService.GetAvailableTemplates();
        }

        private ActionResult GenerateProject(GeneratorModel model)
        {
            var list = _templateService.GetAvailableTemplates();
            var currentTemplate = (model.TemplateShortName ?? DEFAULT_TEMPLATE).ToLower();

            if (list == null || !list.Any(x => x.ShortName.ToLower() == currentTemplate))
            {
                return NotFound($"Template {currentTemplate} was not found");
            }

            var templateParameters = model.Dependencies.ToList();

            if (!string.IsNullOrEmpty(model.SteeltoeVersion))
            {
                if (model.SteeltoeVersion == "3.0")
                {
                    currentTemplate = "steeltoe";
                }
                else
                {
                    templateParameters.Add($"SteeltoeVersion={model.SteeltoeVersion}");
                }
            }

            string outFolder = _templateService.GenerateProject(currentTemplate, model.ProjectName, templateParameters.ToArray()).Result;
            var zipName = (model.ProjectName ?? "steeltoeProject") + ".zip";

            var zipFile = Path.Combine(outFolder, "..", zipName);
            ZipFile.CreateFromDirectory(outFolder, zipFile);

            var cd = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = zipName,
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return File(System.IO.File.ReadAllBytes(zipFile), "application/zip");
        }
    }
}
