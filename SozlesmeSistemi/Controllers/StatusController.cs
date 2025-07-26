using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SozlesmeSistemi.Data;
using SozlesmeSistemi.Models;
using SozlesmeSistemi.Services;
using System.Linq;

namespace SozlesmeSistemi.Controllers
{
    public class StatusController : Controller
    {
        private readonly SozlesmeSistemiDbContext _context;
        private readonly IContractStatusService _contractStatusService;
        private readonly NotificationService _notificationService; // NotificationService eklendi
        private readonly SozlesmeService _sozlesmeService;

        public StatusController(SozlesmeSistemiDbContext context, IContractStatusService contractStatusService, NotificationService notificationService, SozlesmeService sozlesmeService)
        {
            _context = context;
            _contractStatusService = contractStatusService;
            _notificationService = notificationService; // Constructor'a eklendi
            _sozlesmeService = sozlesmeService;

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Incele(int contractId)
        {
            var contract = _context.Contracts
                .Include(c => c.ContractSigners)
                .FirstOrDefault(c => c.Id == contractId);

            if (contract == null)
            {
                return NotFound();
            }

            // Sözleşmenin imzalayan kişilerini kontrol et
            var imzalayanKisiler = contract.ContractSigners
                .Where(cs => cs.Role == "Imzalayan")
                .Select(cs => cs.UserId)
                .ToList();

            if (!imzalayanKisiler.Any())
            {
                ModelState.AddModelError("", "İmzalayan kişi atanmadığı için inceleme yapılamaz.");
                return RedirectToAction("IncelemeBekleyenler");
            }

            // İlk imzalayan kişiyi al (veya başka bir mantık kullanılabilir)
            int userId = imzalayanKisiler.First();

            try
            {
                _contractStatusService.AddStatus(contractId, "İncelendi", userId);
                return RedirectToAction("IncelemeBekleyenler");
            }
            catch
            {
                return RedirectToAction("IncelemeBekleyenler");
            }
        }

        [HttpGet]
        public IActionResult Imzala(int contractId)
        {
            var contract = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .FirstOrDefault(c => c.Id == contractId);

            if (contract == null)
            {
                return NotFound();
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            // Kullanıcının imzalayanlar arasında olup olmadığını kontrol et
            var isSigner = contract.ContractSigners
                .Any(cs => cs.Role == "Imzalayan" && cs.UserId == currentUserId.Value);
            if (!isSigner)
            {
                return Unauthorized("Bu sözleşmeyi imzalama yetkiniz yok.");
            }

            // Kullanıcının zaten imzalayıp imzalamadığını kontrol et
            if (_sozlesmeService.HasUserSigned(contractId, currentUserId.Value))
            {
                return RedirectToAction("Imzalananlar");
            }

            ViewBag.ContractId = contractId;
            ViewBag.CurrentUserId = currentUserId.Value;
            ViewBag.ImzalayanKisiler = contract.ContractSigners
                .Where(cs => cs.Role == "Imzalayan")
                .Select(cs => cs.User.Username)
                .ToList();

            return View(contract);
        }
        [HttpGet]
        public IActionResult IncelemeBekleyenler()
        {
            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            System.Diagnostics.Debug.WriteLine($"UserIdClaim: {userIdClaim}");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                System.Diagnostics.Debug.WriteLine("Kullanıcı kimliği bulunamadı.");
                TempData["ErrorMessage"] = "Kullanıcı kimliği bulunamadı.";
                return RedirectToAction("Index", "Login");
            }

            var contracts = _context.Contracts
                .Include(r => r.OurUnit)
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                .ThenInclude(cs => cs.User)
                .Where(c => c.CurrentStatus == "Karşı Birim Onayında" && c.ManagerId == userId)
                .ToList() ?? new List<Contract>();

            System.Diagnostics.Debug.WriteLine($"Contracts Count: {contracts.Count}");
            if (!contracts.Any())
            {
                ViewBag.Message = "İnceleme bekleyen sözleşme bulunamadı.";
            }

            return View(contracts);
        }
        [HttpGet]
        public IActionResult ImzaBekleyenler()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var contracts = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .Where(c => (c.CurrentStatus == "İncelendi" || c.CurrentStatus == "Kısmen İmzalandı" || c.CurrentStatus == "İmzalanmayı Bekliyor")
                    && c.ContractSigners.Any(cs => cs.Role == "Imzalayan" && cs.UserId == userId.Value
                        && !_context.ContractSignatures.Any(sig => sig.ContractId == c.Id && sig.UserId == userId.Value)))
                .ToList();

            if (!contracts.Any())
            {
                ViewBag.Message = "İmza bekleyen sözleşme bulunamadı.";
            }

            return View(contracts);
        }
        [HttpGet]
        public IActionResult Imzalananlar()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var contracts = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .Where(c => c.CurrentStatus == "İmzalandı" &&
                    (c.UserId == userId || c.ContractSigners.Any(cs => cs.Role == "Imzalayan" && cs.UserId == userId.Value)))
                .ToList();

            if (!contracts.Any())
            {
                ViewBag.Message = "İmzalanmış sözleşme bulunamadı.";
            }

            return View(contracts);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Kontrol(int contractId)
        {
            var contract = _context.Contracts.FirstOrDefault(c => c.Id == contractId);
            if (contract == null)
            {
                return NotFound();
            }

            int userId = contract.UserId;

            try
            {
                _contractStatusService.AddStatus(contractId, "Tamamlandı", userId);
                return RedirectToAction("Imzalananlar");
            }
            catch
            {
                return RedirectToAction("Imzalananlar");
            }
        }

        public IActionResult Incelenenler()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var contracts = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .Where(c => c.CurrentStatus == "Tamamlandı" && c.UserId == userId)
                .ToList();

