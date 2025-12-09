using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Web.Models
{
    /// <summary>
    /// Antrenor - Spor salonunda çalışan antrenörleri/eğitmenleri temsil eden varlık
    /// </summary>
    public class Antrenor
    {
        // Kimlik bilgisi
        public int Id { get; set; }

        /// <summary>
        /// Antrenörün adı (zorunlu, max 50 karakter)
        /// </summary>
        [Required(ErrorMessage = "Antrenörün adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad 50 karakterden fazla olamaz.")]
        public string? Ad { get; set; }

        /// <summary>
        /// Antrenörün soyadı (zorunlu, max 50 karakter)
        /// </summary>
        [Required(ErrorMessage = "Antrenörün soyadı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyadı 50 karakterden fazla olamaz.")]
        public string? Soyad { get; set; }

        /// <summary>
        /// Antrenörün uzmanlık alanları (virgülle ayrılmış)
        /// Örn: "Fitness, Yoga, Pilates"
        /// </summary>
        [StringLength(200, ErrorMessage = "Uzmanlık alanları 200 karakterden fazla olamaz.")]
        public string? UzmanlıkAlanlari { get; set; }

        /// <summary>
        /// Antrenörün telefon numarası
        /// </summary>
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(20)]
        public string? Telefon { get; set; }

        /// <summary>
        /// Antrenörün e-posta adresi
        /// </summary>
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100)]
        public string? Email { get; set; }

        // Foreign Key - Hangi spor salonunda çalışır
        /// <summary>
        /// Bu antrenörün çalıştığı spor salonu kimliği
        /// </summary>
        [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
        [ForeignKey("SporSalonu")]
        public int SporSalonuId { get; set; }

        // Günlük çalışma saatleri
        /// <summary>
        /// Antrenörün günde başlangıç saati (örn: 09:00)
        /// </summary>
        public TimeSpan? MevcutBaslangicSaati { get; set; }

        /// <summary>
        /// Antrenörün günde bitiş saati (örn: 22:00)
        /// </summary>
        public TimeSpan? MevcutBitisSaati { get; set; }

        // Navigation Properties
        /// <summary>
        /// Bu antrenörün çalıştığı spor salonu nesnesi
        /// </summary>
        public SporSalonu? SporSalonu { get; set; }

        /// <summary>
        /// Bu antrenöre ait randevular
        /// </summary>
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
