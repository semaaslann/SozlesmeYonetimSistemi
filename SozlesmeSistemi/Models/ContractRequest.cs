using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SozlesmeSistemi.Models
{
    public class ContractRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }


        [ForeignKey("RequestedById")]
        public User? RequestedByUser { get; set; }

        public int RequestedById { get; set; } // Yönetici


        [ForeignKey("RequestedToId")]
        public User? RequestedToUser { get; set; }
        public int RequestedToId { get; set; } // Kullanıcı (rolü 3 olan)


        public DateTime RequestDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Beklemede"; // "Beklemede", "Onaylandı", "Reddedildi"

        public int? AssignedToUserId { get; set; }

        [ForeignKey("AssignedToUserId")]
        public User? AssignedToUser { get; set; }





        //public string Subject { get; set; } // Sözleşme Konusu
        public string Justification { get; set; } // Gerekçe / Açıklama

        [Required(ErrorMessage = "Tahmini bitiş tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime EstimatedEndDate { get; set; }




        public int UnitId { get; set; }
        [ForeignKey("UnitId")]
        public Unit? Unit { get; set; }

        public string? Subject {  get; set; }

    }

}