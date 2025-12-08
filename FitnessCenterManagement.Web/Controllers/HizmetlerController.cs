using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Web.Data;
using FitnessCenterManagement.Web.Models;

namespace FitnessCenterManagement.Web.Controllers
{
    /// <summary>
    /// HizmetlerController - Hizmet yönetimi (Yoga, Fitness, Pilates vb.)
    /// Admin'ler hizmet ekleyebilir, düzenleyebilir, silebilir
    /// Üyeler hizmetleri görüntüleyebilir
    /// </summary>
    [Authorize] // Sadece giriş yapmış kullanıcılar
    public class HizmetlerController : Controller
    {
        private readonly FitnessCenterDbContext _dbContext;
        private readonly ILogger<HizmetlerController> _logger;

        /// <summary>
        /// Constructor - Bağımlılıkları alır
        /// </summary>
        public HizmetlerController(FitnessCenterDbContext dbContext, ILogger<HizmetlerController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // =============================================
        // GET: /Hizmetler/Index
        // Tüm hizmetleri listele
        // =============================================
        /// <summary>
        /// Tüm hizmetleri liste halinde gösterir
        /// Tüm giriş yapmış kullanıcılar görebilir
        /// </summary>
        public async Task<IActionResult> Index(int? sporSalonuId)
        {
            try
            {
                var hizmetleriSorgula = _dbContext.Hizmetler
                    .Include(h => h.SporSalonu)
                    .AsQueryable();

                // Spor salonu filtrelemesi
                if (sporSalonuId.HasValue)
                {
                    hizmetleriSorgula = hizmetleriSorgula
                        .Where(h => h.SporSalonuId == sporSalonuId.Value);
                }

                var hizmetler = await hizmetleriSorgula
                    .OrderBy(h => h.Ad)
                    .ToListAsync();

                // ViewBag'a spor salonlarını ekle (dropdown için)
                ViewBag.SporSalonları = await _dbContext.SporSalonlari
                    .OrderBy(s => s.Ad)
                    .ToListAsync();

                return View(hizmetler);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Hizmetler indeksinde hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Hizmetler/Create
        // Hizmet oluşturma formunu göster (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Yeni hizmet oluşturma sayfasını gösterir
        /// Sadece Admin rolesu yapabilir
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            try
            {
                // ViewBag'a spor salonlarını ekle
                ViewBag.SporSalonları = await _dbContext.SporSalonlari
                    .OrderBy(s => s.Ad)
                    .ToListAsync();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Hizmet oluşturma sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /Hizmetler/Create
        // Yeni hizmet kaydını veritabanına ekle
        // =============================================
        /// <summary>
        /// Yeni hizmet kaydını oluşturur
        /// Sadece Admin rolesu yapabilir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Ad,SureDakika,Ucret,Aciklama,SporSalonuId")] Hizmet hizmet)
        {
            try
            {
                // Model doğrulama
                if (!ModelState.IsValid)
                {
                    ViewBag.SporSalonları = await _dbContext.SporSalonlari
                        .OrderBy(s => s.Ad)
                        .ToListAsync();
                    return View(hizmet);
                }

                // Veritabanına ekle
                _dbContext.Add(hizmet);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Yeni hizmet oluşturuldu: {hizmet.Ad} (ID: {hizmet.Id})");

                TempData["BasariliMesaj"] = $"Hizmet '{hizmet.Ad}' başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Hizmet oluştururken veritabanı hatası: {ex.Message}");
                ModelState.AddModelError("", "Hizmet oluşturulurken bir hata oluştu.");

                ViewBag.SporSalonları = await _dbContext.SporSalonlari
                    .OrderBy(s => s.Ad)
                    .ToListAsync();
                return View(hizmet);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Hizmet oluştururken hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Hizmetler/Edit/5
        // Hizmet düzenleme formunu göster (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Hizmet düzenleme sayfasını gösterir
        /// Sadece Admin rolesu yapabilir
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var hizmet = await _dbContext.Hizmetler.FindAsync(id);
                if (hizmet == null)
                {
                    return NotFound();
                }

                // ViewBag'a spor salonlarını ekle
                ViewBag.SporSalonları = await _dbContext.SporSalonlari
                    .OrderBy(s => s.Ad)
                    .ToListAsync();

                return View(hizmet);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Hizmet düzenlemede hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /Hizmetler/Edit/5
        // Hizmet bilgilerini güncelle
        // =============================================
        /// <summary>
        /// Hizmet bilgilerini günceller
        /// Sadece Admin rolesu yapabilir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,SureDakika,Ucret,Aciklama,SporSalonuId")] Hizmet hizmet)
        {
            try
            {
                if (id != hizmet.Id)
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.SporSalonları = await _dbContext.SporSalonlari
                        .OrderBy(s => s.Ad)
                        .ToListAsync();
                    return View(hizmet);
                }

                _dbContext.Update(hizmet);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Hizmet güncellendi: {hizmet.Ad} (ID: {hizmet.Id})");

                TempData["BasariliMesaj"] = $"Hizmet '{hizmet.Ad}' başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_dbContext.Hizmetler.Any(e => e.Id == hizmet.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Hizmet güncellemede hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Hizmetler/Delete/5
        // Hizmet silme onay sayfası (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Hizmet silme onay sayfasını gösterir
        /// Sadece Admin rolesu yapabilir
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

                var hizmet = await _dbContext.Hizmetler
                    .Include(h => h.SporSalonu)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (hizmet == null)
                {
                    return NotFound();
                }

                return View(hizmet);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Hizmet silme sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /Hizmetler/Delete/5
        // Hizmet silme işlemini gerçekleştir
        // =============================================
        /// <summary>
        /// Hizmet silme işlemini gerçekleştirir
        /// Sadece Admin rolesu yapabilir
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var hizmet = await _dbContext.Hizmetler.FindAsync(id);
                if (hizmet != null)
                {
                    // Hizmetle ilgili randevuları kontrol et
                    var ilgiliRandevular = await _dbContext.Randevular
                        .Where(r => r.HizmetId == id)
                        .CountAsync();

                    if (ilgiliRandevular > 0)
                    {
                        TempData["HataMesaji"] = $"Bu hizmeti silemezsiniz. {ilgiliRandevular} adet randevu ile bağlıdır.";
                        return RedirectToAction(nameof(Index));
                    }

                    _dbContext.Hizmetler.Remove(hizmet);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Hizmet silindi: {hizmet.Ad} (ID: {hizmet.Id})");

                    TempData["BasariliMesaj"] = $"Hizmet '{hizmet.Ad}' başarıyla silindi.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Hizmet silmede hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Hizmetler/Details/5
        // Hizmet detaylarını göster
        // =============================================
        /// <summary>
        /// Hizmetin ayrıntılarını gösterir
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var hizmet = await _dbContext.Hizmetler
                    .Include(h => h.SporSalonu)
                    .Include(h => h.Randevular)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (hizmet == null)
                {
                    return NotFound();
                }

                return View(hizmet);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Hizmet detaylarında hata: {ex.Message}");
                return View("Error");
            }
        }
    }
}
