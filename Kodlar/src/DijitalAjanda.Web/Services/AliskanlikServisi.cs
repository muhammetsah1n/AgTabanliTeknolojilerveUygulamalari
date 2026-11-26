using DijitalAjanda.Web.Data;
using DijitalAjanda.Web.Models.Entities;
using DijitalAjanda.Web.Models.ViewModels.Aliskanliklar;
using DijitalAjanda.Web.Models.ViewModels.Bugun;
using Microsoft.EntityFrameworkCore;

namespace DijitalAjanda.Web.Services;

public class AliskanlikServisi : IAliskanlikServisi
{
    private readonly AjandaDbContext _db;

    public AliskanlikServisi(AjandaDbContext db)
    {
        _db = db;
    }

    public async Task<AliskanliklarViewModel> SayfaGetirAsync(string kullaniciId, DateOnly bugun)
    {
        var haftaBasi = TurkceTarih.HaftaBasi(bugun);

        // Seri hesabı bütün geçmişe bakar; alışkanlık başına günde tek kayıt olduğundan veri küçüktür
        var aliskanliklar = await _db.Aliskanliklar
            .AsNoTracking()
            .Where(a => a.KullaniciId == kullaniciId)
            .OrderBy(a => a.Id)
            .Select(a => new
            {
                a.Id,
                a.Ad,
                a.HaftalikHedef,
                a.Renk,
                Gunler = a.Kayitlar.Select(k => k.Tarih).ToList()
            })
            .ToListAsync();

        return new AliskanliklarViewModel
        {
            Bugun = bugun,
            Gunler = Enumerable.Range(0, 7).Select(i =>
            {
                var tarih = haftaBasi.AddDays(i);
                return new IzgaraGunu
                {
                    Tarih = tarih,
                    KisaAd = TurkceTarih.KisaGunler[i],
                    BugunMu = tarih == bugun,
                    GelecekMi = tarih > bugun
                };
            }).ToList(),
            Satirlar = aliskanliklar.Select(a =>
            {
                var renk = AliskanlikRenkleri.Bul(a.Renk);
                return new AliskanlikSatiri
                {
                    Id = a.Id,
                    Ad = a.Ad,
                    RenkKodu = renk.Kod,
                    RenkSinifi = renk.Sinif,
                    HaftalikHedef = a.HaftalikHedef,
                    IsaretliGunler = a.Gunler.Where(g => g >= haftaBasi && g <= haftaBasi.AddDays(6)).ToHashSet(),
                    HaftadaYapilan = SeriHesaplayici.HaftadakiGunSayisi(a.Gunler, bugun),
                    Seri = SeriHesaplayici.GuncelSeri(a.Gunler, bugun),
                    EnUzunSeri = SeriHesaplayici.EnUzunSeri(a.Gunler)
                };
            }).ToList()
        };
    }

    public Task<Aliskanlik?> GetirAsync(string kullaniciId, int id) =>
        _db.Aliskanliklar.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id && a.KullaniciId == kullaniciId);

    public async Task<int> EkleAsync(string kullaniciId, string ad, int haftalikHedef, string renk, DateOnly baslangic)
    {
        var aliskanlik = new Aliskanlik
        {
            KullaniciId = kullaniciId,
            Ad = ad.Trim(),
            HaftalikHedef = Math.Clamp(haftalikHedef, 1, 7),
            Renk = AliskanlikRenkleri.Bul(renk).Kod,
            BaslangicTarihi = baslangic
        };
        _db.Aliskanliklar.Add(aliskanlik);
        await _db.SaveChangesAsync();
        return aliskanlik.Id;
    }

    public async Task<bool> GuncelleAsync(string kullaniciId, int id, string ad, int haftalikHedef, string renk)
    {
        var aliskanlik = await _db.Aliskanliklar.FirstOrDefaultAsync(a => a.Id == id && a.KullaniciId == kullaniciId);
        if (aliskanlik is null) return false;

        aliskanlik.Ad = ad.Trim();
        aliskanlik.HaftalikHedef = Math.Clamp(haftalikHedef, 1, 7);
        aliskanlik.Renk = AliskanlikRenkleri.Bul(renk).Kod;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SilAsync(string kullaniciId, int id)
    {
        // İşaretlenen günler (AliskanlikKayitlari) veritabanında cascade ile silinir
        var silinen = await _db.Aliskanliklar
            .Where(a => a.Id == id && a.KullaniciId == kullaniciId)
            .ExecuteDeleteAsync();
        return silinen > 0;
    }

    public async Task<AliskanlikDurumu?> IsaretDegistirAsync(string kullaniciId, int id, DateOnly tarih, DateOnly bugun)
    {
        if (tarih > bugun) return null;

        var aliskanlik = await _db.Aliskanliklar
            .Include(a => a.Kayitlar)
            .FirstOrDefaultAsync(a => a.Id == id && a.KullaniciId == kullaniciId);
        if (aliskanlik is null) return null;

        var kayit = aliskanlik.Kayitlar.FirstOrDefault(k => k.Tarih == tarih);
        if (kayit is null)
        {
            aliskanlik.Kayitlar.Add(new AliskanlikKaydi { Tarih = tarih });
        }
        else
        {
            _db.AliskanlikKayitlari.Remove(kayit);
            aliskanlik.Kayitlar.Remove(kayit);
        }
        await _db.SaveChangesAsync();

        var gunler = aliskanlik.Kayitlar.Select(k => k.Tarih).ToList();
        return new AliskanlikDurumu
        {
            Isaretli = kayit is null,
            HaftalikHedef = aliskanlik.HaftalikHedef,
            HaftadaYapilan = SeriHesaplayici.HaftadakiGunSayisi(gunler, tarih),
            Seri = SeriHesaplayici.GuncelSeri(gunler, bugun),
            EnUzunSeri = SeriHesaplayici.EnUzunSeri(gunler)
        };
    }
}
