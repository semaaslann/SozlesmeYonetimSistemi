using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SozlesmeSistemi.Models
{
    public class Unit
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Birim adı (örn. "Finans Departmanı")

        [StringLength(500)]
        public string? Description { get; set; } // Birim açıklaması (opsiyonel)

        //public int? ParentUnitId { get; set; } // Üst birim (hiyerarşi için, opsiyonel)
        //public virtual Unit? ParentUnit { get; set; } // Üst birim ilişkisi

        public bool IsActive { get; set; } = true; // Birim aktif mi?

        public DateTime CreatedDate { get; set; } = DateTime.Now; // Oluşturma tarihi
        public DateTime UpdatedDate { get; set; } = DateTime.Now; // Güncelleme tarihi

        // İlişkiler

        public virtual ICollection<User> Users { get; set; } = new List<User>(); // Birimdeki kullanıcılar
        public virtual ICollection<Contract> OurContracts { get; set; } = new List<Contract>(); // Tarafımız olarak sözleşmeler
        public virtual ICollection<Contract> CounterContracts { get; set; } = new List<Contract>(); // Karşı taraf olarak sözleşmeler

        public virtual ICollection<ContractRequest> ContractRequests { get; set; } = new List<ContractRequest>(); // Birimle ilişkili sözleşme talepleri
    }
}