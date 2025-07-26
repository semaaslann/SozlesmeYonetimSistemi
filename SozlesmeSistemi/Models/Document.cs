namespace SozlesmeSistemi.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int ContractId { get; set; }
        public Contract Contract { get; set; } // Contracts ilişkisi

        public string? Description { get; set; } // edanın açıklama alanı
    }
}
