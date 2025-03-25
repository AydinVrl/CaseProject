
using Project.Web.Service; // Do�ru namespace kullan�m�
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// JSON dosyas� y�klenirken hata kontrol�
try
{
    Console.WriteLine("Config dosyas� y�kleniyor: " +
        builder.Configuration.GetDebugView());
}
catch (Exception ex)
{
    Console.WriteLine($"Config y�kleme hatas�: {ex}");
    throw;
}

// Add services to the container.
builder.Services.AddRazorPages();

// HttpClient servis kayd� (daha sa�lam versiyon)
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? "https://localhost:5001";
    client.BaseAddress = new Uri(apiUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

    // Timeout ayar� (opsiyonel)
    client.Timeout = TimeSpan.FromSeconds(30);
});

// HttpContext eri�imi
builder.Services.AddHttpContextAccessor();

// ApiService kayd� (interface ile)
builder.Services.AddScoped<IApiService, ApiService>();

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

// Authentication ve Authorization
app.UseAuthentication(); // Bu sat�r� eklemeyi unutmay�n
app.UseAuthorization();

app.MapRazorPages();

app.Run();