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
    /// IRendevuSirvisi - Randevu yönetimi işlemlerinin arayüzü
    /// Randevu oluşturma, silme, güncelleme vb. operasyonlarını tanımlar
    /// </summary>
    public interface IRendevuSirvisi
    {
        /// <summary>
        /// Yeni bir randevu oluşturur
        /// </summary>
        /// <param name="randevu">Oluşturulacak randevu nesnesi</param>
        /// <returns>Oluşturulan randevu</returns>
        Task<Randevu> RandevuOlustur(Randevu randevu);

        /// <summary>
        /// Belirli tarih aralığında çakışan randevuyu kontrol eder
        /// </summary>
        /// <param name="antrenorId">Antrenörün kimliği</param>
        /// <param name="baslamaTarihi">Başlangıç tarihi</param>
        /// <param name="bitisTarihi">Bitiş tarihi</param>
        /// <param name="hariçTutulacakRandevuId">Güncellemede hariç tutulacak randevu kimliği (opsiyonel)</param>
        /// <returns>Çakışan randevu var mı?</returns>
        Task<bool> CakisanRandevuVarMi(int antrenorId, DateTime baslamaTarihi, DateTime bitisTarihi, int? hariçTutulacakRandevuId = null);

        /// <summary>
        /// Antrenörün belirli tarihte müsait olup olmadığını kontrol eder
        /// </summary>
        /// <param name="antrenorId">Antrenörün kimliği</param>
        /// <param name="tarih">Kontrol edilecek tarih ve saat</param>
        /// <returns>Müsait mi?</returns>
        Task<bool> AntrenorMusaitMi(int antrenorId, DateTime tarih);

        /// <summary>
        /// Kullanıcının belirtilen dönemdeki randevularını getirir
        /// </summary>
        /// <param name="uyeId">Üyenin kimliği</param>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Randevular listesi</returns>
        Task<List<Randevu>> UyeRandevulariniGetir(int uyeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Antrenörün belirtilen dönemdeki randevularını getirir
        /// </summary>
        /// <param name="antrenorId">Antrenörün kimliği</param>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Randevular listesi</returns>
        Task<List<Randevu>> AntrenorRandevulariniGetir(int antrenorId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Randevuyu iptal eder
        /// </summary>
        /// <param name="randevuId">İptal edilecek randevunun kimliği</param>
        /// <returns>İptal başarılı mı?</returns>
        Task<bool> RandevuIptalEt(int randevuId);

        /// <summary>
        /// Randevuyu onayla
        /// </summary>
        /// <param name="randevuId">Onaylanacak randevunun kimliği</param>
        /// <returns>Onay başarılı mı?</returns>
        Task<bool> RandevuOnayla(int randevuId);
    }
}
