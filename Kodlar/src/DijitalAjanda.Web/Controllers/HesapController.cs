using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Web.Models.ViewModels;

namespace DijitalAjanda.Web.Controllers;

public class HesapController : Controller
{
    public IActionResult Giris()
    {
        var model = new YakindaViewModel
        {
            Baslik = "Giriş Yap",
            Ikon = "bi-box-arrow-in-right",
            PlanlananHafta = 2,
            Aciklama = "ASP.NET Core Identity ile güvenli kullanıcı girişi.",
            Ozellikler = new()
            {
                "E-posta ve şifre ile giriş, doğrulamalı form",
                "Beni hatırla seçeneği",
                "Admin ve Kullanıcı rollerine göre farklı menüler"
            }
        };
        return View("Yakinda", model);
    }

    public IActionResult Kayit()
    {
        var model = new YakindaViewModel
        {
            Baslik = "Kayıt Ol",
            Ikon = "bi-person-plus",
            PlanlananHafta = 2,
            Aciklama = "Yeni kullanıcı hesabı oluşturma sayfası.",
            Ozellikler = new()
            {
                "Ad soyad, e-posta ve şifre alanları",
                "Şifre kuralları ve form doğrulama",
                "Yeni hesaba otomatik olarak Kullanıcı rolü atanması"
            }
        };
        return View("Yakinda", model);
    }
}
