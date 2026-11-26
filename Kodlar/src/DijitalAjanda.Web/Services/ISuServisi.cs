using DijitalAjanda.Web.Models.ViewModels.Aliskanliklar;

namespace DijitalAjanda.Web.Services;

// Su takibi: kullanıcı başına günde tek kayıt (SuKayitlari), 1 bardak = 250 ml.
// Günlük hedef kullanıcının hesabında (Kullanici.GunlukSuHedefiMl) tutulur.
public interface ISuServisi
{
    Task<SuTakibiViewModel> SayfaGetirAsync(string kullaniciId, DateOnly bugun);

    // Günlük üst sınıra (40 bardak) ulaşıldıysa bardak eklenmez, durum değişmeden döner
    Task<SuDurumu> BardakEkleAsync(string kullaniciId, DateOnly bugun);

    // Son eklenen bardağı geri alır; bardak yoksa durum değişmeden döner
    Task<SuDurumu> GeriAlAsync(string kullaniciId, DateOnly bugun);

    // hedefMl yalnızca SuAyarlari.HedefSecenekleri içinden olabilir; değilse false döner
    Task<bool> HedefKaydetAsync(string kullaniciId, int hedefMl);
}