            return View(contracts);
        }









        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Onayla(int contractId)
        {
            var contract = _context.Contracts
                .Include(c => c.ContractSigners)
                .Include(c => c.CounterUnit)
                .Include(c => c.User)
                .FirstOrDefault(c => c.Id == contractId);

            if (contract == null)
            {
                System.Diagnostics.Debug.WriteLine($"Hata: Sözleşme ID {contractId} bulunamadı.");
                TempData["ErrorMessage"] = "Sözleşme bulunamadı.";
                return RedirectToAction("OnaylanacakSozlesmeler", "Sozlesme");
            }

            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                System.Diagnostics.Debug.WriteLine("Hata: Kullanıcı kimliği bulunamadı.");
                TempData["ErrorMessage"] = "Kullanıcı kimliği bulunamadı.";
                return Unauthorized("Kullanıcı kimliği bulunamadı.");
            }

            try
            {
                // Karşı birim yöneticisi mi kontrol et
                var isCounterUnitManager = _context.UserHierarchies
                    .Any(h => h.ManagerId == userId && h.Subordinate.UnitId == contract.CounterUnitId);

                if (isCounterUnitManager && string.Equals(contract.CurrentStatus, "Karşı Birim Onayında", StringComparison.OrdinalIgnoreCase))
                {
                    // Karşı birim yöneticisi onaylıyor
                    _contractStatusService.AddStatus(contractId, "Karşı Birim Onayladı", userId);
                    contract.CurrentStatus = "İncelendi"; // İmzalama sürecine ilerle
                    _context.SaveChanges();

                    // Sözleşme sahibine bildirim
                    var notificationToOwner = new Notification
                    {
                        ReceiverId = contract.UserId,
                        SenderId = userId,
                        ContractId = contract.Id,
                        ActionType = "Sözleşme Onaylandı",
                        Message = $"Karşı birim tarafından '{contract.Title}' adlı sözleşme onaylandı.",
                        CreatedDate = DateTime.Now
                    };
                    _notificationService.AddNotification(notificationToOwner);

                    // İmzalayan kişilere bildirim
                    var imzalayanlar = contract.ContractSigners
                        .Where(cs => cs.Role == "Imzalayan")
                        .Select(cs => cs.UserId)
                        .ToList();

                    foreach (var signerId in imzalayanlar)
                    {
                        var notificationToSigner = new Notification
                        {
                            ReceiverId = signerId,
                            SenderId = userId,
                            ContractId = contract.Id,
                            ActionType = "Sözleşme İnceleme",
                            Message = $"'{contract.Title}' adlı sözleşme imzalama için bekliyor.",
                            CreatedDate = DateTime.Now
                        };
                        _notificationService.AddNotification(notificationToSigner);
                    }

                    TempData["SuccessMessage"] = "Sözleşme karşı birim tarafından onaylandı ve imzalama sürecine ilerledi.";
                    return RedirectToAction("IncelemeBekleyenler");
                }
                // Mevcut birim yöneticisi mi kontrol et
                else if (contract.ManagerId == userId && string.Equals(contract.CurrentStatus, "grup yoneticisi İnceliyor", StringComparison.OrdinalIgnoreCase))
                {
                    // Mevcut birim yöneticisi onaylıyor
                    _contractStatusService.AddStatus(contractId, "Grup Yöneticisi Onayladı", userId);

                    var notificationToOwner = new Notification
                    {
                        ReceiverId = contract.UserId,
                        SenderId = userId,
                        ContractId = contract.Id,
                        ActionType = "Sözleşme Onaylandı",
                        Message = $"'{contract.Title}' adlı sözleşme onaylandı.",
                        CreatedDate = DateTime.Now
                    };
                    _notificationService.AddNotification(notificationToOwner);

                    var counterUnitManager = _context.UserHierarchies
                        .Where(h => h.Subordinate.UnitId == contract.CounterUnitId)
                        .Select(h => h.Manager)
                        .FirstOrDefault();

                    if (counterUnitManager != null)
                    {
                        contract.ManagerId = counterUnitManager.Id;
                        _contractStatusService.AddStatus(contractId, "Karşı Birim Onayında", userId);
                        contract.CurrentStatus = "Karşı Birim Onayında";
                        _context.SaveChanges();

                        var notificationToCounterManager = new Notification
                        {
                            ReceiverId = counterUnitManager.Id,
                            SenderId = userId,
                            ContractId = contract.Id,
                            ActionType = "Sözleşme İnceleme",
                            Message = $"Karşı birimden '{contract.Title}' (ID: {contract.Id}) adlı sözleşme inceleme için bekliyor.",
                            CreatedDate = DateTime.Now
                        };
                        _notificationService.AddNotification(notificationToCounterManager);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Karşı birim (ID: {contract.CounterUnitId}) için yönetici bulunamadı.");
                        TempData["WarningMessage"] = "Karşı birimin yöneticisi bulunamadı, bildirim gönderilemedi.";
                    }

                    TempData["SuccessMessage"] = "Sözleşme başarıyla onaylandı ve karşı birime gönderildi.";
                    return RedirectToAction("OnaylanacakSozlesmeler", "Sozlesme");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Yetki hatası: Kullanıcı {userId}, ManagerId {contract.ManagerId} değil veya durum '{contract.CurrentStatus}' uygun değil.");
                    TempData["ErrorMessage"] = "Bu sözleşmeyi onaylama yetkiniz yok.";
                    return Unauthorized("Bu sözleşmeyi onaylama yetkiniz yok.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Onaylama hatası: {ex.Message}");
                TempData["ErrorMessage"] = $"Onaylama sırasında hata oluştu: {ex.Message}";
                return RedirectToAction("IncelemeBekleyenler");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reddet(int contractId, string rejectionReason)
        {
            var contract = _context.Contracts
                .Include(c => c.ContractSigners)
                .FirstOrDefault(c => c.Id == contractId);

            if (contract == null)
            {
                return NotFound();
            }

            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Kullanıcı kimliği bulunamadı.");
            }

            //if (contract.ManagerId != userId || contract.CurrentStatus != "grup yoneticisi İnceliyor")
            //{
            //    return Unauthorized("Bu sözleşmeyi reddetme yetkiniz yok.");
            //}

            try
            {
                // Red durumunu ve gerekçeyi kaydet
                _contractStatusService.AddStatus(contractId, "Grup Yöneticisi Reddetti", userId, rejectionReason);

                // Sözleşme sahibine red bildirimi gönder
                var notificationToOwner = new Notification
                {
                    ReceiverId = contract.UserId,
                    SenderId = userId,
                    ContractId = contract.Id,
                    ActionType = "Sözleşme Reddedildi",
                    Message = $"'{contract.Title}' adlı sözleşme reddedildi. Gerekçe: {rejectionReason}",
                    CreatedDate = DateTime.Now
                };
                _notificationService.AddNotification(notificationToOwner);

                return RedirectToAction("OnaylanacakSozlesmeler", "Sozlesme");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Reddetme sırasında hata oluştu: {ex.Message}");
                return RedirectToAction("OnaylanacakSozlesmeler", "Sozlesme");
            }
        }




        //[HttpGet]
        //public IActionResult KarsiReddet()
        //{
        //    var contracts = _context.Contracts
        //        .Where(c => c.CurrentStatus == "Karşı Grup Yöneticisi Reddetti")
        //        .ToList();
        //    return View(contracts);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult KarsiReddet(int contractId, string rejectionReason)
        //{
        //    var contract = _context.Contracts
        //        .Include(c => c.ContractSigners)
        //        .FirstOrDefault(c => c.Id == contractId);

        //    if (contract == null)
        //    {
        //        return NotFound();
        //    }

        //    var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
        //    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        //    {
        //        return Unauthorized("Kullanıcı kimliği bulunamadı.");
        //    }

        //    try
        //    {
        //        _contractStatusService.AddStatus(contractId, "Karşı Grup Yöneticisi Reddetti", userId, rejectionReason);

        //        var notificationToOwner = new Notification
        //        {
        //            ReceiverId = contract.UserId,
        //            SenderId = userId,
        //            ContractId = contract.Id,
        //            ActionType = "Sözleşme Reddedildi",
        //            Message = $"'{contract.Title}' adlı sözleşme reddedildi. Gerekçe: {rejectionReason}",
        //            CreatedDate = DateTime.Now
        //        };
        //        _notificationService.AddNotification(notificationToOwner);

        //        return RedirectToAction("KarsiReddet");
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", $"Reddetme sırasında hata oluştu: {ex.Message}");
        //        return RedirectToAction("KarsiReddet");
        //    }
        //}


        [HttpGet]
        public IActionResult KarsiReddet()
        {
            // Mevcut kullanıcının ID'sini al
            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized("Kullanıcı kimliği bulunamadı.");
            }

            // Mevcut kullanıcının yönetici olup olmadığını kontrol et
            var currentUser = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Unit)
                .FirstOrDefault(u => u.Id == currentUserId);

