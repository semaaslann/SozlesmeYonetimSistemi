using System.ComponentModel.DataAnnotations.Schema;

namespace SozlesmeSistemi.Models
{
    public class Contract
    {
        public int Id { get; set; }
        public string Title { get; set; } // Sözleşme Adı
        public string Content { get; set; } // Sözleşme İçeriği
        public string SozlesmeKonusu { get; set; } // Yeni: Sözleşme Konusu
        public string SozlesmeTuru { get; set; } // Yeni: Sözleşme Türü
        public string SozlesmeSuresi { get; set; } // Yeni: Sözleşme Süresi
        public DateTime? ImzaOnayTarihi { get; set; } // Yeni: İmza/Onay Tarihi (opsiyonel)
        public decimal SozlesmeTutari { get; set; } // Yeni: Sözleşme Bedeli - Tutar
        public string ParaBirimi { get; set; } // Yeni: Sözleşme Bedeli - Para Birimi
        public string OdemeVadeleri { get; set; } // Yeni: Sözleşme Bedeli - Ödeme Vadeleri
        //public DateTime? EnflasyonArtisTarihi { get; set; } // Yeni: Sözleşme Bedeli - Enflasyon Artış Tarihi (opsiyonel)
        //public string Tarafimiz { get; set; } // Yeni: Tarafımız
        //public string KarsiTaraf { get; set; } // Yeni: Karşı Taraf

        // Örnek olarak, sadece ilgili kısım
        public int OurUnitId { get; set; } // Tarafımız (Birim ID)
        public virtual Unit? OurUnit { get; set; } // Tarafımız birim ilişkisi
        public int CounterUnitId { get; set; } // Karşı Taraf (Birim ID)
        public virtual Unit? CounterUnit { get; set; } // Karşı taraf birim ilişkisi

        //public string SorumluGruplar { get; set; } // Yeni: Sorumlu Gruplar
        public string FesihDurumu { get; set; } // Yeni: Fesih Durumu
        public string FesihDisCozelSart { get; set; } // Yeni: Fesih Dış Çözel Şart
        public string ArsivKlasorNumarasi { get; set; } // Yeni: Arşiv Klasör Numarası
        public string IlgiliIsBirimi { get; set; } // Yeni: İlgili İş Birimi
        public string BuroKategoriNo { get; set; } // Yeni: Büro - Kategori No
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public DateTime? FinisDate { get; set; } // 🌟 Yeni: Bitiş Tarihi


        [ForeignKey("User")]
        public int UserId { get; set; } // Sorumlu Kişi (Users tablosundan çekilecek)
                                        //public User User { get; set; }


        //ebrununki


        public virtual User? User { get; set; } // Navigation property

        //revizeden sonra
        public string? CurrentStatus { get; set; } // Örneğin: "Draft", "Pending", "Approved", "Rejected"

        public int? ContractRequestId { get; set; }

        [ForeignKey("ContractRequestId")]
        public ContractRequest? ContractRequest { get; set; }


        public virtual ICollection<ContractSigners> ContractSigners { get; set; } = new List<ContractSigners>();



        public string? RejectionReason { get; set; } // Red gerekçesi



        public int? ManagerId { get; set; }
        public virtual User? Manager { get; set; } // Nullable yapıldı    }


        public virtual ICollection<ContractSignature> ContractSignatures { get; set; } = new List<ContractSignature>();


        public virtual ICollection<ContractParaf> ContractParafs { get; set; } = new List<ContractParaf>();
    }
}