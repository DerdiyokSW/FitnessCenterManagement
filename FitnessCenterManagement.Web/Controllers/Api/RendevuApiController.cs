using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Web.Data;
using FitnessCenterManagement.Web.Models;

namespace FitnessCenterManagement.Web.Controllers.Api
{
    /// <summary>
    /// RendevuApiController - Randevuları REST API üzerinden sunan controller
    /// LINQ ile filtreleme ve arama yapılır
    /// Üye randevularını, tarih bazlı randevuları getirir
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // API çağrıları yetkilendirme gerektirir
    public class RendevuApiController : ControllerBase
    {
        private readonly FitnessCenterDbContext _dbContext;
        private readonly ILogger<RendevuApiController> _logger;

        /// <summary>
        /// Constructor - Bağımlılıkları alır
        /// </summary>
        public RendevuApiController(FitnessCenterDbContext dbContext, ILogger<RendevuApiController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // =============================================
        // GET: /api/randevu
        // Tüm randevuları getir
        // =============================================
        /// <summary>
        /// Tüm randevuları JSON formatında döndürür
        /// Üye, antrenör ve hizmet bilgilerini içerir
        /// 
        /// Örnek istek:
        /// GET /api/randevu
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RendevuDto>>> GetRandevular()
        {
            try
            {
                // LINQ ile tüm randevuları getir (navigation properties dahil)
                var randevular = await _dbContext.Randevular
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet)
                    .Select(r => new RendevuDto
                    {
                        Id = r.Id,
                        UyeAdi = r.Uye != null ? $"{r.Uye.Ad} {r.Uye.Soyad}" : "Belirtilmedi",
                        AntrenorAdi = r.Antrenor != null ? $"{r.Antrenor.Ad} {r.Antrenor.Soyad}" : "Belirtilmedi",
                        HizmetAdi = r.Hizmet != null ? r.Hizmet.Ad : "Belirtilmedi",
                        BaslamaTarihi = r.BaslamaTarihi,
                        BitisTarihi = r.BitisTarihi,
                        Ucret = r.Ucret,
                        Durum = r.Durum ?? "Beklemede"
                    })
                    .OrderByDescending(r => r.BaslamaTarihi)
                    .ToListAsync();

                _logger.LogInformation($"API: {randevular.Count} randevu getirildi.");
                return Ok(randevular);
            }
            catch (Exception ex)
            {
                _logger.LogError($"API GET /randevu hatası: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Randevuları getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // =============================================
        // GET: /api/randevu/uye/{uyeId}
        // Belirli üyenin tüm randevularını getir (LINQ FILTRELEME)
        // =============================================
        /// <summary>
        /// Belirtilen üyenin tüm randevularını döndürür
        /// 
        /// LINQ ile filtreleme yapılır:
        /// - Üye ID'sine göre randevular filtrelenir
        /// - Tarihine göre sıralanır
        /// 
        /// Örnek istek:
        /// GET /api/randevu/uye/1
        /// </summary>
        [HttpGet("uye/{uyeId}")]
        public async Task<ActionResult<IEnumerable<RendevuDto>>> GetUyeRendevusu(int uyeId)
        {
            try
            {
                // LINQ ile üyenin randevularını getir
                var uyeRandevusu = await _dbContext.Randevular
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet)
                    .Where(r => r.UyeId == uyeId)
                    .Select(r => new RendevuDto
                    {
                        Id = r.Id,
                        UyeAdi = r.Uye != null ? $"{r.Uye.Ad} {r.Uye.Soyad}" : "Belirtilmedi",
                        AntrenorAdi = r.Antrenor != null ? $"{r.Antrenor.Ad} {r.Antrenor.Soyad}" : "Belirtilmedi",
                        HizmetAdi = r.Hizmet != null ? r.Hizmet.Ad : "Belirtilmedi",
                        BaslamaTarihi = r.BaslamaTarihi,
                        BitisTarihi = r.BitisTarihi,
                        Ucret = r.Ucret,
                        Durum = r.Durum ?? "Beklemede"
                    })
                    .OrderByDescending(r => r.BaslamaTarihi)
                    .ToListAsync();

                if (!uyeRandevusu.Any())
                {
                    _logger.LogWarning($"API: Üye ID {uyeId} için randevu bulunamadı.");
                    return NotFound(new { message = $"Üye ID {uyeId} için randevu bulunamadı." });
                }

                _logger.LogInformation($"API: Üye {uyeId} için {uyeRandevusu.Count} randevu getirildi.");
                return Ok(new
                {
                    uyeId = uyeId,
                    count = uyeRandevusu.Count,
                    data = uyeRandevusu
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"API GET /randevu/uye/{uyeId} hatası: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Üye randevuları getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // =============================================
        // GET: /api/randevu/antrenor/{antrenorId}
        // Belirli antrenörün randevularını getir (LINQ FILTRELEME)
        // =============================================
        /// <summary>
        /// Belirtilen antrenörün tüm randevularını döndürür
        /// 
        /// Örnek istek:
        /// GET /api/randevu/antrenor/1
        /// </summary>
        [HttpGet("antrenor/{antrenorId}")]
        public async Task<ActionResult<IEnumerable<RendevuDto>>> GetAntrenorRendevusu(int antrenorId)
        {
            try
            {
                // LINQ ile antrenörün randevularını getir
                var antrenorRandevusu = await _dbContext.Randevular
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet)
                    .Where(r => r.AntrenorId == antrenorId)
                    .Select(r => new RendevuDto
                    {
                        Id = r.Id,
                        UyeAdi = r.Uye != null ? $"{r.Uye.Ad} {r.Uye.Soyad}" : "Belirtilmedi",
                        AntrenorAdi = r.Antrenor != null ? $"{r.Antrenor.Ad} {r.Antrenor.Soyad}" : "Belirtilmedi",
                        HizmetAdi = r.Hizmet != null ? r.Hizmet.Ad : "Belirtilmedi",
                        BaslamaTarihi = r.BaslamaTarihi,
                        BitisTarihi = r.BitisTarihi,
                        Ucret = r.Ucret,
                        Durum = r.Durum ?? "Beklemede"
                    })
                    .OrderByDescending(r => r.BaslamaTarihi)
                    .ToListAsync();

                if (!antrenorRandevusu.Any())
                {
                    _logger.LogWarning($"API: Antrenör ID {antrenorId} için randevu bulunamadı.");
                    return NotFound(new { message = $"Antrenör ID {antrenorId} için randevu bulunamadı." });
                }

                _logger.LogInformation($"API: Antrenör {antrenorId} için {antrenorRandevusu.Count} randevu getirildi.");
                return Ok(new
                {
                    antrenorId = antrenorId,
                    count = antrenorRandevusu.Count,
                    data = antrenorRandevusu
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"API GET /randevu/antrenor/{antrenorId} hatası: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Antrenör randevuları getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // =============================================
        // GET: /api/randevu/tarih-araligi?baslangic=2025-12-01&bitis=2025-12-31
        // Belirli tarih aralığında randevuları getir (LINQ FILTRELEME)
        // =============================================
        /// <summary>
        /// Belirtilen tarih aralığındaki randevuları döndürür
        /// 
        /// LINQ ile filtreleme yapılır:
        /// - Başlangıç ve bitiş tarihleri arasındaki randevular getirilir
        /// 
        /// Örnek istek:
        /// GET /api/randevu/tarih-araligi?baslangic=2025-12-01&bitis=2025-12-31
        /// 
        /// Query Parameters:
        /// - baslangic (DateTime): Başlangıç tarihi (YYYY-MM-DD formatında)
        /// - bitis (DateTime): Bitiş tarihi (YYYY-MM-DD formatında)
        /// </summary>
        [HttpGet("tarih-araligi")]
        public async Task<ActionResult<IEnumerable<RendevuDto>>> GetRendevuByTarihAraligi(
            [FromQuery] DateTime baslangic, 
            [FromQuery] DateTime bitis)
        {
            try
            {
                // Tarih doğrulaması
                if (baslangic == default(DateTime) || bitis == default(DateTime))
                {
                    return BadRequest(new { message = "Lütfen geçerli tarih aralığı giriniz (YYYY-MM-DD formatında)." });
                }

                if (baslangic > bitis)
                {
                    return BadRequest(new { message = "Başlangıç tarihi, bitiş tarihinden önce olmalıdır." });
                }

                // LINQ ile tarih aralığında randevuları getir
                var tarihAraligindaRandevular = await _dbContext.Randevular
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet)
                    .Where(r => r.BaslamaTarihi.Date >= baslangic.Date && 
                                r.BaslamaTarihi.Date <= bitis.Date)
                    .Select(r => new RendevuDto
                    {
                        Id = r.Id,
                        UyeAdi = r.Uye != null ? $"{r.Uye.Ad} {r.Uye.Soyad}" : "Belirtilmedi",
                        AntrenorAdi = r.Antrenor != null ? $"{r.Antrenor.Ad} {r.Antrenor.Soyad}" : "Belirtilmedi",
                        HizmetAdi = r.Hizmet != null ? r.Hizmet.Ad : "Belirtilmedi",
                        BaslamaTarihi = r.BaslamaTarihi,
                        BitisTarihi = r.BitisTarihi,
                        Ucret = r.Ucret,
                        Durum = r.Durum ?? "Beklemede"
                    })
                    .OrderByDescending(r => r.BaslamaTarihi)
                    .ToListAsync();

                _logger.LogInformation($"API: {tarihAraligindaRandevular.Count} randevu getirildi " +
                    $"({baslangic:yyyy-MM-dd} - {bitis:yyyy-MM-dd}).");

                return Ok(new
                {
                    baslangic = baslangic.ToString("yyyy-MM-dd"),
                    bitis = bitis.ToString("yyyy-MM-dd"),
                    count = tarihAraligindaRandevular.Count,
                    data = tarihAraligindaRandevular
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"API GET /randevu/tarih-araligi hatası: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Randevuları getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // =============================================
        // GET: /api/randevu/durum/{durum}
        // Belirli durumda olan randevuları getir (LINQ FILTRELEME)
        // =============================================
        /// <summary>
        /// Belirtilen durumda olan randevuları döndürür
        /// 
        /// Durumlar: "Beklemede", "Onaylı", "Reddedildi", "İptal Edildi"
        /// 
        /// Örnek istek:
        /// GET /api/randevu/durum/Onaylı
        /// </summary>
        [HttpGet("durum/{durum}")]
        public async Task<ActionResult<IEnumerable<RendevuDto>>> GetRendevuByDurum(string durum)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(durum))
                {
                    return BadRequest(new { message = "Lütfen bir durum giriniz." });
                }

                // LINQ ile duruma göre randevuları getir
                var durumnaRandevular = await _dbContext.Randevular
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet)
                    .Where(r => (r.Durum ?? "Beklemede") == durum)
                    .Select(r => new RendevuDto
                    {
                        Id = r.Id,
                        UyeAdi = r.Uye != null ? $"{r.Uye.Ad} {r.Uye.Soyad}" : "Belirtilmedi",
                        AntrenorAdi = r.Antrenor != null ? $"{r.Antrenor.Ad} {r.Antrenor.Soyad}" : "Belirtilmedi",
                        HizmetAdi = r.Hizmet != null ? r.Hizmet.Ad : "Belirtilmedi",
                        BaslamaTarihi = r.BaslamaTarihi,
                        BitisTarihi = r.BitisTarihi,
                        Ucret = r.Ucret,
                        Durum = r.Durum ?? "Beklemede"
                    })
                    .OrderByDescending(r => r.BaslamaTarihi)
                    .ToListAsync();

                _logger.LogInformation($"API: '{durum}' durumunda {durumnaRandevular.Count} randevu getirildi.");

                return Ok(new
                {
                    durum = durum,
                    count = durumnaRandevular.Count,
                    data = durumnaRandevular
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"API GET /randevu/durum/{durum} hatası: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Randevuları getirilirken bir hata oluştu.", error = ex.Message });
            }
        }
    }

    // =============================================
    // DTO SINIFLARı (DATA TRANSFER OBJECT)
    // =============================================

    /// <summary>
    /// RendevuDto - API tarafından döndürülen randevu bilgisi
    /// Entity yerine kullanılıyor (güvenlik ve performans için)
    /// </summary>
    public class RendevuDto
    {
        /// <summary>
        /// Randevunun kimliği
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Randevu yapan üyenin adı ve soyadı
        /// </summary>
        public string? UyeAdi { get; set; }

        /// <summary>
        /// Antrenörün adı ve soyadı
        /// </summary>
        public string? AntrenorAdi { get; set; }

        /// <summary>
        /// Hizmetin adı
        /// </summary>
        public string? HizmetAdi { get; set; }

        /// <summary>
        /// Randevunun başlama tarihi ve saati
        /// </summary>
        public DateTime BaslamaTarihi { get; set; }

        /// <summary>
        /// Randevunun bitiş tarihi ve saati
        /// </summary>
        public DateTime BitisTarihi { get; set; }

        /// <summary>
        /// Randevunun ücreti
        /// </summary>
        public decimal Ucret { get; set; }

        /// <summary>
        /// Randevunun durumu (Beklemede, Onaylı, Reddedildi, İptal Edildi)
        /// </summary>
        public string? Durum { get; set; }
    }
}
