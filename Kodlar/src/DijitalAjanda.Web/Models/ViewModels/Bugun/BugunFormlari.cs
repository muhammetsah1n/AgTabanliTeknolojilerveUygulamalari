using System.ComponentModel.DataAnnotations;
using DijitalAjanda.Web.Models.Entities;

namespace DijitalAjanda.Web.Models.ViewModels.Bugun;

// Her form, işlemden sonra aynı görünüme ve tarihe dönebilmek için bu iki alanı taşır
public class SayfaKonumu
{
    public Gorunum Gorunum { get; set; } = Gorunum.Gun;
    public DateOnly Odak { get; set; }
}

public class YeniNotFormu : SayfaKonumu
{
    public DateOnly Tarih { get; set; }

    [Required(ErrorMessage = "Not boş olamaz.")]
    [StringLength(1000, ErrorMessage = "Not en fazla {1} karakter olabilir.")]
    public string Metin { get; set; } = string.Empty;
}

public class NotGuncelleFormu : SayfaKonumu
{
    public int Id { get; set; }

    // Boş bırakılan not silinir
    [StringLength(1000, ErrorMessage = "Not en fazla {1} karakter olabilir.")]
    public string? Metin { get; set; }
}

public class YeniYapilacakFormu : SayfaKonumu
{
    public DateOnly Tarih { get; set; }

    [Required(ErrorMessage = "Yapılacak boş olamaz.")]
    [StringLength(150, ErrorMessage = "Yapılacak en fazla {1} karakter olabilir.")]
    public string Baslik { get; set; } = string.Empty;
}

public class YeniEtkinlikFormu : SayfaKonumu, IValidatableObject
{
    [Required(ErrorMessage = "Etkinlik adı boş olamaz.")]
    [StringLength(150, ErrorMessage = "Etkinlik adı en fazla {1} karakter olabilir.")]
    public string Baslik { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tarih seçin.")]
    public DateOnly? Tarih { get; set; }

    [Required(ErrorMessage = "Saat seçin.")]
    public TimeOnly? Saat { get; set; }

    // Hafta ve ay kutularından hızlı eklemede bu alanlar gönderilmez; varsayılanları kullanılır
    [EnumDataType(typeof(EtkinlikTuru), ErrorMessage = "Geçerli bir etkinlik türü seçin.")]
    public EtkinlikTuru Tur { get; set; } = EtkinlikTuru.Etkinlik;

    // Tur = Ozel iken kullanıcının yazdığı tür adı
    [StringLength(40, ErrorMessage = "Tür adı en fazla {1} karakter olabilir.")]
    public string? OzelTur { get; set; }

    // Boşsa etkinliğin bitiş saati yok
    public TimeOnly? BitisSaati { get; set; }

    // Boşsa hatırlatma yok; dolu ise kaç × birim (dakika, saat, gün, hafta) olarak dakika cinsinden
    public int? HatirlaticiDk { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Tur == EtkinlikTuru.Ozel && string.IsNullOrWhiteSpace(OzelTur))
        {
            yield return new ValidationResult("Kendi türünün adını yazın.", new[] { nameof(OzelTur) });
        }
        if (BitisSaati is TimeOnly bitis && Saat is TimeOnly baslangic && bitis <= baslangic)
        {
            yield return new ValidationResult("Bitiş saati başlangıç saatinden sonra olmalı.", new[] { nameof(BitisSaati) });
        }
        if (HatirlaticiDk is int dk && EtkinlikSecenekleri.HatirlaticiParcala(dk) is null)
        {
            yield return new ValidationResult("Geçerli bir hatırlatıcı seçin.", new[] { nameof(HatirlaticiDk) });
        }
    }
}

