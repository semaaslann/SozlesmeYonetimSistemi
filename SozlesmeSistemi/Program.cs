using Microsoft.EntityFrameworkCore;
using SozlesmeSistemi.Data;
using SozlesmeSistemi.Services;
using Microsoft.AspNetCore.Authentication.Cookies; //�NEML� D�NE EKLED� �ste ekle


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

// Oturum ve HTTP context eri�imi i�in servisleri ekle
builder.Services.AddSession(); // <--- OTURUM DESTE��
builder.Services.AddHttpContextAccessor(); // <--- SESSION'A ER��MEK ���N GEREKL�

//DONE BA�LANGI�
// AUTHENTICATION ve AUTHORIZATION ayarlar�
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/AccessDenied"; // iste�e ba�l�
    });

builder.Services.AddAuthorization();
//DONE B�T��


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

// Oturum y�netimini etkinle�tir
app.UseSession(); // <--- BU SATIR �NEML�

//DONE BA�LANGI� 
app.UseAuthentication(); // Bu sat�r eksikti �OK �NEML� BU SATIR
//DONE B�T��

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
