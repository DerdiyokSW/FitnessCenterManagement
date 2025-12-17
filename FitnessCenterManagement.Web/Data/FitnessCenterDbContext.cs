using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using FitnessCenterManagement.Web.Models;

namespace FitnessCenterManagement.Web.Data
{
    /// <summary>
    /// FitnessCenterDbContext - Tüm veritabanı işlemlerini yönetmek için kullanılan DbContext sınıfı
    /// Entity Framework Core ile veritabanı tablolarına erişimi sağlar
    /// </summary>
    public class FitnessCenterDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        // Constructor
        /// <summary>
        /// DbContext'i başlatır ve options'ı geçer
        /// </summary>
        public FitnessCenterDbContext(DbContextOptions<FitnessCenterDbContext> options)
            : base(options)
        {
        }

        // DbSet'ler - Veritabanı tabloları
        /// <summary>
        /// Spor Salonu tablosu
        /// </summary>
        public DbSet<SporSalonu> SporSalonlari { get; set; }

        /// <summary>
        /// Hizmet tablosu (Yoga, Fitness, Pilates vb.)
        /// </summary>
        public DbSet<Hizmet> Hizmetler { get; set; }

        /// <summary>
        /// Antrenör tablosu
        /// </summary>
        public DbSet<Antrenor> Antrenorler { get; set; }

        /// <summary>
        /// Üye tablosu
        /// </summary>
        public DbSet<Uye> Uyeler { get; set; }

        /// <summary>
        /// Randevu tablosu
        /// </summary>
        public DbSet<Randevu> Randevular { get; set; }

        /// <summary>
        /// Yapay Zeka Tavsiye tablosu
        /// </summary>
        public DbSet<YapayzekaTavsiye> YapayzekaTavsiyeler { get; set; }

        // Model Yapılandırması
        /// <summary>
        /// Entity'ler arasındaki ilişkileri, veri doğrulamalarını ve kısıtlamaları tanımlar
        /// Migration sırasında veritabanı şeması oluşturulur
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // 1. SporSalonu İlişkileri
            // ============================================
            
