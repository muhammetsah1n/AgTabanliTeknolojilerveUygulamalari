using DijitalAjanda.Web.Data;
using DijitalAjanda.Web.Models.Entities;
using DijitalAjanda.Web.Models.ViewModels.Bugun;
using Microsoft.EntityFrameworkCore;

namespace DijitalAjanda.Web.Services;

// Bugün sayfasının gün / hafta / ay görünümleri için gereken bütün veriyi hazırlar
public class BugunServisi : IBugunServisi
{
    private const int YaklasanEtkinlikSayisi = 4;

    private readonly AjandaDbContext _db;

    public BugunServisi(AjandaDbContext db)
    {
        _db = db;
    }

    public async Task<BugunViewModel> SayfaGetirAsync(string kullaniciId, Gorunum gorunum, DateOnly odak)
    {
        var bugun = DateOnly.FromDateTime(DateTime.Now);

        // Görünümün kapsadığı günler: gün için tek gün, hafta için Pzt–Paz, ay için 5–6 haftalık ızgara
        var (ilkGun, sonGun) = gorunum switch
        {
            Gorunum.Hafta => (TurkceTarih.HaftaBasi(odak), TurkceTarih.HaftaBasi(odak).AddDays(6)),
            Gorunum.Ay => AyIzgarasi(odak),
            _ => (odak, odak)
        };

        var maddeler = await MaddeleriGetirAsync(kullaniciId, ilkGun, sonGun);
        var gunlere = maddeler.ToLookup(m => m.Tarih);

        var model = new BugunViewModel
        {
            Ad = await AdGetirAsync(kullaniciId),
            Gorunum = gorunum,
            Odak = odak,
            BugununTarihi = bugun,
            Notlar = gunlere[odak].Where(m => m.Tur == MaddeTuru.Not).ToList(),
            Yapilacaklar = gunlere[odak].Where(m => m.Tur == MaddeTuru.Yapilacak).ToList(),
            Etkinlikler = gunlere[odak].Where(m => m.Tur == MaddeTuru.Etkinlik).ToList(),
            Takvim = await TakvimAyiGetirAsync(kullaniciId, odak),
            YaklasanEtkinlikler = await YaklasanEtkinlikleriGetirAsync(kullaniciId, bugun)
        };

        var bitenSayisi = model.Yapilacaklar.Count(m => m.Bitti);
        model.TamamlanmaYuzdesi = model.Yapilacaklar.Count == 0
            ? 0
            : (int)Math.Round(100.0 * bitenSayisi / model.Yapilacaklar.Count);

        if (gorunum != Gorunum.Gun)
        {
            model.Hucreler = HucreleriOlustur(ilkGun, sonGun, odak, bugun, gunlere);
        }

        (model.GezintiBasligi, model.Onceki, model.Sonraki) = gorunum switch
        {
            Gorunum.Hafta => (TurkceTarih.HaftaBasligi(ilkGun), odak.AddDays(-7), odak.AddDays(7)),
            Gorunum.Ay => (TurkceTarih.AyBasligi(odak), odak.AddMonths(-1), odak.AddMonths(1)),
            _ => (TurkceTarih.UzunTarih(odak), odak.AddDays(-1), odak.AddDays(1))
        };

        return model;
    }

    public async Task<TakvimAyi> TakvimAyiGetirAsync(string kullaniciId, DateOnly ay)
    {
        var (ilkGun, sonGun) = AyIzgarasi(ay);
        var ilkAn = ilkGun.ToDateTime(TimeOnly.MinValue);
        var sonAn = sonGun.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var baslangiclar = await _db.Etkinlikler
            .Where(e => e.KullaniciId == kullaniciId && e.Baslangic >= ilkAn && e.Baslangic < sonAn)
            .Select(e => e.Baslangic)
            .ToListAsync();

        var takvim = new TakvimAyi
        {
            Ay = TurkceTarih.Iso(new DateOnly(ay.Year, ay.Month, 1)),
            EtkinlikGunleri = baslangiclar
                .Select(b => TurkceTarih.Iso(DateOnly.FromDateTime(b)))
                .Distinct()
                .ToList()
        };

        for (var gun = ilkGun; gun <= sonGun; gun = gun.AddDays(1))
        {
            if (TurkceTarih.Tatil(gun) is string tatil)
            {
                takvim.Tatiller.Add(new TatilGunu { Tarih = TurkceTarih.Iso(gun), Ad = tatil });
            }
        }

        return takvim;
    }

    // Ayın ilk gününün haftasından son gününün haftasına kadar olan günler
    private static (DateOnly IlkGun, DateOnly SonGun) AyIzgarasi(DateOnly tarih)
    {
        var ayBasi = new DateOnly(tarih.Year, tarih.Month, 1);
        var ilk = TurkceTarih.HaftaBasi(ayBasi);
        var son = TurkceTarih.HaftaBasi(ayBasi.AddMonths(1).AddDays(-1)).AddDays(6);
        return (ilk, son);
    }

