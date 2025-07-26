namespace SozlesmeSistemi.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public int ContractId { get; set; }
        public Contract Contract { get; set; }


        public bool IsNew { get; set; } // Yeni özellik


    }
}