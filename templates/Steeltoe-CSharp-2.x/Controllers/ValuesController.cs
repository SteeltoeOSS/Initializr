using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if (!NoAuth)
using Microsoft.AspNetCore.Authorization;
#endif
using Microsoft.AspNetCore.Mvc;

namespace Company.WebApplication1.Controllers
{
#if (!NoAuth)
    [Authorize]
#endif
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger _logger;
#if (AnyEFCore)
        private readonly TestContext _context;
        public ValuesController(ILogger<ValuesController> logger, [FromServices] TestContext context)
        {
            _context = context;
            _logger = logger;
        }
#else
        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger;
        }

#endif
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


#if(AnyEFCore)
               return Ok(_context.TestData.Select(x => $"{x.Id}:{x.Data}"));
 else
               return new string[] { "value1", "value2" };
#endif
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
#if (OrganizationalAuth || WindowsAuth)
            // For more information on protecting this API from Cross Site Request Forgery (CSRF) attacks, see https://go.microsoft.com/fwlink/?LinkID=717803
#endif
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
#if (OrganizationalAuth || WindowsAuth)
            // For more information on protecting this API from Cross Site Request Forgery (CSRF) attacks, see https://go.microsoft.com/fwlink/?LinkID=717803
#endif
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
#if (OrganizationalAuth || WindowsAuth)
            // For more information on protecting this API from Cross Site Request Forgery (CSRF) attacks, see https://go.microsoft.com/fwlink/?LinkID=717803
#endif
        }
    }
}
