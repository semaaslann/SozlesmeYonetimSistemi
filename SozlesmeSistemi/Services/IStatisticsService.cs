using SozlesmeSistemi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SozlesmeSistemi.Services
{
    public interface IStatisticsService
    {
        Task<(string[] Labels, int[] Data)> GetMonthlyRequestCountsAsync(int year, int? userId = null);
        Task<(string[] Labels, int[] Data)> GetMonthlyContractFinishCountsAsync(int year, int? userId = null);
        Task<(string[] Labels, int[] Data)> GetContractStatusCountsAsync(int? userId = null, int? year = null);
        Task<List<int>> GetAvailableYearsAsync();
        //Task<List<User>> GetUsersAsync();
    }
}