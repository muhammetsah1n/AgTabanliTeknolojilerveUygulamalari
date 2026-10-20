using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Web.Models.ViewModels;

namespace DijitalAjanda.Web.Controllers;

public class NotlarController : Controller
{
    public IActionResult Index()
    {
        var model = new YakindaViewModel
        {
            Baslik = "Not Sayfaları",
            Ikon = "bi-journal-text",
            PlanlananHafta = 6,
            Aciklama = "Notion benzeri, iç içe sayfalar ve bloklardan oluşan not alanı.",
            Ozellikler = new()
            {
                "Kenar çubuğunda sayfa hiyerarşisi",
                "Kapak görseli ve sayfa ikonu",
                "Metin, başlık, liste, yapılacak, alıntı ve görsel blokları",
                "Blokları sürükleyerek yeniden sıralama"
            }
        };
        return View("Yakinda", model);
    }
}
