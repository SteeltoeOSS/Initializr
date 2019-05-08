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
    }
}
