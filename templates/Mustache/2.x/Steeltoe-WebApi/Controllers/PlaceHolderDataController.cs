using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace {{ProjectNameSpace}}.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaceholderDataController : ControllerBase
    {
        IOptionsMonitor<SampleOptions> _opts;

        private SampleOptions Options
        {
            get
            {
                return _opts.CurrentValue;
            }
        }

        public PlaceholderDataController(IOptionsMonitor<SampleOptions> opts)
        {
            _opts = opts;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IDictionary<string, string>> Get()
        {
            return Ok(GetPlaceholderData());
        }

        private IDictionary<string, string> GetPlaceholderData()
        {

            var configData = new Dictionary<string, string>();
            configData["ResolvedPlaceholderFromEnvVariables"] = Options.ResolvedPlaceholderFromEnvVariables;
            configData["ResolvedPlaceholderFromJson"] = Options.ResolvedPlaceholderFromJson;
            configData["UnresolvedPlaceholder"] = Options.UnresolvedPlaceholder;
            return configData;
        }
    }
}

