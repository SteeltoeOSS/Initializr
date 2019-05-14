using System.Collections.Generic;
using System.IO;
using InitializrApi.Models;

namespace InitializrApi.Services
{
    public interface ISteeltoeTemplateService
    {
        byte[] GenerateProject(GeneratorModel model);
        List<string> GetAvailableTemplates();
    }
}