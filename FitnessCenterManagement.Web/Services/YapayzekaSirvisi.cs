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
    /// </summary>
    public class YapayzekaSirvisi : IYapayzekaSirvisi
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<YapayzekaSirvisi> _logger;
        private readonly OpenAiClient _openAiClient;

        /// <summary>
        /// Constructor - Bağımlılıkları alır
        /// </summary>
        public YapayzekaSirvisi(IConfiguration configuration, ILogger<YapayzekaSirvisi> logger, OpenAiClient openAiClient)
        {
            _configuration = configuration;
            _logger = logger;
            _openAiClient = openAiClient;
        }

        /// <summary>
        /// Fitness tavsiyesi oluşturur - OpenAI API'ı kullanarak
        /// </summary>
        public async Task<string> EgzersizTavsiyesiAl(int boy, int agirlik, string cinsiyet, string hedef)
        {
            try
            {
                _logger.LogInformation($"Egzersiz tavsiyesi talep edildi: Boy={boy}cm, Ağırlık={agirlik}kg, Cinsiyet={cinsiyet}, Hedef={hedef}");

                // OpenAI API'ını çağır
                var tavsiye = await _openAiClient.GetFitnessTavsiesiAsync(boy, agirlik, cinsiyet, hedef);

                _logger.LogInformation("Egzersiz tavsiyesi başarıyla oluşturuldu.");
                return tavsiye;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Egzersiz tavsiyesi oluşturma hatası: {ex.Message}");
                // Hata durumunda dummy tavsiye döndür
                return oluşturDummyEgzersizTavsiyesi(boy, agirlik, cinsiyet, hedef);
            }
        }

        /// <summary>
        /// Diyet tavsiyesi oluşturur - OpenAI API'ı kullanarak
        /// </summary>
        public async Task<string> DiyetTavsiyesiAl(int boy, int agirlik, string cinsiyet, string hedef)
        {
            try
            {
                _logger.LogInformation($"Diyet tavsiyesi talep edildi: Boy={boy}cm, Ağırlık={agirlik}kg, Cinsiyet={cinsiyet}, Hedef={hedef}");

                // OpenAI API'ını çağır
                var tavsiye = await _openAiClient.GetDiyetTavsiesiAsync(boy, agirlik, cinsiyet, hedef);

                _logger.LogInformation("Diyet tavsiyesi başarıyla oluşturuldu.");
                return tavsiye;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Diyet tavsiyesi oluşturma hatası: {ex.Message}");
                // Hata durumunda dummy tavsiye döndür
                return oluşturDummyDiyetTavsiyesi(boy, agirlik, cinsiyet, hedef);
            }
        }

        /// <summary>
        /// Vücut tipi analizi yapar - OpenAI API'ı kullanarak
        /// </summary>
        public async Task<string> VucutTipiAnaliziYap(int boy, int agirlik, string cinsiyet)
        {
            try
            {
                _logger.LogInformation($"Vücut tipi analizi talep edildi: Boy={boy}cm, Ağırlık={agirlik}kg, Cinsiyet={cinsiyet}");

                // OpenAI API'ını çağır
                var analiz = await _openAiClient.GetVucutTipiAnaliziAsync(boy, agirlik, cinsiyet);

                _logger.LogInformation("Vücut tipi analizi başarıyla oluşturuldu.");
                return analiz;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Vücut tipi analizi hatası: {ex.Message}");
                // Hata durumunda dummy analiz döndür
                return oluşturDummyVucutTipiAnalizi(boy, agirlik, cinsiyet);
            }
        }

        // ============================================
        // DUMMY (ÖRNEK) VERİ OLUŞTURMA METODLARI
        // ============================================

        /// <summary>
        /// Örnek egzersiz tavsiyesi oluşturur
        /// Gerçek projede silinecek, OpenAI'dan cevap alınacak
        /// </summary>
        private string oluşturDummyEgzersizTavsiyesi(int boy, int agirlik, string cinsiyet, string hedef)
        {
            var tavsiye = $@"
🏋️ EGZERSİZ TAVSIYE PLANI

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
";

            return tavsiye;
        }

        /// <summary>
        /// Örnek diyet tavsiyesi oluşturur
        /// Gerçek projede silinecek, OpenAI'dan cevap alınacak
        /// </summary>
        private string oluşturDummyDiyetTavsiyesi(int boy, int agirlik, string cinsiyet, string hedef)
        {
            // BMR (Bazal Metabolizm Hızı) basit hesaplama
            double bmr = cinsiyet.ToLower() == "erkek" 
                ? 88.362 + (13.397 * agirlik) + (4.799 * boy) - (5.677 * 25)  // 25 yaş varsayılan
                : 447.593 + (9.247 * agirlik) + (3.098 * boy) - (4.330 * 25);

            double gunlukKalori = bmr * 1.55; // Orta aktivite seviyesi

            var tavsiye = $@"
🍽️ DİYET TAVSIYE PLANI

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
";

            return tavsiye;
        }

        /// <summary>
        /// Örnek vücut tipi analizi oluşturur
        /// Gerçek projede silinecek, OpenAI'dan cevap alınacak
        /// </summary>
        private string oluşturDummyVucutTipiAnalizi(int boy, int agirlik, string cinsiyet)
        {
            // BMI Hesaplama
            double boyMetre = boy / 100.0;
            double bmi = agirlik / (boyMetre * boyMetre);

            string bmiKategorisi = bmi switch
            {
                < 18.5 => "Zayıf",
                >= 18.5 and < 25 => "Normal Kilolu",
                >= 25 and < 30 => "Hafif Obez",
                >= 30 => "Obez",
                _ => "Bilinmiyor"
            };

            // Vücut tipi tahmini (somatype)
            string vucutTipi = (boy, agirlik) switch
            {
                (> 180, < 75) => "Ektomorf (İnce, Uzun Yapı)",
                (> 175 and <= 180, 75) => "Mezomorf (Atletik, Kaslı Yapı)",
                (> 170 and <= 175, > 85) => "Endomorf (Geniş, Dolgun Yapı)",
                _ => "Karma Vücut Tipi"
            };

            var analiz = $@"
📊 VÜCUT TİPİ ANALİZİ RAPORU

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

NOT: Bu analiz genel bir rehberdir. Kişisel öneriler için 
     profesyonel bir diyetisyen veya antrenöre danış.
";

            return analiz;
        }
    }
}
