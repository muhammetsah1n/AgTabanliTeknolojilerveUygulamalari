using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Web.Models.ViewModels;

namespace DijitalAjanda.Web.Controllers;

[Authorize]
public class GorevlerController : Controller
{
    public IActionResult Index()
    {
        var model = new YakindaViewModel
        {
            Baslik = "Görevler ve Etkinlikler",
            Ikon = "bi-kanban",
            PlanlananHafta = 4,
            Aciklama = "Görev, etkinlik ve kategorilerin yönetildiği sayfalar.",
            Ozellikler = new()
            {
                "Renkli kategori kartları",
                "Görev ve etkinlik ekleme / düzenleme / silme",
                "Kanban panosu ve sürükle-bırak (5. hafta)",
                "Alt görevler ve ilerleme çubukları (5. hafta)"
            }
        };
        return View("Yakinda", model);
    }
}
