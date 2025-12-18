# 🧪 PROFIL KAYDEDILME TESTİ

## ✅ TEST AKIŞI

### ADIM 1: REGISTER (Hesap Oluştur)
```
URL: http://localhost:5000/Account/Register
Form Doldur:
  - Username: testuser1
  - Email: test1@example.com
  - Password: Test123!@#
  - Confirm Password: Test123!@#
Gönder: Register
Beklenen: Login sayfasına yönlendi
```

---

### ADIM 2: LOGIN (Giriş Yap)
```
URL: http://localhost:5000/Account/Login
Form Doldur:
  - Email: test1@example.com
  - Password: Test123!@#
Gönder: Login
Beklenen: Ana sayfa + Navbar'da "Profilim" linki görünür
```

---

### ADIM 3: PROFİL OLUŞTUR
```
URL: http://localhost:5000/Uyeler/OlusturProfilim
Form Doldur:
  - Adınız: Ahmet
  - Soyadınız: Yılmaz
  - Boyunuz (cm): 180
  - Ağırlığınız (kg): 75
  - Cinsiyet: Erkek
  - Fitness Hedefi: Kas Kazanma
  - Doğum Tarihi: 1990-05-15
Gönder: Profil Oluştur
```

**BEKLENEN SONUÇ:**
```
✅ "Profiliniz başarıyla oluşturuldu!" MESAJI GÖRÜNÜR
✅ Sayfa otomatik DuzenleProfilim'e yönlendirilir
✅ Form'da girdiğin bilgiler PRE-FILLED olarak görünür
```

**BUNLAR DB'DEN GELIYOR DEMEKTİR!**

---

### ADIM 4: SAYFAYI YENILE (Verinin Kalıcılığını Test Et)
```
Tarayıcıda F5'e bas (Refresh)
```

**BEKLENEN SONUÇ:**
```
✅ Bilgilerin hepsi hala görünür
✅ Ahmet, Yılmaz, 180, 75, Erkek, Kas Kazanma, 1990-05-15
```

**BUNLAR DB'DEN YÜKLENIYORDUR!**

---

### ADIM 5: PROFILE GIT → ÇIKIŞ YAP → GİRİŞ YAP
```
1. Navbar'dan çıkış yap (Logout)
2. Tekrar giriş yap (ADIM 2'yi tekrar)
3. Navbar'dan "Profilim" linkine tıkla
```

**BEKLENEN SONUÇ:**
```
✅ Tüm bilgiler hala orada
✅ Ahmet, Yılmaz, 180, 75, Erkek, Kas Kazanma, 1990-05-15
```

**BUNLAR KALICI VERİDİR!**

---

## 🔍 SQL VERİTABANI KONTROL

### SSMS'de Manuel Kontrol:
```sql
USE [Fitness_Center_DB]

-- Kullanıcıyı bul
SELECT * FROM AspNetUsers WHERE Email = 'test1@example.com';

-- Üye profilini bul
SELECT * FROM Uyeler WHERE Ad = 'Ahmet' AND Soyad = 'Yılmaz';

-- İlişkiyi kontrol et
SELECT 
    u.Id AS UyeId,
    u.Ad,
    u.Soyad,
    u.KullaniciId,
    au.Email
FROM Uyeler u
INNER JOIN AspNetUsers au ON u.KullaniciId = au.Id
WHERE au.Email = 'test1@example.com';
```

**BEKLENEN SONUÇ:**
```
Id    | Ad    | Soyad   | KullaniciId | BouSantimetre | AgirlikKilogram
1     | Ahmet | Yılmaz  | xxx-yyy-zzz | 180           | 75
```

---

## ✅ BAŞARILI TEST ÖZETİ

Eğer tüm adımlar başarılıysa:
- ✅ Profil oluşturma çalışıyor
- ✅ Veritabanına kaydediliyor
- ✅ Veritabanından geri yükleniyor
- ✅ Veri kalıcı (persistent)

**SONUÇ: SİSTEM HAZIR! 🎉**

---

## 🐛 HATA OLURSA

### Profil oluştururken hata:
→ Form altında kırmızı hata mesajı görünür
→ Konsol'da (Terminal) çıktısını oku
→ ErrorViewModel ayrıntılarını kontrol et

### Bilgiler gösterilmiyorsa:
→ OlusturProfilim.cshtml'de validation error olabilir
→ SSMS'de SELECT * FROM Uyeler; çalıştır
→ Veri orada mı kontrol et

### DB kaydı başarılı ama gösterilmiyorsa:
→ DuzenleProfilim GET metodunu kontrol et
→ Konsol loglarını oku
→ Session/Cache problemi olabilir (F5 ile yenile)

---

## 📝 NOTLAR

- **TempData["BasariliMesaj"]** → Yönlendirmenin ardından 1 kere gösterilir
- **@Model** → DB'den getirilen Uye nesnesi
- **View(uye)** → DuzenleProfilim'de gösterilen profil
- Eğer profil yoksa → OlusturProfilim'e yönlendir (OlusturProfilim GET metodunda)

---

## 🚀 SONRAKI TEST: AI TAVSIYE

Profil başarılı ise, AI tavsiye test edebilirsin:
```
1. Navbar → "AI Öneriler"
2. "Egzersiz Tavsiyesi" / "Diyet Tavsiyesi"
3. OpenAI'dan gerçek yanıt al
```
