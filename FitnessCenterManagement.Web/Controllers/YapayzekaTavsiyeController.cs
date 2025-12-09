using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using FitnessCenterManagement.Web.Data;
using FitnessCenterManagement.Web.Models;
using FitnessCenterManagement.Web.Services;

namespace FitnessCenterManagement.Web.Controllers
{
    /// <summary>
    /// YapayzekaTavsiyeController - Yapay zeka tarafından verilen tavsiyeler
    /// Üyeler egzersiz, diyet, vücut tipi analizi gibi tavsiyeler alabilirler
    /// </summary>
    [Authorize(Roles = "Uye")] // Sadece üyeler bu işlemleri yapabilir
    public class YapayzekaTavsiyeController : Controller
    {
        private readonly FitnessCenterDbContext _dbContext;
        private readonly IYapayzekaSirvisi _yapayzekaSirvisi;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<YapayzekaTavsiyeController> _logger;

        /// <summary>
        /// Constructor - Bağımlılıkları alır
        /// </summary>
        public YapayzekaTavsiyeController(
            FitnessCenterDbContext dbContext,
            IYapayzekaSirvisi yapayzekaSirvisi,
            UserManager<IdentityUser> userManager,
            ILogger<YapayzekaTavsiyeController> logger)
        {
            _dbContext = dbContext;
            _yapayzekaSirvisi = yapayzekaSirvisi;
            _userManager = userManager;
            _logger = logger;
        }

        // =============================================
        // GET: /YapayzekaTavsiye/Index
        // Geçmiş tavsiye isteğilerini listele
        // =============================================
        /// <summary>
        /// Üyenin geçmiş yapay zeka tavsiye isteğlerini gösterir
        /// </summary>
        public async Task<IActionResult> Index()
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
                    TempData["HataMesaji"] = "Lütfen önce üye profilinizi oluşturunuz.";
                    return RedirectToAction("OlusturProfilim", "Uyeler");
                }

                // Üyenin tavsiye isteğlerini getir
                var tavsiyeler = await _dbContext.YapayzekaTavsiyeler
                    .Where(yt => yt.UyeId == uye.Id)
                    .OrderByDescending(yt => yt.OlusturmaTarihi)
                    .ToListAsync();

