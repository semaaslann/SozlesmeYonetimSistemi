namespace SozlesmeSistemi.Models
{
    public class UserHierarchy
    {
        public int Id { get; set; }

        public int ManagerId { get; set; } // Yöneticinin ID'si
        public User Manager { get; set; }

        public int SubordinateId { get; set; } // Personelin ID'si
        public User Subordinate { get; set; }
    }
}