using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
{{#Auth}}
using Microsoft.AspNetCore.Authorization;
{{/Auth}}
using Microsoft.AspNetCore.Mvc;
{{#SQLServer}}
using System.Data.SqlClient;
using System.Data;
{{/SQLServer}}
{{#MySql}}
using System.Data.MySqlClient;
using System.Data;
{{/MySql}}
{{#Postgres}}
using Npgsql;
using System.Data;
{{/Postgres}}
{{#MongoDB}}
using MongoDB.Driver;
using System.Data;
{{/MongoDB}}
{{#Redis}}
using Microsoft.Extensions.Caching.Distributed;
{{/Redis}}
{{#RabbitMQ}}
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
{{/RabbitMQ}}
namespace {{ProjectNameSpace}}.Controllers
{
    {{#Auth}}
    [Authorize]
    {{/Auth}}
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
{
        {{#SQLServer}}
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
        {{/SQLServer}}
        {{#MySql}}
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
        {{/MySql}}
        {{#Postgres}}
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
        {{/Postgres}}
        {{#MongoDB}}
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
        {{/MongoDB}}
        {{#Redis}}
        private readonly IDistributedCache _cache;
        public ValuesController(IDistributedCache cache)
        {
            _cache = cache;
        }

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            await _cache.SetStringAsync("MyValue1", "123");
            await _cache.SetStringAsync("MyValue2", "456");
            string myval1 = await _cache.GetStringAsync("MyValue1");
            string myval2 = await _cache.GetStringAsync("MyValue2");
            return new string[]{ myval1, myval2};
        }
        {{/Redis}}
        {{#RabbitMQ}}
        private readonly ILogger _logger;
        private readonly ConnectionFactory _factory;
        private const string queueName = "my-queue";
        public ValuesController(ILogger<ValuesController> logger, [FromServices] ConnectionFactory factory)
        {
            _logger = logger;
            _factory = factory;
        }
        
        // GET api/values
        [HttpGet]
        public ActionResult<string> Get()
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //the queue
                channel.QueueDeclare(queue: queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
                // consumer
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    string msg = Encoding.UTF8.GetString(ea.Body);
                    _logger.LogInformation("Received message: " + msg);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);
                // publisher
                int i = 0;
                while (i<5) { //write a message every second, for 5 seconds
                    var body = Encoding.UTF8.GetBytes($"Message {++i}");
                    channel.BasicPublish(exchange: "",
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: body);
                    Thread.Sleep(1000);
                }
            }
            return "Wrote 5 message to the info log. Have a look!";
        }
        {{/RabbitMQ}}
        {{^ValuesControllerWithArgs}}
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "value";
        }
        {{/ValuesControllerWithArgs}}
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
