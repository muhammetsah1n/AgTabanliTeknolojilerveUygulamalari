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
            new() { Baslik = "Haftalık Planlayıcı", Ikon = "bi-calendar-week", RenkSinifi = "renk-is",
                    Aciklama = "Toplantı, randevu ve son tarihlerini Pazartesi–Pazar düzeninde tek ekranda gör." },
            new() { Baslik = "Görev ve Etkinlik Takibi", Ikon = "bi-kanban", RenkSinifi = "renk-saglik",
                    Aciklama = "Kategorilere ayır, Kanban panosunda sürükle, ilerlemeyi takip et." },
            new() { Baslik = "Not Sayfaları", Ikon = "bi-journal-text", RenkSinifi = "renk-kisisel",
                    Aciklama = "Kapak görselli, iç içe sayfalar ve sürükle-bırak bloklarla notlarını düzenle." },
            new() { Baslik = "Alışkanlık Takibi", Ikon = "bi-graph-up-arrow", RenkSinifi = "renk-sosyal",
                    Aciklama = "Günlük alışkanlıklarını işaretle, serini ve gelişimini grafiklerle izle." }
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