                return View(tavsiyeler);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Yapay zeka tavsiye indeksinde hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /YapayzekaTavsiye/EgzersizTavsiyesi
        // Egzersiz tavsiyesi formu
        // =============================================
        /// <summary>
        /// Egzersiz tavsiyesi almak için formu gösterir
        /// </summary>
        public async Task<IActionResult> EgzersizTavsiyesi()
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
                    TempData["HataMesaji"] = "Lütfen önce üye profilinizi oluşturunuz.";
                    return RedirectToAction("OlusturProfilim", "Uyeler");
                }

                // Üyenin mevcut bilgilerini model'e yükle
                var model = new EgzersizTavsiyesiViewModel
                {
                    Boy = uye.BouSantimetre ?? 0,
                    Agirlik = uye.AgirlikKilogram ?? 0,
                    Cinsiyet = uye.Cinsiyet ?? "",
                    FitnessHedefi = uye.FitnessHedefi ?? ""
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Egzersiz tavsiyesi sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /YapayzekaTavsiye/EgzersizTavsiyesi
        // Egzersiz tavsiyesi isteği gönder
        // =============================================
        /// <summary>
        /// Yapay zeka'ya egzersiz tavsiyesi isteği gönderir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EgzersizTavsiyesi(EgzersizTavsiyesiViewModel model)
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

                // Model doğrulama
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Yapay zeka isteğini oluştur
                var tavsiye = new YapayzekaTavsiye
                {
                    UyeId = uye.Id,
                    TavsiyeTipi = "EgzersizPlani",
                    GirdiVeri = JsonConvert.SerializeObject(new
                    {
                        boy = model.Boy,
                        agirlik = model.Agirlik,
                        cinsiyet = model.Cinsiyet,
                        hedef = model.FitnessHedefi
                    }),
                    OlusturmaTarihi = DateTime.UtcNow
                };

                // Veritabanına ekle
                _dbContext.YapayzekaTavsiyeler.Add(tavsiye);
                await _dbContext.SaveChangesAsync();

                try
                {
                    // Yapay zeka servisini çağır
                    if (model.Cinsiyet == null || model.FitnessHedefi == null)
                    {
                        return BadRequest("Cinsiyet ve Fitness hedefi gereklidir.");
                    }

                    var cevap = await _yapayzekaSirvisi.EgzersizTavsiyesiAl(
                        model.Boy,
                        model.Agirlik,
                        model.Cinsiyet,
                        model.FitnessHedefi
                    );

                    // Tavsiye nesnesi güncelle
                    tavsiye.CiktiVeri = cevap;
                    tavsiye.IslemBasarili = true;
                    tavsiye.CevapTarihi = DateTime.UtcNow;

                    _dbContext.YapayzekaTavsiyeler.Update(tavsiye);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Egzersiz tavsiyesi başarıyla oluşturuldu. Tavsiye ID: {tavsiye.Id}");

                    TempData["BasariliMesaj"] = "Tavsiye başarıyla oluşturuldu!";
                    return RedirectToAction(nameof(TavsiyeDetaylari), new { id = tavsiye.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Yapay zeka çağrısında hata: {ex.Message}");

                    // Hata bilgisi kaydet
                    tavsiye.IslemBasarili = false;
                    tavsiye.HataMesaji = $"Yapay zeka servisi hatası: {ex.Message}";
                    tavsiye.CevapTarihi = DateTime.UtcNow;

                    _dbContext.YapayzekaTavsiyeler.Update(tavsiye);
                    await _dbContext.SaveChangesAsync();

                    TempData["HataMesaji"] = "Yapay zeka servisinde hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Egzersiz tavsiyesi oluştururken hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /YapayzekaTavsiye/DiyetTavsiyesi
        // Diyet tavsiyesi formu
        // =============================================
        /// <summary>
        /// Diyet tavsiyesi almak için formu gösterir
        /// </summary>
        public async Task<IActionResult> DiyetTavsiyesi()
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
                    TempData["HataMesaji"] = "Lütfen önce üye profilinizi oluşturunuz.";
                    return RedirectToAction("OlusturProfilim", "Uyeler");
                }

                // Üyenin mevcut bilgilerini model'e yükle
                var model = new DiyetTavsiyesiViewModel
                {
                    Boy = uye.BouSantimetre ?? 0,
                    Agirlik = uye.AgirlikKilogram ?? 0,
                    Cinsiyet = uye.Cinsiyet ?? "",
                    FitnessHedefi = uye.FitnessHedefi ?? ""
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Diyet tavsiyesi sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /YapayzekaTavsiye/DiyetTavsiyesi
        // Diyet tavsiyesi isteği gönder
        // =============================================
        /// <summary>
        /// Yapay zeka'ya diyet tavsiyesi isteği gönderir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DiyetTavsiyesi(DiyetTavsiyesiViewModel model)
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

                // Model doğrulama
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Yapay zeka isteğini oluştur
                var tavsiye = new YapayzekaTavsiye
                {
                    UyeId = uye.Id,
                    TavsiyeTipi = "DiyetPlani",
                    GirdiVeri = JsonConvert.SerializeObject(new
                    {
                        boy = model.Boy,
                        agirlik = model.Agirlik,
                        cinsiyet = model.Cinsiyet,
                        hedef = model.FitnessHedefi
                    }),
                    OlusturmaTarihi = DateTime.UtcNow
                };

                // Veritabanına ekle
                _dbContext.YapayzekaTavsiyeler.Add(tavsiye);
                await _dbContext.SaveChangesAsync();

                try
                {
                    // Yapay zeka servisini çağır
                    if (model.Cinsiyet == null || model.FitnessHedefi == null)
                    {
                        return BadRequest("Cinsiyet ve Fitness hedefi gereklidir.");
                    }

                    var cevap = await _yapayzekaSirvisi.DiyetTavsiyesiAl(
                        model.Boy,
                        model.Agirlik,
                        model.Cinsiyet,
                        model.FitnessHedefi
                    );

                    // Tavsiye nesnesi güncelle
                    tavsiye.CiktiVeri = cevap;
                    tavsiye.IslemBasarili = true;
                    tavsiye.CevapTarihi = DateTime.UtcNow;

                    _dbContext.YapayzekaTavsiyeler.Update(tavsiye);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Diyet tavsiyesi başarıyla oluşturuldu. Tavsiye ID: {tavsiye.Id}");

                    TempData["BasariliMesaj"] = "Tavsiye başarıyla oluşturuldu!";
                    return RedirectToAction(nameof(TavsiyeDetaylari), new { id = tavsiye.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Yapay zeka çağrısında hata: {ex.Message}");

                    // Hata bilgisi kaydet
                    tavsiye.IslemBasarili = false;
                    tavsiye.HataMesaji = $"Yapay zeka servisi hatası: {ex.Message}";
                    tavsiye.CevapTarihi = DateTime.UtcNow;

                    _dbContext.YapayzekaTavsiyeler.Update(tavsiye);
                    await _dbContext.SaveChangesAsync();

                    TempData["HataMesaji"] = "Yapay zeka servisinde hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Diyet tavsiyesi oluştururken hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /YapayzekaTavsiye/VucutTipiAnalizi
        // Vücut tipi analiz formu
        // =============================================
        /// <summary>
        /// Vücut tipi analizi almak için formu gösterir
        /// </summary>
        public async Task<IActionResult> VucutTipiAnalizi()
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
                    TempData["HataMesaji"] = "Lütfen önce üye profilinizi oluşturunuz.";
                    return RedirectToAction("OlusturProfilim", "Uyeler");
                }

                // Üyenin mevcut bilgilerini model'e yükle
                var model = new VucutTipiAnaliziViewModel
                {
                    Boy = uye.BouSantimetre ?? 0,
                    Agirlik = uye.AgirlikKilogram ?? 0,
                    Cinsiyet = uye.Cinsiyet ?? ""
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Vücut tipi analizi sayfasında hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // POST: /YapayzekaTavsiye/VucutTipiAnalizi
        // Vücut tipi analiz isteği gönder
        // =============================================
        /// <summary>
        /// Yapay zeka'ya vücut tipi analiz isteği gönderir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VucutTipiAnalizi(VucutTipiAnaliziViewModel model)
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

                // Model doğrulama
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Yapay zeka isteğini oluştur
                var tavsiye = new YapayzekaTavsiye
                {
                    UyeId = uye.Id,
                    TavsiyeTipi = "VucutTipiAnalizi",
                    GirdiVeri = JsonConvert.SerializeObject(new
                    {
                        boy = model.Boy,
                        agirlik = model.Agirlik,
                        cinsiyet = model.Cinsiyet
                    }),
                    OlusturmaTarihi = DateTime.UtcNow
                };

                // Veritabanına ekle
                _dbContext.YapayzekaTavsiyeler.Add(tavsiye);
                await _dbContext.SaveChangesAsync();

                try
                {
                    // Yapay zeka servisini çağır
                    if (model.Cinsiyet == null)
                    {
                        return BadRequest("Cinsiyet gereklidir.");
                    }

                    var cevap = await _yapayzekaSirvisi.VucutTipiAnaliziYap(
                        model.Boy,
                        model.Agirlik,
                        model.Cinsiyet
                    );

                    // Tavsiye nesnesi güncelle
                    tavsiye.CiktiVeri = cevap;
                    tavsiye.IslemBasarili = true;
                    tavsiye.CevapTarihi = DateTime.UtcNow;

                    _dbContext.YapayzekaTavsiyeler.Update(tavsiye);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Vücut tipi analizi başarıyla oluşturuldu. Tavsiye ID: {tavsiye.Id}");

                    TempData["BasariliMesaj"] = "Analiz başarıyla oluşturuldu!";
                    return RedirectToAction(nameof(TavsiyeDetaylari), new { id = tavsiye.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Yapay zeka çağrısında hata: {ex.Message}");

                    // Hata bilgisi kaydet
                    tavsiye.IslemBasarili = false;
                    tavsiye.HataMesaji = $"Yapay zeka servisi hatası: {ex.Message}";
                    tavsiye.CevapTarihi = DateTime.UtcNow;

                    _dbContext.YapayzekaTavsiyeler.Update(tavsiye);
                    await _dbContext.SaveChangesAsync();

                    TempData["HataMesaji"] = "Yapay zeka servisinde hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Vücut tipi analizi oluştururken hata: {ex.Message}");
                return View("Error");
            }
        }

        // =============================================
        // GET: /YapayzekaTavsiye/TavsiyeDetaylari/5
        // Tavsiye detaylarını göster
        // =============================================
        /// <summary>
        /// Yapay zeka tavsiyesinin ayrıntılarını gösterir
        /// </summary>
        public async Task<IActionResult> TavsiyeDetaylari(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var tavsiye = await _dbContext.YapayzekaTavsiyeler
                    .Include(yt => yt.Uye)
                    .FirstOrDefaultAsync(yt => yt.Id == id);

                if (tavsiye == null)
                {
                    return NotFound();
                }

                // Yetki kontrolü (kendi tavsiyeleri veya Admin)
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }
                var uye = await _dbContext.Uyeler.FirstOrDefaultAsync(u => u.KullaniciId == currentUser.Id);

                if (!User.IsInRole("Admin") && (uye == null || uye.Id != tavsiye.UyeId))
                {
                    return Unauthorized();
                }

                return View(tavsiye);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Tavsiye detaylarında hata: {ex.Message}");
                return View("Error");
            }
        }
    }

    // =============================================
    // VIEW MODEL'LER
    // =============================================

    /// <summary>
    /// Egzersiz tavsiyesi form modeli
    /// </summary>
    public class EgzersizTavsiyesiViewModel
    {
        [Required(ErrorMessage = "Boy zorunludur.")]
        [Range(50, 300, ErrorMessage = "Boy 50 ile 300 cm arasında olmalıdır.")]
        public int Boy { get; set; }

        [Required(ErrorMessage = "Ağırlık zorunludur.")]
        [Range(10, 300, ErrorMessage = "Ağırlık 10 ile 300 kg arasında olmalıdır.")]
        public int Agirlik { get; set; }

        [Required(ErrorMessage = "Cinsiyet zorunludur.")]
        public string? Cinsiyet { get; set; }

        [Required(ErrorMessage = "Fitness hedefi zorunludur.")]
        public string? FitnessHedefi { get; set; }
    }

    /// <summary>
    /// Diyet tavsiyesi form modeli
    /// </summary>
    public class DiyetTavsiyesiViewModel
    {
        [Required(ErrorMessage = "Boy zorunludur.")]
        [Range(50, 300, ErrorMessage = "Boy 50 ile 300 cm arasında olmalıdır.")]
        public int Boy { get; set; }

        [Required(ErrorMessage = "Ağırlık zorunludur.")]
        [Range(10, 300, ErrorMessage = "Ağırlık 10 ile 300 kg arasında olmalıdır.")]
        public int Agirlik { get; set; }

        [Required(ErrorMessage = "Cinsiyet zorunludur.")]
        public string? Cinsiyet { get; set; }

        [Required(ErrorMessage = "Fitness hedefi zorunludur.")]
        public string? FitnessHedefi { get; set; }
    }

    /// <summary>
    /// Vücut tipi analiz form modeli
    /// </summary>
    public class VucutTipiAnaliziViewModel
    {
        [Required(ErrorMessage = "Boy zorunludur.")]
        [Range(50, 300, ErrorMessage = "Boy 50 ile 300 cm arasında olmalıdır.")]
        public int Boy { get; set; }

        [Required(ErrorMessage = "Ağırlık zorunludur.")]
        [Range(10, 300, ErrorMessage = "Ağırlık 10 ile 300 kg arasında olmalıdır.")]
        public int Agirlik { get; set; }

        [Required(ErrorMessage = "Cinsiyet zorunludur.")]
        public string? Cinsiyet { get; set; }
    }
}
