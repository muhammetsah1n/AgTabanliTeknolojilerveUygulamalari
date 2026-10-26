using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Web.Models.ViewModels;

namespace DijitalAjanda.Web.Controllers;

[Authorize]
public class AliskanliklarController : Controller
{
    public IActionResult Index()
    {
        var model = new YakindaViewModel
        {
            Baslik = "Alışkanlıklar",
            Ikon = "bi-graph-up-arrow",
            PlanlananHafta = 6,
            Aciklama = "Alışkanlıkların ve günlük su tüketiminin takip edildiği sayfa.",
            Ozellikler = new()
            {
                "Haftalık işaretleme ızgarası ve haftalık hedef",
                "Seri (streak) ve en uzun seri sayacı",
                "Su takibi: bardak ekle / geri al, günlük hedef",
                "Son 7 günün grafiği"
            }
        };
        return View("Yakinda", model);
    }
}
