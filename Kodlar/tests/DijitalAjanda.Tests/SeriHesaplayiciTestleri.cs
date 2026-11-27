using DijitalAjanda.Web.Services;

namespace DijitalAjanda.Tests;

// Alışkanlık ve su takibindeki seri (streak) hesaplarının birim testleri
public class SeriHesaplayiciTestleri
{
    // 26 Kasım 2026 Perşembe
    private static readonly DateOnly Bugun = new(2026, 11, 26);

    private static DateOnly[] Gunler(params int[] bugundenOnce) =>
        bugundenOnce.Select(g => Bugun.AddDays(-g)).ToArray();

    // ---------- Güncel seri ----------

    [Fact]
    public void GuncelSeri_KayitYoksa_Sifir()
    {
        Assert.Equal(0, SeriHesaplayici.GuncelSeri(Array.Empty<DateOnly>(), Bugun));
    }

    [Fact]
    public void GuncelSeri_BugunDahilArdisikGunleriSayar()
    {
        Assert.Equal(3, SeriHesaplayici.GuncelSeri(Gunler(0, 1, 2), Bugun));
    }

    [Fact]
    public void GuncelSeri_BugunHenuzIsaretlenmediyse_DundenSayar()
    {
        // Gün bitmeden seri bozulmuş sayılmaz
        Assert.Equal(2, SeriHesaplayici.GuncelSeri(Gunler(1, 2), Bugun));
    }

    [Fact]
    public void GuncelSeri_DunIsaretlenmediyse_SeriBozulur()
    {
        Assert.Equal(0, SeriHesaplayici.GuncelSeri(Gunler(2, 3, 4), Bugun));
    }

    [Fact]
    public void GuncelSeri_AradaBosGunVarsa_BosGundeDurur()
    {
        // Bugün, dün, önceki gün; 3 gün önce boş; 4 ve 5 gün önce işaretli
        Assert.Equal(3, SeriHesaplayici.GuncelSeri(Gunler(0, 1, 2, 4, 5), Bugun));
    }

    [Fact]
    public void GuncelSeri_GelecektekiGunleriSaymaz()
    {
        var gunler = Gunler(0, 1).Append(Bugun.AddDays(1)).Append(Bugun.AddDays(2));
        Assert.Equal(2, SeriHesaplayici.GuncelSeri(gunler, Bugun));
    }

    [Fact]
    public void GuncelSeri_TekrarlananTarihleriBirKezSayar()
    {
        Assert.Equal(2, SeriHesaplayici.GuncelSeri(Gunler(0, 0, 1, 1), Bugun));
    }

    [Fact]
    public void GuncelSeri_AyVeYilDegisimindeKesilmez()
    {
        var yilbasi = new DateOnly(2027, 1, 1);
        var gunler = new[] { new DateOnly(2026, 12, 30), new DateOnly(2026, 12, 31), yilbasi };
        Assert.Equal(3, SeriHesaplayici.GuncelSeri(gunler, yilbasi));
    }

    // ---------- En uzun seri ----------

    [Fact]
    public void EnUzunSeri_KayitYoksa_Sifir()
    {
        Assert.Equal(0, SeriHesaplayici.EnUzunSeri(Array.Empty<DateOnly>()));
    }

    [Fact]
    public void EnUzunSeri_TekGun_Bir()
    {
        Assert.Equal(1, SeriHesaplayici.EnUzunSeri(Gunler(10)));
    }

    [Fact]
    public void EnUzunSeri_GecmistekiUzunDiziyiBulur()
    {
        // Geçmişte 5 günlük dizi, son günlerde 2 günlük dizi
        var gunler = Gunler(0, 1, 10, 11, 12, 13, 14, 20);
        Assert.Equal(5, SeriHesaplayici.EnUzunSeri(gunler));
        Assert.Equal(2, SeriHesaplayici.GuncelSeri(gunler, Bugun));
    }

    [Fact]
    public void EnUzunSeri_SirasizVeTekrarliTarihlerdeDogruSonucVerir()
    {
        var gunler = Gunler(3, 1, 2, 2, 7, 1);
        Assert.Equal(3, SeriHesaplayici.EnUzunSeri(gunler));
    }

    [Fact]
    public void EnUzunSeri_HicbirZamanGuncelSeridenKucukOlmaz()
    {
        var gunler = Gunler(0, 1, 2, 3, 8, 9);
        Assert.True(SeriHesaplayici.EnUzunSeri(gunler) >= SeriHesaplayici.GuncelSeri(gunler, Bugun));
    }

    // ---------- Haftalık hedef ----------

    [Fact]
    public void HaftadakiGunSayisi_YalnizcaPazartesiPazarArasiniSayar()
    {
        // Bugün perşembe: hafta 23 Kasım pazartesi – 29 Kasım pazar
        var gunler = new[]
        {
            new DateOnly(2026, 11, 22), // önceki haftanın pazarı
            new DateOnly(2026, 11, 23),
            new DateOnly(2026, 11, 25),
            new DateOnly(2026, 11, 26),
            new DateOnly(2026, 11, 30)  // sonraki haftanın pazartesisi
        };
        Assert.Equal(3, SeriHesaplayici.HaftadakiGunSayisi(gunler, Bugun));
    }

    [Fact]
    public void HaftadakiGunSayisi_PazarGunuAyniHaftayiKullanir()
    {
        var pazar = new DateOnly(2026, 11, 29);
        var gunler = new[] { new DateOnly(2026, 11, 23), pazar };
        Assert.Equal(2, SeriHesaplayici.HaftadakiGunSayisi(gunler, pazar));
    }

    // ---------- Su hedef serisi ----------

    private static Dictionary<DateOnly, int> SuKayitlari(params (int OnceGun, int Ml)[] kayitlar) =>
        kayitlar.ToDictionary(k => Bugun.AddDays(-k.OnceGun), k => k.Ml);

    [Fact]
    public void SuHedefSerisi_HedefeUlasilanArdisikGunleriSayar()
    {
        var kayitlar = SuKayitlari((0, 2000), (1, 2250), (2, 2500), (3, 1500));
        Assert.Equal(3, SeriHesaplayici.SuHedefSerisi(kayitlar, 2000, Bugun));
    }

    [Fact]
    public void SuHedefSerisi_BugunHedefeUlasilmadiysa_DundenSayar()
    {
        var kayitlar = SuKayitlari((0, 750), (1, 2000), (2, 2000));
        Assert.Equal(2, SeriHesaplayici.SuHedefSerisi(kayitlar, 2000, Bugun));
    }

    [Fact]
    public void SuHedefSerisi_HedefinAltindakiGunSeriyiBozar()
    {
        var kayitlar = SuKayitlari((0, 2000), (1, 1750), (2, 2000));
        Assert.Equal(1, SeriHesaplayici.SuHedefSerisi(kayitlar, 2000, Bugun));
    }

    [Fact]
    public void SuHedefSerisi_TamHedefKadarIcmekYeterli()
    {
        var kayitlar = SuKayitlari((0, 1500));
        Assert.Equal(1, SeriHesaplayici.SuHedefSerisi(kayitlar, 1500, Bugun));
    }

    [Fact]
    public void SuHedefSerisi_HedefSifirsa_Sifir()
    {
        var kayitlar = SuKayitlari((0, 2000));
        Assert.Equal(0, SeriHesaplayici.SuHedefSerisi(kayitlar, 0, Bugun));
    }
}
