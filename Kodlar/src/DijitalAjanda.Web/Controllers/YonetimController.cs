using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Web.Data;
using DijitalAjanda.Web.Models.ViewModels;

namespace DijitalAjanda.Web.Controllers;

// Yalnızca Admin rolü; diğer kullanıcılar Erişim engellendi sayfasına yönlenir
[Authorize(Roles = Roller.Admin)]
public class YonetimController : Controller
{
    public IActionResult Index()
    {
        var model = new YakindaViewModel
        {
            Baslik = "Yönetim Paneli",
            Ikon = "bi-shield-check",
            PlanlananHafta = 8,
            Aciklama = "Portal içeriğinin ve kullanıcıların yönetildiği yönetici paneli.",
            Ozellikler = new()
            {
                "Özet kartları",
                "Kullanıcı yönetimi: listeleme, pasifleştirme, rol atama",
                "İletişim mesajları",
                "SSS yönetimi"
            }
        };
        return View("Yakinda", model);
    }
}
