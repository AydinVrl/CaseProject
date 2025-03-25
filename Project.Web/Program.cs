using Project.Web.Service;
using System.Net.Http.Headers;
using Project.Service.Interfaces;
using Project.Service.Contracts;
using Project.Core.Interfaces;
using Project.Repository;
using Project.Repository.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// JSON dosyası yüklenirken hata kontrolü
try
{
    Console.WriteLine("Config dosyası yükleniyor: " +
        builder.Configuration.GetDebugView());
}
catch (Exception ex)
{
    Console.WriteLine($"Config yükleme hatası: {ex}");
    throw;
}

// Add services to the container.
builder.Services.AddRazorPages();

// Session desteği ekle
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Database context'i ekle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// HttpClient servis kaydı (daha sağlam versiyon)
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? "https://localhost:5001";
    client.BaseAddress = new Uri(apiUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

    // Timeout ayarı (opsiyonel)
    client.Timeout = TimeSpan.FromSeconds(30);
});

// HttpContext erişimi
builder.Services.AddHttpContextAccessor();

// ApiService kaydı (interface ile)
builder.Services.AddScoped<IApiService, ApiService>();

// UnitOfWork kaydı
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// AuthService kaydı
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware'ini ekle
app.UseSession();

// Authentication ve Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();