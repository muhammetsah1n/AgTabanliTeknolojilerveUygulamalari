namespace DijitalAjanda.Web.Services;

// Bugün sayfasındaki not, yapılacak ve etkinlik maddelerinin eklenmesi, düzenlenmesi ve silinmesi.
// Ekleme işlemleri yeni kaydın numarasını döndürür (sayfa yenilenmeden düzenlenebilsin diye).
// Güncelleme ve silme işlemleri kayıt bulunamazsa ya da başka kullanıcıya aitse false döner.
public interface IMaddeServisi
{
    Task<int> NotEkleAsync(string kullaniciId, DateOnly tarih, string metin);
    Task<bool> NotGuncelleAsync(string kullaniciId, int id, string? metin);
    Task<bool> NotSilAsync(string kullaniciId, int id);

    Task<int> YapilacakEkleAsync(string kullaniciId, DateOnly tarih, string baslik);
    Task<bool> YapilacakGuncelleAsync(string kullaniciId, int id, string baslik);
    Task<bool> YapilacakDurumDegistirAsync(string kullaniciId, int id);
    Task<bool> YapilacakSilAsync(string kullaniciId, int id);

    Task<int> EtkinlikEkleAsync(string kullaniciId, string baslik, DateTime baslangic);
    Task<bool> EtkinlikGuncelleAsync(string kullaniciId, int id, string baslik, TimeOnly? saat);
    Task<bool> EtkinlikSilAsync(string kullaniciId, int id);
}
