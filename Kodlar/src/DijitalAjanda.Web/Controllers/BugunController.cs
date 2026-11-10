using DijitalAjanda.Web.Models.Entities;
using DijitalAjanda.Web.Models.ViewModels.Bugun;
using DijitalAjanda.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DijitalAjanda.Web.Controllers;

[Authorize]
public class BugunController : Controller
{
    private readonly IBugunServisi _bugunServisi;
    private readonly IMaddeServisi _maddeServisi;
    private readonly IYapilacakServisi _yapilacakServisi;
    private readonly UserManager<Kullanici> _kullaniciYonetici;

    public BugunController(IBugunServisi bugunServisi, IMaddeServisi maddeServisi, IYapilacakServisi yapilacakServisi,
        UserManager<Kullanici> kullaniciYonetici)
    {
        _bugunServisi = bugunServisi;
        _maddeServisi = maddeServisi;
        _yapilacakServisi = yapilacakServisi;
        _kullaniciYonetici = kullaniciYonetici;
    }

    private string KullaniciId => _kullaniciYonetici.GetUserId(User)!;

    // Sayfadaki JavaScript istekleri bu başlıkla gelir; onlara yönlendirme yerine JSON döner
    private bool ArkaPlanIstegi => Request.Headers.XRequestedWith == "XMLHttpRequest";

    // /Bugun?gorunum=Hafta&tarih=2026-11-02
    public async Task<IActionResult> Index(Gorunum gorunum = Gorunum.Gun, DateOnly? tarih = null)
    {
        var odak = tarih ?? DateOnly.FromDateTime(DateTime.Now);
        var model = await _bugunServisi.SayfaGetirAsync(KullaniciId, gorunum, odak);
        return View(model);
    }

    // Sağdaki takvimde ay değiştirildiğinde: /Bugun/Takvim?ay=2026-12-01
    [HttpGet]
    public async Task<IActionResult> Takvim(DateOnly ay)
    {
        return Json(await _bugunServisi.TakvimAyiGetirAsync(KullaniciId, ay));
    }

    // ---------- Notlar ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NotEkle(YeniNotFormu form)
    {
        int? id = null;
        if (ModelState.IsValid)
        {
            id = await _maddeServisi.NotEkleAsync(KullaniciId, form.Tarih, form.Metin);
        }
        return Sonuc(form, new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NotGuncelle(NotGuncelleFormu form)
    {
        var bulundu = ModelState.IsValid && await _maddeServisi.NotGuncelleAsync(KullaniciId, form.Id, form.Metin);
        return Sonuc(form, bulundu: bulundu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NotSil(MaddeSilFormu form)
    {
        var bulundu = await _maddeServisi.NotSilAsync(KullaniciId, form.Id);
        return Sonuc(form, bulundu: bulundu);
    }

    // ---------- Yapılacaklar ----------
    // Sayfadaki JavaScript yapılacakları /api/yapilacaklar Web API'si ile yönetir.
    // Bu üç işlem yalnızca JavaScript kapalıyken gün sayfasındaki formların çalışması içindir.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YapilacakEkle(YeniYapilacakFormu form)
    {
        if (ModelState.IsValid)
        {
            await _yapilacakServisi.EkleAsync(KullaniciId, form.Tarih, form.Baslik);
        }
        return Sonuc(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YapilacakDegistir(MaddeSilFormu form)
    {
        await _yapilacakServisi.DurumAyarlaAsync(KullaniciId, form.Id, null);
        return Sonuc(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YapilacakSil(MaddeSilFormu form)
    {
        await _yapilacakServisi.SilAsync(KullaniciId, form.Id);
        return Sonuc(form);
    }

    // ---------- Etkinlikler ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EtkinlikEkle(YeniEtkinlikFormu form)
    {
        int? id = null;
        if (ModelState.IsValid)
        {
            var tarih = form.Tarih!.Value;
            id = await _maddeServisi.EtkinlikEkleAsync(KullaniciId, form.Baslik, tarih.ToDateTime(form.Saat!.Value),
                form.Tur, form.BitisSaati is TimeOnly bitis ? tarih.ToDateTime(bitis) : null, form.HatirlaticiDk, form.OzelTur);

            // Gün sayfasından başka bir güne eklendiyse kullanıcıya nereye eklendiği söylenir
            if (tarih != form.Odak && !ArkaPlanIstegi)
            {
                TempData["BugunMesaj"] = $"{TurkceTarih.KisaTarih(tarih)} tarihine eklendi";
            }
        }
        return Sonuc(form, new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EtkinlikGuncelle(EtkinlikGuncelleFormu form)
    {
        var bulundu = ModelState.IsValid && await _maddeServisi.EtkinlikGuncelleAsync(KullaniciId, form.Id, form.Baslik, form.Saat);
        return Sonuc(form, bulundu: bulundu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EtkinlikSil(MaddeSilFormu form)
    {
        var bulundu = await _maddeServisi.EtkinlikSilAsync(KullaniciId, form.Id);
        return Sonuc(form, bulundu: bulundu);
    }

    // Arka plan isteğine JSON, normal form gönderimine aynı görünüme ve tarihe yönlendirme döner
    private IActionResult Sonuc(SayfaKonumu konum, object? veri = null, bool bulundu = true)
    {
        var hata = ModelState.IsValid
            ? null
            : ModelState.Values.SelectMany(v => v.Errors).Select(h => h.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Bilgiler geçersiz.";

        if (ArkaPlanIstegi)
        {
            if (hata is not null) return BadRequest(new { hata });
            if (!bulundu) return NotFound();
            return Ok(veri ?? new { });
        }

        if (hata is not null)
        {
            TempData["BugunHata"] = hata;
        }
        var odak = konum.Odak == default ? DateOnly.FromDateTime(DateTime.Now) : konum.Odak;
        return RedirectToAction(nameof(Index), new { gorunum = konum.Gorunum, tarih = TurkceTarih.Iso(odak) });
    }
}
