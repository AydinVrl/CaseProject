using Project.Web.Service;
using System.Net.Http.Headers;
using Project.Service.Interfaces;
using Project.Service.Contracts;
using Project.Core.Interfaces;
using Project.Repository;
using Project.Repository.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


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


builder.Services.AddRazorPages();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? "https://localhost:5001";
    client.BaseAddress = new Uri(apiUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

    client.Timeout = TimeSpan.FromSeconds(30);
});


builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
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

app.UseSession();


app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();