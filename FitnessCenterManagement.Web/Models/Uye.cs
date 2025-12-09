using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Web.Models
{
    /// <summary>
    /// Uye - Spor salonunun üyelerini/müşterilerini temsil eden varlık
    /// ASP.NET Core Identity tarafındaki IdentityUser ile ilişkilidir
    /// </summary>
    public class Uye
    {
        // Kimlik bilgisi
        public int Id { get; set; }

        /// <summary>
        /// ASP.NET Core Identity'deki User tablosunun kimliği
        /// Bu üye hangi kullanıcı hesabına ait olduğunu gösterir
        /// </summary>
        [Required(ErrorMessage = "Kullanıcı kimliği zorunludur.")]
        [StringLength(450)] // Identity default değer
        public string? KullaniciId { get; set; }

        /// <summary>
        /// Üyenin adı (zorunlu, max 50 karakter)
        /// </summary>
        [Required(ErrorMessage = "Adınız zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad 50 karakterden fazla olamaz.")]
        public string? Ad { get; set; }

        /// <summary>
        /// Üyenin soyadı (zorunlu, max 50 karakter)
        /// </summary>
        [Required(ErrorMessage = "Soyadınız zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyadı 50 karakterden fazla olamaz.")]
        public string? Soyad { get; set; }

        /// <summary>
        /// Üyenin doğum tarihi (opsiyonel)
        /// </summary>
        public DateTime? DogumTarihi { get; set; }

        /// <summary>
        /// Üyenin boyu (santimetre cinsinden, opsiyonel)
        /// Örn: 175 cm
        /// </summary>
        [Range(50, 300, ErrorMessage = "Boy 50 ile 300 cm arasında olmalıdır.")]
        public int? BouSantimetre { get; set; }

        /// <summary>
        /// Üyenin ağırlığı (kilogram cinsinden, opsiyonel)
        /// Örn: 75 kg
        /// </summary>
        [Range(10, 300, ErrorMessage = "Ağırlık 10 ile 300 kg arasında olmalıdır.")]
        public int? AgirlikKilogram { get; set; }

        /// <summary>
        /// Üyenin cinsiyeti (Erkek / Kadın / Diğer)
        /// </summary>
        [StringLength(10)]
        public string? Cinsiyet { get; set; }

        /// <summary>
        /// Üyenin fitness hedefi (kilo verme, kas kazanma, sağlıklı yaşam vb.)
        /// </summary>
        [StringLength(200, ErrorMessage = "Hedef 200 karakterden fazla olamaz.")]
        public string? FitnessHedefi { get; set; }

        // Navigation Properties
        /// <summary>
        /// Bu üyenin tüm randevuları
        /// </summary>
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        /// <summary>
        /// Bu üyenin yapay zeka tarafından aldığı öneriler
        /// </summary>
        public ICollection<YapayzekaTavsiye> YapayzekaTavsiyeler { get; set; } = new List<YapayzekaTavsiye>();
    }
}
