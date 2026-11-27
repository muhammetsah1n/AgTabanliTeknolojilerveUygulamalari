using DijitalAjanda.Web.Models.Entities;
using DijitalAjanda.Web.Models.ViewModels.Aliskanliklar;
using DijitalAjanda.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DijitalAjanda.Web.Controllers;

// Alışkanlıklar sayfasının "Su takibi" sekmesi
[Authorize]
public class SuTakibiController : Controller
{
    private readonly ISuServisi _suServisi;
    private readonly UserManager<Kullanici> _kullaniciYonetici;

    public SuTakibiController(ISuServisi suServisi, UserManager<Kullanici> kullaniciYonetici)
    {
        _suServisi = suServisi;
        _kullaniciYonetici = kullaniciYonetici;
    }

    private string KullaniciId => _kullaniciYonetici.GetUserId(User)!;

    private static DateOnly Bugun => DateOnly.FromDateTime(DateTime.Now);

    private bool ArkaPlanIstegi => Request.Headers.XRequestedWith == "XMLHttpRequest";

    public async Task<IActionResult> Index()
    {
        return View(await _suServisi.SayfaGetirAsync(KullaniciId, Bugun));
    }

    // "1 bardak (250 ml)" düğmesi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BardakEkle()
    {
        var durum = await _suServisi.BardakEkleAsync(KullaniciId, Bugun);
        return ArkaPlanIstegi ? Ok(durum) : RedirectToAction(nameof(Index));
    }

    // "Geri al" düğmesi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeriAl()
    {
        var durum = await _suServisi.GeriAlAsync(KullaniciId, Bugun);
        return ArkaPlanIstegi ? Ok(durum) : RedirectToAction(nameof(Index));
    }

    // Günlük hedef seçimi (1,5 / 2 / 2,5 / 3 litre)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Hedef(int hedefMl)
    {
        if (await _suServisi.HedefKaydetAsync(KullaniciId, hedefMl))
        {
            TempData["SuMesaj"] = $"Günlük hedef {SuAyarlari.Litre(hedefMl)} litre olarak kaydedildi.";
        }
        else
        {
            TempData["SuHata"] = "Listeden bir hedef seçin.";
        }
        return RedirectToAction(nameof(Index));
    }
}
