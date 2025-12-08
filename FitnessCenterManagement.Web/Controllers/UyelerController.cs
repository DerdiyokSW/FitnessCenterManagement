using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Web.Data;
using FitnessCenterManagement.Web.Models;

namespace FitnessCenterManagement.Web.Controllers
{
    /// <summary>
    /// UyelerController - Üye profili yönetimi
    /// Üyeler kendi profillerini oluşturabilir ve güncelleyebilir
    /// Admin'ler tüm üyeleri yönetebilir
    /// </summary>
    [Authorize] // Sadece giriş yapmış kullanıcılar
    public class UyelerController : Controller
    {
        private readonly FitnessCenterDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<UyelerController> _logger;

        /// <summary>
        /// Constructor - Bağımlılıkları alır
        /// </summary>
        public UyelerController(
            FitnessCenterDbContext dbContext,
            UserManager<IdentityUser> userManager,
            ILogger<UyelerController> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = logger;
        }

        // =============================================
        // GET: /Uyeler/Index
        // Tüm üyeleri listele (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Tüm üyeleri liste halinde gösterir
        /// Sadece Admin'ler görebilir
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var uyeler = await _dbContext.Uyeler
                    .OrderBy(u => u.Ad)
                    .ThenBy(u => u.Soyad)
                    .ToListAsync();

                return View(uyeler);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Üyeler indeksinde hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Uyeler/OlusturProfilim
        // Giriş yapmış kullanıcı için üye profili oluştur
        // =============================================
        /// <summary>
        /// Giriş yapmış kullanıcı için üye profili oluşturma sayfasını gösterir
        /// </summary>
        [Authorize(Roles = "Uye")]
        public async Task<IActionResult> OlusturProfilim()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                // Üyenin daha önce profili oluşturduysa müdüğünü kontrol et
                var mevcutUye = await _dbContext.Uyeler
                    .FirstOrDefaultAsync(u => u.KullaniciId == currentUser.Id);

                if (mevcutUye != null)
                {
                    // Profil zaten var, düzenleme sayfasına yönlendir
                    return RedirectToAction(nameof(DuzenleProfilim));
                }

                // Yeni profil oluştur
                var model = new Uye
                {
                    Ad = currentUser.UserName?.Split('@')[0] ?? "", // Email'den adı çıkar
                    KullaniciId = currentUser.Id
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Profil oluşturma sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /Uyeler/OlusturProfilim
        // Profili veritabanına kaydet
        // =============================================
        /// <summary>
        /// Yeni üye profil kaydını oluşturur
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Uye")]
        public async Task<IActionResult> OlusturProfilim([Bind("Ad,Soyad,DogumTarihi,BouSantimetre,AgirlikKilogram,Cinsiyet,FitnessHedefi")] Uye uye)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                // Üyenin kimliğini otomatik ekle
                uye.KullaniciId = currentUser.Id;

                // Model doğrulama
                if (!ModelState.IsValid)
                {
                    return View(uye);
                }

                // Veritabanına ekle
                _dbContext.Add(uye);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Yeni üye profili oluşturuldu: {uye.Ad} {uye.Soyad} (ID: {uye.Id})");

                TempData["BasariliMesaj"] = "Profiliniz başarıyla oluşturuldu!";
                return RedirectToAction(nameof(DuzenleProfilim));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Profil oluştururken veritabanı hatası: {ex.Message}");
                ModelState.AddModelError("", "Profil oluşturulurken bir hata oluştu.");
                return View(uye);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Profil oluştururken hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Uyeler/DuzenleProfilim
        // Giriş yapmış kullanıcı için profil düzenle
        // =============================================
        /// <summary>
        /// Giriş yapmış kullanıcının profilini düzenleme sayfasını gösterir
        /// </summary>
        [Authorize(Roles = "Uye")]
        public async Task<IActionResult> DuzenleProfilim()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                var uye = await _dbContext.Uyeler
                    .FirstOrDefaultAsync(u => u.KullaniciId == currentUser.Id);

                if (uye == null)
                {
                    // Profil yoksa oluşturma sayfasına yönlendir
                    return RedirectToAction(nameof(OlusturProfilim));
                }

                return View(uye);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Profil düzenleme sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /Uyeler/DuzenleProfilim
        // Profili güncelle
        // =============================================
        /// <summary>
        /// Üye profilini günceller
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Uye")]
        public async Task<IActionResult> DuzenleProfilim([Bind("Id,Ad,Soyad,DogumTarihi,BouSantimetre,AgirlikKilogram,Cinsiyet,FitnessHedefi")] Uye uye)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                // Güvenlik: Sadece kendi profilini düzenleyebilir
                var mevcutUye = await _dbContext.Uyeler
                    .FirstOrDefaultAsync(u => u.KullaniciId == currentUser.Id);

                if (mevcutUye == null || mevcutUye.Id != uye.Id)
                {
                    return Unauthorized();
                }

                // Model doğrulama
                if (!ModelState.IsValid)
                {
                    return View(uye);
                }

                // Güncelle
                _dbContext.Update(uye);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Üye profili güncellendi: {uye.Ad} {uye.Soyad} (ID: {uye.Id})");

                TempData["BasariliMesaj"] = "Profiliniz başarıyla güncellendi!";
                return RedirectToAction(nameof(DuzenleProfilim));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Profil güncellemede hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Uyeler/Details/5
        // Üye detaylarını göster (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Üyenin ayrıntılarını ve randevularını gösterir
        /// Sadece Admin'ler görebilir
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var uye = await _dbContext.Uyeler
                    .Include(u => u.Randevular)
                    .ThenInclude(r => r.Antrenor)
                    .Include(u => u.Randevular)
                    .ThenInclude(r => r.Hizmet)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (uye == null)
                {
                    return NotFound();
                }

                return View(uye);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Üye detaylarında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Uyeler/Delete/5
        // Üye silme onay sayfası (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Üye silme onay sayfasını gösterir
        /// Sadece Admin'ler silebilir
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var uye = await _dbContext.Uyeler.FirstOrDefaultAsync(u => u.Id == id);

                if (uye == null)
                {
                    return NotFound();
                }

                return View(uye);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Üye silme sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /Uyeler/Delete/5
        // Üye silme işlemini gerçekleştir
        // =============================================
        /// <summary>
        /// Üyeyi silme işlemini gerçekleştirir
        /// Sadece Admin'ler silebilir
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var uye = await _dbContext.Uyeler.FindAsync(id);
                if (uye != null)
                {
                    // Üye silinirse, kaskadat silme işlemi veritabanı tarafından yapılır
                    // (Foreign Key'de OnDelete(DeleteBehavior.Cascade) ayarlanmış)
                    _dbContext.Uyeler.Remove(uye);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Üye silindi: {uye.Ad} {uye.Soyad} (ID: {uye.Id})");

                    TempData["BasariliMesaj"] = $"Üye '{uye.Ad} {uye.Soyad}' başarıyla silindi.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Üye silmede hata: {ex.Message}");
                return View("Error");
            }
        }
    }
}
