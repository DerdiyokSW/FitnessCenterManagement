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
    /// AntrenorlerController - Antrenör/Eğitmen yönetimi
    /// Admin'ler antrenörleri yönetebilir
    /// Üyeler antrenörleri görüntüleyebilir
    /// </summary>
    [Authorize] // Sadece giriş yapmış kullanıcılar
    public class AntrenorlerController : Controller
    {
        private readonly FitnessCenterDbContext _dbContext;
        private readonly ILogger<AntrenorlerController> _logger;

        /// <summary>
        /// Constructor - Bağımlılıkları alır
        /// </summary>
        public AntrenorlerController(FitnessCenterDbContext dbContext, ILogger<AntrenorlerController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // =============================================
        // GET: /Antrenorler/Index
        // Tüm antrenörleri listele
        // =============================================
        /// <summary>
        /// Tüm antrenörleri liste halinde gösterir
        /// Tüm giriş yapmış kullanıcılar görebilir
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? sporSalonuId)
        {
            try
            {
                var antrenorlerSorgula = _dbContext.Antrenorler
                    .Include(a => a.SporSalonu)
                    .AsQueryable();

                // Spor salonu filtrelemesi
                if (sporSalonuId.HasValue)
                {
                    antrenorlerSorgula = antrenorlerSorgula
                        .Where(a => a.SporSalonuId == sporSalonuId.Value);
                }

                var antrenorler = await antrenorlerSorgula
                    .OrderBy(a => a.Ad)
                    .ThenBy(a => a.Soyad)
                    .ToListAsync();

                // ViewBag'a spor salonlarını ekle (dropdown için)
                ViewBag.SporSalonları = await _dbContext.SporSalonlari
                    .OrderBy(s => s.Ad)
                    .ToListAsync();

                return View(antrenorler);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Antrenörler indeksinde hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Antrenorler/Create
        // Antrenör oluşturma formunu göster (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Yeni antrenör oluşturma sayfasını gösterir
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
                _logger.LogError($"Antrenör oluşturma sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /Antrenorler/Create
        // Yeni antrenör kaydını veritabanına ekle
        // =============================================
        /// <summary>
        /// Yeni antrenör kaydını oluşturur
        /// Sadece Admin rolesu yapabilir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Ad,Soyad,UzmanlıkAlanlari,Telefon,Email,SporSalonuId,MevcutBaslangicSaati,MevcutBitisSaati")] Antrenor antrenor)
        {
            try
            {
                // Model doğrulama
                if (!ModelState.IsValid)
                {
                    ViewBag.SporSalonları = await _dbContext.SporSalonlari
                        .OrderBy(s => s.Ad)
                        .ToListAsync();
                    return View(antrenor);
                }

                // Veritabanına ekle
                _dbContext.Add(antrenor);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Yeni antrenör oluşturuldu: {antrenor.Ad} {antrenor.Soyad} (ID: {antrenor.Id})");

                TempData["BasariliMesaj"] = $"Antrenör '{antrenor.Ad} {antrenor.Soyad}' başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Antrenör oluştururken veritabanı hatası: {ex.Message}");
                ModelState.AddModelError("", "Antrenör oluşturulurken bir hata oluştu.");

                ViewBag.SporSalonları = await _dbContext.SporSalonlari
                    .OrderBy(s => s.Ad)
                    .ToListAsync();
                return View(antrenor);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Antrenör oluştururken hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Antrenorler/Edit/5
        // Antrenör düzenleme formunu göster (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Antrenör düzenleme sayfasını gösterir
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

                var antrenor = await _dbContext.Antrenorler.FindAsync(id);
                if (antrenor == null)
                {
                    return NotFound();
                }

                // ViewBag'a spor salonlarını ekle
                ViewBag.SporSalonları = await _dbContext.SporSalonlari
                    .OrderBy(s => s.Ad)
                    .ToListAsync();

                return View(antrenor);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Antrenör düzenlemede hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /Antrenorler/Edit/5
        // Antrenör bilgilerini güncelle
        // =============================================
        /// <summary>
        /// Antrenör bilgilerini günceller
        /// Sadece Admin rolesu yapabilir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Soyad,UzmanlıkAlanlari,Telefon,Email,SporSalonuId,MevcutBaslangicSaati,MevcutBitisSaati")] Antrenor antrenor)
        {
            try
            {
                if (id != antrenor.Id)
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.SporSalonları = await _dbContext.SporSalonlari
                        .OrderBy(s => s.Ad)
                        .ToListAsync();
                    return View(antrenor);
                }

                _dbContext.Update(antrenor);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Antrenör güncellendi: {antrenor.Ad} {antrenor.Soyad} (ID: {antrenor.Id})");

                TempData["BasariliMesaj"] = $"Antrenör '{antrenor.Ad} {antrenor.Soyad}' başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_dbContext.Antrenorler.Any(e => e.Id == antrenor.Id))
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
                _logger.LogError($"Antrenör güncellemede hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Antrenorler/Delete/5
        // Antrenör silme onay sayfası (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Antrenör silme onay sayfasını gösterir
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

                var antrenor = await _dbContext.Antrenorler
                    .Include(a => a.SporSalonu)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (antrenor == null)
                {
                    return NotFound();
                }

                return View(antrenor);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Antrenör silme sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /Antrenorler/Delete/5
        // Antrenör silme işlemini gerçekleştir
        // =============================================
        /// <summary>
        /// Antrenör silme işlemini gerçekleştirir
        /// Sadece Admin rolesu yapabilir
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var antrenor = await _dbContext.Antrenorler.FindAsync(id);
                if (antrenor != null)
                {
                    // Antrenörle ilgili randevuları kontrol et
                    var ilgiliRandevular = await _dbContext.Randevular
                        .Where(r => r.AntrenorId == id)
                        .CountAsync();

                    if (ilgiliRandevular > 0)
                    {
                        TempData["HataMesaji"] = $"Bu antrenörü silemezsiniz. {ilgiliRandevular} adet randevu ile bağlıdır.";
                        return RedirectToAction(nameof(Index));
                    }

                    _dbContext.Antrenorler.Remove(antrenor);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Antrenör silindi: {antrenor.Ad} {antrenor.Soyad} (ID: {antrenor.Id})");

                    TempData["BasariliMesaj"] = $"Antrenör '{antrenor.Ad} {antrenor.Soyad}' başarıyla silindi.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Antrenör silmede hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Antrenorler/Details/5
        // Antrenör detaylarını göster
        // =============================================
        /// <summary>
        /// Antrenörün ayrıntılarını ve randevularını gösterir
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var antrenor = await _dbContext.Antrenorler
                    .Include(a => a.SporSalonu)
                    .Include(a => a.Randevular)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (antrenor == null)
                {
                    return NotFound();
                }

                // Randevuları tarih sırasına koy
                antrenor.Randevular = antrenor.Randevular
                    .OrderBy(r => r.BaslamaTarihi)
                    .ToList();

                return View(antrenor);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Antrenör detaylarında hata: {ex.Message}");
                return View("Error");
            }
        }
    }
}
