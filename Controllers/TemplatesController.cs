using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Cli;
using Microsoft.TemplateEngine.Cli.PostActionProcessors;
using Microsoft.TemplateEngine.Edge;
using Microsoft.TemplateEngine.Edge.TemplateUpdates;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects;
using Microsoft.TemplateEngine.Utils;
using System.IO.Compression;
using System.Net.Http.Headers;
using Microsoft.TemplateEngine.Edge.Settings;
using Microsoft.TemplateEngine.Edge.Template;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using InitializrApi.Models;
using InitializrApi.Services;

namespace InitializrApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private ITemplateService _templateService;

        public TemplatesController(ITemplateService service)
        {
            _templateService = service;
        }

        // GET api/Template
        [Route("/starter.zip")]
        [HttpPost]
        public ActionResult GenerateProjectPost([FromForm] GeneratorModel model)
        {
            return GenerateProject(model);
        }

        private ActionResult GenerateProject(GeneratorModel model)
        {
            var form = Request.Form;
            var list = _templateService.GetAvailableTemplates();

            if (!list.Any(x => x.ShortName.ToLower() == model.type.ToLower()))
            {
                return NotFound($"Type {model.type} was not found");
            }
            string zipFile = _templateService.GenerateProject(model);
            var cd = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = zipFile
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return File(System.IO.File.ReadAllBytes(zipFile), "application/zip");

        }
        // GET api/templates

        [Route("debug-reinstall")]
        public ActionResult<string> DebugReinstall()
        {
            return _templateService.DebugReinstall();
        }
        [Route("all")]
        public ActionResult<IEnumerable<TemplateViewModel>> GetTemplates()
        {
            return _templateService.GetAvailableTemplates();
        }

        [Route("paths")]
        public ActionResult<IEnumerable<string>> GetAllPaths()
        {

            return _templateService.GetPaths();
        }

    }
 
}
