using DijitalAjanda.Web.Models.ViewModels.Bugun;

namespace DijitalAjanda.Web.Services;

// Alışkanlık ve su takibindeki seri (streak) hesapları. Veritabanına dokunmaz; yalnızca tarih
// listeleri üzerinde çalışır, bu yüzden birim testleriyle doğrudan denenebilir.
public static class SeriHesaplayici
{
    // Bugüne kadar kesintisiz devam eden seri (gün).
    // Bugün henüz işaretlenmediyse seri bozulmuş sayılmaz: dünden geriye doğru sayılır.
    // Gelecekteki tarihler hesaba katılmaz.
    public static int GuncelSeri(IEnumerable<DateOnly> gunler, DateOnly bugun)
    {
        var kume = gunler.Where(g => g <= bugun).ToHashSet();

        var gun = kume.Contains(bugun) ? bugun : bugun.AddDays(-1);
        var seri = 0;
        while (kume.Contains(gun))
        {
            seri++;
            gun = gun.AddDays(-1);
        }
        return seri;
    }

    // Kayıtlar içindeki en uzun ardışık gün dizisi (gün). Tekrarlanan tarihler bir kez sayılır.
    public static int EnUzunSeri(IEnumerable<DateOnly> gunler)
    {
        var sirali = gunler.Distinct().OrderBy(g => g).ToList();
        if (sirali.Count == 0) return 0;

        int enUzun = 1, mevcut = 1;
        for (var i = 1; i < sirali.Count; i++)
        {
            mevcut = sirali[i].DayNumber - sirali[i - 1].DayNumber == 1 ? mevcut + 1 : 1;
            enUzun = Math.Max(enUzun, mevcut);
        }
        return enUzun;
    }

    // Su takibinde hedefe ulaşılan günlerin bugüne kadar kesintisiz serisi.
    // gunlukMl: tarih → o gün içilen ml. Bugün hedefe henüz ulaşılmadıysa seri dünden sayılır.
    public static int SuHedefSerisi(IReadOnlyDictionary<DateOnly, int> gunlukMl, int hedefMl, DateOnly bugun)
    {
        if (hedefMl <= 0) return 0;
        var ulasilanlar = gunlukMl.Where(k => k.Value >= hedefMl).Select(k => k.Key);
        return GuncelSeri(ulasilanlar, bugun);
    }

    // Verilen günün pazartesi–pazar haftasında işaretlenen gün sayısı
    public static int HaftadakiGunSayisi(IEnumerable<DateOnly> gunler, DateOnly haftaninBiriGunu)
    {
        var pazartesi = TurkceTarih.HaftaBasi(haftaninBiriGunu);
        var pazar = pazartesi.AddDays(6);
        return gunler.Where(g => g >= pazartesi && g <= pazar).Distinct().Count();
    }
}
