using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace FitnessCenterManagement.Web.Services
{
    /// <summary>
    /// YapayzekaSirvisi - Yapay zeka tarafından tavsiye oluşturan gerçek implementasyon
    /// 
    /// OpenAI API'ı kullanarak:
    /// - Kişiye özel egzersiz programları oluşturur
    /// - Kişiye özel diyet planları oluşturur
    /// - Vücut tipi analizi ve BMI hesaplaması yapar
    /// 
    /// Özellikler:
    /// - Hata yönetimi - API çöküş durumunda dummy data döndürür
    /// - Logging - Her işlem ve hata kaydedilir
    /// - Performans - Asenkron işlemler ile UI bloklanmaz
    /// </summary>
    public class YapayzekaSirvisi : IYapayzekaSirvisi
    {
        // Konfigürasyon bilgileri (API key vb.)
        private readonly IConfiguration _configuration;
        // Loglama - Debug ve hata takibi için
        private readonly ILogger<YapayzekaSirvisi> _logger;
        // OpenAI API istemcisi
        private readonly OpenAiClient _openAiClient;

        /// <summary>
        /// Constructor - Bağımlılıkları alır (Dependency Injection)
        /// </summary>
        /// <param name="configuration">Konfigürasyon dosyasından ayarları okur</param>
        /// <param name="logger">Loglama işlemleri için</param>
        /// <param name="openAiClient">OpenAI API'ye istek gönderme</param>
        public YapayzekaSirvisi(IConfiguration configuration, ILogger<YapayzekaSirvisi> logger, OpenAiClient openAiClient)
        {
            _configuration = configuration;
            _logger = logger;
            _openAiClient = openAiClient;
        }

        /// <summary>
        /// Fitness tavsiyesi oluşturur - OpenAI API'ı kullanarak
        /// Boy, ağırlık, cinsiyet ve hedef bilgisine göre
        /// Kişiye özel antrenman programı oluşturur
        /// </summary>
        /// <param name="boy">Kullanıcının boyu (cm)</param>
        /// <param name="agirlik">Kullanıcının ağırlığı (kg)</param>
        /// <param name="cinsiyet">Kullanıcının cinsiyeti (Erkek/Kadın/Diğer)</param>
        /// <param name="hedef">Fitness hedefi (Kilo verme, Kas kazanma vb.)</param>
        /// <returns>OpenAI tarafından oluşturulan egzersiz tavsiyesi metni</returns>
        public async Task<string> EgzersizTavsiyesiAl(int boy, int agirlik, string cinsiyet, string hedef)
        {
            try
            {
                // İşlemi log'la - debug için yararlı
                _logger.LogInformation($"Egzersiz tavsiyesi talep edildi: Boy={boy}cm, Ağırlık={agirlik}kg, Cinsiyet={cinsiyet}, Hedef={hedef}");

                // OpenAI API'ını çağır - asenkron olarak
                var tavsiye = await _openAiClient.GetFitnessTavsiesiAsync(boy, agirlik, cinsiyet, hedef);

                // Başarılı işlemi log'la
                _logger.LogInformation("Egzersiz tavsiyesi başarıyla oluşturuldu.");
                return tavsiye;
            }
            catch (Exception ex)
            {
                // API hatası - örnek veri döndür
                _logger.LogError($"Egzersiz tavsiyesi oluşturma hatası: {ex.Message}");
                // Hata durumunda dummy tavsiye döndür (alt düşey çökmesini engelle)
                return oluşturDummyEgzersizTavsiyesi(boy, agirlik, cinsiyet, hedef);
            }
        }

        /// <summary>
        /// Diyet tavsiyesi oluşturur - OpenAI API'ı kullanarak
        /// Boy, ağırlık, cinsiyet ve hedef bilgisine göre
        /// Kişiye özel beslenme planı oluşturur
        /// </summary>
        /// <param name="boy">Kullanıcının boyu (cm)</param>
        /// <param name="agirlik">Kullanıcının ağırlığı (kg)</param>
        /// <param name="cinsiyet">Kullanıcının cinsiyeti (Erkek/Kadın/Diğer)</param>
        /// <param name="hedef">Fitness hedefi (Kilo verme, Kas kazanma vb.)</param>
        /// <returns>OpenAI tarafından oluşturulan diyet tavsiyesi metni</returns>
        public async Task<string> DiyetTavsiyesiAl(int boy, int agirlik, string cinsiyet, string hedef)
        {
            try
            {
                // İşlemi log'la - debug için yararlı
                _logger.LogInformation($"Diyet tavsiyesi talep edildi: Boy={boy}cm, Ağırlık={agirlik}kg, Cinsiyet={cinsiyet}, Hedef={hedef}");

                // OpenAI API'ını çağır - asenkron olarak
                var tavsiye = await _openAiClient.GetDiyetTavsiesiAsync(boy, agirlik, cinsiyet, hedef);

                // Başarılı işlemi log'la
                _logger.LogInformation("Diyet tavsiyesi başarıyla oluşturuldu.");
                return tavsiye;
            }
            catch (Exception ex)
            {
                // API hatası - örnek veri döndür
                _logger.LogError($"Diyet tavsiyesi oluşturma hatası: {ex.Message}");
                // Hata durumunda dummy tavsiye döndür (alt düşey çökmesini engelle)
                return oluşturDummyDiyetTavsiyesi(boy, agirlik, cinsiyet, hedef);
            }
        }

        /// <summary>
        /// Vücut tipi analizi yapar - OpenAI API'ı kullanarak
        /// Boy, ağırlık ve cinsiyet bilgisine göre
        /// BMI hesaplar ve vücut tipi analizi yapar
        /// </summary>
        /// <param name="boy">Kullanıcının boyu (cm)</param>
        /// <param name="agirlik">Kullanıcının ağırlığı (kg)</param>
        /// <param name="cinsiyet">Kullanıcının cinsiyeti (Erkek/Kadın/Diğer)</param>
        /// <returns>OpenAI tarafından oluşturulan vücut analizi metni</returns>
        public async Task<string> VucutTipiAnaliziYap(int boy, int agirlik, string cinsiyet)
        {
            try
            {
                // İşlemi log'la - debug için yararlı
                _logger.LogInformation($"Vücut tipi analizi talep edildi: Boy={boy}cm, Ağırlık={agirlik}kg, Cinsiyet={cinsiyet}");

                // OpenAI API'ını çağır - asenkron olarak
                var analiz = await _openAiClient.GetVucutTipiAnaliziAsync(boy, agirlik, cinsiyet);

                // Başarılı işlemi log'la
                _logger.LogInformation("Vücut tipi analizi başarıyla oluşturuldu.");
                return analiz;
            }
            catch (Exception ex)
            {
                // API hatası - örnek veri döndür
                _logger.LogError($"Vücut tipi analizi hatası: {ex.Message}");
                // Hata durumunda dummy analiz döndür (alt düşey çökmesini engelle)
                return oluşturDummyVucutTipiAnalizi(boy, agirlik, cinsiyet);
            }
        }

        // ============================================
        // DUMMY (ÖRNEK) VERİ OLUŞTURMA METODLARI
        // ============================================
        // Bu metodlar OpenAI API'si çöktüğünde 
        // uygulamanın çökmesini engeller.
        // Gerçek ortamda API kullanılır.

        /// <summary>
        /// Örnek egzersiz tavsiyesi oluşturur
        /// 
        /// Kullanım: OpenAI API çöküş durumunda
        /// Bu dummy (örnek) veriler gösterilir
        /// </summary>
        /// <param name="boy">Kullanıcının boyu (cm)</param>
        /// <param name="agirlik">Kullanıcının ağırlığı (kg)</param>
        /// <param name="cinsiyet">Kullanıcının cinsiyeti</param>
        /// <param name="hedef">Fitness hedefi</param>
        /// <returns>Önceden hazırlanmış örnek egzersiz planı</returns>
        private string oluşturDummyEgzersizTavsiyesi(int boy, int agirlik, string cinsiyet, string hedef)
        {
            // Örnek antrenman planı - gerçek tavsiye yerine
            var tavsiye = $@"
🏋️ EGZERSİZ TAVSIYE PLANI (Otomatik Hazırlanan Örnek)

Kişisel Bilgiler:
- Boy: {boy} cm
- Ağırlık: {agirlik} kg
- Cinsiyet: {cinsiyet}
- Hedef: {hedef}

HAFTANIN ANTRENMAN PROGRAMI:

📅 PAZARTESİ - Üst Vücut Gücü
- Bench Press: 4 set x 6-8 tekrar
- Barre Rows: 4 set x 6-8 tekrar
- Shoulder Press: 3 set x 8-10 tekrar
- Pull-ups: 3 set x maksimum
- Bitirme: Bicep curls 3 set x 12-15 tekrar

📅 SALI - Alt Vücut
- Squatlar: 4 set x 6-8 tekrar
- Romanian Deadlifts: 4 set x 8-10 tekrar
- Leg Press: 3 set x 8-10 tekrar
- Leg Curls: 3 set x 12-15 tekrar
- Calf Raises: 3 set x 15-20 tekrar

📅 ÇARŞAMBA - Dinlenme veya Hafif Kardiyyo (30 dakika)

📅 PERŞEMBE - Bütün Vücut
- Deadlifts: 4 set x 3-5 tekrar
- Incline Bench Press: 4 set x 6-8 tekrar
- Barbell Rows: 3 set x 6-8 tekrar
- Dips: 3 set x 8-12 tekrar
- Face Pulls: 3 set x 15-20 tekrar

📅 CUMA - Kardiyyo & Core
- 20 dakika yüksek yoğunluklu aralık antrenmanı (HIIT)
- Plank: 3 set x 60 saniye
- Russian Twists: 3 set x 20 tekrar
- Leg Raises: 3 set x 12-15 tekrar

📅 CUMARTESİ - Fonksiyonel Antrenman
- Kettlebell Swings: 4 set x 20 tekrar
- Med Ball Slams: 3 set x 12 tekrar
- Battle Ropes: 3 set x 40 saniye
- Box Jumps: 3 set x 8 tekrar

📅 PAZAR - Dinlenme Günü

⚠️ ÖNEMLİ NOTLAR:
1. Her antrenmandan önce 5-10 dakika ısınma yap
2. Her antrenmandan sonra 5-10 dakika soğuma hareketi yap
3. 48 saat ağır antrenman arası ver
4. Bol su iç (günde 3-4 litre)
5. Yeterli uyku al (günde 7-9 saat)

Başarı İçin İpuçları:
✅ Sabırlı ol - sonuçlar 4-6 haftada görülür
✅ Progresyon eklemeye devam et (agırlık, set, tekrar)
✅ Formu sahip çık - kalite miktardan önemliydi
✅ Tutarlı kal - en iyi plan, devamlı yapılanıdır

NOT: Bu örnek bir plandır. Daha iyi sonuçlar için OpenAI API aktif olmalıdır.
";

            return tavsiye;
        }

        /// <summary>
        /// Örnek diyet tavsiyesi oluşturur
        /// 
        /// Kullanım: OpenAI API çöküş durumunda
        /// Bu dummy (örnek) veriler gösterilir
        /// </summary>
        /// <param name="boy">Kullanıcının boyu (cm)</param>
        /// <param name="agirlik">Kullanıcının ağırlığı (kg)</param>
        /// <param name="cinsiyet">Kullanıcının cinsiyeti</param>
        /// <param name="hedef">Fitness hedefi</param>
        /// <returns>Önceden hazırlanmış örnek diyet planı</returns>
        private string oluşturDummyDiyetTavsiyesi(int boy, int agirlik, string cinsiyet, string hedef)
        {
            // BMR (Bazal Metabolizm Hızı) basit hesaplama
            // Vücut hareketsiz durumda bile harcanan kalori
            double bmr = cinsiyet.ToLower() == "erkek" 
                ? 88.362 + (13.397 * agirlik) + (4.799 * boy) - (5.677 * 25)  // Erkekler için formül (25 yaş varsayılan)
                : 447.593 + (9.247 * agirlik) + (3.098 * boy) - (4.330 * 25); // Kadınlar için formül (25 yaş varsayılan)

            // Günlük kalori ihtiyacı (orta aktivite seviyesi)
            double gunlukKalori = bmr * 1.55;

            // Örnek diyet planı
            var tavsiye = $@"
🍽️ DİYET TAVSIYE PLANI (Otomatik Hazırlanan Örnek)

Kişisel Bilgiler:
- Boy: {boy} cm
- Ağırlık: {agirlik} kg
- Cinsiyet: {cinsiyet}
- Fitness Hedefi: {hedef}

GÜNLÜK KALORİ HEDEFI: {gunlukKalori:F0} Kalori

MAKRO NUTRIENT DAĞILIMI:
- Protein: 30% ({(gunlukKalori * 0.30 / 4):F0}g)
  → Amino asitler için kas gelişimi
- Karbonhidrat: 45% ({(gunlukKalori * 0.45 / 4):F0}g)
  → Enerji ve antrenman performansı
- Yağ: 25% ({(gunlukKalori * 0.25 / 9):F0}g)
  → Hormon üretimi ve sağlık

GÜNLÜK BESLENME ÖRNEĞİ:

🌅 KAHVALTISI (07:00 - 08:00)
- 3 adet yumurta beyazı + 1 bütün yumurta
- 1 bardak tam buğday ekmekleri
- 1 muz
- 1 çay kaşığı almond yağı
- Kalorileri: ~450 cal

☕ SABAH ATIŞTIRILMASI (10:00 - 10:30)
- Protein shake: 1 kepçe whey + 1 muz + 250ml süt
- 1 avuç badem
- Kalorileri: ~350 cal

🍽️ ÖĞLEN YEMEĞİ (12:30 - 13:30)
- 150g tavuk göğsü (ızgara)
- 1 kase pirinç (1,5 fincan)
- Yeşil salatası
- 1 çay kaşığı zeytin yağı
- Kalorileri: ~550 cal

🥗 ÖĞLEDEN SONRA ATIŞTIRILMASI (16:00 - 16:30)
- Yunan yogurtu (200g)
- 1 avuç fındık
- Honey 1 tatlı kaşığı
- Kalorileri: ~250 cal

🍴 AKŞAM YEMEĞİ (19:00 - 20:00)
- 150g balık (somon veya levrek)
- 1 medium patates (haşlanmış)
- Brokoli porsiyonu
- 1 çay kaşığı zeytin yağı
- Kalorileri: ~500 cal

🌙 GECE ATIŞTIRILMASI (21:30 - 22:00) - İSTEĞE BAĞLI
- 150ml Casein shake
- Kalorileri: ~150 cal

TOPLAM GÜNLÜK KALORİ: ~2650 cal

⚠️ ÖNEMLİ BESLENME İLKELERİ:
1. Bol su iç (günde 3-4 litre, en az)
2. Tuz tüketimini sınırla
3. Şekerlı içeceklerden kaçın
4. İşlenmiş yiyecekleri minimize et
5. Vitamin takviyesi almayı düşün (D3, Omega3, Multivitamin)

ALIŞVERIŞ LİSTESİ:
✅ Protein Kaynakları: Tavuk, balık, yumurta, hindi, sığır eti
✅ Kompleks Karbonhidratlar: Pirinç, makarna, tatlı patates, avena
✅ Sağlıklı Yağlar: Zeytinyağı, fındık, badem, avokado
✅ Sebzeler: Brokoli, ıspanak, mısır, havuç
✅ Meyveler: Muz, elma, çilek, portakal

BESLENME İPUÇLARİ:
🎯 Yemeği haftalık hazırla (meal prep)
🎯 Kasalarında sakla - hızlı erişim için
🎯 Her günü kaydını tut - ilerlemeni takip et
🎯 Esneklik göster - sosyal durumlar da var
🎯 Özel günlerde az miktar afet yiyeceğini tüket

NOT: Bu örnek bir plandır. Daha iyi sonuçlar için OpenAI API aktif olmalıdır.
";

            return tavsiye;
        }

        /// <summary>
        /// Örnek vücut tipi analizi oluşturur
        /// 
        /// Kullanım: OpenAI API çöküş durumunda
        /// Bu dummy (örnek) veriler gösterilir
        /// </summary>
        /// <param name="boy">Kullanıcının boyu (cm)</param>
        /// <param name="agirlik">Kullanıcının ağırlığı (kg)</param>
        /// <param name="cinsiyet">Kullanıcının cinsiyeti</param>
        /// <returns>Önceden hazırlanmış örnek vücut analizi</returns>
        private string oluşturDummyVucutTipiAnalizi(int boy, int agirlik, string cinsiyet)
        {
            // BMI Hesaplama
            // BMI = Ağırlık (kg) / Boy (m)²
            double boyMetre = boy / 100.0;
            double bmi = agirlik / (boyMetre * boyMetre);

            // BMI kategorisini belirle
            string bmiKategorisi = bmi switch
            {
                < 18.5 => "Zayıf",
                >= 18.5 and < 25 => "Normal Kilolu",
                >= 25 and < 30 => "Hafif Obez",
                >= 30 => "Obez",
                _ => "Bilinmiyor"
            };

            // Vücut tipi tahmini (somatype)
            // Boy ve ağırlık oranına göre tahmin
            string vucutTipi = (boy, agirlik) switch
            {
                (> 180, < 75) => "Ektomorf (İnce, Uzun Yapı)",
                (> 175 and <= 180, 75) => "Mezomorf (Atletik, Kaslı Yapı)",
                (> 170 and <= 175, > 85) => "Endomorf (Geniş, Dolgun Yapı)",
                _ => "Karma Vücut Tipi"
            };

            // Örnek analiz raporu
            var analiz = $@"
📊 VÜCUT TİPİ ANALİZİ RAPORU (Otomatik Hazırlanan Örnek)

TEMEL KİŞİSEL VERİLER:
- Boy: {boy} cm
- Ağırlık: {agirlik} kg
- Cinsiyet: {cinsiyet}
- Hesaplandığı Tarih: {DateTime.Now:dd.MM.yyyy}

BMI (VÜcut Kitle İndeksi) SONUÇLARI:
- BMI Değeri: {bmi:F1}
- Kategori: {bmiKategorisi}
- Sağlık Durumu: Uzman doktora danış

VÜCUT TİPİ (SOMATYPE):
{vucutTipi}

TİP AÇIKLAMASI:

📌 EKTOMORF (İnce, Uzun Yapı):
   - Doğal olarak zayıf ve uzun boylu
   - Hızlı metabolizma
   - Kas kazanmak daha zordu
   - Beslenme: Yüksek kalori, yüksek protein

📌 MEZOMORF (Atletik, Kaslı):
   - Doğal olarak kaslı ve geliştirilmiş kemik yapısı
   - Orta metabolizma
   - Kas ve kuvvet kazanmak kolay
   - Beslenme: Dengeli makro nutrient

📌 ENDOMORF (Geniş, Dolgun Yapı):
   - Geniş kemik yapısı ve daha fazla yağ depolama
   - Yavaş metabolizma
   - Yağ kaybı daha uzun sürer
   - Beslenme: Daha düşük kalori, yüksek protein

KIŞISEL GELIŞIM ÖNERİLERİ:
✅ Düzenli egzersiz (haftada en az 4-5 gün)
✅ Dengeli beslenme planı
✅ Yeterli uyku (günde 7-9 saat)
✅ Su tüketimini artır (günde 3+ litre)
✅ Stres yönetimi (meditasyon, yoga)
✅ İlerlemeyi düzenli takip et (ölçüm, fotoğraf)

HEDEF AĞIRLIK HESAPLAMASI:
- Optimal BMI Aralığı: 18.5 - 25
- Hedef Ağırlık Aralığı: {(18.5 * boyMetre * boyMetre):F1} - {(25 * boyMetre * boyMetre):F1} kg

BAŞARILILIK İÇİN STRATEJ:
1️⃣ Kısa vadeli hedefler belirle (aylık)
2️⃣ Uzun vadeli hedefler yaz (6 aylık, 1 yıllık)
3️⃣ Haftalık ilerlemeyi takip et
4️⃣ Ayınlık vücut ölçümlerini al
5️⃣ Her 3 ayda bir fotoğraf çek
6️⃣ Motivasyonu koru - sosyal destek al
7️⃣ Başarılarını kutla - ödüllendir

NOT: Bu analiz genel bir rehberdir. Bu örnek verilerdir.
     Daha iyi sonuçlar için OpenAI API aktif olmalıdır.
     Kişisel öneriler için profesyonel bir diyetisyen veya antrenöre danış.
";

            return analiz;
        }
    }
}
