using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Web.Controllers
{
    /// <summary>
    /// Kullanıcı giriş ve kayıt işlemlerini yönetir
    /// Login (Giriş), Register (Kayıt), Logout (Çıkış) işlemlerini sağlar
    /// </summary>
    [AllowAnonymous] // Bu sayfalara giriş yapmayan kullanıcılar da erişebilir
    public class AccountController : Controller
    {
        // Giriş yapma işlemlerini yönetir
        private readonly SignInManager<IdentityUser> _signInManager;
        // Kullanıcı oluşturma, düzenleme, silme işlemlerini yönetir
        private readonly UserManager<IdentityUser> _userManager;

        /// <summary>
        /// Constructor - Gerekli servisleri başlatır
        /// </summary>
        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Giriş sayfasını gösterir (GET isteği)
        /// </summary>
        /// <param name="returnUrl">Giriş sonrası dönülecek sayfa adresi</param>
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Giriş sonrası nereye dönüleceğini belirt
            ViewData["ReturnUrl"] = returnUrl;
            // Login.cshtml sayfasını göster
            return View();
        }

        /// <summary>
        /// Giriş işlemini gerçekleştirir (POST isteği)
        /// E-posta ve şifre ile kullanıcı kimliğini doğrular
        /// </summary>
        /// <param name="model">E-posta, şifre ve "Beni Hatırla" bilgileri</param>
        /// <param name="returnUrl">Giriş sonrası dönülecek sayfa adresi</param>
        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF saldırılarına karşı koruma
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            // Giriş sonrası nereye dönüleceğini belirt
            ViewData["ReturnUrl"] = returnUrl;
            // Form verilerinin geçerli olup olmadığını kontrol et
            if (ModelState.IsValid)
            {
                // Giriş yap - e-posta, şifre, beni hatırla seçeneği ile
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                // Giriş başarılı ise
                if (result.Succeeded)
                {
                    // Önceki sayfaya dön veya anasayfaya yönlendir
                    return LocalRedirect(returnUrl ?? "/");
                }
                else
                {
                    // Giriş başarısız - hata mesajı göster
                    ModelState.AddModelError(string.Empty, "Giriş başarısız. E-posta veya şifre yanlış.");
                }
            }
            // Hata varsa sayfayı yeniden göster
            return View(model);
        }

        /// <summary>
        /// Kayıt sayfasını gösterir (GET isteği)
        /// </summary>
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // Register.cshtml sayfasını göster
            return View();
        }

        /// <summary>
        /// Yeni kullanıcı hesabı oluşturur (POST isteği)
        /// E-posta ve şifre ile yeni üye kayıt eder
        /// </summary>
        /// <param name="model">E-posta, şifre ve şifre onayı bilgileri</param>
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF saldırılarına karşı koruma
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Form verilerinin geçerli olup olmadığını kontrol et
            if (ModelState.IsValid)
            {
                // Yeni kullanıcı nesnesi oluştur
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                // Şifreyle birlikte veritabanına kaydet
                var result = await _userManager.CreateAsync(user, model.Password);
                // Kullanıcı başarıyla oluşturuldu ise
                if (result.Succeeded)
                {
                    // Yeni kullanıcıya "Uye" (üye) rolünü ver
                    await _userManager.AddToRoleAsync(user, "Uye");
                    // Kullanıcıyı otomatik olarak giriş yaptır
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    // Anasayfaya yönlendir
                    return RedirectToAction("Index", "Home");
                }
                // Hata varsa hata mesajlarını göster
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // Hata varsa sayfayı yeniden göster
            return View(model);
        }

        /// <summary>
        /// Kullanıcıyı sistemden çıkartır (POST isteği)
        /// Oturumu sonlandırır ve anasayfaya yönlendirir
        /// </summary>
        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Kullanıcıyı oturumdan çık
            await _signInManager.SignOutAsync();
            // Anasayfaya yönlendir
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// Giriş için gereken bilgiler (E-posta, Şifre)
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>E-posta adresi (kullanıcı adı olarak da kullanılır)</summary>
        [Required(ErrorMessage = "E-posta alanı gereklidir")]
        [EmailAddress] // Geçerli e-posta formatı kontrolü
        public string? Email { get; set; }

        /// <summary>Kullanıcı şifresi</summary>
        [Required(ErrorMessage = "Şifre alanı gereklidir")]
        [DataType(DataType.Password)] // Şifre alanı olarak işaretler (input type="password")
        public string? Password { get; set; }

        /// <summary>Kullanıcı oturumunu hatırla (30 gün boyunca giriş yapmaya gerek yok)</summary>
        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Yeni üye kaydı için gereken bilgiler (E-posta, Şifre, Şifre Onayı)
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>Kullanıcının e-posta adresi (kullanıcı adı olarak da kullanılır)</summary>
        [Required(ErrorMessage = "E-posta alanı gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin")] // E-posta formatı kontrolü
        public string? Email { get; set; }

        /// <summary>Yeni şifre (En az 6 karakter olmalı)</summary>
        [Required(ErrorMessage = "Şifre alanı gereklidir")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır", MinimumLength = 6)]
        [DataType(DataType.Password)] // Şifre alanı olarak işaretler (input type="password")
        public string? Password { get; set; }

        /// <summary>Şifre onaylaması (Yukarıdaki şifre ile aynı olmalı)</summary>
        [DataType(DataType.Password)] // Şifre alanı olarak işaretler (input type="password")
        [Display(Name = "Şifreyi Onayla")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")] // Password alanıyla karşılaştırma
        public string? ConfirmPassword { get; set; }
    }
}
