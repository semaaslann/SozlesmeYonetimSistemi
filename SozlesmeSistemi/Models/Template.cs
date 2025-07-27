namespace SozlesmeSistemi.Models
{
    public class Template
    {
        public int Id { get; set; }
        public string Name { get; set; } // Örnek: "Gizlilik Eki"
        public string Content { get; set; } // HTML içinde {{Değişken}} şeklinde alanlar
        public DateTime CreatedDate { get; set; } // Yeni eklenen alan

    }
}
