using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Web.Models.ViewModels;

namespace DijitalAjanda.Web.Controllers;

[Authorize]
public class BugunController : Controller
{
    public IActionResult Index()
    {
        var model = new YakindaViewModel
        {
            Baslik = "Bugün",
            Ikon = "bi-journal-check",
            PlanlananHafta = 3,
            Aciklama = "Bullet journal düzeninde günün, haftanın ve ayın tek sayfada görüldüğü ana ekran.",
            Ozellikler = new()
            {
                "Gün / hafta / ay görünümleri",
                "Not (–), yapılacak (•) ve etkinlik (○) maddeleri",
                "Günlük tamamlanma yüzdesi ve mini takvim",
                "Yaklaşan etkinlikler"
            }
        };
        return View("Yakinda", model);
    }
}
