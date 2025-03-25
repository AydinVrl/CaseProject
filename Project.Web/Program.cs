
using Project.Web.Service; // Doðru namespace kullanýmý
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// JSON dosyasý yüklenirken hata kontrolü
try
{
    Console.WriteLine("Config dosyasý yükleniyor: " +
        builder.Configuration.GetDebugView());
}
catch (Exception ex)
{
    Console.WriteLine($"Config yükleme hatasý: {ex}");
    throw;
}

// Add services to the container.
builder.Services.AddRazorPages();

// HttpClient servis kaydý (daha saðlam versiyon)
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? "https://localhost:5001";
    client.BaseAddress = new Uri(apiUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

    // Timeout ayarý (opsiyonel)
    client.Timeout = TimeSpan.FromSeconds(30);
});

// HttpContext eriþimi
builder.Services.AddHttpContextAccessor();

// ApiService kaydý (interface ile)
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
app.UseAuthentication(); // Bu satýrý eklemeyi unutmayýn
app.UseAuthorization();

app.MapRazorPages();

app.Run();