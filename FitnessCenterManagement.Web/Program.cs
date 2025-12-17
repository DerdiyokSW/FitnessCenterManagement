using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Web.Data;
using FitnessCenterManagement.Web.Services;

/// <summary>
/// ASP.NET Core MVC Fitness Center Management Web Uygulaması
/// 
/// Bu uygulama kullanıcıların spor merkezinde antrenörler, hizmetler ve 
/// randevu rezervasyonları yönetmesini sağlar.
/// 
/// Özellikleri:
/// - Kullanıcı kimlik doğrulaması (Login/Register)
/// - Admin paneli (Randevu yönetimi)
/// - REST API (CORS etkinleştirilmiş)
/// - Yapay zeka önerileri (OpenAI GPT-3.5-turbo)
/// - Veritabanı: SQL Server LocalDB
/// </summary>

var builder = WebApplication.CreateBuilder(args);

try
{
    // Veritabanı bağlantı stringini al
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Bağlantı stringi varsa veritabanını ayarla
    if (!string.IsNullOrEmpty(connectionString))
    {
        // Entity Framework Core ve SQL Server ayarla
        builder.Services.AddDbContext<FitnessCenterDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        // ASP.NET Core Identity ayarla (Kullanıcı ve Rol yönetimi)
        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<FitnessCenterDbContext>();
    }
}
catch (Exception ex)
{
    // Veritabanı bağlantı hatalarını yakalayıp konsola yaz
    Console.WriteLine($"DB Error: {ex.Message}");
}

// Özel servisleri dependency injection ile kaydet
builder.Services.AddScoped<IYapayzekaSirvisi, YapayzekaSirvisi>(); // AI önerileri
builder.Services.AddScoped<IRendevuSirvisi, RendevuSirvisi>(); // Randevu işlemleri
builder.Services.AddHttpClient<OpenAiClient>(); // OpenAI API istemcisi

// ASP.NET Core MVC desteğini ekle
builder.Services.AddControllersWithViews();

// Razor Pages desteğini ekle (Identity sayfaları için gerekli)
builder.Services.AddRazorPages();

var app = builder.Build();

// Üretim ortamında exception işleme
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Statik dosyaları (CSS, JS, resimler) sunma
app.UseStaticFiles();

// Routing ayarla
app.UseRouting();

// Kimlik doğrulama ve yetkilendirmeyi etkinleştir
app.UseAuthentication(); // Kimlik doğrulama (kim olduğu)
app.UseAuthorization();  // Yetkilendirme (ne yapabileceği)

// Varsayılan MVC rotasını tanımla: /Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Razor Pages rotasını tanımla (Identity sayfaları için)
app.MapRazorPages();

try
{
    // Uygulamayı başlat ve dinle (localhost:5000)
    app.Run();
}
catch (Exception ex)
{
    // Çalışma zamanı hatalarını yakalayıp konsola yaz
    Console.WriteLine($"Runtime Error: {ex}");
}

