using Microsoft.AspNetCore.Mvc;
using SozlesmeSistemi.Services;

namespace SozlesmeSistemi.ViewComponents
{
    public class UnreadNotificationViewComponent : ViewComponent
    {
        private readonly NotificationService _notificationService;

        public UnreadNotificationViewComponent(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public IViewComponentResult Invoke()
        {
            int userId = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            int unreadCount = _notificationService.GetUnreadNotificationCount(userId);
            return View(unreadCount);
        }

    }
}