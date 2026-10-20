using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Web.Models.ViewModels;

namespace DijitalAjanda.Web.Controllers;

public class AliskanliklarController : Controller
{
    public IActionResult Index()
    {
        var model = new YakindaViewModel
        {
            Baslik = "Alışkanlık Takibi",
            Ikon = "bi-graph-up-arrow",
            PlanlananHafta = 7,
            Aciklama = "Günlük alışkanlıkların işaretlendiği ve gelişimin izlendiği sayfa.",
            Ozellikler = new()
            {
                "Günlük / haftalık işaretleme ızgarası",
                "Seri (streak) sayacı",
                "Chart.js ile haftalık tamamlanma grafiği",
                "Yıllık tutarlılık ısı haritası"
            }
        };
        return View("Yakinda", model);
    }
}
