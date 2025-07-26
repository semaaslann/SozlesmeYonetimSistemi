namespace SozlesmeSistemi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SozlesmeSistemi.Data;
    using SozlesmeSistemi.Models;
    using SozlesmeSistemi.Services;

    public class ReminderController : Controller
    {
        private readonly ReminderService _reminderService;
        private readonly SozlesmeSistemiDbContext _context;

        public ReminderController(ReminderService reminderService, SozlesmeSistemiDbContext context)
        {
            _reminderService = reminderService;
            _context = context;
        }

        public IActionResult Index()
        {
            var reminders = _reminderService.GetUpcomingRemindersWithin30Days();

            // View'e döndürmeden önce deep clone al (SaveChanges yapmadan önce)
            var displayedReminders = reminders.Select(r => new Reminder
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                DueDate = r.DueDate,
                IsCompleted = r.IsCompleted,
                ContractId = r.ContractId,
                Contract = r.Contract,
                IsNew = r.IsNew // Bu hali View'a yansıyacak
            }).ToList();

            // View döndükten sonra IsNew'leri false yap
            foreach (var reminder in reminders.Where(r => r.IsNew))
            {
                reminder.IsNew = false;
            }

            _context.SaveChanges(); // kalıcı hale getir

            return View(displayedReminders);
        }



        // Yeni aksiyon: Satıra tıklandığında IsNew'i false yap
        public IActionResult MarkAsSeen(int id)
        {
            var reminder = _context.Reminders.Find(id);
            if (reminder != null)
            {
                reminder.IsNew = false;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }



    }
}