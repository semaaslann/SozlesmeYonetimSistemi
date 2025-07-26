using Microsoft.EntityFrameworkCore;
using SozlesmeSistemi.Data;
using SozlesmeSistemi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace SozlesmeSistemi.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly SozlesmeSistemiDbContext _context;

        public StatisticsService(SozlesmeSistemiDbContext context)
        {
            _context = context;
        }

        public async Task<(string[] Labels, int[] Data)> GetMonthlyRequestCountsAsync(int year, int? userId = null)
        {
            var query = _context.ContractRequests
                .Where(r => r.RequestDate.Year == year);

            if (userId.HasValue)
            {
                query = query.Where(r => r.RequestedById == userId.Value || r.RequestedToId == userId.Value || r.AssignedToUserId == userId.Value);
            }

            var requests = await query
                .GroupBy(r => r.RequestDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var monthlyData = Enumerable.Range(1, 12).Select(month => new
            {
                Month = month,
                Count = requests.FirstOrDefault(r => r.Month == month)?.Count ?? 0
            }).ToList();

            return (
                Labels: monthlyData.Select(m => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m.Month)).ToArray(),
                Data: monthlyData.Select(m => m.Count).ToArray()
            );
        }

        public async Task<(string[] Labels, int[] Data)> GetMonthlyContractFinishCountsAsync(int year, int? userId = null)
        {
            var query = _context.Contracts
                .Where(c => c.FinisDate.HasValue && c.FinisDate.Value.Year == year);

            if (userId.HasValue)
            {
                query = query.Where(c => c.UserId == userId.Value || c.ManagerId == userId.Value);
            }

            var finishCounts = await query
                .GroupBy(c => c.FinisDate.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            int[] counts = new int[12];
            foreach (var item in finishCounts)
            {
                counts[item.Month - 1] = item.Count;
            }

            var labels = new[] { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };
            return (Labels: labels, Data: counts);
        }

        public async Task<(string[] Labels, int[] Data)> GetContractStatusCountsAsync(int? userId = null, int? year = null)
        {
            var query = _context.Contracts.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(c => c.UserId == userId.Value || c.ManagerId == userId.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(c => c.FinisDate.HasValue && c.FinisDate.Value.Year == year.Value);
            }

            var statusCounts = await query
                .GroupBy(c => c.CurrentStatus)
                .Select(g => new
                {
                    Status = g.Key ?? "Bilinmeyen",
                    Count = g.Count()
                })
                .ToListAsync();

            var allStatuses = new List<string> { "grup yöneticisi onayladı", "Karşı Birim Onayında", "İncelendi", "Paraflandı", "İmzalandı", "Tamamlandı","Karşı Birim Personel İnceliyor","Diğer" };
            var result = allStatuses
                .Select(status => (
                    Status: status,
                    Count: statusCounts.FirstOrDefault(sc => sc.Status == status)?.Count ?? 0
                ))
                .Where(sc => sc.Count > 0)
                .ToList();

            return (
                Labels: result.Select(r => r.Status).ToArray(),
                Data: result.Select(r => r.Count).ToArray()
            );
        }

        public async Task<List<int>> GetAvailableYearsAsync()
        {
            return await _context.Contracts
                .Where(c => c.FinisDate.HasValue)
                .Select(c => c.FinisDate.Value.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToListAsync();
        }

        //public async Task<List<User>> GetUsersAsync()
        //{
        //    return await _context.Users
        //        .Select(u => new User { Id = u.Id, Name = u.Name }) // Name yerine doğru alanı kullanın (ör. u.UserName, u.FullName)
        //        .ToListAsync();
        //}
    }
}