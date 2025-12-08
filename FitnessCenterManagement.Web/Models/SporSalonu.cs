using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Web.Models
{
    /// <summary>
    /// SporSalonu - Spor salonu/fitness center bilgilerini temsil eden varlık
    /// </summary>
    public class SporSalonu
    {
        // Kimlik bilgisi
        public int Id { get; set; }

        /// <summary>
        /// Spor salonunun adı (zorunlu, max 100 karakter)
        /// </summary>
        [Required(ErrorMessage = "Spor salonu adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Spor salonu adı 100 karakterden fazla olamaz.")]
        public string Ad { get; set; }

        /// <summary>
        /// Spor salonunun adresi (zorunlu, max 200 karakter)
        /// </summary>
        [Required(ErrorMessage = "Adres zorunludur.")]
        [StringLength(200, ErrorMessage = "Adres 200 karakterden fazla olamaz.")]
        public string Adres { get; set; }

        /// <summary>
        /// Çalışma saatleri (örn: "09:00-22:00")
        /// </summary>
        [StringLength(100)]
        public string CalismaZamanlari { get; set; }

        /// <summary>
        /// Spor salonunun açıklaması/tanıtımı
        /// </summary>
        [StringLength(500)]
        public string Aciklama { get; set; }

        // Navigation Properties
        /// <summary>
        /// Bu salona ait hizmetler
        /// </summary>
        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();

        /// <summary>
        /// Bu salonda çalışan antrenörler
        /// </summary>
        public ICollection<Antrenor> Antrenorler { get; set; } = new List<Antrenor>();
    }
}