            // SporSalonu -> Hizmet (One-to-Many)
            modelBuilder.Entity<SporSalonu>()
                .HasMany(s => s.Hizmetler)
                .WithOne(h => h.SporSalonu)
                .HasForeignKey(h => h.SporSalonuId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Hizmet_SporSalonu");

            // SporSalonu -> Antrenör (One-to-Many)
            modelBuilder.Entity<SporSalonu>()
                .HasMany(s => s.Antrenorler)
                .WithOne(a => a.SporSalonu)
                .HasForeignKey(a => a.SporSalonuId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Antrenor_SporSalonu");

            // ============================================
            // 2. Hizmet İlişkileri
            // ============================================

            // Hizmet -> Randevu (One-to-Many)
            modelBuilder.Entity<Hizmet>()
                .HasMany(h => h.Randevular)
                .WithOne(r => r.Hizmet)
                .HasForeignKey(r => r.HizmetId)
                .OnDelete(DeleteBehavior.Restrict) // Hizmeti silen randevuyu da silmez
                .HasConstraintName("FK_Randevu_Hizmet");

            // ============================================
            // 3. Antrenör İlişkileri
            // ============================================

            // Antrenör -> Randevu (One-to-Many)
            modelBuilder.Entity<Antrenor>()
                .HasMany(a => a.Randevular)
                .WithOne(r => r.Antrenor)
                .HasForeignKey(r => r.AntrenorId)
                .OnDelete(DeleteBehavior.Restrict) // Antrenörü silen randevuyu da silmez
                .HasConstraintName("FK_Randevu_Antrenor");

            // ============================================
            // 4. Üye İlişkileri
            // ============================================

            // Üye -> Randevu (One-to-Many)
            modelBuilder.Entity<Uye>()
                .HasMany(u => u.Randevular)
                .WithOne(r => r.Uye)
                .HasForeignKey(r => r.UyeId)
                .OnDelete(DeleteBehavior.Cascade) // Üyeyi silen tüm randevularını siler
                .HasConstraintName("FK_Randevu_Uye");

            // Üye -> Yapay Zeka Tavsiyesi (One-to-Many)
            modelBuilder.Entity<Uye>()
                .HasMany(u => u.YapayzekaTavsiyeler)
                .WithOne(yt => yt.Uye)
                .HasForeignKey(yt => yt.UyeId)
                .OnDelete(DeleteBehavior.Cascade) // Üyeyi silen tüm tavsiyeleri siler
                .HasConstraintName("FK_YapayzekaTavsiye_Uye");

            // ============================================
            // 5. SporSalonu Tablosu Yapılandırması
            // ============================================

            modelBuilder.Entity<SporSalonu>()
                .Property(s => s.Ad)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<SporSalonu>()
                .Property(s => s.Adres)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<SporSalonu>()
                .Property(s => s.CalismaZamanlari)
                .HasMaxLength(100);

            modelBuilder.Entity<SporSalonu>()
                .Property(s => s.Aciklama)
                .HasMaxLength(500);

            // ============================================
            // 6. Hizmet Tablosu Yapılandırması
            // ============================================

            modelBuilder.Entity<Hizmet>()
                .Property(h => h.Ad)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Hizmet>()
                .Property(h => h.SureDakika)
                .IsRequired();

            modelBuilder.Entity<Hizmet>()
                .Property(h => h.Ucret)
                .HasPrecision(18, 2); // Para için: 18 haneli, 2 ondalak

            modelBuilder.Entity<Hizmet>()
                .Property(h => h.Aciklama)
                .HasMaxLength(500);

            // ============================================
            // 7. Antrenör Tablosu Yapılandırması
            // ============================================

            modelBuilder.Entity<Antrenor>()
                .Property(a => a.Ad)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Antrenor>()
                .Property(a => a.Soyad)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Antrenor>()
                .Property(a => a.UzmanlıkAlanlari)
                .HasMaxLength(200);

            modelBuilder.Entity<Antrenor>()
                .Property(a => a.Telefon)
                .HasMaxLength(20);

            modelBuilder.Entity<Antrenor>()
                .Property(a => a.Email)
                .HasMaxLength(100);

            // ============================================
            // 8. Üye Tablosu Yapılandırması
            // ============================================

            modelBuilder.Entity<Uye>()
                .Property(u => u.KullaniciId)
                .IsRequired()
                .HasMaxLength(450);

            modelBuilder.Entity<Uye>()
                .Property(u => u.Ad)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Uye>()
                .Property(u => u.Soyad)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Uye>()
                .Property(u => u.Cinsiyet)
                .HasMaxLength(10);

            modelBuilder.Entity<Uye>()
                .Property(u => u.FitnessHedefi)
                .HasMaxLength(200);

            // ============================================
            // 9. Randevu Tablosu Yapılandırması
            // ============================================

            modelBuilder.Entity<Randevu>()
                .Property(r => r.Ucret)
                .HasPrecision(18, 2); // Para için: 18 haneli, 2 ondalak

            modelBuilder.Entity<Randevu>()
                .Property(r => r.Durum)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Beklemede");

            modelBuilder.Entity<Randevu>()
                .Property(r => r.OlusturmaTarihi)
                .HasDefaultValueSql("GETUTCDATE()"); // SQL Server varsayılan değer

            // ============================================
            // 10. Yapay Zeka Tavsiyesi Tablosu Yapılandırması
            // ============================================

            modelBuilder.Entity<YapayzekaTavsiye>()
                .Property(yt => yt.TavsiyeTipi)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<YapayzekaTavsiye>()
                .Property(yt => yt.GirdiVeri)
                .IsRequired();

            modelBuilder.Entity<YapayzekaTavsiye>()
                .Property(yt => yt.CiktiVeri)
                .HasMaxLength(5000); // Yapay zeka cevapları uzun olabilir

            modelBuilder.Entity<YapayzekaTavsiye>()
                .Property(yt => yt.HataMesaji)
                .HasMaxLength(500);

            modelBuilder.Entity<YapayzekaTavsiye>()
                .Property(yt => yt.OlusturmaTarihi)
                .HasDefaultValueSql("GETUTCDATE()"); // SQL Server varsayılan değer

            // ============================================
            // 11. İndeksler (Sorgu Performansı için)
            // ============================================

            // Üye tarafından randevuları hızlı bulmak için
            modelBuilder.Entity<Randevu>()
                .HasIndex(r => r.UyeId)
                .HasDatabaseName("IX_Randevu_UyeId");

            // Antrenör tarafından randevuları hızlı bulmak için
            modelBuilder.Entity<Randevu>()
                .HasIndex(r => r.AntrenorId)
                .HasDatabaseName("IX_Randevu_AntrenorId");

            // Hizmet tarafından randevuları hızlı bulmak için
            modelBuilder.Entity<Randevu>()
                .HasIndex(r => r.HizmetId)
                .HasDatabaseName("IX_Randevu_HizmetId");

            // Tarih aralığında randevuları hızlı bulmak için
            modelBuilder.Entity<Randevu>()
                .HasIndex(r => new { r.BaslamaTarihi, r.BitisTarihi })
                .HasDatabaseName("IX_Randevu_TarihAraligi");

            // Kullanıcı kimliğine göre üyeleri hızlı bulmak için
            modelBuilder.Entity<Uye>()
                .HasIndex(u => u.KullaniciId)
                .HasDatabaseName("IX_Uye_KullaniciId")
                .IsUnique(); // Her kullanıcının sadece bir üye profili olmalı
        }
    }
}
