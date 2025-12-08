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
    /// SporSalonuController - Spor salonu yönetimi
    /// Admin'ler spor salonlarını yönetebilir
    /// Üyeler spor salonlarını görüntüleyebilir
    /// </summary>
    [Authorize] // Sadece giriş yapmış kullanıcılar
    public class SporSalonuController : Controller
    {
        private readonly FitnessCenterDbContext _dbContext;
        private readonly ILogger<SporSalonuController> _logger;

        /// <summary>
        /// Constructor - Bağımlılıkları alır
        /// </summary>
        public SporSalonuController(FitnessCenterDbContext dbContext, ILogger<SporSalonuController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // =============================================
        // GET: /SporSalonu/Index
        // Tüm spor salonlarını listele
        // =============================================
        /// <summary>
        /// Tüm spor salonlarını liste halinde gösterir
        /// Tüm giriş yapmış kullanıcılar görebilir
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var sporSalonları = await _dbContext.SporSalonlari
                    .Include(s => s.Hizmetler)
                    .Include(s => s.Antrenorler)
                    .OrderBy(s => s.Ad)
                    .ToListAsync();

                return View(sporSalonları);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Spor salonları indeksinde hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /SporSalonu/Create
        // Spor salonu oluşturma formunu göster (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Yeni spor salonu oluşturma sayfasını gösterir
        /// Sadece Admin rolesu yapabilir
        /// </summary>
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Spor salonu oluşturma sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /SporSalonu/Create
        // Yeni spor salonu kaydını veritabanına ekle
        // =============================================
        /// <summary>
        /// Yeni spor salonu kaydını oluşturur
        /// Sadece Admin rolesu yapabilir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Ad,Adres,CalismaZamanlari,Aciklama")] SporSalonu sporSalonu)
        {
            try
            {
                // Model doğrulama
                if (!ModelState.IsValid)
                {
                    return View(sporSalonu);
                }

                // Veritabanına ekle
                _dbContext.Add(sporSalonu);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Yeni spor salonu oluşturuldu: {sporSalonu.Ad} (ID: {sporSalonu.Id})");

                TempData["BasariliMesaj"] = $"Spor salonu '{sporSalonu.Ad}' başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Spor salonu oluştururken veritabanı hatası: {ex.Message}");
                ModelState.AddModelError("", "Spor salonu oluşturulurken bir hata oluştu.");
                return View(sporSalonu);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Spor salonu oluştururken hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /SporSalonu/Edit/5
        // Spor salonu düzenleme formunu göster (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Spor salonu düzenleme sayfasını gösterir
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

                var sporSalonu = await _dbContext.SporSalonlari.FindAsync(id);
                if (sporSalonu == null)
                {
                    return NotFound();
                }

                return View(sporSalonu);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Spor salonu düzenlemede hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /SporSalonu/Edit/5
        // Spor salonu bilgilerini güncelle
        // =============================================
        /// <summary>
        /// Spor salonu bilgilerini günceller
        /// Sadece Admin rolesu yapabilir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Adres,CalismaZamanlari,Aciklama")] SporSalonu sporSalonu)
        {
            try
            {
                if (id != sporSalonu.Id)
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    return View(sporSalonu);
                }

                _dbContext.Update(sporSalonu);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Spor salonu güncellendi: {sporSalonu.Ad} (ID: {sporSalonu.Id})");

                TempData["BasariliMesaj"] = $"Spor salonu '{sporSalonu.Ad}' başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_dbContext.SporSalonlari.Any(e => e.Id == sporSalonu.Id))
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
                _logger.LogError($"Spor salonu güncellemede hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /SporSalonu/Delete/5
        // Spor salonu silme onay sayfası (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Spor salonu silme onay sayfasını gösterir
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

                var sporSalonu = await _dbContext.SporSalonlari
                    .Include(s => s.Hizmetler)
                    .Include(s => s.Antrenorler)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (sporSalonu == null)
                {
                    return NotFound();
                }

                return View(sporSalonu);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Spor salonu silme sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /SporSalonu/Delete/5
        // Spor salonu silme işlemini gerçekleştir
        // =============================================
        /// <summary>
        /// Spor salonu silme işlemini gerçekleştirir
        /// Kaskadat silme: İlişkili hizmetler ve antrenörler de silinir
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sporSalonu = await _dbContext.SporSalonlari.FindAsync(id);
                if (sporSalonu != null)
                {
                    _dbContext.SporSalonlari.Remove(sporSalonu);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Spor salonu silindi: {sporSalonu.Ad} (ID: {sporSalonu.Id})");

                    TempData["BasariliMesaj"] = $"Spor salonu '{sporSalonu.Ad}' başarıyla silindi.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Spor salonu silmede hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /SporSalonu/Details/5
        // Spor salonu detaylarını göster
        // =============================================
        /// <summary>
        /// Spor salonunun ayrıntılarını, hizmetlerini ve antrenörlerini gösterir
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var sporSalonu = await _dbContext.SporSalonlari
                    .Include(s => s.Hizmetler)
                    .Include(s => s.Antrenorler)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (sporSalonu == null)
                {
                    return NotFound();
                }

                return View(sporSalonu);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Spor salonu detaylarında hata: {ex.Message}");
                return View("Error");
            }
        }
    }
}
