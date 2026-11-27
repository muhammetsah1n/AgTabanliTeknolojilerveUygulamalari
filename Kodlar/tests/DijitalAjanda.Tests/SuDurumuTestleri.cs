using DijitalAjanda.Web.Models.ViewModels.Aliskanliklar;

namespace DijitalAjanda.Tests;

// Su takibi sayfasında gösterilen litre, yüzde ve hedef hesapları
public class SuDurumuTestleri
{
    [Theory]
    [InlineData(0, 2000, 0)]
    [InlineData(4, 2000, 50)]
    [InlineData(5, 2000, 63)]   // 1250 / 2000 = %62,5 → %63
    [InlineData(8, 2000, 100)]
    [InlineData(12, 2000, 100)] // hedef aşılınca bardak %100'de kalır
    [InlineData(3, 1500, 50)]
    public void Yuzde_HedefeGoreHesaplanir(int bardak, int hedefMl, int beklenen)
    {
        var durum = new SuDurumu { Bardak = bardak, HedefMl = hedefMl };
        Assert.Equal(beklenen, durum.Yuzde);
    }

    [Theory]
    [InlineData(1500, 6)]
    [InlineData(2000, 8)]
    [InlineData(2500, 10)]
    [InlineData(3000, 12)]
    public void HedefBardak_250MlBardagaGoreHesaplanir(int hedefMl, int beklenen)
    {
        Assert.Equal(beklenen, new SuDurumu { HedefMl = hedefMl }.HedefBardak);
    }

    [Fact]
    public void HedefeUlasildi_HedefMiktarindaTrueOlur()
    {
        Assert.False(new SuDurumu { Bardak = 7, HedefMl = 2000 }.HedefeUlasildi);
        Assert.True(new SuDurumu { Bardak = 8, HedefMl = 2000 }.HedefeUlasildi);
    }

    [Theory]
    [InlineData(1500, "1,5")]
    [InlineData(2000, "2")]
    [InlineData(2250, "2,25")]
    [InlineData(0, "0")]
    public void Litre_TurkceOndalikAyiriciylaYazilir(int ml, string beklenen)
    {
        Assert.Equal(beklenen, SuAyarlari.Litre(ml));
    }
}
