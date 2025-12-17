# 🎬 BAŞLAMAK İÇİN ADIM-ADIM REHBER

## 1️⃣ PROJEYİ BAŞLAT

Terminal'de:
```powershell
cd "c:\Users\yusuf\Desktop\web proje gym\FitnessCenterManagement.Web"
dotnet run
```

**Beklenen Çıktı:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

---

## 2️⃣ TARAYICIDA AÇ

```
http://localhost:5000
```

---

## 3️⃣ HESAP OLUŞTUR (REGISTER)

1. Sayfadaki "Register" linkine tıkla (veya `/Account/Register`)
2. Form doldur:
   - **Username**: test1
   - **Email**: test1@example.com
   - **Password**: Test123!@#
   - **Confirm Password**: Test123!@#
3. "Register" butonuna tıkla

**Beklenen Sonuç**: Login sayfasına yönlendirilir

---

## 4️⃣ GİRİŞ YAP (LOGIN)

1. Email: test1@example.com
2. Password: Test123!@#
3. "Login" butonuna tıkla

**Beklenen Sonuç**: Sabit sayfaya yönlendirilir, Navbar'da "Profilim" linki görünür

---

## 5️⃣ PROFİL OLUŞTUR

1. Navbar'da "Profilim" linkine tıkla (veya `/Uyeler/DuzenleProfilim`)
   - Bu sayfada hata görebilir: **"Profiliniz bulunamadı"** → Beklenen
2. **"Profil Oluştur"** linkine tıkla (veya `/Uyeler/OlusturProfilim`)

3. Form doldur:
   - **Adınız**: Ahmet
   - **Soyadınız**: Yılmaz
   - **Boyunuz (cm)**: 180
   - **Ağırlığınız (kg)**: 75
   - **Cinsiyet**: Erkek
   - **Fitness Hedefi**: Kas Kazanma
   - **Doğum Tarihi**: 1990-01-15 (isteğe bağlı)

4. **"Profil Oluştur"** butonuna tıkla

---

## 6️⃣ HATA KONTROL ET

### Eğer Forma Hata Mesajı Yazılırsa:
- **Screenshot al** (Ctrl+Print Screen)
- **Hata mesajını not et**
- **OUTPUT pane'ini aç** (View → Output)
- **Entity Framework Core loglarını ara**

Örnek hata mesajları:
```
- "Ad alanı boş olamaz"
- "Boyunuz 50 ile 300 arasında olmalıdır"
- "Veritabanı bağlantısı başarısız"
```

---

## 7️⃣ BAŞARILI ÖN KOŞULLAR

✅ Profil başarıyla oluşturulduysa:
- Sayfanın URL'si: `http://localhost:5000/Uyeler/DuzenleProfilim`
- **Yeşil başarı mesajı** görüntülenir: "Profiliniz başarıyla oluşturuldu!"
- **Form alanları** oluşturduğunuz verilerle dolu görünür

---

## 8️⃣ VERİTABANI KONTROL ET

### SSMS Aç (SQL Server Management Studio)

1. Windows'ta "SQL Server Management Studio" arayıp aç
2. "Connect to Server" penceresinde:
   - **Server name**: `(localdb)\mssqllocaldb`
   - **Authentication**: Windows Authentication
   - **Connect** butonuna tıkla

3. Sol tarafta:
   ```
   Databases
   ├── Fitness_Center_DB
   │   ├── Tables
   │   │   ├── AspNetUsers
   │   │   ├── Uyeler  ← BU TABLODA PROFİL OLMALI!
   │   │   └── ...
   ```

4. **Uyeler** tablosuna sağ tıkla → **Select Top 1000 Rows**

**Beklenen Sonuç:**
```
Id  | Ad     | Soyad   | KullaniciId | BouSantimetre | AgirlikKilogram
1   | Ahmet  | Yılmaz  | user-id-123 | 180           | 75
```

---

## 9️⃣ SQL TEST SCRIPT'I ÇALIŞTIR

1. SSMS'de:
   - **File** → **Open** → **File**
   - `c:\Users\yusuf\Desktop\web proje gym\DATABASE_TEST_SCRIPT.sql` seç
   - Aç

2. **Tüm script'i seç** (Ctrl+A)

3. **Çalıştır** (F5 veya Ctrl+E)

4. **Sonuçları oku:**
   - Sayfanın ortasında "ÖZET RAPOR" bölümü
   - ✓ "VERİTABANI SAĞLAM!" mesajı mı görüyor?
   - ✗ Uyarı mesajı mı görüyor?

---

## 🔟 SONUÇ

| Durum | Yapılacak |
|-------|-----------|
| ✅ Profil SSMS'de görünüyor | **SORUN ÇÖZÜLDÜ!** GitHub'a push et |
| ❌ Profil SSMS'de görünmüyor | OUTPUT pane'deki hata mesajını kopyala ve gönder |
| ❌ Form hata gösteriyor | Hata mesajını ekran görüntüsü ile gönder |
| ❌ Veritabanına bağlanılmıyor | appsettings.json bağlantı string'ini kontrol et |

---

## 📝 NOTLAR

- **KullaniciId boş mu?** → Bağlantı problemi (daha önce fixed)
- **SaveChangesAsync hataları?** → OUTPUT pane'de InnerException'ı ara
- **ModelState hatası?** → Form alanlarını tekrar kontrol et

---

## 🆘 SORUN GIDERİCİ

| Sorun | Çözüm |
|-------|-------|
| "Profiliniz bulunamadı" (DuzenleProfilim'de) | **NORMAL** - Profil henüz oluşturmadınız |
| Form yazısı görünmüyor | F12 → Browser Console → Hata var mı? |
| "Veritabanı bağlantısı başarısız" | `dotnet ef database update` çalıştır |
| Halihazırda var hata | Mevcut Uyeler tablosunu sil ve yeniden oluştur |

---

## ✅ TÜM ADIMLAR TAMAMLANDI

Artık:
- ✅ Profil oluşturma kodu düzeltildi
- ✅ SQL rehberi hazırlandı
- ✅ Veritabanı test script'i oluşturuldu
- ✅ Bu başlamak rehberi oluşturuldu

**Sonraki Adım**: Bu rehberi takip et ve sonuçları bildir!