            if (currentUser == null || !currentUser.UserRoles.Any(ur => ur.Role.Name == "Yönetici"))
            {
                return Unauthorized("Bu sayfaya sadece yöneticiler erişebilir.");
            }

            // Bu kullanıcının (yöneticinin) reddettiği sözleşme ID'lerini al
            var rejectedContractIds = _context.ContractStatusHistories
                .Where(cs => cs.Status == "Karsi Birime Geri Gönderildi" && cs.ChangedByUserId == currentUserId)
                .Select(cs => cs.ContractId)
                .ToList();

            // DÜZELTME: Sözleşme akışını doğru takip etmek için hem OurUnit hem CounterUnit kontrolü
            var contracts = _context.Contracts
                .Include(c => c.OurUnit)
                .Include(c => c.CounterUnit)
                .Include(c => c.User)
                .Where(c => c.CurrentStatus == "Karsi Birime Geri Gönderildi" || c.CurrentStatus == "Karsi Grup Yöneticisi Reddetti")
                .Where(c => !rejectedContractIds.Contains(c.Id))
                .Where(c =>
                    // Eğer status "Karsi Birime Geri Gönderildi" ise, bu sözleşme OurUnit'e geri gönderilmiş demektir
                    // Dolayısıyla mevcut kullanıcının birimi OurUnit olmalı
                    (c.CurrentStatus == "Karsi Birime Geri Gönderildi" && c.OurUnitId == currentUser.UnitId) ||
                    // Eğer status "Karsi Grup Yöneticisi Reddetti" ise, sözleşme hala CounterUnit'te
                    (c.CurrentStatus == "Karsi Grup Yöneticisi Reddetti" && c.CounterUnitId == currentUser.UnitId)
                )
                .ToList();

            return View(contracts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult KarsiReddet(int contractId, string rejectionReason)
        {
            var contract = _context.Contracts
                .Include(c => c.ContractSigners)
                .Include(c => c.OurUnit)
                .Include(c => c.CounterUnit)
                .FirstOrDefault(c => c.Id == contractId);

            if (contract == null)
            {
                return NotFound();
            }

            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Kullanıcı kimliği bulunamadı.");
            }

            // Mevcut kullanıcının yönetici olup olmadığını kontrol et
            var currentUser = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Unit)
                .FirstOrDefault(u => u.Id == userId);

            if (currentUser == null || !currentUser.UserRoles.Any(ur => ur.Role.Name == "Yönetici"))
            {
                return Unauthorized("Bu işlemi sadece yöneticiler yapabilir.");
            }

