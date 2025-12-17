# SQL Veritabanı Yönetimi Rehberi (Turkish/English)

## 📋 İçindekiler
1. Veritabanına Erişim Yöntemleri
2. Manuel SQL Sorguları
3. Veri Aktarımı Yöntemleri
4. Veritabanı Yedekleme
5. Sorun Giderme

---

## 1️⃣ VERİTABANINA ERIŞIM YÖNTEMLERİ

### Yöntem A: SQL Server Management Studio (SSMS) - ÖNERİLEN
**Adım 1:** SQL Server Management Studio'yu aç
**Adım 2:** "Connect to Server" penceresinde şunu gir:
```
Server name: (localdb)\mssqllocaldb
Authentication: Windows Authentication
```
**Adım 3:** "Connect" butonuna tıkla

**Adım 4:** Sol tarafta "Databases" → "Fitness_Center_DB" (veya appsettings.json'da belirtilen isim) → sağ tıkla

### Yöntem B: Visual Studio - Hızlı Erişim
```
View → SQL Server Object Explorer
→ SQL Server
  → (localdb)\mssqllocaldb
    → Databases
      → Fitness_Center_DB
        → Tables
```

### Yöntem C: Package Manager Console (Visual Studio)
```powershell
# Veritabanına bağlan ve sorgula
Get-DbContext

# Entity Framework üzerinden veri sor
Select-DbSet -DbContext FitnessCenterDbContext
```

---

## 2️⃣ TEMEL VERİTABANI SORGUSU

### Adım A: SSMS'de Yeni Query Aç
1. SSMS menüsünde: `File → New → Query with Current Connection`
2. Veya: `Databases → Fitness_Center_DB` üzerinde sağ tıkla → `New Query`

### Adım B: Örnek Sorgular

#### 📍 TÜM KAYITLI ÜYELERİ GÖSTER
```sql
USE [Fitness_Center_DB]

SELECT * FROM Uyeler;
```

**Sonuç beklentisi:**
```
Id | Ad     | Soyad    | KullaniciId | BouSantimetre | AgirlikKilogram | Cinsiyet | FitnessHedefi
1  | Test   | User     | abc-123-def | 180           | 75              | Erkek    | Kas kazanma
2  | Ayşe   | Yılmaz   | xyz-456-ghi | 165           | 60              | Kadın    | Kilo kaybı
```

#### 📍 BELİRLİ BİR ÜYENİN PROFİLİNİ KONTROL ET
```sql
USE [Fitness_Center_DB]

-- EmailAddress'ini bil (örn: test@example.com)
SELECT 
    u.Id,
    u.Ad,
    u.Soyad,
    u.KullaniciId,
    u.BouSantimetre,
    u.AgirlikKilogram,
    u.Cinsiyet,
    u.FitnessHedefi,
    au.Email AS KullaniciEmail
FROM Uyeler u
INNER JOIN AspNetUsers au ON u.KullaniciId = au.Id
WHERE au.Email = 'test@example.com';
```

#### 📍 KAYITLI TÜMLÜĞÜ KONTROL ET (Foreign Key)
```sql
USE [Fitness_Center_DB]

SELECT 
    u.Id AS UyeId,
    u.Ad,
    u.Soyad,
    u.KullaniciId,
    au.Email,
    au.UserName,
    CASE 
        WHEN au.Id IS NULL THEN 'HATA: Kullanıcı sistemde yok!'
        ELSE 'OK'
    END AS Durum
FROM Uyeler u
LEFT JOIN AspNetUsers au ON u.KullaniciId = au.Id;
```

#### 📍 ÖNERİ KAYITLARINI (AI Recommendations) GÖSTER
```sql
USE [Fitness_Center_DB]

SELECT 
    y.Id,
    y.TavsiyeTipi,
    y.GirdiVeri,
    y.CiktiVeri,
    y.IslemBasarili,
    y.OlusturulduTarihi,
    u.Ad + ' ' + u.Soyad AS UyeAdi
FROM YapayzekaTavsiyeleri y
INNER JOIN Uyeler u ON y.UyeId = u.Id
ORDER BY y.OlusturulduTarihi DESC;
```

#### 📍 RANDEVULAR LİSTESİ
```sql
USE [Fitness_Center_DB]

SELECT 
    r.Id,
    r.RandevuTarihi,
    r.RandevuSaati,
    u.Ad + ' ' + u.Soyad AS UyeAdi,
    a.Ad + ' ' + a.Soyad AS AntrenorAdi,
    h.Ad AS HizmetAdi,
    r.Durum
FROM Randevular r
INNER JOIN Uyeler u ON r.UyeId = u.Id
INNER JOIN Antrenorler a ON r.AntrenorId = a.Id
INNER JOIN Hizmetler h ON r.HizmetId = h.Id
ORDER BY r.RandevuTarihi DESC;
```

---

## 3️⃣ VERİ AKTARIMI (Data Transfer/Seeding)

### Yöntem A: Entity Framework Code Seeding (OTOMATİK)

**Dosya: `Data/FitnessCenterDbContext.cs` güncelleyin:**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Örnek veriler ekle (Migration oluşturulacak)
    modelBuilder.Entity<SporSalonu>().HasData(
        new SporSalonu { 
            Id = 1, 
            Ad = "Merkez Spor Salonu", 
            Sehir = "İstanbul",
            BakiBilgisi = "Göztepe'de konumlanmış modern spor salonu"
        }
    );

    modelBuilder.Entity<Hizmet>().HasData(
        new Hizmet { Id = 1, Ad = "Yoga", Aciklama = "Esneklik ve meditasyon", SporSalonuId = 1 },
        new Hizmet { Id = 2, Ad = "Pilates", Aciklama = "Çekirdek kuvveti antrenmanı", SporSalonuId = 1 },
        new Hizmet { Id = 3, Ad = "Yüzme", Aciklama = "Kardiyovasküler fitness", SporSalonuId = 1 }
    );
}
```

**Sonra Migration oluştur:**
```powershell
Add-Migration AddSeedData
Update-Database
```

### Yöntem B: CSV Dosyasından Veri İçeri Aktar

**CSV Dosyası: `seed_uyeler.csv`**
```csv
Ad,Soyad,BouSantimetre,AgirlikKilogram,Cinsiyet,FitnessHedefi,KullaniciId
Ahmet,Yılmaz,180,75,Erkek,Kas Kazanma,user-id-1
Ayşe,Demir,165,60,Kadın,Kilo Kaybı,user-id-2
Mehmet,Kaya,175,85,Erkek,Genel Fitness,user-id-3
```

**C# Kodu - Yeni Controller Action:**
```csharp
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> CSVdenVeriAktar(IFormFile csvDosyasi)
{
    if (csvDosyasi == null || csvDosyasi.Length == 0)
    {
        ModelState.AddModelError("", "Lütfen bir CSV dosyası seçin");
        return View();
    }

    var csv = new CsvReader(new StreamReader(csvDosyasi.OpenReadStream()), CultureInfo.InvariantCulture);
    csv.Read();
    csv.ReadHeader();

    while (csv.Read())
    {
        var uye = new Uye
        {
            Ad = csv.GetField("Ad"),
            Soyad = csv.GetField("Soyad"),
            BouSantimetre = int.Parse(csv.GetField("BouSantimetre")),
            AgirlikKilogram = int.Parse(csv.GetField("AgirlikKilogram")),
            Cinsiyet = csv.GetField("Cinsiyet"),
            FitnessHedefi = csv.GetField("FitnessHedefi"),
            KullaniciId = csv.GetField("KullaniciId")
        };

        _dbContext.Uyeler.Add(uye);
    }

    await _dbContext.SaveChangesAsync();
    TempData["BasariliMesaj"] = "Veriler başarıyla içeri aktarıldı!";
    return RedirectToAction(nameof(Index));
}
```

**Gerekli NuGet paketi:**
```powershell
Install-Package CsvHelper
```

### Yöntem C: SQL Script ile Doğrudan Ekleme

**SQL Script:**
```sql
USE [Fitness_Center_DB]

