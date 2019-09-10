using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Steeltoe.Initializr.Tests
{
    public class TestHelper
    {
        public static IConfigurationRoot GetConfiguration()
        {
            var settings = new Dictionary<string, string>()
            {
                ["FriendlyNames:CloudFoundry"] = "Cloud Foundry",
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }
    }
}
