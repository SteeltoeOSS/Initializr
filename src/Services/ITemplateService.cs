using System.Collections.Generic;
using System.Threading.Tasks;
using InitializrApi.Models;

namespace InitializrApi.Services
{
    public interface ITemplateService
    {
      
        Task<string> GenerateProject();
        List<TemplateViewModel> GetAvailableTemplates();
      
    }
}