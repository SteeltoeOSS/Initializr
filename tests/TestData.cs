using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.Services;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Steeltoe.InitializrTests
{
    public class TestData : IEnumerable<object[]>
    {
        private readonly List<object[]> _data;

        public TestData()
        {
            var settings = new Dictionary<string, string>()
            {
                ["FriendlyNames:CloudFoundry"] = "Cloud Foundry",
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
            var implementations = new ITemplateService[]
            {
                new TemplateService(configuration, new MemoryCache(new MemoryCacheOptions())),
                new MustacheTemplateService( new LoggerFactory().CreateLogger<MustacheTemplateService>(), configuration),
            };
            var templateNames = new string[]
            {
                string.Empty, //test default
                "react",
                "CSharp-WebApi-2.x",
            };
            var data = from implementation in implementations
                        from templateName in templateNames
                        select new object[] { implementation, templateName };
            _data = data.ToList();
        }

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
