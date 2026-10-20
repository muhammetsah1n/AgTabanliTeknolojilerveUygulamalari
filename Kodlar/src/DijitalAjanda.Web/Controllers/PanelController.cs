using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Web.Models.ViewModels;

namespace DijitalAjanda.Web.Controllers;

public class PanelController : Controller
{
    public IActionResult Index()
    {
        var model = new YakindaViewModel
        {
            Baslik = "Panel",
            Ikon = "bi-grid-1x2",
            PlanlananHafta = 3,
            Aciklama = "Günün ve haftanın özetini tek bakışta sunan ana ekran.",
            Ozellikler = new()
            {
                "Pazartesi–Pazar haftalık planlayıcı",
                "Aylık takvim ve resmi tatiller",
                "Bugün yapılacaklar listesi ve tamamlanma yüzdesi",
                "Yaklaşan etkinlikler kartı"
            }
        };
        return View("Yakinda", model);
    }
}
