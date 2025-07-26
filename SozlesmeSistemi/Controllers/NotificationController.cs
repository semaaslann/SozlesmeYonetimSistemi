using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SozlesmeSistemi.Data;
using SozlesmeSistemi.Services;

namespace SozlesmeSistemi.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly SozlesmeSistemiDbContext _context;

        public NotificationController(NotificationService notificationService, SozlesmeSistemiDbContext context)
        {
            _notificationService = notificationService;
            _context = context;
        }

        // Bildirim listesi
        public IActionResult Index()
        {
            // Login kullanıcı ID
            var userId = int.Parse(User.FindFirst("UserId").Value);

            var notifications = _notificationService.GetUserNotifications(userId);
            return View(notifications);
        }

        // Bildirimi sil
        [HttpPost]
        public IActionResult Delete(int id)
        {
            _notificationService.DeleteNotification(id);
            return RedirectToAction("Index");
        }

        // Sözleşmeyi İncele butonuna tıklandığında yönlendirme
        public IActionResult ViewContract(int contractId, int notificationId)
        {
            var notification = _context.Notifications.Find(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                _context.Update(notification);
                _context.SaveChanges();
            }

            return RedirectToAction("Details", "Contract", new { id = contractId });
        }

        public IActionResult BildirimDetay(int id)
        {
            var bildirim = _context.Notifications.Find(id);
            if (bildirim == null)
            {
                return NotFound();
            }

            if (!bildirim.IsRead)
            {
                bildirim.IsRead = true;
                _context.Update(bildirim);
                _context.SaveChanges();
            }

            return RedirectToAction("Detay", "Sozlesme", new { id = bildirim.ContractId });
        }

    }



}
