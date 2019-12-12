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
using System.Net;
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
        [HttpPost]
        public Task<ActionResult> GenerateProjectPost([FromForm] GeneratorModel model)
        {
            return GenerateProject(model);
        }

        [Route("/starter.zip")]
        [HttpGet]
        public Task<ActionResult> GenerateProjectGet([FromQuery] GeneratorModel model)
        {
            return GenerateProject(model);
        }

        [Route("dependencies")]
        public ActionResult GetDependencies([FromQuery(Name = "templateShortName")] string templateShortName, [FromQuery(Name = "templateVersion")] TemplateVersion? templateVersion)
        {
            return Ok(_sttemplateService.GetDependencies(templateShortName, templateVersion ?? TemplateVersion.V2));
        }

        [Route("templates")]
        public ActionResult<IEnumerable<TemplateViewModel>> GetTemplates([FromQuery(Name = "Mustache")] bool useMustache)
        {
            return _sttemplateService.GetAvailableTemplates();
        }

        private async Task<ActionResult> GenerateProject(GeneratorModel model)
        {
            try
            {
                var archiveBytes = await _sttemplateService.GenerateProjectArchiveAsync(model);

                var cd = new ContentDispositionHeaderValue("attachment")
                {
                    FileNameStar = model.ArchiveName,
                };

                Response.Headers.Add("Content-Disposition", cd.ToString());

                return File(archiveBytes, "application/zip");
            }
            catch (Exception ex)
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                var message = ex.Message;
                if (model.TargetFrameworkVersion == "netcoreapp3.1" && model.SteeltoeVersion == "2.3.0")
                {
                    message = "2.4.0 is the lowest version of Steeltoe that works with netcoreapp3.1\n";
                }

                return Content(message);
            }
        }
    }
}
