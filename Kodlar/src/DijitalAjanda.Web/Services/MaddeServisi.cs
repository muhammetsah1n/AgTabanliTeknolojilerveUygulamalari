using DijitalAjanda.Web.Data;
using DijitalAjanda.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DijitalAjanda.Web.Services;

public class MaddeServisi : IMaddeServisi
{
    private readonly AjandaDbContext _db;

    public MaddeServisi(AjandaDbContext db)
    {
        _db = db;
    }

    // ---------- Notlar ----------

    public async Task<int> NotEkleAsync(string kullaniciId, DateOnly tarih, string metin)
    {
        var sonSira = await _db.Notlar
            .Where(n => n.KullaniciId == kullaniciId && n.Tarih == tarih)
            .MaxAsync(n => (int?)n.Sira) ?? 0;

        var not = new Not
        {
            KullaniciId = kullaniciId,
            Tarih = tarih,
            Metin = metin.Trim(),
            Sira = sonSira + 1
        };
        _db.Notlar.Add(not);
        await _db.SaveChangesAsync();

        // Sayfa yenilenmeden sonraki düzenlemelerde kullanılmak üzere yeni notun numarası döner
        return not.Id;
    }

    public async Task<bool> NotGuncelleAsync(string kullaniciId, int id, string? metin)
    {
        var not = await _db.Notlar.FirstOrDefaultAsync(n => n.Id == id && n.KullaniciId == kullaniciId);
        if (not is null) return false;

        if (string.IsNullOrWhiteSpace(metin))
        {
            _db.Notlar.Remove(not);
        }
        else
        {
            not.Metin = metin.Trim();
        }
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> NotSilAsync(string kullaniciId, int id)
    {
        var silinen = await _db.Notlar
            .Where(n => n.Id == id && n.KullaniciId == kullaniciId)
            .ExecuteDeleteAsync();
        return silinen > 0;
    }

    // ---------- Yapılacaklar ----------

    public async Task<int> YapilacakEkleAsync(string kullaniciId, DateOnly tarih, string baslik)
    {
        // O günün listesinin sonuna eklenir
        var sonSira = await _db.Gorevler
            .Where(g => g.KullaniciId == kullaniciId && g.SonTarih == tarih)
            .MaxAsync(g => (int?)g.Sira) ?? 0;

        var gorev = new Gorev
        {
            KullaniciId = kullaniciId,
            Baslik = baslik.Trim(),
            SonTarih = tarih,
            Sira = sonSira + 1
        };
        _db.Gorevler.Add(gorev);
        await _db.SaveChangesAsync();
        return gorev.Id;
    }

    public async Task<bool> YapilacakGuncelleAsync(string kullaniciId, int id, string baslik)
    {
        var gorev = await _db.Gorevler.FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == kullaniciId);
        if (gorev is null) return false;

        gorev.Baslik = baslik.Trim();
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> YapilacakDurumDegistirAsync(string kullaniciId, int id)
    {
        var gorev = await _db.Gorevler.FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == kullaniciId);
        if (gorev is null) return false;

        // • yapılacak ↔ × bitti
        gorev.TamamlandiMi = !gorev.TamamlandiMi;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> YapilacakSilAsync(string kullaniciId, int id)
    {
        var silinen = await _db.Gorevler
            .Where(g => g.Id == id && g.KullaniciId == kullaniciId)
            .ExecuteDeleteAsync();
        return silinen > 0;
    }

    // ---------- Etkinlikler ----------

    public async Task<int> EtkinlikEkleAsync(string kullaniciId, string baslik, DateTime baslangic)
    {
        var etkinlik = new Etkinlik
        {
            KullaniciId = kullaniciId,
            Baslik = baslik.Trim(),
            Tur = EtkinlikTuru.Etkinlik,
            Baslangic = baslangic
        };
        _db.Etkinlikler.Add(etkinlik);
        await _db.SaveChangesAsync();
        return etkinlik.Id;
    }

    public async Task<bool> EtkinlikGuncelleAsync(string kullaniciId, int id, string baslik, TimeOnly? saat)
    {
        var etkinlik = await _db.Etkinlikler.FirstOrDefaultAsync(e => e.Id == id && e.KullaniciId == kullaniciId);
        if (etkinlik is null) return false;

        if (saat is TimeOnly yeniSaat)
        {
            // Gün aynı kalır, yalnızca saat değişir; bitiş varsa süre korunur
            var yeniBaslangic = DateOnly.FromDateTime(etkinlik.Baslangic).ToDateTime(yeniSaat);
            if (etkinlik.Bitis.HasValue)
            {
                etkinlik.Bitis = etkinlik.Bitis.Value + (yeniBaslangic - etkinlik.Baslangic);
            }
            etkinlik.Baslangic = yeniBaslangic;
        }

        etkinlik.Baslik = baslik.Trim();
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EtkinlikSilAsync(string kullaniciId, int id)
    {
        var silinen = await _db.Etkinlikler
            .Where(e => e.Id == id && e.KullaniciId == kullaniciId)
            .ExecuteDeleteAsync();
        return silinen > 0;
    }
}
