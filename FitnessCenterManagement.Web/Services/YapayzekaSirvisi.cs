using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace FitnessCenterManagement.Web.Services
{
    /// <summary>
    /// YapayzekaSirvisi - Yapay zeka tarafından tavsiye oluşturan gerçek implementasyon
    /// 
    /// OpenAI API'ı kullanarak (GPT-3.5-turbo):
    /// - Kişiye özel egzersiz programları oluşturur
    /// - Kişiye özel diyet planları oluşturur
    /// - Vücut tipi analizi ve BMI hesaplaması yapar
    /// 
    /// Özellikler:
    /// ✅ Gerçek OpenAI API istekleri - dummy data yok
    /// ✅ Hata yönetimi - Detaylı exception handling ve loglama
    /// ✅ Input doğrulama - Geçersiz parametreleri kontrol eder
    /// ✅ Logging - Her işlem ve hata HttpRequestException dahil kaydedilir
    /// ✅ Asenkron işlemler - UI bloklanmaz
    /// 
    /// Konfigürasyon:
    /// - API Key: appsettings.json → AiSettings:ApiKey
    /// - Model: gpt-3.5-turbo (ucuz ve hızlı)
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
                // Giriş parametrelerini doğrula
                if (boy <= 0 || agirlik <= 0 || string.IsNullOrWhiteSpace(cinsiyet) || string.IsNullOrWhiteSpace(hedef))
                {
                    _logger.LogWarning("Egzersiz tavsiyesi: Geçersiz parametreler");
                    throw new ArgumentException("Tüm parametreler gereklidir ve sıfırdan büyük olmalıdır");
                }

                _logger.LogInformation($"🏋️ Egzersiz tavsiyesi talep edildi: Boy={boy}cm, Ağırlık={agirlik}kg, Cinsiyet={cinsiyet}, Hedef={hedef}");

                // OpenAI API'ını çağır - gerçek API isteği
                var tavsiye = await _openAiClient.GetFitnessTavsiesiAsync(boy, agirlik, cinsiyet, hedef);

                if (string.IsNullOrWhiteSpace(tavsiye))
                {
                    _logger.LogWarning("OpenAI API boş yanıt döndürdü");
                    throw new InvalidOperationException("API boş yanıt verdi");
                }

                _logger.LogInformation("✅ Egzersiz tavsiyesi başarıyla OpenAI API'den alındı");
                return tavsiye;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"❌ OpenAI API bağlantı hatası: {ex.Message}");
                throw new InvalidOperationException("OpenAI API bağlantısı başarısız. API key'i kontrol edin.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Egzersiz tavsiyesi oluşturmada hata: {ex.Message}");
                throw;
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
                // Giriş parametrelerini doğrula
                if (boy <= 0 || agirlik <= 0 || string.IsNullOrWhiteSpace(cinsiyet) || string.IsNullOrWhiteSpace(hedef))
                {
                    _logger.LogWarning("Diyet tavsiyesi: Geçersiz parametreler");
                    throw new ArgumentException("Tüm parametreler gereklidir ve sıfırdan büyük olmalıdır");
                }

                _logger.LogInformation($"🍽️ Diyet tavsiyesi talep edildi: Boy={boy}cm, Ağırlık={agirlik}kg, Cinsiyet={cinsiyet}, Hedef={hedef}");

                // OpenAI API'ını çağır - gerçek API isteği
                var tavsiye = await _openAiClient.GetDiyetTavsiesiAsync(boy, agirlik, cinsiyet, hedef);

                if (string.IsNullOrWhiteSpace(tavsiye))
                {
                    _logger.LogWarning("OpenAI API boş yanıt döndürdü");
                    throw new InvalidOperationException("API boş yanıt verdi");
                }

                _logger.LogInformation("✅ Diyet tavsiyesi başarıyla OpenAI API'den alındı");
                return tavsiye;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"❌ OpenAI API bağlantı hatası: {ex.Message}");
                throw new InvalidOperationException("OpenAI API bağlantısı başarısız. API key'i kontrol edin.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Diyet tavsiyesi oluşturmada hata: {ex.Message}");
                throw;
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
                // Giriş parametrelerini doğrula
                if (boy <= 0 || agirlik <= 0 || string.IsNullOrWhiteSpace(cinsiyet))
                {
                    _logger.LogWarning("Vücut analizi: Geçersiz parametreler");
                    throw new ArgumentException("Tüm parametreler gereklidir ve sıfırdan büyük olmalıdır");
                }

                _logger.LogInformation($"📊 Vücut tipi analizi talep edildi: Boy={boy}cm, Ağırlık={agirlik}kg, Cinsiyet={cinsiyet}");

                // OpenAI API'ını çağır - gerçek API isteği
                var analiz = await _openAiClient.GetVucutTipiAnaliziAsync(boy, agirlik, cinsiyet);

                if (string.IsNullOrWhiteSpace(analiz))
                {
                    _logger.LogWarning("OpenAI API boş yanıt döndürdü");
                    throw new InvalidOperationException("API boş yanıt verdi");
                }

                _logger.LogInformation("✅ Vücut tipi analizi başarıyla OpenAI API'den alındı");
                return analiz;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"❌ OpenAI API bağlantı hatası: {ex.Message}");
                throw new InvalidOperationException("OpenAI API bağlantısı başarısız. API key'i kontrol edin.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Vücut tipi analizi yapılırken hata: {ex.Message}");
                throw;
            }
        }

        // ============================================
        // YARDIMCI METODLAR
        // ============================================
        
        // NOT: Bu sınıf artık OpenAI API'ye doğru istek yapıyor.
        // Dummy metodlar kaldırıldı. Sadece gerçek API sonuçları döndürülüyor.
    }
}
