using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.Entities;

public class Kitap
{
    public int Id { get; set; }

    [Required]
    public string KullaniciId { get; set; } = string.Empty;
    public Kullanici? Kullanici { get; set; }

    [Required, StringLength(150)]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(100)]
    public string? YazarYonetmen { get; set; }

    public KitapTuru Tur { get; set; } = KitapTuru.Kitap;

    [Range(0, 5)]
    public int Puan { get; set; }

    [Required, StringLength(7)]
    public string SirtRengi { get; set; } = "#cddff2";

    public DateTime EklenmeTarihi { get; set; } = DateTime.UtcNow;
}
