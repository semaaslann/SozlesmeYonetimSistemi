namespace SozlesmeSistemi.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; }

        public int ReceiverId { get; set; }
        public User Receiver { get; set; }

        public string Message { get; set; }
        public string ActionType { get; set; } // Ör: "Sözleşme Eklendi", "Sözleşme Güncellendi

        public int? ContractId { get; set; }
        public Contract? Contract { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;//İlk başta okunmayacak

        public int? ContractRequestId { get; set; } // Eğer talep üzerinden geliyorsa
        public ContractRequest? ContractRequest { get; set; }


    }
}