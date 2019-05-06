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
        ITemplateService _templateService;
        public HomeController(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        //[Route("/")]
        //public IActionResult Index()
        //{
        //    var templates = _templateService.GetAvailableTemplates();
        //    if (Request.Headers["Accept"].Any(x=>x.Contains("text/html")))
        //    {
        //        ViewData.Model = templates;
        //        return View();
        //    }

        //    else
        //    {
        //        return Content("Hi result");
        //    }
        //}

        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}
