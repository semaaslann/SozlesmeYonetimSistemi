namespace SozlesmeSistemi.Models
{
    public class ContractAttachment
    {
        public int Id { get; set; }

        public int ContractId { get; set; }
        public virtual Contract Contract { get; set; }

        public int TemplateId { get; set; }
        public virtual Template Template { get; set; }

        public string Content { get; set; } // Değiştirilebilir içeriğin doldurulmuş hali
        public DateTime CreatedDate { get; set; }

    }
}
