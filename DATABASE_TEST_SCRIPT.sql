-- ============================================================================
-- Fitness Center Management - Database Testing and Verification Script
-- Kullanım: SSMS'de aç, tüm script'i seç, F5'e bas
-- ============================================================================

-- ADIM 1: Veritabanını kontrol et
USE [Fitness_Center_DB]
GO

PRINT '========== ADIM 1: Veritabanı Bağlantısı =========='
PRINT 'Veritabanı: ' + DB_NAME()
PRINT 'Tarih: ' + CAST(GETDATE() AS VARCHAR(30))
GO

-- ADIM 2: Tabloların varlığını kontrol et
PRINT ''
PRINT '========== ADIM 2: Tabloların Varlığı =========='

-- AspNetUsers tablosunun varlığını kontrol et
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
    PRINT '✓ AspNetUsers tablosu mevcut'
ELSE
    PRINT '✗ AspNetUsers tablosu YOK - Migration gerekli!'
GO

-- Uyeler tablosunun varlığını kontrol et
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Uyeler')
    PRINT '✓ Uyeler tablosu mevcut'
ELSE
    PRINT '✗ Uyeler tablosu YOK - Migration gerekli!'
GO

-- ADIM 3: Kayıt sayılarını göster
PRINT ''
PRINT '========== ADIM 3: Mevcut Veri =========='

DECLARE @uyeSayisi INT = (SELECT COUNT(*) FROM Uyeler)
DECLARE @kullaniciSayisi INT = (SELECT COUNT(*) FROM AspNetUsers)

PRINT 'Toplam Üye Sayısı: ' + CAST(@uyeSayisi AS VARCHAR(10))
PRINT 'Toplam Kullanıcı Sayısı (AspNetUsers): ' + CAST(@kullaniciSayisi AS VARCHAR(10))
GO

-- ADIM 4: Tüm üyeleri listele
PRINT ''
PRINT '========== ADIM 4: Kayıtlı Üyeler =========='

SELECT 
    u.Id AS [Üye ID],
    u.Ad AS [Adı],
    u.Soyad AS [Soyadı],
    u.BouSantimetre AS [Boy (cm)],
    u.AgirlikKilogram AS [Ağırlık (kg)],
    u.Cinsiyet,
    u.FitnessHedefi AS [Fitness Hedefi],
    u.KullaniciId AS [Kullanıcı ID],
    CASE WHEN u.KullaniciId IS NULL THEN '✗ HATA: ID Boş!' ELSE '✓ OK' END AS [Durum]
FROM Uyeler u
ORDER BY u.Id DESC
GO

-- ADIM 5: Tüm kullanıcıları listele
PRINT ''
PRINT '========== ADIM 5: Kayıtlı Kullanıcılar (AspNetUsers) =========='

SELECT 
    Id AS [Kullanıcı ID],
    UserName AS [Kullanıcı Adı],
    Email AS [Email],
    CASE WHEN LEN(PasswordHash) > 0 THEN '✓ Şifre Ayarlanmış' ELSE '✗ Şifre Yok' END AS [Durum]
FROM AspNetUsers
ORDER BY Id DESC
GO

-- ADIM 6: Kullanıcı-Üye İlişkisini Kontrol Et
PRINT ''
PRINT '========== ADIM 6: Kullanıcı-Üye İlişkisi =========='

SELECT 
    u.Id AS [Üye ID],
    u.Ad + ' ' + u.Soyad AS [Üye Adı],
    u.KullaniciId AS [Kullanıcı ID],
    au.UserName AS [Kullanıcı Adı],
    au.Email,
    CASE 
        WHEN au.Id IS NULL THEN '✗ HATA: AspNetUsers''da bu ID yok!'
        WHEN u.KullaniciId IS NULL THEN '✗ HATA: KullaniciId boş!'
        ELSE '✓ Bağlantı OK'
    END AS [İlişki Durumu]
FROM Uyeler u
FULL OUTER JOIN AspNetUsers au ON u.KullaniciId = au.Id
ORDER BY u.Id DESC
GO

-- ADIM 7: Özel Kontroller
PRINT ''
PRINT '========== ADIM 7: Veri Doğrulama =========='

-- Boş KullaniciId kontrol et
DECLARE @bosKullaniciId INT = (SELECT COUNT(*) FROM Uyeler WHERE KullaniciId IS NULL OR KullaniciId = '')
IF @bosKullaniciId > 0
    PRINT '⚠ UYARI: ' + CAST(@bosKullaniciId AS VARCHAR(10)) + ' kaydın KullaniciId boş!'
ELSE
    PRINT '✓ Tüm kayıtlarda KullaniciId dolu'

