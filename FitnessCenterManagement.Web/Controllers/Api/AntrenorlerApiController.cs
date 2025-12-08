using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Web.Data;
using FitnessCenterManagement.Web.Models;

namespace FitnessCenterManagement.Web.Controllers.Api
{
    /// <summary>
    /// AntrenorlerApiController - Antrenörleri REST API üzerinden sunan controller
    /// JSON formatında veri döndürür
    /// LINQ ile filtreleme yapılır
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // API çağrıları da yetkilendirme gerektirir
    public class AntrenorlerApiController : ControllerBase
    {
        private readonly FitnessCenterDbContext _dbContext;
        private readonly ILogger<AntrenorlerApiController> _logger;

        /// <summary>
        /// Constructor - Bağımlılıkları alır
        /// </summary>
        public AntrenorlerApiController(FitnessCenterDbContext dbContext, ILogger<AntrenorlerApiController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // =============================================
        // GET: /api/antrenorler
        // Tüm antrenörleri getir
        // =============================================
        /// <summary>
        /// Tüm antrenörleri JSON formatında döndürür
        /// 
        /// Örnek istek:
        /// GET /api/antrenorler
        /// 
        /// Örnek cevap:
        /// [
        ///   {
        ///     "id": 1,
        ///     "ad": "Ahmet",
        ///     "soyad": "Yilmaz",
        ///     "uzmanlıkAlanlari": "Fitness, Yoga",
        ///     "email": "ahmet@example.com"
        ///   }
        /// ]
        /// </summary>
        [HttpGet]
        [ProduceResponseType(StatusCodes.Status200OK)]
        [ProduceResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AntrenorDto>>> GetAntrenorler()
        {
            try
            {
                // LINQ ile antrenörleri getir ve DTO'ya dönüştür
                var antrenorler = await _dbContext.Antrenorler
                    .Include(a => a.SporSalonu)
                    .Select(a => new AntrenorDto
                    {
                        Id = a.Id,
                        Ad = a.Ad,
                        Soyad = a.Soyad,
                        UzmanlıkAlanlari = a.UzmanlıkAlanlari,
                        Telefon = a.Telefon,
                        Email = a.Email,
                        SporSalonuAdi = a.SporSalonu.Ad,
                        MevcutBaslangicSaati = a.MevcutBaslangicSaati,
                        MevcutBitisSaati = a.MevcutBitisSaati
                    })
                    .OrderBy(a => a.Ad)
                    .ThenBy(a => a.Soyad)
                    .ToListAsync();

                _logger.LogInformation($"API: {antrenorler.Count} antrenör getirildi.");

                return Ok(antrenorler);
            }
            catch (Exception ex)
            {
                _logger.LogError($"API GET /antrenorler hatası: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Antrenörleri getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // =============================================
        // GET: /api/antrenorler/5
        // Belirli antrenörü getir
        // =============================================
        /// <summary>
        /// Belirtilen ID'li antrenörü JSON formatında döndürür
        /// 
        /// Örnek istek:
        /// GET /api/antrenorler/1
        /// </summary>
        [HttpGet("{id}")]
        [ProduceResponseType(StatusCodes.Status200OK)]
        [ProduceResponseType(StatusCodes.Status404NotFound)]
        [ProduceResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AntrenorDto>> GetAntrenor(int id)
        {
            try
            {
                // LINQ ile belirli antrenörü getir
                var antrenor = await _dbContext.Antrenorler
                    .Include(a => a.SporSalonu)
                    .Where(a => a.Id == id)
                    .Select(a => new AntrenorDto
                    {
                        Id = a.Id,
                        Ad = a.Ad,
                        Soyad = a.Soyad,
                        UzmanlıkAlanlari = a.UzmanlıkAlanlari,
                        Telefon = a.Telefon,
                        Email = a.Email,
                        SporSalonuAdi = a.SporSalonu.Ad,
                        MevcutBaslangicSaati = a.MevcutBaslangicSaati,
                        MevcutBitisSaati = a.MevcutBitisSaati
                    })
                    .FirstOrDefaultAsync();

                if (antrenor == null)
                {
                    _logger.LogWarning($"API: Antrenör ID {id} bulunamadı.");
                    return NotFound(new { message = $"Antrenör ID {id} bulunamadı." });
                }

                _logger.LogInformation($"API: Antrenör {id} getirildi.");

                return Ok(antrenor);
            }
            catch (Exception ex)
            {
                _logger.LogError($"API GET /antrenorler/{id} hatası: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Antrenörü getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // =============================================
        // GET: /api/antrenorler/available?date=2025-12-09
        // Belirli tarihte müsait antrenörleri getir (LINQ FİLTRELEME)
        // =============================================
        /// <summary>
        /// Belirli bir tarihte müsait antrenörleri döndürür
        /// 
        /// LINQ ile filtreleme yapılır:
        /// - Antrenörün çalışma saatleri kontrol edilir
        /// - Mevcut randevular kontrol edilir
        /// 
        /// Örnek istek:
        /// GET /api/antrenorler/available?date=2025-12-09
        /// 
        /// Query Parameters:
        /// - date (DateTime): Kontrol edilecek tarih (YYYY-MM-DD formatında)
        /// </summary>
        [HttpGet("available")]
        [ProduceResponseType(StatusCodes.Status200OK)]
        [ProduceResponseType(StatusCodes.Status400BadRequest)]
        [ProduceResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AntrenorDto>>> GetMusaitAntrenorler([FromQuery] DateTime date)
        {
            try
            {
                // Tarih doğrulaması
                if (date == default(DateTime))
                {
                    return BadRequest(new { message = "Lütfen geçerli bir tarih giriniz (YYYY-MM-DD formatında)." });
                }

                // LINQ ile müsait antrenörleri bulma
                var musaitAntrenorler = await _dbContext.Antrenorler
                    .Include(a => a.SporSalonu)
                    .Include(a => a.Randevular) // Randevuları da yükle
                    .Where(a =>
                        // Çalışma saatleri kontrol et (varsa)
                        (a.MevcutBaslangicSaati == null || a.MevcutBitisSaati == null) ||
                        (a.MevcutBaslangicSaati <= DateTime.Now.TimeOfDay && 
                         a.MevcutBitisSaati >= DateTime.Now.TimeOfDay)
                    )
                    .Where(a =>
                        // Belirtilen tarihte çakışan randevu yoksa müsait
                        !a.Randevular.Any(r =>
                            r.BaslamaTarihi.Date == date.Date &&
                            r.Durum != "İptal Edildi"
                        )
                    )
                    .Select(a => new AntrenorDto
                    {
                        Id = a.Id,
                        Ad = a.Ad,
                        Soyad = a.Soyad,
                        UzmanlıkAlanlari = a.UzmanlıkAlanlari,
                        Telefon = a.Telefon,
                        Email = a.Email,
                        SporSalonuAdi = a.SporSalonu.Ad,
                        MevcutBaslangicSaati = a.MevcutBaslangicSaati,
                        MevcutBitisSaati = a.MevcutBitisSaati
                    })
                    .OrderBy(a => a.Ad)
                    .ToListAsync();

                _logger.LogInformation($"API: {musaitAntrenorler.Count} müsait antrenör bulundu (Tarih: {date:yyyy-MM-dd}).");

                return Ok(new
                {
                    date = date.ToString("yyyy-MM-dd"),
                    count = musaitAntrenorler.Count,
                    data = musaitAntrenorler
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"API GET /antrenorler/available hatası: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Müsait antrenörleri getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // =============================================
        // GET: /api/antrenorler/search?uzmanlik=Yoga
        // Uzmanlık alanına göre antrenörleri ara (LINQ ARAMA)
        // =============================================
        /// <summary>
        /// Belirtilen uzmanlık alanına sahip antrenörleri döndürür
        /// 
        /// LINQ ile arama yapılır:
        /// - Uzmanlık alanlarında metin araması yapılır (büyük/küçük harf duyarsız)
        /// 
        /// Örnek istek:
        /// GET /api/antrenorler/search?uzmanlik=Yoga
        /// 
        /// Query Parameters:
        /// - uzmanlik (string): Aranacak uzmanlık alanı
        /// </summary>
        [HttpGet("search")]
        [ProduceResponseType(StatusCodes.Status200OK)]
        [ProduceResponseType(StatusCodes.Status400BadRequest)]
        [ProduceResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AntrenorDto>>> SearchAntrenorler([FromQuery] string uzmanlik)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(uzmanlik))
                {
                    return BadRequest(new { message = "Lütfen bir uzmanlık alanı giriniz." });
                }

                // LINQ ile ara
                var aramaSonuclari = await _dbContext.Antrenorler
                    .Include(a => a.SporSalonu)
                    .Where(a => a.UzmanlıkAlanlari != null && 
                                a.UzmanlıkAlanlari.ToLower().Contains(uzmanlik.ToLower()))
                    .Select(a => new AntrenorDto
                    {
                        Id = a.Id,
                        Ad = a.Ad,
                        Soyad = a.Soyad,
                        UzmanlıkAlanlari = a.UzmanlıkAlanlari,
                        Telefon = a.Telefon,
                        Email = a.Email,
                        SporSalonuAdi = a.SporSalonu.Ad,
                        MevcutBaslangicSaati = a.MevcutBaslangicSaati,
                        MevcutBitisSaati = a.MevcutBitisSaati
                    })
                    .OrderBy(a => a.Ad)
                    .ToListAsync();

                _logger.LogInformation($"API: '{uzmanlik}' için {aramaSonuclari.Count} antrenör bulundu.");

                return Ok(new
                {
                    searchTerm = uzmanlik,
                    count = aramaSonuclari.Count,
                    data = aramaSonuclari
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"API GET /antrenorler/search hatası: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Araştırma sırasında bir hata oluştu.", error = ex.Message });
            }
        }
    }

    // =============================================
    // DTO SINIFLARı (DATA TRANSFER OBJECT)
    // =============================================

    /// <summary>
    /// AntrenorDto - API tarafından döndürülen antrenör bilgisi
    /// Entity yerine kullanılıyor (güvenlik ve performans için)
    /// </summary>
    public class AntrenorDto
    {
        /// <summary>
        /// Antrenörün kimliği
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Antrenörün adı
        /// </summary>
        public string Ad { get; set; }

        /// <summary>
        /// Antrenörün soyadı
        /// </summary>
        public string Soyad { get; set; }

        /// <summary>
        /// Antrenörün uzmanlık alanları
        /// </summary>
        public string UzmanlıkAlanlari { get; set; }

        /// <summary>
        /// Antrenörün telefonu
        /// </summary>
        public string Telefon { get; set; }

        /// <summary>
        /// Antrenörün e-postaı
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Antrenörün çalıştığı spor salonunun adı
        /// </summary>
        public string SporSalonuAdi { get; set; }

        /// <summary>
        /// Antrenörün başlangıç saati
        /// </summary>
        public TimeSpan? MevcutBaslangicSaati { get; set; }

        /// <summary>
        /// Antrenörün bitiş saati
        /// </summary>
        public TimeSpan? MevcutBitisSaati { get; set; }
    }
}
