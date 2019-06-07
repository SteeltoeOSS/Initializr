using System.Collections.Generic;
using System.Threading.Tasks;
using Steeltoe.Initializr.Models;

namespace Steeltoe.Initializr.Services
{
    public interface ITemplateService
    {
        Task<string> GenerateProject(string TemplateShortName, string ProjectName, string [] TemplateParameters);
        List<TemplateViewModel> GetAvailableTemplates();

        List<ProjectDependency> GetDependencies(string shortName);
      
    }
}