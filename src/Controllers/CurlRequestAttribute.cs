using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Steeltoe.Initializr.Controllers
{
    public class IsCurlRequestAttribute : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            var headers = routeContext.HttpContext.Request.Headers;
            return headers["User-Agent"].Any(h => h.ToLower().Contains("curl"));
        }
    }
}
