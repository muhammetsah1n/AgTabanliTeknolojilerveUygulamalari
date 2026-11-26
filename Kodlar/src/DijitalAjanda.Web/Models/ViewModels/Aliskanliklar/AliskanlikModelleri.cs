using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.ViewModels.Aliskanliklar;

// Alışkanlık renkleri: veritabanında açık temadaki renk kodu (#rrggbb) saklanır,
// sayfada aynı rengin açık/koyu temaya uyan CSS sınıfı (renk-is, renk-saglik...) kullanılır.
public static class AliskanlikRenkleri
{
    public record Renk(string Kod, string Sinif, string Ad);

    public static readonly IReadOnlyList<Renk> Liste = new[]
    {
        new Renk("#cddff2", "renk-is", "Mavi"),
        new Renk("#d4e8d4", "renk-saglik", "Yeşil"),
        new Renk("#f4e5b8", "renk-kisisel", "Sarı"),
        new Renk("#e0d6f0", "renk-sosyal", "Mor"),
        new Renk("#f3c9c9", "renk-acil", "Kırmızı"),
        new Renk("#f8d8c2", "renk-turuncu", "Turuncu"),
        new Renk("#f1ece2", "renk-ev", "Bej")
    };

    public const string VarsayilanKod = "#d4e8d4";

    public static bool GecerliMi(string? kod) => Liste.Any(r => r.Kod == kod);

    // Listede olmayan eski kayıtlar yeşil gösterilir
    public static Renk Bul(string? kod) => Liste.FirstOrDefault(r => r.Kod == kod) ?? Liste[1];
}

// Yeni alışkanlık ve düzenleme formu (ayrı form sayfası ve listedeki açılır form aynı modeli kullanır)
public class AliskanlikFormu : IValidatableObject
{
    // Boşsa yeni alışkanlık
    public int? Id { get; set; }

    [Display(Name = "Alışkanlık")]
    [Required(ErrorMessage = "Alışkanlık adı zorunludur.")]
    [StringLength(80, ErrorMessage = "Alışkanlık adı en fazla {1} karakter olabilir.")]
    public string Ad { get; set; } = string.Empty;

    [Display(Name = "Haftalık hedef (gün)")]
    [Range(1, 7, ErrorMessage = "Haftalık hedef 1 ile 7 gün arasında olmalı.")]
    public int HaftalikHedef { get; set; } = 5;

    [Display(Name = "Renk")]
    public string Renk { get; set; } = AliskanlikRenkleri.VarsayilanKod;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!AliskanlikRenkleri.GecerliMi(Renk))
        {
            yield return new ValidationResult("Listeden bir renk seçin.", new[] { nameof(Renk) });
        }
    }
}

// Haftalık ızgaranın sütun başlığı
public class IzgaraGunu
{
    public DateOnly Tarih { get; set; }
    public string KisaAd { get; set; } = string.Empty;   // Pzt, Sal...
    public bool BugunMu { get; set; }
    public bool GelecekMi { get; set; }
}

// Izgaradaki bir alışkanlık satırı
public class AliskanlikSatiri
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string RenkKodu { get; set; } = AliskanlikRenkleri.VarsayilanKod;
    public string RenkSinifi { get; set; } = "renk-saglik";
    public int HaftalikHedef { get; set; }
    public HashSet<DateOnly> IsaretliGunler { get; set; } = new();
    public int HaftadaYapilan { get; set; }
    public int Seri { get; set; }
    public int EnUzunSeri { get; set; }

    public bool HedefeUlasildi => HaftadaYapilan >= HaftalikHedef;
}

public class AliskanliklarViewModel
{
    public DateOnly Bugun { get; set; }
    public List<IzgaraGunu> Gunler { get; set; } = new();
    public List<AliskanlikSatiri> Satirlar { get; set; } = new();
}

// Bir kutu işaretlendiğinde sayfaya dönen güncel durum
public class AliskanlikDurumu
{
    public bool Isaretli { get; set; }
    public int HaftadaYapilan { get; set; }
    public int HaftalikHedef { get; set; }
    public int Seri { get; set; }
    public int EnUzunSeri { get; set; }
}
