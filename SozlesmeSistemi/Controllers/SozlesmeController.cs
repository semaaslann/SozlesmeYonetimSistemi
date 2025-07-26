using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SozlesmeSistemi.Models;
using SozlesmeSistemi.Services;
using System.Security.Claims;
using System.Linq;
using SozlesmeSistemi.Models;
using SozlesmeSistemi.Data;
using System;


namespace SozlesmeSistemi.Controllers
{


    public class SozlesmeController : Controller
    {
        private readonly SozlesmeService _sozlesmeService;
        private readonly SozlesmeSistemiDbContext _context; // DbContext alanı
                                                            // using dependency injection ile NotificationService'i ekle
        private readonly NotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor; // login kullanıcı bilgisi içinfjdd
        private readonly IContractStatusService _contractStatusService;


        public SozlesmeController(SozlesmeService sozlesmeService, SozlesmeSistemiDbContext context, NotificationService notificationService, IHttpContextAccessor httpContextAccessor, IContractStatusService contractStatusService)
        {
            _sozlesmeService = sozlesmeService;
            _context = context; // DbContext'i al
            _notificationService = notificationService; // NotificationService'i al
            _httpContextAccessor = httpContextAccessor; // HttpContextAccessor'ı al
            _contractStatusService = contractStatusService;


        }

        public IActionResult GirisEkran(string searchTitle)
        {
            var contractsQuery = _context.Contracts
                .Include(c => c.User)
                .AsQueryable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(searchTitle))
            {
                contractsQuery = contractsQuery.Where(c => c.Title.Contains(searchTitle, StringComparison.OrdinalIgnoreCase));
            }

            var contracts = contractsQuery.ToList();
            ViewBag.SearchTitle = searchTitle;

