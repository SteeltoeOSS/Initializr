using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
#if (!NoAuth)
using Microsoft.AspNetCore.Authorization;
#endif
using Microsoft.AspNetCore.Mvc;
#if (SQLServer)
using System.Data.SqlClient;
using System.Data;
#endif
#if (MySql)
using System.Data.MySqlClient;
using System.Data;
#endif
#if (Postgres)
using Npgsql;
using System.Data;
#endif
#if(MongoDB)
using MongoDB.Driver;
using System.Data;
#endif
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
#if (SQLServer)
        private readonly SqlConnection _dbConnection;
        public ValuesController([FromServices] SqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            List<string> tables = new List<string>();
        
            _dbConnection.Open();
            DataTable dt = _dbConnection.GetSchema("Tables");
            _dbConnection.Close();
            foreach (DataRow row in dt.Rows)
            {
                string tablename = (string)row[2];
                tables.Add(tablename);
            }
            return tables;
        }
#endif
#if (MySql)
        private readonly SqlConnection _dbConnection;
        public ValuesController([FromServices] SqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            List<string> tables = new List<string>();

            _dbConnection.Open();
            DataTable dt = _dbConnection.GetSchema("Tables");
            _dbConnection.Close();
            foreach (DataRow row in dt.Rows)
            {
                string tablename = (string)row[2];
                tables.Add(tablename);
            }
            return tables;
        }
#endif
#if (Postgres)
        private readonly NpgsqlConnection _dbConnection;
        public ValuesController([FromServices] NpgsqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            List<string> tables = new List<string>();

            _dbConnection.Open();
            DataTable dt = _dbConnection.GetSchema("Databases");
            _dbConnection.Close();
            foreach (DataRow row in dt.Rows)
            {
                string tablename = (string)row[2];
                tables.Add(tablename);
            }
            return tables;
        }
#endif
#if (MongoDB)
        private readonly IMongoClient _mongoClient;
        private readonly MongoUrl _mongoUrl;
        public ValuesController(IMongoClient mongoClient, MongoUrl mongoUrl)
        {
            _mongoClient = mongoClient;
            _mongoUrl = mongoUrl;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return _mongoClient.ListDatabaseNames().ToList();
        }
#endif
#if (!ValuesControllerWithArgs)
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "value";
        }
#endif

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
