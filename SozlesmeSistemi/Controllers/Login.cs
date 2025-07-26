using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SozlesmeSistemi.Data;
using SozlesmeSistemi.Models;
using System.Linq;

//DÖNE EKLEDİ 
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
public class Login : Controller
{
    private readonly SozlesmeSistemiDbContext _context;

    public Login(SozlesmeSistemiDbContext context)
    {
        _context = context;
    }

    // Giriş Sayfası
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

   

    [HttpPost]
    public async Task<IActionResult> Index(string username, string password)
    {
        // Kullanıcıyı veritabanından al
        var user = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Username == username && u.PasswordHash == password && u.IsActive);

        if (user != null)
        {
            // Cookie Authentication için kimlik bilgileri
            var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

            // Rol bilgilerini claim'lere ekle
            var userRoleId = user.UserRoles.FirstOrDefault()?.RoleId;
            if (userRoleId.HasValue)
            {
                claims.Add(new Claim("UserRole", userRoleId.Value.ToString()));
            }

            var isAdmin = user.UserRoles.Any(ur => ur.RoleId == 4); // Teknik ekip rolü
            claims.Add(new Claim("IsAdmin", isAdmin.ToString()));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Kullanıcıyı cookie ile oturum açtır
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Session verilerini ayarla (isteğe bağlı, cookie'de saklanıyorsa gereksiz olabilir)
            HttpContext.Session.SetInt32("UserId", user.Id);
            if (userRoleId.HasValue)
            {
                HttpContext.Session.SetInt32("UserRole", userRoleId.Value);
            }
            HttpContext.Session.SetString("IsAdmin", isAdmin.ToString());

            // Yönlendirme
            return RedirectToAction("GirisEkran", "Sozlesme");
        }

        // Hata durumunda
        ViewBag.Error = "Geçersiz kullanıcı adı veya şifre.";
        return View();
    }



    // Profil Sayfası
    public IActionResult Profil()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Index");
        }

        var user = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Id == userId);

        return View(user);
    }


    // Çıkış
    public IActionResult Logout()
    {

        //DONE BAŞLANGIÇ
        // Cookie ve session temizle
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //DONE BİTİŞ
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Login");

    }
}