namespace DijitalAjanda.Web.Models.ViewModels.Aliskanliklar;

public static class SuAyarlari
{
    public const int BardakMl = 250;

    // SuKaydi.BardakSayisi üst sınırı (10 litre)
    public const int GunlukEnFazlaBardak = 40;

    // Günlük hedef seçenekleri (ml)
    public static readonly IReadOnlyList<int> HedefSecenekleri = new[] { 1500, 2000, 2500, 3000 };

    // 1500 → "1,5 litre"
    public static string Litre(int ml) => (ml / 1000m).ToString("0.##", TurkceKultur);

    private static readonly System.Globalization.CultureInfo TurkceKultur = new("tr-TR");
}

// Su takibi sayfasının o anki durumu; bardak eklenip geri alındığında JSON olarak da döner
public class SuDurumu
{
    public int Bardak { get; set; }
    public int HedefMl { get; set; }
    public int Seri { get; set; }

    public int BugunMl => Bardak * SuAyarlari.BardakMl;
    public int HedefBardak => (int)Math.Ceiling(HedefMl / (double)SuAyarlari.BardakMl);
    public int Yuzde => HedefMl <= 0 ? 0 : Math.Min(100, (int)Math.Round(BugunMl * 100.0 / HedefMl, MidpointRounding.AwayFromZero));
    public bool HedefeUlasildi => HedefMl > 0 && BugunMl >= HedefMl;
    public string BugunLitre => SuAyarlari.Litre(BugunMl);
    public string HedefLitre => SuAyarlari.Litre(HedefMl);
}

public class SuGunu
{
    public DateOnly Tarih { get; set; }
    public string KisaAd { get; set; } = string.Empty;
    public int Ml { get; set; }
}

public class SuTakibiViewModel
{
    public SuDurumu Durum { get; set; } = new();

    // Bugün dâhil son 7 gün, eskiden yeniye
    public List<SuGunu> SonYediGun { get; set; } = new();
}
