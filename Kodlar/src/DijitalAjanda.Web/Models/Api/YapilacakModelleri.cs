using System.ComponentModel.DataAnnotations;
using DijitalAjanda.Web.Models.Entities;

namespace DijitalAjanda.Web.Models.Api;

/// <summary>API'nin döndürdüğü yapılacak maddesi.</summary>
public class YapilacakDto
{
    /// <example>12</example>
    public int Id { get; set; }

    /// <example>Rapor taslağını bitir</example>
    public string Baslik { get; set; } = string.Empty;

    /// <summary>true ise madde bitti (×), false ise yapılacak (•).</summary>
    public bool TamamlandiMi { get; set; }

    /// <summary>Maddenin Bugün sayfasında görüneceği gün.</summary>
    /// <example>2026-11-09</example>
    public DateOnly? Tarih { get; set; }

    /// <summary>Gün içindeki sıra (1'den başlar).</summary>
    public int Sira { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public static YapilacakDto Olustur(Gorev g) => new()
    {
        Id = g.Id,
        Baslik = g.Baslik,
        TamamlandiMi = g.TamamlandiMi,
        Tarih = g.SonTarih,
        Sira = g.Sira,
        OlusturmaTarihi = g.OlusturmaTarihi
    };
}

/// <summary>Yeni yapılacak eklemek için gönderilen bilgiler (POST).</summary>
public class YapilacakEkleIstegi
{
    /// <example>Rapor taslağını bitir</example>
    // Required yalnızca boşluktan oluşan metni de reddeder
    [Required(ErrorMessage = "Yapılacak boş olamaz.")]
    [StringLength(150, ErrorMessage = "Yapılacak en fazla {1} karakter olabilir.")]
    public string? Baslik { get; set; }

    /// <example>2026-11-09</example>
    [Required(ErrorMessage = "Tarih seçin.")]
    public DateOnly? Tarih { get; set; }
}

/// <summary>Bir yapılacağın bütün alanlarını güncellemek için gönderilen bilgiler (PUT).</summary>
public class YapilacakGuncelleIstegi : YapilacakEkleIstegi
{
    [Required(ErrorMessage = "Tamamlanma durumu belirtilmeli.")]
    public bool? TamamlandiMi { get; set; }
}

/// <summary>Tamamlanma durumunu değiştirmek için gönderilen bilgiler (PATCH).</summary>
public class YapilacakDurumIstegi
{
    /// <summary>true: bitti (×), false: yapılacak (•).</summary>
    [Required(ErrorMessage = "Tamamlanma durumu belirtilmeli.")]
    public bool? TamamlandiMi { get; set; }
}