-- UYARI: Önce AspNetUsers tablosunda kayıt olması gerekir!
-- Bu örnek sadece Demo amaçlı

INSERT INTO Uyeler (Ad, Soyad, BouSantimetre, AgirlikKilogram, Cinsiyet, FitnessHedefi, KullaniciId)
VALUES 
('Ahmet', 'Yılmaz', 180, 75, 'Erkek', 'Kas Kazanma', 'USER-ID-FROM-ASPNETUSERS-TABLE'),
('Ayşe', 'Demir', 165, 60, 'Kadın', 'Kilo Kaybı', 'USER-ID-FROM-ASPNETUSERS-TABLE');

-- Kontrol et
SELECT * FROM Uyeler;
```

---

## 4️⃣ PROFIL VERİSİNİN KAYDEDILMEME SORUNUNUN ÇÖZÜMÜ

### ❌ Yaygın Hatalar ve Çözümleri

#### HATA #1: KullaniciId Boş Kalması
**Belirti:** Profil oluşturuldu ama KullaniciId = NULL

**Çözüm:** Controller'da şunu kontrol et:
```csharp
// OlusturProfilim POST method'unda
var currentUser = await _userManager.GetUserAsync(User);
uye.KullaniciId = currentUser.Id; // ← BU SATIRı MUTLAKA KONTROL ET
```

**Veritabanında Kontrol:**
```sql
SELECT * FROM Uyeler WHERE KullaniciId IS NULL;
-- Eğer kayıt varsa, KullaniciId'si boş demek
```

#### HATA #2: Model Validation Hatası
**Belirti:** Form gönderiliyor ama hata mesajı görmüyorsun

**Çözüm:** OlusturProfilim.cshtml'de şunu kontrol et:
```html
<!-- Form hata mesajlarını göster -->
@if (!ViewData.ModelState.IsValid)
{
    <div class="alert alert-danger">
        @foreach (var modelState in ViewData.ModelState.Values)
        {
            @foreach (var error in modelState.Errors)
            {
                <p>@error.ErrorMessage</p>
            }
        }
    </div>
}
```

#### HATA #3: Sayısal Alan Hatası
**Belirti:** BouSantimetre ve AgirlikKilogram kaydedilmiyor

**Çözüm:** HTML'de input type'ını kontrol et:
```html
<!-- YANLIŞ -->
<input type="text" name="BouSantimetre" />

