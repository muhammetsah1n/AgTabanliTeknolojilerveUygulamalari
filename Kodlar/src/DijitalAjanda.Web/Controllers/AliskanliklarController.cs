using DijitalAjanda.Web.Models.Entities;
using DijitalAjanda.Web.Models.ViewModels.Aliskanliklar;
using DijitalAjanda.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DijitalAjanda.Web.Controllers;

[Authorize]
public class AliskanliklarController : Controller
{
    private readonly IAliskanlikServisi _aliskanlikServisi;
    private readonly UserManager<Kullanici> _kullaniciYonetici;

    public AliskanliklarController(IAliskanlikServisi aliskanlikServisi, UserManager<Kullanici> kullaniciYonetici)
    {
        _aliskanlikServisi = aliskanlikServisi;
        _kullaniciYonetici = kullaniciYonetici;
    }

    private string KullaniciId => _kullaniciYonetici.GetUserId(User)!;

    private static DateOnly Bugun => DateOnly.FromDateTime(DateTime.Now);

    // Sayfadaki JavaScript istekleri bu başlıkla gelir; onlara yönlendirme yerine JSON döner
    private bool ArkaPlanIstegi => Request.Headers.XRequestedWith == "XMLHttpRequest";

    // Haftalık işaretleme ızgarası
    public async Task<IActionResult> Index()
    {
        return View(await _aliskanlikServisi.SayfaGetirAsync(KullaniciId, Bugun));
    }

    // ---------- Yeni / düzenle ----------
    // Listede "Yeni alışkanlık" ve kalem düğmesi formu sayfa içinde açar; JavaScript kapalıysa
    // ya da sunucu doğrulaması başarısız olursa ayrı form sayfası gösterilir.

    [HttpGet]
    public IActionResult Yeni()
    {
        return View("Form", new AliskanlikFormu());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Yeni(AliskanlikFormu form)
    {
        form.Id = null;
        if (!ModelState.IsValid) return View("Form", form);

        await _aliskanlikServisi.EkleAsync(KullaniciId, form.Ad, form.HaftalikHedef, form.Renk, Bugun);
        TempData["AliskanlikMesaj"] = $"“{form.Ad.Trim()}” eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var aliskanlik = await _aliskanlikServisi.GetirAsync(KullaniciId, id);
        if (aliskanlik is null) return NotFound();

        return View("Form", new AliskanlikFormu
        {
            Id = aliskanlik.Id,
            Ad = aliskanlik.Ad,
            HaftalikHedef = aliskanlik.HaftalikHedef,
            Renk = AliskanlikRenkleri.Bul(aliskanlik.Renk).Kod
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, AliskanlikFormu form)
    {
        form.Id = id;

        // Satır içi düzenleme (JavaScript) sonucu JSON olarak alır; sayfa yenilenmez
        if (ArkaPlanIstegi)
        {
            if (!ModelState.IsValid)
            {
                var hata = ModelState.Values.SelectMany(v => v.Errors).Select(h => h.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Bilgiler geçersiz.";
                return BadRequest(new { hata });
            }
            if (!await _aliskanlikServisi.GuncelleAsync(KullaniciId, id, form.Ad, form.HaftalikHedef, form.Renk))
            {
                return NotFound(new { hata = "Alışkanlık bulunamadı." });
            }
            var renk = AliskanlikRenkleri.Bul(form.Renk);
            return Ok(new { ad = form.Ad.Trim(), haftalikHedef = form.HaftalikHedef, renkKodu = renk.Kod, renkSinifi = renk.Sinif });
        }

        if (!ModelState.IsValid) return View("Form", form);

        var bulundu = await _aliskanlikServisi.GuncelleAsync(KullaniciId, id, form.Ad, form.HaftalikHedef, form.Renk);
        if (!bulundu) return NotFound();

        TempData["AliskanlikMesaj"] = "Alışkanlık güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(int id)
    {
        var bulundu = await _aliskanlikServisi.SilAsync(KullaniciId, id);
        if (ArkaPlanIstegi) return bulundu ? Ok(new { }) : NotFound();

        if (bulundu) TempData["AliskanlikMesaj"] = "Alışkanlık silindi.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Izgara ----------

    // Kutuya tıklandığında günü işaretler ya da işareti kaldırır: tarih = 2026-11-26
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Isaretle(int id, DateOnly tarih)
    {
        var durum = await _aliskanlikServisi.IsaretDegistirAsync(KullaniciId, id, tarih, Bugun);

        if (ArkaPlanIstegi)
        {
            if (durum is null) return NotFound(new { hata = "Bu gün işaretlenemez." });
            return Ok(durum);
        }
        return RedirectToAction(nameof(Index));
    }
}
