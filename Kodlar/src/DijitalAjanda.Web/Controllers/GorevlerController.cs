using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Web.Models.ViewModels;

namespace DijitalAjanda.Web.Controllers;

public class GorevlerController : Controller
{
    public IActionResult Index()
    {
        var model = new YakindaViewModel
        {
            Baslik = "Görevler ve Etkinlikler",
            Ikon = "bi-kanban",
            PlanlananHafta = 4,
            Aciklama = "Görev ve etkinliklerin kategorilere göre yönetildiği sayfa.",
            Ozellikler = new()
            {
                "Görev, etkinlik ve kategori ekleme / düzenleme / silme",
                "Tablo görünümü, durum ve kategori filtreleri",
                "Kanban panosu ve sürükle-bırak (5. hafta)",
                "Alt görevler ve ilerleme çubukları (5. hafta)"
            }
        };
        return View("Yakinda", model);
    }
}
