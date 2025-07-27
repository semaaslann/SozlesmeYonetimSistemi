using SozlesmeSistemi.Models;

namespace SozlesmeSistemi.Services
{
    public interface ITemplateService
    {
        List<Template> GetAllTemplates();
        Template GetTemplateById(int id);
        void AddTemplate(Template template);
        void UpdateTemplate(Template template);
    }
}
