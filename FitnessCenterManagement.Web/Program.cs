// ================================================
// FITNESs CENTER YÖNETİM SİSTEMİ - BAŞLANGIÇ
// Program.cs - Uygulamanın konfigürasyonu
// ================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Web.Data;
using FitnessCenterManagement.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ================================================
// 1. VERİTABANI BAĞLANTISI (DbContext)
// ================================================
// SQL Server veya başka bir veritabanına bağlanır
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("'DefaultConnection' bağlantı dizesi bulunamadı!");

builder.Services.AddDbContext<FitnessCenterDbContext>(options =>
    options.UseSqlServer(connectionString));

// ================================================
// 2. ASP.NET CORE IDENTITY KURULUMU
// ================================================
// Kullanıcı yönetimi, rol yönetimi, şifre hash'leme vb.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Parola gereksinimleri
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Oturum konfigürasyonu
    options.SignIn.RequireConfirmedEmail = false;
})
    .AddEntityFrameworkStores<FitnessCenterDbContext>()
    .AddDefaultTokenProviders();

// ================================================
// 3. CUSTOM SERVİSLER (Bağımlılık Enjeksiyonu)
// ================================================
// Kendi yazılan servisleri kaydet
builder.Services.AddScoped<IYapayzekaSirvisi, YapayzekaSirvisi>();
builder.Services.AddScoped<IRendevuSirvisi, RendevuSirvisi>();

// ================================================
// ================================================
// 5. VERİTABANI BAŞLATILMASI======================
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ================================================
// 4. VERİTABANI BAŞLATILMASI
// ================================================
// Migration'ları otomatik olarak uygula
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FitnessCenterDbContext>();
    
    // Veritabanını oluştur (yoksa)
    dbContext.Database.EnsureCreated();

    // Varsayılan Admin kullanıcı ve rollerini oluştur
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Roller
    var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
    if (!adminRoleExists)
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    var memberRoleExists = await roleManager.RoleExistsAsync("Uye");
    if (!memberRoleExists)
    {
        await roleManager.CreateAsync(new IdentityRole("Uye"));
    }

    // Admin kullanıcı
    var adminUser = await userManager.FindByEmailAsync("ogrencinumarasi@sakarya.edu.tr");
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = "ogrencinumarasi@sakarya.edu.tr",
            Email = "ogrencinumarasi@sakarya.edu.tr",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "sau");
        
        if (result.Succeeded)
        {
            // Admin rolü ekle
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}


// ================================================
// 6. HTTP PIPELINE KONFIGÜRASYONU
// ================================================
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Hata yönetimi
    app.UseExceptionHandler("/Home/Error");
    // HSTS'yi üretim için etkinleştir
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ================================================
// 7. AUTHENTICATION & AUTHORIZATION
// ================================================
// Kimlik doğrulama ve yetkilendirme middleware'leri
app.UseAuthentication();
app.UseAuthorization();

// ================================================
// 8. ROUTING KURALLARI
// ================================================
// Varsayılan rota: Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Razor Pages desteği (isteğe bağlı)
app.MapRazorPages();

app.Run();
