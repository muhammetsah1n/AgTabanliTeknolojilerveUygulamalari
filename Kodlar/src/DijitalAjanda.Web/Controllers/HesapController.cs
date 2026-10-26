using System.Text.Encodings.Web;
using DijitalAjanda.Web.Data;
using DijitalAjanda.Web.Models.Entities;
using DijitalAjanda.Web.Models.ViewModels.Hesap;
using DijitalAjanda.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DijitalAjanda.Web.Controllers;

public class HesapController : Controller
{
    private readonly UserManager<Kullanici> _kullaniciYonetici;
    private readonly SignInManager<Kullanici> _girisYonetici;
    private readonly IEpostaGonderici _eposta;
    private readonly ILogger<HesapController> _logger;

    public HesapController(
        UserManager<Kullanici> kullaniciYonetici,
        SignInManager<Kullanici> girisYonetici,
        IEpostaGonderici eposta,
        ILogger<HesapController> logger)
    {
        _kullaniciYonetici = kullaniciYonetici;
        _girisYonetici = girisYonetici;
        _eposta = eposta;
        _logger = logger;
    }

    // ---------- Giriş ----------

    [HttpGet]
    public IActionResult Giris(GirisTuru tur = GirisTuru.Kullanici, string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return AnaSayfayaYonlendir(User.IsInRole(Roller.Admin));
        }

        return View(new GirisViewModel { Tur = tur, ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(GirisViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var kullanici = await _kullaniciYonetici.FindByEmailAsync(model.Email.Trim());
        if (kullanici is null)
        {
            ModelState.AddModelError(string.Empty, HataliGirisMesaji(model));
            return View(model);
        }

        if (!kullanici.AktifMi)
        {
            ModelState.AddModelError(string.Empty, "Hesabın yönetici tarafından pasifleştirilmiş.");
            return View(model);
        }

        // Önce şifre kontrol edilir; rol bilgisi şifreyi bilmeyen birine gösterilmez
        var sonuc = await _girisYonetici.CheckPasswordSignInAsync(kullanici, model.Sifre, lockoutOnFailure: true);
        if (sonuc.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Çok fazla hatalı deneme yapıldı. 5 dakika sonra tekrar dene.");
            return View(model);
        }
        if (!sonuc.Succeeded)
        {
            ModelState.AddModelError(string.Empty, HataliGirisMesaji(model));
            return View(model);
        }

        var adminMi = await _kullaniciYonetici.IsInRoleAsync(kullanici, Roller.Admin);
        if (model.YoneticiMi && !adminMi)
        {
            ModelState.AddModelError(string.Empty, "Yönetici e-postası veya şifresi hatalı.");
            return View(model);
        }
        if (!model.YoneticiMi && adminMi)
        {
            ModelState.AddModelError(nameof(model.Email), "Bu bir yönetici hesabı; “Yönetici girişi” sekmesini seç.");
            return View(model);
        }

        await _girisYonetici.SignInAsync(kullanici, isPersistent: model.BeniHatirla);

        kullanici.SonGirisTarihi = DateTime.UtcNow;
        await _kullaniciYonetici.UpdateAsync(kullanici);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }
        return AnaSayfayaYonlendir(adminMi);
    }

    // ---------- Kayıt ----------

    [HttpGet]
    public IActionResult Kayit()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return AnaSayfayaYonlendir(User.IsInRole(Roller.Admin));
        }

