namespace DijitalAjanda.Web.Models.ViewModels.Bugun;

public enum Gorunum
{
    Gun,
    Hafta,
    Ay
}

public enum MaddeTuru
{
    Not,
    Yapilacak,
    Etkinlik
}

// Bugün sayfasındaki tek bir madde: not, yapılacak (görev) ya da etkinlik
public class MaddeOge
{
    public int Id { get; set; }
    public MaddeTuru Tur { get; set; }
    public DateOnly Tarih { get; set; }
    public string Metin { get; set; } = string.Empty;
    public bool Bitti { get; set; }
    public TimeOnly? Saat { get; set; }

    // Bullet journal işareti: – not, • yapılacak, × bitti, ○ etkinlik
    public string Isaret => Tur switch
    {
        MaddeTuru.Not => "–",
        MaddeTuru.Etkinlik => "○",
        _ => Bitti ? "×" : "•"
    };
}

// Hafta ve ay görünümündeki bir gün kutusu
public class TakvimHucresi
{
    public DateOnly Tarih { get; set; }
    public bool BuAyMi { get; set; }
    public bool BugunMu { get; set; }
    public string? Tatil { get; set; }
    public List<MaddeOge> Maddeler { get; set; } = new();
}

// Sağdaki takvim bu bilgiyle tarayıcıda çizilir; ay değiştirildikçe aynı biçimde yeniden istenir
public class TakvimAyi
{
    public string Ay { get; set; } = string.Empty;
    public List<string> EtkinlikGunleri { get; set; } = new();
    public List<TatilGunu> Tatiller { get; set; } = new();
}

public class TatilGunu
{
    public string Tarih { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
}

public class BugunViewModel
{
    public string Ad { get; set; } = string.Empty;
    public Gorunum Gorunum { get; set; }
    public DateOnly Odak { get; set; }
    public DateOnly BugununTarihi { get; set; }

    public string GezintiBasligi { get; set; } = string.Empty;
    public DateOnly Onceki { get; set; }
    public DateOnly Sonraki { get; set; }

    // Gün görünümü
    public List<MaddeOge> Notlar { get; set; } = new();
    public List<MaddeOge> Yapilacaklar { get; set; } = new();
    public List<MaddeOge> Etkinlikler { get; set; } = new();
    public int TamamlanmaYuzdesi { get; set; }
    public int AcikYapilacakSayisi => Yapilacaklar.Count(m => !m.Bitti);

    // Hafta (7 hücre) veya ay (5–6 hafta) görünümü
    public List<TakvimHucresi> Hucreler { get; set; } = new();

    // Sağ sütun
    public TakvimAyi Takvim { get; set; } = new();
    public List<MaddeOge> YaklasanEtkinlikler { get; set; } = new();

    public bool BugunMu => Odak == BugununTarihi;
}
