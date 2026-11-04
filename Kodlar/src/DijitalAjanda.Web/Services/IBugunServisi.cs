using DijitalAjanda.Web.Models.ViewModels.Bugun;

namespace DijitalAjanda.Web.Services;

public interface IBugunServisi
{
    Task<BugunViewModel> SayfaGetirAsync(string kullaniciId, Gorunum gorunum, DateOnly odak);

    // Sağdaki takvimin bir ayı: etkinlik olan günler ve resmi tatiller
    Task<TakvimAyi> TakvimAyiGetirAsync(string kullaniciId, DateOnly ay);
}