        return View(new KayitViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Kayit(KayitViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();
        var kullanici = new Kullanici
        {
            UserName = email,
            Email = email,
            AdSoyad = model.AdSoyad.Trim(),
            KayitTarihi = DateTime.UtcNow,
            SonGirisTarihi = DateTime.UtcNow
        };

        var sonuc = await _kullaniciYonetici.CreateAsync(kullanici, model.Sifre);
        if (!sonuc.Succeeded)
        {
            foreach (var hata in sonuc.Errors)
            {
                // Aynı e-posta için hem e-posta hem kullanıcı adı hatası döner, biri yeterli
                if (hata.Code == nameof(IdentityErrorDescriber.DuplicateUserName)) continue;
                var alan = hata.Code.StartsWith("Password") ? nameof(model.Sifre) : nameof(model.Email);
                ModelState.AddModelError(alan, hata.Description);
            }
            return View(model);
        }

        // Yeni hesaplar her zaman Kullanıcı rolüyle başlar
        await _kullaniciYonetici.AddToRoleAsync(kullanici, Roller.Kullanici);
        await _girisYonetici.SignInAsync(kullanici, isPersistent: false);

        _logger.LogInformation("Yeni kullanıcı kaydı: {Email}", email);
        return AnaSayfayaYonlendir(adminMi: false);
    }

    // ---------- Çıkış ----------

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cikis()
    {
        await _girisYonetici.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ---------- Şifremi unuttum (3 adım) ----------

    [HttpGet]
    public IActionResult SifremiUnuttum()
    {
        return View(new SifremiUnuttumViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SifremiUnuttum(SifremiUnuttumViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();
        await SifirlamaKoduGonderAsync(email);

        // Hesap olsun ya da olmasın aynı ekrana geçilir; böylece kayıtlı e-postalar tahmin edilemez
        return RedirectToAction(nameof(SifreSifirla), new { email });
    }

    [HttpGet]
    public IActionResult SifreSifirla(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction(nameof(SifremiUnuttum));
        }

        ViewBag.KodTekrarGonderildi = TempData["KodTekrarGonderildi"] is true;
        return View(new SifreSifirlaViewModel { Email = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SifreSifirla(SifreSifirlaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var kullanici = await _kullaniciYonetici.FindByEmailAsync(model.Email.Trim());
        if (kullanici is null)
        {
            ModelState.AddModelError(nameof(model.Kod), "Kod hatalı ya da süresi dolmuş.");
            return View(model);
        }

        var sonuc = await _kullaniciYonetici.ResetPasswordAsync(kullanici, model.Kod, model.YeniSifre);
        if (!sonuc.Succeeded)
        {
            foreach (var hata in sonuc.Errors)
            {
                var alan = hata.Code.StartsWith("Password") ? nameof(model.YeniSifre) : nameof(model.Kod);
                ModelState.AddModelError(alan, hata.Description);
            }
            return View(model);
        }

        // Çok sayıda hatalı denemeden sonra kilitlendiyse şifre yenilenince kilit kalkar
        await _kullaniciYonetici.SetLockoutEndDateAsync(kullanici, null);
        await _kullaniciYonetici.ResetAccessFailedCountAsync(kullanici);

        return RedirectToAction(nameof(SifreYenilendi));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KoduTekrarGonder(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction(nameof(SifremiUnuttum));
        }

        await SifirlamaKoduGonderAsync(email.Trim());
        TempData["KodTekrarGonderildi"] = true;
        return RedirectToAction(nameof(SifreSifirla), new { email });
    }

    [HttpGet]
    public IActionResult SifreYenilendi()
    {
        return View();
    }

    // ---------- Erişim engellendi ----------

    [HttpGet]
    public IActionResult ErisimEngellendi()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    // ---------- Yardımcılar ----------

    private async Task SifirlamaKoduGonderAsync(string email)
    {
        var kullanici = await _kullaniciYonetici.FindByEmailAsync(email);
        if (kullanici is null || !kullanici.AktifMi)
        {
            _logger.LogInformation("Şifre sıfırlama istendi ama {Email} için aktif hesap yok.", email);
            return;
        }

        // DefaultEmailProvider, güvenlik damgasından türetilen 6 haneli bir kod üretir
        var kod = await _kullaniciYonetici.GeneratePasswordResetTokenAsync(kullanici);
        var ad = HtmlEncoder.Default.Encode(kullanici.AdSoyad);

        var icerik = $"""
            <p>Merhaba {ad},</p>
            <p>Dijital Ajanda hesabının şifresini sıfırlamak için kodun:</p>
            <p style="font-size:28px;font-weight:700;letter-spacing:6px">{kod}</p>
            <p>Kod birkaç dakika içinde geçerliliğini yitirir. Bu isteği sen yapmadıysan bu e-postayı yok sayabilirsin.</p>
            """;

        try
        {
            await _eposta.GonderAsync(email, "Dijital Ajanda – şifre sıfırlama kodu", icerik);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şifre sıfırlama e-postası gönderilemedi: {Email}", email);
        }
    }

    private static string HataliGirisMesaji(GirisViewModel model) =>
        model.YoneticiMi ? "Yönetici e-postası veya şifresi hatalı." : "E-posta veya şifre hatalı.";

    private IActionResult AnaSayfayaYonlendir(bool adminMi) =>
        adminMi ? RedirectToAction("Index", "Yonetim") : RedirectToAction("Index", "Bugun");
}
