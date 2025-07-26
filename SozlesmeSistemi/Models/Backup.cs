namespace SozlesmeSistemi.Models
{
    public class Backup
    {
        public int Id { get; set; }
        public string BackupName { get; set; }
        public DateTime BackupDate { get; set; }
        public string BackupPath { get; set; }
        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } // Users ilişkisi
    }
}
