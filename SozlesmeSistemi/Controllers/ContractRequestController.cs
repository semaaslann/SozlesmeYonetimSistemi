using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SozlesmeSistemi.Models;
using SozlesmeSistemi.Services;

namespace SozlesmeSistemi.Controllers
{
    public class ContractRequestController : Controller
    {
        private readonly IContractRequestService _contractRequestService;

        public ContractRequestController(IContractRequestService contractRequestService)
        {
            _contractRequestService = contractRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Index", "Login");

            var units = await _contractRequestService.GetAllUnitsAsync(userId.Value);
            var managers = await _contractRequestService.GetManagersByUserUnitAsync(userId.Value); // Kendi birimine ait yöneticiler

            ViewBag.Units = new SelectList(units, "Id", "Name");
            ViewBag.Managers = new SelectList(managers, "Id", "Username"); // Yönetici dropdown'ı
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ContractRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Index", "Login");

            if (!ModelState.IsValid)
            {
                var units = await _contractRequestService.GetAllUnitsAsync(userId.Value);
                var managers = await _contractRequestService.GetManagersByUserUnitAsync(userId.Value);
                ViewBag.Units = new SelectList(units, "Id", "Name");
                ViewBag.Managers = new SelectList(managers, "Id", "Username");
                return View(request);
            }

            await _contractRequestService.CreateRequestAsync(request, userId.Value);


            //BEN EKLEDİM BAŞLANGIÇ 1
            try
            {
                request.RequestedById = userId.Value;
                request.Status = "Beklemede";
                request.RequestDate = DateTime.Now;

                var realRequest = await _contractRequestService.GetRequestByIdAsync(request.Id);

                var notification = new Notification
                {
                    ContractRequestId = realRequest.Id,
                    SenderId = userId.Value,
                    ReceiverId = realRequest.RequestedToId,
                    Message = $"'{realRequest.Title}' başlıklı bir sözleşme talebi aldınız.",
                    ActionType = "Yeni Talep",
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };

                await _contractRequestService.CreateNotificationAsync(notification);
                //BEN EKLEDİM BİTİŞ 1
                return RedirectToAction("MyRequests");
            }

            //BEN EKLEDİM TEKRAR BAŞLANGIÇ 2
            catch (Exception ex)
            {
                Console.WriteLine(" HATA OLUŞTU: " + ex.Message);
                //return Content("Hata oluştu: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("INNER: " + ex.InnerException.Message);
                return Content("Hata oluştu: " + ex.Message + " | " + ex.InnerException?.Message);
            }
        }
        //BEN EKLEDİM TEKRAR BİTİŞ 2

        public async Task<IActionResult> MyRequests()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Index", "Login");

            var incomingRequests = await _contractRequestService.GetIncomingRequestsAsync(userId.Value);
            var subordinates = await _contractRequestService.GetSubordinatesAsync(userId.Value);
            ViewBag.Subordinates = new SelectList(subordinates, "Id", "Name");
            return View(incomingRequests);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id, int selectedSubordinateId)
        {
            //EKLEDİM BAŞLANGIÇ 3
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Index", "Login");
            //EKLEDİM BİTİŞ 3


            // 1. İsteği onayla
            await _contractRequestService.ApproveRequestAsync(id);

            // 2. Onaylanan isteği seçilen personele ata (yeni metod)
            await _contractRequestService.AssignRequestToUserAsync(id, selectedSubordinateId);

            //EKLEDİM BAŞLANGIÇ 4
            var realRequest = await _contractRequestService.GetRequestByIdAsync(id);

            var notification = new Notification
            {
                ContractRequestId = realRequest.Id,
                SenderId = userId.Value,
                ReceiverId = selectedSubordinateId,
                Message = $"'{realRequest.Title}' başlıklı sözleşme talebi yönetici tarafından onaylandı.",
                ActionType = "Talep Onaylandı",
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            await _contractRequestService.CreateNotificationAsync(notification);
            //EKLEDİM BİTİŞ 4

            return RedirectToAction("MyRequests");
        }


        public async Task<IActionResult> AssignedRequests()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Index", "Login");

            var assignedRequests = await _contractRequestService.GetAssignedRequestsAsync(userId.Value);

            return View(assignedRequests);
        }


        [HttpPost]
        public async Task<IActionResult> Reject(int requestId, string rejectionReason)
        {
            //EKLEDİM BAŞLANGIÇ 5
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Index", "Login");
            //EKLEDİM BİTİŞ 5

            var request = await _contractRequestService.GetRequestByIdAsync(requestId);
            if (request == null) return NotFound();

            request.Status = "Reddedildi";
            request.Subject = rejectionReason;
            await _contractRequestService.UpdateRequestAsync(request);

            //EKLEDİM BAŞLANGIÇ 6
            var notification = new Notification
            {
                ContractRequestId = request.Id,
                SenderId = userId.Value,
                ReceiverId = request.RequestedById,
                Message = $"'{request.Title}' başlıklı talebiniz reddedildi. Gerekçe: {rejectionReason}",
                ActionType = "Talep Reddedildi",
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            await _contractRequestService.CreateNotificationAsync(notification);
            //EKLEDİM BİTİŞ 6

            return RedirectToAction("MyRequests");
        }






        [HttpGet]
        public async Task<IActionResult> GetManagersByUnit(int unitId)
        {
            var users = await _contractRequestService.GetManagersByUnitAsync(unitId);
            var userList = users.Select(u => new { u.Id, u.Username }).ToList();
            return Json(userList);
        }


        [HttpGet]
        public async Task<IActionResult> MySentRequests()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Index", "Login");

                int? userRole = HttpContext.Session.GetInt32("UserRole");
                if (userRole != 4)
                {
                    return Unauthorized("Bu sayfayı görüntüleme yetkiniz yok.");
                }

                var sentRequests = await _contractRequestService.GetSentRequestsAsync(userId.Value);
                ViewBag.UserRole = userRole;
                return View(sentRequests);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "İstekler yüklenirken bir hata oluştu. Lütfen tekrar deneyin.";
                return View(new List<ContractRequest>());
            }
        }
    }
}