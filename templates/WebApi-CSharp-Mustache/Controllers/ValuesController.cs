using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
{{#Auth}}
using Microsoft.AspNetCore.Authorization;
{{/Auth}}
using Microsoft.AspNetCore.Mvc;

namespace {{ProjectNameSpace }}.Controllers
{
    
{{#Auth}}
    [Authorize]
{{/Auth}}
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger _logger;
       // {{#AnyEFCore}}
        private readonly TestContext _context;
        public ValuesController(ILogger<ValuesController> logger, [FromServices] TestContext context)
        {
            _context = context;
            _logger = logger;
        }
        {{/AnyEFCore}}
        {{^AnyEFCore}}
        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger;
        }
        {{/AnyEFCore}}
        
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            _logger.LogCritical("Test Critical message");
            _logger.LogError("Test Error message");
            _logger.LogWarning("Test Warning message");
            _logger.LogInformation("Test Informational message");
            _logger.LogDebug("Test Debug message");
            _logger.LogTrace("Test Trace message");

            {{#AnyEFCore}}
            return Ok(_context.TestData.Select(x => $"{x.Id}:{x.Data}"));
            {{/AnyEFCore}}
            {{^AnyEFCore}}
            return new string[] { "value1", "value2" };
            {{/AnyEFCore}}

        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {

        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {

        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }
    }
}
