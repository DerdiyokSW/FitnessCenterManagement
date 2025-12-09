using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Web.Models
{
    /// <summary>
    /// Randevu - Üyeler tarafından antrenörler ile alınan randevuları temsil eden varlık
    /// Randevu: Üye + Antrenör + Hizmet + Tarih/Saat kombinasyonudur
    /// </summary>
    public class Randevu
    {
        // Kimlik bilgisi
        public int Id { get; set; }

        // Foreign Keys
        /// <summary>
        /// Randevuyu alan üyenin kimliği (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Üye seçimi zorunludur.")]
        [ForeignKey("Uye")]
        public int UyeId { get; set; }

        /// <summary>
        /// Randevu verilen antrenörün kimliği (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Antrenör seçimi zorunludur.")]
        [ForeignKey("Antrenor")]
        public int AntrenorId { get; set; }

        /// <summary>
        /// Randevudaki hizmetin kimliği (zorunlu)
        /// Örn: Yoga, Fitness, Pilates vb.
        /// </summary>
        [Required(ErrorMessage = "Hizmet seçimi zorunludur.")]
        [ForeignKey("Hizmet")]
        public int HizmetId { get; set; }

        // Tarih/Saat bilgileri
        /// <summary>
        /// Randevunun başlangıç tarihi ve saati (zorunlu)
        /// Örn: 2025-12-09 14:30
        /// </summary>
        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        public DateTime BaslamaTarihi { get; set; }

        /// <summary>
        /// Randevunun bitiş tarihi ve saati (zorunlu)
        /// Hizmetin süresi ile otomatik hesaplanabilir
        /// </summary>
        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        public DateTime BitisTarihi { get; set; }

        // Ödeme bilgisi
        /// <summary>
        /// Randevunun ücreti (zorunlu, TL cinsinden)
        /// Hizmetin ücretinden alınabilir
        /// </summary>
        [Required(ErrorMessage = "Randevu ücreti zorunludur.")]
        [Range(0, 10000, ErrorMessage = "Randevu ücreti 0 ile 10000 TL arasında olmalıdır.")]
        public decimal Ucret { get; set; }

        // Durum bilgisi
        /// <summary>
        /// Randevunun durumu (Beklemede / Onaylandı / İptal Edildi)
        /// Varsayılan değer: "Beklemede"
        /// </summary>
        [Required(ErrorMessage = "Randevu durumu zorunludur.")]
        [StringLength(20, ErrorMessage = "Durum 20 karakterden fazla olamaz.")]
        public string Durum { get; set; } = "Beklemede";

        /// <summary>
        /// Randevunun oluşturulduğu tarih
        /// </summary>
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        /// <summary>
        /// Bu randevuyu alan üye nesnesi
        /// </summary>
        public Uye? Uye { get; set; }

        /// <summary>
        /// Bu randevuyu veren antrenör nesnesi
        /// </summary>
        public Antrenor? Antrenor { get; set; }

        /// <summary>
        /// Bu randevudaki hizmet nesnesi
        /// </summary>
        public Hizmet? Hizmet { get; set; }
    }
}
