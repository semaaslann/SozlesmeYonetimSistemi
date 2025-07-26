using System.ComponentModel.DataAnnotations.Schema;

namespace SozlesmeSistemi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }



        public int UnitId { get; set; }
        public virtual Unit? Unit { get; set; }

        //DÖNENİN EKLEDİĞİ KOD BAŞLANGIÇ
        // Kullanıcıya gelen bildirimler
        public ICollection<Notification> ReceivedNotifications { get; set; }

        // Kullanıcının gönderdiği bildirimler
        public ICollection<Notification> SentNotifications { get; set; }
        ////DÖNENİN EKLEDİĞİ KOD BİTİŞ


        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<UserHierarchy> ManagedHierarchies { get; set; } = new List<UserHierarchy>(); // Yönetici olduğu hiyerarşiler
        public virtual ICollection<UserHierarchy> SubordinateHierarchies { get; set; } = new List<UserHierarchy>(); // Ast olduğu hiyerarşiler
    



    }
}
