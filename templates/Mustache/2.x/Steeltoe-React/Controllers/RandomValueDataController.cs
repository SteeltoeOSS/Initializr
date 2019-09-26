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
    public class RandomValueDataController : ControllerBase
    {
    private readonly IConfiguration _config;

    public RandomValueDataController(IConfiguration config)
        {
            _config = config;
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

            configData["random:int"] = _config["random:int"];
            configData["random:long"] = _config["random:long"];
            configData["random:int(10)"] = _config["random:int(10)"];
            configData["random:long(100)"] = _config["random:long(100)"];
            configData["random:int(10,20)"] = _config["random:int(10,20)"];
            configData["random:long(100,200)"] = _config["random:long(100,200)"];
            configData["random:uuid"] = _config["random:uuid"];
            configData["random:string"] = _config["random:string"];
            return configData;
        }
    }
}

