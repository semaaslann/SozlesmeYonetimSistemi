using Microsoft.EntityFrameworkCore;
using SozlesmeSistemi.Data;
using SozlesmeSistemi.Services;
using Microsoft.AspNetCore.Authentication.Cookies; //ÖNEMLÝ DÖNE EKLEDÝ üste ekle


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<SozlesmeService>();
builder.Services.AddScoped<ReminderService>();
builder.Services.AddScoped<IContractRequestService, ContractRequestService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IContractStatusService, ContractStatusService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();//edagrafik

// SozlesmeSistemiDbContext'i ekle
builder.Services.AddDbContext<SozlesmeSistemiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Oturum ve HTTP context eriþimi için servisleri ekle
builder.Services.AddSession(); // <--- OTURUM DESTEÐÝ
builder.Services.AddHttpContextAccessor(); // <--- SESSION'A ERÝÞMEK ÝÇÝN GEREKLÝ

//DONE BAÞLANGIÇ
// AUTHENTICATION ve AUTHORIZATION ayarlarý
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/AccessDenied"; // isteðe baðlý
    });

builder.Services.AddAuthorization();
//DONE BÝTÝÞ


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Oturum yönetimini etkinleþtir
app.UseSession(); // <--- BU SATIR ÖNEMLÝ

//DONE BAÞLANGIÇ 
app.UseAuthentication(); // Bu satýr eksikti ÇOK ÖNEMLÝ BU SATIR
//DONE BÝTÝÞ

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
