using SozlesmeSistemi.Data;
using SozlesmeSistemi.Models;

namespace SozlesmeSistemi.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly SozlesmeSistemiDbContext _context;

        public TemplateService(SozlesmeSistemiDbContext context)
        {
            _context = context;
        }

        public List<Template> GetAllTemplates()
        {
            return _context.Templates.ToList();
        }

        public Template GetTemplateById(int id)
        {
            return _context.Templates.FirstOrDefault(t => t.Id == id);
        }


        public void AddTemplate(Template template)
        {
            template.CreatedDate = DateTime.Now;
            _context.Templates.Add(template);
            _context.SaveChanges();
        }

        public void UpdateTemplate(Template template)
        {
            var existingTemplate = _context.Templates.Find(template.Id);
            if (existingTemplate != null)
            {
                existingTemplate.Name = template.Name;
                existingTemplate.Content = template.Content;
                existingTemplate.CreatedDate = template.CreatedDate; // İsteğe bağlı, tarihi değiştirmek istemiyorsanız bu satırı kaldırabilirsiniz
                _context.SaveChanges();
            }
        }

    }

}