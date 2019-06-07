using System.Collections.Generic;
using System.IO;
using Steeltoe.Initializr.Models;

namespace Steeltoe.Initializr.Services
{
    public interface ISteeltoeTemplateService
    {
        byte[] GenerateProject(GeneratorModel model);
        List<string> GetAvailableTemplates();
    }
}