-- Yinelenen KullaniciId kontrol et
DECLARE @yinelenKullaniciId INT = (SELECT COUNT(*) FROM (
    SELECT KullaniciId, COUNT(*) as cnt 
    FROM Uyeler 
    WHERE KullaniciId IS NOT NULL 
    GROUP BY KullaniciId 
    HAVING COUNT(*) > 1
) AS T)
IF @yinelenKullaniciId > 0
    PRINT '⚠ UYARI: ' + CAST(@yinelenKullaniciId AS VARCHAR(10)) + ' kullanıcı için birden fazla profil var!'
ELSE
    PRINT '✓ Her kullanıcının sadece bir profili var'

-- Eksik Alan Kontrol Et
DECLARE @eksikAd INT = (SELECT COUNT(*) FROM Uyeler WHERE Ad IS NULL OR Ad = '')
DECLARE @eksikSoyad INT = (SELECT COUNT(*) FROM Uyeler WHERE Soyad IS NULL OR Soyad = '')
IF @eksikAd > 0 OR @eksikSoyad > 0
    PRINT '⚠ UYARI: Bazı kayıtlarda Ad veya Soyad eksik!'
ELSE
    PRINT '✓ Tüm kayıtlarda Ad ve Soyad dolu'
GO

-- ADIM 8: Tablo Yapısını Göster
PRINT ''
PRINT '========== ADIM 8: Uyeler Tablosu Yapısı =========='

EXEC sp_help 'Uyeler'
GO

-- ADIM 9: Son 5 Kaydı Göster
PRINT ''
PRINT '========== ADIM 9: Son 5 Oluşturulan Üye =========='

SELECT TOP 5
    u.Id,
    u.Ad,
    u.Soyad,
    u.BouSantimetre,
    u.AgirlikKilogram,
    u.Cinsiyet,
    u.FitnessHedefi,
    u.KullaniciId,
    au.Email AS KullaniciEmail
FROM Uyeler u
LEFT JOIN AspNetUsers au ON u.KullaniciId = au.Id
ORDER BY u.Id DESC
GO

-- ADIM 10: Özet Rapor
PRINT ''
PRINT '========== ADIM 10: ÖZET RAPOR =========='
PRINT 'Kontrol Tarihi: ' + CAST(GETDATE() AS VARCHAR(30))

DECLARE @totalUsers INT = (SELECT COUNT(*) FROM AspNetUsers)
DECLARE @totalMembers INT = (SELECT COUNT(*) FROM Uyeler)
DECLARE @linkedProfiles INT = (SELECT COUNT(*) FROM Uyeler WHERE KullaniciId IS NOT NULL AND KullaniciId != '')
DECLARE @unlinkedProfiles INT = @totalMembers - @linkedProfiles

PRINT ''
PRINT 'ÖZET:'
PRINT '  Toplam Sistem Kullanıcısı (AspNetUsers): ' + CAST(@totalUsers AS VARCHAR(10))
PRINT '  Toplam Üye Profili (Uyeler): ' + CAST(@totalMembers AS VARCHAR(10))
PRINT '  Bağlantılı Profiller: ' + CAST(@linkedProfiles AS VARCHAR(10))
PRINT '  Bağlantısız Profiller: ' + CAST(@unlinkedProfiles AS VARCHAR(10))

IF @unlinkedProfiles > 0
    PRINT ''
    PRINT '⚠ UYARI: ' + CAST(@unlinkedProfiles AS VARCHAR(10)) + ' profilin KullaniciId''si boş!'
    PRINT 'Bu profiller AspNetUsers ile bağlantılı değil!'
    PRINT 'FIX: Aşağıdaki SQL'i çalıştır:'
    PRINT '     UPDATE Uyeler SET KullaniciId = [VALID USER ID] WHERE KullaniciId IS NULL'
ELSE
    PRINT ''
    PRINT '✓ VERİTABANI SAĞLAM! Tüm profiller düzgün şekilde bağlantılı.'

GO

-- ADIM 11: Profil Detaylarını Bul (Örnek)
PRINT ''
PRINT '========== ADIM 11: Profil Arama Örneği =========='
PRINT 'AÇIKLAMA: Aşağıdaki sorguyu çalıştırarak belirli bir kullanıcının profilini bulabilirsiniz'
PRINT ''
PRINT '-- Örnek: test@example.com kullanıcısının profilini bul'
PRINT '-- Kullanıcı e-postasını değiştir:'
PRINT ''

SELECT TOP 1
    u.Id AS UyeId,
    u.Ad,
    u.Soyad,
    u.BouSantimetre,
    u.AgirlikKilogram,
    u.Cinsiyet,
    u.FitnessHedefi,
    au.Email,
    au.UserName
FROM Uyeler u
INNER JOIN AspNetUsers au ON u.KullaniciId = au.Id
-- WHERE au.Email = 'test@example.com'  -- E-postayı değiştir
ORDER BY u.Id DESC
GO

PRINT ''
PRINT '========== SCRIPT TAMAMLANDI =========='
