namespace SozlesmeSistemi.Models
{
    public class ContractStatusHistory
    {
        public int Id { get; set; }
        public string Status { get; set; } //contracttan gelecek
        public DateTime ChangeDate { get; set; }

        public int ContractId { get; set; }
        public Contract Contract { get; set; } // Contact ilişkisi

 

        public int ChangedByUserId { get; set; }
        public User ChangedByUser { get; set; } // Users ilişkisi



        public string? RejectionReason { get; set; } // Red gerekçesi
    }
}
