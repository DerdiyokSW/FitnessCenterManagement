using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Web.Models
{
    /// <summary>
    /// Hizmet - Spor salonunun sunduğu hizmetleri temsil eden varlık
    /// Örn: Fitness, Yoga, Pilates, Kişisel Antrenörlük vb.
    /// </summary>
    public class Hizmet
    {
        // Kimlik bilgisi
        public int Id { get; set; }

        /// <summary>
        /// Hizmetin adı (zorunlu, max 100 karakter)
        /// Örn: "Yoga", "Fitness", "Pilates"
        /// </summary>
        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Hizmet adı 100 karakterden fazla olamaz.")]
        public string? Ad { get; set; }

        /// <summary>
        /// Hizmetin süresi (dakika cinsinden)
        /// Örn: 60 dakika = 1 saat
        /// </summary>
        [Required(ErrorMessage = "Hizmet süresi zorunludur.")]
        [Range(10, 300, ErrorMessage = "Hizmet süresi 10 ile 300 dakika arasında olmalıdır.")]
        public int SureDakika { get; set; }

        /// <summary>
        /// Hizmetin ücreti (TL)
        /// </summary>
        [Required(ErrorMessage = "Hizmet ücreti zorunludur.")]
        [Range(0, 10000, ErrorMessage = "Hizmet ücreti 0 ile 10000 TL arasında olmalıdır.")]
        public decimal Ucret { get; set; }

        /// <summary>
        /// Hizmetin detaylı açıklaması
        /// </summary>
        [StringLength(500, ErrorMessage = "Açıklama 500 karakterden fazla olamaz.")]
        public string? Aciklama { get; set; }

        // Foreign Key - Hangi spor salonuna ait
        /// <summary>
        /// Bu hizmetin ait olduğu spor salonu kimliği
        /// </summary>
        [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
        [ForeignKey("SporSalonu")]
        public int SporSalonuId { get; set; }

        // Navigation Property
        /// <summary>
        /// Bu hizmetin ait olduğu spor salonu nesnesi
        /// </summary>
        public SporSalonu? SporSalonu { get; set; }

        /// <summary>
        /// Bu hizmet için alınan randevular
        /// </summary>
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
