using System.ComponentModel.DataAnnotations.Schema;

namespace SozlesmeSistemi.Models
{
    public class ContractSigners
    {
        public int Id { get; set; }
        public int ContractId { get; set; } // Sözleşme ile ilişki
        public int UserId { get; set; } // Kullanıcı ile ilişki
        public string Role { get; set; } // "Imzalayan" veya "Paraflayan"

        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }



        public virtual Unit Unit { get; set; } // Birim ilişkisi
        public int UnitId { get; set; } // Yeni: Birim ID
    }
}