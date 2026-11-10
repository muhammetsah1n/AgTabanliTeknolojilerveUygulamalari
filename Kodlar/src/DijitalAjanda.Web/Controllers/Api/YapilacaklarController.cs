using DijitalAjanda.Web.Models.Api;
using DijitalAjanda.Web.Models.Entities;
using DijitalAjanda.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DijitalAjanda.Web.Controllers.Api;

/// <summary>
/// Bugün sayfasındaki yapılacak maddeleri (• yapılacak, × bitti).
/// Oturum açmış kullanıcının çerezi ile çalışır; her kullanıcı yalnızca kendi maddelerini görür ve değiştirir.
/// </summary>
[ApiController]
[Route("api/yapilacaklar")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class YapilacaklarController : ControllerBase
{
    private readonly IYapilacakServisi _servis;
    private readonly UserManager<Kullanici> _kullaniciYonetici;

    public YapilacaklarController(IYapilacakServisi servis, UserManager<Kullanici> kullaniciYonetici)
    {
        _servis = servis;
        _kullaniciYonetici = kullaniciYonetici;
    }

    private string KullaniciId => _kullaniciYonetici.GetUserId(User)!;

    /// <summary>Yapılacakları listeler.</summary>
    /// <param name="tarih">Verilirse yalnızca o günün maddeleri (yyyy-MM-dd); verilmezse hepsi.</param>
    [HttpGet]
    [ProducesResponseType<List<YapilacakDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<YapilacakDto>>> Listele([FromQuery] DateOnly? tarih)
    {
        var liste = await _servis.ListeleAsync(KullaniciId, tarih);
        return liste.Select(YapilacakDto.Olustur).ToList();
    }

    /// <summary>Tek bir yapılacağı getirir.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<YapilacakDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<YapilacakDto>> Getir(int id)
    {
        var gorev = await _servis.GetirAsync(KullaniciId, id);
        return gorev is null ? Bulunamadi() : YapilacakDto.Olustur(gorev);
    }

    /// <summary>Verilen güne yeni yapılacak ekler; madde o günün listesinin sonuna eklenir.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<YapilacakDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<YapilacakDto>> Ekle(YapilacakEkleIstegi istek)
    {
        var gorev = await _servis.EkleAsync(KullaniciId, istek.Tarih!.Value, istek.Baslik!);
        return CreatedAtAction(nameof(Getir), new { id = gorev.Id }, YapilacakDto.Olustur(gorev));
    }

    /// <summary>Yapılacağın başlığını, gününü ve tamamlanma durumunu birlikte günceller.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<YapilacakDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<YapilacakDto>> Guncelle(int id, YapilacakGuncelleIstegi istek)
    {
        var gorev = await _servis.GuncelleAsync(KullaniciId, id, istek.Baslik!, istek.Tarih!.Value, istek.TamamlandiMi!.Value);
        return gorev is null ? Bulunamadi() : YapilacakDto.Olustur(gorev);
    }

    /// <summary>Yapılacağı siler.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Sil(int id)
    {
        return await _servis.SilAsync(KullaniciId, id) ? NoContent() : Bulunamadi();
    }

    /// <summary>Yapılacağı bitti (×) ya da yapılacak (•) olarak işaretler.</summary>
    /// <remarks>
    /// Gövdede <c>{ "tamamlandiMi": true }</c> gönderilirse o duruma getirilir.
    /// Gövde boş bırakılırsa mevcut durum tersine çevrilir.
    /// </remarks>
    [HttpPatch("{id:int}/tamamla")]
    [ProducesResponseType<YapilacakDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<YapilacakDto>> Tamamla(
        int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] YapilacakDurumIstegi? istek)
    {
        var gorev = await _servis.DurumAyarlaAsync(KullaniciId, id, istek?.TamamlandiMi);
        return gorev is null ? Bulunamadi() : YapilacakDto.Olustur(gorev);
    }

    private ObjectResult Bulunamadi() =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "Yapılacak bulunamadı.");
}
