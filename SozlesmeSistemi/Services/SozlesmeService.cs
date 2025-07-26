using SozlesmeSistemi.Data;
using SozlesmeSistemi.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace SozlesmeSistemi.Services
{
    public class SozlesmeService
    {
        private readonly SozlesmeSistemiDbContext _context;
        public SozlesmeService(SozlesmeSistemiDbContext context)
        {
            _context = context;
        }

        public void AddContract(Contract contract)
        {

            contract.CurrentStatus = "grup yoneticisi İnceliyor";  // DURUM EKLE
            // User nesnelerini tekrar yükleme (güvenlik için)
            if (contract.UserId > 0 && contract.User == null)
            {
                contract.User = _context.Users.Find(contract.UserId);
            }

        

            contract.CreatedDate = DateTime.Now;
            contract.UpdatedDate = DateTime.Now;
            _context.Contracts.Add(contract);
            _context.SaveChanges();

            // contractStatusHistory kaydı
            var initialStatus = new ContractStatusHistory
            {
                ContractId = contract.Id,
                Status = "grup yoneticisi İnceliyor",
                ChangedByUserId = contract.UserId, // Öncelik: imzalayan kişi
                ChangeDate = DateTime.Now
            };

            _context.ContractStatusHistories.Add(initialStatus);

            _context.SaveChanges();


        }

        public void UpdateContract(Contract contract)
        {
            var existingContract = _context.Contracts.Find(contract.Id);
            if (existingContract != null)
            {
                existingContract.Title = contract.Title;
                existingContract.Content = contract.Content;
                existingContract.ImzaOnayTarihi = contract.ImzaOnayTarihi;
                existingContract.FinisDate = contract.FinisDate;
                existingContract.UserId = contract.UserId;
                existingContract.SozlesmeKonusu = contract.SozlesmeKonusu;
                existingContract.SozlesmeTuru = contract.SozlesmeTuru;

                // Sözleşme süresini otomatik hesapla
                if (contract.ImzaOnayTarihi.HasValue && contract.FinisDate.HasValue)
                {
                    var timeDiff = contract.FinisDate.Value - contract.ImzaOnayTarihi.Value;
                    existingContract.SozlesmeSuresi = $"{(int)timeDiff.TotalDays} gün";
                }

                existingContract.SozlesmeTutari = contract.SozlesmeTutari;
                existingContract.ParaBirimi = contract.ParaBirimi;
                existingContract.OdemeVadeleri = contract.OdemeVadeleri;
                existingContract.OurUnitId = contract.OurUnitId;
                existingContract.CounterUnitId = contract.CounterUnitId;
                existingContract.FesihDurumu = contract.FesihDurumu;
                existingContract.FesihDisCozelSart = contract.FesihDisCozelSart;
                existingContract.ArsivKlasorNumarasi = contract.ArsivKlasorNumarasi;
                existingContract.IlgiliIsBirimi = contract.IlgiliIsBirimi;
                existingContract.BuroKategoriNo = contract.BuroKategoriNo;
                existingContract.CurrentStatus = contract.CurrentStatus; // Bu satırı ekleyin
                //existingContract.RejectionReason = contract.RejectionReason; // Red gerekçesini de güncelle
                existingContract.RejectionReason = existingContract.RejectionReason; // Mevcut değeri korumak için açıkça belirtelim
                existingContract.UpdatedDate = DateTime.Now;

                if (contract.UserId > 0)
                {
                    existingContract.User = _context.Users.Find(contract.UserId);
                }

                _context.SaveChanges();
            }
        }

        public void DeleteContract(int id)
        {
            var sozlesme = _context.Contracts.Find(id);
            if (sozlesme != null)
            {
                _context.Contracts.Remove(sozlesme);
                _context.SaveChanges();
            }
        }

        public List<Contract> GetContracts()
        {
            //return _context.Contracts.ToList() ?? new List<Contract>();
            return _context.Contracts
                .Include(c => c.User)
                    .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .ToList();
        }

        public Contract GetContract(int id)
        {
            return _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .FirstOrDefault(c => c.Id == id);
        }

        public List<User> GetUsers()
        {
            return _context.Users
                .Include(u => u.UserRoles) // UserRoles ilişkisini yükle
                .ToList();
        }

        public User GetUserById(int userId)
        {
            var user = _context.Users
                .Include(u => u.UserRoles) // UserRoles ilişkisini yükle
                .FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                Console.WriteLine($"User with ID {userId} not found at {DateTime.Now}");
            }
            return user;
        }
        public List<Unit> GetUnits()
        {
            return _context.Units.ToList();
        }

        public List<Contract> GetDraftContracts(int userId)
        {
            return _context.Contracts
                .Where(c => c.UserId == userId && c.CurrentStatus == "Draft")
                .Include(c => c.User)
              .Include(c => c.ContractSigners)
                .ThenInclude(cs => cs.User)
                .ToList();
        }




        public void AddContractSigners(int contractId, int userId, string role)
        {
            var contractSigner = new ContractSigners
            {
                ContractId = contractId,
                UserId = userId,
                Role = role
            };
            _context.ContractSigners.Add(contractSigner);
            _context.SaveChanges();
        }









        public bool AreAllSignersSigned(int contractId)
        {
            var signers = _context.ContractSigners
                .Where(cs => cs.ContractId == contractId && cs.Role == "Imzalayan")
                .Select(cs => cs.UserId)
                .ToList();

            var signatures = _context.ContractSignatures
                .Where(cs => cs.ContractId == contractId)
                .Select(cs => cs.UserId)
                .ToList();

            return signers.All(signerId => signatures.Contains(signerId));
        }

        public bool HasUserSigned(int contractId, int userId)
        {
            return _context.ContractSignatures
                .Any(cs => cs.ContractId == contractId && cs.UserId == userId);
        }

        public void AddSignature(int contractId, int userId, string signatureBase64)
        {
            var signature = new ContractSignature
            {
                ContractId = contractId,
                UserId = userId,
                SignatureBase64 = signatureBase64,
                SignedAt = DateTime.Now
            };
            _context.ContractSignatures.Add(signature);
            _context.SaveChanges();
        }



        public void AddParaflayanlar(int contractId, List<int> paraflayanKisiIds)
        {
            // Mevcut paraf kayıtlarını sil
            var existingParafs = _context.ContractParafs
                .Where(p => p.ContractId == contractId)
                .ToList();
            _context.ContractParafs.RemoveRange(existingParafs);

            // Paraflayanları sırayla ekle
            int sira = 1;
            foreach (var userId in paraflayanKisiIds)
            {
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    _context.ContractParafs.Add(new ContractParaf
                    {
                        ContractId = contractId,
                        ParaflayanUserId = userId,
                        Sira = sira++,
                        IsParaflandi = false,
                        ParafTarihi = null,
                        Not = "",
                        IsActive = true
                    });
                }
            }
            _context.SaveChanges();
        }
    }
}