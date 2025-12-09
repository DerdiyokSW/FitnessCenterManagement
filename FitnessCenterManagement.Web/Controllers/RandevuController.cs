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
using FitnessCenterManagement.Web.Services;

namespace FitnessCenterManagement.Web.Controllers
{
    /// <summary>
    /// RandevuController - Randevu yönetimi
    /// Üyeler randevu alabilir, görebilir, iptal edebilir
    /// Admin'ler tüm randevuları görebilir ve yönetebilir
    /// </summary>
    [Authorize] // Sadece giriş yapmış kullanıcılar
    public class RandevuController : Controller
    {
        private readonly FitnessCenterDbContext _dbContext;
        private readonly IRendevuSirvisi _rendevuSirvisi;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RandevuController> _logger;

        /// <summary>
        /// Constructor - Bağımlılıkları alır
        /// </summary>
        public RandevuController(
            FitnessCenterDbContext dbContext,
            IRendevuSirvisi rendevuSirvisi,
            UserManager<IdentityUser> userManager,
            ILogger<RandevuController> logger)
        {
            _dbContext = dbContext;
            _rendevuSirvisi = rendevuSirvisi;
            _userManager = userManager;
            _logger = logger;
        }

        // =============================================
        // GET: /Randevu/Index
        // Tüm randevuları listele (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Tüm randevuları liste halinde gösterir
        /// Sadece Admin'ler görebilir
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string durum)
        {
            try
            {
                var randevularSorgula = _dbContext.Randevular
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet)
                    .AsQueryable();

                // Tarih filtresi
                if (startDate.HasValue)
                {
                    randevularSorgula = randevularSorgula.Where(r => r.BaslamaTarihi >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    randevularSorgula = randevularSorgula.Where(r => r.BitisTarihi <= endDate.Value);
                }

                // Durum filtresi
                if (!string.IsNullOrEmpty(durum))
                {
                    randevularSorgula = randevularSorgula.Where(r => r.Durum == durum);
                }

                var randevular = await randevularSorgula
                    .OrderByDescending(r => r.BaslamaTarihi)
                    .ToListAsync();

                return View(randevular);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Randevu indeksinde hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Randevu/BenimRandevularim
        // Giriş yapmış üyenin randevularını listele
        // =============================================
        /// <summary>
        /// Giriş yapmış üyenin tüm randevularını gösterir
        /// </summary>
        [Authorize(Roles = "Uye")]
        public async Task<IActionResult> BenimRandevularim()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                // Üyenin profil bilgilerini al
                var uye = await _dbContext.Uyeler
                    .FirstOrDefaultAsync(u => u.KullaniciId == currentUser.Id);

                if (uye == null)
                {
                    // Üye profili yoksa yönlendir
                    return RedirectToAction("OlusturProfilim", "Uyeler");
                }

                // Üyenin randevularını getir
                var randevular = await _rendevuSirvisi.UyeRandevulariniGetir(uye.Id);

                return View(randevular);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Benim randevularım sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Randevu/YeniRandevu
        // Randevu oluşturma formunu göster
        // =============================================
        /// <summary>
        /// Yeni randevu oluşturma sayfasını gösterir
        /// </summary>
        [Authorize(Roles = "Uye")]
        public async Task<IActionResult> YeniRandevu()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                // Üyenin profil bilgilerini al
                var uye = await _dbContext.Uyeler
                    .FirstOrDefaultAsync(u => u.KullaniciId == currentUser.Id);

                if (uye == null)
                {
                    // Üye profili yoksa yönlendir
                    TempData["HataMesaji"] = "Lütfen önce üye profilinizi oluşturunuz.";
                    return RedirectToAction("OlusturProfilim", "Uyeler");
                }

                // ViewBag'a antrenörleri ve hizmetleri ekle
                ViewBag.Antrenorler = await _dbContext.Antrenorler
                    .OrderBy(a => a.Ad)
                    .ThenBy(a => a.Soyad)
                    .ToListAsync();

                ViewBag.Hizmetler = await _dbContext.Hizmetler
                    .OrderBy(h => h.Ad)
                    .ToListAsync();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Yeni randevu sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /Randevu/YeniRandevu
        // Randevu kaydını oluştur
        // =============================================
        /// <summary>
        /// Yeni randevu kaydını oluşturur
        /// İş kurallarını kontrol eder (çakışma, çalışma saatleri vb.)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Uye")]
        public async Task<IActionResult> YeniRandevu([Bind("AntrenorId,HizmetId,BaslamaTarihi")] Randevu randevu)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                // Üyenin profil bilgilerini al
                var uye = await _dbContext.Uyeler
                    .FirstOrDefaultAsync(u => u.KullaniciId == currentUser.Id);

                if (uye == null)
                {
                    return RedirectToAction("OlusturProfilim", "Uyeler");
                }

                // Hizmeti getir
                var hizmet = await _dbContext.Hizmetler.FindAsync(randevu.HizmetId);
                if (hizmet == null)
                {
                    ModelState.AddModelError("HizmetId", "Seçilen hizmet bulunamadı.");
                }

                // Model doğrulama
                if (!ModelState.IsValid || hizmet == null)
                {
                    ViewBag.Antrenorler = await _dbContext.Antrenorler.OrderBy(a => a.Ad).ToListAsync();
                    ViewBag.Hizmetler = await _dbContext.Hizmetler.OrderBy(h => h.Ad).ToListAsync();
                    return View(randevu);
                }

                // Randevu zamanını hesapla
                randevu.UyeId = uye.Id;
                randevu.BitisTarihi = randevu.BaslamaTarihi.AddMinutes(hizmet.SureDakika);
                randevu.Ucret = hizmet.Ucret;
                randevu.Durum = "Beklemede";

                try
                {
                    // Randevu oluştur (servis tarafından doğrulanacak)
                    await _rendevuSirvisi.RandevuOlustur(randevu);

                    _logger.LogInformation($"Yeni randevu oluşturuldu: Üye {uye.Id}, Antrenör {randevu.AntrenorId}");

                    TempData["BasariliMesaj"] = "Randevunuz başarıyla oluşturuldu! Durum: Beklemede (Onay için lütfen bekleyiniz)";
                    return RedirectToAction(nameof(BenimRandevularim));
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning($"Randevu oluşturma hatası: {ex.Message}");
                    ModelState.AddModelError("", ex.Message);

                    ViewBag.Antrenorler = await _dbContext.Antrenorler.OrderBy(a => a.Ad).ToListAsync();
                    ViewBag.Hizmetler = await _dbContext.Hizmetler.OrderBy(h => h.Ad).ToListAsync();
                    return View(randevu);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Randevu oluştururken hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /Randevu/Iptal/5
        // Randevuyu iptal et
        // =============================================
        /// <summary>
        /// Randevuyu iptal eder
        /// Sadece o randevunun sahibi veya Admin iptal edebilir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Iptal(int id)
        {
            try
            {
                var randevu = await _dbContext.Randevular.FindAsync(id);
                if (randevu == null)
                {
                    return NotFound();
                }

                // Yetki kontrolü
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                var uye = await _dbContext.Uyeler.FirstOrDefaultAsync(u => u.KullaniciId == currentUser.Id);

                bool isAdmin = User.IsInRole("Admin");
                bool isSinself = uye != null && uye.Id == randevu.UyeId;

                if (!isAdmin && !isSinself)
                {
                    return Unauthorized();
                }

                // İptal et
                bool basarili = await _rendevuSirvisi.RandevuIptalEt(id);

                if (basarili)
                {
                    TempData["BasariliMesaj"] = "Randevu başarıyla iptal edildi.";
                }
                else
                {
                    TempData["HataMesaji"] = "Randevu iptal edilirken bir hata oluştu.";
                }

                return isAdmin
                    ? RedirectToAction(nameof(Index))
                    : RedirectToAction(nameof(BenimRandevularim));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Randevu iptal hatası: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /Randevu/Onayla/5
        // Randevuyu onayla (ADMIN ONLY)
        // =============================================
        /// <summary>
        /// Randevuyu onayla (Beklemede -> Onaylandı)
        /// Sadece Admin yapabilir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Onayla(int id)
        {
            try
            {
                bool basarili = await _rendevuSirvisi.RandevuOnayla(id);

                if (basarili)
                {
                    TempData["BasariliMesaj"] = "Randevu başarıyla onaylandı.";
                }
                else
                {
                    TempData["HataMesaji"] = "Randevu onaylanırken bir hata oluştu.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Randevu onay hatası: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /Randevu/Details/5
        // Randevu detaylarını göster
        // =============================================
        /// <summary>
        /// Randevu detaylarını gösterir
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var randevu = await _dbContext.Randevular
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet)
                    .FirstOrDefaultAsync(r => r.Id == id);

                // Randevu kontrolü
                if (randevu == null)
                {
                    return NotFound();
                }

                // Yetki kontrolü (kendi randevu veya Admin)
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                var uye = await _dbContext.Uyeler.FirstOrDefaultAsync(u => u.KullaniciId == currentUser.Id);

                if (!User.IsInRole("Admin") && (uye == null || uye.Id != randevu.UyeId))
                {
                    return Unauthorized();
                }

                return View(randevu);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Randevu detaylarında hata: {ex.Message}");
                return View("Error");
            }
        }
    }
}
