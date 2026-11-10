using DijitalAjanda.Web.Models.Entities;

namespace DijitalAjanda.Web.Services;

// Bugün sayfasındaki not ve etkinlik maddelerinin eklenmesi, düzenlenmesi ve silinmesi
// (yapılacaklar IYapilacakServisi'nde).
// Ekleme işlemleri yeni kaydın numarasını döndürür (sayfa yenilenmeden düzenlenebilsin diye).
// Güncelleme ve silme işlemleri kayıt bulunamazsa ya da başka kullanıcıya aitse false döner.
public interface IMaddeServisi
{
    Task<int> NotEkleAsync(string kullaniciId, DateOnly tarih, string metin);
    Task<bool> NotGuncelleAsync(string kullaniciId, int id, string? metin);
    Task<bool> NotSilAsync(string kullaniciId, int id);

    // bitis boşsa etkinliğin bitiş saati yok; hatirlaticiDk boşsa hatırlatma yok;
    // ozelTur yalnızca tur = Ozel iken saklanır
    Task<int> EtkinlikEkleAsync(string kullaniciId, string baslik, DateTime baslangic,
        EtkinlikTuru tur = EtkinlikTuru.Etkinlik, DateTime? bitis = null, int? hatirlaticiDk = null, string? ozelTur = null);
    Task<bool> EtkinlikGuncelleAsync(string kullaniciId, int id, string baslik, TimeOnly? saat);
    Task<bool> EtkinlikSilAsync(string kullaniciId, int id);
}
