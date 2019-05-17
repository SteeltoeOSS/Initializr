using System.Collections.Generic;
using InitializrApi.Models;

namespace InitializrApi.Services
{
    public interface ITemplateService
    {
        string DebugReinstall();
        string GenerateProject(GeneratorModel model);
        List<TemplateViewModel> GetAvailableTemplates();
        List<string> GetPaths();
    }
}