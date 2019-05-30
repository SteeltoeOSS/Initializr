using System.Collections.Generic;
using System.Threading.Tasks;
using InitializrApi.Models;

namespace InitializrApi.Services
{
    public interface ITemplateService
    {
        Task<string> GenerateProject(string TemplateShortName, string ProjectName, string [] TemplateParameters);
        List<TemplateViewModel> GetAvailableTemplates();
      
    }
}