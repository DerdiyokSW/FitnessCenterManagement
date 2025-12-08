# 💪 Spor Salonu Yönetim ve Randevu Sistemi

ASP.NET Core MVC ile geliştirilen, **gerçek hayata yakın** bir fitness center yönetim platformu.

---

## 📋 Proje Özeti

Bu proje, bir spor salonunun tüm işlemlerini dijital ortamda yönetebilen kapsamlı bir web uygulamasıdır:

✅ **Spor Salonu Yönetimi** - Salon bilgileri, hizmetler, antrenörler  
✅ **Üye Yönetimi** - Üye kaydı, profil düzenleme, fitness hedefleri  
✅ **Randevu Sistemi** - Müsaitlik kontrolü, çakışma algılama, onay mekanizması  
✅ **Yapay Zeka Entegrasyonu** - Egzersiz/diyet önerileri, vücut tipi analizi  
✅ **Rol Bazlı Yetkilendirme** - Admin ve Üye rolleri  
✅ **REST API** - LINQ ile filtreleme, JSON yanıtları  

---

## 🛠️ Kullanılan Teknolojiler

| Kategori | Teknoloji |
|----------|-----------|
| **Framework** | ASP.NET Core 7.0 MVC |
| **Dil** | C# |
| **Veritabanı** | SQL Server (LocalDB) |
| **ORM** | Entity Framework Core 7.0 |
| **Kimlik Doğrulama** | ASP.NET Core Identity |
| **Frontend** | HTML5, CSS3, JavaScript, Bootstrap 5 |
| **API** | RESTful API |
| **Logging** | ILogger (Built-in) |

---

## 📁 Proje Yapısı

```
FitnessCenterManagement.Web/
├── Models/              # Veri modelleri
│   ├── SporSalonu.cs
│   ├── Hizmet.cs
│   ├── Antrenor.cs
│   ├── Uye.cs
│   ├── Randevu.cs
│   └── YapayzekaTavsiye.cs
│
├── Controllers/         # İş mantığı
│   ├── SporSalonuController.cs
│   ├── HizmetlerController.cs
│   ├── AntrenorlerController.cs
│   ├── UyelerController.cs
│   ├── RandevuController.cs
│   ├── YapayzekaTavsiyeController.cs
│   └── Api/
│       └── AntrenorlerApiController.cs
│
├── Services/            # Servis katmanı
│   ├── IYapayzekaSirvisi.cs
│   ├── YapayzekaSirvisi.cs
│   ├── IRendevuSirvisi.cs
│   └── RendevuSirvisi.cs
│
├── Data/                # Veritabanı
│   └── FitnessCenterDbContext.cs
│
├── Views/               # Razor View'lar
│   ├── Home/
│   ├── Shared/
│   └── [diğer controller view'ları]
│
├── wwwroot/             # Static files
│   ├── css/
│   ├── js/
│   └── lib/
│
├── appsettings.json     # Uygulama ayarları
├── Program.cs           # Başlangıç ayarları
└── FitnessCenterManagement.Web.csproj
```

---

## 🚀 Başlangıç Adımları

### Gereksinimler
- .NET 7.0 SDK veya üzeri
- SQL Server Express / LocalDB
- Visual Studio 2022 veya VS Code

### Kurulum

1. **Repository'yi klonlayın:**
```bash
git clone https://github.com/USERNAME/FitnessCenterManagement.git
cd FitnessCenterManagement
```

2. **Projeyi açın:**
```bash
cd FitnessCenterManagement.Web
```

3. **Paketleri yükleyin:**
```bash
dotnet restore
```

4. **Veritabanını oluşturun:**
```bash
# Package Manager Console'da:
Add-Migration InitialCreate
Update-Database

# Veya .NET CLI'de:
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. **Uygulamayı çalıştırın:**
```bash
dotnet run
```

Uygulama `https://localhost:5001` adresinde açılacaktır.

---

## 👤 Varsayılan Kullanıcılar

### Admin Hesabı
- **Email:** `ogrencinumarasi@sakarya.edu.tr`
- **Şifre:** `sau`
- **Rol:** Admin

### Üye Kayıt
Yeni üyeler uygulamada kayıt olabilirler.

---

## 📋 Temel Özellikler

### 🏢 Spor Salonu Yönetimi (Admin)
- Spor salonu bilgileri ekleme/düzenleme/silme
- Çalışma saatleri tanımlama
- Hizmet ve antrenör yönetimi

### 🏋️ Hizmet Yönetimi (Admin)
- Hizmet türü ekleme (Yoga, Fitness, Pilates vb.)
- Hizmet süresi ve ücret belirleme
- Hizmet açıklamaları

### 👨‍🏫 Antrenör Yönetimi (Admin)
- Antrenör bilgileri kaydetme
- Uzmanlık alanları ve çalışma saatleri
- İletişim bilgileri

### 👤 Üye Yönetimi
- Üye kaydı ve profil oluşturma
- Kişisel bilgiler (boy, kilo, cinsiyet, hedef)
- Profilimi düzenle seçeneği