// Etkinlik formundaki tür ve hatırlatma seçim pencerelerinin seçenekleri (Etkinlik Paneli tasarımı)
public static class EtkinlikSecenekleri
{
    // Ikon: Bootstrap Icons sınıfı; "yemek" için çatal-bıçak SVG'si kullanılır (ikon setinde yok)
    public static readonly (EtkinlikTuru Tur, string Ad, string Ikon)[] Turler =
    {
        (EtkinlikTuru.Etkinlik, "Etkinlik", "bi-tag"),
        (EtkinlikTuru.Toplanti, "Toplantı", "bi-people"),
        (EtkinlikTuru.Randevu, "Randevu", "bi-calendar-check"),
        (EtkinlikTuru.Gorev, "Görev", "bi-check2-square"),
        (EtkinlikTuru.DogumGunu, "Doğum günü", "bi-gift"),
        (EtkinlikTuru.Yemek, "Yemek", "yemek"),
        (EtkinlikTuru.Alisveris, "Alışveriş", "bi-bag")
    };

    // Hatırlatma = kaç × birim, dakika olarak saklanır; örn. 15 × Dakika = 15, 5 × Saat = 300
    public static readonly int[] HatirlaticiKaclari = { 5, 10, 15, 30, 45, 60 };

    public static readonly (int Dakika, string Ad)[] HatirlaticiBirimleri =
    {
        (1, "Dakika"),
        (60, "Saat"),
        (1440, "Gün"),
        (10080, "Hafta")
    };

    public static string TurAdi(EtkinlikTuru tur, string? ozelTur = null) =>
        tur == EtkinlikTuru.Ozel
            ? (string.IsNullOrWhiteSpace(ozelTur) ? "Diğer" : ozelTur.Trim())
            : Turler.Where(t => t.Tur == tur).Select(t => t.Ad).FirstOrDefault() ?? "Etkinlik";

    // Dakikayı geçerli bir kaç × birim'e ayırır; geçersizse null. Kaç ve birim aralıkları çakışmaz,
    // bu yüzden her değerin tek bir karşılığı vardır (ör. 60 = 60 dakika, 300 = 5 saat).
    public static (int Kac, int BirimDk, string Birim)? HatirlaticiParcala(int dakika)
    {
        foreach (var (birimDk, ad) in HatirlaticiBirimleri)
        {
            if (dakika % birimDk == 0 && HatirlaticiKaclari.Contains(dakika / birimDk))
            {
                return (dakika / birimDk, birimDk, ad);
            }
        }
        return null;
    }

    // "15 dakika önce", "5 saat önce"
    public static string HatirlaticiAdi(int dakika) =>
        HatirlaticiParcala(dakika) is var (kac, _, birim)
            ? $"{kac} {birim.ToLower(new System.Globalization.CultureInfo("tr-TR"))} önce"
            : $"{dakika} dakika önce";

    // Etkinlik satırının altındaki küçük yazı: "Toplantı · 15 dakika önce hatırlatılır".
    // Varsayılan tür (Etkinlik) ve hatırlatıcısız etkinlikte gösterilmez; satır tasarımdaki gibi kalır.
    public static string? Ayrinti(EtkinlikTuru tur, string? ozelTur, int? hatirlaticiDk)
    {
        var parcalar = new List<string>();
        if (tur != EtkinlikTuru.Etkinlik) parcalar.Add(TurAdi(tur, ozelTur));
        if (hatirlaticiDk is int dk) parcalar.Add($"{HatirlaticiAdi(dk)} hatırlatılır");
        return parcalar.Count == 0 ? null : string.Join(" · ", parcalar);
    }
}

public class EtkinlikGuncelleFormu : SayfaKonumu
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Etkinlik adı boş olamaz.")]
    [StringLength(150, ErrorMessage = "Etkinlik adı en fazla {1} karakter olabilir.")]
    public string Baslik { get; set; } = string.Empty;

    // Boşsa etkinliğin saati değişmez
    public TimeOnly? Saat { get; set; }
}

public class MaddeSilFormu : SayfaKonumu
{
    public int Id { get; set; }
}
