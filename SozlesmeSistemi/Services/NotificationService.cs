using Microsoft.EntityFrameworkCore;
using SozlesmeSistemi.Data;
using SozlesmeSistemi.Models;

namespace SozlesmeSistemi.Services
{
    public class NotificationService
    {
        private readonly SozlesmeSistemiDbContext _context;

        public NotificationService(SozlesmeSistemiDbContext context)
        {
            _context = context;
        }

        // Bildirim ekle
        public void AddNotification(Notification notification)
        {
            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        // Kullanıcının bildirimlerini al
        public List<Notification> GetUserNotifications(int userId)
        {
            return _context.Notifications
                .Include(n => n.Contract)
                .Include(n => n.Sender)
                .Include(n => n.Receiver)
                .Where(n => n.ReceiverId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
        }

        // Bildirim sil
        public void DeleteNotification(int id)
        {
            var notification = _context.Notifications.Find(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                _context.SaveChanges();
            }
        }


        //EKELDİM BAŞLANGIÇ
        public int GetUnreadNotificationCount(int userId)
        {
            return _context.Notifications
                .Count(n => n.ReceiverId == userId && !n.IsRead);
        }
        //EKLEDİM BİTİŞ


    }
}