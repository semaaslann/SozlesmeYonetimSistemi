namespace SozlesmeSistemi.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string ReportType { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string Content { get; set; }
        public int GeneratedByUserId { get; set; }
        public User GeneratedByUser { get; set; } // Users ilişkisi
    }
}
