using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using InitializrApi.Services;
using Microsoft.AspNetCore.Mvc;
using testDir.Models;

namespace InitializrApi.Controllers
{
    public class HomeController : Controller
    {
        ISteeltoeTemplateService _templateService;
        public HomeController(ISteeltoeTemplateService templateService)
        {
            _templateService = templateService;
        }
        //[Route("/all")]
        //public IActionResult Index()
        //{
        //    var templates = _templateService.GetAvailableTemplates();
        //    if (Request.Headers["Accept"].Any(x => x.Contains("text/html")))
        //    {
        //        ViewData.Model = templates;
        //        return View();
        //    }

        //    else
        //    {
        //        return Content("Hi result");
        //    }
        //}
    }
}