<!-- DOĞRU -->
<input type="number" name="BouSantimetre" min="100" max="250" />
```

---

## 5️⃣ VERİTABANI YEDEKLEMESİ VE İLKLENDİRME

### Yedekleme (Backup)

**SSMS ile:**
1. `Databases` → `Fitness_Center_DB` → sağ tıkla
2. `Tasks` → `Back Up`
3. Hedef klasörü seç
4. `OK`

**PowerShell ile:**
```powershell
$server = New-Object "Microsoft.SqlServer.Management.Smo.Server" "(localdb)\mssqllocaldb"
$backup = New-Object "Microsoft.SqlServer.Management.Smo.Backup"
$backup.Database = "Fitness_Center_DB"
$backup.MediaName = "C:\Backups\Fitness_DB_$(Get-Date -Format yyyyMMdd).bak"
$backup.SqlBackup($server)
```

### Veritabanı Sıfırlama (Tamamen Yeni Başlatma)

```powershell
# 1. SSMS'de tüm bağlantıları kapat
# 2. Package Manager Console'da:
Drop-Database -Force
Add-Migration InitialCreate
Update-Database
```

---

## 6️⃣ KONTROL ÇEK LİSTESİ ✅

Profil oluşturulduktan sonra sırasıyla kontrol et:

### ✓ 1. Veritabanına Bağlantı
```powershell
# Terminal'de:
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT @@VERSION"
```

### ✓ 2. Tablo Varlığı
```sql
USE [Fitness_Center_DB]
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'Uyeler';
```

### ✓ 3. Veri Doğrulaması
```sql
SELECT COUNT(*) AS ToplamUyeSayisi FROM Uyeler;
SELECT * FROM AspNetUsers;
```

### ✓ 4. İlişki Kontrolü (Foreign Key)
```sql
SELECT 
    u.Id, u.Ad,
    CASE WHEN au.Id IS NOT NULL THEN '✓ OK' ELSE '✗ HATA' END AS KullaniciVarMi
FROM Uyeler u
LEFT JOIN AspNetUsers au ON u.KullaniciId = au.Id;
```

### ✓ 5. Uygulama Logs Kontrolü
```
Visual Studio → Output pane
"Entity Framework Core" çıktısını incele
Hata mesajlarını ara
```

---

## 📊 VERİ YAPISI (Schema)

```
AspNetUsers (Identity tarafından oluşturulur)
├── Id (Primary Key)
├── UserName
├── Email
├── PasswordHash
└── ...

Uyeler (Custom)
├── Id (Primary Key)
├── Ad
├── Soyad
├── BouSantimetre
├── AgirlikKilogram
├── Cinsiyet
├── FitnessHedefi
├── DogumTarihi
├── KullaniciId (Foreign Key → AspNetUsers.Id) ← ÇOKONOMLU!
└── ...
```

---

## 🔧 CEVAP ARAMA (FAQ)

**S: Profil neden kaydedilmiyor?**
C: Kontrol sırası: 1) Veritabanı bağlantısı 2) Migration uygulanmış mı 3) KullaniciId NULL mı 4) Model validation hataları var mı

**S: CSV dosyasından nasıl veri içeri aktarırım?**
C: `Yöntem B: CSV Dosyasından Veri İçeri Aktar` bölümüne bakın

**S: Veritabanı tamamen sıfırlanmak istem?**
C: `Veritabanı Sıfırlama` bölümüne bakın

**S: Hangi SQL Server sürümü kullanıyorum?**
C: İleri → About Microsoft SQL Server → Express ile başladığını gör

---

## 📞 YARDıM ALMAK İÇİN

1. SSMS'de hata iletisini al
2. `View` → `Error List` kontrol et
3. Visual Studio `Output` pane'inde logs'a bak
4. `UyelerController.cs` `_logger.LogError()` satırlarını kontrol et
