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
using Steeltoe.Initializr.Services.DotNetTemplateEngine;
using Steeltoe.Initializr.Services.Mustache;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Steeltoe.Initializr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        // private readonly TemplateService _templateService;
        private readonly MustacheTemplateService _sttemplateService;

        public TemplatesController(IEnumerable<ITemplateService> services)
        {
          // _templateService = services.OfType<TemplateService>().FirstOrDefault();
            _sttemplateService = services.OfType<MustacheTemplateService>().FirstOrDefault();
        }

        [Route("/starter.zip")]
        public Task<ActionResult> GenerateProjectPost([FromForm] GeneratorModel model)
        {
            return GenerateProject(model);
        }

        [Route("/createtest")]
        public async Task<ActionResult> GenerateProjectTest([FromQuery(Name = "templateShortName")] string templateShortName)
        {
            var testModel = new GeneratorModel
            {
                ProjectName = "mytest",
                Dependencies = new[] { "actuators,mysql" },
            };
            return await GenerateProject(testModel);
        }

        [Route("/createtest2")]
        public ActionResult GenerateProjectTest2([FromQuery(Name = "templateShortName")] string templateShortName)
        {
            var testModel = new GeneratorModel
            {
                ProjectName = "mytest",
                Dependencies = new[] { "actuators,mysql" },
            };
            return GenerateProject2(testModel);
        }

        [Route("/dependencies")]
        public ActionResult GetDependencies([FromQuery(Name = "templateShortName")] string templateShortName)
        {
            return Ok(_sttemplateService.GetDependencies(templateShortName));
        }

        [Route("all")]
        public ActionResult<IEnumerable<TemplateViewModel>> GetTemplates([FromQuery(Name = "Mustache")] bool useMustache)
        {
            return _sttemplateService.GetAvailableTemplates();
        }

        private async Task<ActionResult> GenerateProject(GeneratorModel model)
        {
            var archiveBytes = await _sttemplateService.GenerateProjectArchiveAsync(model);

            var cd = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = model.ArchiveName,
            };

            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(archiveBytes, "application/zip");
        }

        private ActionResult GenerateProject2(GeneratorModel model)
        {
            var fileBytes = _sttemplateService.GenerateProjectArchiveAsync(model).Result;
            var cd = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = (model.ProjectName ?? "SteeltoeProject") + ".zip",
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(fileBytes, "application/zip");
        }
    }
}
