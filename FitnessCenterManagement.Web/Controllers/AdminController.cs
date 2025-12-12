using FitnessCenterManagement.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManagement.Web.Controllers
{
    /// <summary>
    /// Admin Kontrolleri - Sistem yönetimi ve istatistikleri
    /// Sadece Admin rolüne sahip kullanıcılar tarafından erişilebilir
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly FitnessCenterDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(FitnessCenterDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Admin Dashboard - Sistem istatistiklerini gösterir
        /// GET: /Admin
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Admin dashboard yükleniyor");

                // Sistem istatistikleri
                var sporSalonuSayisi = await _context.SporSalonlari.CountAsync();
                var antrenorSayisi = await _context.Antrenorler.CountAsync();
                var uyeSayisi = await _context.Uyeler.CountAsync();
                var randevuSayisi = await _context.Randevular.CountAsync();

                // ViewBag'e istatistikleri ekle
                ViewBag.SporSalonuSayisi = sporSalonuSayisi;
                ViewBag.AntrenorSayisi = antrenorSayisi;
                ViewBag.UyeSayisi = uyeSayisi;
                ViewBag.RandevuSayisi = randevuSayisi;

                _logger.LogInformation(
                    "Admin Dashboard - Salon: {SalonSayisi}, Antrenor: {AntrenorSayisi}, Uye: {UyeSayisi}, Randevu: {RandevuSayisi}",
                    sporSalonuSayisi, antrenorSayisi, uyeSayisi, randevuSayisi
                );

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin dashboard yüklenirken hata oluştu");
                return View("Error");
            }
        }

        /// <summary>
        /// Onay Bekleyen Randevuları Göster
        /// GET: /Admin/ManageRandevu
        /// </summary>
        public async Task<IActionResult> ManageRandevu()
        {
            try
            {
                _logger.LogInformation("Randevu yönetim sayfası yükleniyor");

                // Duruma göre randevuları gruplayan sorgu
                var randevular = await _context.Randevular
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet)
                    .OrderBy(r => r.BaslamaTarihi)
                    .ToListAsync();

                return View(randevular);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Randevu yönetim sayfası yüklenirken hata oluştu");
                return View("Error");
            }
        }

        /// <summary>
        /// Randevu Durumunu Güncelle (Onayla/Reddet/İptal Et)
        /// POST: /Admin/UpdateRandevuStatus
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateRandevuStatus(int id, string durum)
        {
            try
            {
                // Geçerli durum değerleri
                var gecerliDurumlar = new[] { "Beklemede", "Onaylı", "Reddedildi", "İptal Edildi" };

                // Durum değerini kontrol et
                if (!gecerliDurumlar.Contains(durum))
                {
                    _logger.LogWarning("Geçersiz randevu durumu girildi: {Durum}", durum);
                    return BadRequest("Geçersiz durum değeri");
                }

                // Randevuyu bulup güncelle
                var randevu = await _context.Randevular.FindAsync(id);
                if (randevu == null)
                {
                    _logger.LogWarning("Randevu bulunamadı: {RandevuId}", id);
                    return NotFound("Randevu bulunamadı");
                }

                var eskiDurum = randevu.Durum;
                randevu.Durum = durum;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Randevu durumu güncellendi - ID: {RandevuId}, Eski: {EskiDurum}, Yeni: {YeniDurum}",
                    id, eskiDurum, durum
                );

                return Ok(new { message = "Randevu başarıyla güncellendi", durum = durum });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Randevu durumu güncellenirken hata oluştu - ID: {RandevuId}", id);
                return StatusCode(500, "Sunucu hatası");
            }
        }

        /// <summary>
        /// Sistem İstatistikleri API Endpoint
        /// GET: /Admin/GetStatistics
        /// JSON formatında istatistikleri döner
        /// </summary>
        [HttpGet]
        [Route("api/admin/statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var istatistikler = new
                {
                    sporSalonuSayisi = await _context.SporSalonlari.CountAsync(),
                    antrenorSayisi = await _context.Antrenorler.CountAsync(),
                    uyeSayisi = await _context.Uyeler.CountAsync(),
                    randevuSayisi = await _context.Randevular.CountAsync(),
                    
                    // Durum bazında randevu sayıları
                    randevuDurumuBeklemede = await _context.Randevular
                        .Where(r => r.Durum == "Beklemede")
                        .CountAsync(),
                    randevuDurumuOnaylı = await _context.Randevular
                        .Where(r => r.Durum == "Onaylı")
                        .CountAsync(),
                    randevuDurumuReddedildi = await _context.Randevular
                        .Where(r => r.Durum == "Reddedildi")
                        .CountAsync(),
                    
                    // Tarih bilgisi
                    güncelleme = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")
                };

                _logger.LogInformation("İstatistikler API çağrıldı");
                return Ok(istatistikler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İstatistikler API'sinde hata oluştu");
                return StatusCode(500, "Hata oluştu");
            }
        }

        /// <summary>
        /// Onay Bekleyen Randevuları API Endpoint'ten Getir
        /// GET: /Admin/GetPendingRandevu
        /// </summary>
        [HttpGet]
        [Route("api/admin/pending-randevu")]
        public async Task<IActionResult> GetPendingRandevu()
        {
            try
            {
                var beklemedeliRandevular = await _context.Randevular
                    .Where(r => r.Durum == "Beklemede")
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet)
                    .Select(r => new
                    {
                        id = r.Id,
                        uyeAdi = r.Uye.Ad + " " + r.Uye.Soyad,
                        antrenorAdi = r.Antrenor.Ad + " " + r.Antrenor.Soyad,
                        hizmetAdi = r.Hizmet.Ad,
                        baslamaTarihi = r.BaslamaTarihi.ToString("dd.MM.yyyy HH:mm"),
                        bitisTarihi = r.BitisTarihi.ToString("dd.MM.yyyy HH:mm"),
                        ucret = r.Ucret
                    })
                    .OrderBy(r => r.baslamaTarihi)
                    .ToListAsync();

                _logger.LogInformation("Onay bekleyen randevular listelendi: {Sayi}", beklemedeliRandevular.Count);
                return Ok(beklemedeliRandevular);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Onay bekleyen randevular API'sinde hata oluştu");
                return StatusCode(500, "Hata oluştu");
            }
        }

        /// <summary>
        /// Admin İşlemleri Log'u
        /// GET: /Admin/ActivityLog
        /// </summary>
        public IActionResult ActivityLog()
        {
            try
            {
                _logger.LogInformation("Etkinlik günlüğü sayfası yükleniyor");
                // Bu özellik future geliştirmeler için yer tutucu
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Etkinlik günlüğü yüklenirken hata oluştu");
                return View("Error");
            }
        }
    }
}
