using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FitnessCenterManagement.Web.Services
{
    /// <summary>
    /// OpenAiClient - OpenAI API'ye istek gönderen client
    /// 
    /// USAGE:
    /// - HttpClient dependency injection yapılmalı
    /// - appsettings.json'da OpenAI API key tanımlanmalı
    /// - GPT-3.5-turbo model kullanılıyor (ucuz ve stabil)
    /// 
    /// EXAMPLE:
    /// var response = await _openAiClient.CreateCompletionAsync(prompt);
    /// </summary>
    public class OpenAiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAiClient> _logger;

        /// <summary>
        /// OpenAI API Base URL
        /// </summary>
        private const string OpenAiApiUrl = "https://api.openai.com/v1/chat/completions";

        /// <summary>
        /// GPT Model adı - gpt-3.5-turbo (ucuz ve yeterli)          
        /// </summary>
        private const string ModelName = "gpt-3.5-turbo";

        /// <summary>
        /// Constructor - HttpClient ve Configuration inject edilir
        /// </summary>
        public OpenAiClient(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAiClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// OpenAI API'ye istek gönderip cevap alır
        /// 
        /// Parametreler:
        /// - prompt: Kullanıcının isteği
        /// - maxTokens: Maksimum cevap uzunluğu (varsayılan: 2000)
        /// - temperature: Cevapların yaratıcılık seviyesi 0-1 arası (varsayılan: 0.7)
        /// 
        /// Dönüş:
        /// OpenAI tarafından üretilen metin cevap
        /// </summary>
        public async Task<string> CreateCompletionAsync(
            string prompt, 
            int maxTokens = 2000, 
            double temperature = 0.7)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    _logger.LogWarning("OpenAI: Boş prompt gönderildi");
                    throw new ArgumentException("Prompt boş olamaz", nameof(prompt));
                }

                // API Key'i configuration'dan al
                var apiKey = _configuration["AiSettings:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "your-openai-api-key-here")
                {
                    _logger.LogError("OpenAI API Key yapılandırılmamış");
                    throw new InvalidOperationException("OpenAI API Key appsettings.json'da tanımlanmadı");
                }

                // Request body oluştur
                var requestBody = new
                {
                    model = ModelName,
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "Sen bir fitness ve sağlık danışmanısın. Kişiselleştirilmiş, profesyonel " +
                                      "ve doğru fitness önerileri sağlarsın. Türkçe cevap ver."
                        },
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    max_tokens = maxTokens,
                    temperature = temperature
                };

                // Request gönderi
                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Authorization header ekle
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                _logger.LogInformation($"OpenAI API'ye istek gönderiliyor (Model: {ModelName}, MaxTokens: {maxTokens})");

                // API çağrısını yap
                var response = await _httpClient.PostAsync(OpenAiApiUrl, content);

                // Yanıtı kontrol et
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"OpenAI API Hatası ({response.StatusCode}): {errorContent}");
                    throw new HttpRequestException($"OpenAI API error: {response.StatusCode} - {errorContent}");
                }

                // Cevabı parse et
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseContent);

                // Metin cevabı çıkar
                var textResponse = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString();

                if (string.IsNullOrEmpty(textResponse))
                {
                    _logger.LogWarning("OpenAI: Boş cevap alındı");
                    throw new InvalidOperationException("OpenAI boş cevap döndürdü");
                }

                _logger.LogInformation("OpenAI API'den başarıyla cevap alındı");
                return textResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"OpenAI API HTTP hatası: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"OpenAI yanıtı parse hatası: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"OpenAI completion hatası: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Fitness tavsiyesi için özel prompt oluşturur
        /// </summary>
        public async Task<string> GetFitnessTavsiesiAsync(int boy, int agirlik, string cinsiyet, string hedef)
        {
            var prompt = $@"
Lütfen {cinsiyet} bir kişi için aşağıdaki bilgilere dayanarak detaylı bir fitness antrenman planı oluştur:

- Boy: {boy} cm
- Ağırlık: {agirlik} kg
- Fitness Hedefi: {hedef}

Plan şunları içermeli:
1. Haftalık antrenman günleri (Pazartan Pazara)
2. Her gün için egzersizlerin listesi
3. Her egzersiz için set ve tekrar sayıları
4. Dinlenme günleri
5. Öneriler ve notlar

Cevap Türkçe olmalı ve profesyonel bir antrenman planı gibi biçimlendirilmeli.
";

            return await CreateCompletionAsync(prompt);
        }

        /// <summary>
        /// Diyet tavsiyesi için özel prompt oluşturur
        /// </summary>
        public async Task<string> GetDiyetTavsiesiAsync(int boy, int agirlik, string cinsiyet, string hedef)
        {
            var prompt = $@"
Lütfen {cinsiyet} bir kişi için aşağıdaki bilgilere dayanarak detaylı bir diyet planı oluştur:

- Boy: {boy} cm
- Ağırlık: {agirlik} kg
- Fitness Hedefi: {hedef}

Plan şunları içermeli:
1. Günlük kalori hedefi
2. Makro nutrient dağılımı (Protein, Karbonhidrat, Yağ)
3. Günlük beslenme örneği (Kahvaltı, Ara Öğün, Öğlen, Akşam vb.)
4. Alışveriş listesi
5. Beslenme ipuçları ve öneriler

Cevap Türkçe olmalı ve profesyonel bir diyet planı gibi biçimlendirilmeli.
";

            return await CreateCompletionAsync(prompt);
        }

        /// <summary>
        /// Vücut tipi analizi için özel prompt oluşturur
        /// </summary>
        public async Task<string> GetVucutTipiAnaliziAsync(int boy, int agirlik, string cinsiyet)
        {
            var boyMetre = boy / 100.0;
            var bmi = agirlik / (boyMetre * boyMetre);

            var prompt = $@"
Lütfen aşağıdaki bilgilere dayanarak {cinsiyet} bir kişinin vücut tipi analizini yap:

- Boy: {boy} cm
- Ağırlık: {agirlik} kg
- BMI: {bmi:F1}

Analiz şunları içermeli:
1. Vücut tipi kategorisi (Ektomorf, Mezomorf, Endomorf)
2. BMI ve sağlık durumu yorumu
3. Vücut tipi açıklaması ve özelikleri
4. Kişiselleştirilmiş fitness önerileri
5. Beslenme önerileri
6. Hedef ağırlık ve gelişim stratejileri

Cevap Türkçe olmalı ve profesyonel bir tıbbi analiz gibi biçimlendirilmeli.
";

            return await CreateCompletionAsync(prompt);
        }
    }
}
