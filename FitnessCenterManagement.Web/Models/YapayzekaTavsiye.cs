using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Web.Models
{
    /// <summary>
    /// YapayzekaTavsiye - Yapay zeka tarafından verilen egzersiz/diyet tavsiyelerini temsil eden varlık
    /// Üyeler fitness hedeflerine göre kişiselleştirilmiş öneriler alabilirler
    /// </summary>
    public class YapayzekaTavsiye
    {
        // Kimlik bilgisi
        public int Id { get; set; }

        // Foreign Key
        /// <summary>
        /// Tavsiye talep eden üyenin kimliği (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Üye seçimi zorunludur.")]
        [ForeignKey("Uye")]
        public int UyeId { get; set; }

        // Tavsiye türü
        /// <summary>
        /// Tavsiye tipi: "EgzersizPlani" / "DiyetPlani" / "VucutTipiAnalizi" vb. (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Tavsiye tipi zorunludur.")]
        [StringLength(50, ErrorMessage = "Tavsiye tipi 50 karakterden fazla olamaz.")]
        public string TavsiyeTipi { get; set; }

        // Girdi verisi
        /// <summary>
        /// Yapay zekaya gönderilen giriş verileri (JSON formatında)
        /// Örn: {"boy":175,"agirlik":75,"cinsiyet":"Erkek","hedef":"Kas kazanma"}
        /// Bu alan zorunludur
        /// </summary>
        [Required(ErrorMessage = "Giriş verileri zorunludur.")]
        public string GirdiVeri { get; set; }

        // Çıktı verisi
        /// <summary>
        /// Yapay zeka tarafından dönen tavsiye metni
        /// Örn: "Haftalık egzersiz planı, beslenme önerileri vb."
        /// </summary>
        public string CiktiVeri { get; set; }

        // Hata yönetimi
        /// <summary>
        /// İşlem başarılı olmuş mu? (Yapay zeka isteği başarılı/başarısız)
        /// </summary>
        public bool IslemBasarili { get; set; } = false;

        /// <summary>
        /// İşlem sırasında oluşan hata mesajı (varsa)
        /// </summary>
        [StringLength(500)]
        public string HataMesaji { get; set; }

        // Zaman bilgisi
        /// <summary>
        /// Tavsiye talebinin yapılma tarihi ve saati
        /// </summary>
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Yapay zekadan cevap alındığı tarih ve saat
        /// </summary>
        public DateTime? CevapTarihi { get; set; }

        // Navigation Property
        /// <summary>
        /// Tavsiye talep eden üye nesnesi
        /// </summary>
        public Uye Uye { get; set; }
    }
}