            try
            {
                // Karşı birim yöneticisi mi kontrol et
                var isCounterUnitManager = _context.UserHierarchies
                    .Any(h => h.ManagerId == userId && h.Subordinate.UnitId == contract.CounterUnitId);

                // Mevcut birim yöneticisi mi kontrol et  
                var isCurrentUnitManager = contract.ManagerId == userId;

                if (isCounterUnitManager && (contract.CurrentStatus == "Karşı Birim Onayında" || contract.CurrentStatus == "Karsi Grup Yöneticisi Reddetti"))
                {
                    // Karşı birim yöneticisi reddediyor - sözleşme asıl birime geri gönderilir
                    _contractStatusService.AddStatus(contractId, "Karsi Birime Geri Gönderildi", userId, rejectionReason);
                    contract.CurrentStatus = "grup yoneticisi İnceliyor";

                    // Sözleşmenin asıl sahibine geri ver
                    var originalManager = _context.UserHierarchies
                        .Where(h => h.Subordinate.UnitId == contract.OurUnitId)
                        .Select(h => h.Manager)
                        .FirstOrDefault();

                    if (originalManager != null)
                    {
                        contract.ManagerId = originalManager.Id;
                        _context.SaveChanges();

                        var notificationToOriginalManager = new Notification
                        {
                            ReceiverId = originalManager.Id,
                            SenderId = userId,
                            ContractId = contract.Id,
                            ActionType = "Sözleşme Reddedildi",
                            Message = $"'{contract.Title}' adlı sözleşme karşı birim tarafından reddedildi ve size geri gönderildi. Gerekçe: {rejectionReason}",
                            CreatedDate = DateTime.Now
                        };
                        _notificationService.AddNotification(notificationToOriginalManager);
                    }

                    TempData["SuccessMessage"] = "Sözleşme karşı birim tarafından reddedildi ve asıl birime geri gönderildi.";
                }
                else if (isCurrentUnitManager && (contract.CurrentStatus == "grup yoneticisi İnceliyor" || contract.CurrentStatus == "Karsi Birime Geri Gönderildi"))
                {
                    // Mevcut birim yöneticisi reddediyor - sözleşme karşı birime geri gönderilir
                    _contractStatusService.AddStatus(contractId, "Karsi Grup Yöneticisi Reddetti", userId, rejectionReason);
                    contract.CurrentStatus = "Karşı Birim Onayında";

                    // Karşı birimin yöneticisini bul
                    var counterUnitManager = _context.UserHierarchies
                        .Where(h => h.Subordinate.UnitId == contract.CounterUnitId)
                        .Select(h => h.Manager)
                        .FirstOrDefault();

                    if (counterUnitManager != null)
                    {
                        contract.ManagerId = counterUnitManager.Id;
                        _context.SaveChanges();

                        var notificationToCounterManager = new Notification
                        {
                            ReceiverId = counterUnitManager.Id,
                            SenderId = userId,
                            ContractId = contract.Id,
                            ActionType = "Sözleşme Reddedildi",
                            Message = $"'{contract.Title}' adlı sözleşme {currentUser.Unit?.Name} birimi tarafından reddedildi ve size geri gönderildi. Gerekçe: {rejectionReason}",
                            CreatedDate = DateTime.Now
                        };
                        _notificationService.AddNotification(notificationToCounterManager);
                    }

                    TempData["SuccessMessage"] = "Sözleşme başarıyla reddedildi ve karşı birime geri gönderildi.";
                }
                else
                {
                    // Varsayılan davranış - eski kodunuzdaki gibi
                    _contractStatusService.AddStatus(contractId, "Karsi Birime Geri Gönderildi", userId, rejectionReason);
                    contract.CurrentStatus = "Karsi Birime Geri Gönderildi";
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Sözleşme başarıyla reddedildi.";
                }

                TempData["SuccessMessage"] = "Sözleşme başarıyla reddedildi ve ilgili birim yöneticisine bildirildi.";
                return RedirectToAction("KarsiReddet");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Reddetme sırasında hata oluştu: {ex.Message}");
                TempData["ErrorMessage"] = $"Reddetme sırasında hata oluştu: {ex.Message}";
                return RedirectToAction("KarsiReddet");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Gonder(int contractId, int subordinateId)
        {
            var contract = _context.Contracts
                .Include(c => c.CounterUnit)
                .FirstOrDefault(c => c.Id == contractId);

            if (contract == null)
            {
                TempData["ErrorMessage"] = "Sözleşme bulunamadı.";
                return RedirectToAction("IncelemeBekleyenler");
            }

            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı kimliği bulunamadı.";
                return Unauthorized("Kullanıcı kimliği bulunamadı.");
            }

            if (contract.ManagerId != userId || !string.Equals(contract.CurrentStatus, "Karşı Birim Onayında", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Bu sözleşmeyi gönderme yetkiniz yok.";
                return Unauthorized("Bu sözleşmeyi gönderme yetkiniz yok.");
            }

            var isValidSubordinate = _context.UserHierarchies
                .Any(h => h.ManagerId == userId && h.SubordinateId == subordinateId && h.Subordinate.UnitId == contract.CounterUnitId);

            if (!isValidSubordinate)
            {
                TempData["ErrorMessage"] = "Seçilen personel, biriminiz altında değil.";
                return RedirectToAction("IncelemeBekleyenler");
            }

            try
            {
                contract.ManagerId = subordinateId;
                _context.SaveChanges();

                var notificationToSubordinate = new Notification
                {
                    ReceiverId = subordinateId,
                    SenderId = userId,
                    ContractId = contract.Id,
                    ActionType = "Sözleşme İnceleme",
                    Message = $"Karşı birimden '{contract.Title}' (ID: {contract.Id}) adlı sözleşme inceleme için size gönderildi.",
                    CreatedDate = DateTime.Now
                };
                _notificationService.AddNotification(notificationToSubordinate);

                TempData["SuccessMessage"] = "Sözleşme başarıyla gönderildi.";
                return RedirectToAction("IncelemeBekleyenler");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gönderme hatası: {ex.Message}");
                TempData["ErrorMessage"] = $"Gönderme sırasında hata oluştu: {ex.Message}";
                return RedirectToAction("IncelemeBekleyenler");
            }
        }

        [HttpGet]
        public IActionResult GetSubordinates(int contractId)
        {
            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false, message = "Kullanıcı kimliği bulunamadı." });
            }

            var contract = _context.Contracts.FirstOrDefault(c => c.Id == contractId);
            if (contract == null)
            {
                return Json(new { success = false, message = "Sözleşme bulunamadı." });
            }

            var subordinates = _context.UserHierarchies
                .Where(h => h.ManagerId == userId && h.Subordinate.UnitId == contract.CounterUnitId)
                .Select(h => new { id = h.SubordinateId, name = h.Subordinate.Name })
                .ToList();

            return Json(subordinates);
        }







        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSignature(int contractId, int userId, string signatureBase64)
        {
            if (string.IsNullOrEmpty(signatureBase64))
            {
                return BadRequest("İmza gerekli.");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue || currentUserId.Value != userId)
            {
                return Unauthorized("Yetkisiz işlem.");
            }

            var contract = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                .FirstOrDefault(c => c.Id == contractId);

            if (contract == null)
            {
                return NotFound();
            }

            // Kullanıcının imzalayanlar arasında olup olmadığını kontrol et
            var isSigner = contract.ContractSigners
                .Any(cs => cs.Role == "Imzalayan" && cs.UserId == userId);
            if (!isSigner)
            {
                return Unauthorized("Bu sözleşmeyi imzalama yetkiniz yok.");
            }

            try
            {
                // İmzayı kaydet
                _sozlesmeService.AddSignature(contractId, userId, signatureBase64);

                // Sözleşme durumunu güncelle
                if (_sozlesmeService.AreAllSignersSigned(contractId))
                {
                    contract.CurrentStatus = "İmzalandı";
                    _contractStatusService.AddStatus(contractId, "İmzalandı", userId);
                }
                else
                {
                    contract.CurrentStatus = "Kısmen İmzalandı";
                    _contractStatusService.AddStatus(contractId, "Kısmen İmzalandı", userId);
                }

                // Bildirimler
                // 1. İmzacıya bildirim
                var notificationToSigner = new Notification
                {
                    SenderId = userId,
                    ReceiverId = userId,
                    ContractId = contractId,
                    Message = $"Sözleşme '{contract.Title}' imzaladınız.",
                    ActionType = "İmzalandı",
                    CreatedDate = DateTime.Now
                };
                _notificationService.AddNotification(notificationToSigner);

                // 2. Sözleşme sahibine bildirim
                var signerName = _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.Username)
                    .FirstOrDefault() ?? "Bir kullanıcı";
                var notificationToOwner = new Notification
                {
                    SenderId = userId,
                    ReceiverId = contract.UserId,
                    ContractId = contractId,
                    Message = $"Sözleşmeniz '{contract.Title}', {signerName} tarafından imzalandı.",
                    ActionType = "İmzalandı",
                    CreatedDate = DateTime.Now
                };
                _notificationService.AddNotification(notificationToOwner);

                // 3. Diğer imzalayanlara bildirim (yeni eklendi)
                var otherSigners = contract.ContractSigners
                    .Where(cs => cs.Role == "Imzalayan" && cs.UserId != userId)
                    .Select(cs => cs.UserId)
                    .ToList();
                foreach (var otherSignerId in otherSigners)
                {
                    if (!_sozlesmeService.HasUserSigned(contractId, otherSignerId))
                    {
                        var notificationToOtherSigner = new Notification
                        {
                            SenderId = userId,
                            ReceiverId = otherSignerId,
                            ContractId = contractId,
                            Message = $"Sözleşme '{contract.Title}' için imzanız bekleniyor.",
                            ActionType = "İmza Bekleniyor",
                            CreatedDate = DateTime.Now
                        };
                        _notificationService.AddNotification(notificationToOtherSigner);
                    }
                }

                _context.SaveChanges();

                return RedirectToAction("Imzalananlar");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"İmza kaydedilirken hata oluştu: {ex.Message}");
                return RedirectToAction("Imzala", new { contractId });
            }
        }


        [HttpGet]
        public IActionResult KarsidanGelenler()
        {
            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            System.Diagnostics.Debug.WriteLine($"KarsidanGelenler - UserIdClaim: {userIdClaim}");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["ErrorMessage"] = "Kullanıcı kimliği bulunamadı.";
                return RedirectToAction("Index", "Login");
            }

            var contracts = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .Where(c => (c.CurrentStatus == "Karşı Birim İnceleme Bekliyor" || c.CurrentStatus == "Karşı Birim Personel İnceliyor") && c.ManagerId == userId)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"KarsidanGelenler - Contracts Count: {contracts.Count}");
            foreach (var contract in contracts)
            {
                System.Diagnostics.Debug.WriteLine($"Contract Id: {contract.Id}, Status: {contract.CurrentStatus}, ManagerId: {contract.ManagerId}");
            }

            if (!contracts.Any())
            {
                ViewBag.Message = "Karşı birimden gelen düzenlenmesi veya incelenmesi gereken sözleşme bulunamadı.";
            }

            return View(contracts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult KarsiBirimGonder(int contractId, int subordinateId)
        {
            var contract = _context.Contracts
                .Include(c => c.CounterUnit)
                .FirstOrDefault(c => c.Id == contractId);

            if (contract == null)
            {
                System.Diagnostics.Debug.WriteLine($"Hata: Sözleşme ID {contractId} bulunamadı.");
                TempData["ErrorMessage"] = "Sözleşme bulunamadı.";
                return RedirectToAction("IncelemeBekleyenler");
            }

            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            System.Diagnostics.Debug.WriteLine($"UserIdClaim: {userIdClaim}");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                System.Diagnostics.Debug.WriteLine("Kullanıcı kimliği bulunamadı.");
                TempData["ErrorMessage"] = "Kullanıcı kimliği bulunamadı.";
                return Unauthorized("Kullanıcı kimliği bulunamadı.");
            }

            System.Diagnostics.Debug.WriteLine($"Contract ManagerId: {contract.ManagerId}, UserId: {userId}, CurrentStatus: {contract.CurrentStatus}");
            if (contract.ManagerId != userId || !string.Equals(contract.CurrentStatus, "Karşı Birim Onayında", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"Yetki hatası: Kullanıcı {userId}, ManagerId {contract.ManagerId} değil veya durum '{contract.CurrentStatus}' uygun değil.");
                TempData["ErrorMessage"] = "Bu sözleşmeyi gönderme yetkiniz yok.";
                return Unauthorized("Bu sözleşmeyi gönderme yetkiniz yok.");
            }

            // Seçilen personelin geçerli olup olmadığını kontrol et
            var isValidSubordinate = _context.UserHierarchies
                .Any(h => h.ManagerId == userId && h.SubordinateId == subordinateId && h.Subordinate.UnitId == contract.CounterUnitId);
            System.Diagnostics.Debug.WriteLine($"IsValidSubordinate: {isValidSubordinate}, SubordinateId: {subordinateId}");

            if (!isValidSubordinate)
            {
                System.Diagnostics.Debug.WriteLine($"Geçersiz personel: SubordinateId {subordinateId}, biriminiz altında değil.");
                TempData["ErrorMessage"] = "Seçilen personel, biriminiz altında değil.";
                return RedirectToAction("IncelemeBekleyenler");
            }

            try
            {
                // Sözleşme durumunu güncelle
                contract.ManagerId = subordinateId;
                contract.CurrentStatus = "Karşı Birim Personel İnceliyor";
                _contractStatusService.AddStatus(contractId, "Karşı Birim Personel İnceliyor", userId); // Durumu düzelttik
                _context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"Sözleşme durumu güncellendi: ContractId: {contractId}, Yeni Durum: {contract.CurrentStatus}, Yeni ManagerId: {contract.ManagerId}");

                // Seçilen personele bildirim gönder
                var notificationToSubordinate = new Notification
                {
                    ReceiverId = subordinateId,
                    SenderId = userId,
                    ContractId = contract.Id,
                    ActionType = "Sözleşme İnceleme",
                    Message = $"Karşı birimden '{contract.Title}' (ID: {contract.Id}) adlı sözleşme inceleme için size gönderildi.",
                    CreatedDate = DateTime.Now
                };
                _notificationService.AddNotification(notificationToSubordinate);
                System.Diagnostics.Debug.WriteLine($"Bildirim gönderildi: ReceiverId: {subordinateId}, ContractId: {contract.Id}");

                TempData["SuccessMessage"] = "Sözleşme personele başarıyla gönderildi.";
                return RedirectToAction("IncelemeBekleyenler");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gönderme hatası: {ex.Message}, StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Gönderme sırasında hata oluştu: {ex.Message}";
                return RedirectToAction("IncelemeBekleyenler");
            }
        }
        [HttpGet]
        public IActionResult Details(int id)
        {
            var contract = _context.Contracts
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .Include(c => c.User)
                .Include(c => c.ContractSignatures)
                .Include(c => c.ContractParafs)
                    .ThenInclude(p => p.ParaflayanUser)
                .FirstOrDefault(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            ViewBag.Signers = contract.ContractSigners
                .Where(cs => cs.Role == "Imzalayan")
                .Select(cs => new
                {
                    cs.UserId,
                    cs.User.Name,
                    Signature = contract.ContractSignatures
                        .FirstOrDefault(sig => sig.UserId == cs.UserId)?
                        .SignatureBase64
                })
                .ToList();

            ViewBag.Parafs = contract.ContractParafs
           .Where(p => p.ParaflayanUser != null) // Null ilişkileri filtrele
           .Select(p => new
           {
               p.ParaflayanUserId,
               p.ParaflayanUser.Name,
               p.Sira,
               p.ParafTarihi,
               p.Not,
               p.IsParaflandi
           })
           .ToList();

            // Hata ayıklama için log ekleyelim
            System.Diagnostics.Debug.WriteLine($"Contract ID: {id}, Parafs Count: {ViewBag.Parafs.Count}");
            foreach (var paraf in ViewBag.Parafs)
            {
                System.Diagnostics.Debug.WriteLine($"Paraf - UserId: {paraf.ParaflayanUserId}, Name: {paraf.Name}, IsParaflandi: {paraf.IsParaflandi}");
            }

            return View(contract);
        }
        [HttpGet]
        public IActionResult Parafla(int contractId)
        {
            var contract = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .Include(c => c.ContractParafs)
                    .ThenInclude(p => p.ParaflayanUser)
                .FirstOrDefault(c => c.Id == contractId);

            if (contract == null)
            {
                return NotFound();
            }

            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            // Kullanıcının paraf yetkisi var mı kontrol et
            var isParaflayan = contract.ContractSigners
                .Any(cs => cs.Role == "Paraflayan" && cs.UserId == currentUserId.Value);
            if (!isParaflayan)
            {
                TempData["ErrorMessage"] = "Bu sözleşmeyi paraflama yetkiniz bulunmamaktadır.";
                return RedirectToAction("ParafBekleyenler");
            }

            // Paraf kaydını al
            var parafKaydi = _context.ContractParafs
                .FirstOrDefault(p => p.ContractId == contractId && p.ParaflayanUserId == currentUserId.Value);

            if (parafKaydi == null)
            {
                TempData["ErrorMessage"] = "Paraflama kaydınız bulunmamaktadır.";
                return RedirectToAction("ParafBekleyenler");
            }

            // Sıradaki paraflayanın mevcut kullanıcı olup olmadığını kontrol et
            var parafRecords = _context.ContractParafs
                .Where(p => p.ContractId == contractId)
                .OrderBy(p => p.Sira)
                .ToList();

            bool isUserTurn = false;
            var nextParaf = parafRecords.FirstOrDefault(p => !p.IsParaflandi);
            if (nextParaf != null && nextParaf.ParaflayanUserId == currentUserId.Value)
            {
                isUserTurn = true;
            }

            if (!isUserTurn)
            {
                TempData["ErrorMessage"] = "Sıra sizde değil, başka bir paraflayanın işlemi tamamlaması gerekiyor.";
                return RedirectToAction("ParafBekleyenler");
            }

            // Zaten paraflanmış mı kontrol et
            if (parafKaydi.IsParaflandi)
            {
                TempData["ErrorMessage"] = "Bu sözleşme zaten paraflanmıştır.";
                return RedirectToAction("ParafBekleyenler");
            }

            ViewBag.ContractId = contractId;
            ViewBag.ContractTitle = contract.Title;
            ViewBag.Not = parafKaydi.Not;
            ViewBag.Paraflayanlar = contract.ContractSigners
                .Where(cs => cs.Role == "Paraflayan")
                .Select(cs => cs.User.Name)
                .ToList();

            return View(contract);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveParaf(int contractId, string not)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Kullanıcı oturumu geçersiz.";
                return RedirectToAction("ParafBekleyenler");
            }

            var contract = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .Include(c => c.ContractParafs)
                    .ThenInclude(p => p.ParaflayanUser)
                .FirstOrDefault(c => c.Id == contractId);

            if (contract == null)
            {
                TempData["ErrorMessage"] = "Sözleşme bulunamadı.";
                return RedirectToAction("ParafBekleyenler");
            }

            var parafKaydi = _context.ContractParafs
                .FirstOrDefault(p => p.ContractId == contractId && p.ParaflayanUserId == userId.Value);

            if (parafKaydi == null)
            {
                TempData["ErrorMessage"] = "Paraflama hakkınız bulunmamaktadır.";
                return RedirectToAction("ParafBekleyenler");
            }

            // Sıradaki paraflayanı kontrol et
            var parafRecords = _context.ContractParafs
                .Where(p => p.ContractId == contractId)
                .OrderBy(p => p.Sira)
                .ToList();

            bool isUserTurn = false;
            var nextParaf = parafRecords.FirstOrDefault(p => !p.IsParaflandi);
            if (nextParaf != null && nextParaf.ParaflayanUserId == userId.Value)
            {
                isUserTurn = true;
            }

            if (!isUserTurn)
            {
                TempData["ErrorMessage"] = "Sıra sizde değil, başka bir paraflayanın işlemi tamamlaması gerekiyor.";
                return RedirectToAction("ParafBekleyenler");
            }

            if (parafKaydi.IsParaflandi)
            {
                TempData["ErrorMessage"] = "Bu sözleşme zaten paraflanmıştır.";
                return RedirectToAction("ParafBekleyenler");
            }

            try
            {
                // Paraf bilgilerini güncelle
                parafKaydi.IsParaflandi = true;
                parafKaydi.ParafTarihi = DateTime.Now;
                parafKaydi.Not = not ?? "";
                _context.ContractParafs.Update(parafKaydi);

                // Sözleşme durumunu güncelle
                bool allParaflandi = !parafRecords.Any(p => !p.IsParaflandi);
                contract.CurrentStatus = allParaflandi ? "İmzalanmayı Bekliyor" : "Kısmen Paraflandı";
                _contractStatusService.AddStatus(contractId, contract.CurrentStatus, userId.Value);
                _context.SaveChanges();

                // Bildirimler
                var notificationToParaflayan = new Notification
                {
                    SenderId = userId.Value,
                    ReceiverId = userId.Value,
                    ContractId = contractId,
                    Message = $"Sözleşme '{contract.Title}' parafladınız.",
                    ActionType = "Paraflandı",
                    CreatedDate = DateTime.Now
                };
                _notificationService.AddNotification(notificationToParaflayan);

                var notificationToOwner = new Notification
                {
                    SenderId = userId.Value,
                    ReceiverId = contract.UserId,
                    ContractId = contractId,
                    Message = $"Sözleşmeniz '{contract.Title}', {parafKaydi.ParaflayanUser?.Name} tarafından paraflandı.",
                    ActionType = "Paraflandı",
                    CreatedDate = DateTime.Now
                };
                _notificationService.AddNotification(notificationToOwner);

                if (!allParaflandi)
                {
                    var nextParaflayan = parafRecords.FirstOrDefault(p => !p.IsParaflandi);
                    if (nextParaflayan != null)
                    {
                        var notificationToNextParaflayan = new Notification
                        {
                            SenderId = userId.Value,
                            ReceiverId = nextParaflayan.ParaflayanUserId,
                            ContractId = contractId,
                            Message = $"Sözleşme '{contract.Title}' için sıra sizde, lütfen paraflayın.",
                            ActionType = "Paraf Bekleniyor",
                            CreatedDate = DateTime.Now
                        };
                        _notificationService.AddNotification(notificationToNextParaflayan);
                    }
                }
                else
                {
                    var imzalayanlar = contract.ContractSigners
                        .Where(cs => cs.Role == "Imzalayan")
                        .Select(cs => cs.UserId)
                        .ToList();
                    foreach (var imzalayanId in imzalayanlar)
                    {
                        var notificationToSigner = new Notification
                        {
                            SenderId = userId.Value,
                            ReceiverId = imzalayanId,
                            ContractId = contractId,
                            Message = $"Sözleşme '{contract.Title}' imza için sizi bekliyor.",
                            ActionType = "İmzalanmayı Bekliyor",
                            CreatedDate = DateTime.Now
                        };
                        _notificationService.AddNotification(notificationToSigner);
                    }
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = allParaflandi
                    ? "Sözleşme başarıyla paraflandı ve imzalama aşamasına geçti."
                    : "Sözleşme başarıyla paraflandı, bir sonraki paraflayanın işlemi bekleniyor.";
                return allParaflandi
                    ? RedirectToAction("ImzaBekleyenler")
                    : RedirectToAction("ParafBekleyenler");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Paraflama hatası: {ex.Message}\nStackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Paraflama işlemi sırasında hata oluştu: {ex.Message}";
                return RedirectToAction("ParafBekleyenler");
            }
        }
        [HttpGet]
        public IActionResult ParafBekleyenler()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            // Paraflayan olarak atanan sözleşmeleri getir
            var contracts = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .Include(c => c.ContractParafs)
                    .ThenInclude(p => p.ParaflayanUser)
                .Where(c => (c.CurrentStatus == "İncelendi" || c.CurrentStatus == "Kısmen Paraflandı") &&
                            c.ContractSigners.Any(cs => cs.Role == "Paraflayan" && cs.UserId == userId.Value))
                .ToList();

            var parafStatuses = new Dictionary<int, bool>();
            foreach (var contract in contracts)
            {
                var userParaf = contract.ContractParafs
                    .FirstOrDefault(p => p.ParaflayanUserId == userId && p.IsParaflandi);
                parafStatuses[contract.Id] = userParaf != null; // Kullanıcı parafladıysa true
            }

            ViewBag.ParafStatuses = parafStatuses;
            ViewBag.CurrentUserId = userId.Value;

            if (!contracts.Any())
            {
                ViewBag.Message = "Paraf bekleyen sözleşme bulunmamaktadır.";
            }

            System.Diagnostics.Debug.WriteLine($"ParafBekleyenler - Contracts Count: {contracts.Count}, UserId: {userId.Value}");
            foreach (var contract in contracts)
            {
                System.Diagnostics.Debug.WriteLine($"Contract Id: {contract.Id}, Status: {contract.CurrentStatus}");
            }

            return View(contracts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik için ekleyelim
        public IActionResult Parafla(int contractId, string Not)
        {
            var contract = _context.Contracts
                .Include(c => c.ContractParafs)
                    .ThenInclude(p => p.ParaflayanUser)
                .FirstOrDefault(c => c.Id == contractId);

            if (contract == null)
            {
                TempData["ErrorMessage"] = "Sözleşme bulunamadı.";
                return RedirectToAction("ParafBekleyenler");
            }

            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                TempData["ErrorMessage"] = "Kullanıcı oturumu geçersiz.";
                return RedirectToAction("ParafBekleyenler");
            }

            // Sıradaki paraflayıcıyı bul
            var parafRecords = _context.ContractParafs
                .Where(p => p.ContractId == contractId)
                .OrderBy(p => p.Sira)
                .ToList();

            var currentParaf = parafRecords.FirstOrDefault(p => p.ParaflayanUserId == currentUserId && !p.IsParaflandi);
            if (currentParaf == null)
            {
                TempData["ErrorMessage"] = "Sıra sizde değil veya zaten parafladınız.";
                return RedirectToAction("ParafBekleyenler");
            }

            // Mevcut paraflayıcıyı güncelle
            currentParaf.IsParaflandi = true;
            currentParaf.ParafTarihi = DateTime.Now;
            currentParaf.Not = Not ?? "";
            _context.SaveChanges();

            System.Diagnostics.Debug.WriteLine($"Paraf güncellendi - ContractId: {contractId}, UserId: {currentUserId.Value}, Not: {Not}");

            // Sıradaki paraflayıcıyı kontrol et ve bildirim gönder
            var nextParaf = parafRecords.FirstOrDefault(p => !p.IsParaflandi);
            if (nextParaf != null)
            {
                // Durumu "Kısmen Paraflandı" olarak güncelle
                contract.CurrentStatus = "Kısmen Paraflandı";
                _context.SaveChanges();

                var notification = new Notification
                {
                    ReceiverId = nextParaf.ParaflayanUserId,
                    SenderId = currentUserId.Value,
                    ContractId = contractId,
                    ActionType = "Paraf Bekleniyor",
                    Message = $"Sözleşme '{contract.Title}' için sıra sizde, lütfen paraflayın.",
                    CreatedDate = DateTime.Now
                };
                _notificationService.AddNotification(notification);
                System.Diagnostics.Debug.WriteLine($"Bildirim gönderildi - ReceiverId: {nextParaf.ParaflayanUserId}");
            }
            else
            {
                // Tüm paraflar tamamlandıysa durumu güncelle
                contract.CurrentStatus = "İmzalanmayı Bekliyor";
                _context.SaveChanges();

                // İmzacılara bildirim gönder
                var signers = _context.ContractSigners
                    .Where(cs => cs.ContractId == contractId && cs.Role == "Imzalayan")
                    .Select(cs => cs.UserId)
                    .ToList();
                foreach (var signerId in signers)
                {
                    var notification = new Notification
                    {
                        ReceiverId = signerId,
                        SenderId = currentUserId.Value,
                        ContractId = contractId,
                        ActionType = "İmza Bekleniyor",
                        Message = $"Sözleşme '{contract.Title}' için imza bekleniyor.",
                        CreatedDate = DateTime.Now
                    };
                    _notificationService.AddNotification(notification);
                }
                System.Diagnostics.Debug.WriteLine($"Tüm paraflar tamamlandı - Yeni Durum: {contract.CurrentStatus}");
            }

            TempData["SuccessMessage"] = "Sözleşme başarıyla paraflandı.";
            return RedirectToAction("ParafBekleyenler");
        }
    }
}





    