            return View(contracts);
        }
        [HttpGet]
        public IActionResult EklemeSayfasi(int? contractRequestId)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdClaim);
            var currentUser = _sozlesmeService.GetUserById(userId);

            if (currentUser == null)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View(new Contract());
            }

            var contract = new Contract
            {
                ContractRequestId = contractRequestId,
                UserId = userId,
                OurUnitId = currentUser.UnitId
            };

            // Aynı birimdeki imzalayan (RoleId = 6) ve paraflayan (RoleId = 5) kullanıcıları al
            var usersInUnit = _sozlesmeService.GetUsers()
                .Where(u => u.UnitId == contract.OurUnitId)
                .ToList();

            // İmzalayanlar (RoleId = 6)
            var imzalayanKisiler = usersInUnit
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 6))
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Username
                }).ToList();
            ViewBag.ImzalayanKisiler = imzalayanKisiler; // Doğrudan ata

            // Paraflayanlar (RoleId = 5)
            var paraflayanKisiler = usersInUnit
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 5))
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Username
                }).ToList();
            ViewBag.ParaflayanKisiler = paraflayanKisiler; // Doğrudan ata

            // Karşı taraf birimleri
            var units = _sozlesmeService.GetUnits() ?? new List<Unit>();
            ViewBag.Units = units
                .Where(u => u.Id != contract.OurUnitId)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Name
                }).ToList();

            if (contractRequestId.HasValue)
            {
                var request = _context.ContractRequests.Find(contractRequestId.Value);
                ViewBag.ContractRequestTitle = request?.Title;
            }

            ViewBag.CurrentUserName = currentUser.Username;
            ViewBag.CurrentUnitName = _context.Units.FirstOrDefault(u => u.Id == contract.OurUnitId)?.Name;

            // Yerel değişkenler üzerinden Any() kontrolü
            if (!imzalayanKisiler.Any() || !paraflayanKisiler.Any())
            {
                ViewBag.ErrorMessage = "Bu birimde imzalayan veya paraflayan kişi bulunamadı.";
            }

            return View(contract);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EklemeSayfasi(Contract contract, int[] ImzalayanKisiIds, int[] ParaflayanKisiIds)
        {
            var userId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirst("UserId")?.Value ?? "0");
            var currentUser = _sozlesmeService.GetUserById(userId);

            contract.UserId = userId;
            contract.OurUnitId = currentUser?.UnitId ?? 0;
            contract.CurrentStatus = "grup yoneticisi İnceliyor"; // Yeni durum

            // Tarih doğrulama ve diğer mevcut doğrulamalar...
            if (contract.ImzaOnayTarihi < DateTime.Today)
            {
                ModelState.AddModelError("ImzaOnayTarihi", "İmza/Onay Tarihi geçmiş bir tarih olamaz.");
            }

            if (contract.FinisDate < DateTime.Today)
            {
                ModelState.AddModelError("FinisDate", "Bitiş Tarihi geçmiş bir tarih olamaz.");
            }

            if (contract.FinisDate < contract.ImzaOnayTarihi)
            {
                ModelState.AddModelError("FinisDate", "Bitiş tarihi, imza tarihinden önce olamaz.");
            }

            if (contract.ImzaOnayTarihi.HasValue && contract.FinisDate.HasValue)
            {
                var timeDiff = contract.FinisDate.Value - contract.ImzaOnayTarihi.Value;
                contract.SozlesmeSuresi = $"{(int)timeDiff.TotalDays} gün";
            }
            else
            {
                ModelState.AddModelError("SozlesmeSuresi", "Sözleşme süresi hesaplanamadı. Lütfen tarihleri kontrol edin.");
            }

            if (ImzalayanKisiIds == null || ImzalayanKisiIds.Length == 0)
            {
                ModelState.AddModelError("ImzalayanKisiIds", "En az bir imzalayan kişi seçmelisiniz.");
            }

            if (ParaflayanKisiIds == null || ParaflayanKisiIds.Length == 0)
            {
                ModelState.AddModelError("ParaflayanKisiIds", "En az bir paraflayan kişi seçmelisiniz.");
            }

            // ViewBag verilerini tekrar yükle
            var usersInUnit = _sozlesmeService.GetUsers()
                .Where(u => u.UnitId == contract.OurUnitId)
                .ToList();

            ViewBag.ImzalayanKisiler = usersInUnit
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 6))
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Username
                }).ToList();

            ViewBag.ParaflayanKisiler = usersInUnit
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 5))
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Username
                }).ToList();

            var units = _sozlesmeService.GetUnits() ?? new List<Unit>();
            ViewBag.Units = units
                .Where(u => u.Id != contract.OurUnitId)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Name
                }).ToList();

            if (ModelState.IsValid)
            {
                try
                {
                    // Sözleşmeyi ekle
                    _sozlesmeService.AddContract(contract);

                    // Imzalayanları ContractSigners tablosuna ekle
                    foreach (var id in ImzalayanKisiIds)
                    {
                        var user = _sozlesmeService.GetUserById(id);
                        if (user != null)
                        {
                            _context.ContractSigners.Add(new ContractSigners
                            {
                                ContractId = contract.Id,
                                UserId = id,
                                Role = "Imzalayan",
                                UnitId = user.UnitId
                            });
                        }
                    }

                    // Paraflayanları ContractSigners ve ContractParafs tablosuna ekle
                    foreach (var id in ParaflayanKisiIds)
                    {
                        var user = _sozlesmeService.GetUserById(id);
                        if (user != null)
                        {
                            _context.ContractSigners.Add(new ContractSigners
                            {
                                ContractId = contract.Id,
                                UserId = id,
                                Role = "Paraflayan",
                                UnitId = user.UnitId
                            });
                        }
                    }

                    // Paraflayanları sırayla ContractParafs tablosuna ekle
                    _sozlesmeService.AddParaflayanlar(contract.Id, ParaflayanKisiIds.ToList());

                    _context.SaveChanges();

                    // Yöneticisini bul
                    var manager = _context.UserHierarchies
                        .Where(h => h.SubordinateId == userId)
                        .Select(h => h.Manager)
                        .FirstOrDefault();

                    if (manager != null)
                    {
                        contract.ManagerId = manager.Id;
                        // Yöneticiye bildirim gönder
                        var notificationToManager = new Notification
                        {
                            ReceiverId = manager.Id,
                            SenderId = userId,
                            ContractId = contract.Id,
                            ActionType = "Sözleşme İnceleme",
                            Message = $"İnceleme için '{contract.Title}' adlı sözleşme eklendi.",
                            CreatedDate = DateTime.Now
                        };
                        _notificationService.AddNotification(notificationToManager);
                    }

                    // Bildirimler
                    var notificationToResponsible = new Notification
                    {
                        ReceiverId = contract.UserId,
                        SenderId = userId,
                        ContractId = contract.Id,
                        ActionType = "Sözleşme Ekleme",
                        Message = $"Sorumlu olduğunuz '{contract.Title}' adlı sözleşme eklendi.",
                        CreatedDate = DateTime.Now
                    };
                    _notificationService.AddNotification(notificationToResponsible);

                    foreach (var id in ImzalayanKisiIds)
                    {
                        var notificationToSigner = new Notification
                        {
                            ReceiverId = id,
                            SenderId = userId,
                            ContractId = contract.Id,
                            ActionType = "Sözleşme Ekleme",
                            Message = $"İmzalamanız gereken '{contract.Title}' adlı sözleşme eklendi.",
                            CreatedDate = DateTime.Now
                        };
                        _notificationService.AddNotification(notificationToSigner);
                    }

                    // İlk paraflayana bildirim gönder
                    var firstParaflayan = _context.ContractParafs
                        .Where(p => p.ContractId == contract.Id && p.Sira == 1)
                        .Select(p => p.ParaflayanUserId)
                        .FirstOrDefault();
                    if (firstParaflayan != 0)
                    {
                        var notificationToParapher = new Notification
                        {
                            ReceiverId = firstParaflayan,
                            SenderId = userId,
                            ContractId = contract.Id,
                            ActionType = "Paraf Bekleniyor",
                            Message = $"Sözleşme '{contract.Title}' için sıra sizde, lütfen paraflayın.",
                            CreatedDate = DateTime.Now
                        };
                        _notificationService.AddNotification(notificationToParapher);
                    }

                    if (contract.ContractRequestId.HasValue)
                    {
                        var request = _context.ContractRequests.Find(contract.ContractRequestId.Value);
                        if (request != null)
                        {
                            request.Status = "Tamamlandı";
                            _context.SaveChanges();
                        }
                    }

                    return RedirectToAction("GirisEkran");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Sözleşme eklenirken bir hata oluştu: {ex.Message}");
                }
            }

            if (contract.ContractRequestId.HasValue)
            {
                var request = _context.ContractRequests.Find(contract.ContractRequestId.Value);
                ViewBag.ContractRequestTitle = request?.Title;
            }

            ViewBag.CurrentUserName = currentUser?.Username;
            ViewBag.CurrentUnitName = _context.Units.FirstOrDefault(u => u.Id == contract.OurUnitId)?.Name;

            return View(contract);
        }




        [HttpGet]
        public IActionResult DuzenlemeSayfasi(int id)
        {
            var sozlesme = _sozlesmeService.GetContract(id);
            if (sozlesme == null)
            {
                return NotFound();
            }

            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Kullanıcı kimliği bulunamadı.");
            }

            var currentUser = _sozlesmeService.GetUserById(userId);
            if (currentUser == null)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View(sozlesme);
            }

            // ViewBag verilerini yükle
            var usersInUnit = _sozlesmeService.GetUsers()
                .Where(u => u.UnitId == sozlesme.OurUnitId)
                .ToList();

            ViewBag.ImzalayanKisiler = usersInUnit
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 6))
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Username
                }).ToList();

            ViewBag.ParaflayanKisiler = usersInUnit
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 5))
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Username
                }).ToList();

            ViewBag.SelectedImzalayanKisiler = _context.ContractSigners
                .Where(cs => cs.ContractId == id && cs.Role == "Imzalayan")
                .Select(cs => cs.UserId.ToString())
                .ToList();

            ViewBag.SelectedParaflayanKisiler = _context.ContractSigners
                .Where(cs => cs.ContractId == id && cs.Role == "Paraflayan")
                .Select(cs => cs.UserId.ToString())
                .ToList();

            var units = _sozlesmeService.GetUnits() ?? new List<Unit>();
            ViewBag.Units = units
                .Where(u => u.Id != sozlesme.OurUnitId)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Name
                }).ToList();

            ViewBag.CurrentUserName = currentUser.Username;
            ViewBag.CurrentUnitName = _context.Units.FirstOrDefault(u => u.Id == sozlesme.OurUnitId)?.Name;

            return View(sozlesme);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DuzenlemeSayfasi(Contract contract, int[] ImzalayanKisiIds, int[] ParaflayanKisiIds)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false, message = "Kullanıcı kimliği bulunamadı.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            if (contract.FinisDate < contract.ImzaOnayTarihi)
            {
                ModelState.AddModelError("FinisDate", "Bitiş tarihi, imza tarihinden önce olamaz.");
            }

            if (contract.ImzaOnayTarihi < DateTime.Today)
            {
                ModelState.AddModelError("ImzaOnayTarihi", "İmza/Onay Tarihi geçmiş bir tarih olamaz.");
            }

            if (contract.FinisDate < DateTime.Today)
            {
                ModelState.AddModelError("FinisDate", "Bitiş Tarihi geçmiş bir tarih olamaz.");
            }

            if (ImzalayanKisiIds == null || ImzalayanKisiIds.Length == 0)
            {
                ModelState.AddModelError("ImzalayanKisiIds", "En az bir imzalayan kişi seçmelisiniz.");
            }

            if (ParaflayanKisiIds == null || ParaflayanKisiIds.Length == 0)
            {
                ModelState.AddModelError("ParaflayanKisiIds", "En az bir paraflayan kişi seçmelisiniz.");
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Formda geçersiz veriler var.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            try
            {
                // Durumu güncelle
                contract.CurrentStatus = "personel duzenledi";
                _sozlesmeService.UpdateContract(contract);

                // Mevcut imzalayan ve paraflayanları sil
                var existingSigners = _context.ContractSigners.Where(cs => cs.ContractId == contract.Id);
                _context.ContractSigners.RemoveRange(existingSigners);

                // Yeni imzalayanları ekle
                foreach (var id in ImzalayanKisiIds)
                {
                    var user = _sozlesmeService.GetUserById(id);
                    if (user != null)
                    {
                        _context.ContractSigners.Add(new ContractSigners
                        {
                            ContractId = contract.Id,
                            UserId = id,
                            Role = "Imzalayan",
                            UnitId = user.UnitId
                        });
                    }
                }

                // Yeni paraflayanları ekle
                foreach (var id in ParaflayanKisiIds)
                {
                    var user = _sozlesmeService.GetUserById(id);
                    if (user != null)
                    {
                        _context.ContractSigners.Add(new ContractSigners
                        {
                            ContractId = contract.Id,
                            UserId = id,
                            Role = "Paraflayan",
                            UnitId = user.UnitId
                        });
                    }
                }

                // Paraflayanları sırayla ContractParafs tablosuna ekle
                _sozlesmeService.AddParaflayanlar(contract.Id, ParaflayanKisiIds.ToList());

                // Sözleşme durum geçmişini güncelle
                var statusHistory = new ContractStatusHistory
                {
                    ContractId = contract.Id,
                    Status = "personel duzenledi",
                    ChangedByUserId = userId,
                    ChangeDate = DateTime.Now
                };
                _context.ContractStatusHistories.Add(statusHistory);

                // Yöneticiye bildirim gönder
                var manager = _context.UserHierarchies
                    .Where(h => h.SubordinateId == userId)
                    .Select(h => h.Manager)
                    .FirstOrDefault();
                if (manager != null)
                {
                    var notificationToManager = new Notification
                    {
                        ReceiverId = manager.Id,
                        SenderId = userId,
                        ContractId = contract.Id,
                        ActionType = "Sözleşme İnceleme",
                        Message = $"İnceleme için '{contract.Title}' adlı sözleşme güncellendi.",
                        CreatedDate = DateTime.Now
                    };
                    _notificationService.AddNotification(notificationToManager);
                }

                // İlk paraflayana bildirim gönder
                var firstParaflayan = _context.ContractParafs
                    .Where(p => p.ContractId == contract.Id && p.Sira == 1)
                    .Select(p => p.ParaflayanUserId)
                    .FirstOrDefault();
                if (firstParaflayan != 0)
                {
                    var notificationToParapher = new Notification
                    {
                        ReceiverId = firstParaflayan,
                        SenderId = userId,
                        ContractId = contract.Id,
                        ActionType = "Paraf Bekleniyor",
                        Message = $"Sözleşme '{contract.Title}' için sıra sizde, lütfen paraflayın.",
                        CreatedDate = DateTime.Now
                    };
                    _notificationService.AddNotification(notificationToParapher);
                }

                _context.SaveChanges();

                return RedirectToAction("DuzenlememGerekenSozlesmeler");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Güncelleme sırasında hata oluştu: {ex.Message}");
                // ViewBag verilerini yeniden yükle
                var usersInUnit = _sozlesmeService.GetUsers()
                    .Where(u => u.UnitId == contract.OurUnitId)
                    .ToList();
                ViewBag.ImzalayanKisiler = usersInUnit
                    .Where(u => u.UserRoles.Any(ur => ur.RoleId == 6))
                    .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Username })
                    .ToList();
                ViewBag.ParaflayanKisiler = usersInUnit
                    .Where(u => u.UserRoles.Any(ur => ur.RoleId == 5))
                    .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Username })
                    .ToList();
                ViewBag.SelectedImzalayanKisiler = ImzalayanKisiIds?.Select(id => id.ToString()).ToList() ?? new List<string>();
                ViewBag.SelectedParaflayanKisiler = ParaflayanKisiIds?.Select(id => id.ToString()).ToList() ?? new List<string>();
                var units = _sozlesmeService.GetUnits() ?? new List<Unit>();
                ViewBag.Units = units
                    .Where(u => u.Id != contract.OurUnitId)
                    .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Name })
                    .ToList();
                ViewBag.CurrentUserName = _sozlesmeService.GetUserById(userId)?.Username;
                ViewBag.CurrentUnitName = _context.Units.FirstOrDefault(u => u.Id == contract.OurUnitId)?.Name;

                return View(contract);
            }
        }


        // Silme
        public IActionResult SilmeSayfası(int id)
        {
            //var sozlesme = _sozlesmeService.GetContract(id);
            //if (sozlesme == null) return NotFound();
            //return View(sozlesme);

            try
            {
                var sozlesme = _sozlesmeService.GetContract(id);
                if (sozlesme == null)
                {
                    return Json(new { success = false, message = "Sözleşme bulunamadı." });
                }

                _sozlesmeService.DeleteContract(id);
                return Json(new { success = true, message = "Sözleşme başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Silme sırasında bir hata oluştu: " + ex.Message });
            }
        }

        public IActionResult DetaySayfasi(int id)
        {
            var sozlesme = _context.Contracts.Include(c => c.User).FirstOrDefault(c => c.Id == id);
            if (sozlesme == null)
            {
                return NotFound();
            }
            return View(sozlesme);
        }

        //[HttpPost, ActionName("Sil")]
        [HttpPost("SilOnay")]
        [ValidateAntiForgeryToken]
        public IActionResult SilOnay(int id)
        {
            //_sozlesmeService.DeleteContract(id);
            //return RedirectToAction("GirisEkran");

            try
            {
                var contract = _context.Contracts.Find(id);
                if (contract == null)
                {
                    return Json(new { success = false, message = "Sözleşme bulunamadı!" });
                }

                _context.Contracts.Remove(contract);
                _context.SaveChanges();

                return Json(new { success = true, message = "Sözleşme başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        public IActionResult Taslaklar()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var taslaklar = _sozlesmeService.GetDraftContracts(userId);
            return View(taslaklar);
        }





        public IActionResult OnaylanacakSozlesmeler()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Kullanıcı kimliği bulunamadı veya geçersiz.");
            }

            var contracts = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .Where(c => (c.CurrentStatus == "grup yoneticisi İnceliyor" || c.CurrentStatus == "personel duzenledi") && c.ManagerId == userId)
                .ToList();

            if (!contracts.Any())
            {
                ViewBag.Message = "Onaylanacak sözleşme bulunamadı. Contract tablosunda ManagerId kontrol edin.";
            }

            return View(contracts);
        }

        [HttpGet]
        public IActionResult KarsiSozlesmeIncelemeleri(int id)
        {
            var sozlesme = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.OurUnit)
                .Include(c => c.CounterUnit)
                .Include(c => c.ContractSigners)
                 .ThenInclude(cs => cs.User)
                .FirstOrDefault(c => c.Id == id);

            if (sozlesme == null)
            {
                return NotFound();
            }

            var userId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirst("UserId")?.Value ?? "0");
            //if (sozlesme.ManagerId != userId || sozlesme.CurrentStatus != "grup yoneticisi İnceliyor")
            //{
            //    return Unauthorized("Bu sözleşmeyi inceleme yetkiniz yok.");
            //}

            ViewBag.ImzalayanKisiler = _context.ContractSigners
                .Where(cs => cs.ContractId == id && cs.Role == "Imzalayan")
                .Select(cs => cs.User.Username)
                .ToList();

            ViewBag.ParaflayanKisiler = _context.ContractSigners
                .Where(cs => cs.ContractId == id && cs.Role == "Paraflayan")
                .Select(cs => cs.User.Username)
                .ToList();

            return View(sozlesme);
        }



        [HttpGet]
        public IActionResult KarsiRedIncelemeSayfasi(int id)
        {
            var sozlesme = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.OurUnit)
                .Include(c => c.CounterUnit)
                .Include(c => c.ContractSigners)
                 .ThenInclude(cs => cs.User)
                .FirstOrDefault(c => c.Id == id);

            if (sozlesme == null)
            {
                return NotFound();
            }

            var userId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirst("UserId")?.Value ?? "0");
            //if (sozlesme.ManagerId != userId || sozlesme.CurrentStatus != "grup yoneticisi İnceliyor")
            //{
            //    return Unauthorized("Bu sözleşmeyi inceleme yetkiniz yok.");
            //}

            ViewBag.ImzalayanKisiler = _context.ContractSigners
                .Where(cs => cs.ContractId == id && cs.Role == "Imzalayan")
                .Select(cs => cs.User.Username)
                .ToList();

            ViewBag.ParaflayanKisiler = _context.ContractSigners
                .Where(cs => cs.ContractId == id && cs.Role == "Paraflayan")
                .Select(cs => cs.User.Username)
                .ToList();

            return View(sozlesme);
        }
        [HttpGet]
        public IActionResult IncelemeSayfasi(int id)
        {
            var sozlesme = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.OurUnit)
                .Include(c => c.CounterUnit)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .FirstOrDefault(c => c.Id == id);

            if (sozlesme == null)
            {
                return NotFound();
            }

            var userId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirst("UserId")?.Value ?? "0");
            //if (sozlesme.ManagerId != userId || sozlesme.CurrentStatus != "grup yoneticisi İnceliyor")
            //{
            //    return Unauthorized("Bu sözleşmeyi inceleme yetkiniz yok.");
            //}

            ViewBag.ImzalayanKisiler = _context.ContractSigners
                .Where(cs => cs.ContractId == id && cs.Role == "Imzalayan")
                .Select(cs => cs.User.Username)
                .ToList();

            ViewBag.ParaflayanKisiler = _context.ContractSigners
                .Where(cs => cs.ContractId == id && cs.Role == "Paraflayan")
                .Select(cs => cs.User.Username)
                .ToList();

            return View(sozlesme);
        }


        [HttpGet]
        public IActionResult DuzenlememGerekenSozlesmeler()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Kullanıcı kimliği bulunamadı veya geçersiz.");
            }

            var contracts = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .Where(c => (c.CurrentStatus == "Grup Yöneticisi Reddetti" || c.CurrentStatus == "personel duzenledi") && c.UserId == userId)
                .ToList();

            if (!contracts.Any())
            {
                ViewBag.Message = "Düzenlenmesi gereken sözleşme bulunamadı.";
            }

            return View(contracts);
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendContract(int id)
        {
            try
            {
                var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Json(new { success = false, message = "Kullanıcı kimliği bulunamadı veya geçersiz." });
                }

                var contract = _context.Contracts
                    .Include(c => c.User)
                    .FirstOrDefault(c => c.Id == id);

                if (contract == null)
                {
                    return Json(new { success = false, message = "Sözleşme bulunamadı." });
                }

                // Sadece ilgili kullanıcı düzenleme yapabiliyorsa kontrol et
                if (contract.UserId != userId)
                {
                    return Json(new { success = false, message = "Bu sözleşmeyi gönderme yetkiniz yok." });
                }

                // Sözleşme durumunu güncelle
                contract.CurrentStatus = "grup yoneticisi İnceliyor";
                contract.UpdatedDate = DateTime.Now;

                // Sözleşme durum geçmişini güncelle
                var statusHistory = new ContractStatusHistory
                {
                    ContractId = contract.Id,
                    Status = "grup yoneticisi İnceliyor",
                    ChangedByUserId = userId,
                    ChangeDate = DateTime.Now
                };
                _context.ContractStatusHistories.Add(statusHistory);

                // Yöneticisine bildirim gönder
                var manager = _context.UserHierarchies
                    .Where(h => h.SubordinateId == userId)
                    .Select(h => h.Manager)
                    .FirstOrDefault();

                if (manager != null)
                {
                    var notificationToManager = new Notification
                    {
                        ReceiverId = manager.Id,
                        SenderId = userId,
                        ContractId = contract.Id,
                        ActionType = "Sözleşme İnceleme",
                        Message = $"İnceleme için '{contract.Title}' adlı sözleşme gönderildi.",
                        CreatedDate = DateTime.Now
                    };
                    _notificationService.AddNotification(notificationToManager);
                }

                _context.SaveChanges();

                return Json(new { success = true, message = "Sözleşme başarıyla gönderildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }



    }
}