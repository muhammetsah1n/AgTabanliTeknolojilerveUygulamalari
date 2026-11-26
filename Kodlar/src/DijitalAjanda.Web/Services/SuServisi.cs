using DijitalAjanda.Web.Data;
using DijitalAjanda.Web.Models.Entities;
using DijitalAjanda.Web.Models.ViewModels.Aliskanliklar;
using DijitalAjanda.Web.Models.ViewModels.Bugun;
using Microsoft.EntityFrameworkCore;

namespace DijitalAjanda.Web.Services;

public class SuServisi : ISuServisi
{
    private readonly AjandaDbContext _db;

    public SuServisi(AjandaDbContext db)
    {
        _db = db;
    }

    public async Task<SuTakibiViewModel> SayfaGetirAsync(string kullaniciId, DateOnly bugun)
    {
        var durum = await DurumGetirAsync(kullaniciId, bugun);
        var ilkGun = bugun.AddDays(-6);

        var kayitlar = await _db.SuKayitlari.AsNoTracking()
            .Where(s => s.KullaniciId == kullaniciId && s.Tarih >= ilkGun && s.Tarih <= bugun)
            .ToDictionaryAsync(s => s.Tarih, s => s.BardakSayisi);

        return new SuTakibiViewModel
        {
            Durum = durum,
            SonYediGun = Enumerable.Range(0, 7).Select(i =>
            {
                var tarih = ilkGun.AddDays(i);
                return new SuGunu
                {
                    Tarih = tarih,
                    KisaAd = TurkceTarih.KisaGunler[TurkceTarih.GunSirasi(tarih)],
                    Ml = kayitlar.GetValueOrDefault(tarih) * SuAyarlari.BardakMl
                };
            }).ToList()
        };
    }

    public async Task<SuDurumu> BardakEkleAsync(string kullaniciId, DateOnly bugun)
    {
        var kayit = await _db.SuKayitlari.FirstOrDefaultAsync(s => s.KullaniciId == kullaniciId && s.Tarih == bugun);
        if (kayit is null)
        {
            _db.SuKayitlari.Add(new SuKaydi { KullaniciId = kullaniciId, Tarih = bugun, BardakSayisi = 1 });
            await _db.SaveChangesAsync();
        }
        else if (kayit.BardakSayisi < SuAyarlari.GunlukEnFazlaBardak)
        {
            kayit.BardakSayisi++;
            await _db.SaveChangesAsync();
        }
        return await DurumGetirAsync(kullaniciId, bugun);
    }

    public async Task<SuDurumu> GeriAlAsync(string kullaniciId, DateOnly bugun)
    {
        var kayit = await _db.SuKayitlari.FirstOrDefaultAsync(s => s.KullaniciId == kullaniciId && s.Tarih == bugun);
        if (kayit is not null)
        {
            // Son bardak da geri alınırsa günün kaydı silinir
            if (kayit.BardakSayisi <= 1)
            {
                _db.SuKayitlari.Remove(kayit);
            }
            else
            {
                kayit.BardakSayisi--;
            }
            await _db.SaveChangesAsync();
        }
        return await DurumGetirAsync(kullaniciId, bugun);
    }

    public async Task<bool> HedefKaydetAsync(string kullaniciId, int hedefMl)
    {
        if (!SuAyarlari.HedefSecenekleri.Contains(hedefMl)) return false;

        var guncellenen = await _db.Users
            .Where(u => u.Id == kullaniciId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.GunlukSuHedefiMl, hedefMl));
        return guncellenen > 0;
    }

    // Bugünkü bardak sayısı, günlük hedef ve hedefe ulaşılan günlerin serisi.
    // Seri, geçmiş günler için de kullanıcının şu anki hedefine göre hesaplanır.
    private async Task<SuDurumu> DurumGetirAsync(string kullaniciId, DateOnly bugun)
    {
        var hedefMl = await _db.Users.Where(u => u.Id == kullaniciId)
            .Select(u => u.GunlukSuHedefiMl).FirstOrDefaultAsync();

        var hedefBardak = (int)Math.Ceiling(hedefMl / (double)SuAyarlari.BardakMl);
        var gunlukMl = await _db.SuKayitlari.AsNoTracking()
            .Where(s => s.KullaniciId == kullaniciId && s.Tarih <= bugun
                        && (s.Tarih == bugun || s.BardakSayisi >= hedefBardak))
            .ToDictionaryAsync(s => s.Tarih, s => s.BardakSayisi * SuAyarlari.BardakMl);

        return new SuDurumu
        {
            Bardak = gunlukMl.GetValueOrDefault(bugun) / SuAyarlari.BardakMl,
            HedefMl = hedefMl,
            Seri = SeriHesaplayici.SuHedefSerisi(gunlukMl, hedefMl, bugun)
        };
    }
}