### 📅 Randevu Sistemi
**Müsaitlik Kontrolleri:**
- ✅ Antrenörün çalışma saatleri kontrolü
- ✅ Çakışan randevu algılama
- ✅ Aynı saate iki randevu kaydı engelleme

**Randevu İşlemleri:**
- Yeni randevu oluşturma (hizmet, antrenör, tarih seçimi)
- Randevu onay mekanizması (Beklemede → Onaylandı)
- Randevu iptal etme
- Kişisel randevu takvimi

### 🤖 Yapay Zeka Tavsiyesi
**Üyelere sunulan tavsiyeler:**
- 💪 **Egzersiz Planı** - Hedef bazlı antrenman programı
- 🍽️ **Diyet Planı** - Kalori hesaplama ve öneriler
- 📊 **Vücut Tipi Analizi** - BMI ve somatype analizi

### 🔐 Kimlik Doğrulama & Yetkilendirme
- Kayıt/Giriş sistemi
- Rol bazlı erişim kontrolü (Admin/Üye)
- Güvenli şifre saklama (Hash)
- Oturum yönetimi

### 📡 REST API
**API Endpoint Örnekleri:**
- `GET /api/antrenorler` - Tüm antrenörleri listele
- `GET /api/antrenorler/available?date=2025-12-09` - Müsait antrenörleri getir
- `GET /api/members/{id}/appointments` - Üyenin randevularını getir

---

## 🗂️ Veritabanı Modeli

### İlişkiler
```
SporSalonu (1) ──── (N) Hizmet
SporSalonu (1) ──── (N) Antrenor
Uye (1) ──── (N) Randevu
Antrenor (1) ──── (N) Randevu
Hizmet (1) ──── (N) Randevu
Uye (1) ──── (N) YapayzekaTavsiye
```

### Ana Tablolar
- **SporSalonlari** - Salon bilgileri
- **Hizmetler** - Hizmet türleri
- **Antrenorler** - Antrenör bilgileri
- **Uyeler** - Üye profilleri
- **Randevular** - Randevu kayıtları
- **YapayzekaTavsiyeler** - Yapay zeka önerileri
- **AspNetUsers** - Identity kullanıcıları
- **AspNetRoles** - Roller

---

## 🔧 İş Mantığı & Kurallar

### Randevu Oluşturma Kuralları
```csharp
1. Üyenin profili olmalı ✓
2. Başlangıç < Bitiş tarihi ✓
3. Geçmiş tarih olamaz ✓
4. Antrenöre ait çakışan randevu yok ✓
5. Antrenörün çalışma saatleri aralığında ✓
```

### Hata Yönetimi
- Try-catch blokları ile exception handling
- Logging (ILogger)
- User-friendly hata mesajları
- Database constraints

---

## 📸 Ekran Görüntüleri

### 🏠 Ana Sayfa
- Hizmetler listesi
- Antrenör tanıtımı
- Kayıt/Giriş butonları

### 📅 Randevu Sayfası
- Hizmet seçimi
- Antrenör seçimi
- Tarih/Saat seçimi
- Çakışma uyarısı

### 🤖 Yapay Zeka Sayfası
- Egzersiz önerisi formu
- Diyet planı sonuçları
- Vücut tipi analizi

### 👨‍💼 Admin Paneli
- Tüm üyeleri görüntüle
- Tüm randevuları yönet
- Hizmet ve antrenör yönetimi

---

## 🐛 Bilinen Sorunlar & Geliştirme Planı

### Mevcut Sürüm (v1.0)
- Dummy yapay zeka servisi (OpenAI entegrasyonu yapılabilir)
- Temel CRUD işlemleri
- Rol bazlı erişim kontrolü

### Gelecek Sürümler
- [ ] Ödeme sistemi entegrasyonu
- [ ] Email bildirimleri
- [ ] SMS hatırlatıcısı
- [ ] Raporlar ve istatistikler
- [ ] Mobil uygulama
- [ ] Gerçek OpenAI entegrasyonu

---

## 📝 Lisans

Bu proje eğitim amaçlıdır.

---

## 👨‍💻 Geliştirici

**Yusuf** - Sakarya Üniversitesi

---

## 📧 İletişim

Sorular veya öneriler için lütfen issue açınız.

---

## 🎯 Commit History

Proje aşağıdaki adımlarla geliştirilmiştir:

1. ✅ Proje kurulumu ve NuGet paketleri
2. ✅ Entity modelleri oluşturma
3. ✅ DbContext ve migration
4. ✅ Identity kurulumu
5. ✅ Service katmanı
6. ✅ Controllers (CRUD işlemleri)
7. ⏳ Views (Razor şablonları)
8. ⏳ API endpoint'leri
9. ⏳ UI tasarımı ve styling
10. ⏳ Test ve deployment

---

**Son Güncelleme:** 8 Aralık 2025

Kaynakça: Proje, ASP.NET Core MVC en iyi uygulamalarına göre tasarlanmıştır.
