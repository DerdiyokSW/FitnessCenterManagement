using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Web.Data;
using FitnessCenterManagement.Web.Models;

namespace FitnessCenterManagement.Web.Services
{
    /// <summary>
    /// RendevuSirvisi - Randevu yönetimi işlemlerinin implementasyonu
    /// Veritabanı ile ilişki kurar, randevu işlem kurallarını uygular
    /// </summary>
    public class RendevuSirvisi : IRendevuSirvisi
    {
        private readonly FitnessCenterDbContext _dbContext;
        private readonly ILogger<RendevuSirvisi> _logger;

        /// <summary>
        /// Constructor - Bağımlılıkları alır
        /// </summary>
        public RendevuSirvisi(FitnessCenterDbContext dbContext, ILogger<RendevuSirvisi> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Yeni bir randevu oluşturur
        /// Öncesinde çakışma kontrolü yapar
        /// </summary>
        public async Task<Randevu> RandevuOlustur(Randevu randevu)
        {
            try
            {
                // ========================================
                // 1. VERİ DOĞRULAMASI
                // ========================================
                if (randevu == null)
                {
                    throw new ArgumentNullException(nameof(randevu), "Randevu nesnesi null olamaz.");
                }

                if (randevu.BaslamaTarihi >= randevu.BitisTarihi)
                {
                    throw new InvalidOperationException("Başlangıç tarihi bitiş tarihinden önce olmalıdır.");
                }

                if (randevu.BaslamaTarihi < DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Geçmiş tarihlere randevu alınamaz.");
                }

                // ========================================
                // 2. ANTRENÖR MÜSAITLIK KONTROLÜ
                // ========================================
                bool cakisma = await CakisanRandevuVarMi(
                    randevu.AntrenorId,
                    randevu.BaslamaTarihi,
                    randevu.BitisTarihi
                );

                if (cakisma)
                {
                    throw new InvalidOperationException(
                        "Antrenör bu tarih aralığında zaten bir randevusu vardır. " +
                        "Lütfen başka bir saati seçiniz."
                    );
                }

                // ========================================
                // 3. ANTRENÖR ÇALIŞMA SAATLERİ KONTROLÜ
                // ========================================
                var antrenor = await _dbContext.Antrenorler
                    .FirstOrDefaultAsync(a => a.Id == randevu.AntrenorId);

                if (antrenor != null && antrenor.MevcutBaslangicSaati.HasValue && antrenor.MevcutBitisSaati.HasValue)
                {
                    var baslangicSaati = randevu.BaslamaTarihi.TimeOfDay;
                    var bitisSaati = randevu.BitisTarihi.TimeOfDay;

                    if (baslangicSaati < antrenor.MevcutBaslangicSaati || bitisSaati > antrenor.MevcutBitisSaati)
                    {
                        throw new InvalidOperationException(
                            $"Antrenörün çalışma saatleri {antrenor.MevcutBaslangicSaati:hh\\:mm} - " +
                            $"{antrenor.MevcutBitisSaati:hh\\:mm} arasındadır."
                        );
                    }
                }

                // ========================================
                // 4. VERİTABANINAEKLE VE KAYDET
                // ========================================
                _dbContext.Randevular.Add(randevu);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Randevu başarıyla oluşturuldu. Randevu ID: {randevu.Id}, " +
                    $"Üye ID: {randevu.UyeId}, Antrenör ID: {randevu.AntrenorId}");

                return randevu;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Randevu oluşturma hatası: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Belirli tarih aralığında çakışan randevuyu kontrol eder
        /// Aynı antrenöre aynı saatte iki randevu olamaz
        /// </summary>
        public async Task<bool> CakisanRandevuVarMi(int antrenorId, DateTime baslamaTarihi, DateTime bitisTarihi, int? hariçTutulacakRandevuId = null)
        {
            try
            {
                var cakisanRandevular = await _dbContext.Randevular
                    .Where(r => r.AntrenorId == antrenorId &&
                                r.Durum != "İptal Edildi" && // İptal edilen randevuları ignore et
                                r.BaslamaTarihi < bitisTarihi &&
                                r.BitisTarihi > baslamaTarihi)
                    .ToListAsync();

                // Güncelleme durumunda, güncellenen randevuyu hariç tut
                if (hariçTutulacakRandevuId.HasValue)
                {
                    cakisanRandevular = cakisanRandevular
                        .Where(r => r.Id != hariçTutulacakRandevuId.Value)
                        .ToList();
                }

                bool varMi = cakisanRandevular.Any();

                if (varMi)
                {
                    _logger.LogWarning($"Antrenör {antrenorId} için {baslamaTarihi} - {bitisTarihi} arasında çakışan randevu bulundu.");
                }

                return varMi;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Çakışma kontrolü hatası: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Antrenörün belirli tarihte müsait olup olmadığını kontrol eder
        /// Çalışma saatleri ve mevcut randevular kontrol edilir
        /// </summary>
        public async Task<bool> AntrenorMusaitMi(int antrenorId, DateTime tarih)
        {
            try
            {
                var antrenor = await _dbContext.Antrenorler
                    .FirstOrDefaultAsync(a => a.Id == antrenorId);

                if (antrenor == null)
                {
                    return false;
                }

                // ========================================
                // 1. ÇALIŞMA SAATİ KONTROLÜ
                // ========================================
                if (antrenor.MevcutBaslangicSaati.HasValue && antrenor.MevcutBitisSaati.HasValue)
                {
                    var saatOfDay = tarih.TimeOfDay;
                    if (saatOfDay < antrenor.MevcutBaslangicSaati || saatOfDay > antrenor.MevcutBitisSaati)
                    {
                        return false;
                    }
                }

                // ========================================
                // 2. MEVCUTRANDEVULARkONTROLÜ
                // ========================================
                var mevcutRandevular = await _dbContext.Randevular
                    .Where(r => r.AntrenorId == antrenorId &&
                                r.Durum != "İptal Edildi" &&
                                r.BaslamaTarihi.Date == tarih.Date)
                    .ToListAsync();

                return !mevcutRandevular.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Antrenör müsaitlik kontrolü hatası: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Kullanıcının belirtilen dönemdeki randevularını getirir
        /// </summary>
        public async Task<List<Randevu>> UyeRandevulariniGetir(int uyeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _dbContext.Randevular
                    .Where(r => r.UyeId == uyeId)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet)
                    .AsQueryable();

                // Tarih filtresi varsa uygula
                if (startDate.HasValue)
                {
                    query = query.Where(r => r.BaslamaTarihi >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(r => r.BitisTarihi <= endDate.Value);
                }

                query = query.OrderBy(r => r.BaslamaTarihi);

                var randevular = await query.ToListAsync();

                _logger.LogInformation($"Üye {uyeId} için {randevular.Count} randevu getirildi.");

                return randevular;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Üye randevuları getirme hatası: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Antrenörün belirtilen dönemdeki randevularını getirir
        /// Admin panelinde kullanılır
        /// </summary>
        public async Task<List<Randevu>> AntrenorRandevulariniGetir(int antrenorId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _dbContext.Randevular
                    .Where(r => r.AntrenorId == antrenorId)
                    .Include(r => r.Uye)
                    .Include(r => r.Hizmet)
                    .AsQueryable();

                // Tarih filtresi varsa uygula
                if (startDate.HasValue)
                {
                    query = query.Where(r => r.BaslamaTarihi >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(r => r.BitisTarihi <= endDate.Value);
                }

                query = query.OrderBy(r => r.BaslamaTarihi);

                var randevular = await query.ToListAsync();

                _logger.LogInformation($"Antrenör {antrenorId} için {randevular.Count} randevu getirildi.");

                return randevular;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Antrenör randevuları getirme hatası: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Randevuyu iptal eder
        /// </summary>
        public async Task<bool> RandevuIptalEt(int randevuId)
        {
            try
            {
                var randevu = await _dbContext.Randevular.FindAsync(randevuId);

                if (randevu == null)
                {
                    _logger.LogWarning($"Randevu {randevuId} bulunamadı.");
                    return false;
                }

                // İptal edilirse durumu değiştir
                randevu.Durum = "İptal Edildi";
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Randevu {randevuId} iptal edildi.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Randevu iptal hatası: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Randevuyu onayla
        /// </summary>
        public async Task<bool> RandevuOnayla(int randevuId)
        {
            try
            {
                var randevu = await _dbContext.Randevular.FindAsync(randevuId);

                if (randevu == null)
                {
                    _logger.LogWarning($"Randevu {randevuId} bulunamadı.");
                    return false;
                }

                randevu.Durum = "Onaylandı";
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Randevu {randevuId} onaylandı.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Randevu onay hatası: {ex.Message}");
                throw;
            }
        }
    }
}
