using DijitalAjanda.Web.Models.Entities;

namespace DijitalAjanda.Web.Services;

// Bugün sayfasındaki yapılacak maddeleri (Gorevler tablosu). Hem Web API hem de sayfanın
// JavaScript'siz form gönderimleri bu servisi kullanır. Her işlem yalnızca kullanıcının kendi
// kayıtlarına uygulanır; kayıt bulunamazsa ya da başkasına aitse null / false döner.
public interface IYapilacakServisi
{
    // tarih boşsa kullanıcının bütün yapılacakları (tarihe ve sıraya göre)
    Task<List<Gorev>> ListeleAsync(string kullaniciId, DateOnly? tarih);
    Task<Gorev?> GetirAsync(string kullaniciId, int id);
    Task<Gorev> EkleAsync(string kullaniciId, DateOnly tarih, string baslik);
    Task<Gorev?> GuncelleAsync(string kullaniciId, int id, string baslik, DateOnly tarih, bool tamamlandiMi);

    // tamamlandiMi boşsa durum tersine çevrilir (• ↔ ×)
    Task<Gorev?> DurumAyarlaAsync(string kullaniciId, int id, bool? tamamlandiMi);
    Task<bool> SilAsync(string kullaniciId, int id);
}
