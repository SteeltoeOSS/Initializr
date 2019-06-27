using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Steeltoe.Initializr.Services;
using System.Collections;
using System.Collections.Generic;

namespace Steeltoe.InitializrTests
{
    public class TemplateServiceImplementations : IEnumerable<object[]>
    {
        private readonly List<object[]> _services;

        public TemplateServiceImplementations()
        {
            var settings = new Dictionary<string, string>()
            {
                ["FriendlyNames:CloudFoundry"] = "Cloud Foundry",
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
            _services = new List<object[]>
            {
                new object[] { new TemplateService(configuration, new MemoryCache(new MemoryCacheOptions())) },
                new object[] { new MustacheTemplateService( new LoggerFactory().CreateLogger<MustacheTemplateService>(), configuration) },
            };
        }

        public IEnumerator<object[]> GetEnumerator() => _services.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
