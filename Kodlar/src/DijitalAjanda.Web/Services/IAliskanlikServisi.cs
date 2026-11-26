using DijitalAjanda.Web.Models.Entities;
using DijitalAjanda.Web.Models.ViewModels.Aliskanliklar;

namespace DijitalAjanda.Web.Services;

// Alışkanlıklar ve haftalık işaretleme ızgarası. Her işlem yalnızca kullanıcının kendi
// alışkanlıklarına uygulanır; kayıt bulunamazsa ya da başkasına aitse null / false döner.
public interface IAliskanlikServisi
{
    // bugun: ızgaranın haftası ve seri hesabı için kullanılan gün
    Task<AliskanliklarViewModel> SayfaGetirAsync(string kullaniciId, DateOnly bugun);

    Task<Aliskanlik?> GetirAsync(string kullaniciId, int id);
    Task<int> EkleAsync(string kullaniciId, string ad, int haftalikHedef, string renk, DateOnly baslangic);
    Task<bool> GuncelleAsync(string kullaniciId, int id, string ad, int haftalikHedef, string renk);
    Task<bool> SilAsync(string kullaniciId, int id);

    // Günün kutusunu işaretler ya da işareti kaldırır. Gelecekteki günler işaretlenemez (null döner).
    Task<AliskanlikDurumu?> IsaretDegistirAsync(string kullaniciId, int id, DateOnly tarih, DateOnly bugun);
}
