using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Steeltoe.Extensions.Configuration.ConfigServer;
using Microsoft.Extensions.Configuration;

namespace {{ProjectNameSpace}}.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigDataController : ControllerBase
    {
        private IOptionsSnapshot<ConfigServerData> IConfigServerData { get; set; }

        private ConfigServerClientSettingsOptions ConfigServerClientSettingsOptions { get; set; }

        private IConfigurationRoot Config { get; set; }

        public ConfigDataController(IConfigurationRoot config, IOptionsSnapshot<ConfigServerData> configServerData, IOptions<ConfigServerClientSettingsOptions> confgServerSettings)
        {
            // The ASP.NET DI mechanism injects the data retrieved from the Spring Cloud Config Server 
            // as an IOptionsSnapshot<ConfigServerData>. This happens because we added the call to:
            // "services.Configure<ConfigServerData>(Configuration);" in the StartUp class
            if (configServerData != null)
                IConfigServerData = configServerData;

            // The settings used in communicating with the Spring Cloud Config Server
            if (confgServerSettings != null)
                ConfigServerClientSettingsOptions = confgServerSettings.Value;

            Config = config;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IDictionary<string, string>> Get()
        {
            return Ok(CreateConfigServerData());
        }

        [HttpPost]
        public IActionResult Reload()
        {
            if (Config != null)
            {
                Config.Reload();
            }
        return Ok();
        }

        private IDictionary<string, string> CreateConfigServerData()
        {

            var configData = new Dictionary<string, string>();

            // IConfigServerData property is set to a IOptionsSnapshot<ConfigServerData> that has been
            // initialized with the configuration data returned from the Spring Cloud Config Server
            if (IConfigServerData != null && IConfigServerData.Value != null)
            {
                var data = IConfigServerData.Value;
                configData["Bar"] = data.Bar ?? "Not returned";
                configData["Foo"] = data.Foo ?? "Not returned";

                configData["Info.Url"] = "Not returned";
                configData["Info.Description"] = "Not returned";

                if (data.Info != null)
                {
                    configData["Info.Url"] = data.Info.Url ?? "Not returned";
                    configData["Info.Description"] = data.Info.Description ?? "Not returned";
                }
            }
            else
            {
                configData["Bar"] = "Not Available";
                configData["Foo"] = "Not Available";
                configData["Info.Url"] = "Not Available";
                configData["Info.Description"] = "Not Available";
            }

            return configData;
        }
    }
}

