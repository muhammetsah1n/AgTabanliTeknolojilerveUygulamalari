using DijitalAjanda.Web.Data;
using DijitalAjanda.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DijitalAjanda.Web.Services;

public class YapilacakServisi : IYapilacakServisi
{
    private readonly AjandaDbContext _db;

    public YapilacakServisi(AjandaDbContext db)
    {
        _db = db;
    }

    public async Task<List<Gorev>> ListeleAsync(string kullaniciId, DateOnly? tarih)
    {
        var sorgu = _db.Gorevler.AsNoTracking().Where(g => g.KullaniciId == kullaniciId);
        if (tarih is DateOnly gun)
        {
            sorgu = sorgu.Where(g => g.SonTarih == gun);
        }
        return await sorgu
            .OrderBy(g => g.SonTarih).ThenBy(g => g.Sira).ThenBy(g => g.Id)
            .ToListAsync();
    }

    public Task<Gorev?> GetirAsync(string kullaniciId, int id) =>
        _db.Gorevler.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == kullaniciId);

    public async Task<Gorev> EkleAsync(string kullaniciId, DateOnly tarih, string baslik)
    {
        var gorev = new Gorev
        {
            KullaniciId = kullaniciId,
            Baslik = baslik.Trim(),
            SonTarih = tarih,
            // O günün listesinin sonuna eklenir
            Sira = await SonrakiSiraAsync(kullaniciId, tarih)
        };
        _db.Gorevler.Add(gorev);
        await _db.SaveChangesAsync();
        return gorev;
    }

    public async Task<Gorev?> GuncelleAsync(string kullaniciId, int id, string baslik, DateOnly tarih, bool tamamlandiMi)
    {
        var gorev = await BulAsync(kullaniciId, id);
        if (gorev is null) return null;

        // Başka güne taşınan madde o günün listesinin sonuna geçer
        if (gorev.SonTarih != tarih)
        {
            gorev.Sira = await SonrakiSiraAsync(kullaniciId, tarih);
            gorev.SonTarih = tarih;
        }
        gorev.Baslik = baslik.Trim();
        gorev.TamamlandiMi = tamamlandiMi;
        await _db.SaveChangesAsync();
        return gorev;
    }

    public async Task<Gorev?> DurumAyarlaAsync(string kullaniciId, int id, bool? tamamlandiMi)
    {
        var gorev = await BulAsync(kullaniciId, id);
        if (gorev is null) return null;

        gorev.TamamlandiMi = tamamlandiMi ?? !gorev.TamamlandiMi;
        await _db.SaveChangesAsync();
        return gorev;
    }

    public async Task<bool> SilAsync(string kullaniciId, int id)
    {
        var silinen = await _db.Gorevler
            .Where(g => g.Id == id && g.KullaniciId == kullaniciId)
            .ExecuteDeleteAsync();
        return silinen > 0;
    }

    private Task<Gorev?> BulAsync(string kullaniciId, int id) =>
        _db.Gorevler.FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == kullaniciId);

    private async Task<int> SonrakiSiraAsync(string kullaniciId, DateOnly tarih) =>
        (await _db.Gorevler
            .Where(g => g.KullaniciId == kullaniciId && g.SonTarih == tarih)
            .MaxAsync(g => (int?)g.Sira) ?? 0) + 1;
}
