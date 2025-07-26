using Microsoft.AspNetCore.Mvc;
using SozlesmeSistemi.Data; // DbContext için
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SozlesmeSistemi.ViewComponents
{
    public class ContractProgressViewComponent : ViewComponent
    {
        private readonly SozlesmeSistemiDbContext _context;

        public ContractProgressViewComponent(SozlesmeSistemiDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int contractId)
        {
            var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == contractId);
            if (contract == null)
            {
                return Content("Sözleşme bulunamadı.");
            }

            return View("Default", contract);


        }
    }
}