namespace SozlesmeSistemi.Models
{
    public class ContractParaf
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int ParaflayanUserId { get; set; }
        public int Sira { get; set; }
        public bool IsParaflandi { get; set; }
        public DateTime? ParafTarihi { get; set; }
        public string Not { get; set; }
        public bool IsActive { get; set; }
        public virtual Contract Contract { get; set; }
        public virtual User ParaflayanUser { get; set; }
    }
}