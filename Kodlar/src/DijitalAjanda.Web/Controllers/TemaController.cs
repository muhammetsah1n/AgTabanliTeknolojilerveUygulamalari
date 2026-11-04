using DijitalAjanda.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DijitalAjanda.Web.Controllers;

// Açık / koyu tema seçimini kullanıcının hesabına kaydeder; böylece başka cihazda da aynı tema açılır
[Authorize]
public class TemaController : Controller
{
    private readonly UserManager<Kullanici> _kullaniciYonetici;

    public TemaController(UserManager<Kullanici> kullaniciYonetici)
    {
        _kullaniciYonetici = kullaniciYonetici;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Kaydet(Tema tema)
    {
        if (!Enum.IsDefined(tema))
        {
            return BadRequest();
        }

        var kullanici = await _kullaniciYonetici.GetUserAsync(User);
        if (kullanici is null)
        {
            return Unauthorized();
        }

        kullanici.TemaTercihi = tema;
        await _kullaniciYonetici.UpdateAsync(kullanici);
        return NoContent();
    }
}
