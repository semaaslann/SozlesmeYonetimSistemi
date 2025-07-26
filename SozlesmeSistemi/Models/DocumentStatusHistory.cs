namespace SozlesmeSistemi.Models
{
    public class DocumentStatusHistory
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime ChangeDate { get; set; }
        public int DocumentId { get; set; }
        public Document Document { get; set; } // Documents ilişkisi
        public int ChangedByUserId { get; set; }
        public User ChangedByUser { get; set; } // Users ilişkisi

        public string? Description { get; set; } // edanın açıklama alanı
    }
}
