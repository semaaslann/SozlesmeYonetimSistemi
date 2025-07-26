using Microsoft.AspNetCore.Mvc;
using SozlesmeSistemi.Services;
using System.Threading.Tasks;

namespace SozlesmeSistemi.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet]
        public async Task<IActionResult> CombinedChart(int? year, int? userId)
        {
            int selectedYear = year ?? System.DateTime.Now.Year;

            var (requestLabels, requestData) = await _statisticsService.GetMonthlyRequestCountsAsync(selectedYear, userId);
            var (finishLabels, finishData) = await _statisticsService.GetMonthlyContractFinishCountsAsync(selectedYear, userId);
            var (statusLabels, statusData) = await _statisticsService.GetContractStatusCountsAsync(userId, year);
            var availableYears = await _statisticsService.GetAvailableYearsAsync();
            //var users = await _statisticsService.GetUsersAsync();

            var model = new
            {
                RequestLabels = requestLabels,
                RequestData = requestData,
                FinishLabels = finishLabels,
                FinishData = finishData,
                StatusLabels = statusLabels,
                StatusData = statusData,
                SelectedYear = selectedYear,
                AvailableYears = availableYears,
                //Users = users,
                UserId = userId
            };

            return View(model);
        }
    }
}