using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SozlesmeSistemi.Models;
using SozlesmeSistemi.Services;

namespace SozlesmeSistemi.Controllers
{
    public class TemplateController : Controller
    {
        private readonly ITemplateService _templateService;

        public TemplateController(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet]
        public IActionResult Liste()
        {
            var templates = _templateService.GetAllTemplates();
            return View(templates);
        }

        [HttpGet]
        public IActionResult Ekle()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Ekle(Template template)
        {
            if (ModelState.IsValid)
            {
                _templateService.AddTemplate(template);
                return RedirectToAction("Ekle"); // Aynı sayfaya dön veya listeye
            }

            return View(template);
        }

        // Düzenleme sayfasını açar
        [HttpGet]
        public IActionResult Duzenle(int id)
        {
            var template = _templateService.GetTemplateById(id);
            if (template == null)
            {
                return NotFound();
            }
            return View(template);
        }

        // Düzenleme işlemini kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Duzenle(Template template)
        {
            if (ModelState.IsValid)
            {
                _templateService.UpdateTemplate(template);
                return RedirectToAction("Liste");
            }
            return View(template);
        }
    }
}
