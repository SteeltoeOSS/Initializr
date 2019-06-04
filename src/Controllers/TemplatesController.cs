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
        private ISteeltoeTemplateService _sttemplateService;
        private const string DEFAULT_TEMPLATE = "steeltoe2";

        public TemplatesController(ITemplateService service, ISteeltoeTemplateService stTemplateService)
        {
            _templateService = service;
            _sttemplateService = stTemplateService;
        }

        // GET api/Template
        [Route("/starter.zip")]
        [HttpPost]
        public ActionResult GenerateProjectPost([FromForm] GeneratorModel model)
        {
          
             return GenerateProject(model);
            //else
            //{
            //    var bytes = _sttemplateService.GenerateProject(model);
            //    return File(bytes, "application/zip");
            //}
        }

        [Route("/createtest")]
        public ActionResult GenerateProjectTest([FromQuery(Name = "templateShortName")] string templateShortName)
        {
            return GenerateProject(new GeneratorModel { templateShortName = templateShortName ?? DEFAULT_TEMPLATE, projectName = "mytest", dependencies = new[] { "actuators,mysql" } }); ;
        }

        private ActionResult GenerateProject(GeneratorModel model)
        {
            //var form = Request.Form;
            var list = _templateService.GetAvailableTemplates();
            var currentTemplate = (model.templateShortName ?? DEFAULT_TEMPLATE).ToLower();


            if (list == null || !list.Any(x => x.ShortName.ToLower() == currentTemplate))
            {
                return NotFound($"Template {currentTemplate} was not found");
            }
            var templateParameters = model.dependencies.ToList();
            if(!string.IsNullOrEmpty(model.steeltoeVersion))
            {
                templateParameters.Add($"SteeltoeVersion={model.steeltoeVersion}");
            }
            string outFolder =  _templateService.GenerateProject(currentTemplate, model.projectName, templateParameters.ToArray() ).Result;
            var zipName = (model.projectName ?? "steeltoeProject")+".zip";

            var zipFile = Path.Combine(outFolder,"..", zipName);
            ZipFile.CreateFromDirectory(outFolder, zipFile);

            var cd = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = zipName
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return File(System.IO.File.ReadAllBytes(zipFile), "application/zip");

        }
        // GET api/templates
        [Route("/dependencies")]
        public ActionResult GetDependencies()
        {
            return Ok( _templateService.GetDependencies("steeltoe"));
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

       

    }
 
}
