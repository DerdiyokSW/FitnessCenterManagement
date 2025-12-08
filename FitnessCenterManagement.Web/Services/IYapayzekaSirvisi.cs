using System.Threading.Tasks;

namespace FitnessCenterManagement.Web.Services
{
    /// <summary>
    /// IYapayzekaSirvisi - Yapay zeka işlemlerini yapan servisin arayüzü
    /// Bağımlılık enjeksiyonu (Dependency Injection) için kullanılır
    /// </summary>
    public interface IYapayzekaSirvisi
    {
        /// <summary>
        /// Fitness tavsiyesi oluşturur
        /// </summary>
        /// <param name="boy">Kullanıcının boyu (cm)</param>
        /// <param name="agirlik">Kullanıcının ağırlığı (kg)</param>
        /// <param name="cinsiyet">Kullanıcının cinsiyeti</param>
        /// <param name="hedef">Fitness hedefi (kilo verme, kas kazanma vb.)</param>
        /// <returns>Yapay zeka tarafından oluşturulan tavsiye metni</returns>
        Task<string> EgzersizTavsiyesiAl(int boy, int agirlik, string cinsiyet, string hedef);

        /// <summary>
        /// Diyet tavsiyesi oluşturur
        /// </summary>
        /// <param name="boy">Kullanıcının boyu (cm)</param>
        /// <param name="agirlik">Kullanıcının ağırlığı (kg)</param>
        /// <param name="cinsiyet">Kullanıcının cinsiyeti</param>
        /// <param name="hedef">Fitness hedefi</param>
        /// <returns>Yapay zeka tarafından oluşturulan diyet tavsiyesi</returns>
        Task<string> DiyetTavsiyesiAl(int boy, int agirlik, string cinsiyet, string hedef);

        /// <summary>
        /// Vücut tipi analizi yapar
        /// </summary>
        /// <param name="boy">Kullanıcının boyu (cm)</param>
        /// <param name="agirlik">Kullanıcının ağırlığı (kg)</param>
        /// <param name="cinsiyet">Kullanıcının cinsiyeti</param>
        /// <returns>Vücut tipi analizi sonucu</returns>
        Task<string> VucutTipiAnaliziYap(int boy, int agirlik, string cinsiyet);
    }
}
