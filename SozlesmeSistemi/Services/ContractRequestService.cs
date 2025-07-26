using Microsoft.EntityFrameworkCore;
using SozlesmeSistemi.Data;
using SozlesmeSistemi.Models;

namespace SozlesmeSistemi.Services
{
    public class ContractRequestService : IContractRequestService
    {
        private readonly SozlesmeSistemiDbContext _context;

        public ContractRequestService(SozlesmeSistemiDbContext context)
        {
            _context = context;
        }


  

        // yeni sözlesme isteği oluşturur. Requestbyıd parametresi talebin kimin yaptıgı, status beklemede olarak başlar
        public async Task CreateRequestAsync(ContractRequest request, int requestedById)
        {
            request.RequestedById = requestedById;
            request.Status = "Beklemede";
            request.RequestDate = DateTime.Now;

            _context.ContractRequests.Add(request);
            await _context.SaveChangesAsync();
        }


        // bir kullanıcıya gelen bekleyen sözleşme isteklerini getirir.
        public async Task<List<ContractRequest>> GetIncomingRequestsAsync(int userId)
        {
            return await _context.ContractRequests
                .Include(r => r.RequestedByUser)
                .Include(r => r.RequestedToUser)
                .Include(r => r.Unit) // <-- Birimi dahil ettik
                .Where(r => r.RequestedToId == userId && r.Status == "Beklemede")
                .ToListAsync();
        }


        // request ıd ile bulunan isteği onaylandı durumuna getirir.
        public async Task ApproveRequestAsync(int requestId)
        {
            var request = await _context.ContractRequests.FindAsync(requestId);
            if (request == null) return;

            request.Status = "Onaylandı";
            await _context.SaveChangesAsync();

            // Yöneticiye bağlı personelleri getir
            var subordinates = await _context.UserHierarchies
                .Where(h => h.ManagerId == request.RequestedToId) // sözleşmeyi alan kişi yönetici
                .Select(h => h.Subordinate)
                .ToListAsync();


        }
        //gelen isteğin onay kısmına dropdown eklendi ,Yönetici altındaki personel listesi
        public async Task<List<User>> GetSubordinatesAsync(int managerId)
        {
            return await _context.UserHierarchies
                .Where(h => h.ManagerId == managerId)
                .Select(h => h.Subordinate)
                .ToListAsync();
        }

        // onaylanmıs bir isteği belirli kullanıcıya atar.
        public async Task AssignRequestToUserAsync(int requestId, int userId)
        {
            var request = await _context.ContractRequests.FindAsync(requestId);
            if (request == null) return;

            request.AssignedToUserId = userId;
            await _context.SaveChangesAsync();
        }


        // belirli kullanıcıya  atanmıs ve onaylandı durumunda olan sözleşme isteklerini getirir.
        public async Task<List<ContractRequest>> GetAssignedRequestsAsync(int userId)
        {
            return await _context.ContractRequests
           .Include(r => r.Unit) // Birimi dahil et
           .Include(r => r.RequestedByUser) // Göndereni dahil et
           .Include(r => r.RequestedToUser) // Alıcıyı dahil et
           .Include(r => r.AssignedToUser) // Atanan kullanıcıyı dahil et
           .Where(r => r.AssignedToUserId == userId && r.Status == "Onaylandı")
           .ToListAsync();
        }

        //  isteği id ye göre getirir. 

        public async Task<ContractRequest> GetRequestByIdAsync(int requestId)
        {
            return await _context.ContractRequests
                .Include(r => r.RequestedByUser)
                .Include(r => r.RequestedToUser)
                .FirstOrDefaultAsync(r => r.Id == requestId);
        }


        // contract request listesini günceller

        public async Task UpdateRequestAsync(ContractRequest request)
        {
            _context.ContractRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        // Teknik ekibin kendi birimi dışındaki birimleri getirir
        public async Task<List<Unit>> GetAllUnitsAsync(int userId)
        {
            // Kullanıcının birimini al
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.UnitId })
                .FirstOrDefaultAsync();

            if (user == null)
                return new List<Unit>(); // Kullanıcı bulunamazsa boş liste döndür

            // Kullanıcının birimi dışındaki birimleri getir
            return await _context.Units
                .Where(u => u.Id != user.UnitId)
                .ToListAsync();
        }


        //birimlere bağlı yoneticileri getirir
        public async Task<List<User>> GetManagersByUnitAsync(int unitId)
        {
            return await _context.UserRoles
                .Where(ur => ur.RoleId == 1 && ur.User.UnitId == unitId) // RoleId = 1 (Yönetici) ve birim eşleşmesi
                .Select(ur => ur.User)
                .ToListAsync();
        }

        public async Task<List<ContractRequest>> GetSentRequestsAsync(int userId)
        {
            // Kullanıcının birimini ve rolünü al
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.UnitId,
                    RoleId = u.UserRoles.Any() ? u.UserRoles.First().RoleId : 0 // Null check without ?.
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return new List<ContractRequest>();

            // Teknik ekip (RoleId = 4) için kendi birimine ait istekleri getir
            if (user.RoleId == 4)
            {
                return await _context.ContractRequests
                    .Include(r => r.RequestedByUser)
                    .Include(r => r.RequestedToUser)
                    .Include(r => r.Unit) // Karşı birim
                    .Where(r => r.RequestedByUser.UnitId == user.UnitId) // Kendi birimi tarafından oluşturulan istekler
                    .Select(r => new ContractRequest
                    {
                        Id = r.Id,
                        RequestedById = r.RequestedById,
                        RequestedByUser = r.RequestedByUser, // Gönderen kişi
                        RequestedToId = r.RequestedToId,
                        RequestedToUser = r.RequestedToUser, // Sorumlu kişi
                        UnitId = r.UnitId,
                        Unit = r.Unit, // Karşı birim
                        Status = r.Status,
                        Justification = r.Justification ?? "-",
                        RequestDate = r.RequestDate,
                        Title = r.Title,
                        Content = r.Content,
                        //Subject = r.Subject,
                        EstimatedEndDate = r.EstimatedEndDate
                    })
                    .ToListAsync();
            }

            // Diğer roller için mevcut mantığı koru
            return await _context.ContractRequests
                .Include(r => r.RequestedByUser)
                .Include(r => r.RequestedToUser)
                .Include(r => r.Unit)
                .Where(r => r.RequestedById == userId)
                .Select(r => new ContractRequest
                {
                    Id = r.Id,
                    RequestedById = r.RequestedById,
                    RequestedByUser = r.RequestedByUser,
                    RequestedToId = r.RequestedToId,
                    RequestedToUser = r.RequestedToUser,
                    UnitId = r.UnitId,
                    Unit = r.Unit,
                    Status = r.Status,
                    Justification = r.Justification ?? "-",
                    RequestDate = r.RequestDate,
                    Title = r.Title,
        
                    Content = r.Content,
                    //Subject = r.Subject,
                    EstimatedEndDate = r.EstimatedEndDate
                })
                .ToListAsync();
        }





        public async Task<int?> GetUserUnitIdAsync(int userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.UnitId })
                .FirstOrDefaultAsync();

            return user?.UnitId;
        }


        public async Task<List<User>> GetManagersByUserUnitAsync(int userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.UnitId })
                .FirstOrDefaultAsync();

            if (user == null)
                return new List<User>();

            return await _context.UserRoles
                .Where(ur => ur.RoleId == 1 && ur.User.UnitId == user.UnitId) // Kullanıcının birimine ait yöneticiler
                .Select(ur => ur.User)
                .ToListAsync();
        }



        public async Task CreateNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

    }
}