    private async Task<string> AdGetirAsync(string kullaniciId)
    {
        var adSoyad = await _db.Users
            .Where(u => u.Id == kullaniciId)
            .Select(u => u.AdSoyad)
            .FirstOrDefaultAsync();

        // Selamlamada yalnızca ilk ad kullanılır
        return string.IsNullOrWhiteSpace(adSoyad) ? "" : adSoyad.Trim().Split(' ')[0];
    }

    private async Task<List<MaddeOge>> MaddeleriGetirAsync(string kullaniciId, DateOnly baslangic, DateOnly bitis)
    {
        var notlar = await _db.Notlar
            .Where(n => n.KullaniciId == kullaniciId && n.Tarih >= baslangic && n.Tarih <= bitis)
            .OrderBy(n => n.Sira).ThenBy(n => n.Id)
            .Select(n => new MaddeOge { Id = n.Id, Tur = MaddeTuru.Not, Tarih = n.Tarih, Metin = n.Metin })
            .ToListAsync();

        // Yapılacaklar son tarihlerine göre günlere dağılır
        var yapilacaklar = await _db.Gorevler
            .Where(g => g.KullaniciId == kullaniciId && g.SonTarih >= baslangic && g.SonTarih <= bitis)
            .OrderBy(g => g.Sira).ThenBy(g => g.Id)
            .Select(g => new MaddeOge
            {
                Id = g.Id,
                Tur = MaddeTuru.Yapilacak,
                Tarih = g.SonTarih!.Value,
                Metin = g.Baslik,
                Bitti = g.TamamlandiMi
            })
            .ToListAsync();

        var ilkAn = baslangic.ToDateTime(TimeOnly.MinValue);
        var sonAn = bitis.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var etkinlikler = await _db.Etkinlikler
            .Where(e => e.KullaniciId == kullaniciId && e.Baslangic >= ilkAn && e.Baslangic < sonAn)
            .OrderBy(e => e.Baslangic)
            .Select(e => new { e.Id, e.Baslik, e.Baslangic })
            .ToListAsync();

        return notlar
            .Concat(yapilacaklar)
            .Concat(etkinlikler.Select(e => EtkinlikMaddesi(e.Id, e.Baslik, e.Baslangic)))
            .ToList();
    }

    // Bugünden itibaren (bugünün geçmiş saatleri dahil) en yakın etkinlikler
    private async Task<List<MaddeOge>> YaklasanEtkinlikleriGetirAsync(string kullaniciId, DateOnly bugun)
    {
        var gunBasi = bugun.ToDateTime(TimeOnly.MinValue);
        var etkinlikler = await _db.Etkinlikler
            .Where(e => e.KullaniciId == kullaniciId && e.Baslangic >= gunBasi)
            .OrderBy(e => e.Baslangic)
            .Take(YaklasanEtkinlikSayisi)
            .Select(e => new { e.Id, e.Baslik, e.Baslangic })
            .ToListAsync();

        return etkinlikler.Select(e => EtkinlikMaddesi(e.Id, e.Baslik, e.Baslangic)).ToList();
    }

    private static MaddeOge EtkinlikMaddesi(int id, string baslik, DateTime baslangic) => new()
    {
        Id = id,
        Tur = MaddeTuru.Etkinlik,
        Tarih = DateOnly.FromDateTime(baslangic),
        Saat = TimeOnly.FromDateTime(baslangic),
        Metin = baslik
    };

    private static List<TakvimHucresi> HucreleriOlustur(
        DateOnly ilk, DateOnly son, DateOnly odak, DateOnly bugun, ILookup<DateOnly, MaddeOge> gunlere)
    {
        var hucreler = new List<TakvimHucresi>();
        for (var gun = ilk; gun <= son; gun = gun.AddDays(1))
        {
            hucreler.Add(new TakvimHucresi
            {
                Tarih = gun,
                BuAyMi = gun.Month == odak.Month,
                BugunMu = gun == bugun,
                Tatil = TurkceTarih.Tatil(gun),
                // Önce etkinlikler (saat sırasıyla), sonra yapılacaklar, en son boş olmayan notlar
                Maddeler = gunlere[gun]
                    .Where(m => m.Tur != MaddeTuru.Not || !string.IsNullOrWhiteSpace(m.Metin))
                    .OrderBy(m => m.Tur == MaddeTuru.Etkinlik ? 0 : m.Tur == MaddeTuru.Yapilacak ? 1 : 2)
                    .ThenBy(m => m.Saat)
                    .ToList()
            });
        }
        return hucreler;
    }
}
