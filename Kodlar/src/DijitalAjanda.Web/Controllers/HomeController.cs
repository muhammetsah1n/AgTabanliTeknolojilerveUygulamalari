using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Web.Models;
using DijitalAjanda.Web.Models.ViewModels;

namespace DijitalAjanda.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var ozellikler = new List<OzellikKarti>
        {
            new() { Baslik = "Bugün Sayfası", Ikon = "bi-journal-check", RenkSinifi = "renk-is",
                    Aciklama = "Gününü bullet journal gibi yaz; haftanı ve ayını aynı defterde gör." },
            new() { Baslik = "Takvim ve Hatırlatıcılar", Ikon = "bi-calendar-event", RenkSinifi = "renk-saglik",
                    Aciklama = "Etkinliklerini saatiyle ekle, resmi tatilleri takvimde gör, zamanı gelince hatırlatılsın." },
            new() { Baslik = "Alışkanlık ve Su Takibi", Ikon = "bi-droplet-half", RenkSinifi = "renk-kisisel",
                    Aciklama = "Alışkanlıklarını işaretle, serini koru, günlük su hedefini bardak bardak doldur." },
            new() { Baslik = "Kitaplık", Ikon = "bi-bookshelf", RenkSinifi = "renk-sosyal",
                    Aciklama = "Okuduğun kitapları ve izlediğin filmleri puanla, renkli rafına diz." }
        };

        return View(ozellikler);
    }

    public IActionResult Gizlilik()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? kod)
    {
        // Durum kodu sayfası yeniden çalıştırıldığında orijinal kod korunur
        if (kod.HasValue)
        {
            Response.StatusCode = kod.Value;
        }

        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            DurumKodu = kod
        });
    }
}
